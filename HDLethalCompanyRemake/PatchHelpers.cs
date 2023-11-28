using UnityEngine;

namespace HDLethalCompany;

internal static class PatchHelpers
{
    internal static void ReapplyTargetTextures()
    {
        if (!ModConfig.EnableResolutionFix)
            return;

        foreach (var camera in Resources.FindObjectsOfTypeAll<Camera>())
            if (camera is { name: "MainCamera" })
                // To trigger prefix
                camera.targetTexture = camera.targetTexture;
    }
}