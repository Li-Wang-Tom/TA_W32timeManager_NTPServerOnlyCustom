using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace W32TimeManager
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer statusTimer;
        private DispatcherTimer blinkTimer;
        private DispatcherTimer ntpConfigTimer;
        private bool isLampVisible = true;
        private const string ServiceName = "W32Time";

        public MainWindow()
        {
            InitializeComponent();
            InitializeStatusTimer();
            InitializeNtpConfigTimer();
            UpdateServiceStatus();
            UpdateNtpConfigStatus();
            StartBlinking();
            CheckAdministratorPrivileges();
        }

        /// <summary>
        /// ランプを点滅させる処理（サービス稼働中のみ）
        /// </summary>
        private void StartBlinking()
        {
            blinkTimer = new DispatcherTimer();
            blinkTimer.Interval = TimeSpan.FromMilliseconds(200);
            blinkTimer.Tick += (s, e) =>
            {
                if (StatusText.Text.Contains("RUNNING"))
                {
                    StatusLamp.Opacity = isLampVisible ? 1.0 : 0.3;
                    isLampVisible = !isLampVisible;
                }
                else
                {
                    StatusLamp.Opacity = 1.0;
                }
            };
            blinkTimer.Start();
        }

        /// <summary>
        /// サービスステータス更新タイマーの初期化
        /// </summary>
        private void InitializeStatusTimer()
        {
            statusTimer = new DispatcherTimer();
            statusTimer.Interval = TimeSpan.FromSeconds(2);
            statusTimer.Tick += (s, e) => UpdateServiceStatus();
            statusTimer.Start();
        }

        /// <summary>
        /// NTP設定状態更新タイマーの初期化
        /// </summary>
        private void InitializeNtpConfigTimer()
        {
            ntpConfigTimer = new DispatcherTimer();
            ntpConfigTimer.Interval = TimeSpan.FromSeconds(5);
            ntpConfigTimer.Tick += (s, e) => UpdateNtpConfigStatus();
            ntpConfigTimer.Start();
        }

        /// <summary>
        /// 管理者権限チェック
        /// </summary>
        private void CheckAdministratorPrivileges()
        {
            if (!IsRunAsAdministrator())
            {
                MessageBox.Show("このアプリケーションは管理者権限で実行する必要があります。\n" +
                              "右クリックして「管理者として実行」を選択してください。",
                              "権限不足", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 管理者権限で実行されているかチェック
        /// </summary>
        private bool IsRunAsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// サービスステータスの更新
        /// </summary>
        private void UpdateServiceStatus()
        {
            try
            {
                using (var service = new ServiceController(ServiceName))
                {
                    bool isRunning = service.Status == ServiceControllerStatus.Running;

                    Dispatcher.Invoke(() =>
                    {
                        if (isRunning)
                        {
                            StatusText.Text = "Service: RUNNING";
                            StatusLamp.Fill = new SolidColorBrush(Color.FromRgb(56, 161, 105));
                        }
                        else
                        {
                            StatusText.Text = "Service: STOPPED";
                            StatusLamp.Fill = new SolidColorBrush(Color.FromRgb(245, 101, 101));
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = "Service: ERROR";
                    StatusLamp.Fill = new SolidColorBrush(Color.FromRgb(255, 165, 0));
                });
                Debug.WriteLine($"ステータス更新エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// NTP設定状態の更新
        /// </summary>
        private void UpdateNtpConfigStatus()
        {
            try
            {
                bool isServerOnlyMode = IsNtpServerOnlyMode();

                Dispatcher.Invoke(() =>
                {
                    if (isServerOnlyMode)
                    {
                        NtpConfigText.Text = "Operation";
                        NtpConfigLamp.Fill = new SolidColorBrush(Color.FromRgb(56, 161, 105)); // 緑色
                        NtpConfigLamp.Effect = new System.Windows.Media.Effects.DropShadowEffect
                        {
                            Color = Color.FromRgb(56, 161, 105),
                            BlurRadius = 10,
                            ShadowDepth = 0,
                            Opacity = 0.9
                        };
                    }
                    else
                    {
                        NtpConfigText.Text = "Maintenance";
                        NtpConfigLamp.Fill = new SolidColorBrush(Color.FromRgb(245, 101, 101)); // 赤色
                        NtpConfigLamp.Effect = new System.Windows.Media.Effects.DropShadowEffect
                        {
                            Color = Color.FromRgb(245, 101, 101),
                            BlurRadius = 10,
                            ShadowDepth = 0,
                            Opacity = 0.9
                        };
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    NtpConfigText.Text = "UNKNOWN";
                    NtpConfigLamp.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 128)); // 灰色
                    NtpConfigLamp.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = Color.FromRgb(128, 128, 128),
                        BlurRadius = 10,
                        ShadowDepth = 0,
                        Opacity = 0.9
                    };
                });
                Debug.WriteLine($"NTP設定状態更新エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// NTPサーバ専用モードかどうかを確認（改善版）
        /// </summary>
        private bool IsNtpServerOnlyMode()
        {
            try
            {
                bool ntpClientDisabled = false;
                bool ntpServerEnabled = false;

                // NtpClientの状態をレジストリから直接確認
                try
                {
                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                        @"SYSTEM\CurrentControlSet\Services\W32Time\TimeProviders\NtpClient"))
                    {
                        if (key != null)
                        {
                            var enabled = key.GetValue("Enabled");
                            ntpClientDisabled = (enabled != null && enabled.ToString() == "0");
                        }
                    }
                }
                catch { }

                // NtpServerの状態をレジストリから直接確認
                try
                {
                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                        @"SYSTEM\CurrentControlSet\Services\W32Time\TimeProviders\NtpServer"))
                    {
                        if (key != null)
                        {
                            var enabled = key.GetValue("Enabled");
                            ntpServerEnabled = (enabled != null && enabled.ToString() == "1");
                        }
                    }
                }
                catch { }

                // デバッグ出力
                Debug.WriteLine($"NtpClient Disabled: {ntpClientDisabled}, NtpServer Enabled: {ntpServerEnabled}");

                return ntpClientDisabled && ntpServerEnabled;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NTP設定確認エラー: {ex.Message}");
                return false;
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteServiceCommand("start", "W32Timeサービスを開始しています...");
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteServiceCommand("stop", "W32Timeサービスを停止しています...");
        }

        private async void StartupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowProgress(true, "スタートアップに登録しています...");

                await Task.Run(() =>
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "sc",
                        Arguments = "config W32Time start=auto",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        process.WaitForExit();
                        if (process.ExitCode != 0)
                        {
                            throw new Exception($"スタートアップ登録に失敗しました。終了コード: {process.ExitCode}");
                        }
                    }
                });

                ShowProgress(false);
                MessageBox.Show("スタートアップ登録が完了しました。", "完了",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowProgress(false);
                MessageBox.Show($"スタートアップ登録に失敗しました:\n{ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteServiceCommand(string command, string progressMessage)
        {
            try
            {
                ShowProgress(true, progressMessage);

                await Task.Run(() =>
                {
                    using (var service = new ServiceController(ServiceName))
                    {
                        if (command == "start" && service.Status != ServiceControllerStatus.Running)
                        {
                            service.Start();
                            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        }
                        else if (command == "stop" && service.Status == ServiceControllerStatus.Running)
                        {
                            service.Stop();
                            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        }
                    }
                });

                ShowProgress(false);
                UpdateServiceStatus();
                UpdateNtpConfigStatus();
            }
            catch (Exception ex)
            {
                ShowProgress(false);
                MessageBox.Show($"操作に失敗しました:\n{ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowProgress(bool show, string message = "")
        {
            Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressMessage.Text = message;
                    ProgressMessage.Visibility = Visibility.Visible;
                    ProgressBar.IsIndeterminate = true;

                    // W32Time Service Control Group
                    StartButton.IsEnabled = false;
                    StopButton.IsEnabled = false;
                    StartupButton.IsEnabled = false;

                    // NTP Configuration Mode Group
                    ConfigureNtpButton.IsEnabled = false;
                    RestoreStandardButton.IsEnabled = false;
                }
                else
                {
                    ProgressBar.Visibility = Visibility.Collapsed;
                    ProgressMessage.Visibility = Visibility.Collapsed;
                    ProgressBar.IsIndeterminate = false;

                    // W32Time Service Control Group
                    StartButton.IsEnabled = true;
                    StopButton.IsEnabled = true;
                    StartupButton.IsEnabled = true;

                    // NTP Configuration Mode Group
                    ConfigureNtpButton.IsEnabled = true;
                    RestoreStandardButton.IsEnabled = true;
                }
            });
        }

        /// <summary>
        /// コマンド実行ヘルパー
        /// </summary>
        private void ExecuteCommand(string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    string output = process.StandardOutput.ReadToEnd();
                    Debug.WriteLine($"Command failed: {fileName} {arguments}");
                    Debug.WriteLine($"Exit code: {process.ExitCode}");
                    Debug.WriteLine($"Error: {error}");
                    Debug.WriteLine($"Output: {output}");
                    throw new Exception($"Command failed: {fileName} {arguments} (Exit code: {process.ExitCode})");
                }
            }
        }

        private async void ConfigureNtpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowProgress(true, "NTPサーバ専用モードに設定中...");

                await Task.Run(() =>
                {
                    // まずサービスを停止
                    ExecuteCommand("net", "stop w32time");

                    // NtpClientを明示的に無効化
                    ExecuteCommand("reg", "add HKLM\\SYSTEM\\CurrentControlSet\\Services\\W32Time\\TimeProviders\\NtpClient /v Enabled /t REG_DWORD /d 0 /f");

                    // NtpServerを有効化
                    ExecuteCommand("reg", "add HKLM\\SYSTEM\\CurrentControlSet\\Services\\W32Time\\TimeProviders\\NtpServer /v Enabled /t REG_DWORD /d 1 /f");

                    // W32Timeの設定をリセット（クライアント機能を無効化）
                    ExecuteCommand("w32tm", "/config /manualpeerlist:\"\" /syncfromflags:manual");

                    // さらに確実にするため、Type値を設定（NTP = サーバーのみ）
                    ExecuteCommand("reg", "add HKLM\\SYSTEM\\CurrentControlSet\\Services\\W32Time\\Parameters /v Type /t REG_SZ /d NTP /f");

                    // 設定を更新
                    ExecuteCommand("w32tm", "/config /update");

                    // サービスを開始
                    ExecuteCommand("net", "start w32time");

                    // 少し待ってからさらに設定を確実にする
                    System.Threading.Thread.Sleep(2000);
                    ExecuteCommand("w32tm", "/config /update");
                });

                ShowProgress(false);

                // 設定反映のために少し待つ
                await Task.Delay(3000);
                UpdateNtpConfigStatus();

                MessageBox.Show("NTPサーバ専用モードに変更しました。\nサーバー機能のみが有効になりました。", "生産開始可能",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowProgress(false);
                MessageBox.Show($"設定に失敗しました:\n{ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 標準NTP設定に戻す（クライアント+サーバー両方有効）
        /// </summary>
        private async void RestoreStandardNtpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowProgress(true, "メンテナンスモード(標準NTP)に設定中...");

                await Task.Run(() =>
                {
                    // まずサービスを停止
                    ExecuteCommand("net", "stop w32time");

                    // NtpClientを有効化
                    ExecuteCommand("reg", "add HKLM\\SYSTEM\\CurrentControlSet\\Services\\W32Time\\TimeProviders\\NtpClient /v Enabled /t REG_DWORD /d 1 /f");

                    // NtpServerを有効化
                    ExecuteCommand("reg", "add HKLM\\SYSTEM\\CurrentControlSet\\Services\\W32Time\\TimeProviders\\NtpServer /v Enabled /t REG_DWORD /d 1 /f");

                    // 標準的な設定に戻す
                    ExecuteCommand("w32tm", "/config /manualpeerlist:\"time.windows.com,0x1\" /syncfromflags:manual /reliable:yes /update");

                    // Type値を標準設定に戻す
                    ExecuteCommand("reg", "add HKLM\\SYSTEM\\CurrentControlSet\\Services\\W32Time\\Parameters /v Type /t REG_SZ /d NT5DS /f");

                    // 設定を更新
                    ExecuteCommand("w32tm", "/config /update");

                    // サービスを開始
                    ExecuteCommand("net", "start w32time");

                    // 少し待ってからさらに設定を確実にする
                    System.Threading.Thread.Sleep(2000);
                    ExecuteCommand("w32tm", "/config /update");
                });

                ShowProgress(false);

                // 設定反映のために少し待つ
                await Task.Delay(3000);
                UpdateNtpConfigStatus();

                MessageBox.Show("メンテナンスモード(標準NTP)にしました。\nクライアント機能とサーバー機能の両方が有効になりました。", "メンテナンス実行中",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowProgress(false);
                MessageBox.Show($"設定に失敗しました:\n{ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            statusTimer?.Stop();
            blinkTimer?.Stop();
            ntpConfigTimer?.Stop();
            base.OnClosed(e);
        }
    }
}