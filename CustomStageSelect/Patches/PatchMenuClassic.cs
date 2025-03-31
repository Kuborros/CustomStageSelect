using FP2Lib.Stage;
using HarmonyLib;
using System;
using UnityEngine;

namespace CustomStageSelect.Patches
{
    internal class PatchMenuClassic
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuClassic), "Start", MethodType.Normal)]
        static void PatchMenuClassicStart(MenuClassic __instance)
        {
            //Append our tile to Classic Mode map.
            GameObject mapTilePrefab = CustomStageSelect.menuAssets.LoadAsset<GameObject>("StageIcon_CustomStage");
            GameObject mapTile = GameObject.Instantiate(mapTilePrefab);

            //Create new tile definition object
            MenuClassicTile levelSelectTile = new MenuClassicTile();

            levelSelectTile.bgm = __instance.stages[10].bgm;
            levelSelectTile.background = __instance.stages[10].background;
            levelSelectTile.hub = true;

            levelSelectTile.left = -1;
            levelSelectTile.right = -1;
            levelSelectTile.up = 10;
            levelSelectTile.down = -1;
            levelSelectTile.stageRequirement = new[] { 10 };
            levelSelectTile.icon = mapTile.GetComponent<SpriteRenderer>();

            levelSelectTile.stageID = StageHandler.getCustomStageByUid("kuborro.customstageselectmenu").id;

            //Add our tile to the map's location list - then obtain it's index number used for route linking
            __instance.stages = __instance.stages.AddToArray(levelSelectTile);
            int location = Array.IndexOf(__instance.stages, levelSelectTile);

            __instance.stages[10].down = location;
        }
    }
}
