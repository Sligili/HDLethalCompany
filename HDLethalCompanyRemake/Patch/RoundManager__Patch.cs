using HarmonyLib;
using UnityEngine;

namespace HDLethalCompany.Patch;

[HarmonyPatch(typeof(RoundManager))]
internal class RoundManager__Patch
{
    [HarmonyPatch("GenerateNewFloor")]
    [HarmonyPostfix]
    private static void GenerateNewFloor__Postfix()
    {
        //The weather system overrides the fog settings without this
        QualitySettingsPatch.SetFogQuality();

        if (ModConfig.SetLOD != 0)
            return;
        RemoveLodFromGameObject("CatwalkStairs");
    }

    private static void RemoveLodFromGameObject(string name)
    {
        foreach (var lodGroup in Resources.FindObjectsOfTypeAll<LODGroup>())
        {
            if (lodGroup.gameObject.name != name)
                continue;

            lodGroup.enabled = false;
        }
    }
}