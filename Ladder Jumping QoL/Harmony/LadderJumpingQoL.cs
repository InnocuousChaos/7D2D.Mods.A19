using Harmony;
using System.Reflection;
using UnityEngine;
using DMT;
using System.Collections.Generic;
using System;

[HarmonyPatch(typeof(EntityPlayerLocal), "MoveEntityHeaded")]
public class LadderJumpingQoL : IHarmony
{
    public void Start()
    {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        HarmonyInstance harmony = HarmonyInstance.Create(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    static bool Prefix(EntityPlayerLocal __instance, ref Boolean ___canAttachToLadder, Boolean ___ladderJump)
    {
        return true;
    }
}
