# vdenv

[![.NET Core Package](https://github.com/Freeesia/vdenv/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Freeesia/vdenv/actions/workflows/dotnet.yml)
[![GitHub Release](https://img.shields.io/github/v/release/Freeesia/vdenv)](https://github.com/Freeesia/vdenv/releases/latest)
[![NuGet Version](https://img.shields.io/nuget/v/vdenv)](https://www.nuget.org/packages/vdenv)
[![NuGet Downloads](https://img.shields.io/nuget/dt/vdenv)](https://www.nuget.org/packages/vdenv)

vdenvは仮想デスクトップごとに環境変数およびプロファイルを切り替えるためのdotnetツールです。

## インストール

```cmd
dotnet tool install -g vdenv
```

## 使用方法

ターミナル起動時に以下のように引数を渡します:
```cmd
cmd.exe /k for /f "delims=" %f in ('vdenv') do @call "%f"
```

> [!TIP]
> Windows Terminalの場合、`settings.json` に以下のように設定することで、起動時にvdenvを実行できます:
> ```json
> {
>   "profiles": {
>     "list": [
>       {
>         "commandline": "%SystemRoot%\\System32\\cmd.exe /k for /f \"delims=\" %f in ('vdenv') do @call \"%f\""
>       }
>     ]
>   }
> }
> ```
> UIで設定する場合は以下のように設定します:
> ![設定UI](docs/wt.png)

その他の基本的なコマンド:

- `vdenv init`  
  設定ファイルの初期化を行い、デスクトップ毎の設定を作成します。

- `vdenv config`  
  現在のデスクトップの設定内容を表示します。

- `vdenv config open`  
  設定ファイルをエディタで開きます。

## 設定ファイル (vdenv.yaml)

vdenvはユーザープロファイル直下の `vdenv.yaml` を参照します。

設定例:
```yaml
desktops:
  90e9c8af-e2e9-44e5-8289-7f8d8fb55e21: #　仮想デスクトップの内部管理ID
    exists: true       # デスクトップが存在するかのフラグ (true/false)
    env: {}            # 環境変数のキーと値のペア。例: { "PATH": "C:\\path", "VAR": "value" }
    envPath: ""        # 環境変数を定義した.envファイルへのパス。空の場合は無視されます。
    profilePath: ""    # ログイン時に実行するスクリプト/バッチファイルのパス。空の場合は無視されます。
    startDir: ""       # 初期作業ディレクトリ。例: "C:\\Users\\UserName"
# ...その他のデスクトップ設定...
```

各項目の詳細:

- exists:  
  `init` コマンドによって自動的に設定されるフラグです。デスクトップが存在するかどうかを示します。

- env:  
  各デスクトップ固有の環境変数を定義します。キーが環境変数名、値がその設定値となります。

- envPath:  
  環境変数を一覧定義する外部の.envファイルのパスを指定します。指定がなければ無視されます。

- profilePath:  
  ユーザーの初期化スクリプト (例: CMD/バッチファイル) のパスを指定します。指定された場合、ログイン時等に自動実行されます。

- startDir:  
  作業開始ディレクトリを記載します。設定がある場合、起動時にそのディレクトリへ移動します。

## ライセンス

このプロジェクトは [MIT License](LICENSE) の下でライセンスされています。
