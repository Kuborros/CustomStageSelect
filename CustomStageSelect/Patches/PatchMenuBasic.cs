using CustomStageSelect.Menus;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomStageSelect.Patches
{
    internal class PatchMenuBasic
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuBasic),"Start",MethodType.Normal)]
        static void PatchMenuBasicStart(MenuBasic __instance)
        {
            //Swap to out own Menu implementation
            if (SceneManager.GetActiveScene().name == "CustomStageSelect")
            {
                __instance.gameObject.AddComponent<MenuCustomStageSelect>();
                GameObject.Destroy(__instance);
            }
        }
    }
}
