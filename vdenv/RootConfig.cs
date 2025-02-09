using System.Security.Cryptography;
using VYaml.Annotations;
using VYaml.Serialization;

[YamlObject]
partial record RootConfig(IReadOnlyDictionary<Guid, DesktopConfig>? Desktops);

[YamlObject]
partial record DesktopConfig(bool Exists, IReadOnlyDictionary<string, string?> Env, string EnvPath, string ProfilePath, string StartDir)
{
    public string GetHash()
    {
        var buf = YamlSerializer.Serialize(this);
        Span<byte> hashBytes = stackalloc byte[18];
        MD5.TryHashData(buf.Span, hashBytes, out _);
        BitConverter.TryWriteBytes(hashBytes[16..], (ushort)buf.Length);
        return Convert.ToHexString(hashBytes);
    }
}