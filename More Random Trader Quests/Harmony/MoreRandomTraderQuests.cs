using Harmony;
using System.Reflection;
using UnityEngine;
using DMT;
using System.Collections.Generic;
using System;

[HarmonyPatch(typeof(EntityNPC), "PopulateActiveQuests")]
public class MoreRandomTraderQuests : IHarmony
{
    public void Start()
    {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        HarmonyInstance harmony = HarmonyInstance.Create(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    static bool Prefix(EntityNPC __instance, ref List<Vector2> ___usedPOILocations, ref List<int> ___tempTopTierQuests, ref List<Quest> __result, EntityPlayer player, int currentTier = -1)
    {
        if (__instance.questList == null)
        {
            __instance.PopulateQuestList();
        }
        bool @bool = GameStats.GetBool(EnumGameStats.EnemySpawnMode);
        List<Quest> list = new List<Quest>();
        ___usedPOILocations.Clear();
        ___tempTopTierQuests.Clear();
        if (currentTier == -1)
        {
            currentTier = player.QuestJournal.GetCurrentFactionTier(0, 0, false);
        }
        for (int i = 0; i < __instance.questList.Count; i++)
        {
            if ((int)QuestClass.s_Quests[__instance.questList[i].QuestID].DifficultyTier == currentTier)
            {
                ___tempTopTierQuests.Add(i);
            }
        }
        if (___tempTopTierQuests.Count > 0)
        {
            for (int j = 0; j < 100; j++)
            {
                int index = __instance.rand.RandomRange(___tempTopTierQuests.Count);
                QuestEntry questEntry = __instance.questList[___tempTopTierQuests[index]];
                if (__instance.rand.RandomFloat < questEntry.Prob)
                {
                    Quest quest = __instance.questList[___tempTopTierQuests[index]].QuestClass.CreateQuest();
                    quest.QuestGiverID = __instance.entityId;
                    quest.SetPositionData(Quest.PositionDataTypes.QuestGiver, __instance.position);
                    quest.SetupTags();
                    if (@bool || (quest.QuestTags & QuestTags.clear) == QuestTags.none)
                    {
                        if (quest.SetupPosition(__instance, ___usedPOILocations, player.entityId)) list.Add(quest);
                        if (list.Count == 1) break;
                    }
                }
            }
        }
        for (int k = 0; k < 200; k++)
        {
            int index2 = __instance.rand.RandomRange(__instance.questList.Count);
            QuestEntry questEntry2 = __instance.questList[index2];
            QuestClass questClass = questEntry2.QuestClass;
            if (__instance.rand.RandomFloat < questEntry2.Prob && (int)questClass.DifficultyTier <= currentTier)
            {
                Quest quest2 = questClass.CreateQuest();
                quest2.QuestGiverID = __instance.entityId;
                quest2.SetPositionData(Quest.PositionDataTypes.QuestGiver, __instance.position);
                quest2.SetupTags();
                if (@bool || (quest2.QuestTags & QuestTags.clear) == QuestTags.none)
                {
                    if (!quest2.NeedsNPCSetPosition || quest2.SetupPosition(__instance, ___usedPOILocations, player.entityId))
                    {
                        list.Add(quest2);
                    }
                    if (list.Count == 5)
                    {
                        break;
                    }
                }
            }
        }
        __result = list;

        return false;
    }
}
