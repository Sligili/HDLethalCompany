using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using GameNetcodeStuff;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using TMPro;
using HDLethalCompany.Patch;
using HDLethalCompany.Tools;
using System.Reflection;
using System.IO;
using UnityEngine.Rendering;

namespace HDLethalCompany
{
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Ver)]
    public class HDLethalCompanyInitialization : BaseUnityPlugin
    {
        //Configuration Entrys
        private static ConfigEntry<float> 
            config_ResMult;

        private static ConfigEntry<bool>
            config_EnablePostProcessing,
            config_EnableFog,
            config_EnableAntialiasing,
            config_EnableResolution,
            config_EnableFoliage;             

        private static ConfigEntry<int> 
            config_FogQuality, 
            config_TextureQuality, 
            config_LOD, 
            config_ShadowmapQuality;


        private Harmony _harmony;

        private void Awake()
        {
            Logger.LogInfo(PluginInfo.Guid + " loaded");

            ConfigFile();

            //Load asset bundle
            GraphicsPatch.assetBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "HDLethalCompany/hdlethalcompany"));

            _harmony = new Harmony(PluginInfo.Guid);
            _harmony.PatchAll(typeof(GraphicsPatch));
            
        }

        private void ConfigFile()
        {
            //Setup Config File
            #region
            config_ResMult = Config.Bind("RESOLUTION", "Value", 2.233f, "Resolution Scale Multiplier - <EXAMPLES -> | 1.000 = 860x520p | 2.233 =~ 1920x1080p | 2.977 = 2560x1440p | 4.465 = 3840x2060p > - The UI scanned elements have slightly incorrect offsets after 3.000");
            config_EnableResolution = Config.Bind("RESOLUTION", "EnableRes", true, "Resolution Fix - In case you wanna use another resolution mod or apply any widescreen mod while keeping the graphics settings");
            config_EnableAntialiasing = Config.Bind("EFFECTS", "EnableAA", false, "Anti-Aliasing (Unity's SMAA)");
            config_EnablePostProcessing = Config.Bind("EFFECTS", "EnablePP", true, "Post-Processing (Color grading)");
            config_TextureQuality = Config.Bind("EFFECTS", "TextureQuality", 3, "Texture Resolution Quality - <PRESETS -> | 0 = VERY LOW (1/8) | 1 = LOW (1/4) | 2 = MEDIUM (1/2) | 3 = HIGH (1/1 VANILLA) >");
            config_FogQuality = Config.Bind("EFFECTS", "FogQuality", 1, "Volumetric Fog Quality - <PRESETS -> | 0 = VERY LOW | 1 = VANILLA FOG | 2 = MEDIUM | 3 = HIGH >");
            config_EnableFog = Config.Bind("EFFECTS", "EnableFOG", true, "Volumetric Fog Toggle - Use this as a last resource in case lowering the fog quality is not enough to get decent performance");
            config_LOD = Config.Bind("EFFECTS", "LOD", 1, "Level Of Detail - <PRESETS -> | 0 = LOW (HALF DISTANCE) | 1 = VANILLA | 2 = HIGH (TWICE THE DISTANCE) >");
            config_ShadowmapQuality = Config.Bind("EFFECTS", "ShadowQuality", 3, "Shadows Resolution - <PRESETS -> 0 = VERY LOW (SHADOWS DISABLED)| 1 = LOW (256) | 2 = MEDIUM (1024) | 3 = VANILLA (2048) > - Shadowmap max resolution");
            config_EnableFoliage = Config.Bind("EFFECTS", "EnableF", true, "Foliage Toggle - If the game camera should or not render bushes/grass (trees won't be affected)");
            #endregion

            //Load Config File
            #region
            GraphicsPatch.m_enableFoliage = config_EnableFoliage.Value;
            GraphicsPatch.m_enableResolutionFix = config_EnableResolution.Value;
            GraphicsPatch.m_setShadowQuality = config_ShadowmapQuality.Value;
            GraphicsPatch.m_setLOD = config_LOD.Value;
            GraphicsPatch.m_setTextureResolution = config_TextureQuality.Value;
            GraphicsPatch.m_setFogQuality = config_FogQuality.Value;
            GraphicsPatch.multiplier = config_ResMult.Value;
            GraphicsPatch.anchorOffsetZ = 0.123f * config_ResMult.Value + 0.877f; //This formula works
            GraphicsPatch.m_widthResolution = 860 * config_ResMult.Value;
            GraphicsPatch.m_heightResolution = 520 * config_ResMult.Value;
            GraphicsPatch.m_enableAntiAliasing = config_EnableAntialiasing.Value;
            GraphicsPatch.m_enableFog = config_EnableFog.Value;
            GraphicsPatch.m_enablePostProcessing = config_EnablePostProcessing.Value;
            #endregion
        }
    }
    public static class PluginInfo
    {
        public const string
            Guid = "HDLethalCompany",
            Name = "HDLethalCompany-Sligili",
            Ver = "1.5.6";
    }
}

namespace HDLethalCompany.Patch
{
    internal class GraphicsPatch
    {
        //Graphics Settings
        public static bool
            m_enablePostProcessing,
            m_enableFog,
            m_enableAntiAliasing,
            m_enableResolutionFix,
            m_enableFoliage=true;
        
        public static int
            m_setFogQuality,
            m_setTextureResolution,
            m_setLOD,
            m_setShadowQuality;

        static HDRenderPipelineAsset myAsset;
        public static AssetBundle assetBundle;

        //Resolution Fix
        //Anchor offsets
        public static float
            anchorOffsetX = 439.48f,
            anchorOffsetY = 244.8f,
            anchorOffsetZ;

        //Resolution multiplier
        public static float multiplier;

        //Resolution
        public static float
            m_widthResolution,
            m_heightResolution;

        //Others

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPrefix]
        private static void StartPrefix(PlayerControllerB __instance)
        {
            UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(HDAdditionalCameraData));

            for (int i = 0; i < array.Length; i++)
            {
                HDAdditionalCameraData cameraData = array[i] as HDAdditionalCameraData;

                if (cameraData.gameObject.name == "MapCamera") continue;

                cameraData.customRenderingSettings = true;

                ToggleCustomPass(cameraData, m_enablePostProcessing);

                SetLevelOfDetail(cameraData);

                ToggleVolumetricFog(cameraData, m_enableFog);

                if (!m_enableFoliage)
                {
                    LayerMask mask = cameraData.GetComponent<Camera>().cullingMask;
                    
                    //Layer 10 = Foliage
                    mask &= ~(1 << 10);

                    cameraData.GetComponent<Camera>().cullingMask = mask;
                }

                SetShadowQuality(assetBundle, cameraData);

                if (cameraData.gameObject.name == "SecurityCamera" || cameraData.gameObject.name == "ShipCamera") continue;

                SetAntiAliasing(cameraData);

                                      
            }

            array = null;

            SetTextureQuality();

            SetFogQuality();

            if (m_enableResolutionFix && multiplier!=1.000f)
            {
                int newWidth = (int)Math.Round(m_widthResolution, 0),
                newHeight = (int)Math.Round(m_heightResolution, 0);

                __instance.gameplayCamera.targetTexture.width = newWidth;
                __instance.gameplayCamera.targetTexture.height = newHeight;
            }
        }

        [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
        [HarmonyPrefix]
        private static void RoundPostFix()
        {
            //The weather system overrides the fog settings without this
            SetFogQuality();
            if (m_setLOD != 0) return;
            RemoveLodFromGameObject("CatwalkStairs");

        }

        //SCANNER FIX
        [HarmonyPatch(typeof(HUDManager), "UpdateScanNodes")]
        [HarmonyPostfix]
        private static void UpdateScanNodesPostfix(PlayerControllerB playerScript, HUDManager __instance)
        {

            //Scanned elements won't render above this value
            if (anchorOffsetZ > 1.238f) anchorOffsetZ = 1.238f;

            if (!m_enableResolutionFix || multiplier==1.000f) return;

            //Honestly, this part should be remade to improve compatibility (and performance), but sorry, im lazy af.
            Vector3 vector = Vector3.zero;
            bool flag = false;
            int i = 0;
            while (i < __instance.scanElements.Length)
            {
                ScanNodeProperties scanNodeProperties;
                if ((Traverse.Create(__instance).Field("scanNodes").GetValue() as Dictionary<RectTransform, ScanNodeProperties>).Count > 0 && (Traverse.Create(__instance).Field("scanNodes").GetValue() as Dictionary<RectTransform, ScanNodeProperties>).TryGetValue(__instance.scanElements[i], out scanNodeProperties) && scanNodeProperties != null)
                {
                    try
                    {
                        if ((bool)Reflection.CallMethod(__instance, "NodeIsNotVisible", scanNodeProperties, i))
                        {

                            goto IL_1CA;
                        }
                        if (!__instance.scanElements[i].gameObject.activeSelf)
                        {

                            __instance.scanElements[i].gameObject.SetActive(true);
                            __instance.scanElements[i].GetComponent<Animator>().SetInteger("colorNumber", scanNodeProperties.nodeType);

                            if (scanNodeProperties.creatureScanID != -1)
                            {

                                Traverse.Create(__instance).Method("AttemptScanNewCreature", scanNodeProperties.creatureScanID);
                            }
                        }
                    }
                    catch (Exception arg)
                    {
                        UnityEngine.Debug.LogError(string.Format("Error in updatescanNodes A: {0}", arg));
                    }
                    try
                    {
                        Traverse.Create(__instance).Field("scanElementText").SetValue(__instance.scanElements[i].gameObject.GetComponentsInChildren<TextMeshProUGUI>());
                        if ((Traverse.Create(__instance).Field("scanElementText").GetValue() as TextMeshProUGUI[]).Length > 1)
                        {
                            (Traverse.Create(__instance).Field("scanElementText").GetValue() as TextMeshProUGUI[])[0].text = scanNodeProperties.headerText;
                            (Traverse.Create(__instance).Field("scanElementText").GetValue() as TextMeshProUGUI[])[1].text = scanNodeProperties.subText;
                        }
                        if (scanNodeProperties.nodeType == 2)
                        {
                            flag = true;
                        }
                        vector = playerScript.gameplayCamera.WorldToScreenPoint(scanNodeProperties.transform.position);

                        __instance.scanElements[i].position = new Vector3(__instance.scanElements[i].position.x, __instance.scanElements[i].position.y, 12.17f * anchorOffsetZ);

                        __instance.scanElements[i].anchoredPosition = new Vector3(vector.x - anchorOffsetX * multiplier, vector.y - anchorOffsetY * multiplier);
                        if (!(multiplier > 3)) __instance.scanElements[i].localScale = new Vector3(multiplier, multiplier, multiplier);
                        else __instance.scanElements[i].localScale = new Vector3(3, 3, 3);

                        goto IL_1CA;
                    }
                    catch (Exception arg2)
                    {
                        UnityEngine.Debug.LogError(string.Format("Error in updatescannodes B: {0}", arg2));
                        goto IL_1CA;
                    }
                    goto IL_1A3;
                }
                goto IL_1A3;
            IL_1CA:
                i++;
                continue;
            IL_1A3:
                (Traverse.Create(__instance).Field("scanNodes").GetValue() as Dictionary<RectTransform, ScanNodeProperties>).Remove(__instance.scanElements[i]);
                __instance.scanElements[i].gameObject.SetActive(false);
                goto IL_1CA;
            }
            try
            {
                if (!flag)
                {
                    __instance.totalScrapScanned = 0;
                    Traverse.Create(__instance).Field("totalScrapScannedDisplayNum").SetValue(0);
                    Traverse.Create(__instance).Field("addToDisplayTotalInterval").SetValue(0.35f);
                }
                __instance.scanInfoAnimator.SetBool("display", (int)Reflection.GetInstanceField(typeof(HUDManager), __instance, "scannedScrapNum") >= 2 && flag);
            }
            catch (Exception arg3)
            {
                UnityEngine.Debug.LogError(string.Format("Error in updatescannodes C: {0}", arg3));
            }
        }

        //QUALITY METHODS
        public static void SetShadowQuality(AssetBundle assetBundle, HDAdditionalCameraData cameraData)
        {
            if (assetBundle == null)
            {
                Debug.LogError("HDLETHALCOMPANY: Something is wrong with the Asset Bundle - Null");
                return;
            }

            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.ShadowMaps] = true;

            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ShadowMaps, m_setShadowQuality == 0 ? false : true);

            myAsset = (m_setShadowQuality == 1 ? myAsset = assetBundle.LoadAsset<HDRenderPipelineAsset>("Assets/HDLethalCompany/VeryLowShadowsAsset.asset")
                : (m_setShadowQuality == 2 ? myAsset = assetBundle.LoadAsset<HDRenderPipelineAsset>("Assets/HDLethalCompany/MediumShadowsAsset.asset") : (HDRenderPipelineAsset)QualitySettings.renderPipeline));

            QualitySettings.renderPipeline = myAsset;

        }

        public static void SetLevelOfDetail(HDAdditionalCameraData cameraData)
        {
            if (m_setLOD == 1) return;

            //Setting up custom frame settings
            #region
            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.LODBiasMode] = true;
            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.LODBias] = true;

            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LODBiasMode, true);
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LODBias, true);

            cameraData.renderingPathCustomFrameSettings.lodBiasMode = LODBiasMode.OverrideQualitySettings;
            #endregion

            cameraData.renderingPathCustomFrameSettings.lodBias = m_setLOD == 0 ? 0.6f : 2.3f;

            if (m_setLOD == 0 && cameraData.GetComponent<Camera>().farClipPlane>180) cameraData.GetComponent<Camera>().farClipPlane = 170f;

        }

        public static void SetTextureQuality()
        {
            if (m_setTextureResolution >= 3) return;

            int textureQuality = 3 - m_setTextureResolution;

            QualitySettings.globalTextureMipmapLimit = textureQuality;

        }

        public static void SetAntiAliasing(HDAdditionalCameraData cameraData)
        {
            if (!m_enableAntiAliasing) return;

            cameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        }

        public static void ToggleCustomPass(HDAdditionalCameraData cameraData, bool enable)
        {
            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.CustomPass] = true;
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.CustomPass, enable);
        }

        public static void ToggleVolumetricFog(HDAdditionalCameraData cameraData, bool enable)
        {
            cameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;
            cameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, enable);
        }

        public static void SetFogQuality()
        {

            Fog fog;

            UnityEngine.Object[] volumes = Resources.FindObjectsOfTypeAll(typeof(Volume));

            if (volumes.Length == 0)
            {
                Debug.LogError("No volumes found");
                return;
            }

            for (int i = 0; i < volumes.Length; i++)
            {

                Volume vol = volumes[i] as Volume;

                if (!vol.sharedProfile.TryGet<Fog>(out fog)) continue;

                fog.quality.Override(3);

                switch (m_setFogQuality)
                {
                    case -1: //Old config file used to have this value - Back Compat to old Very Low value
                        fog.volumetricFogBudget = 0.05f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;

                    case 0: //Very Low
                        fog.volumetricFogBudget = 0.05f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;

                    case 2: //Medium

                        fog.volumetricFogBudget = 0.333f;
                        fog.resolutionDepthRatio = 0.666f;
                        break;

                    case 3: //High

                        fog.volumetricFogBudget = 0.666f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;
                }
            }
        }

        //FIXES
        public static void RemoveLodFromGameObject(string name)
        {
            UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(LODGroup));

            for (int i = 0; i < array.Length; i++)
            {
                LODGroup lod = array[i] as LODGroup;

                if (!(lod.gameObject.name == name)) continue;

                lod.enabled = false;
            }
        }


    }
}

namespace HDLethalCompany.Tools
    {
        public class Reflection
        {
            public static object GetInstanceField(Type type, object instance, string fieldName)
            {
                BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    | BindingFlags.Static;
                FieldInfo field = type.GetField(fieldName, bindFlags);
                return field.GetValue(instance);
            }
            public static object CallMethod(object instance, string methodName, params object[] args)
            {
                var mi = instance.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (mi != null)
                {
                    return mi.Invoke(instance, args);
                }
                return null;
            }
        }
    }

    