using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace HDLethalCompany;

internal static class QualitySettingsPatch
{
    internal static void Patch()
    {
        HDLethalCompany.Logger.LogInfo("Applying quality settings");
        var findObjectsOfTypeAll = Resources.FindObjectsOfTypeAll<HDAdditionalCameraData>();
        foreach (var cameraData in findObjectsOfTypeAll)
        {
            if (cameraData.gameObject.name is "UICamera")
                continue;

            cameraData.customRenderingSettings = true;

            cameraData.ToggleCustomPass();

            cameraData.SetLevelOfDetail();

            cameraData.ToggleVolumetricFog();

            new HDAdditionalCameraData();

            if (!ModConfig.EnableFoliage)
                //Layer 10 = Foliage
                cameraData.GetComponent<Camera>().cullingMask &= ~(1 << 10);

            cameraData.SetShadowQuality();

            if (cameraData.gameObject.name is "SecurityCamera" or "ShipCamera") continue;

            cameraData.SetAntiAliasing();
        }

        SetTextureQuality();

        SetFogQuality();

        HDLethalCompany.Logger.LogInfo("Quality settings applied");
    }

    private static void SetShadowQuality(this HDAdditionalCameraData cameraData)
    {
        if (HDLethalCompany.AssetBundle is not { } assetBundle)
        {
            HDLethalCompany.Logger.LogError("Something is wrong with the Asset Bundle - Null");
            return;
        }

        var shadowQuality = ModConfig.SetShadowQuality;

        cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.ShadowMaps] = true;

        cameraData.renderingPathCustomFrameSettings.SetEnabled(
            FrameSettingsField.ShadowMaps, shadowQuality != ModConfig.ShadowQuality.VeryLow
        );

        var asset = shadowQuality switch
        {
            ModConfig.ShadowQuality.Low =>
                assetBundle.LoadAsset<HDRenderPipelineAsset>("Assets/HDLethalCompany/VeryLowShadowsAsset.asset"),
            ModConfig.ShadowQuality.Medium =>
                assetBundle.LoadAsset<HDRenderPipelineAsset>("Assets/HDLethalCompany/MediumShadowsAsset.asset"),
            _ =>
                QualitySettings.renderPipeline
        };

        QualitySettings.renderPipeline = asset;
    }

    private static void SetLevelOfDetail(this HDAdditionalCameraData cameraData)
    {
        if (ModConfig.SetLOD == ModConfig.LOD.Medium)
            return;

        //Setting up custom frame settings

        #region

        cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.LODBiasMode] = true;
        cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.LODBias] = true;

        cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LODBiasMode, true);
        cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LODBias, true);

        cameraData.renderingPathCustomFrameSettings.lodBiasMode = LODBiasMode.OverrideQualitySettings;

        #endregion

        cameraData.renderingPathCustomFrameSettings.lodBias = ModConfig.SetLOD switch
        {
            ModConfig.LOD.Low => 0.6f,
            ModConfig.LOD.High => 2.3f,
            _ => 1f
        };
    }

    private static void SetTextureQuality()
    {
        var setTextureResolution = ModConfig.SetTextureResolution;
        if (setTextureResolution >= ModConfig.TextureQuality.High)
            return;

        QualitySettings.globalTextureMipmapLimit = 3 - (int)setTextureResolution;
    }

    private static void SetAntiAliasing(this HDAdditionalCameraData cameraData)
    {
        cameraData.antialiasing = ModConfig.EnableAntiAliasing
            ? HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing
            : HDAdditionalCameraData.AntialiasingMode.None;
    }

    private static void ToggleCustomPass(this HDAdditionalCameraData cameraData)
    {
        cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.CustomPass] = true;
        cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.CustomPass,
            ModConfig.EnablePostProcessing);
    }

    private static void ToggleVolumetricFog(this HDAdditionalCameraData cameraData)
    {
        cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;
        cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, ModConfig.EnableFog);
    }

    internal static void SetFogQuality()
    {
        foreach (var volume in Resources.FindObjectsOfTypeAll<Volume>())
        {
            if (!volume.sharedProfile.TryGet(out Fog fog))
                continue;

            fog.quality.Override(3);
            switch (ModConfig.SetFogQuality)
            {
                case ModConfig.FogQuality.VeryLow:
                    fog.volumetricFogBudget = 0.05f;
                    fog.resolutionDepthRatio = 0.5f;
                    break;
                case ModConfig.FogQuality.Medium:
                    fog.volumetricFogBudget = 0.333f;
                    fog.resolutionDepthRatio = 0.666f;
                    break;
                case ModConfig.FogQuality.High:
                    fog.volumetricFogBudget = 0.666f;
                    fog.resolutionDepthRatio = 0.5f;
                    break;
                case ModConfig.FogQuality.Low:
                default:
                    fog.volumetricFogBudget = 0.166f;
                    fog.resolutionDepthRatio = 0.666f;
                    break;
            }
        }
    }
}