using ConsoleAppFramework;
using VYaml.Serialization;
using WindowsDesktop;

// Set STAThread 
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

VirtualDesktop.Configure();
string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "vdenv.yaml");
var app = ConsoleApp.Create();
app.Add("", (string msg) => Console.WriteLine(msg));
app.Add("init", Init);
app.Add("sum", (int x, int y) => Console.WriteLine(x + y));
app.Run(args);

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
            newDesktops.Add(desktop.Id, new DesktopConfig(true, [], "", ""));
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
