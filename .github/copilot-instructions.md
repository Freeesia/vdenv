# Copilot Instructions

## プロジェクト概要

`vdenv` はWindows専用の dotnet toolで、Windowsの仮想デスクトップごとに環境変数とシェルプロファイルを切り替えます。実行すると一時バッチファイルを生成してそのパスを標準出力に書き出し、呼び出し元（`cmd.exe /k for /f ...`）がそのバッチファイルを実行することでシェルに設定を適用します。

## ビルド・パック

```sh
# ビルド（Windows）
dotnet build vdenv

# 非Windows環境でのビルド（CI等）
dotnet build vdenv -p:EnableWindowsTargeting=true

# ローカル実行
dotnet run --project vdenv/vdenv.csproj

# 変更を監視しながら実行（開発用）
dotnet watch run --project vdenv/vdenv.csproj

# NuGet dotnet tool としてパック
dotnet pack vdenv -c Release -o pack -p:EnableWindowsTargeting=true
```

このリポジトリにテストプロジェクトはありません。

## アーキテクチャ

```
vdenv/
  Program.cs      # トップレベルステートメント。全CLIコマンドをここで定義
  RootConfig.cs   # データモデル（RootConfig, DesktopConfig, HashData）
Lib/VirtualDesktop/  # Gitサブモジュール: Windows Virtual Desktop COM APIのC#ラッパー
                     # 名前空間: WindowsDesktop
                     # 複数Windowsビルド(10240, 20348, 22000, 22621, 26100)向けの
                     # COMインターフェース定義をEmbeddedResourceとして埋め込み、
                     # Roslynで実行時にコンパイルする
```

**CLIコマンド**（`ConsoleAppFramework` 使用）:

| コマンド | 説明 |
|---------|------|
| `vdenv`（デフォルト） | `~/vdenv.yaml` を読み込み、現在の仮想デスクトップの設定に基づいて `.bat` ファイルを `%TEMP%\vdenv\` にキャッシュし、そのパスを出力 |
| `vdenv init` | 全仮想デスクトップを列挙して `~/vdenv.yaml` を作成/マージ |
| `vdenv config` | 現在のデスクトップ設定を `~/vdenv.yaml` から表示 |
| `vdenv config open` | `~/vdenv.yaml` をデフォルトエディタで開く |

**バッチファイルのキャッシュ**: デフォルトコマンドはデスクトップ設定・ID・名前をMD5ハッシュし、同一ハッシュのキャッシュがあれば再利用します（不要なファイル書き込みを回避）。

**STAスレッド必須**: `VirtualDesktop.*` APIを呼ぶ前に `Thread.CurrentThread.SetApartmentState(ApartmentState.STA)` が必要です。

**Debug/Release の違い**: VirtualDesktopのアセンブリキャッシュディレクトリが `vdenv-debug`（Debug）と `vdenv`（Release）で異なります（`#if DEBUG` で制御）。

## 主要な規約

- **データモデル**は `partial record` 型に `[YamlObject]` 属性を付けます（VYamlの要件）。シリアライズ対象の型はすべて `partial` にする必要があります。
- **YAMLシリアライズ**には `VYaml` を使用します。`YamlSerializer.Serialize` / `YamlSerializer.Deserialize<T>` / `YamlSerializer.SerializeToString` を使います。
- **設定ファイル**: `~/vdenv.yaml`（`Environment.SpecialFolder.UserProfile` 直下）。`Guid`（仮想デスクトップの内部ID）をキーとする辞書構造。
- **ConsoleAppFramework v5**: `app.Add("コマンド名", メソッドデリゲート)` でコマンドを登録。メソッドやパラメータのXMLドキュメントコメントがヘルプテキストになります。
- **ターゲットフレームワーク**: `net8.0-windows10.0.19041.0`（Windows専用）。Linux/macOSでビルドする場合は `-p:EnableWindowsTargeting=true` が必要です。
- **バージョン管理**: GitVersion（`GitVersion.yml`）で管理。`MajorMinorPatchTag` スキーム、`v*` タグプレフィックス。パック時は `-p:Version=...` で指定。
- **NuGetパッケージ**: 自動生成ではなくカスタムの `vdenv.nuspec` を使用。パッケージタイプは `DotnetTool`。
