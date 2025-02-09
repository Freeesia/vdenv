using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using VYaml.Annotations;
using VYaml.Serialization;
using WindowsDesktop;

[YamlObject]
partial record RootConfig(IReadOnlyDictionary<Guid, DesktopConfig>? Desktops);

[YamlObject]
partial record DesktopConfig(bool Exists, IReadOnlyDictionary<string, string?> Env, string EnvPath, string ProfilePath, string StartDir)
{
    public string GetHash(VirtualDesktop desktop)
    {
        var buf = YamlSerializer.Serialize(new HashData(desktop.Id, desktop.Name, this));
#if DEBUG
        Debug.WriteLine(Encoding.UTF8.GetString(buf.Span));
#endif
        Span<byte> hashBytes = stackalloc byte[18];
        MD5.TryHashData(buf.Span, hashBytes, out _);
        BitConverter.TryWriteBytes(hashBytes[16..], (ushort)buf.Length);
        return Convert.ToHexString(hashBytes);
    }
}

[YamlObject]
partial record HashData(Guid Id, string Name, DesktopConfig Config);
