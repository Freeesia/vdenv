using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using VYaml.Annotations;
using VYaml.Serialization;

[YamlObject]
partial record RootConfig(IReadOnlyDictionary<Guid, DesktopConfig>? Desktops);

[YamlObject]
partial record DesktopConfig(bool Exists, IReadOnlyDictionary<string, string?> Env, string EnvPath, string ProfilePath, string StartDir)
{
    public string GetHash(Guid id)
    {
        var buf = YamlSerializer.Serialize(KeyValuePair.Create(id, this));
#if DEBUG
        Debug.WriteLine(Encoding.UTF8.GetString(buf.Span));
#endif
        Span<byte> hashBytes = stackalloc byte[18];
        MD5.TryHashData(buf.Span, hashBytes, out _);
        BitConverter.TryWriteBytes(hashBytes[16..], (ushort)buf.Length);
        return Convert.ToHexString(hashBytes);
    }
}