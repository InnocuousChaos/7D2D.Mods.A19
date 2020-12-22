using HarmonyLib;
using System.Reflection;
using UnityEngine;
using DMT;
using System.Collections.Generic;
using System;
using System.Reflection.Emit;

public class EntityNPCPatch
{
    [HarmonyPatch(typeof(EntityNPC), "PopulateActiveQuests")]
    public class EntityNPC_PopulateActiveQuests
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int foundCount = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (foundCount == 0 && instruction.Calls(typeof(List<Quest>).GetMethod("get_Count")))
                {
                    foundCount++;
                }
                else if (foundCount == 1)
                {
                    if (instruction.opcode == OpCodes.Ldc_I4_3)
                    {
                        instruction.opcode = OpCodes.Ldc_I4_1;
                    }

                }
                yield return instruction;
            }
        }
    }


    [HarmonyPatch(typeof(EntityNPC), "OnEntityActivated")]
    public class EntityNPC_OnEntityActivated
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int foundCount = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (foundCount == 0 && instruction.opcode == OpCodes.Stfld && instruction.LoadsField(typeof(EntityNPC).GetField("activeQuests")))
                {
                    foundCount++;
                }
                else if (foundCount == 1)
                {
                    if (instruction.Branches(out Label? n))
                    {
                        instruction.opcode = OpCodes.Nop;
                    }
                    else
                    {
                        foundCount = 0;
                    }

                }
                yield return instruction;
            }
        }
    }

    //Patch for testing, level 5 quests always available.
    //[HarmonyPatch(typeof(QuestJournal), "GetCurrentFactionTier")]
    //public class QuestJournal_GetCurrentFactionTier
    //{
    //    static bool Prefix(ref int __result)
    //    {
    //        __result = 5;
    //        return false;
    //    }
    //}
}