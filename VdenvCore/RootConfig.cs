using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using VYaml.Annotations;
using VYaml.Serialization;

[YamlObject]
public partial record RootConfig(IReadOnlyDictionary<Guid, DesktopConfig>? Desktops);

[YamlObject]
public partial record DesktopConfig(bool Exists, IReadOnlyDictionary<string, string?> Env, string EnvPath, string ProfilePath, string StartDir)
{
    public string GetHash(Guid id, string name)
    {
        var buf = YamlSerializer.Serialize(new HashData(id, name, this));
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
internal partial record HashData(Guid Id, string Name, DesktopConfig Config);
