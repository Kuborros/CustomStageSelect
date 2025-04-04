using BepInEx;
using BepInEx.Logging;
using FP2Lib.Stage;
using System.IO;
using HarmonyLib;
using UnityEngine;
using CustomStageSelect.Patches;
using System.Collections.Generic;

namespace CustomStageSelect;

[BepInPlugin("com.kuborro.plugins.fp2.customstageselect", "Custom Stage Select", "0.1.0")]
[BepInDependency("000.kuborro.libraries.fp2.fp2lib")]
public class CustomStageSelect : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    internal static AssetBundle menuAssets;
    internal static AssetBundle menuScene;

    internal static List<CustomStage> extraStages;

    private void Awake()
    {
        Logger = base.Logger;

        string assetPath = Path.Combine(Paths.GameRootPath, "mod_overrides");
        menuAssets = AssetBundle.LoadFromFile(Path.Combine(assetPath, "customstagemenu.assets"));
        menuScene = AssetBundle.LoadFromFile(Path.Combine(assetPath, "customstagemenu.scene"));

        if (menuAssets == null || menuScene == null)
        {
            Logger.LogError("Failed to load AssetBundles! This mod cannot work without it, exiting. Please reinstall it.");
            return;
        }
        Logger.LogDebug("Scene file contains scenes: " + menuScene.GetAllScenePaths());

        Sprite icon = menuAssets.LoadAsset<Sprite>("StageIcon_Custom");

        //Add the stage select as "HUB" - just like the Battlesphere menu.
        CustomStage menuStageSelect = new CustomStage
        {
            uid = "kuborro.customstageselectmenu",
            name = "Custom Stage Select",
            author = "Kubo",
            description = "Menu used for Custom Stage Select Mod",
            isHUB = true,
            showInCustomStageLoaders = false,
            sceneName = "CustomStageSelect",
            preview = icon
        };

        StageHandler.RegisterStage(menuStageSelect);

        Harmony.CreateAndPatchAll(typeof(PatchMenuBasic));
        Harmony.CreateAndPatchAll(typeof(PatchMenuClassic));
        Harmony.CreateAndPatchAll(typeof(PatchMenuWorldMap));
        Harmony.CreateAndPatchAll(typeof(PatchStageExits));

    }
}
