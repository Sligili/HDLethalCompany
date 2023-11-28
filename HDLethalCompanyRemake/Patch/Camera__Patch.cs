using HarmonyLib;
using UnityEngine;

namespace HDLethalCompany.Patch;

[HarmonyPatch(typeof(Camera))]
public class Camera__Patch
{
    [HarmonyPatch("targetTexture", MethodType.Setter)]
    [HarmonyPrefix]
    private static void TargetTexture__Prefix(Camera __instance, ref RenderTexture value)
    {
        if (value == null)
            return;

        var width = ModConfig.EnableResolutionFix ? ModConfig.WidthResolution : 860;
        var height = ModConfig.EnableResolutionFix ? ModConfig.HeightResolution : 520;
        if (value.width == width && value.height == height)
            return;

        __instance.targetTexture = null;

        value.Release();
        value.width = width;
        value.height = height;
        value.Create();
    }
}