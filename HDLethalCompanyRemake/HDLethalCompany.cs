using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HDLethalCompany;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class HDLethalCompany : BaseUnityPlugin
{
    internal static AssetBundle AssetBundle;
    private Harmony _harmony;
    internal new static ManualLogSource Logger { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;

        ModConfig.Init(Config);

        AssetBundle = AssetBundle.LoadFromFile(Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Paths.PluginPath,
            "HDLethalCompany/hdlethalcompany"
        ));

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll();

        SceneManager.sceneLoaded += OnActiveSceneChanged;

        Logger.LogInfo("Loaded");
    }

    private static void OnActiveSceneChanged(Scene scene, LoadSceneMode loadSceneMode)
    {
        PatchHelpers.ReapplyTargetTextures();
        QualitySettingsPatch.Patch();
    }
}