using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class BanditCampsControl : MonoBehaviour, OrderedStart
{
	public static BanditCampsControl Instance;

	public Dictionary<string, BanditCampMap> loaded_bandit_camp_maps = new Dictionary<string, BanditCampMap>();

	public Dictionary<string, BanditCampInstance> loaded_bandit_camp_instances = new Dictionary<string, BanditCampInstance>();

	public Dictionary<string, Dictionary<string, string>> bandit_camp_paints = new Dictionary<string, Dictionary<string, string>>();

	private Dictionary<string, FactionMobsFile> faction_mobs_files = new Dictionary<string, FactionMobsFile>();

	private Dictionary<string, FactionLoreFile> faction_lore_files = new Dictionary<string, FactionLoreFile>();

	private Dictionary<string, FactionBossNameFile> faction_boss_possible_names = new Dictionary<string, FactionBossNameFile>();

	public Dictionary<string, BanditCampTemplate> Templates { get; private set; }

	public void Start_0()
	{
		Instance = this;
		LoadBanditCampPaints();
		LoadBanditCampTemplates();
	}

	public void Start_1()
	{
	}

	private void LoadBanditCampTemplates()
	{
		Templates = new Dictionary<string, BanditCampTemplate>();
		BanditCampTemplate banditCampTemplate = new BanditCampTemplate
		{
			Name = "BanditMines",
			Width = 3,
			Height = 3
		};
		Templates.Add(banditCampTemplate.Name, banditCampTemplate);
	}

	public string GetUniqueBanditCampInstanceName()
	{
		int slotLong = PlayerData.Instance.GetSlotLong("bandit_camp_id_iterator", PlayerData.filename_t.general);
		PlayerData.Instance.SetSlotLong("bandit_camp_id_iterator", slotLong + 1, PlayerData.filename_t.general);
		return "BanditCampInstance" + slotLong;
	}

	public BanditCampMap.bandit_camp_instance_info_request_response GetSimpleBanditCampInfo(int chunkX, int chunkZ)
	{
		string biomeString = ChunkControl.Instance.GetBiomeString(chunkX, chunkZ);
		int biomeInnerX = ChunkControl.Instance.GetBiomeInnerX(chunkX);
		int biomeInnerZ = ChunkControl.Instance.GetBiomeInnerZ(chunkZ);
		if (!loaded_bandit_camp_maps.ContainsKey(biomeString))
		{
			BanditCampMap value;
			if (PlayerData.Instance.GetSlotShort("exists", "bandit-camp-map(" + biomeString + ")") == 1)
			{
				value = BanditCampMap.LoadFromDisk(biomeString);
			}
			else
			{
				int biome_X = 0;
				int biome_Z = 0;
				ChunkControl.Instance.GetBiomeMapCoordinates(chunkX, chunkZ, ref biome_X, ref biome_Z);
				value = BanditCampMap.GenerateNewBanditCampMap(26, biome_X, biome_Z);
				value.SaveToDisk(biomeString);
			}
			loaded_bandit_camp_maps.Add(biomeString, value);
		}
		return loaded_bandit_camp_maps[biomeString].GetInstanceInfoAt(biomeInnerX, biomeInnerZ);
	}

	public void PopulateOverworldChunk(ChunkData chunk_data, int X, int Z, string zone, Dictionary<string, ZoneData> auto_built_zones, BanditCampMap.bandit_camp_instance_info_request_response info)
	{
		BanditCampInstance banditCampInstance = GetBanditCampInstanceByCoordinates(zone, X, Z);
		if (banditCampInstance == null)
		{
			float instance_depth = GameController.Instance.DepthAt(new Vector3((float)X * 10f + 5f, 0f, (float)Z * 10f + 5f));
			banditCampInstance = CreateBanditCampInstance(zone, X, Z, info.instance_name, info.instance_template, info.instance_rot, instance_depth);
		}
		int biome_id = banditCampInstance.biome_id;
		string[] floor_texture_paths = ChunkControl.Instance.biomes[biome_id].floor_texture_paths;
		chunk_data.biome = biome_id;
		ChunkControl.Instance.AssignRandomBiomeMobs(biome_id, ref chunk_data.biome_mobA, ref chunk_data.biome_mobB);
		chunk_data.floor_rotation = Random.Range(0, 4);
		chunk_data.floor_texture_index = Random.Range(0, floor_texture_paths.Length);
		chunk_data.InjectDevPlacedBuildables("bandit-camps/" + banditCampInstance.template + "/" + info.instance_template_specific_file, "", banditCampInstance.instance_rot, true, auto_built_zones, banditCampInstance.instance_name);
	}

	public void PopulateIndoorsChunk(ChunkData chunk_data, int X, int Z, string zone, Dictionary<string, ZoneData> auto_built_zones, string bandit_camp_instance_name, int bandit_camp_original_shack_id, int curr_shack_originChunkX, int curr_shack_originChunkZ)
	{
		BanditCampInstance banditCampInstanceByName = GetBanditCampInstanceByName(bandit_camp_instance_name);
		InventoryItem house_item = new InventoryItem("");
		int interior_model_chunkX = 0;
		int interior_model_chunkZ = 0;
		int interior_model_innerX = 0;
		int interior_model_innerZ = 0;
		int outer_item_rot = 0;
		string outer_item_zone = "";
		ZoneData.GetNpcHomeData(Path.Combine("bandit-camps/" + banditCampInstanceByName.template, "(Auto Gen) npc_home_data"), "shack" + bandit_camp_original_shack_id, ref house_item, ref interior_model_chunkX, ref interior_model_chunkZ, ref interior_model_innerX, ref interior_model_innerZ, ref outer_item_rot, ref outer_item_zone);
		int num = X - curr_shack_originChunkX + interior_model_chunkX;
		int num2 = Z - curr_shack_originChunkZ + interior_model_chunkZ;
		string text = "chunk(shack" + bandit_camp_original_shack_id + "," + num + ", " + num2 + ")";
		chunk_data.InjectDevPlacedBuildables("bandit-camps/" + banditCampInstanceByName.template + "/" + text, "", banditCampInstanceByName.instance_rot, true, auto_built_zones, bandit_camp_instance_name);
	}

	public BanditCampInstance GetBanditCampInstanceByName(string instance_name)
	{
		if (DevBuildControl.Instance.debug_bandit_camp_data != null)
		{
			return DevBuildControl.Instance.debug_bandit_camp_data;
		}
		if (loaded_bandit_camp_instances.ContainsKey(instance_name))
		{
			return loaded_bandit_camp_instances[instance_name];
		}
		if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
		{
			return null;
		}
		string filename = "bandit-camp-instance(" + instance_name + ")";
		BanditCampInstance banditCampInstance = null;
		if (PlayerData.Instance.GetSlotShort("exists", filename) == 1)
		{
			banditCampInstance = BanditCampInstance.LoadFromDisk(instance_name);
			loaded_bandit_camp_instances.Add(instance_name, banditCampInstance);
		}
		return banditCampInstance;
	}

	public BanditCampInstance GetBanditCampInstanceByCoordinates(string zone, int chunkX, int chunkZ)
	{
		BanditCampMap.bandit_camp_instance_info_request_response simpleBanditCampInfo = GetSimpleBanditCampInfo(chunkX, chunkZ);
		if (string.IsNullOrEmpty(simpleBanditCampInfo.instance_name))
		{
			return null;
		}
		return GetBanditCampInstanceByName(simpleBanditCampInfo.instance_name);
	}

	public BanditCampInstance CreateBanditCampInstance(string zone, int chunkX, int chunkZ, string instance_name, string instance_template, int instance_rot, float instance_depth)
	{
		ForcedChunkData forcedChunkData = ChunkControl.Instance.TryLoadForcedChunkData_(ChunkControl.Instance.GetForcedChunkDataFilePath(ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ)));
		int num = forcedChunkData.forced_biome;
		if (num == -1)
		{
			num = ChunkControl.Instance.GetBiomeIdAt(chunkX, chunkZ);
		}
		BanditCampInstance banditCampInstance = new BanditCampInstance(instance_name, num, instance_template, instance_rot, instance_depth, flag_destroyed: false, is_debug_data: false);
		PlayerData.Instance.SetSlotShort("exists", 1, "bandit-camp-instance(" + instance_name + ")");
		banditCampInstance.SaveToDisk();
		loaded_bandit_camp_instances.Add(instance_name, banditCampInstance);
		return banditCampInstance;
	}

	public void ModifyIfBanditPaint(ref string final_paint, string item_name, string bandit_camp_instance_name)
	{
		if (!Instance.bandit_camp_paints.ContainsKey(final_paint) || !(bandit_camp_instance_name != ""))
		{
			return;
		}
		BanditCampInstance banditCampInstanceByName = GetBanditCampInstanceByName(bandit_camp_instance_name);
		if (banditCampInstanceByName != null)
		{
			string key = DevBuildControl.BiomeIdToBiomeString(banditCampInstanceByName.biome_id);
			string text = "";
			if (Instance.bandit_camp_paints[final_paint].ContainsKey(key))
			{
				text = Instance.bandit_camp_paints[final_paint][key];
			}
			if (!Startup.StringNullOrWhitespace(text))
			{
				final_paint = text;
				return;
			}
		}
		string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item_name, "Default Coloring");
		if (!Startup.StringNullOrWhitespace(stringFromItemFile))
		{
			final_paint = stringFromItemFile;
		}
		else
		{
			final_paint = "";
		}
	}

	public void LoadBanditCampPaints()
	{
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("bandit_camp_paints", ref file_exists);
		if (file_exists)
		{
			bandit_camp_paints = ParseBanditCampPaintsFile(textFileLines);
		}
	}

	private static Dictionary<string, Dictionary<string, string>> ParseBanditCampPaintsFile(List<string> lines)
	{
		Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>(System.StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> dictionary2 = null;
		foreach (string line in lines)
		{
			string text = line.Trim();
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			if (text.StartsWith("[") && text.EndsWith("]"))
			{
				string text2 = text.Substring(1, text.Length - 2).Trim();
				dictionary2 = null;
				if (!string.IsNullOrWhiteSpace(text2))
				{
					dictionary2 = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
					dictionary[text2] = dictionary2;
				}
			}
			else if (dictionary2 != null)
			{
				int num = text.IndexOf('=');
				if (num != -1)
				{
					string key = text.Substring(0, num).Trim();
					string value = text.Substring(num + 1).Trim();
					dictionary2[key] = value;
				}
			}
		}
		return dictionary;
	}

	public string GetMobFromFaction(string suffix, int biome_id)
	{
		string key = "BANDIT_FACTION";
		string key2 = "";
		if (suffix.Contains("Weak"))
		{
			key2 = "weak";
		}
		else if (suffix.Contains("Stronger"))
		{
			key2 = "stronger";
		}
		else if (suffix.Contains("Boss"))
		{
			key2 = "boss";
		}
		if (!faction_mobs_files.ContainsKey(key))
		{
			faction_mobs_files.Add(key, LoadBanditMobList(key));
		}
		return faction_mobs_files[key].entries[DevBuildControl.BiomeIdToBiomeString(biome_id)][key2];
	}

	private FactionMobsFile LoadBanditMobList(string faction)
	{
		bool file_exists = false;
		Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();
		file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("FactionData/" + faction + "/" + faction + "_mobs_", ref file_exists);
		if (file_exists)
		{
			string text = null;
			foreach (string item in textFileLines)
			{
				if (item.StartsWith("[") && item.EndsWith("]"))
				{
					text = item.Trim('[', ']');
					if (!dictionary.ContainsKey(text))
					{
						dictionary[text] = new Dictionary<string, string>();
					}
				}
				else if (text != null && !string.IsNullOrWhiteSpace(item))
				{
					string[] array = item.Split('=');
					if (array.Length == 2)
					{
						string key = array[0].Trim();
						string value = array[1].Trim();
						dictionary[text][key] = value;
					}
				}
			}
		}
		return new FactionMobsFile
		{
			entries = dictionary
		};
	}

	public string GetRandomLoreFromFaction(int biome_id)
	{
		string key = "BANDIT_FACTION";
		if (!faction_lore_files.ContainsKey(key))
		{
			faction_lore_files.Add(key, LoadBanditLoreList(key));
		}
		List<string> list = faction_lore_files[key].entries[DevBuildControl.BiomeIdToBiomeString(biome_id)];
		if (list.Count != 0)
		{
			return list[Random.Range(0, list.Count)];
		}
		return "";
	}

	private FactionLoreFile LoadBanditLoreList(string faction)
	{
		bool file_exists = false;
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("FactionData/" + faction + "/" + faction + "_lore", ref file_exists);
		if (file_exists)
		{
			string text = null;
			foreach (string item in textFileLines)
			{
				if (item.StartsWith("[") && item.EndsWith("]"))
				{
					text = item.Trim('[', ']');
					if (!dictionary.ContainsKey(text))
					{
						dictionary[text] = new List<string>();
					}
				}
				else if (text != null && !string.IsNullOrWhiteSpace(item))
				{
					dictionary[text].Add(item);
				}
			}
		}
		return new FactionLoreFile
		{
			entries = dictionary
		};
	}

	public string GetRandomBossNameFromFaction(int biome_id)
	{
		string key = "BANDIT_FACTION";
		if (!faction_boss_possible_names.ContainsKey(key))
		{
			faction_boss_possible_names.Add(key, LoadBanditBossNameList(key));
		}
		List<string> list = faction_boss_possible_names[key].entries[DevBuildControl.BiomeIdToBiomeString(biome_id)];
		return list[Random.Range(0, list.Count)];
	}

	private FactionBossNameFile LoadBanditBossNameList(string faction)
	{
		bool file_exists = false;
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("FactionData/" + faction + "/" + faction + "_boss_names", ref file_exists);
		if (file_exists)
		{
			string text = null;
			foreach (string item in textFileLines)
			{
				if (item.StartsWith("[") && item.EndsWith("]"))
				{
					text = item.Trim('[', ']');
					if (!dictionary.ContainsKey(text))
					{
						dictionary[text] = new List<string>();
					}
				}
				else if (text != null && !string.IsNullOrWhiteSpace(item))
				{
					dictionary[text].Add(item);
				}
			}
		}
		return new FactionBossNameFile
		{
			entries = dictionary
		};
	}
}
