using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using ConsoleAppFramework;
using dotenv.net;
using VYaml.Serialization;
using WindowsDesktop;

// Set STAThread 
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

VirtualDesktop.Configure();
string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "vdenv.yaml");
var app = ConsoleApp.Create();
app.Add("", Root);
app.Add("init", Init);
app.Add("config", PrintConfig);
app.Add("config open", () => Process.Start(new ProcessStartInfo { FileName = configPath, UseShellExecute = true }));
app.Run(args);

/// <summary>
/// コンフィグを読み込んでデスクトップ毎に環境変数を設定します
/// </summary>
async Task<int> Root()
{
    var bat = new StringBuilder();
    bat.AppendLine("@echo off");
    try
    {
        if (!File.Exists(configPath))
        {
            bat.AppendLine("echo Config file not found. Run `vdenv init`.");
            return 1;
        }
        var buf = await File.ReadAllBytesAsync(configPath);
        var config = YamlSerializer.Deserialize<RootConfig>(buf);

        var current = VirtualDesktop.Current;
        if (!(config.Desktops?.TryGetValue(current.Id, out var desktop) ?? false))
        {
            bat.AppendLine($"echo `{current.Name}({current.Id})` not found in config.");
            return 1;
        }

        foreach (var (key, value) in desktop.Env)
        {
            bat.AppendLine($"set {key}={value}");
        }

        if (!string.IsNullOrEmpty(desktop.EnvPath))
        {
            try
            {
                var envVars = DotEnv.Fluent()
                    .WithEnvFiles(desktop.EnvPath)
                    .Read();
                foreach (var (key, value) in envVars)
                {
                    bat.AppendLine($"set {key}={value}");
                }
            }
            catch (Exception e)
            {
                bat.AppendLine($"echo {e.Message}");
                return 1;
            }
        }

        if (!string.IsNullOrEmpty(desktop.ProfilePath))
        {
            bat.AppendLine($"call \"{desktop.ProfilePath}\"");
        }

        if (!string.IsNullOrEmpty(desktop.StartDir))
        {
            bat.AppendLine($"cd /d \"{desktop.StartDir}\"");
        }

        return 0;
    }
    finally
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bat");
        await File.WriteAllTextAsync(tempPath, bat.ToString());
        Console.Write(tempPath);
    }
}

/// <summary>
/// コンフィグ初期化
/// </summary>
/// <param name="reset">既存の設定をリセットする</param>
/// <param name="prune">存在しないデスクトップ設定を削除する</param>
async Task Init(bool reset = false, bool prune = false)
{
    IReadOnlyDictionary<Guid, DesktopConfig>? oldDesktops = null;
    if (File.Exists(configPath) && !reset)
    {
        var buf = await File.ReadAllBytesAsync(configPath);
        oldDesktops = YamlSerializer.Deserialize<RootConfig>(buf).Desktops;
    }

    var newDesktops = new Dictionary<Guid, DesktopConfig>();

    foreach (var desktop in VirtualDesktop.GetDesktops())
    {
        if (oldDesktops?.TryGetValue(desktop.Id, out var oldDesktop) ?? false)
        {
            newDesktops.Add(desktop.Id, oldDesktop);
        }
        else
        {
            newDesktops.Add(desktop.Id, new DesktopConfig(true, ReadOnlyDictionary<string, string?>.Empty, "", "", ""));
        }
    }
    if (!prune && oldDesktops is not null)
    {
        foreach (var (id, desktop) in oldDesktops)
        {
            newDesktops.TryAdd(id, desktop with { Exists = false });
        }
    }

    // 書き込み
    using var fs = new FileStream(configPath, FileMode.Create, FileAccess.Write);
    await fs.WriteAsync(YamlSerializer.Serialize(new RootConfig(newDesktops)));
}

/// <summary>
/// 現在のデスクトップの設定を表示
/// </summary>
/// <param name="all">全ての設定を表示</param>
async Task PrintConfig(bool all = false)
{
    if (!File.Exists(configPath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Config file not found. Run `vdenv init`.");
        Console.ResetColor();
        return;
    }
    var buf = await File.ReadAllBytesAsync(configPath);
    if (all)
    {
        Console.Write(Encoding.UTF8.GetString(buf));
        return;
    }

    var config = YamlSerializer.Deserialize<RootConfig>(buf);

    var current = VirtualDesktop.Current;
    if (config.Desktops?.TryGetValue(current.Id, out var desktop) ?? false)
    {
        Console.WriteLine(current.Name);
        Console.WriteLine("---");
        Console.WriteLine(YamlSerializer.SerializeToString(desktop));
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"`{current.Name}({current.Id})` not found in config.");
        Console.ResetColor();
    }
}