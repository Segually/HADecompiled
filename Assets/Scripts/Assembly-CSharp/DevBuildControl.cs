using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
		// Added so the message shows up in the editor. In the shipped game this method is empty (the line below is how it looks there).
#if UNITY_EDITOR
		UnityEditor.EditorUtility.DisplayDialog(header, body, "OK");
#endif
		// (empty)
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
		PopupControl.Instance.SetButtonWasPressed();
		if (debug_bandit_camp_data != null)
		{
			BanditCampsControl.Instance.bandit_camp_paints.Clear();
			BanditCampsControl.Instance.LoadBanditCampPaints();
			MobControl.Instance.loaded_mob_files.Clear();
			Debug.Log("Memory Cleared!");
		}
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
		return random_freeFollower_names[UnityEngine.Random.Range(0, random_freeFollower_names.Length)];
	}

	public void PressBuildButton()
	{
		PopupControl.Instance.SetButtonWasPressed();
		curr_selected_tool = "build";
		ShowOrHideBaseDevElements(false);
		disable_movement = true;
		ShowObjectSelectScreen("Pick an Object to build");
	}

	public void PressBanditCampButton()
	{
		PopupControl.Instance.SetButtonWasPressed();
		ShowOrHideBaseDevElements(false);
		disable_movement = true;
		WindowPrefabsControl.Instance.CreateScreen("dev - bandit new load", WindowPrefabsControl.build_into_t.GAME_CTR);
	}

	public void PressPaintButton()
	{
		PopupControl.Instance.SetButtonWasPressed();
		curr_selected_tool = "paint";
		ShowOrHideBaseDevElements(false);
		disable_movement = true;
		ShowObjectSelectScreen("Pick a Paint");
	}

	public void PressStampButton()
	{
		PopupControl.Instance.SetButtonWasPressed();
		curr_selected_tool = "stamp";
		ShowOrHideBaseDevElements(false);
		disable_movement = true;
		ShowObjectSelectScreen("Pick a Stamp");
	}

	public void PressModifyDataButton()
	{
		PopupControl.Instance.SetButtonWasPressed();
		curr_selected_tool = "modify_data";
		ShowOrHideBaseDevElements(false);
		disable_movement = true;
		CreateToolHeader("Pick an object to modify", "");
		CreateCancelButton("CANCEL");
		StartPlacingItems("");
	}

	public void PressLockButton()
	{
		PopupControl.Instance.SetButtonWasPressed();
		curr_selected_tool = "lock";
		ShowOrHideBaseDevElements(false);
		disable_movement = true;
		CreateToolHeader("Pick an object to Lock", "");
		CreateCancelButton("CANCEL");
		StartPlacingItems("");
	}

	public void PressVisionButton()
	{
		PopupControl.Instance.SetButtonWasPressed();
		PerkData perkData = PerkControl.Instance.ClonePerkForCasting("perk_eagle_eye", GameController.Instance.player);
		perkData.all_effects["EFFECT_VISION"].data["Duration"] = "60 seconds";
		PerkControl.Instance.ApplyInitialCastOnto(perkData, 1, "LOCAL", 1, GameController.Instance.player);
	}

	public void PressRotateView()
	{
		PopupControl.Instance.SetButtonWasPressed();
		view_rot = ((view_rot != 3) ? (view_rot + 1) : 0);
		switch (view_rot)
		{
		case 0:
			GameController.Instance.cam_angle = new Vector3(5f, 15f, -5f);
			break;
		case 1:
			GameController.Instance.cam_angle = new Vector3(5f, 15f, 5f);
			break;
		case 2:
			GameController.Instance.cam_angle = new Vector3(-5f, 15f, 5f);
			break;
		case 3:
			GameController.Instance.cam_angle = new Vector3(-5f, 15f, -5f);
			break;
		}
		Debug.Log("view_rot=" + view_rot);
	}

	public void PressChangeBiome()
	{
		PopupControl.Instance.SetButtonWasPressed();
		if (debug_bandit_camp_data != null)
		{
			ShowOrHideBaseDevElements(false);
			disable_movement = true;
			WindowPrefabsControl.Instance.CreateScreen("dev - bandit biome", WindowPrefabsControl.build_into_t.GAME_CTR);
		}
	}

	public void SelectedBiome(int biome_id)
	{
		PopupControl.Instance.SetButtonWasPressed();
		WindowPrefabsControl.Instance.DestroyScreen("dev - bandit biome");
		disable_movement = false;
		ShowOrHideBaseDevElements(true);
		debug_bandit_camp_data.biome_id = biome_id;
		debug_bandit_camp_data.ClearBanditCampData();
		RefreshWorld(GameController.Instance.player.transform.position);
	}

	public void PressCancelBiomeChange()
	{
		PopupControl.Instance.SetButtonWasPressed();
		WindowPrefabsControl.Instance.DestroyScreen("dev - bandit biome");
		disable_movement = false;
		ShowOrHideBaseDevElements(true);
	}

	public void PressRecentlyUsed(int index)
	{
		PopupControl.Instance.SetButtonWasPressed();
		if (index < recently_used.Count)
		{
			TryEnterBuildMode(recently_used[index]);
		}
	}

	private void ShowObjectSelectScreen(string header_str)
	{
		WindowPrefabsControl.Instance.CreateScreen("dev - object pick", WindowPrefabsControl.build_into_t.GAME_CTR);
		WindowPrefabsControl.Instance.GetTextMeshPro("dev - object pick", "header_text").text = header_str;
		curr_shortcut_function = enter_shortcut_function.accept_object;
		GameObject screen = WindowPrefabsControl.Instance.GetScreen("dev - object pick");
		for (int i = 0; i < recently_used.Count; i++)
		{
			screen.transform.Find("tooltips").Find("recently-used (" + i + ")").Find("header (1)").GetComponent<TextMeshProUGUI>().text = recently_used[i];
		}
	}

	public void PressCancelOnObjectSelectScreen()
	{
		PopupControl.Instance.SetButtonWasPressed();
		curr_selected_tool = "";
		WindowPrefabsControl.Instance.DestroyScreen("dev - object pick");
		disable_movement = false;
		ShowOrHideBaseDevElements(true);
		curr_shortcut_function = enter_shortcut_function.none;
	}

	public void PressCancelOnBanditNewLoadScreen()
	{
		PopupControl.Instance.SetButtonWasPressed();
		WindowPrefabsControl.Instance.DestroyScreen("dev - bandit new load");
		disable_movement = false;
		ShowOrHideBaseDevElements(true);
	}

	public void PressOkayOnObjectSelectScreen()
	{
		PopupControl.Instance.SetButtonWasPressed();
		TryEnterBuildMode(WindowPrefabsControl.Instance.GetObject("dev - object pick", "input").GetComponent<TMP_InputField>().text);
	}

	public void PressViewDevObjects()
	{
		PopupControl.Instance.SetButtonWasPressed();
		view_dev_objects = !view_dev_objects;
		Debug.Log("view_dev_objects=" + view_dev_objects);
		RefreshWorld(GameController.Instance.player.transform.position);
	}

	private void TryEnterBuildMode(string typed_obj)
	{
		string text;
		string sub_str;
		if (curr_selected_tool == "build")
		{
			if (!(inventory_ctr.Instance.GetItemWorldObjPath(typed_obj) != "") && !(ResourceControl.Instance.GetStringFromItemFile(typed_obj, "is_wall_obj") == "true") && !(ResourceControl.Instance.GetStringFromItemFile(typed_obj, "is_flooring_obj") == "true") && !(typed_obj == "RANDOM_BIOME_GEM") && !(typed_obj == "RANDOM_BIOME_GEM_LARGE") && !(typed_obj == "BIOME_LARGE_STALAGMITE") && !(typed_obj == "BIOME_SMALL_STALAGMITE"))
			{
				// Custom header text added so the message shows up. The shipped game doesn't store the header text, so the commented-out line below shows the call with an empty header.
				ShowEditorDialogue("Cannot Build", "Object '" + typed_obj + "' does not have a 3d world model");
				// ShowEditorDialogue("", "Object '" + typed_obj + "' does not have a 3d world model");
				return;
			}
			text = "Building with '";
			sub_str = "LMB=place object, RMB=delete objects";
		}
		else if (curr_selected_tool == "paint")
		{
			// Changed so it works on any computer. The commented-out line below is how it looked on the developer's machine.
			if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath + "/SYNCHRONOUS/TextFiles/PaintBrushData", typed_obj) + ".txt") && !BanditCampsControl.Instance.bandit_camp_paints.ContainsKey(typed_obj))
			// if (!System.IO.File.Exists(System.IO.Path.Combine("C:\\Hybrid Animals Stuff\\Hybrid Animals Mobile\\Assets\\SYNCHRONOUS\\TextFiles\\PaintBrushData", typed_obj) + ".txt") && !BanditCampsControl.Instance.bandit_camp_paints.ContainsKey(typed_obj))
			{
				// Custom header text added so the message shows up. The shipped game doesn't store the header text, so the commented-out line below shows the call with an empty header.
				ShowEditorDialogue("Cannot Paint", "Object '" + typed_obj + "' has no paintbrush item");
				// ShowEditorDialogue("", "Object '" + typed_obj + "' has no paintbrush item");
				return;
			}
			text = "Painting with '";
			sub_str = "LMB=paint";
		}
		else
		{
			if (!(curr_selected_tool == "stamp"))
			{
				return;
			}
			if (!InventoryUtils.IsStamp(typed_obj))
			{
				// Custom header text added so the message shows up. The shipped game doesn't store the header text, so the commented-out line below shows the call with an empty header.
				ShowEditorDialogue("Cannot Stamp", "Object '" + typed_obj + "' has no stamp item");
				// ShowEditorDialogue("", "Object '" + typed_obj + "' has no stamp item");
				return;
			}
			text = "Stamping with '";
			sub_str = "LMB=stamp";
		}
		string main_str = text + typed_obj + "'";
		int num = -1;
		for (int i = 0; i < recently_used.Count; i++)
		{
			if (recently_used[i] == typed_obj)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			recently_used.RemoveAt(num);
		}
		recently_used.Insert(0, typed_obj);
		if (recently_used.Count > 7)
		{
			recently_used.RemoveAt(recently_used.Count - 1);
		}
		WindowPrefabsControl.Instance.DestroyScreen("dev - object pick");
		CreateToolHeader(main_str, sub_str);
		CreateCancelButton("ALL DONE");
		StartPlacingItems(typed_obj);
	}

	public void PressNewBanditCamp()
	{
		PopupControl.Instance.SetButtonWasPressed();
		WindowPrefabsControl.Instance.DestroyScreen("dev - bandit new load");
		WindowPrefabsControl.Instance.CreateScreen("dev - bandit new", WindowPrefabsControl.build_into_t.GAME_CTR);
	}

	public void PressLoadBanditCamp()
	{
		PopupControl.Instance.SetButtonWasPressed();
		// Changed so it works on any computer. The commented-out line below is how it looks in the shipped game, where the editor folder picker is not compiled in.
		string text = "";
#if UNITY_EDITOR
		text = UnityEditor.EditorUtility.OpenFolderPanel("Load Bandit Camp", Application.dataPath + "/SYNCHRONOUS/TextFiles/bandit-camps", "");
#endif
		// string text = "";
		if (string.IsNullOrEmpty(text))
		{
			Debug.Log("No directory was selected.");
			return;
		}
		string fileName = System.IO.Path.GetFileName(text.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
		StartBanditCampEditor(fileName);
		WindowPrefabsControl.Instance.DestroyScreen("dev - bandit new load");
		disable_movement = false;
		ShowOrHideBaseDevElements(true);
	}

	private Dictionary<string, string> LoadBanditCampOverviewFile(string filePath)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!System.IO.File.Exists(filePath))
		{
			Debug.LogError("File not found at path: " + filePath);
			return dictionary;
		}
		string[] array = System.IO.File.ReadAllLines(filePath);
		foreach (string text in array)
		{
			if (string.IsNullOrWhiteSpace(text) || text.TrimStart().StartsWith("#"))
			{
				continue;
			}
			string[] array2 = text.Split('=');
			if (array2.Length == 2)
			{
				dictionary[array2[0].Trim()] = array2[1].Trim();
			}
			else
			{
				Debug.LogWarning("Skipping malformed line: " + text);
			}
		}
		return dictionary;
	}

	public void StartBanditCampEditor(string bandit_camp_name)
	{
		debug_bandit_camp_data = new BanditCampInstance("debug_instance", 0, bandit_camp_name, 0, 0f, false, true);
		debug_bandit_camp_template = System.IO.Path.Combine("bandit-camps", bandit_camp_name);
		Dictionary<string, string> dictionary = LoadBanditCampOverviewFile(System.IO.Path.Combine(System.IO.Path.Combine(Application.dataPath + "/SYNCHRONOUS/TextFiles/", debug_bandit_camp_template), "_overview.txt"));
		debug_bandit_camp_W = int.Parse(dictionary["w"]);
		debug_bandit_camp_H = int.Parse(dictionary["h"]);
		RefreshWorld(new Vector3((float)debug_bandit_camp_W * 0.5f * 10f, 5f, (float)debug_bandit_camp_H * 0.5f * 10f));
	}

	private void RefreshWorld(Vector3 position)
	{
		ChunkControl.Instance.DestroyAllTerrain();
		ZoneData new_zone_data = ZoneDataControl.Instance.LoadZoneDataFromDisk(ChunkControl.Instance.player_zone);
		ZoneDataControl.Instance.ChangeZone(new_zone_data, ZoneDataControl.change_zone_type.custom_position, position, null, false, false);
	}

	public void ShowOrHideBaseDevElements(bool set_to)
	{
		WindowPrefabsControl.Instance.GetScreen("dev - top left").SetActive(set_to);
		WindowPrefabsControl.Instance.GetScreen("dev - bottom center").SetActive(set_to);
		GameplayGUIControl.Instance.teleport_button.SetActive(set_to);
		GameplayGUIControl.Instance.distance_display.SetActive(set_to);
		GameplayGUIControl.Instance.bottom_left_buttons.SetActive(set_to);
		RedrawBanditButtons();
	}

	public void RedrawBanditButtons()
	{
		GameObject gameObject = WindowPrefabsControl.Instance.GetScreen("dev - bottom center").transform.Find("button-biome").gameObject;
		GameObject gameObject2 = WindowPrefabsControl.Instance.GetScreen("dev - bottom center").transform.Find("button-clear paints").gameObject;
		bool flag = Instance.debug_bandit_camp_data != null;
		gameObject.GetComponent<CanvasGroup>().alpha = (flag ? 1f : 0.5f);
		gameObject2.GetComponent<CanvasGroup>().alpha = (flag ? 1f : 0.5f);
	}

	private void CreateToolHeader(string main_str, string sub_str)
	{
		WindowPrefabsControl.Instance.CreateScreen("dev - top text", WindowPrefabsControl.build_into_t.GAME_CTR);
		WindowPrefabsControl.Instance.GetTextMeshPro("dev - top text", "header_text").text = "<color=#ffffff>" + main_str + "</color>\n" + sub_str;
	}

	private void CreateCancelButton(string cancel_str)
	{
		WindowPrefabsControl.Instance.CreateScreen("dev - bottom cancel", WindowPrefabsControl.build_into_t.GAME_CTR);
		WindowPrefabsControl.Instance.GetTextMeshPro("dev - bottom cancel", "cancel_text").text = cancel_str;
	}

	public void PressCancelOnUsingTool()
	{
		PopupControl.Instance.SetButtonWasPressed();
		WindowPrefabsControl.Instance.DestroyScreen("dev - top text");
		WindowPrefabsControl.Instance.DestroyScreen("dev - bottom cancel");
		StopPlacingItems();
		curr_selected_tool = "";
		disable_movement = false;
		ShowOrHideBaseDevElements(true);
	}

	public void PressCancelOnModifyObjectDataScreen()
	{
		PopupControl.Instance.SetButtonWasPressed();
		WindowPrefabsControl.Instance.DestroyScreen("dev - modify obj data");
		curr_selected_tool = "";
		disable_movement = false;
		ShowOrHideBaseDevElements(true);
	}

	public void PressSaveOnModifyObjectDataScreen()
	{
		PopupControl.Instance.SetButtonWasPressed();
		string text = System.IO.Path.Combine(Application.dataPath + "/SYNCHRONOUS/TextFiles/" + quest_scenics_folder_, mod_chunkStr + ".txt");
		QuestBuildableFile questBuildableFile = LoadQuestBuildableFile(text);
		foreach (QuestBuildableEntry buildable_entry in questBuildableFile.buildable_entries)
		{
			if (!(buildable_entry.main_str == mod_main_str))
			{
				continue;
			}
			string[] array = System.Text.RegularExpressions.Regex.Split(WindowPrefabsControl.Instance.GetObject("dev - modify obj data", "input").GetComponent<TMP_InputField>().text, "\n|\r|\r\n");
			List<string> list = new List<string>();
			foreach (string text2 in array)
			{
				if (!string.IsNullOrWhiteSpace(text2))
				{
					if (text2[0] == '\t')
					{
						list.Add(text2);
					}
					else
					{
						list.Add("\t" + text2);
					}
				}
			}
			buildable_entry.extra_data = list;
			break;
		}
		SaveQuestBuildableFile(questBuildableFile, text);
		ChunkControl.Instance.DevRebuildEntireChunk(ChunkControl.Instance.player_zone, mod_chunkX, mod_chunkZ);
		WindowPrefabsControl.Instance.DestroyScreen("dev - modify obj data");
		curr_selected_tool = "";
		disable_movement = false;
		mod_chunkStr = "";
		mod_main_str = "";
		mod_chunkX = 0;
		mod_chunkZ = 0;
		ShowOrHideBaseDevElements(true);
	}

	public void StartPlacingItems(string item_name)
	{
		curr_place_item = item_name;
		ResetDevMouseObject();
		dev_mouse_obj.transform.Rotate(Vector3.up, place_rot * 90);
		is_placing_items = true;
		curr_shortcut_function = enter_shortcut_function.close_tool_mode;
	}

	public void StopPlacingItems()
	{
		UnityEngine.Object.Destroy(dev_mouse_obj);
		is_placing_items = false;
		curr_shortcut_function = enter_shortcut_function.none;
	}

	private void ResetDevMouseObject()
	{
		if (dev_mouse_obj != null)
		{
			UnityEngine.Object.Destroy(dev_mouse_obj);
		}
		dev_mouse_obj = new GameObject("DEV MOUSE OBJ");
		Action<GameObject> action = delegate(GameObject new_mouse_obj)
		{
			new_mouse_obj.transform.SetParent(dev_mouse_obj.transform);
			new_mouse_obj.transform.localPosition = Vector3.zero;
			new_mouse_obj.transform.localScale = Vector3.one;
			new_mouse_obj.transform.localRotation = Quaternion.identity;
		};
		if (!Startup.StringNullOrWhitespace(inventory_ctr.Instance.GetItemWorldObjPath(curr_place_item)))
		{
			ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab(inventory_ctr.Instance.GetItemWorldObjPath(curr_place_item), null, action);
		}
		else
		{
			action(new GameObject("Empty prefab"));
		}
	}

	private int GetAvailableUniqueId()
	{
		foreach (UniqueIdStatus item in InventoryUtils.GenerateUniqueIdSummary())
		{
			if (item.status == UniqueIdStatus.status_.available)
			{
				return item.id;
			}
		}
		return -1;
	}

	private void Update()
	{
		if (Application.isEditor)
		{
			if (GamepadInput.Instance.GetKeyDown(Key.R) && is_placing_items && curr_selected_tool != "modify_data" && curr_selected_tool != "lock" && !GameServerConnector.Instance.FullyInGame())
			{
				place_rot = ((place_rot + 1 < 4) ? (place_rot + 1) : 0);
				dev_mouse_obj.transform.Rotate(Vector3.up, 90f);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.Enter))
			{
				if (curr_shortcut_function == enter_shortcut_function.close_tool_mode)
				{
					PressCancelOnUsingTool();
				}
				else if (curr_shortcut_function == enter_shortcut_function.accept_object)
				{
					PressOkayOnObjectSelectScreen();
				}
			}
		}
		if (!is_placing_items || GameServerConnector.Instance.FullyInGame() || (!Mouse.current.leftButton.wasPressedThisFrame && !Mouse.current.rightButton.wasPressedThisFrame) || PopupControl.Instance.GetButtonWasPressed())
		{
			return;
		}
		Vector3 chunkCoords = ChunkControl.Instance.GetChunkCoords(dev_mouse_obj.transform.position);
		Vector3 position = dev_mouse_obj.transform.position;
		string chunkString = ChunkControl.Instance.GetChunkString(dev_mouse_obj.transform.position);
		string text = System.IO.Path.Combine(Application.dataPath + "/SYNCHRONOUS/TextFiles/" + quest_scenics_folder_, chunkString + ".txt");
		float num = position.x - chunkCoords.x * 10f;
		float num2 = position.z - chunkCoords.z * 10f;
		int num3 = (int)chunkCoords.x;
		int num4 = (int)num;
		int num5 = (int)num2;
		int num6 = (int)chunkCoords.z;
		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			QuestBuildableFile questBuildableFile = LoadQuestBuildableFile(text);
			if (curr_selected_tool == "paint" || curr_selected_tool == "stamp")
			{
				string text2 = ((curr_selected_tool == "paint") ? "paint" : ((curr_selected_tool == "stamp") ? "stamp" : ""));
				QuestBuildableEntry questBuildableEntry = questBuildableFile.FindBuildableEntry("[" + num4 + "," + num5 + "]");
				if (questBuildableEntry != null)
				{
					string item = "\t" + text2 + " = *string* " + curr_place_item;
					int num7 = questBuildableEntry.FindExtraDataIndex("\t" + text2 + " = ");
					if (num7 == -1)
					{
						questBuildableEntry.extra_data.Add(item);
					}
					else
					{
						questBuildableEntry.extra_data[num7] = item;
					}
				}
				SaveQuestBuildableFile(questBuildableFile, text);
			}
			else if (curr_selected_tool == "build")
			{
				QuestBuildableEntry questBuildableEntry2 = new QuestBuildableEntry("[" + num4 + "," + num5 + "] = " + curr_place_item + " (rot " + place_rot + ")");
				foreach (QuestBuildableEntry buildable_entry in questBuildableFile.buildable_entries)
				{
					if (buildable_entry.overlap_check_str == questBuildableEntry2.overlap_check_str)
					{
						Debug.Log("<color=#ff0000>did not place object - overlapping duplicate</color>");
						return;
					}
				}
				if (InventoryUtils.UsesShackId(curr_place_item))
				{
					int availableUniqueId = GetAvailableUniqueId();
					if (availableUniqueId == -1)
					{
						return;
					}
					questBuildableEntry2.extra_data.Add("\tshack_id = *long* " + availableUniqueId);
					questBuildableFile.buildable_entries.Add(questBuildableEntry2);
					SaveQuestBuildableFile(questBuildableFile, text);
					// Custom header text added so the message shows up. The shipped game doesn't store the header text, so the commented-out line below shows the call with an empty header.
					ShowEditorDialogue("Shack Placed", "Auto Assigned shack_id=" + availableUniqueId);
					// ShowEditorDialogue("", "Auto Assigned shack_id=" + availableUniqueId);
					ZoneDataControl.GenerateNPCShackData();
				}
				else if (InventoryUtils.UsesBasketId(new InventoryItem(curr_place_item)))
				{
					int availableUniqueId2 = GetAvailableUniqueId();
					if (availableUniqueId2 == -1)
					{
						return;
					}
					questBuildableEntry2.extra_data.Add("\tbasket_id = *long* " + availableUniqueId2);
					questBuildableFile.buildable_entries.Add(questBuildableEntry2);
					SaveQuestBuildableFile(questBuildableFile, text);
					// Custom header text added so the message shows up. The shipped game doesn't store the header text, so the commented-out line below shows the call with an empty header.
					ShowEditorDialogue("Basket Placed", "Auto Assigned basket_id=" + availableUniqueId2);
					// ShowEditorDialogue("", "Auto Assigned basket_id=" + availableUniqueId2);
				}
				else
				{
					questBuildableFile.buildable_entries.Add(questBuildableEntry2);
					SaveQuestBuildableFile(questBuildableFile, text);
				}
			}
			else if (curr_selected_tool == "modify_data")
			{
				string value = "[" + num4 + "," + num5 + "]";
				foreach (QuestBuildableEntry buildable_entry2 in questBuildableFile.buildable_entries)
				{
					if (!buildable_entry2.main_str.Contains(value))
					{
						continue;
					}
					WindowPrefabsControl.Instance.DestroyScreen("dev - top text");
					WindowPrefabsControl.Instance.DestroyScreen("dev - bottom cancel");
					StopPlacingItems();
					mod_chunkStr = chunkString;
					mod_main_str = buildable_entry2.main_str;
					mod_chunkX = num3;
					mod_chunkZ = num6;
					WindowPrefabsControl.Instance.CreateScreen("dev - modify obj data", WindowPrefabsControl.build_into_t.GAME_CTR);
					WindowPrefabsControl.Instance.GetTextMeshPro("dev - modify obj data", "header_text").text = "<color=#ffffff>Modifying: </color>" + buildable_entry2.item_name;
					TMP_InputField component = WindowPrefabsControl.Instance.GetObject("dev - modify obj data", "input").GetComponent<TMP_InputField>();
					string text3 = "";
					foreach (string extra_datum in buildable_entry2.extra_data)
					{
						text3 = ((extra_datum[0] != '\t') ? (text3 + extra_datum + "\n") : (text3 + extra_datum.Substring(1, extra_datum.Length - 1) + "\n"));
					}
					component.SetTextWithoutNotify(text3);
					// Changed so it works on any computer. The commented-out line below is how it looked on the developer's machine.
					string path = System.IO.Path.Combine(Application.dataPath + "/SYNCHRONOUS/TextFiles/Dev Tooltips", buildable_entry2.item_name + ".txt");
					// string path = System.IO.Path.Combine("C:\\Hybrid Animals Stuff\\Dev Tooltips", buildable_entry2.item_name + ".txt");
					GameObject gameObject = WindowPrefabsControl.Instance.GetObject("dev - modify obj data", "tooltips");
					if (!System.IO.File.Exists(path))
					{
						gameObject.SetActive(false);
						return;
					}
					gameObject.SetActive(true);
					WindowPrefabsControl.Instance.GetTextMeshPro("dev - modify obj data", "tooltip_text").text = System.IO.File.ReadAllText(path);
					return;
				}
				return;
			}
			else
			{
				if (!(curr_selected_tool == "lock"))
				{
					return;
				}
				string value2 = "[" + num4 + "," + num5 + "]";
				foreach (QuestBuildableEntry buildable_entry3 in questBuildableFile.buildable_entries)
				{
					if (!buildable_entry3.main_str.Contains(value2))
					{
						continue;
					}
					bool flag = false;
					foreach (string extra_datum2 in buildable_entry3.extra_data)
					{
						flag |= extra_datum2.Contains("password = ");
					}
					if (!flag)
					{
						buildable_entry3.extra_data.Add("\tpassword = *string* DEV_LOCKED");
						SaveQuestBuildableFile(questBuildableFile, text);
						ChunkControl.Instance.DevRebuildEntireChunk(ChunkControl.Instance.player_zone, num3, num6);
						// Custom header text added so the message shows up. The shipped game doesn't store the header text, so the commented-out line below shows the call with an empty header.
						ShowEditorDialogue("Locked", "Locked: " + buildable_entry3.item_name);
						// ShowEditorDialogue("", "Locked: " + buildable_entry3.item_name);
					}
					else
					{
						// Custom header text added so the message shows up. The shipped game doesn't store the header text, so the commented-out line below shows the call with an empty header.
						ShowEditorDialogue("Already Locked", "'" + buildable_entry3.item_name + "' already locked!");
						// ShowEditorDialogue("", "'" + buildable_entry3.item_name + "' already locked!");
					}
					PressCancelOnUsingTool();
					return;
				}
				return;
			}
		}
		else
		{
			if (!Mouse.current.rightButton.wasPressedThisFrame || !(curr_selected_tool == "build"))
			{
				return;
			}
			if (System.IO.File.Exists(text))
			{
				QuestBuildableFile questBuildableFile2 = LoadQuestBuildableFile(text);
				QuestBuildableEntry questBuildableEntry3 = questBuildableFile2.FindBuildableEntry("[" + num4 + "," + num5 + "]");
				if (questBuildableEntry3 != null)
				{
					bool flag2 = InventoryUtils.UsesShackId(questBuildableEntry3.item_name);
					bool flag3 = false;
					if (flag2)
					{
						int num8 = -1;
						foreach (string extra_datum3 in questBuildableEntry3.extra_data)
						{
							if (extra_datum3.Contains("\tshack_id = *long* "))
							{
								num8 = int.Parse(extra_datum3.Replace("\tshack_id = *long* ", ""));
								break;
							}
						}
						if (num8 != -1)
						{
							string[] files = System.IO.Directory.GetFiles(Application.dataPath + "/SYNCHRONOUS/TextFiles/" + quest_scenics_folder_);
							for (int i = 0; i < files.Length; i++)
							{
								if (System.IO.Path.GetFileNameWithoutExtension(files[i]).Contains("chunk(shack" + num8))
								{
									// Custom header text added so the message shows up. The shipped game doesn't store the header text, so the commented-out line below shows the call with an empty header.
									ShowEditorDialogue("Cannot Delete", "Error: Must delete all objects within shack" + num8 + " first");
									// ShowEditorDialogue("", "Error: Must delete all objects within shack" + num8 + " first");
									flag3 = true;
									break;
								}
							}
						}
					}
					if (!flag3)
					{
						questBuildableFile2.buildable_entries.Remove(questBuildableEntry3);
						SaveQuestBuildableFile(questBuildableFile2, text);
						if (flag2)
						{
							ZoneDataControl.GenerateNPCShackData();
						}
					}
				}
			}
		}
		ChunkControl.Instance.DevRebuildEntireChunk(ChunkControl.Instance.player_zone, num3, num6);
	}

	private void FixedUpdate()
	{
		if (!is_placing_items)
		{
			return;
		}
		Ray ray = Camera.main.ScreenPointToRay(GamepadInput.Instance.GetMousePosition());
		if (GameController.Instance.plane.Raycast(ray, out var enter))
		{
			Vector3 point = ray.GetPoint(enter);
			Vector3 chunkCoords = ChunkControl.Instance.GetChunkCoords(point);
			Vector3 inner = ChunkControl.Instance.GetInner(point);
			dev_mouse_obj.transform.position = new Vector3(chunkCoords.x * 10f + (float)(int)inner.x + 0.5f, 0f, chunkCoords.z * 10f + (float)(int)inner.z + 0.5f);
		}
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
