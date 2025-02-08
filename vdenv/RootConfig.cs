using VYaml.Annotations;

[YamlObject]
partial record RootConfig(IReadOnlyDictionary<Guid, DesktopConfig> Desktops);

[YamlObject]
partial record DesktopConfig(bool Exists, IReadOnlyList<string> Env, string EnvPath, string ProfilePath);