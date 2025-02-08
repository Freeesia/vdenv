using VYaml.Annotations;

[YamlObject]
partial record RootConfig(IReadOnlyDictionary<Guid, DesktopConfig>? Desktops);

[YamlObject]
partial record DesktopConfig(bool Exists, IReadOnlyDictionary<string, string?> Env, string EnvPath, string ProfilePath, string StartDir);