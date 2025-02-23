using BepInEx.Configuration;

namespace FixMyChoo.Settings;

public class FixMyChooSettings(ConfigFile config)
{
    public ConfigEntry<bool> MySettingsBool = config.Bind<bool>("SectionName", "MySettingsBool", true, "This is an example boolean setting!");
}
