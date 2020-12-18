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

    static bool Prefix(EntityNPC __instance, ref List<Vector2> ___usedPOILocations, ref List<int> ___tempTopTierQuests, ref List<int> ___tempSpecialQuests, ref List<string> ___uniqueKeysUsed, ref List<Quest> __result, EntityPlayer player, int currentTier = -1)
    {
			if (__instance.questList == null)
			{
			__instance.PopulateQuestList();
				if (__instance.questList == null)
			{
				__result = null;
                return false;
            }
        }
			bool @bool = GameStats.GetBool(EnumGameStats.EnemySpawnMode);
			List<Quest> list = new List<Quest>();
			___usedPOILocations.Clear();
			___tempTopTierQuests.Clear();
			___tempSpecialQuests.Clear();
			___uniqueKeysUsed.Clear();
			if (currentTier == -1)
			{
				currentTier = player.QuestJournal.GetCurrentFactionTier(0, 0, false);
			}
			for (int i = 0; i < __instance.questList.Count; i++)
			{
				MethodInfo getQuestMethod = __instance.GetType().GetMethod("GetQuest", BindingFlags.NonPublic | BindingFlags.Static);			
				QuestClass quest = (QuestClass)getQuestMethod.Invoke(null, new object[] { __instance.questList[i].QuestID });
		if ((int)quest.DifficultyTier == currentTier && quest.QuestType == "")
				{
					___tempTopTierQuests.Add(i);
				}
				if (quest.QuestType != "")
				{
					___tempSpecialQuests.Add(i);
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
						Quest quest2 = __instance.questList[___tempTopTierQuests[index]].QuestClass.CreateQuest();
						quest2.QuestGiverID = __instance.entityId;
						quest2.SetPositionData(Quest.PositionDataTypes.QuestGiver, __instance.position);
						quest2.SetupTags();
						if (@bool || (quest2.QuestTags & QuestTags.clear) == QuestTags.none)
						{
							if (quest2.SetupPosition(__instance, player, ___usedPOILocations, player.entityId))
							{
								list.Add(quest2);
							}
							if ((quest2.QuestTags & QuestTags.treasure) == QuestTags.none && GameSparksCollector.CollectGamePlayData)
							{
								GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestOfferedDistance, ((int)Vector3.Distance(quest2.Position, __instance.position) / 50 * 50).ToString(), 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
							}
							if (list.Count == 1)
							{
								break;
							}
						}
					}
				}
			}
			for (int k = 0; k < 200; k++)
			{
				int index2 = __instance.rand.RandomRange(__instance.questList.Count);
				QuestEntry questEntry2 = __instance.questList[index2];
				QuestClass questClass = questEntry2.QuestClass;
				if (__instance.rand.RandomFloat < questEntry2.Prob && (int)questClass.DifficultyTier <= currentTier && !(questClass.QuestType != ""))
				{
					Quest quest3 = questClass.CreateQuest();
					quest3.QuestGiverID = __instance.entityId;
					quest3.SetPositionData(Quest.PositionDataTypes.QuestGiver, __instance.position);
					quest3.SetupTags();
					if (@bool || (quest3.QuestTags & QuestTags.clear) == QuestTags.none)
					{
						if (!quest3.NeedsNPCSetPosition || quest3.SetupPosition(__instance, player, ___usedPOILocations, player.entityId))
						{
							list.Add(quest3);
						}
						if ((quest3.QuestTags & QuestTags.treasure) == QuestTags.none && GameSparksCollector.CollectGamePlayData)
						{
							GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestOfferedDistance, ((int)Vector3.Distance(quest3.Position, __instance.position) / 50 * 50).ToString(), 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
						}
						if (list.Count == 5)
						{
							break;
						}
					}
				}
			}
			for (int l = 0; l < ___tempSpecialQuests.Count; l++)
			{
				if (__instance.questList[___tempSpecialQuests[l]].QuestClass.UniqueKey == "" || !___uniqueKeysUsed.Contains(__instance.questList[___tempSpecialQuests[l]].QuestClass.UniqueKey))
				{
					QuestClass questClass2 = __instance.questList[___tempSpecialQuests[l]].QuestClass;
					if ((int)(questClass2.DifficultyTier - 1) <= currentTier && player.QuestJournal.FindQuest(questClass2.ID) == null)
					{
						Quest quest4 = questClass2.CreateQuest();
						quest4.QuestGiverID = __instance.entityId;
						quest4.SetPositionData(Quest.PositionDataTypes.QuestGiver, __instance.position);
						quest4.SetupTags();
						if (!quest4.NeedsNPCSetPosition || quest4.SetupPosition(__instance, player, ___usedPOILocations, player.entityId))
						{
							list.Add(quest4);
							if (questClass2.UniqueKey != "")
							{
								___uniqueKeysUsed.Add(questClass2.UniqueKey);
							}
							if (GameSparksCollector.CollectGamePlayData)
							{
								GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestTraderToTraderDistance, ((int)Vector3.Distance(quest4.Position, __instance.position) / 50 * 50).ToString(), 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
							}
						}
					}
				}
		}
		__result = list;
		return false;
	}
}