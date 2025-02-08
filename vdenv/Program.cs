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

async Task Init()
{
    var config = new RootConfig(VirtualDesktop.GetDesktops().ToDictionary(d => d.Id, d => new DesktopConfig(true, [], string.Empty, string.Empty)));
    using var fs = new FileStream(configPath, FileMode.Create, FileAccess.Write);
    await fs.WriteAsync(YamlSerializer.Serialize(config));
}
