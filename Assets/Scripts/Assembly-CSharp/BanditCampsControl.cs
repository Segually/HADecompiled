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
		return null;
	}

	public BanditCampMap.bandit_camp_instance_info_request_response GetSimpleBanditCampInfo(int chunkX, int chunkZ)
	{
		return default(BanditCampMap.bandit_camp_instance_info_request_response);
	}

	public void PopulateOverworldChunk(ChunkData chunk_data, int X, int Z, string zone, Dictionary<string, ZoneData> auto_built_zones, BanditCampMap.bandit_camp_instance_info_request_response info)
	{
	}

	public void PopulateIndoorsChunk(ChunkData chunk_data, int X, int Z, string zone, Dictionary<string, ZoneData> auto_built_zones, string bandit_camp_instance_name, int bandit_camp_original_shack_id, int curr_shack_originChunkX, int curr_shack_originChunkZ)
	{
	}

	public BanditCampInstance GetBanditCampInstanceByName(string instance_name)
	{
		return null;
	}

	public BanditCampInstance GetBanditCampInstanceByCoordinates(string zone, int chunkX, int chunkZ)
	{
		return null;
	}

	public BanditCampInstance CreateBanditCampInstance(string zone, int chunkX, int chunkZ, string instance_name, string instance_template, int instance_rot, float instance_depth)
	{
		return null;
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
		return null;
	}

	private FactionMobsFile LoadBanditMobList(string faction)
	{
		return null;
	}

	public string GetRandomLoreFromFaction(int biome_id)
	{
		return null;
	}

	private FactionLoreFile LoadBanditLoreList(string faction)
	{
		return null;
	}

	public string GetRandomBossNameFromFaction(int biome_id)
	{
		return null;
	}

	private FactionBossNameFile LoadBanditBossNameList(string faction)
	{
		return null;
	}
}
