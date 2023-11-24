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
using System.Reflection;
using UnityEngine.InputSystem;
using LC_API;
using LC_API.BundleAPI;
using UnityEngine.Assertions;
using System.IO;
using UnityEditor;

namespace HDLethalCompany
{
    [BepInPlugin(Guid, Name, Ver)]
    public class HDLethalCompany : BaseUnityPlugin
    {
        public const string 
            Guid = "HDLethalCompany",
            Name = "HDLethalCompany-Sligili",
            Ver = "1.4.0";

        //Configuration Entrys
        private static ConfigEntry<float> config_ResMult;
        private static ConfigEntry<bool> config_EnablePostProcessing;
        private static ConfigEntry<bool> config_EnableFog;
        private static ConfigEntry<bool> config_EnableAntialiasing;
        private static ConfigEntry<int> config_FogQuality;
        private static ConfigEntry<int> config_TextureQuality;
        private static ConfigEntry<int> config_LOD;
        private static ConfigEntry<int> config_ShadowmapQuality;

        private Harmony _harmony;

        private void Awake()
        {
            Logger.LogInfo(Guid + " loaded");

            //Configuration File
            config_ResMult = Config.Bind("RESOLUTION", "Value", 2.233f, "Resolution Scale Multiplier - <EXAMPLES -> | 1.000 = 860x520p | 2.233 =~ 1920x1080p | 2.977 = 2560x1440p | 4.465 = 3840x2060p > - The UI scanned elements have slightly incorrect offsets after 3.000");
            config_EnableAntialiasing = Config.Bind("EFFECTS", "EnableAA", false, "Anti-Aliasing (Unity's SMAA)");
            config_EnablePostProcessing = Config.Bind("EFFECTS", "EnablePP", true, "Post-Processing (Color grading)");
            config_TextureQuality = Config.Bind("EFFECTS", "TextureQuality", 3, "Texture Resolution Quality - <PRESETS -> | 0 = VERY LOW (1/8) | 1 = LOW (1/4) | 2 = MEDIUM (1/2) | 3 = HIGH (1/1 VANILLA) >");
            config_FogQuality = Config.Bind("EFFECTS", "FogQuality", 1, "Volumetric Fog Quality - <PRESETS -> | 0 = VERY LOW | 1 = VANILLA FOG | 2 = MEDIUM | 3 = HIGH >");
            config_EnableFog = Config.Bind("EFFECTS", "EnableFOG", true, "Volumetric Fog Toggle - Use this as a last resource in case lowering the fog quality is not enough to get decent performance");
            config_LOD = Config.Bind("EFFECTS", "LOD", 1, "Level Of Detail - <PRESETS -> | 0 = LOW (HALF DISTANCE) | 1 = VANILLA | 2 = HIGH (TWICE THE DISTANCE) >");
            config_ShadowmapQuality = Config.Bind("EFFECTS", "ShadowQuality", 2, "Shadows Resolution - <PRESETS -> 0 = VERY LOW (SHADOWS DISABLED)| 1 = LOW (256) | 2 = MEDIUM (1024) | 3 = VANILLA (2048) > - Shadowmap max resolution");

            GraphicsPatch.m_setShadowQuality = config_ShadowmapQuality.Value;
            GraphicsPatch.m_setLOD = config_LOD.Value;
            GraphicsPatch.m_setTextureResolution = config_TextureQuality.Value;
            GraphicsPatch.m_setFogQuality = config_FogQuality.Value;
            GraphicsPatch.multiplier = config_ResMult.Value;
            GraphicsPatch.OffsetZ = 0.123f * config_ResMult.Value + 0.877f;
            GraphicsPatch.m_widthResolution = 860 * config_ResMult.Value;
            GraphicsPatch.m_heightResolution = 520 * config_ResMult.Value;
            GraphicsPatch.m_enableAntiAliasing = config_EnableAntialiasing.Value;
            GraphicsPatch.m_enableFog = config_EnableFog.Value;
            GraphicsPatch.m_enablePostProcessing = config_EnablePostProcessing.Value;

            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(GraphicsPatch));

            
        }
    }
}

namespace HDLethalCompany.Patch
{
    internal class GraphicsPatch
    {
        internal static object GetInstanceField(Type type, object instance, string fieldName)
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

        //Graphics Settings
        public static bool m_enablePostProcessing;
        public static bool m_enableFog;
        public static bool m_enableAntiAliasing;
        public static int m_setFogQuality; 
        public static int m_setTextureResolution;
        public static int m_setLOD;
        public static int m_setShadowQuality;

        static HDRenderPipelineAsset myAsset;

        //Resolution
        static float anchorOffsetX = 439.48f;
        static float anchorOffsetY = 244.8f;
        public static float multiplier;
        public static float OffsetZ;

        public static float m_widthResolution;
        public static float m_heightResolution;

        int maxShadowResolution=512;

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPrefix]

        private static void StartPrefix(PlayerControllerB __instance)
        {
            GameObject player = __instance.gameObject;
            player.AddComponent<PipelineAsset>();
            UnityEngine.Debug.Log("HDLethalCompanyStart");

            HDAdditionalCameraData Gameplay_hdCameraData = __instance.gameplayCamera.GetComponent<HDAdditionalCameraData>();
            //HDAdditionalCameraData Spectator_hdCameraData = StartOfRound.Instance.spectateCamera.GetComponent<HDAdditionalCameraData>();


            Gameplay_hdCameraData.customRenderingSettings = true;

            //Shadow Quality
            if (m_setShadowQuality != 3)
            {
                switch (m_setShadowQuality)
                {

                    case 0:

                        myAsset = BundleLoader.GetLoadedAsset<HDRenderPipelineAsset>("Assets/HDLethalCompany/VeryLowShadowsAsset.asset");

                        Gameplay_hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.ShadowMaps] = true;
                        Gameplay_hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ShadowMaps, false);
                        break;

                    case 1:

                        myAsset = BundleLoader.GetLoadedAsset<HDRenderPipelineAsset>("Assets/HDLethalCompany/VeryLowShadowsAsset.asset");

                        break;

                    case 2:

                        myAsset = BundleLoader.GetLoadedAsset<HDRenderPipelineAsset>("Assets/HDLethalCompany/MediumShadowsAsset.asset");

                        break;
                }

                QualitySettings.renderPipeline = myAsset;
            }

            //Level Of Detail
            if (m_setLOD != 1)
            {
                Gameplay_hdCameraData.customRenderingSettings = true;

                Gameplay_hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.LODBiasMode] = true;
                Gameplay_hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.LODBias] = true;


                Gameplay_hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LODBiasMode, true);
                Gameplay_hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.LODBias, true);

                Gameplay_hdCameraData.renderingPathCustomFrameSettings.lodBiasMode = LODBiasMode.OverrideQualitySettings;

                switch (m_setLOD)
                {
                    case 0:
                        Gameplay_hdCameraData.renderingPathCustomFrameSettings.lodBias = 0.6f;
                        break;
                    case 1:
                        Gameplay_hdCameraData.renderingPathCustomFrameSettings.lodBias = 1f;
                        break;
                    case 2:
                        Gameplay_hdCameraData.renderingPathCustomFrameSettings.lodBias = 2.3f;
                        break;
                }
            }

            //Texture Resolution
            #region
            if (m_setTextureResolution != 3)
            {
                switch (m_setTextureResolution)
                {
                    case 0: QualitySettings.globalTextureMipmapLimit = 3; break;
                    case 1: QualitySettings.globalTextureMipmapLimit = 2; break;
                    case 2: QualitySettings.globalTextureMipmapLimit = 1; break;
                }
            }
            #endregion

            //SMAA
            #region
            if (m_enableAntiAliasing)
            {
                Gameplay_hdCameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                //spectatorcamera = gameplaycameraantialiasing tatatatatatta
            }
            #endregion

            //HDRP Custom Pass
            #region
            if (!m_enablePostProcessing)
            {          
                Gameplay_hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.CustomPass] = true;
                Gameplay_hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.CustomPass, false);

                //Spectator_hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.CustomPass] = true;
                //Spectator_hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.CustomPass, false);                
            }
            #endregion

            //Volumetric Fog (toggle)
            #region
            if (!m_enableFog)
            {
                Gameplay_hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;
                Gameplay_hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, false);

                //Spectator_hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;
                //Spectator_hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, false);
            }
            #endregion

            //Render Resolution
            #region
            int newWidth = (int)Math.Round(m_widthResolution, 0),
                newHeight = (int)Math.Round(m_heightResolution, 0);
            
            __instance.gameplayCamera.targetTexture.width = newWidth;
            __instance.gameplayCamera.targetTexture.height = newHeight;
            #endregion

        }

        [HarmonyPatch(typeof(HUDManager), "UpdateScanNodes")]
        [HarmonyPostfix]
        private static void UpdateScanNodesPrefix(PlayerControllerB playerScript, HUDManager __instance)
        {

            if (OffsetZ > 1.238f) OffsetZ = 1.238f;

            if (m_setFogQuality != 1)
            {
                Fog fog;
                __instance.playerGraphicsVolume.sharedProfile.TryGet<Fog>(out fog);
                fog.quality.Override(3); //sets preset to custom
                switch (m_setFogQuality)
                {
                    case -1: //VEEERY LOW BackCompat

                        fog.volumetricFogBudget = 0.05f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;
                    case 0: //VEEERY LOW

                        fog.volumetricFogBudget = 0.05f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;
                    case 2: //MEDIUM
                        fog.volumetricFogBudget = 0.333f;
                        fog.resolutionDepthRatio = 0.666f;
                        break;
                    case 3: //HIGH
                        fog.volumetricFogBudget = 0.666f;
                        fog.resolutionDepthRatio = 0.5f;
                        break;
                }

                m_setFogQuality = 1;

            }

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
                        if((bool)CallMethod(__instance, "NodeIsNotVisible", scanNodeProperties, i))
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
                        Debug.LogError(string.Format("Error in updatescanNodes A: {0}", arg));
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

                        __instance.scanElements[i].position = new Vector3(__instance.scanElements[i].position.x, __instance.scanElements[i].position.y, 12.17f * OffsetZ);

                        __instance.scanElements[i].anchoredPosition = new Vector3(vector.x - anchorOffsetX * multiplier, vector.y - anchorOffsetY * multiplier);
                        if (!(multiplier > 3)) __instance.scanElements[i].localScale = new Vector3(multiplier, multiplier, multiplier);
                        else __instance.scanElements[i].localScale = new Vector3(3, 3, 3);

                        goto IL_1CA;
                    }
                    catch (Exception arg2)
                    {
                        Debug.LogError(string.Format("Error in updatescannodes B: {0}", arg2));
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
                __instance.scanInfoAnimator.SetBool("display", (int)GetInstanceField(typeof(HUDManager), __instance, "scannedScrapNum") >= 2 && flag);
            }
            catch (Exception arg3)
            {
                Debug.LogError(string.Format("Error in updatescannodes C: {0}", arg3));
            }
        }


    }

    public class PipelineAsset : MonoBehaviour
    {
        public HDRenderPipelineAsset Asset;

        void Start()
        {
            Asset = (HDRenderPipelineAsset)QualitySettings.renderPipeline;
        }
    }
}
