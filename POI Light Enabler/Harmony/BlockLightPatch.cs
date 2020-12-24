using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Reflection.Emit;

public class BlockLightPatch
{
    [HarmonyPatch]
    public class BlockLightPatches
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(BlockLight).GetMethod("GetActivationText");
            yield return typeof(BlockLight).GetMethod("GetBlockActivationCommands");            
            yield return typeof(BlockLight).GetMethod("OnBlockActivated", new Type[] { typeof(int), typeof(WorldBase), typeof(int), typeof(Vector3i), typeof(BlockValue), typeof(EntityAlive) });
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Is(OpCodes.Callvirt, AccessTools.Method("WorldBase:IsEditor")))
                {
                    //Pop the object off the stack, then push a 1 on the stack. Essentially returning true.
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    //Don't return the original instruction
                    continue;
                }
                //Return the original instruction if not handled above.
                yield return instruction;
            }
        }
    }
}