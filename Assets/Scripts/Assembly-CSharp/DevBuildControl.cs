using System;
using System.Collections.Generic;
using UnityEngine;

public class DevBuildControl : MonoBehaviour, OrderedStart
{
	[Serializable]
	public struct NPC_musicbox
	{
		public string name;

		public int unique_id;

		public string song_filename;

		public string sale_name;

		public Color sale_col;
	}

	[Serializable]
	public struct NPC_painting
	{
		public string name;

		public string creator;

		public string desc;

		public short speed;

		public string painting_filename;
	}

	private enum enter_shortcut_function
	{
		none = 0,
		accept_object = 1,
		close_tool_mode = 2
	}

	public static DevBuildControl Instance;

	public Sprite lockedhome_notif;

	public Sprite spr_teleport_unknown;

	public Sprite quest_updated_ico;

	public Sprite[] overhead_logos;

	public string[] random_freeFollower_names;

	public NPC_musicbox[] NPC_musicboxes;

	public NPC_painting[] NPC_paintings;

	public bool disable_movement;

	private bool is_placing_items;

	public string curr_place_item;

	private int place_rot;

	private GameObject dev_mouse_obj;

	private string mod_chunkStr;

	private string mod_main_str;

	private int mod_chunkX;

	private int mod_chunkZ;

	public bool all_doors_unlocked;

	public BanditCampInstance debug_bandit_camp_data;

	public string debug_bandit_camp_template = "";

	public int debug_bandit_camp_W = -1;

	public int debug_bandit_camp_H = -1;

	private int view_rot;

	private List<string> recently_used = new List<string>();

	public bool view_dev_objects;

	private string curr_selected_tool = "";

	private enter_shortcut_function curr_shortcut_function;

	public static string quest_scenics_folder_
	{
		get
		{
			if (Instance != null && Instance.debug_bandit_camp_data != null)
			{
				return Instance.debug_bandit_camp_template;
			}
			return "quest-buildables";
		}
	}

	private void ShowEditorDialogue(string header, string body)
	{
	}

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
	}

	public void PressClearPaintsFromMemory()
	{
	}

	public static string BiomeIdToBiomeString(int biome_id)
	{
		switch (biome_id)
		{
		case 0:
			return "grasslands";
		case 1:
			return "snow";
		case 2:
			return "desert";
		case 3:
			return "evergreen";
		case 4:
		case 5:
			return "ocean";
		case 6:
		case 7:
			return "swamp";
		case 8:
			return "woodlands";
		case 9:
			return "sakura";
		default:
			return "";
		}
	}

	public bool IsDevPlacedShackZone(string zone)
	{
		if (zone == "overworld")
		{
			return false;
		}
		int num = int.Parse(zone.Replace("shack", ""), Startup.parse_culture);
		if (num >= ChunkControl.dedicated_quest_range_start && num < ChunkControl.dedicated_quest_range_start + 1000)
		{
			return true;
		}
		return false;
	}

	public bool IsLockedByDev(InventoryItem item)
	{
		if (all_doors_unlocked)
		{
			return false;
		}
		return item.GetString("password") == "DEV_LOCKED";
	}

	public string GetRandomFreeFollowerName()
	{
		return null;
	}

	public void PressBuildButton()
	{
	}

	public void PressBanditCampButton()
	{
	}

	public void PressPaintButton()
	{
	}

	public void PressStampButton()
	{
	}

	public void PressModifyDataButton()
	{
	}

	public void PressLockButton()
	{
	}

	public void PressVisionButton()
	{
	}

	public void PressRotateView()
	{
	}

	public void PressChangeBiome()
	{
	}

	public void SelectedBiome(int biome_id)
	{
	}

	public void PressCancelBiomeChange()
	{
	}

	public void PressRecentlyUsed(int index)
	{
	}

	private void ShowObjectSelectScreen(string header_str)
	{
	}

	public void PressCancelOnObjectSelectScreen()
	{
	}

	public void PressCancelOnBanditNewLoadScreen()
	{
	}

	public void PressOkayOnObjectSelectScreen()
	{
	}

	public void PressViewDevObjects()
	{
	}

	private void TryEnterBuildMode(string typed_obj)
	{
	}

	public void PressNewBanditCamp()
	{
	}

	public void PressLoadBanditCamp()
	{
	}

	private Dictionary<string, string> LoadBanditCampOverviewFile(string filePath)
	{
		return null;
	}

	public void StartBanditCampEditor(string bandit_camp_name)
	{
	}

	private void RefreshWorld(Vector3 position)
	{
	}

	public void ShowOrHideBaseDevElements(bool set_to)
	{
	}

	public void RedrawBanditButtons()
	{
	}

	private void CreateToolHeader(string main_str, string sub_str)
	{
	}

	private void CreateCancelButton(string cancel_str)
	{
	}

	public void PressCancelOnUsingTool()
	{
	}

	public void PressCancelOnModifyObjectDataScreen()
	{
	}

	public void PressSaveOnModifyObjectDataScreen()
	{
	}

	public void StartPlacingItems(string item_name)
	{
	}

	public void StopPlacingItems()
	{
	}

	private void ResetDevMouseObject()
	{
	}

	private int GetAvailableUniqueId()
	{
		return 0;
	}

	private void Update()
	{
	}

	private void FixedUpdate()
	{
	}

	public QuestBuildableFile LoadQuestBuildableFile(string path)
	{
		QuestBuildableFile file = new QuestBuildableFile();
		if (!System.IO.File.Exists(path)) return file;
		QuestBuildableEntry entry = null;
		foreach (string line in System.IO.File.ReadAllLines(path))
		{
			if (string.IsNullOrWhiteSpace(line)) continue;
			if (line[0] == '*')
			{
				if (line.Contains("*auto_version=")) file.auto_version_entry = line;
				else file.header_entries.Add(line);
			}
			else if (line[0] == '[')
			{
				if (entry != null) file.buildable_entries.Add(entry);
				entry = new QuestBuildableEntry(line);
			}
			else entry.extra_data.Add(line);
		}
		if (entry != null) file.buildable_entries.Add(entry);
		return file;
	}

	public void SaveQuestBuildableFile(QuestBuildableFile file, string save_path)
	{
		List<string> lines = new List<string>();
		if (file.auto_version_entry != "") lines.Add(file.auto_version_entry);
		foreach (string header in file.header_entries) lines.Add(header);
		lines.Add("");
		foreach (QuestBuildableEntry entry in file.buildable_entries)
		{
			lines.Add(entry.main_str);
			foreach (string data in entry.extra_data) lines.Add(data);
			lines.Add("");
		}
		System.IO.File.WriteAllLines(save_path, lines.ToArray());
	}
}
