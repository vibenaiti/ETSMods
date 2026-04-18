using System.Collections.Generic;
using System.Linq;
using ProjectM;
using ModCore.Models;
using ModCore.Data;
using Stunlock.Core;

namespace ModCore.Helpers;

//this is horrible god help us all
public static partial class Helper
{
	public static bool TryGetPrefabGUIDFromString(string prefabNameOrId, out PrefabGUID prefabGUID)
	{
		// NameToPrefabGuidDictionary removed in v1.1.11+; use SpawnableNameToPrefabGuidDictionary.
		if (Core.prefabCollectionSystem.SpawnableNameToPrefabGuidDictionary.ContainsKey(prefabNameOrId))
		{
			prefabGUID = Core.prefabCollectionSystem.SpawnableNameToPrefabGuidDictionary[prefabNameOrId];
			return true;
		}
		else
		{
			if (int.TryParse(prefabNameOrId, out int prefabGuidId))
			{
				var prefabGuid = new PrefabGUID(prefabGuidId);
				// PrefabGuidToNameDictionary removed; check via PrefabLookupMap instead.
				if (Core.prefabCollectionSystem._PrefabLookupMap.ContainsKey(prefabGuid))
				{
					prefabGUID = prefabGuid;
					return true;
				}
			}
		}

		prefabGUID = default;
		return false;
	}

	// Create a structure to store item and its matching score
	struct MatchItem
	{
		public int Score;
		public PrefabData PrefabData;
	}

	public static bool TryGetPrefabDataFromString(string needle, List<PrefabData> prefabDataList, out PrefabData matchedItem)
	{
		List<MatchItem> matchedItems = new List<MatchItem>();

		// Check for direct string match (NameToPrefabGuidDictionary removed; use SpawnableNameToPrefabGuidDictionary)
		if (Core.prefabCollectionSystem.SpawnableNameToPrefabGuidDictionary.TryGetValue(needle, out var prefabGUID))
		{
			matchedItem = prefabDataList.FirstOrDefault(item => item.PrefabGUID.Equals(prefabGUID));
			if (matchedItem != null)
			{
				return true;
			}
			else
			{
				matchedItem = new PrefabData(prefabGUID, prefabGUID.LookupName());
				return true;
			}
		}

		// Check for direct GUID match (PrefabGuidToNameDictionary removed; validate via PrefabLookupMap)
		if (int.TryParse(needle, out int prefabGuidId))
		{
			var prefabGuid = new PrefabGUID(prefabGuidId);
			if (Core.prefabCollectionSystem._PrefabLookupMap.ContainsKey(prefabGuid))
			{
				matchedItem = prefabDataList.FirstOrDefault(item => item.PrefabGUID.Equals(prefabGuid));
				if (matchedItem != null)
					return true;
			}
		}

		foreach (var prefabData in prefabDataList)
		{
			int score = IsSubsequence(needle, prefabData.OverrideName.ToLower() + "s");
			if (score != -1)
			{
				matchedItems.Add(new MatchItem { Score = score, PrefabData = prefabData });
			}

			score = IsSubsequence(needle, prefabData.FormalPrefabName.ToLower() + "s");
			if (score != -1)
			{
				matchedItems.Add(new MatchItem { Score = score, PrefabData = prefabData });
			}

			if (int.TryParse(needle, out int result) && result == prefabData.PrefabGUID.GuidHash)
			{
				matchedItems.Add(new MatchItem { Score = score, PrefabData = prefabData });
			}
		}

		var bestMatch = matchedItems.OrderByDescending(m => m.Score).FirstOrDefault();
		if (bestMatch.Score == 0)
		{
			matchedItem = default;
			return false;
		}

		if (!bestMatch.Equals(default(MatchItem)))
		{
			matchedItem = bestMatch.PrefabData;
			return true;
		}

		matchedItem = default;
		return false;
	}


	public static bool TryGetBloodPrefabDataFromString(string needle, out PrefabData bloodPrefab)
	{
		return TryGetPrefabDataFromString(needle, BloodData.BloodPrefabData, out bloodPrefab);
	}

	public static bool TryGetJewelPrefabDataFromString(string needle, out PrefabData jewelPrefab)
	{
		return TryGetPrefabDataFromString(needle, JewelData.JewelPrefabData, out jewelPrefab);
	}

	public static bool TryGetItemPrefabDataFromString(string needle, out PrefabData itemPrefab)
	{
		return TryGetPrefabDataFromString(needle, Items.GiveableItemsPrefabData, out itemPrefab);
	}

	private static int IsSubsequence(string needle, string haystack)
	{
		int j = 0;
		int maxConsecutiveMatches = 0;
		int currentConsecutiveMatches = 0;

		for (int i = 0; i < needle.Length; i++)
		{
			while (j < haystack.Length && haystack[j] != needle[i])
			{
				j++;
			}

			if (j == haystack.Length)
			{
				return -1;
			}

			if (i > 0 && needle[i - 1] == haystack[j - 1])
			{
				currentConsecutiveMatches++;
			}
			else
			{
				if (currentConsecutiveMatches > maxConsecutiveMatches)
				{
					maxConsecutiveMatches = currentConsecutiveMatches;
				}

				currentConsecutiveMatches = 1;
			}

			j++;
		}

		if (currentConsecutiveMatches > maxConsecutiveMatches)
		{
			maxConsecutiveMatches = currentConsecutiveMatches;
		}

		return maxConsecutiveMatches;
	}
}
