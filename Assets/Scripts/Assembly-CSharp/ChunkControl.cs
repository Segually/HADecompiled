using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkControl : MonoBehaviour, OrderedStart
{
	[Serializable]
	public struct cave_define
	{
		public string name;

		public int corresponding_biome_id;

		public Color color;

		public string grass_biomePrefab_path;

		public string grass_cavePrefab_path;

		public int grass_density;

		public bool grass_hangs_from_cieling;

		public cave_mineral_pairs[] possible_mineral_pairs;

		public bool sparse_minerals;

		public cave_mineral_pairs[] possible_SUPER_RARE_minerals;

		public cave_mineral_pairs[] possible_gem_pairs;

		public int shroom_density;

		public string shroom_obj_name;

		public string giant_shroom_obj_name;

		public int stalagmite_density;

		public string stalagmite_name;

		public string small_stalagmite_name;

		public string overwrite_walltex_deadend_path;

		public string overwrite_walltex_straight_path;

		public string overwrite_walltex_hook_path;

		public string overwrite_walltex_3way_path;

		public string overwrite_walltex_4way_path;

		public string overwrite_floortex_deadend_path;

		public string overwrite_floortex_straight_path;

		public string overwrite_floortex_hook_path;

		public string overwrite_floortex_3way_path;

		public string overwrite_floortex_4way_path;
	}

	[Serializable]
	public struct cave_mineral_pairs
	{
		public string name;

		public string small_mineral;

		public string large_mineral;
	}

	private enum build_pass_type_t
	{
		dev_placed_and_player_placed = 0,
		natural = 1
	}

	public enum pathway_type
	{
		none = 0,
		gravel = 1,
		stone = 2,
		sand = 3,
		lava = 4,
		bouncy = 5
	}

	[Serializable]
	public struct biome_obj
	{
		public string item_name;

		public bool is_item;

		public spawn_commonness spawn_rate;

		public int additional_clump_min;

		public int additional_clump_max;

		public string clump_overwrite_obj;

		public bool dont_rotate;

		public float min_depth;

		public bool hide_from_mimicry_perk;
	}

	public enum spawn_commonness
	{
		average = 0,
		rare_slight = 1,
		rare_medium = 2,
		rare_very = 3,
		DOUBLE = 4,
		TRIPLE = 5
	}

	[Serializable]
	public struct biome
	{
		public string biome_name;

		public string floor_prefab_path;

		public float min_depth;

		public string[] floor_texture_paths;

		public biome_obj[] biome_scenic;

		public string grass_prefab_path;

		public int grass_density;

		public bool grass_rotate;

		public string[] possible_mobs;

		public int copy_possible_mobs_from;

		public Color edge_color;

		public Color edge_color_OCEAN;

		public bool dont_spawn_greens;

		public int scenics_budget_min;

		public int scenics_budget_max;

		public int item_budget_min;

		public int item_budget_max;
	}

	public static ChunkControl Instance;

	public MaterialPropertyBlock edge_mat_block;

	public Dictionary<string, GameObject> active_interactibles = new Dictionary<string, GameObject>();

	public Texture2D biome_map_tex;

	public biome[] biomes;

	public GameObject biome_edge_piece;

	public GameObject cave_art_prefab;

	public string[] cave_art_textures_path;

	public const int template_biome_map_w_ = 36;

	private int[,] template_biome_map_ids;

	public const float chunk_width = 10f;

	private int old_chunk_X;

	private int old_chunk_z;

	public int player_chunk_X;

	public int player_chunk_Z;

	private string player_zone_cache = "overworld";

	private GameObject cached_follow_obj;

	public float view_zoom = 1f;

	public static int newest_biomemap_v = 7;

	public Dictionary<string, BiomeMap> loaded_biome_maps = new Dictionary<string, BiomeMap>();

	public cave_define[] cave_defines;

	public string[] cave_wall_prefab_paths;

	public static float dist_empty_around_cave_ladder = 3f;

	public static int curr_chunk_version = 7;

	private Dictionary<string, Chunk> Chunks = new Dictionary<string, Chunk>();

	private string curr_chunk_loading = "";

	public List<GameObject> enable_on_accept = new List<GameObject>();

	public List<OccupiedSpace> global_spaces_occupied_by_buildables = new List<OccupiedSpace>();

	public static int dedicated_quest_range_start = 9000;

	public string player_zone
	{
		get
		{
			return player_zone_cache;
		}
		set
		{
			player_zone_cache = value;
		}
	}

	public GameObject follow_obj
	{
		get
		{
			if (cached_follow_obj == null && GameController.Instance.player != null)
			{
				cached_follow_obj = GameController.Instance.player;
			}
			return cached_follow_obj;
		}
	}

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		LoadGenericBiomeMap();
		StartCoroutine(TrackPlayerChunk());
		StartCoroutine(SavePlayerLocationPeriodically());
		StartCoroutine(TickAllRespawnWatchers());
		edge_mat_block = new MaterialPropertyBlock();
	}

	private IEnumerator TickAllRespawnWatchers()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);
			foreach (KeyValuePair<string, Chunk> chunk in Chunks)
			{
				if (chunk.Value.status == Chunk.status_t.complete)
				{
					chunk.Value.chunk_obj.TickRespawnWatchers(chunk.Value.X, chunk.Value.Z, chunk.Value.zone);
					chunk.Value.chunk_data.TickAllLandClaimChunkTimers();
				}
			}
		}
	}

	public void AppearQuestMobs(string quest_name, int progress)
	{
		foreach (KeyValuePair<string, Chunk> entry in Chunks)
		{
			Chunk chunk = entry.Value;
			if (chunk.status != Chunk.status_t.complete) continue;
			ChunkData data = chunk.chunk_data;
			for (int x = 0; x < 10; x++)
			{
				for (int z = 0; z < 10; z++)
				{
					foreach (ChunkElement element in data.GetElementsAt(x, z))
					{
						if (element.item.GetString("is_killGoal_mob") == "true" && element.item.GetString("associated_quest_name") == quest_name && element.item.GetShort("associated_quest_step") == progress)
							RedrawAtSquare(GetChunkString(player_zone, chunk.X, chunk.Z), x, z);
					}
				}
			}
		}
	}

	public InventoryItem EncodeRespawnIntoItem(string respawn_prefix, InventoryItem old_item, DateTime UTC_when)
	{
		ExtraInventoryData extraDataCopy = old_item.GetExtraDataCopy();
		string value = UTC_when.ToString("o");
		extraDataCopy.SetString("has_respawn_" + respawn_prefix, "true");
		extraDataCopy.SetString("UTC_dateTime_" + respawn_prefix, value);
		return new InventoryItem(old_item.item_name, extraDataCopy);
	}

	public InventoryItem ProcessItemChange(InventoryItem item, ref bool item_was_changed)
	{
		string item_name = item.item_name;
		if (item_name == "3-day Land Claim" || item_name == "8-day Land Claim" || item_name == "Admin Land Claim")
		{
			if (!item.HasActiveRespawn("landclaim_spawn"))
			{
				item_was_changed = true;
				return new InventoryItem("Old Land Claim");
			}
		}
		else if (item_name == "Gravestone" && !item.HasActiveRespawn("ghost_spawn"))
		{
			ExtraInventoryData extraDataCopy = item.GetExtraDataCopy();
			extraDataCopy.ClearSubItem("ghost");
			if (UnityEngine.Random.value < 0.5f)
			{
				List<InventoryItem> list = CompanionController.Instance.LoadCompanionList(PlayerData.filename_t.dead_companions);
				if (list.Count == 0)
				{
					extraDataCopy.SetShort("has_ghost", 0);
				}
				else
				{
					extraDataCopy.SetShort("has_ghost", 1);
					extraDataCopy.SaveSubItem(list[UnityEngine.Random.Range(0, list.Count)], "ghost");
				}
			}
			else
			{
				extraDataCopy.SetShort("has_ghost", 0);
			}
			InventoryItem old_item = new InventoryItem("Gravestone", extraDataCopy);
			DateTime uTC_when = DateTime.UtcNow.AddHours(2.0);
			InventoryItem result = EncodeRespawnIntoItem("ghost_spawn", old_item, uTC_when);
			item_was_changed = true;
			return result;
		}
		item_was_changed = false;
		return item;
	}

	public InventoryItem EncodeItemListIntoItem(string list_name, ItemCountPair[] items, InventoryItem old_item)
	{
		ExtraInventoryData extraDataCopy = old_item.GetExtraDataCopy();
		extraDataCopy.SetShort(list_name + "_n_encoded_items", items.Length);
		for (int i = 0; i < items.Length; i++)
		{
			extraDataCopy.SaveSubItem(items[i].item, list_name + "_item_" + i);
			extraDataCopy.SetShort(list_name + "_count_" + i, items[i].count);
		}
		return new InventoryItem(old_item.item_name, extraDataCopy);
	}

	public ItemCountPair[] GetItemListFromItem(string list_name, InventoryItem item)
	{
		int num = item.GetShort(list_name + "_n_encoded_items");
		ItemCountPair[] array = new ItemCountPair[num];
		for (int i = 0; i < num; i++)
		{
			InventoryItem item_ = item.LoadSubItem(list_name + "_item_" + i);
			short @short = item.GetShort(list_name + "_count_" + i);
			array[i] = new ItemCountPair(item_, @short);
		}
		return array;
	}

	public ChunkElement GetElementAt(string object_name, string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		string chunkString = GetChunkString(zone, chunkX, chunkZ);
		if (!Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			return null;
		}
		foreach (ChunkElement item in Instance.GetChunk(chunkString).chunk_data.GetElementsAt(innerX, innerZ))
		{
			if (item.item.item_name == object_name)
			{
				return item;
			}
		}
		return null;
	}

	public GameObject GetObjectAt(string object_name, string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		string chunkString = GetChunkString(zone, chunkX, chunkZ);
		if (!Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			return null;
		}
		ChunkObj chunkObj = Instance.GetChunk(chunkString)?.chunk_obj;
		InventoryItem item = null;
		foreach (ChunkElement item2 in Instance.GetChunk(chunkString).chunk_data.GetElementsAt(innerX, innerZ))
		{
			if (item2.item.item_name == object_name)
			{
				item = item2.item;
				break;
			}
		}
		return chunkObj.GetBuildableInstanceByItem(innerX, innerZ, item);
	}

	public static string GetZoneFromChunkStr(string chunkStr)
	{
		int num = chunkStr.IndexOf('(');
		int num2 = chunkStr.IndexOf(',');
		return chunkStr.Substring(num + 1, num2 - (num + 1));
	}

	public static int GetChunkXFromChunkStr(string chunkStr)
	{
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < chunkStr.Length; i++)
		{
			if (chunkStr[i] == ',')
			{
				if (num == -1)
				{
					num = i;
				}
				else if (num2 == -1)
				{
					num2 = i;
				}
			}
		}
		return int.Parse(chunkStr.Substring(num + 1, num2 - (num + 1)), Startup.parse_culture);
	}

	public static int GetChunkZFromChunkStr(string chunkStr)
	{
		int num = chunkStr.IndexOf(')');
		int num2 = -1;
		int num3 = -1;
		for (int i = 0; i < chunkStr.Length; i++)
		{
			if (chunkStr[i] == ',')
			{
				if (num2 == -1)
				{
					num2 = i;
				}
				else if (num3 == -1)
				{
					num3 = i;
				}
			}
		}
		return int.Parse(chunkStr.Substring(num3 + 2, num - (num3 + 2)), Startup.parse_culture);
	}

	public static string GetChunkFilename(string chunkStr)
	{
		string zoneFromChunkStr = GetZoneFromChunkStr(chunkStr);
		int chunkXFromChunkStr = GetChunkXFromChunkStr(chunkStr);
		int chunkZFromChunkStr = GetChunkZFromChunkStr(chunkStr);
		int input_int = (int)((float)chunkXFromChunkStr / 6f);
		int input_int2 = (int)((float)chunkZFromChunkStr / 6f);
		return "ClumpedChunks(" + zoneFromChunkStr + ")(" + IntToChars(input_int) + "," + IntToChars(input_int2) + ")";
	}

	public static string GetBasketFilename(string basket_clump)
	{
		return "ClumpedBaskets(" + IntToChars((int)((float)int.Parse(basket_clump.Replace("basket", ""), Startup.parse_culture) / 50f)) + ")";
	}

	public static string GetZoneDataFilename(string zonedata_clump)
	{
		if (!zonedata_clump.Contains("shack"))
		{
			return "ClumpedZoneDatas(error)";
		}
		return "ClumpedZoneDatas(" + IntToChars((int)((float)int.Parse(zonedata_clump.Replace("shack", "").Replace("-zonedata", ""), Startup.parse_culture) / 50f)) + ")";
	}

	public static string IntToChars(int input_int)
	{
		string text = input_int.ToString() ?? "";
		char[] array = new char[text.Length];
		for (int i = 0; i < text.Length; i++)
		{
			switch (text[i])
			{
			case '-':
				array[i] = '-';
				break;
			case '0':
				array[i] = 'a';
				break;
			case '1':
				array[i] = 'b';
				break;
			case '2':
				array[i] = 'c';
				break;
			case '3':
				array[i] = 'd';
				break;
			case '4':
				array[i] = 'e';
				break;
			case '5':
				array[i] = 'f';
				break;
			case '6':
				array[i] = 'g';
				break;
			case '7':
				array[i] = 'h';
				break;
			case '8':
				array[i] = 'i';
				break;
			case '9':
				array[i] = 'j';
				break;
			}
		}
		return new string(array);
	}

	public ForcedChunkData TryLoadForcedChunkData_(string file_path)
	{
		ForcedChunkData forcedChunkData = new ForcedChunkData();
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines(file_path, ref file_exists);
		if (file_exists)
		{
			foreach (string item in textFileLines)
			{
				if (Startup.StringNullOrWhitespace(item))
				{
					continue;
				}
				if (item[0] == '*')
				{
					forcedChunkData.has_any_data = true;
					if (item == "*is_blank=true*")
					{
						forcedChunkData.is_blank = true;
					}
					else if (item.Contains("forced_biome"))
					{
						forcedChunkData.forced_biome = int.Parse(item.Substring(14, item.Length - 15), Startup.parse_culture);
					}
					else if (item.Contains("forced_floor_model"))
					{
						forcedChunkData.forced_floor_model = int.Parse(item.Substring(20, item.Length - 21), Startup.parse_culture);
					}
					else if (item.Contains("forced_floor_rot"))
					{
						forcedChunkData.forced_floor_rot = int.Parse(item.Substring(18, item.Length - 19), Startup.parse_culture);
					}
					else if (item.Contains("caveart_index"))
					{
						forcedChunkData.caveart_index = int.Parse(item.Substring(15, item.Length - 16), Startup.parse_culture);
					}
					else if (item.Contains("caveart_rot"))
					{
						forcedChunkData.caveart_rot = int.Parse(item.Substring(13, item.Length - 14), Startup.parse_culture);
					}
				}
				else if (item[0] == '[')
				{
					forcedChunkData.has_any_data = true;
					break;
				}
			}
		}
		return forcedChunkData;
	}

	private int GetQuestVersion(string chunkStr)
	{
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines(DevBuildControl.quest_scenics_folder_ + "/" + chunkStr, ref file_exists);
		if (!file_exists)
		{
			return 0;
		}
		foreach (string item in textFileLines)
		{
			if (!Startup.StringNullOrWhitespace(item) && item.Contains("auto_version"))
			{
				return int.Parse(item.Substring(14, item.Length - 15), Startup.parse_culture);
			}
		}
		return 0;
	}

	public string GetForcedChunkDataFilePath(string chunkStr)
	{
		return DevBuildControl.quest_scenics_folder_ + "/" + chunkStr;
	}

	public void AssignRandomBiomeMobs(int biome_id, ref string biome_mob_a, ref string biome_mob_b)
	{
		if (biomes[biome_id].copy_possible_mobs_from != -1)
		{
			biome_id = biomes[biome_id].copy_possible_mobs_from;
		}
		string[] possible_mobs = Instance.biomes[biome_id].possible_mobs;
		if (possible_mobs.Length == 0)
		{
			biome_mob_a = "crab";
			biome_mob_b = "crab";
		}
		else
		{
			biome_mob_a = possible_mobs[UnityEngine.Random.Range(0, possible_mobs.Length)];
			biome_mob_b = possible_mobs[UnityEngine.Random.Range(0, possible_mobs.Length)];
		}
	}

	public ChunkData HostGetChunk(string zone, int chunkX, int chunkZ)
	{
		ChunkData chunkData = new ChunkData();
		chunkData.zone = zone;
		chunkData.X = chunkX;
		chunkData.Z = chunkZ;
		string chunkString = GetChunkString(zone, chunkX, chunkZ);
		string chunkFilename = GetChunkFilename(chunkString);
		if (DevBuildControl.Instance.debug_bandit_camp_data != null)
		{
			chunkData.biome = DevBuildControl.Instance.debug_bandit_camp_data.biome_id;
			chunkData.InjectDevPlacedBuildables(DevBuildControl.quest_scenics_folder_ + "/" + chunkString, "dev_obj", 0, true, null, "debug_instance");
			return chunkData;
		}
		if (PlayerData.Instance.GetSlotShort("exists", chunkFilename, chunkString) == 1)
		{
			ChunkData.LoadFromDisk(chunkData, zone, chunkX, chunkZ);
			int questVersion = GetQuestVersion(chunkString);
			if (chunkData.quest_version != questVersion)
			{
				Debug.Log("chunk_data.quest_version (" + chunkData.quest_version + ") != quest_version (" + questVersion + ")");
				chunkData.RemoveAllElementsWithTag("dev_obj");
				if (questVersion > 0)
				{
					chunkData.InjectDevPlacedBuildables(DevBuildControl.quest_scenics_folder_ + "/" + chunkString, "dev_obj");
				}
				chunkData.SaveWholeChunkToDisk(chunkString);
			}
		}
		else
		{
			ZoneData zoneData = ZoneDataControl.Instance.LoadZoneDataFromDisk(zone);
			Dictionary<string, ZoneData> dictionary = new Dictionary<string, ZoneData>();
			if (zone == "overworld")
			{
				BanditCampMap.bandit_camp_instance_info_request_response simpleBanditCampInfo = BanditCampsControl.Instance.GetSimpleBanditCampInfo(chunkX, chunkZ);
				if (!string.IsNullOrEmpty(simpleBanditCampInfo.instance_name))
				{
					BanditCampsControl.Instance.PopulateOverworldChunk(chunkData, chunkX, chunkZ, zone, dictionary, simpleBanditCampInfo);
				}
				else
				{
					ForcedChunkData forcedChunkData = TryLoadForcedChunkData_(GetForcedChunkDataFilePath(chunkString));
					int biome_id = forcedChunkData.forced_biome;
					string biome_mob_a = "crab";
					string biome_mob_b = "crab";
					if (biome_id == -1)
					{
						biome_id = GetBiomeIdAt(chunkX, chunkZ);
						biome_mob_a = BiomeMobAt(chunkX, chunkZ, true);
						biome_mob_b = BiomeMobAt(chunkX, chunkZ, false);
					}
					ChunkGeneratorOverworld.GenerateUnexplored(chunkData, chunkX, chunkZ, zone, biome_id, biome_mob_a, biome_mob_b, false, dictionary);
					AddNaturalTags(chunkData);
				}
			}
			else if (zoneData.house_item.GetString("bandit_camp_instance") != "")
			{
				int bandit_camp_original_shack_id = zoneData.house_item.GetLong("bandit_camp_original_shack_id");
				BanditCampsControl.Instance.PopulateIndoorsChunk(chunkData, chunkX, chunkZ, zone, dictionary, zoneData.house_item.GetString("bandit_camp_instance"), bandit_camp_original_shack_id, zoneData.interior_model_chunkX, zoneData.interior_model_chunkZ);
			}
			else
			{
				if (InventoryUtils.IsCaveObject(zoneData.house_item.item_name))
				{
					CaveData caveData = CaveData.LoadFromDisk(zone);
					ChunkGeneratorCaves.GenerateUnexplored(chunkData, chunkX, chunkZ, zone, zoneData.house_item, -1, -1, caveData.GetUngeneratedFloorAt(chunkX, chunkZ), dictionary);
				}
				else if (InventoryUtils.IsHeavenDimension(zoneData.house_item.item_name))
				{
					ChunkGeneratorClouds.GenerateUnexplored(chunkData, chunkX, chunkZ, zone, zoneData.house_item, zoneData.interior_model_chunkX, zoneData.interior_model_chunkZ, zoneData.interior_model_innerX, zoneData.interior_model_innerZ, dictionary);
				}
				else if (InventoryUtils.IsPureDimension(zoneData.house_item.item_name) || zoneData.house_item.item_name == "Pocket World Basement")
				{
					if (zoneData.house_item.item_name == "Pocket World Snow")
					{
						ChunkGeneratorOverworld.GenerateUnexplored(chunkData, chunkX, chunkZ, zone, 1, "santa", "santa", true, dictionary);
					}
					else if (zoneData.house_item.item_name == "Pocket World Evergreen")
					{
						ChunkGeneratorOverworld.GenerateUnexplored(chunkData, chunkX, chunkZ, zone, 3, "frog", "frog", true, dictionary);
					}
					else if (zoneData.house_item.item_name == "Pocket World Basement")
					{
						ChunkGeneratorIndoors.GenerateUnexplored(chunkData, chunkX, chunkZ, zone, zoneData.house_item, zoneData.interior_model_chunkX, zoneData.interior_model_chunkZ, zoneData.interior_model_innerX, zoneData.interior_model_innerZ, dictionary);
					}
				}
				else if (InventoryUtils.IsHellDimension(zoneData.house_item.item_name))
				{
					ChunkGeneratorHell.GenerateUnexplored(chunkData, chunkX, chunkZ, zone, zoneData.house_item, zoneData.interior_model_chunkX, zoneData.interior_model_chunkZ, zoneData.interior_model_innerX, zoneData.interior_model_innerZ, dictionary);
				}
				else
				{
					ChunkGeneratorIndoors.GenerateUnexplored(chunkData, chunkX, chunkZ, zone, zoneData.house_item, zoneData.interior_model_chunkX, zoneData.interior_model_chunkZ, zoneData.interior_model_innerX, zoneData.interior_model_innerZ, dictionary);
				}
				AddNaturalTags(chunkData);
			}
			chunkData.InjectDevPlacedBuildables(DevBuildControl.quest_scenics_folder_ + "/" + chunkString, "dev_obj");
			chunkData.SaveWholeChunkToDisk(chunkString);
			foreach (KeyValuePair<string, ZoneData> item in dictionary)
			{
				ZoneDataControl.Instance.CreatePlayerZone(item.Key, item.Value.house_item, item.Value.interior_model_chunkX, item.Value.interior_model_chunkZ, item.Value.interior_model_innerX, item.Value.interior_model_innerZ, item.Value.outer_item_zone, item.Value.outer_item_rot);
			}
		}
		chunkData.LoadLandClaimTimersFromDisk(zone, chunkX, chunkZ);
		bool chunk_was_changed = false;
		chunkData.ProcessAllItemChanges(ref chunk_was_changed);
		if (chunk_was_changed)
		{
			chunkData.SaveWholeChunkToDisk(chunkString);
		}
		return chunkData;
	}

	public void AddNaturalTags(ChunkData chunk_data)
	{
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				foreach (ChunkElement item in chunk_data.GetElementsAt(i, j))
				{
					if (item.item.GetString("tag") == "")
					{
						ExtraInventoryData extraDataCopy = item.item.GetExtraDataCopy();
						extraDataCopy.SetString("tag", "natural");
						item.item = new InventoryItem(item.item.item_name, extraDataCopy);
					}
				}
			}
		}
	}

	private void LoadGenericBiomeMap()
	{
		Color[] pixels = ResourceControl.Instance.LoadImageSynchronously("biome-map").GetPixels();
		Dictionary<Color, int> dictionary = new Dictionary<Color, int>();
		template_biome_map_ids = new int[36, 36];
		int num = 0;
		for (int i = 0; i < 36; i++)
		{
			for (int j = 0; j < 36; j++)
			{
				Color key = pixels[i * 36 + j];
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, num);
					num++;
				}
				template_biome_map_ids[i, j] = dictionary[key];
			}
		}
	}

	public void ClearCachedFollowObj()
	{
		cached_follow_obj = null;
	}

	public void OverrideFollowObject(GameObject to_follow)
	{
		cached_follow_obj = to_follow;
	}

	private IEnumerator TrackPlayerChunk()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.5f);
			CalcPlayerChunk();
			if (old_chunk_X != player_chunk_X || old_chunk_z != player_chunk_Z)
			{
				GameplayGUIControl.Instance.UpdateDistanceDisplay();
				UpdateTerrain(false);
			}
		}
	}

	public void SavePlayerLocation()
	{
		if (ZoneDataControl.Instance.curr_zonedata.house_item.GetString("quest_miniworld") == "true")
		{
			return;
		}
		Vector3 v = ((!(GameController.Instance.player != null)) ? GameController.Instance.prev_player_pos : GameController.Instance.player.transform.position);
		Vector3 chunkCoords = GetChunkCoords(v);
		int value = (int)chunkCoords.x;
		int value2 = (int)chunkCoords.z;
		Vector3 inner = GetInner(v);
		int value3 = (int)inner.x;
		int value4 = (int)inner.z;
		if (DevBuildControl.Instance.debug_bandit_camp_data != null)
		{
			return;
		}
		if (GameServerConnector.Instance.FullyInGame())
		{
			if (!GameServerConnector.Instance.is_host)
			{
				if (!GameServerReceiver.Instance.waiting_on_initial_zone_data)
				{
					string server_name = GameServerConnector.Instance.server_name;
					PlayerData.Instance.SetGlobalString(server_name + "player_zone", player_zone_cache);
					PlayerData.Instance.SetGlobalShort(server_name + "player_chunk_x", value);
					PlayerData.Instance.SetGlobalShort(server_name + "player_chunk_z", value2);
					PlayerData.Instance.SetGlobalShort(server_name + "player_inner_x", value3);
					PlayerData.Instance.SetGlobalShort(server_name + "player_inner_z", value4);
				}
				return;
			}
		}
		else if (GameServerConnector.Instance.MidConnect())
		{
			return;
		}
		PlayerData.Instance.SetSlotString("player_zone", player_zone_cache, PlayerData.filename_t.playerpos);
		PlayerData.Instance.SetSlotShort("player_chunk_x", value, PlayerData.filename_t.playerpos);
		PlayerData.Instance.SetSlotShort("player_chunk_z", value2, PlayerData.filename_t.playerpos);
		PlayerData.Instance.SetSlotShort("player_inner_x", value3, PlayerData.filename_t.playerpos);
		PlayerData.Instance.SetSlotShort("player_inner_z", value4, PlayerData.filename_t.playerpos);
	}

	private IEnumerator SavePlayerLocationPeriodically()
	{
		Vector3 prev_location = Vector3.zero;
		while (true)
		{
			yield return new WaitForSeconds(3f);
			bool flag = GameController.Instance.player == null;
			if (!GameServerReceiver.Instance.waiting_on_initial_zone_data && !flag && Vector3.Distance(GameController.Instance.player.transform.position, prev_location) > 2.5f)
			{
				SavePlayerLocation();
				prev_location = GameController.Instance.player.transform.position;
			}
		}
	}

	public Vector3 GetChunkCoords(Vector3 V)
	{
		return new Vector3(Mathf.FloorToInt(V.x / 10f), 0f, Mathf.FloorToInt(V.z / 10f));
	}

	public Vector3 GetInner(Vector3 V)
	{
		Vector3 chunkCoords = GetChunkCoords(V);
		return new Vector3(Mathf.FloorToInt(V.x - chunkCoords.x * 10f), 0f, Mathf.FloorToInt(V.z - chunkCoords.z * 10f));
	}

	public Vector3 GetRoundedClick(Vector3 clickedAt)
	{
		return new Vector3(Mathf.Floor(clickedAt.x), 0f, Mathf.Floor(clickedAt.z));
	}

	public string GetChunkString(Vector3 V)
	{
		Vector3 chunkCoords = GetChunkCoords(V);
		return GetChunkString(player_zone_cache, (int)chunkCoords.x, (int)chunkCoords.z);
	}

	public string GetChunkString(string zone, int X, int Z)
	{
		return "chunk(" + zone + "," + X + ", " + Z + ")";
	}

	public void CalcPlayerChunk()
	{
		Vector3 vector = ((!(follow_obj != null)) ? GameController.Instance.prev_player_pos : follow_obj.transform.position);
		Vector3 chunkCoords = GetChunkCoords(vector + new Vector3(-2.8f, 0f, 2.8f));
		player_chunk_X = (int)chunkCoords.x;
		player_chunk_Z = (int)chunkCoords.z;
	}

	public bool QuestChunkExists(int chunkX, int chunkZ)
	{
		string chunkString = GetChunkString("overworld", chunkX, chunkZ);
		string file_name = DevBuildControl.quest_scenics_folder_ + "/" + chunkString;
		bool file_exists = false;
		ResourceControl.Instance.GetTextFileLines(file_name, ref file_exists);
		return file_exists;
	}

	public List<string> GetAllChunkKeys()
	{
		return new List<string>(Chunks.Keys);
	}

	public void DestroyAllTerrain()
	{
		curr_chunk_loading = "";
		foreach (KeyValuePair<string, Chunk> chunk in Chunks)
		{
			DeleteChunk(chunk.Key);
		}
		Chunks.Clear();
	}

	public void SaveAllLandClaimChunkTimersWithoutDestroying()
	{
		foreach (KeyValuePair<string, Chunk> chunk in Chunks)
		{
			if (chunk.Value.status == Chunk.status_t.complete)
			{
				chunk.Value.chunk_data.SaveLandClaimChunkTimersToDisk(chunk.Key);
			}
		}
	}

	public bool ShowExtendedRange()
	{
		if (GameController.Instance.player.GetComponent<PerkReceiver>().HasPerkRemaining("perk_eagle_eye"))
		{
			return true;
		}
		return GameController.Instance.player.GetComponent<PerkReceiver>().HasPerkRemaining("perk_giant");
	}

	private void TryAddToSurroundingList(string zone, int X, int Z, ref Dictionary<string, ChunkCoordinates> chunks_surrounding_me_now)
	{
		if (DevBuildControl.Instance.debug_bandit_camp_data == null || (X >= 0 && Z >= 0 && X < DevBuildControl.Instance.debug_bandit_camp_W && Z < DevBuildControl.Instance.debug_bandit_camp_H) || !(zone == "overworld"))
		{
			chunks_surrounding_me_now.Add(GetChunkString(zone, X, Z), new ChunkCoordinates(zone, X, Z));
		}
	}

	public void UpdateTerrain(bool calc_chunk)
	{
		if (calc_chunk)
		{
			CalcPlayerChunk();
		}
		bool flag = (GameServerConnector.Instance.FullyInGame() && BreedControl.Instance.state_t != BreedControl.state.none) || GameServerConnector.Instance.MidConnect();
		Dictionary<string, ChunkCoordinates> chunks_surrounding_me_now = new Dictionary<string, ChunkCoordinates>();
		Dictionary<string, ChunkCoordinates> dictionary = new Dictionary<string, ChunkCoordinates>();
		if (!flag)
		{
			TryAddToSurroundingList(player_zone_cache, player_chunk_X, player_chunk_Z, ref chunks_surrounding_me_now);
			TryAddToSurroundingList(player_zone_cache, player_chunk_X + 1, player_chunk_Z, ref chunks_surrounding_me_now);
			TryAddToSurroundingList(player_zone_cache, player_chunk_X - 1, player_chunk_Z, ref chunks_surrounding_me_now);
			TryAddToSurroundingList(player_zone_cache, player_chunk_X, player_chunk_Z + 1, ref chunks_surrounding_me_now);
			TryAddToSurroundingList(player_zone_cache, player_chunk_X, player_chunk_Z - 1, ref chunks_surrounding_me_now);
			TryAddToSurroundingList(player_zone_cache, player_chunk_X + 1, player_chunk_Z + 1, ref chunks_surrounding_me_now);
			TryAddToSurroundingList(player_zone_cache, player_chunk_X + 1, player_chunk_Z - 1, ref chunks_surrounding_me_now);
			TryAddToSurroundingList(player_zone_cache, player_chunk_X - 1, player_chunk_Z + 1, ref chunks_surrounding_me_now);
			TryAddToSurroundingList(player_zone_cache, player_chunk_X - 1, player_chunk_Z - 1, ref chunks_surrounding_me_now);
			bool flag2 = GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host && (DateTime.UtcNow - GameServerConnector.Instance.slow_load_chunks_begin).TotalSeconds < 4.0;
			if (!flag2 && BreedControl.Instance.state_t == BreedControl.state.none && ((GraphicsControl.Instance.GraphicsLevel() == 4) ? ShowExtendedRange() : (GraphicsControl.Instance.GraphicsLevel() > 4)))
			{
				TryAddToSurroundingList(player_zone_cache, player_chunk_X - 2, player_chunk_Z + 2, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X - 1, player_chunk_Z + 2, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X, player_chunk_Z + 2, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X + 1, player_chunk_Z + 2, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X + 2, player_chunk_Z + 2, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X - 2, player_chunk_Z + 1, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X + 2, player_chunk_Z + 1, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X - 2, player_chunk_Z, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X + 2, player_chunk_Z, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X - 2, player_chunk_Z - 1, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X + 2, player_chunk_Z - 1, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X - 2, player_chunk_Z - 2, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X - 1, player_chunk_Z - 2, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X, player_chunk_Z - 2, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X + 1, player_chunk_Z - 2, ref chunks_surrounding_me_now);
				TryAddToSurroundingList(player_zone_cache, player_chunk_X + 2, player_chunk_Z - 2, ref chunks_surrounding_me_now);
			}
		}
		foreach (KeyValuePair<string, ChunkCoordinates> item in chunks_surrounding_me_now)
		{
			dictionary.Add(item.Key, item.Value);
		}
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (KeyValuePair<string, Chunk> chunk in Chunks)
		{
			if (chunks_surrounding_me_now.ContainsKey(chunk.Key))
			{
				dictionary.Remove(chunk.Key);
			}
			else if (chunk.Value.status >= Chunk.status_t.async_building_ && chunk.Value.status <= Chunk.status_t.complete)
			{
				list.Add(chunk.Key);
			}
			else if (chunk.Value.status >= Chunk.status_t.pls_load_and_build && chunk.Value.status <= Chunk.status_t.MP_got_data)
			{
				list2.Add(chunk.Key);
			}
		}
		foreach (string item2 in list)
		{
			DeleteChunk(item2);
			Chunks.Remove(item2);
		}
		foreach (string item3 in list2)
		{
			Chunks.Remove(item3);
		}
		foreach (KeyValuePair<string, ChunkCoordinates> item4 in dictionary)
		{
			Chunks.Add(item4.Key, new Chunk(Chunk.status_t.pls_load_and_build, item4.Value.zone, item4.Value.X, item4.Value.Z, false));
		}
		MobControl.Instance.TryDeloadDistantMobs();
		old_chunk_X = player_chunk_X;
		old_chunk_z = player_chunk_Z;
	}

	public void DevRebuildChunkAt(Vector3 V)
	{
		Vector3 chunkCoords = GetChunkCoords(V);
		int num = (int)chunkCoords.x;
		int num2 = (int)chunkCoords.z;
		string chunkString = GetChunkString(player_zone_cache, num, num2);
		if (DevBuildControl.Instance.debug_bandit_camp_data == null)
		{
			string[] cached_files_full = new string[1] { QuestControl.quest_cache_path + System.IO.Path.DirectorySeparatorChar + chunkString + ".txt" };
			string text = Application.dataPath + System.IO.Path.DirectorySeparatorChar + "SYNCHRONOUS/TextFiles" + System.IO.Path.DirectorySeparatorChar + DevBuildControl.quest_scenics_folder_ + System.IO.Path.DirectorySeparatorChar + chunkString + ".txt";
			string[] game_files_full = ((!System.IO.File.Exists(text)) ? new string[0] : new string[1] { text });
			QuestControl.ScanQuestDataForChanges(cached_files_full, game_files_full);
		}
		DeleteChunk(chunkString);
		Chunks.Remove(chunkString);
		MobControl.Instance.TryDeloadDistantMobs();
		Chunks.Add(chunkString, new Chunk(Chunk.status_t.pls_load_and_build, player_zone_cache, num, num2, false));
	}

	private void DeleteChunk(string chunkStr)
	{
		Chunk chunk = GetChunk(chunkStr);
		if (chunk.status >= Chunk.status_t.async_building_ && chunk.status <= Chunk.status_t.async_building_modularsComplete)
		{
			chunk.is_deleted = true;
			if (chunk.async_build != null)
			{
				StopCoroutine(chunk.async_build);
			}
			if (chunk.chunk_obj != null)
			{
				chunk.chunk_obj.DeleteObjects(chunk.X, chunk.Z, chunk.zone);
			}
		}
		else if (chunk.status == Chunk.status_t.complete)
		{
			chunk.chunk_obj.DeleteObjects(chunk.X, chunk.Z, chunk.zone);
			if (GameServerConnector.Instance.ShouldSaveLocally())
			{
				chunk.chunk_data.SaveLandClaimChunkTimersToDisk(chunkStr);
			}
		}
	}

	public void DevRebuildEntireChunk(string zone, int X, int Z)
	{
		string chunkString = GetChunkString(zone, X, Z);
		if (IsChunkFullyLoadedOrMidload(chunkString))
		{
			DevRebuildChunkAt(new Vector3(X * 10 + 5, 0f, Z * 10 + 5));
		}
	}

	public void RedrawAtSquare(string chunkStr, int innerX, int innerZ, bool log = false)
	{
		if (!IsChunkFullyLoadedOrMidload(chunkStr))
		{
			return;
		}
		Chunk chunk = GetChunk(chunkStr);
		List<ChunkElement> elementsAt = chunk.chunk_data.GetElementsAt(innerX, innerZ);
		foreach (ChunkElement item in elementsAt)
		{
			chunk.chunk_obj.DestroyBuildableInstance(chunk.chunk_data.X, chunk.chunk_data.Z, innerX, innerZ, item.item, item.rot);
		}
		foreach (ChunkElement item2 in elementsAt)
		{
			ConstructionControl.Instance.AsyncCreateBuildableInstance(item2.item, innerX, innerZ, item2.rot, ConstructionControl.build_context_t.on_regular_load, null, chunk.chunk_data, chunk.chunk_obj);
		}
	}

	public void LoadBiomeMapFromDisk(string biome_map_str)
	{
		BiomeMap biomeMap = new BiomeMap();
		for (int i = 0; i < 36; i++)
		{
			for (int j = 0; j < 36; j++)
			{
				biomeMap.SetBiomeIdAt(i, j, PlayerData.Instance.GetSlotShort(i + "," + j + ",id", "biome-map(" + biome_map_str + ")"));
				biomeMap.SetFirstMobAt(i, j, PlayerData.Instance.GetSlotString(i + "," + j + ",mobA", "biome-map(" + biome_map_str + ")"));
				biomeMap.SetSecondMobAt(i, j, PlayerData.Instance.GetSlotString(i + "," + j + ",mobB", "biome-map(" + biome_map_str + ")"));
			}
		}
		loaded_biome_maps.Add(biome_map_str, biomeMap);
	}

	private void GenBlobBiometype(int blob_id, Dictionary<int, int> blob_to_biome_mapping, float blob_min_depth, int n_grass, int n_snow, int n_desert, int n_evergreen, int n_ocean, int n_swamp, int n_woodlands, int n_sakura)
	{
		List<int> possible_biomes = new List<int>();
		AddBiomeToList(0, n_grass, blob_min_depth, ref possible_biomes);
		AddBiomeToList(1, n_snow, blob_min_depth, ref possible_biomes);
		AddBiomeToList(2, n_desert, blob_min_depth, ref possible_biomes);
		AddBiomeToList(3, n_evergreen, blob_min_depth, ref possible_biomes);
		AddBiomeToList(4, n_ocean, blob_min_depth, ref possible_biomes);
		AddBiomeToList(6, n_swamp, blob_min_depth, ref possible_biomes);
		AddBiomeToList(8, n_woodlands, blob_min_depth, ref possible_biomes);
		AddBiomeToList(9, n_sakura, blob_min_depth, ref possible_biomes);
		int value = possible_biomes[UnityEngine.Random.Range(0, possible_biomes.Count)];
		blob_to_biome_mapping.Add(blob_id, value);
	}

	private void AddBiomeToList(int biome_id, int n_add, float blob_min_depth, ref List<int> possible_biomes)
	{
		if (biomes[biome_id].min_depth <= blob_min_depth)
		{
			for (int i = 0; i < n_add; i++)
			{
				possible_biomes.Add(biome_id);
			}
		}
	}

	private void GenBlobMob(int blob_id, Dictionary<int, int> blob_to_biome_mapping, Dictionary<int, string> blob_to_mobA_mapping, Dictionary<int, string> blob_to_mobB_mapping)
	{
		int num = blob_to_biome_mapping[blob_id];
		if (biomes[num].copy_possible_mobs_from != -1)
		{
			num = biomes[num].copy_possible_mobs_from;
		}
		string[] possible_mobs = biomes[num].possible_mobs;
		string value = "crab";
		string value2 = "crab";
		if (possible_mobs.Length != 0)
		{
			value = possible_mobs[UnityEngine.Random.Range(0, possible_mobs.Length)];
			value2 = possible_mobs[UnityEngine.Random.Range(0, possible_mobs.Length)];
		}
		blob_to_mobA_mapping.Add(blob_id, value);
		blob_to_mobB_mapping.Add(blob_id, value2);
	}

	public BiomeMap GenerateNewBiomeMap(int ref_chunk_X, int ref_chunk_Z, int n_grass, int n_snow, int n_desert, int n_evergreen, int n_ocean, int n_swamp, int n_woodlands, int n_sakura)
	{
		BiomeMap biomeMap = new BiomeMap();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, string> dictionary2 = new Dictionary<int, string>();
		Dictionary<int, string> dictionary3 = new Dictionary<int, string>();
		Dictionary<int, float> dictionary4 = new Dictionary<int, float>();
		for (int i = 0; i < 36; i++)
		{
			for (int j = 0; j < 36; j++)
			{
				int key = template_biome_map_ids[i, j];
				float num = Vector3.Distance(Vector3.zero, new Vector3((ref_chunk_X - Mod(ref_chunk_X, 36) + i) * 10 + 5, 0f, (ref_chunk_Z - Mod(ref_chunk_Z, 36) + j) * 10 + 5));
				if (!dictionary4.ContainsKey(key))
				{
					dictionary4.Add(key, num);
				}
				else if (num < dictionary4[key])
				{
					dictionary4[key] = num;
				}
			}
		}
		for (int k = 0; k < 36; k++)
		{
			for (int l = 0; l < 36; l++)
			{
				int num2 = template_biome_map_ids[k, l];
				if (!dictionary.ContainsKey(num2))
				{
					GenBlobBiometype(num2, dictionary, dictionary4[num2], n_grass, n_snow, n_desert, n_evergreen, n_ocean, n_swamp, n_woodlands, n_sakura);
					GenBlobMob(num2, dictionary, dictionary2, dictionary3);
				}
				biomeMap.SetBiomeIdAt(k, l, dictionary[num2]);
				biomeMap.SetFirstMobAt(k, l, dictionary2[num2]);
				biomeMap.SetSecondMobAt(k, l, dictionary3[num2]);
				int biomeIdAt = biomeMap.GetBiomeIdAt(k, l);
				if (biomeIdAt == 6)
				{
					if (UnityEngine.Random.value < 0.5f)
					{
						biomeMap.SetBiomeIdAt(k, l, 7);
					}
				}
				else if (biomeIdAt == 4 && UnityEngine.Random.value < 0.13f)
				{
					biomeMap.SetBiomeIdAt(k, l, 5);
				}
			}
		}
		return biomeMap;
	}

	public int GetBiomeInnerX(int chunkX)
	{
		return Mod(chunkX, 36);
	}

	public int GetBiomeInnerZ(int chunkZ)
	{
		return Mod(chunkZ, 36);
	}

	public void GetBiomeMapCoordinates(int chunkX, int chunkZ, ref int biome_X, ref int biome_Z)
	{
		biome_X = Mathf.FloorToInt((float)chunkX / 36f);
		biome_Z = Mathf.FloorToInt((float)chunkZ / 36f);
	}

	public string BiomeMobAt(int chunkX, int chunkZ, bool mobA)
	{
		int biome_X = 0;
		int biome_Z = 0;
		GetBiomeMapCoordinates(chunkX, chunkZ, ref biome_X, ref biome_Z);
		string key = biome_X + "," + biome_Z;
		int biomeInnerX = GetBiomeInnerX(chunkX);
		int biomeInnerZ = GetBiomeInnerZ(chunkZ);
		string firstMobAt = loaded_biome_maps[key].GetFirstMobAt(biomeInnerX, biomeInnerZ);
		string secondMobAt = loaded_biome_maps[key].GetSecondMobAt(biomeInnerX, biomeInnerZ);
		if (!mobA)
		{
			return secondMobAt;
		}
		return firstMobAt;
	}

	public string GetBiomeString(int chunkX, int chunkZ)
	{
		int biome_X = 0;
		int biome_Z = 0;
		GetBiomeMapCoordinates(chunkX, chunkZ, ref biome_X, ref biome_Z);
		return biome_X + "," + biome_Z;
	}

	public int GetBiomeIdAt(int chunkX, int chunkZ)
	{
		string biomeString = GetBiomeString(chunkX, chunkZ);
		if (!loaded_biome_maps.ContainsKey(biomeString))
		{
			if (PlayerData.Instance.GetSlotShort("exists", "biome-map(" + biomeString + ")") == 1)
			{
				LoadBiomeMapFromDisk(biomeString);
			}
			else
			{
				BiomeMap biomeMap = GenerateNewBiomeMap(chunkX, chunkZ, 2, 1, 1, 1, 2, 1, 1, 1);
				biomeMap.SaveToDisk(biomeString);
				loaded_biome_maps.Add(biomeString, biomeMap);
			}
		}
		return loaded_biome_maps[biomeString].GetBiomeIdAt(GetBiomeInnerX(chunkX), GetBiomeInnerZ(chunkZ));
	}

	private int Mod(int x, int m)
	{
		int num = x % m;
		if (num >= 0)
		{
			return num;
		}
		return num + m;
	}

	public cave_define GetCorrespondingCave(string zone_obj)
	{
		cave_define[] array = cave_defines;
		for (int i = 0; i < array.Length; i++)
		{
			cave_define result = array[i];
			if (result.name == zone_obj)
			{
				return result;
			}
		}
		return cave_defines[0];
	}

	public bool IsChunkFullyLoaded(string chunkStr)
	{
		if (Chunks.ContainsKey(chunkStr) && Chunks[chunkStr].status == Chunk.status_t.complete)
		{
			return Chunks[chunkStr].chunk_obj != null;
		}
		return false;
	}

	public bool IsChunkFullyLoadedOrMidload(string chunkStr)
	{
		if (!Chunks.ContainsKey(chunkStr))
		{
			return false;
		}
		if (Chunks[chunkStr].status == Chunk.status_t.complete || Chunks[chunkStr].status == Chunk.status_t.async_building_ || Chunks[chunkStr].status == Chunk.status_t.async_building_buildablesComplete || Chunks[chunkStr].status == Chunk.status_t.async_building_modularsComplete)
		{
			return Chunks[chunkStr].chunk_obj != null;
		}
		return false;
	}

	public bool ChunkExists(string chunkStr)
	{
		return Chunks.ContainsKey(chunkStr);
	}

	public int GetNumItemsInSurroundingArea(string item_name)
	{
		int num = 0;
		foreach (KeyValuePair<string, Chunk> chunk in Chunks)
		{
			ChunkData chunk_data = chunk.Value.chunk_data;
			if (chunk_data == null)
			{
				continue;
			}
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					foreach (ChunkElement item in chunk_data.GetElementsAt(i, j))
					{
						if (item.item.item_name == item_name)
						{
							num++;
						}
					}
				}
			}
		}
		return num;
	}

	public void TemporarilyDisableChunkObjects(Vector3 origin, float range, List<GameObject> new_temporarily_disabled)
	{
		foreach (Chunk chunk in Chunks.Values)
		{
			if (((int)chunk.status & -4) == 4)
			{
				chunk.chunk_obj.TemporarilyDisableChunkObjs(origin, range, new_temporarily_disabled);
			}
		}
	}

	public Chunk GetChunk(string chunkStr)
	{
		if (!Chunks.ContainsKey(chunkStr))
		{
			return null;
		}
		return Chunks[chunkStr];
	}

	public ChunkObj GetChunkObj(string chunkStr)
	{
		return GetChunk(chunkStr)?.chunk_obj;
	}

	public ChunkData GetChunkData(string chunkStr)
	{
		return GetChunk(chunkStr)?.chunk_data;
	}

	public void RemoveAllChunksWithTag(Chunk.status_t remove_status)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Chunk> chunk in Chunks)
		{
			if (chunk.Value.status == remove_status)
			{
				list.Add(chunk.Key);
			}
		}
		foreach (string item in list)
		{
			Chunks.Remove(item);
		}
	}

	public void CreateAllEdgePieces()
	{
		foreach (KeyValuePair<string, Chunk> chunk in Chunks)
		{
			if (chunk.Value.status >= Chunk.status_t.async_building_ && chunk.Value.status <= Chunk.status_t.complete)
			{
				chunk.Value.chunk_obj.TryCreateEdgePieces(chunk.Value.chunk_data);
			}
		}
	}

	public void ChangeChunkStatus(string chunkStr, Chunk.status_t new_status)
	{
		if (!Chunks.ContainsKey(chunkStr))
		{
			return;
		}
		Chunk.status_t status = Chunks[chunkStr].status;
		switch (new_status)
		{
		case Chunk.status_t.MP_requested:
			if (status == Chunk.status_t.MP_got_data || status == Chunk.status_t.complete)
			{
				return;
			}
			break;
		case Chunk.status_t.MP_got_data:
			if (status == Chunk.status_t.complete)
			{
				return;
			}
			break;
		case Chunk.status_t.async_building_buildablesComplete:
			if (status == Chunk.status_t.async_building_modularsComplete)
			{
				Chunks[chunkStr].status = Chunk.status_t.complete;
				return;
			}
			break;
		case Chunk.status_t.async_building_modularsComplete:
			if (status == Chunk.status_t.complete)
			{
				return;
			}
			if (status == Chunk.status_t.async_building_buildablesComplete)
			{
				Chunks[chunkStr].status = Chunk.status_t.complete;
				return;
			}
			break;
		}
		Chunks[chunkStr].status = new_status;
	}

	private Chunk GetNearestChunkWithTag(Chunk.status_t status)
	{
		float num = float.MaxValue;
		Chunk result = null;
		foreach (KeyValuePair<string, Chunk> chunk in Chunks)
		{
			if (chunk.Value.status == status)
			{
				float num2 = Vector3.Distance(new Vector3(chunk.Value.X * 10 + 5, 0f, chunk.Value.Z * 10 + 5), GameController.Instance.prev_player_pos);
				if (num2 < num)
				{
					num = num2;
					result = chunk.Value;
				}
			}
		}
		return result;
	}

	public int ChunksUntilEndTransition()
	{
		List<Chunk> list = new List<Chunk>();
		foreach (KeyValuePair<string, Chunk> chunk in Chunks)
		{
			float num = Vector3.Distance(new Vector3(chunk.Value.X * 10 + 5, 0f, chunk.Value.Z * 10 + 5), GameController.Instance.prev_player_pos);
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				if (num < Vector3.Distance(new Vector3(list[i].X * 10 + 5, 0f, list[i].Z * 10 + 5), GameController.Instance.prev_player_pos))
				{
					list.Insert(i, chunk.Value);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(chunk.Value);
			}
		}
		if (list.Count < 9)
		{
			return 9 - list.Count;
		}
		while (list.Count > 9)
		{
			list.RemoveAt(list.Count - 1);
		}
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			if (list[num2].status == Chunk.status_t.complete)
			{
				list.RemoveAt(num2);
			}
		}
		return list.Count;
	}

	private void FixedUpdate()
	{
		bool flag = false;
		bool flag2 = false;
		Connection game_server_connection = GameServerConnector.Instance.game_server_connection;
		if (game_server_connection == null)
		{
			flag2 = true;
		}
		else
		{
			switch (game_server_connection.GetStatus())
			{
			case Connection.connection_status.not_connected:
				flag2 = true;
				break;
			case Connection.connection_status.connecting:
				if (!GameServerConnector.Instance.is_host)
				{
					return;
				}
				flag2 = true;
				break;
			case Connection.connection_status.connected:
				if (GameServerConnector.Instance.is_host)
				{
					flag2 = true;
					break;
				}
				if (!GameServerConnector.Instance.completely_logged_in || GameServerReceiver.Instance.waiting_on_initial_zone_data)
				{
					return;
				}
				foreach (KeyValuePair<string, Chunk> chunk3 in Chunks)
				{
					if (chunk3.Value.status == Chunk.status_t.pls_load_and_build)
					{
						GameServerSender.Instance.SendRequestChunkAt(chunk3.Value.zone, chunk3.Value.X, chunk3.Value.Z);
						ChangeChunkStatus(chunk3.Key, Chunk.status_t.MP_requested);
					}
				}
				flag = true;
				break;
			default:
				return;
			}
		}
		if (curr_chunk_loading != "" && (!Chunks.ContainsKey(curr_chunk_loading) || Chunks[curr_chunk_loading].status == Chunk.status_t.complete))
		{
			curr_chunk_loading = "";
		}
		if (flag2)
		{
			if (curr_chunk_loading == "")
			{
				Chunk nearestChunkWithTag = GetNearestChunkWithTag(Chunk.status_t.pls_load_and_build);
				if (nearestChunkWithTag != null)
				{
					curr_chunk_loading = GetChunkString(nearestChunkWithTag.zone, nearestChunkWithTag.X, nearestChunkWithTag.Z);
					nearestChunkWithTag.chunk_data = HostGetChunk(nearestChunkWithTag.zone, nearestChunkWithTag.X, nearestChunkWithTag.Z);
					nearestChunkWithTag.AsyncBuildAndAdd(this);
				}
			}
		}
		else if (flag && curr_chunk_loading == "")
		{
			Chunk nearestChunkWithTag2 = GetNearestChunkWithTag(Chunk.status_t.MP_got_data);
			if (nearestChunkWithTag2 != null)
			{
				curr_chunk_loading = GetChunkString(nearestChunkWithTag2.zone, nearestChunkWithTag2.X, nearestChunkWithTag2.Z);
				nearestChunkWithTag2.AsyncBuildAndAdd(this);
			}
		}
	}

	private IEnumerator BuildPass(ChunkData chunk_data, ChunkObj chunk_obj, Chunk chunk, build_pass_type_t build_pass_type, Action on_complete)
	{
		for (int x = 0; x < 10; x++)
		{
			for (int z = 0; z < 10; z++)
			{
				foreach (ChunkElement element in chunk_data.GetElementsAt(x, z))
				{
					bool buildable_created = false;
					int chunkX = chunk_data.X;
					int chunkZ = chunk_data.Z;
					string text = element.item.GetString("tag");
					if (build_pass_type == build_pass_type_t.dev_placed_and_player_placed)
					{
						if (text == "natural")
						{
							continue;
						}
					}
					else if (build_pass_type == build_pass_type_t.natural)
					{
						if (text != "natural")
						{
							continue;
						}
						bool flag = false;
						foreach (Vector3 item in ConstructionControl.Instance.GetObjectLocalGeometry(element.item.item_name, element.rot))
						{
							foreach (OccupiedSpace item2 in GetBuildablesThatOverlapThisSpace(new Vector3((float)chunkX * 10f + (float)x + 0.5f + item.x, item.y + 0f, (float)chunkZ * 10f + (float)z + 0.5f + item.z)))
							{
								if (item2.layer != "sub_flooring")
								{
									flag = true;
									break;
								}
							}
							if (flag)
							{
								break;
							}
						}
						if (flag)
						{
							continue;
						}
					}
					if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) && ZoneDataControl.Instance.curr_cave_exit != null && Vector3.Distance(ZoneDataControl.Instance.curr_cave_exit.transform.position, new Vector3((float)chunk_data.X * 10f + (float)x + 0.5f, 0f, (float)chunk_data.Z * 10f + (float)z + 0.5f)) < dist_empty_around_cave_ladder)
					{
						continue;
					}
					buildable_created = false;
					ConstructionControl.Instance.AsyncCreateBuildableInstance(element.item, x, z, element.rot, ConstructionControl.build_context_t.on_regular_load, chunk, chunk_data, chunk_obj, delegate
					{
						buildable_created = true;
					});
					while (!buildable_created)
					{
						yield return null;
					}
				}
			}
		}
		on_complete();
	}

	public IEnumerator AsyncBuildAndAddChunkCoroutine(Chunk chunk)
	{
		ChunkData chunk_data = chunk.chunk_data;
		string zone = chunk.zone;
		string chunkStr = GetChunkString(zone, chunk.X, chunk.Z);
		ChunkObj chunk_obj = new ChunkObj(chunk.X, chunk.Z, zone);
		chunk.chunk_obj = chunk_obj;
		cave_define cave_biome;
		biome overworld_biome;
		if (zone == "overworld" || InventoryUtils.IsPureDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			cave_biome = cave_defines[0];
			overworld_biome = biomes[chunk_data.biome];
		}
		else if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			cave_biome = GetCorrespondingCave(ZoneDataControl.Instance.curr_zonedata.house_item.item_name);
			overworld_biome = biomes[cave_biome.corresponding_biome_id];
		}
		else
		{
			overworld_biome = biomes[0];
			cave_biome = cave_defines[0];
		}
		GameObject floor_parent_obj = new GameObject("floor");
		floor_parent_obj.transform.position = new Vector3((float)chunk_data.X * 10f, 0f, (float)chunk_data.Z * 10f);
		chunk_obj.parent_obj = floor_parent_obj;
		if (zone == "overworld" || InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) || InventoryUtils.IsPureDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			Rotate(floor_parent_obj, new Vector3(5f, 0f, 5f), (float)chunk_data.floor_rotation * 90f);
		}
		if (BreedControl.Instance.state_t != BreedControl.state.none)
		{
			enable_on_accept.Add(floor_parent_obj);
			floor_parent_obj.SetActive(false);
		}
		bool biome_floor_loaded = false;
		if (zone == "overworld" || InventoryUtils.IsPureDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			GameObject check_not_null = floor_parent_obj;
			string floor_tex_path = ((chunk_data.floor_texture_index >= overworld_biome.floor_texture_paths.Length) ? overworld_biome.floor_texture_paths[0] : overworld_biome.floor_texture_paths[chunk_data.floor_texture_index]);
			ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab("BiomePrefabs/" + overworld_biome.floor_prefab_path, chunk, delegate(GameObject new_floor_instance)
			{
				if (check_not_null == null)
				{
					UnityEngine.Object.Destroy(new_floor_instance);
				}
				else
				{
					new_floor_instance.transform.SetParent(floor_parent_obj.transform);
					new_floor_instance.transform.localPosition = Vector3.zero;
					new_floor_instance.transform.localRotation = Quaternion.identity;
					new_floor_instance.transform.localScale = Vector3.one;
					ResourceControl.Instance.AssignBiomeFloorTexture(floor_tex_path, new_floor_instance.transform.Find("floor-plane").GetComponent<MeshRenderer>(), delegate
					{
						biome_floor_loaded = true;
					});
				}
			});
		}
		else if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			if (chunk_data.floor_model_id != 0)
			{
				GameObject check_not_null2 = floor_parent_obj;
				ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab("CavePrefabs/walls etc/" + cave_wall_prefab_paths[chunk_data.floor_model_id], chunk, delegate(GameObject wall_instance)
				{
					if (check_not_null2 == null)
					{
						UnityEngine.Object.Destroy(wall_instance);
					}
					else
					{
						wall_instance.transform.SetParent(floor_parent_obj.transform);
						wall_instance.transform.localPosition = Vector3.zero;
						wall_instance.transform.localRotation = Quaternion.identity;
						wall_instance.transform.localScale = Vector3.one;
						ChunkGeneratorCaves.AsyncAssignTerrainTex(chunk_data.floor_model_id, cave_biome, wall_instance);
						int floor_texture_index = chunk_data.floor_texture_index;
						if (floor_texture_index == 0)
						{
							biome_floor_loaded = true;
						}
						else
						{
							int num3 = ((floor_texture_index - 100 > 0) ? (-101) : (-1));
							GameObject gameObject = UnityEngine.Object.Instantiate(cave_art_prefab);
							gameObject.transform.SetParent(floor_parent_obj.transform);
							gameObject.transform.localPosition = new Vector3(5f, 0f, 5f);
							gameObject.transform.rotation = Quaternion.identity;
							if (floor_texture_index - 100 > 0)
							{
								gameObject.transform.Rotate(Vector3.up, 90f);
							}
							ResourceControl.Instance.AssignCaveArtTexture(cave_art_textures_path[num3 + floor_texture_index], gameObject.transform.Find("cave art tex").GetComponent<MeshRenderer>(), delegate
							{
								biome_floor_loaded = true;
							});
						}
					}
				});
			}
			else
			{
				biome_floor_loaded = true;
			}
		}
		else if (InventoryUtils.IsHeavenDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			GameObject check_not_null3 = floor_parent_obj;
			ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab("BiomePrefabs/biome-heaven/floor-clouds", chunk, delegate(GameObject cloud_floor_instance)
			{
				if (check_not_null3 == null)
				{
					UnityEngine.Object.Destroy(cloud_floor_instance);
				}
				else
				{
					cloud_floor_instance.transform.SetParent(floor_parent_obj.transform);
					cloud_floor_instance.transform.localPosition = Vector3.zero;
					cloud_floor_instance.transform.localRotation = Quaternion.identity;
					cloud_floor_instance.transform.localScale = Vector3.one;
					biome_floor_loaded = true;
				}
			});
		}
		else if (InventoryUtils.IsHellDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			GameObject check_not_null4 = floor_parent_obj;
			ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab("BiomePrefabs/biome-hell/floor-hell", chunk, delegate(GameObject hell_floor_instance)
			{
				if (check_not_null4 == null)
				{
					UnityEngine.Object.Destroy(hell_floor_instance);
				}
				else
				{
					hell_floor_instance.transform.SetParent(floor_parent_obj.transform);
					hell_floor_instance.transform.localPosition = Vector3.zero;
					hell_floor_instance.transform.localRotation = Quaternion.identity;
					hell_floor_instance.transform.localScale = Vector3.one;
					ResourceControl.Instance.AssignBiomeFloorTexture("floor-hell", hell_floor_instance.transform.Find("floor-plane").GetComponent<MeshRenderer>(), delegate
					{
						biome_floor_loaded = true;
					});
				}
			});
		}
		else if (ZoneDataControl.Instance.curr_zonedata.house_item.item_name == "Pocket World Basement")
		{
			GameObject check_not_null5 = floor_parent_obj;
			ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab("BiomePrefabs/biome-all/floor-basic", chunk, delegate(GameObject basement_floor_instance)
			{
				if (check_not_null5 == null)
				{
					UnityEngine.Object.Destroy(basement_floor_instance);
				}
				else
				{
					basement_floor_instance.transform.SetParent(floor_parent_obj.transform);
					basement_floor_instance.transform.localPosition = Vector3.zero;
					basement_floor_instance.transform.localRotation = Quaternion.identity;
					basement_floor_instance.transform.localScale = Vector3.one;
					ResourceControl.Instance.AssignBiomeFloorTexture("floor-king-wing-basement", basement_floor_instance.transform.Find("floor-plane").GetComponent<MeshRenderer>(), delegate
					{
						biome_floor_loaded = true;
					});
				}
			});
		}
		else
		{
			biome_floor_loaded = true;
		}
		CreateAllEdgePieces();
		while (!biome_floor_loaded)
		{
			yield return null;
		}
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				foreach (ChunkElement item in chunk_data.GetElementsAt(i, j))
				{
					string item_name = item.item.item_name;
					if (inventory_ctr.Instance.GetItemBool(item_name, "is_flooring_obj"))
					{
						ModularObjectControl.Instance.ModularChangedAt(i, j, ModularObjectControl.type.PATHWAYS, chunk_data.zone, chunk_data.X, chunk_data.Z);
					}
					else if (inventory_ctr.Instance.GetItemBool(item_name, "is_wall_obj"))
					{
						ModularObjectControl.Instance.ModularChangedAt(i, j, ModularObjectControl.type.WALLS, chunk_data.zone, chunk_data.X, chunk_data.Z);
					}
				}
			}
		}
		chunk.all_modulars_accounted_for = true;
		if (chunk.mid_load_modulars.Count == 0)
		{
			ChangeChunkStatus(chunkStr, Chunk.status_t.async_building_modularsComplete);
		}
		yield return null;
		bool build_pass_1_complete = false;
		StartCoroutine(BuildPass(chunk_data, chunk_obj, chunk, build_pass_type_t.dev_placed_and_player_placed, delegate
		{
			build_pass_1_complete = true;
		}));
		while (!build_pass_1_complete)
		{
			yield return null;
		}
		bool build_pass_2_complete = false;
		StartCoroutine(BuildPass(chunk_data, chunk_obj, chunk, build_pass_type_t.natural, delegate
		{
			build_pass_2_complete = true;
		}));
		while (!build_pass_2_complete)
		{
			yield return null;
		}
		bool flag = zone == "overworld" || InventoryUtils.IsPureDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name);
		bool flag2 = InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name);
		if (((flag && !Startup.StringNullOrWhitespace(overworld_biome.grass_prefab_path)) || (flag2 && (!Startup.StringNullOrWhitespace(cave_biome.grass_biomePrefab_path) || !Startup.StringNullOrWhitespace(cave_biome.grass_cavePrefab_path)))) && (!InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) || chunk_data.floor_model_id != 0))
		{
			int density = 0;
			if (zone == "overworld" || InventoryUtils.IsPureDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
			{
				density = GraphicsControl.Instance.HowMuchGrassToSpawn(overworld_biome.grass_density);
			}
			else if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
			{
				density = GraphicsControl.Instance.HowMuchGrassToSpawn(cave_biome.grass_density);
			}
			for (int k = 0; k < density; k++)
			{
				int num = UnityEngine.Random.Range(0, 10);
				int num2 = UnityEngine.Random.Range(0, 10);
				if (!chunk_data.EmptyAt(num, num2))
				{
					continue;
				}
				Vector3 attempt_pos = new Vector3((float)num + 0.5f + (float)chunk_data.X * 10f, 0f, (float)num2 + 0.5f + (float)chunk_data.Z * 10f);
				if (GetBuildablesThatOverlapThisSpace(attempt_pos).Count != 0)
				{
					continue;
				}
				string obj_path = "";
				if (zone == "overworld" || InventoryUtils.IsPureDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
				{
					obj_path = "BiomePrefabs/" + overworld_biome.grass_prefab_path;
				}
				else if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
				{
					if (!Startup.StringNullOrWhitespace(cave_biome.grass_biomePrefab_path))
					{
						obj_path = "BiomePrefabs/" + cave_biome.grass_biomePrefab_path;
					}
					else if (!Startup.StringNullOrWhitespace(cave_biome.grass_cavePrefab_path))
					{
						obj_path = "CavePrefabs/" + cave_biome.grass_cavePrefab_path;
					}
				}
				bool grass_loaded = false;
				GameObject check_not_null6 = floor_parent_obj;
				ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab(obj_path, chunk, delegate(GameObject grass_instance)
				{
					if (check_not_null6 == null)
					{
						UnityEngine.Object.Destroy(grass_instance);
					}
					else
					{
						grass_instance.name = "grass";
						float num4 = UnityEngine.Random.Range(0.8f, 1.6f);
						grass_instance.transform.position = attempt_pos;
						if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) && cave_biome.grass_hangs_from_cieling)
						{
							grass_instance.transform.position += Vector3.up * 3.5f;
						}
						if (overworld_biome.grass_rotate)
						{
							grass_instance.transform.Rotate(Vector3.up, (float)UnityEngine.Random.Range(0, 4) * 90f);
						}
						grass_instance.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.7f, 1.4f);
						grass_instance.transform.localScale = new Vector3(grass_instance.transform.localScale.x, num4 * grass_instance.transform.localScale.y, grass_instance.transform.localScale.z);
						grass_instance.transform.SetParent(floor_parent_obj.transform);
						chunk_obj.grasses.Add(grass_instance);
						grass_loaded = true;
					}
				});
				while (!grass_loaded)
				{
					yield return null;
				}
			}
		}
		ChangeChunkStatus(curr_chunk_loading, Chunk.status_t.async_building_buildablesComplete);
	}

	public int RemoveUnderlyingWaterForDevPlacedScenics_(int biome_in, string file_path)
	{
		int result = biome_in;
		if (biome_in == 7 || biome_in == 4)
		{
			bool file_exists = false;
			ResourceControl.Instance.GetTextFileLines(file_path, ref file_exists);
			if (file_exists)
			{
				result = 5;
			}
			if (biome_in != 4 && file_exists && biome_in == 7)
			{
				result = 6;
			}
		}
		return result;
	}

	public List<OccupiedSpace> GetBuildablesThatOverlapThisSpace(Vector3 clickRounded)
	{
		List<OccupiedSpace> list = new List<OccupiedSpace>();
		foreach (OccupiedSpace global_spaces_occupied_by_buildable in global_spaces_occupied_by_buildables)
		{
			if (Vector3.Distance(global_spaces_occupied_by_buildable.pos, clickRounded) < 0.1f)
			{
				list.Add(global_spaces_occupied_by_buildable);
			}
		}
		return list;
	}

	public int GetOverworldBiomeBelow(GameObject obj, bool log = false)
	{
		if (obj == null)
		{
			return 0;
		}
		string chunkString = Instance.GetChunkString(obj.transform.position);
		if (!Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			if (log)
			{
				Debug.Log("Chunk not fully loaded [" + chunkString + "]");
			}
			return 0;
		}
		if (log)
		{
			Debug.Log("Chunk loaded [" + chunkString + "]");
		}
		return Instance.GetChunk(chunkString).chunk_data.biome;
	}

	public pathway_type GetPathwayTypeBeneathMe(Vector3 V)
	{
		string chunkString = GetChunkString(V);
		if (!IsChunkFullyLoadedOrMidload(chunkString))
		{
			return pathway_type.none;
		}
		ChunkData chunkData = GetChunk(chunkString)?.chunk_data;
		Vector3 inner = GetInner(V);
		pathway_type result = pathway_type.none;
		foreach (ChunkElement item in chunkData.GetElementsAt((int)inner.x, (int)inner.z))
		{
			string item_name = item.item.item_name;
			if (item_name == "Dirt Path")
			{
				return pathway_type.gravel;
			}
			if (item_name == "Cobblestone Path" || item_name == "Stone Bricks")
			{
				return pathway_type.stone;
			}
			if (item_name == "Bouncy Floor")
			{
				result = pathway_type.bouncy;
			}
			else if (item_name == "Clouds")
			{
				result = pathway_type.sand;
			}
			else if (item_name == "Lava")
			{
				result = pathway_type.lava;
			}
		}
		return result;
	}

	public bool IsMansionWoodFloorBeneathMe(Vector3 V)
	{
		ZoneData curr_zonedata = ZoneDataControl.Instance.curr_zonedata;
		int interior_model_chunkZ = curr_zonedata.interior_model_chunkZ;
		int interior_model_innerZ = curr_zonedata.interior_model_innerZ;
		return (float)(int)GameController.Instance.player.transform.position.z - ((float)(interior_model_innerZ + interior_model_chunkZ * 10) + 0.5f) >= 7.5f;
	}

	public bool IsWindmillWoodFloorBeneathMe(Vector3 V)
	{
		ZoneData curr_zonedata = ZoneDataControl.Instance.curr_zonedata;
		int interior_model_chunkX = curr_zonedata.interior_model_chunkX;
		int interior_model_chunkZ = curr_zonedata.interior_model_chunkZ;
		int interior_model_innerX = curr_zonedata.interior_model_innerX;
		int interior_model_innerZ = curr_zonedata.interior_model_innerZ;
		float x = GameController.Instance.player.transform.position.x;
		if ((float)(int)GameController.Instance.player.transform.position.z - ((float)(interior_model_innerZ + interior_model_chunkZ * 10) + 0.5f) >= 7.5f)
		{
			return true;
		}
		return (float)(int)x - ((float)(interior_model_innerX + interior_model_chunkX * 10) + 0.5f) <= -6.5f;
	}

	public int GetCaveFloorModelBelow(GameObject obj)
	{
		string chunkString = GetChunkString(obj.transform.position);
		return IsChunkFullyLoadedOrMidload(chunkString) ? GetChunk(chunkString).chunk_data.floor_model_id : 0;
	}

	public int GetCaveExitFloorModel()
	{
		if (ZoneDataControl.Instance.curr_cave_exit == null) return -1;
		string chunkString = GetChunkString(ZoneDataControl.Instance.curr_cave_exit.transform.position);
		return IsChunkFullyLoadedOrMidload(chunkString) ? GetChunk(chunkString).chunk_data.floor_model_id : -1;
	}

	private void Rotate(GameObject G, Vector3 local_origin, float degrees)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.SetParent(G.transform);
		gameObject.transform.localPosition = local_origin;
		gameObject.transform.SetParent(G.transform.parent);
		gameObject.transform.localRotation = Quaternion.identity;
		G.transform.SetParent(gameObject.transform);
		gameObject.transform.Rotate(Vector3.up, degrees);
		G.transform.SetParent(gameObject.transform.parent);
		UnityEngine.Object.Destroy(gameObject);
	}
}
