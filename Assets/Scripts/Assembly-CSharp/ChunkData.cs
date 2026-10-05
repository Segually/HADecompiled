using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData
{
	public int X;

	public int Z;

	public string zone = "";

	public int biome;

	public int floor_model_id;

	public int floor_texture_index;

	public int floor_rotation;

	public string biome_mobA = "";

	public string biome_mobB = "";

	public int quest_version;

	public Dictionary<string, List<ChunkElement>> chunk_elements = new Dictionary<string, List<ChunkElement>>();

	public Dictionary<string, LandClaimChunkTimer> land_claim_chunk_timers_ = new Dictionary<string, LandClaimChunkTimer>();

	public string mp_cache_key = "";

	public int mp_chunk_size;

	public DateTime mp_cache_last_used;

	public bool land_claim_chunk_timers_changed;

	public void TickAllLandClaimChunkTimers()
	{
		List<KeyValuePair<string, LandClaimChunkTimer>> list = new List<KeyValuePair<string, LandClaimChunkTimer>>();
		foreach (KeyValuePair<string, LandClaimChunkTimer> item in land_claim_chunk_timers_)
		{
			if (item.Value.when_to_expire < DateTime.UtcNow)
			{
				list.Add(item);
			}
		}
		if (list.Count != 0)
		{
			land_claim_chunk_timers_changed = true;
		}
		foreach (KeyValuePair<string, LandClaimChunkTimer> item2 in list)
		{
			land_claim_chunk_timers_.Remove(item2.Key);
			if (ChunkControl.Instance.player_zone != "overworld")
			{
				ZoneDataControl.Instance.curr_zonedata.outdoor_land_claim_chunk_timers.Remove(item2.Key);
			}
		}
	}

	public void AddElement(int x, int z, ChunkElement element)
	{
		if (!chunk_elements.ContainsKey(x + "," + z))
		{
			chunk_elements.Add(x + "," + z, new List<ChunkElement>());
		}
		chunk_elements[x + "," + z].Add(element);
	}

	public void ReplaceElementItem(int x, int z, InventoryItem new_item, InventoryItem old_element_item, int old_element_rot)
	{
		if (!chunk_elements.ContainsKey(x + "," + z))
		{
			return;
		}
		bool flag = InventoryUtils.ShouldReplaceOrDeleteExactItem(old_element_item.item_name);
		for (int i = 0; i < chunk_elements[x + "," + z].Count; i++)
		{
			InventoryItem item = chunk_elements[x + "," + z][i].item;
			if (flag ? (item == old_element_item && chunk_elements[x + "," + z][i].rot == old_element_rot) : (item.item_name == old_element_item.item_name))
			{
				chunk_elements[x + "," + z][i].item = new_item;
				break;
			}
		}
	}

	public void RemoveElement(int x, int z, ChunkElement delete_element)
	{
		if (!chunk_elements.ContainsKey(x + "," + z))
		{
			return;
		}
		bool flag = InventoryUtils.ShouldReplaceOrDeleteExactItem(delete_element.item.item_name);
		List<ChunkElement> list = chunk_elements[x + "," + z];
		for (int i = 0; i < list.Count; i++)
		{
			if (flag ? (list[i].item == delete_element.item && list[i].rot == delete_element.rot) : (list[i].item.item_name == delete_element.item.item_name))
			{
				chunk_elements[x + "," + z].RemoveAt(i);
				break;
			}
		}
	}

	public bool EmptyAt(int x, int z)
	{
		if (!chunk_elements.ContainsKey(x + "," + z))
		{
			return true;
		}
		return chunk_elements[x + "," + z].Count == 0;
	}

	public List<ChunkElement> GetElementsAt(int x, int z)
	{
		bool flag = chunk_elements.ContainsKey(x + "," + z);
		List<ChunkElement> list = new List<ChunkElement>();
		if (flag)
		{
			foreach (ChunkElement item in chunk_elements[x + "," + z])
			{
				list.Add(item);
			}
		}
		return list;
	}

	public void InjectDevPlacedBuildables(string file_path, string set_items_tag, int new_chunk_rotation = 0, bool on_bandit_camp = false, Dictionary<string, ZoneData> auto_built_zones = null, string bandit_camp_instance_name = "")
	{
		ForcedChunkData forcedChunkData = ChunkControl.Instance.TryLoadForcedChunkData_(file_path);
		if (forcedChunkData.forced_biome == -1)
		{
			if (forcedChunkData.has_any_data && zone == "overworld")
			{
				biome = ChunkControl.Instance.RemoveUnderlyingWaterForDevPlacedScenics_(biome, file_path);
			}
		}
		else
		{
			biome = forcedChunkData.forced_biome;
			ChunkControl.Instance.AssignRandomBiomeMobs(biome, ref biome_mobA, ref biome_mobB);
		}
		if (forcedChunkData.forced_floor_model != -1)
		{
			floor_model_id = forcedChunkData.forced_floor_model;
		}
		if (forcedChunkData.forced_floor_rot != -1)
		{
			floor_rotation = forcedChunkData.forced_floor_rot;
		}
		if (forcedChunkData.caveart_index == -1)
		{
			if (forcedChunkData.has_any_data && InventoryUtils.IsCaveObject(ZoneDataControl.Instance.LoadZoneDataFromDisk(zone).house_item.item_name))
			{
				floor_texture_index = 0;
			}
		}
		else if (forcedChunkData.caveart_rot == 1)
		{
			floor_texture_index = forcedChunkData.caveart_index + 101;
		}
		else if (forcedChunkData.caveart_rot == 0)
		{
			floor_texture_index = forcedChunkData.caveart_index + 1;
		}
		if (forcedChunkData.is_blank)
		{
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					if (!chunk_elements.ContainsKey(i + "," + j))
					{
						continue;
					}
					new List<Vector2>();
					for (int num = chunk_elements[i + "," + j].Count - 1; num >= 0; num--)
					{
						if (chunk_elements[i + "," + j][num].item.GetString("tag") == "natural")
						{
							chunk_elements[i + "," + j].RemoveAt(num);
						}
					}
				}
			}
		}
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines(file_path, ref file_exists);
		if (!file_exists)
		{
			return;
		}
		string text = "";
		ExtraInventoryData extraInventoryData = null;
		int original_inner_x = 0;
		int original_inner_z = 0;
		int original_rot = 0;
		foreach (string item in textFileLines)
		{
			if (Startup.StringNullOrWhitespace(item))
			{
				continue;
			}
			if (item.Contains("auto_version"))
			{
				quest_version = int.Parse(item.Substring(14, item.Length - 15), Startup.parse_culture);
				continue;
			}
			if (item[0] == '[')
			{
				if (text != "")
				{
					AddElementWithRotation(text, extraInventoryData, original_inner_x, original_inner_z, original_rot, new_chunk_rotation, on_bandit_camp, auto_built_zones, bandit_camp_instance_name, set_items_tag);
				}
				int num2 = item.IndexOf('=');
				int num3 = item.IndexOf("(rot");
				original_inner_x = int.Parse(item[1].ToString() ?? "", Startup.parse_culture);
				original_inner_z = int.Parse(item[3].ToString() ?? "", Startup.parse_culture);
				text = item.Substring(num2 + 2, num3 - num2 - 3);
				original_rot = int.Parse(item.Substring(num3 + 5, item.Length - num3 - 6), Startup.parse_culture);
				extraInventoryData = new ExtraInventoryData();
				continue;
			}
			int num4 = -1;
			int num5 = -1;
			for (int k = 0; k < item.Length; k++)
			{
				if (num4 == -1)
				{
					if (char.IsLetterOrDigit(item[k]))
					{
						num4 = k;
					}
				}
				else if (num5 == -1 && item[k] == '=')
				{
					num5 = k;
				}
			}
			string key = item.Substring(num4, num5 - num4 - 1);
			string text2 = item.Substring(num5 + 2, item.Length - (num5 + 2));
			if (text2.Contains("*string*"))
			{
				extraInventoryData.SetString(key, text2.Replace("*string* ", ""));
			}
			else if (text2.Contains("*short*"))
			{
				extraInventoryData.SetShort(key, short.Parse(text2.Replace("*short* ", ""), Startup.parse_culture));
			}
			else if (text2.Contains("*long*"))
			{
				extraInventoryData.SetLong(key, int.Parse(text2.Replace("*long* ", ""), Startup.parse_culture));
			}
		}
		if (text != "")
		{
			AddElementWithRotation(text, extraInventoryData, original_inner_x, original_inner_z, original_rot, new_chunk_rotation, on_bandit_camp, auto_built_zones, bandit_camp_instance_name, set_items_tag);
		}
	}

	private void AddElementWithRotation(string itemName, ExtraInventoryData extraData, int original_inner_x, int original_inner_z, int original_rot, int new_chunk_rotation, bool on_bandit_camp, Dictionary<string, ZoneData> auto_built_zones, string bandit_camp_instance_name, string set_items_tag)
	{
		int add_element_x;
		int add_element_z;
		int add_element_rot;
		switch (new_chunk_rotation)
		{
		case 1:
			add_element_x = 9 - original_inner_z;
			add_element_z = original_inner_x;
			add_element_rot = (original_rot + 3) % 4;
			break;
		case 2:
			add_element_x = 9 - original_inner_x;
			add_element_z = 9 - original_inner_z;
			add_element_rot = (original_rot + 2) % 4;
			break;
		case 3:
			add_element_x = original_inner_z;
			add_element_z = 9 - original_inner_x;
			add_element_rot = (original_rot + 1) % 4;
			break;
		default:
			add_element_x = original_inner_x;
			add_element_z = original_inner_z;
			add_element_rot = original_rot;
			break;
		}
		AdjustThenTryAddDevBuilable(itemName, extraData, add_element_x, add_element_z, add_element_rot, on_bandit_camp, auto_built_zones, bandit_camp_instance_name, set_items_tag);
	}

	private void AdjustThenTryAddDevBuilable(string add_element_item_name, ExtraInventoryData add_element_item_extra_data, int add_element_x, int add_element_z, int add_element_rot, bool on_bandit_camp, Dictionary<string, ZoneData> auto_built_zones, string bandit_camp_instance_name, string set_items_tag)
	{
		switch (add_element_item_name)
		{
		case "BIOME_SMALL_STALAGMITE":
		{
			BanditCampInstance banditCampInstanceByName4 = BanditCampsControl.Instance.GetBanditCampInstanceByName(bandit_camp_instance_name);
			add_element_item_name = ChunkControl.Instance.GetCorrespondingCave(inventory_ctr.BiomeIdToCaveEntrance(banditCampInstanceByName4.biome_id)).small_stalagmite_name;
			break;
		}
		case "BIOME_LARGE_STALAGMITE":
		{
			BanditCampInstance banditCampInstanceByName5 = BanditCampsControl.Instance.GetBanditCampInstanceByName(bandit_camp_instance_name);
			add_element_item_name = ChunkControl.Instance.GetCorrespondingCave(inventory_ctr.BiomeIdToCaveEntrance(banditCampInstanceByName5.biome_id)).stalagmite_name;
			break;
		}
		case "BIOME_2x2":
		{
			BanditCampInstance banditCampInstanceByName = BanditCampsControl.Instance.GetBanditCampInstanceByName(bandit_camp_instance_name);
			List<string> list = new List<string>();
			ChunkControl.biome_obj[] biome_scenic = ChunkControl.Instance.biomes[banditCampInstanceByName.biome_id].biome_scenic;
			for (int i = 0; i < biome_scenic.Length; i++)
			{
				string item_name = biome_scenic[i].item_name;
				if (!(item_name == "Red Blob") && !(item_name == "Blue Blob") && ConstructionControl.Instance.GetItemGeometry(item_name) == ConstructionControl.object_geometry._2_by_2)
				{
					list.Add(item_name);
				}
			}
			if (list.Count > 0)
			{
				add_element_item_name = list[UnityEngine.Random.Range(0, list.Count)];
			}
			break;
		}
		case "BIOME_1x1":
		{
			BanditCampInstance banditCampInstanceByName2 = BanditCampsControl.Instance.GetBanditCampInstanceByName(bandit_camp_instance_name);
			List<string> list2 = new List<string>();
			ChunkControl.biome_obj[] biome_scenic2 = ChunkControl.Instance.biomes[banditCampInstanceByName2.biome_id].biome_scenic;
			for (int j = 0; j < biome_scenic2.Length; j++)
			{
				string item_name2 = biome_scenic2[j].item_name;
				if (ConstructionControl.Instance.GetItemGeometry(item_name2) == ConstructionControl.object_geometry._1_by_1)
				{
					list2.Add(item_name2);
				}
			}
			if (list2.Count > 0)
			{
				add_element_item_name = list2[UnityEngine.Random.Range(0, list2.Count)];
			}
			break;
		}
		case "RANDOM_BIOME_GEM":
		{
			BanditCampInstance banditCampInstanceByName3 = BanditCampsControl.Instance.GetBanditCampInstanceByName(bandit_camp_instance_name);
			if (banditCampInstanceByName3 != null)
			{
				add_element_item_name = banditCampInstanceByName3.GetRandomizedGem();
			}
			break;
		}
		case "RANDOM_BIOME_GEM_LARGE":
		{
			BanditCampInstance banditCampInstanceByName6 = BanditCampsControl.Instance.GetBanditCampInstanceByName(bandit_camp_instance_name);
			if (banditCampInstanceByName6 != null)
			{
				add_element_item_name = banditCampInstanceByName6.GetRandomizedGemLarge();
			}
			break;
		}
		case "BIOME_CAVE_ENTRANCE":
		{
			BanditCampInstance banditCampInstanceByName7 = BanditCampsControl.Instance.GetBanditCampInstanceByName(bandit_camp_instance_name);
			if (banditCampInstanceByName7 != null)
			{
				add_element_item_name = inventory_ctr.BiomeIdToCaveEntrance(banditCampInstanceByName7.biome_id);
			}
			break;
		}
		case "DEBUG-npc":
		{
			Dictionary<string, object> dataForCopying = add_element_item_extra_data.GetDataForCopying();
			if (dataForCopying.ContainsKey("npc_file"))
			{
				string value = (string)dataForCopying["npc_file"];
				if (dataForCopying.ContainsKey("npc_is_free_follower") && (string)dataForCopying["npc_is_free_follower"] == "true")
				{
					string randomCreature = CreatureMorpher.Instance.GetRandomCreature();
					string randomCreature2 = CreatureMorpher.Instance.GetRandomCreature();
					value = DevBuildControl.Instance.GetRandomFreeFollowerName();
					add_element_item_extra_data.SetString("creature_A", randomCreature);
					add_element_item_extra_data.SetString("creature_B", randomCreature2);
				}
				add_element_item_extra_data.SetString("npc_display_name", value);
			}
			break;
		}
		}
		add_element_item_extra_data.SetString("tag", set_items_tag);
		InventoryItem item;
		if (!on_bandit_camp)
		{
			item = new InventoryItem(add_element_item_name, add_element_item_extra_data);
		}
		else
		{
			Dictionary<string, object> dataForCopying2 = add_element_item_extra_data.GetDataForCopying();
			add_element_item_extra_data.SetString("bandit_camp_instance", bandit_camp_instance_name);
			string text = "";
			string final_paint = "";
			if (dataForCopying2.ContainsKey("paint"))
			{
				text = (string)dataForCopying2["paint"];
				final_paint = text;
			}
			if (text != "")
			{
				BanditCampsControl.Instance.ModifyIfBanditPaint(ref final_paint, add_element_item_name, bandit_camp_instance_name);
				if (final_paint != text)
				{
					add_element_item_extra_data.SetString("paint", final_paint);
				}
			}
			if (InventoryUtils.UsesBasketId(new InventoryItem(add_element_item_name)) && DevBuildControl.Instance.debug_bandit_camp_data == null)
			{
				if (dataForCopying2.ContainsKey("basket_id"))
				{
					_ = dataForCopying2["basket_id"];
				}
				add_element_item_extra_data.SetLong("basket_id", ConstructionControl.Instance.GetNewUniqueId());
				item = new InventoryItem(add_element_item_name, add_element_item_extra_data);
			}
			else if (!InventoryUtils.UsesShackId(add_element_item_name) || DevBuildControl.Instance.debug_bandit_camp_data != null)
			{
				item = new InventoryItem(add_element_item_name, add_element_item_extra_data);
			}
			else
			{
				int value2 = -1;
				if (dataForCopying2.ContainsKey("shack_id"))
				{
					value2 = (int)dataForCopying2["shack_id"];
				}
				int newUniqueId = ConstructionControl.Instance.GetNewUniqueId();
				add_element_item_extra_data.SetLong("shack_id", newUniqueId);
				add_element_item_extra_data.SetShort("outer_item_chunkX", X);
				add_element_item_extra_data.SetShort("outer_item_chunkZ", Z);
				add_element_item_extra_data.SetShort("outer_item_innerX", add_element_x);
				add_element_item_extra_data.SetShort("outer_item_innerZ", add_element_z);
				add_element_item_extra_data.SetLong("bandit_camp_original_shack_id", value2);
				item = new InventoryItem(add_element_item_name, add_element_item_extra_data);
				BanditCampInstance banditCampInstanceByName8 = BanditCampsControl.Instance.GetBanditCampInstanceByName(bandit_camp_instance_name);
				InventoryItem house_item = new InventoryItem("");
				string outer_item_zone = "";
				int interior_model_chunkX = 0;
				int interior_model_chunkZ = 0;
				int interior_model_innerX = 0;
				int interior_model_innerZ = 0;
				int outer_item_rot = 0;
				ZoneData.GetNpcHomeData(System.IO.Path.Combine("bandit-camps/" + banditCampInstanceByName8.template, "(Auto Gen) npc_home_data"), "shack" + value2, ref house_item, ref interior_model_chunkX, ref interior_model_chunkZ, ref interior_model_innerX, ref interior_model_innerZ, ref outer_item_rot, ref outer_item_zone);
				ZoneData value3 = new ZoneData("shack" + newUniqueId, item, add_element_rot, zone, X, Z, interior_model_innerX, interior_model_innerZ);
				auto_built_zones.Add("shack" + newUniqueId, value3);
			}
		}
		AddElement(add_element_x, add_element_z, new ChunkElement(item, add_element_rot));
	}

	public void SaveWholeChunkToDisk(string chunkStr, SingleFile file = null)
	{
		if (file == null)
		{
			file = PlayerData.Instance.GetSlotFilesGroup(-1).GetFile(ChunkControl.GetChunkFilename(chunkStr));
		}
		file.ClearSegment(chunkStr);
		file.SetShort("biome", biome, chunkStr);
		file.SetString("biome-mob-a", biome_mobA, chunkStr);
		file.SetString("biome-mob-b", biome_mobB, chunkStr);
		file.SetShort("floor-model-id", floor_model_id, chunkStr);
		file.SetShort("floor-tex-index", floor_texture_index, chunkStr);
		file.SetShort("floor-rot", floor_rotation, chunkStr);
		file.SetShort("quest-version", quest_version, chunkStr);
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				if (!chunk_elements.ContainsKey(i + "," + j))
				{
					continue;
				}
				List<ChunkElement> list = chunk_elements[i + "," + j];
				file.SetShort(i + "," + j + "_numObjects", list.Count, chunkStr);
				int num = 0;
				for (int k = 0; k < list.Count; k++)
				{
					ChunkElement chunkElement = list[k];
					string text = i + "," + j + "," + num;
					file.SetShort(text + "_rot", chunkElement.rot, chunkStr);
					chunkElement.item.SaveToFile(file, text, chunkStr);
					num++;
				}
			}
		}
		land_claim_chunk_timers_changed = true;
		SaveLandClaimChunkTimersToDisk(chunkStr, file);
		file.SetShort("exists", 1, chunkStr);
		file.SetShort("version", ChunkControl.curr_chunk_version, chunkStr);
	}

	public void SaveLandClaimChunkTimersToDisk(string chunkStr, SingleFile file = null)
	{
		if (!land_claim_chunk_timers_changed)
		{
			return;
		}
		if (file == null)
		{
			file = PlayerData.Instance.GetSlotFilesGroup(-1).GetFile(ChunkControl.GetChunkFilename(chunkStr));
		}
		file.SetShort("n_land_claim_chunk_respawns", land_claim_chunk_timers_.Count, chunkStr);
		int num = 0;
		foreach (KeyValuePair<string, LandClaimChunkTimer> item in land_claim_chunk_timers_)
		{
			file.SetString("land_claim_chunk_respawn_" + num + "_str", item.Value.land_claim_str, chunkStr);
			file.SetString("land_claim_chunk_respawn_" + num + "_user0", item.Value.land_claim_user0, chunkStr);
			file.SetString("land_claim_chunk_respawn_" + num + "_user1", item.Value.land_claim_user1, chunkStr);
			file.SetString("land_claim_chunk_respawn_" + num + "_user2", item.Value.land_claim_user2, chunkStr);
			file.SetShort("land_claim_chunk_respawn_" + num + "_second", item.Value.when_to_expire.Second, chunkStr);
			file.SetShort("land_claim_chunk_respawn_" + num + "_minute", item.Value.when_to_expire.Minute, chunkStr);
			file.SetShort("land_claim_chunk_respawn_" + num + "_hour", item.Value.when_to_expire.Hour, chunkStr);
			file.SetShort("land_claim_chunk_respawn_" + num + "_day", item.Value.when_to_expire.Day, chunkStr);
			file.SetShort("land_claim_chunk_respawn_" + num + "_month", item.Value.when_to_expire.Month, chunkStr);
			file.SetShort("land_claim_chunk_respawn_" + num + "_year", item.Value.when_to_expire.Year, chunkStr);
			num++;
		}
	}

	public static List<ChunkElement> LoadElementsAtTile(int x, int z, string chunkStr)
	{
		List<ChunkElement> list = new List<ChunkElement>();
		string chunkFilename = ChunkControl.GetChunkFilename(chunkStr);
		short slotShort = PlayerData.Instance.GetSlotShort(x + "," + z + "_numObjects", chunkFilename, chunkStr);
		for (int i = 0; i < slotShort; i++)
		{
			string text = x + "," + z + "," + i;
			short slotShort2 = PlayerData.Instance.GetSlotShort(text + "_rot", chunkFilename, chunkStr);
			list.Add(new ChunkElement(InventoryItem.LoadFromDisk(text, chunkFilename, chunkStr), slotShort2));
		}
		return list;
	}

	public void TryAutoBuildAt(string item_name, int modX, int modZ, int rot, int origin_chunkX, int origin_chunkZ, int origin_innerX, int origin_innerZ, string zone, Dictionary<string, ZoneData> auto_built_zones)
	{
	}

	public void RemoveAllElementsWithTag(string remove_tag)
	{
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				foreach (ChunkElement item in GetElementsAt(i, j))
				{
					if (item.item.GetString("tag") == remove_tag)
					{
						RemoveElement(i, j, item);
					}
				}
			}
		}
	}

	public static void LoadFromDisk(ChunkData chunk_data, string zone, int X, int Z)
	{
		string chunkString = ChunkControl.Instance.GetChunkString(zone, X, Z);
		string chunkFilename = ChunkControl.GetChunkFilename(chunkString);
		chunk_data.biome = PlayerData.Instance.GetSlotShort("biome", chunkFilename, chunkString);
		chunk_data.floor_model_id = PlayerData.Instance.GetSlotShort("floor-model-id", chunkFilename, chunkString);
		chunk_data.floor_texture_index = PlayerData.Instance.GetSlotShort("floor-tex-index", chunkFilename, chunkString);
		chunk_data.floor_rotation = PlayerData.Instance.GetSlotShort("floor-rot", chunkFilename, chunkString);
		chunk_data.biome_mobA = PlayerData.Instance.GetSlotString("biome-mob-a", chunkFilename, chunkString);
		chunk_data.biome_mobB = PlayerData.Instance.GetSlotString("biome-mob-b", chunkFilename, chunkString);
		chunk_data.quest_version = PlayerData.Instance.GetSlotShort("quest-version", chunkFilename, chunkString);
		PlayerData.Instance.GetSlotShort("version", chunkFilename, chunkString);
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				foreach (ChunkElement item in LoadElementsAtTile(i, j, chunkString))
				{
					chunk_data.AddElement(i, j, item);
				}
			}
		}
	}

	public void ProcessAllItemChanges(ref bool chunk_was_changed)
	{
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				foreach (ChunkElement item in GetElementsAt(i, j))
				{
					bool item_was_changed = false;
					InventoryItem inventoryItem = ChunkControl.Instance.ProcessItemChange(item.item, ref item_was_changed);
					if (item_was_changed)
					{
						item.item = inventoryItem;
						chunk_was_changed = true;
					}
				}
			}
		}
	}

	public void LoadLandClaimTimersFromDisk(string zone, int X, int Z)
	{
		string chunkString = ChunkControl.Instance.GetChunkString(zone, X, Z);
		string chunkFilename = ChunkControl.GetChunkFilename(chunkString);
		short slotShort = PlayerData.Instance.GetSlotShort("n_land_claim_chunk_respawns", chunkFilename, chunkString);
		for (int i = 0; i < slotShort; i++)
		{
			string slotString = PlayerData.Instance.GetSlotString("land_claim_chunk_respawn_" + i + "_str", chunkFilename, chunkString);
			string slotString2 = PlayerData.Instance.GetSlotString("land_claim_chunk_respawn_" + i + "_user0", chunkFilename, chunkString);
			string slotString3 = PlayerData.Instance.GetSlotString("land_claim_chunk_respawn_" + i + "_user1", chunkFilename, chunkString);
			string slotString4 = PlayerData.Instance.GetSlotString("land_claim_chunk_respawn_" + i + "_user2", chunkFilename, chunkString);
			short slotShort2 = PlayerData.Instance.GetSlotShort("land_claim_chunk_respawn_" + i + "_second", chunkFilename, chunkString);
			short slotShort3 = PlayerData.Instance.GetSlotShort("land_claim_chunk_respawn_" + i + "_minute", chunkFilename, chunkString);
			short slotShort4 = PlayerData.Instance.GetSlotShort("land_claim_chunk_respawn_" + i + "_hour", chunkFilename, chunkString);
			short slotShort5 = PlayerData.Instance.GetSlotShort("land_claim_chunk_respawn_" + i + "_day", chunkFilename, chunkString);
			short slotShort6 = PlayerData.Instance.GetSlotShort("land_claim_chunk_respawn_" + i + "_month", chunkFilename, chunkString);
			short slotShort7 = PlayerData.Instance.GetSlotShort("land_claim_chunk_respawn_" + i + "_year", chunkFilename, chunkString);
			if (slotShort3 == 0 && slotShort2 == 0 && slotShort4 == 0 && slotShort5 == 0 && slotShort6 == 0 && slotShort7 == 0)
			{
				land_claim_chunk_timers_changed = true;
				continue;
			}
			DateTime dateTime = new DateTime(slotShort7, slotShort6, slotShort5, slotShort4, slotShort3, slotShort2);
			if ((dateTime - DateTime.UtcNow).TotalSeconds <= 0.0)
			{
				land_claim_chunk_timers_changed = true;
			}
			else
			{
				AddLandClaimChunkTimer(slotString, slotString2, slotString3, slotString4, chunkString, dateTime, true);
			}
		}
	}

	public static LandClaimChunkTimer CreateLandClaimChunkTimer(string land_claim_str, string user0, string user1, string user2, DateTime when_to_respawn)
	{
		return new LandClaimChunkTimer
		{
			land_claim_str = land_claim_str,
			land_claim_user0 = user0,
			land_claim_user1 = user1,
			land_claim_user2 = user2,
			when_to_expire = when_to_respawn
		};
	}

	public void AddLandClaimChunkTimer(string land_claim_str, string user0, string user1, string user2, string chunkStr, DateTime when_to_respawn, bool on_load)
	{
		if (!on_load)
		{
			land_claim_chunk_timers_changed = true;
		}
		LandClaimChunkTimer value = CreateLandClaimChunkTimer(land_claim_str, user0, user1, user2, when_to_respawn);
		if (land_claim_chunk_timers_.ContainsKey(land_claim_str))
		{
			land_claim_chunk_timers_[land_claim_str] = value;
		}
		else
		{
			land_claim_chunk_timers_.Add(land_claim_str, value);
		}
	}

	public static LandClaimChunkTimer CloneLandClaimChunkTimer(LandClaimChunkTimer original)
	{
		return null;
	}

	public void ModifyLandClaimTimer(string land_claim_str, int user_index, string new_username)
	{
	}

	public void RemoveLandClaimChunkTimers(string land_claim_str)
	{
	}

	public void PackForWeb(Packet outgoing)
	{
	}

	public List<string> DetermineBanditCampsWithinChunk(InventoryItem zone_item)
	{
		return null;
	}

	public Dictionary<string, LandClaimChunkTimer> GetAllLandClaimTimers()
	{
		return null;
	}

	public void UnpackFromWeb(Packet incoming)
	{
	}
}
