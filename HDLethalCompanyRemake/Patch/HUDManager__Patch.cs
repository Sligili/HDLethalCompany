using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace HDLethalCompany.Patch;

[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManager__Patch
{
    private static readonly AccessTools.FieldRef<HUDManager, Dictionary<RectTransform, ScanNodeProperties>>
        ScanNodesRef =
            AccessTools.FieldRefAccess<HUDManager, Dictionary<RectTransform, ScanNodeProperties>>("scanNodes");

    [HarmonyPatch("UpdateScanNodes")]
    [HarmonyPostfix]
    private static void UpdateScanNodes__Postfix(PlayerControllerB playerScript, HUDManager __instance)
    {
        if (!ModConfig.EnableResolutionFix)
            return;

        var scanNodes = ScanNodesRef(__instance);
        var halfWidth = ModConfig.WidthResolution / 2f;
        var halfHeight = ModConfig.HeightResolution / 2f;
        var widthMultiplier = ModConfig.WidthResolution / 860f;
        var heightMultiplier = ModConfig.HeightResolution / 520f;
        foreach (var scanElement in __instance.scanElements)
        {
            if (!scanNodes.TryGetValue(scanElement, out var scanNodeProperties))
                continue;

            var vector = playerScript.gameplayCamera.WorldToScreenPoint(scanNodeProperties.transform.position);
            scanElement.anchoredPosition = vector with
            {
                x = (vector.x - halfWidth) / widthMultiplier,
                y = (vector.y - halfHeight) / heightMultiplier
            };
        }
    }
}