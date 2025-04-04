using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CustomStageSelect.Patches
{
    internal class PatchStageExits
    {
        internal static bool returnToLevelSelect = false;

        internal static string getDestinationScene()
        {
            if (returnToLevelSelect)
            {
                returnToLevelSelect = false;
                return "CustomStageSelect";
            }
            if (FPSaveManager.gameMode == FPGameMode.ADVENTURE)
            {
                return "AdventureMenu";
            }
            if (FPSaveManager.gameMode == FPGameMode.CLASSIC)
            {
                return "ClassicMenu";
            }
            //Should never happen but still
            return "MainMenu";
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPResultsMenu), "Update", MethodType.Normal)]
        static IEnumerable<CodeInstruction> PatchResultsMenu(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && codes[i - 1].opcode == OpCodes.Ldloc_S)
                {
                    if (CodeInstructionExtensions.OperandIs(codes[i],"AdventureMenu") || CodeInstructionExtensions.OperandIs(codes[i], "ClassicMenu"))
                    {
                        codes[i] = Transpilers.EmitDelegate(getDestinationScene);
                    }
                }
            }
            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(FPPauseMenu), "Update", MethodType.Normal)]
        [HarmonyPatch(typeof(MenuContinue), "State_Main", MethodType.Normal)]
        static IEnumerable<CodeInstruction> PatchPauseMenu(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (var i = 1; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && codes[i - 1].opcode == OpCodes.Ldarg_0)
                {
                    if (CodeInstructionExtensions.OperandIs(codes[i], "AdventureMenu") || CodeInstructionExtensions.OperandIs(codes[i], "ClassicMenu"))
                    {
                        codes[i] = Transpilers.EmitDelegate(getDestinationScene);
                    }
                }
            }
            return codes;
        }

    }
}
