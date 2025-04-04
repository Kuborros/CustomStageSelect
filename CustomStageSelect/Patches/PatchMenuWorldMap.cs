using HarmonyLib;
using System;
using UnityEngine;
namespace CustomStageSelect.Patches
{
    internal class PatchMenuWorldMap
    {
        [HarmonyPrefix]
        //Has to run before the map patcher fixes the destination ids
        [HarmonyBefore("000.kuborro.libraries.fp2.fp2lib.worldmap")]
        [HarmonyPatch(typeof(MenuWorldMap), "Start", MethodType.Normal)]
        static void PatchMenuWorldMapStart(MenuWorldMap __instance)
        {
            GameObject markerPrefab = CustomStageSelect.menuAssets.LoadAsset<GameObject>("HubVRArcade");
            MenuText marker = GameObject.Instantiate(markerPrefab).GetComponent<MenuText>();
            marker.gameObject.transform.parent = __instance.mapScreens[4].gameObject.transform;

            //Build our own map marker
            FPMapRequirement requirement = new FPMapRequirement {
                self = 12,
                up = 0,
                down = 0,
                left = 0,
                right = 0,
                skipDown = 0,
                skipLeft = 0,
                skipRight = 0,
                skipUp = 0,
                skipIfLocked = false
            };

            FPMapPointer pointer = new FPMapPointer {
                hudChestItem = FPPowerup.NONE,
                hudVinylID = -1,
                stageID = 0,
                locationID = 0,
                mapID = 0
            };


            FPMapLocation vrArcade = new FPMapLocation
            {
                icon = marker,
                type = FPMapLocationType.HUB,
                up = 2,
                down = -1,
                left = -1,
                right = -1,
                requirements = requirement,
                pointers = pointer
            };

            __instance.mapScreens[4].map.locations = __instance.mapScreens[4].map.locations.AddToArray(vrArcade);
            int locationIndex = Array.IndexOf(__instance.mapScreens[4].map.locations, vrArcade);
            
            //Link to Battlesphere's map marker
            __instance.mapScreens[4].map.locations[2].down = locationIndex;
            __instance.mapScreens[4].map.locations[2].requirements.down = 12;

        }
    }
}
