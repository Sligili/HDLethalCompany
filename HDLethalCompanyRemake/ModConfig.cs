using System;
using BepInEx.Configuration;
using UnityEngine.Device;

namespace HDLethalCompany;

internal static class ModConfig
{
    // Graphics Settings
    internal static bool EnablePostProcessing { get; private set; }
    internal static bool EnableFog { get; private set; }
    internal static bool EnableAntiAliasing { get; private set; }

    internal static bool EnableResolutionFix { get; private set; }

    internal static bool EnableFoliage { get; private set; }

    internal static FogQuality SetFogQuality { get; private set; }
    internal static TextureQuality SetTextureResolution { get; private set; }
    internal static LOD SetLOD { get; private set; }
    internal static ShadowQuality SetShadowQuality { get; private set; }

    // Resolution
    internal static int WidthResolution { get; private set; }
    internal static int HeightResolution { get; private set; }

    internal static void Init(ConfigFile configFile)
    {
        ConfigEntries.EnableResolutionFix = configFile.Bind("RESOLUTION", "EnableFix", true,
            "Resolution Fix - In case you wanna use another resolution mod or apply any widescreen mod while keeping the graphics settings");
        ConfigEntries.FromWindowWidth = configFile.Bind("RESOLUTION", "FromWindowWidth", false,
            "Resolution Scale From Window Size. Overrides TargetWidth");
        ConfigEntries.ResolutionWidth = configFile.Bind("RESOLUTION", "TargetWidth", 1920,
            "Resolution Scale Target Width. For example 1920. Game default is 860");

        ConfigEntries.EnableAntiAliasing = configFile.Bind("EFFECTS", "EnableAA", false,
            "Anti-Aliasing (Unity's SMAA)");
        ConfigEntries.EnablePostProcessing = configFile.Bind("EFFECTS", "EnablePP", true,
            "Post-Processing (Color grading)");
        ConfigEntries.EnableFog = configFile.Bind("EFFECTS", "EnableFOG", true,
            "Volumetric Fog Toggle - Use this as a last resource in case lowering the fog quality is not enough to get decent performance");
        ConfigEntries.FogQuality = configFile.Bind("EFFECTS", "FogQuality", FogQuality.Low,
            "Volumetric Fog Quality - <Low => Vanilla>");
        ConfigEntries.TextureQuality = configFile.Bind("EFFECTS", "TextureQuality", TextureQuality.High,
            "Texture Resolution Quality - <VeryLow => 1/8 | Low => 1/4 | Medium => 1/2 | High => 1/1 Vanilla>");
        ConfigEntries.LOD = configFile.Bind("EFFECTS", "LOD", LOD.Medium,
            "Level Of Detail - <Low => half distance | Medium => Vanilla | High => twice distance>");
        ConfigEntries.ShadowQuality = configFile.Bind("EFFECTS", "ShadowQuality", ShadowQuality.High,
            "Shadows Resolution - <VeryLow => shadows disabled | Low => 256 | Medium => 1024 | High => 2048 Vanilla> - Shadowmap max resolution");
        ConfigEntries.EnableFoliage = configFile.Bind("EFFECTS", "EnableF", true,
            "Foliage Toggle - If the game camera should or not render bushes/grass (trees won't be affected)");

        LegacyPostInit(configFile);

        WidthResolution = ConfigEntries.FromWindowWidth.Value ? Screen.width : ConfigEntries.ResolutionWidth.Value;
        HeightResolution = (int)MathF.Round(520f * (WidthResolution / 860f), 0);
        EnableResolutionFix = ConfigEntries.EnableResolutionFix.Value && WidthResolution != 860;

        EnablePostProcessing = ConfigEntries.EnablePostProcessing.Value;
        EnableFog = ConfigEntries.EnableFog.Value;
        EnableAntiAliasing = ConfigEntries.EnableAntiAliasing.Value;

        EnableFoliage = ConfigEntries.EnableFoliage.Value;

        SetFogQuality = ConfigEntries.FogQuality.Value;
        SetTextureResolution = ConfigEntries.TextureQuality.Value;
        SetLOD = ConfigEntries.LOD.Value;
        SetShadowQuality = ConfigEntries.ShadowQuality.Value;
    }

    private static void LegacyPostInit(ConfigFile configFile)
    {
        configFile.SaveOnConfigSet = false;

        // Old config file used to have this value - Back Compat to old Very Low value
        if (ConfigEntries.FogQuality.Value == (FogQuality)(-1)) ConfigEntries.FogQuality.Value = FogQuality.VeryLow;

        var enableResEntry = new ConfigDefinition("RESOLUTION", "EnableRes");
        var enableRes = configFile.Bind(enableResEntry, true).Value;
        if (!enableRes)
            ConfigEntries.EnableResolutionFix.Value = false;
        configFile.Remove(enableResEntry);

        var valueEntry = new ConfigDefinition("RESOLUTION", "Value");
        var resolutionValue = configFile.Bind(valueEntry, 0f).Value;
        if (resolutionValue != 0f)
            ConfigEntries.ResolutionWidth.Value = (int)MathF.Round(860f * resolutionValue, 0);
        configFile.Remove(valueEntry);

        configFile.SaveOnConfigSet = true;
        configFile.Save();
    }

    internal enum FogQuality
    {
        VeryLow = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    internal enum TextureQuality
    {
        VeryLow = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    internal enum LOD
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    internal enum ShadowQuality
    {
        VeryLow = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    private static class ConfigEntries
    {
        internal static ConfigEntry<bool>
            FromWindowWidth,
            EnablePostProcessing,
            EnableFog,
            EnableAntiAliasing,
            EnableResolutionFix,
            EnableFoliage;

        internal static ConfigEntry<int> ResolutionWidth;

        internal static ConfigEntry<FogQuality> FogQuality;
        internal static ConfigEntry<TextureQuality> TextureQuality;
        internal static ConfigEntry<LOD> LOD;
        internal static ConfigEntry<ShadowQuality> ShadowQuality;
    }
}