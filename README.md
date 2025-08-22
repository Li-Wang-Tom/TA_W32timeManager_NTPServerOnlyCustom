# TA_W32TimeManager_NTPServerOnlyCustom

W32Time（Windows Time サービス）を制御・監視するためのカスタムツールです。  
NTPサーバー専用モードとメンテナンスモード（クライアント+サーバー）を簡単に切り替えることができます。

## 🚀 主な機能

### W32Time Service Control

- **サービス状態の監視**: リアルタイムでW32Timeサービスの動作状況を表示
- **サービスの開始・停止**: ワンクリックでサービスを制御
- **スタートアップ設定**: システム起動時の自動開始設定

### NTP Configuration Mode

- **Operation Mode (Server Only)**: NTPサーバー専用モード
  - SNTP時刻配信のみを実行
  - 
- **Maintenance Mode (Client+Server)**: メンテナンスモード
  - NTPクライアント・サーバー両機能を有効化
  - 設定調整や動作確認時に使用

### 視覚的なステータス表示

- **サービス状態ランプ**: 緑（稼働中）/ 赤（停止中）/ オレンジ（エラー）
- **NTP設定ランプ**: 緑（サーバー専用）/ 赤（標準設定）/ 灰（不明）
- **リアルタイム更新**: 状態変化を自動で検出・表示

## 💻 動作環境

- **OS**: Windows 10 / 11
- **Framework**: .NET Framework 4.8 またはそれ以降
- **権限**: 管理者権限必須（W32Timeサービス制御のため）

## 📦 インストール・実行

1. リリースページから最新版をダウンロード
2. 管理者権限でアプリケーションを実行
3. GUIからサービス制御・NTP設定を実行

## 🏭 使用場面

### 製造業での典型的な構成

```
上位システム (MES/SCADA)
    ↓ 時刻同期
Windows PC (W32Time - Server Only Mode)
    ↓ SNTP配信
L2機器 (制御装置等)
```

### 推奨運用

- **通常運用**: Operation Mode（サーバー専用）
- **メンテナンス時**: Maintenance Mode（必要に応じて）

## 🖼️ アプリケーション画面

![アプリケーション画面](https://claude.ai/chat/docs/screenshot.png)

## 🛠️ 開発者向け情報

### ビルド方法

```bash
git clone https://github.com/Li-Wang-Tom/TA_W32TimeManager_NTPServerOnlyCustom.git
cd TA_W32TimeManager_NTPServerOnlyCustom
# Visual Studioでソリューションファイルを開いてビルド
```

## 📝 ライセンス

このプロジェクトはMITライセンスの下で公開されています。  
詳細は [LICENSE](https://claude.ai/chat/LICENSE) ファイルをご確認ください。

## 👨‍💻 作者

**TA Li-Wang-Tom**

- GitHub: [@Li-Wang-Tom](https://github.com/Li-Wang-Tom)

## 🙏 謝辞

このプロジェクトの開発にあたり、以下の技術とコミュニティに感謝いたします。

**技術基盤:**

- **Microsoft .NET Framework** - 安定した開発プラットフォーム
- **WPF (Windows Presentation Foundation)** - 優秀なデスクトップUIフレームワーク
- **Windows Services** - 堅牢なサービス基盤

**開発環境:**

- **Visual Studio** - 統合開発環境
- **GitHub** - コード管理とコラボレーション
- **NuGet** - パッケージ管理システム

**コミュニティ:**

- **Microsoft Docs** - 詳細な技術文書
- **.NET Community** - オープンソースエコシステム

---

感谢以下技术和社区为本项目开发提供的支持：

**核心技术:**

- **Microsoft .NET Framework** - 稳定可靠的开发平台
- **WPF** - 功能强大的桌面应用界面框架
- **Windows Services** - robust的系统服务架构

**开发工具:**

- **Visual Studio** - 功能齐全的集成开发环境
- **GitHub** - 代码版本控制与团队协作平台
- **NuGet** - 便捷的包管理解决方案

**技术社区:**

- **Microsoft Docs** - 详尽的官方技术文档
- **.NET Community** - 活跃的开源开发者社区

向所有为开源技术发展做出贡献的开发者们致敬！  
谢谢大家！🎉
