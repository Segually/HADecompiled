using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConsoleControl : MonoBehaviour, OrderedStart
{
	private struct nuke_element
	{
		public int innerX;

		public int innerZ;

		public ChunkElement element;
	}

	public static ConsoleControl Instance;

	public GameObject console_parent_obj;

	public GameObject main_input_obj;

	public GameObject error_notif_obj;

	public InputField input;

	public bool console_open;

	public bool show_errors;

	public Text debug_log_text;

	public GameObject debug_log_obj;

	private List<string> new_logs = new List<string>();

	private List<string> past_logs = new List<string>();

	public static float auto_sprint_speed = 1.8f;

	public static float dialogue_speed_mod = 3f;

	private string modify_gfx_obj = "";

	public bool god_mode_enabled;

	private int prev_command_index = -1;

	private List<string> previous_commands = new List<string>();

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
			god_mode_enabled = false;
		}
	}

	public void Start_1()
	{
		Application.logMessageReceived += HandleLog;
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		if (!show_errors)
		{
			return;
		}
		if (logString == null)
		{
			if (stackTrace != null)
			{
				LogError("<color=#ff0000>" + stackTrace + "</color>");
			}
		}
		else if (stackTrace != null)
		{
			LogError("<color=#ff0000>" + logString + " ... " + stackTrace + "</color>");
		}
		else
		{
			LogError("<color=#ff0000>" + logString + "</color>");
		}
	}

	private void LogError(string str)
	{
		if (!str.Contains("_MainTex"))
		{
			new_logs.Add(str);
		}
	}

	private void PrintLog(string str)
	{
		past_logs.Insert(0, str);
		if (past_logs.Count == 17)
		{
			past_logs.RemoveAt(16);
		}
		string text = "";
		foreach (string past_log in past_logs)
		{
			text = text + past_log + "\n";
		}
		debug_log_text.text = text;
	}

	private void FixedUpdate()
	{
		if (!show_errors || new_logs.Count == 0)
		{
			return;
		}
		foreach (string new_log in new_logs)
		{
			PrintLog(new_log);
		}
		new_logs.Clear();
	}

	private IEnumerator TrackAverageFps()
	{
		List<float> fps_data = new List<float>();
		ShowNotif("<color=#00ff00>TRACKING FPS (???)(? seconds)</color>");
		int seconds = 15;
		for (int i = 0; i < seconds; i++)
		{
			int subdiv = 5;
			for (int j = 0; j < subdiv; j++)
			{
				float num = 1f / Time.deltaTime;
				fps_data.Add(num);
				ShowNotif("<color=#00ff00>TRACKING FPS (" + num + ")(" + i + " seconds)</color>");
				yield return new WaitForSeconds(1f / (float)subdiv);
			}
		}
		float num2 = 0f;
		foreach (float fps_datum in fps_data)
		{
			num2 += fps_datum;
		}
		float num3 = num2 / (float)fps_data.Count;
		ShowNotif("<color=#00ff00>AVERAGE FPS = " + num3 + "</color>");
	}

	private void ModifyItemValue(string key, float mod, float default_val)
	{
		string path = Application.dataPath + "/SYNCHRONOUS/TextFiles/InventoryItems/" + modify_gfx_obj + ".txt";
		List<string> list = new List<string>(System.IO.File.ReadAllLines(path));
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < list.Count; i++)
		{
			int num = list[i].IndexOf('=');
			if (num == -1)
			{
				continue;
			}
			string text = list[i].Substring(0, num - 1);
			string text2 = list[i].Substring(num + 2, list[i].Length - (num + 2));
			if (text == key)
			{
				float num2 = float.Parse(text2, Startup.parse_culture) + mod;
				list[i] = text + " = " + num2;
				flag = true;
			}
			else if (text == "show_model3d")
			{
				if (text2 != "true")
				{
					list[i] = "show_model3d = true";
				}
				flag2 = true;
			}
		}
		if (!flag)
		{
			list.Add(key + " = " + default_val);
		}
		if (!flag2)
		{
			list.Add("show_model3d = true");
		}
		System.IO.File.WriteAllLines(path, list);
		ItemScreenshotTaker.Instance.cached_model3d_graphics.Clear();
		ResourceControl.Instance.ReleaseItemFile(modify_gfx_obj);
		inventory_ctr.Instance.RedrawInventorySlots();
	}

	public void OnPressSubmit()
	{
		error_notif_obj.SetActive(false);
		string text = input.text;
		if (previous_commands.Count == 0)
		{
			previous_commands.Add(text);
		}
		else
		{
			if (prev_command_index != -1 && previous_commands[prev_command_index] == text)
			{
				previous_commands.RemoveAt(prev_command_index);
			}
			previous_commands.Insert(0, text);
			if (previous_commands.Count > 10)
			{
				previous_commands.RemoveAt(previous_commands.Count - 1);
			}
		}
		prev_command_index = -1;
		input.SetTextWithoutNotify("");
		input.ActivateInputField();
		int num = text.IndexOf(" ");
		string text2 = text;
		if (num != -1)
		{
			text2 = text.Substring(0, num);
		}
		switch (text2)
		{
		case "dev_mode":
		case "dev":
		{
			short globalShort = PlayerData.Instance.GetGlobalShort("dev_mode");
			int num3 = 1;
			bool flag = false;
			if (globalShort == 0)
			{
				ShowNotif("<color=#00ff00>DEV MODE ENABLED</color>");
			}
			else if (globalShort == 1)
			{
				num3 = 0;
				ShowNotif("<color=#00ff00>DEV MODE DISABLED</color>");
			}
			else
			{
				num3 = 0;
				flag = true;
			}
			PlayerData.Instance.SetGlobalShort("dev_mode", num3);
			if (SceneManager.GetActiveScene().name == "Game")
			{
				float walk_speed = GameController.Instance.player.GetComponent<SharedCreature>().walk_speed;
				float num4 = auto_sprint_speed;
				if (num3 == 0)
				{
					GameController.Instance.player.GetComponent<SharedCreature>().walk_speed = walk_speed / num4;
					GameplayGUIControl.Instance.curr_GUI = GameplayGUIControl.GUI_layout_t.standard_gameplay;
				}
				else
				{
					GameController.Instance.player.GetComponent<SharedCreature>().walk_speed = walk_speed * num4;
					GameplayGUIControl.Instance.curr_GUI = GameplayGUIControl.GUI_layout_t.dev_interface;
				}
				GameplayGUIControl.Instance.ShowGameplayGui();
			}
			if (!flag)
			{
				return;
			}
			break;
		}
		case "perk_dev":
			PerkScreenDev.Instance.EnableDevMode();
			ShowNotif("<color=#00ff00>PERK DEV ENABLED</color>");
			return;
		case "banana123":
			PlayerData.Instance.SetGlobalString("username_lower", "banana");
			PlayerData.Instance.SetGlobalString("username_punctuated", "Banana");
			ShowNotif("<color=#00ff00>Welcome, Banana</color>");
			return;
		case "tfh123":
			PlayerData.Instance.SetGlobalString("username_lower", "thefirsthybrid");
			PlayerData.Instance.SetGlobalString("username_punctuated", "TheFirstHybrid");
			ShowNotif("<color=#00ff00>Welcome, Tfh</color>");
			return;
		case "custom_item":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT RECEIVE ITEM IN MENU");
				return;
			}
			ExtraInventoryData extraInventoryData = new ExtraInventoryData();
			extraInventoryData.SetShort("custom_graphic_version", 1);
			Texture2D texture2D = new Texture2D(128, 128);
			// Changed so it works on any computer. The commented-out line below is how it looked on the developer's machine.
			string path_texture2D = Application.dataPath + "/SYNCHRONOUS/Custom Items/item wisdom chest.png";
			if (System.IO.File.Exists(path_texture2D))
			{
				texture2D.LoadImage(System.IO.File.ReadAllBytes(path_texture2D));
			}
			// texture2D.LoadImage(System.IO.File.ReadAllBytes("C:\\Hybrid Animals Stuff\\Custom Items\\item wisdom chest.png"));
			SaveCustomGraphic(texture2D, extraInventoryData, "n_img_bts", "ib");
			extraInventoryData.SetShort("custom_mesh_version", 1);
			SaveCustomItemMesh(inventory_ctr.Instance.CUSTOM_ITEM_MODEL, extraInventoryData);
			extraInventoryData.SetShort("custom_texture_version", 1);
			Texture2D texture2D2 = new Texture2D(64, 64);
			// Changed so it works on any computer. The commented-out line below is how it looked on the developer's machine.
			string path_texture2D2 = Application.dataPath + "/SYNCHRONOUS/Custom Items/wisdom chest tex.png";
			if (System.IO.File.Exists(path_texture2D2))
			{
				texture2D2.LoadImage(System.IO.File.ReadAllBytes(path_texture2D2));
			}
			// texture2D2.LoadImage(System.IO.File.ReadAllBytes("C:\\Hybrid Animals Stuff\\Custom Items\\wisdom chest tex.png"));
			SaveCustomGraphic(texture2D2, extraInventoryData, "n_tex_bts", "Tb");
			extraInventoryData.SetString("custom_type", "furniture");
			extraInventoryData.SetString("interaction_type", "chest");
			extraInventoryData.SetShort("extra_interact_dist", 3);
			extraInventoryData.SetShort("uses_basket_id", 1);
			extraInventoryData.SetShort("loot_reset_hours", 24);
			extraInventoryData.SetString("mono_loot_type", "Coins");
			extraInventoryData.SetShort("mono_loot_amount", 10000);
			InventoryItem item = new InventoryItem(text.Substring(num + 1, text.Length - (num + 1)) + "'s Wisdom Chest", extraInventoryData);
			inventory_ctr.Instance.GiveItem(item, 1, "");
			ShowNotif("<color=#00ff00>RECEIVED CUSTOM ITEM</color>");
			return;
		}
		case "god_mode":
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT SET GOD_MODE IN MENU");
				return;
			}
			god_mode_enabled = !god_mode_enabled;
			ShowNotif(god_mode_enabled ? "<color=#00ff00>GOD_MODE ENABLED</color>" : "<color=#00ff00>GOD_MODE DISABLED</color>");
			return;
		case "give":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT GIVE ITEMS IN MENU");
				return;
			}
			if (text.Length <= 5)
			{
				break;
			}
			int num5;
			string text3;
			if (text[5] == 'x')
			{
				num = text.IndexOf(' ', num + 1);
				num5 = int.Parse(text.Substring(6, num - 6), Startup.parse_culture);
				text3 = text.Substring(num + 1, text.Length - (num + 1));
			}
			else
			{
				text3 = text.Substring(num + 1, text.Length - (num + 1));
				num5 = 1;
			}
			InventoryItem item2;
			if (text3 == "Trophy")
			{
				Trophy trophy = new Trophy("Debug Trophy", "For Testing Purposes", "Basic Red", "Editor", "", System.DateTime.UtcNow.ToString());
				item2 = FriendServerInterface.Instance.GenerateTrophy(trophy);
			}
			else
			{
				ExtraInventoryData extraInventoryData2 = new ExtraInventoryData();
				LootControl.Instance.ResolveLootExtraData(text3, extraInventoryData2);
				item2 = new InventoryItem(text3, extraInventoryData2);
			}
			inventory_ctr.Instance.GiveItem(item2, num5, "");
			ShowNotif("<color=#00ff00>Gave x" + num5 + " of '" + text3 + "'</color>");
			return;
		}
		case "companion":
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT HATCH COMPANION IN MENU");
				return;
			}
			CompanionController.Instance.CreateAnimatedEgg(3, true);
			ShowNotif("<color=#00ff00>HATCHING COMPANION</color>");
			return;
		case "delete_all":
		case "delete_general":
			PlayerData.Instance.DeleteGemsEtc();
			ShowNotif("<color=#00ff00>GENERAL DATA DELETED</color>");
			return;
		case "perk_level":
		{
			int num6 = int.Parse(text.Substring(num + 1, text.Length - (num + 1)), Startup.parse_culture);
			PlayerData.Instance.SetGlobalShort("dev_perk_level", num6);
			ShowNotif("<color=#00ff00>SET PERK_LEVEL=" + num6 + "</color>");
			return;
		}
		case "set_custom_pass":
		case "set_rand_code":
		case "set_random_code":
		case "set_custom_password":
		{
			string text4 = text.Substring(num + 1, text.Length - (num + 1));
			PlayerData.Instance.SetGlobalString("rand_code", text4);
			ShowNotif("<color=#00ff00>SET CUSTOM_PASS=" + text4 + "</color>");
			return;
		}
		case "set_custom_port":
		{
			string text4 = text.Substring(num + 1, text.Length - (num + 1));
			PlayerData.Instance.SetGlobalString("custom_port", text4);
			ShowNotif("<color=#00ff00>SET CUSTOM_PORT=" + text4 + "</color>");
			return;
		}
		case "set_custom_ip":
		{
			string text4 = text.Substring(num + 1, text.Length - (num + 1));
			PlayerData.Instance.SetGlobalString("custom_ip", text4);
			ShowNotif("<color=#00ff00>SET CUSTOM_IP=" + text4 + "</color>");
			return;
		}
		case "perks_neglected":
		case "perks_average":
		case "perks_excellent":
		{
			int num7 = (int)PerkControl.Instance.GetAveragePerkLevel(GameController.Instance.playerLevel);
			if (num7 == 0)
			{
				num7 = 1;
			}
			Debug.Log("average_perk_level=" + num7);
			int new_level = ((text2 == "perks_neglected") ? 1 : ((text2 == "perks_average") ? num7 : ((!(text2 == "perks_excellent")) ? 1 : (num7 + 30))));
			bool file_exists = false;
			List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("AutoGen/(Auto Gen) Perks List", ref file_exists);
			if (file_exists)
			{
				foreach (string item3 in textFileLines)
				{
					if (!Startup.StringNullOrWhitespace(item3))
					{
						PerkData perkDataForInfoDisplay = PerkControl.Instance.GetPerkDataForInfoDisplay(item3);
						if (!perkDataForInfoDisplay.not_unlockable && perkDataForInfoDisplay.max_level == -1)
						{
							PerkControl.Instance.OverwritePerkLevel(item3, new_level);
							PerkControl.Instance.SavePerkLevel(item3);
						}
					}
				}
			}
			PerkControl.Instance.RedrawEquippedPerkSlots();
			ShowNotif("<color=#00ff00>PERKS REVISED</color>");
			return;
		}
		case "delete_prefs":
		case "delete_player_prefs":
		case "clear_prefs":
		case "clear_player_prefs":
			PlayerPrefs.DeleteAll();
			ShowNotif("<color=#00ff00>PLAYER PREFS CLEARED!</color>");
			return;
		case "fps":
			StartCoroutine(TrackAverageFps());
			return;
		case "kill":
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT KILL IN MENU");
				return;
			}
			GameController.Instance.player.GetComponent<Combatant>().WasHit(99999, GameController.Instance.player, false, false, Combatant.hit_col.color_red, true);
			ShowNotif("<color=#00ff00>KILL SUCCESSFUL</color>");
			return;
		case "connect_langford":
			PopupControl.Instance.ShowMessage("Use 'connect_custom' instead", PopupControl.context.message);
			ShowNotif("<color=#ff0000>USE CONNECT_CUSTOM</color>");
			return;
		case "place":
			ShowNotif("Use 'dev' mode to place objects");
			return;
		case "paint":
			ShowNotif("Use 'dev' mode to paint objects");
			return;
		case "stamp":
			ShowNotif("Use 'dev' mode to stamp objects");
			return;
		case "clear_username":
		case "delete_username":
		case "reset_username":
			PlayerData.Instance.SetGlobalString("username_lower", "");
			ShowNotif("<color=#00ff00>USERNAME CLEARED!</color>");
			return;
		case "connect_azure":
			PlayerData.Instance.SetGlobalString("connect_to", "");
			ShowNotif("<color=#00ff00>CONNECT TO AZURE</color>");
			return;
		case "custom_connect":
		case "connect_custom":
			PlayerData.Instance.SetGlobalString("connect_to", "custom");
			ShowNotif("<color=#00ff00>CONNECT TO CUSTOM</color>");
			return;
		case "set_level":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT SET LEVEL IN MENU");
				return;
			}
			int num8 = int.Parse(text.Substring(num + 1, text.Length - (num + 1)), Startup.parse_culture);
			SetLevel(num8);
			ShowNotif("<color=#00ff00>SET LEVEL=" + num8 + "</color>");
			return;
		}
		case "inner":
		case "chunk_str":
		case "chunk":
		case "inner_str":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT CALL CHUNK_STR IN MENU");
				return;
			}
			string chunkString3 = ChunkControl.Instance.GetChunkString(GameController.Instance.player.transform.position);
			Vector3 inner = ChunkControl.Instance.GetInner(GameController.Instance.player.transform.position);
			int num9 = (int)inner.x;
			int num10 = (int)inner.z;
			ShowNotif("<color=#00ff00>CHUNK_STR=" + chunkString3 + " " + num9 + " " + num10 + "</color>");
			return;
		}
		case "origin":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT CALL ORIGIN IN MENU");
				return;
			}
			ZoneData curr_zonedata = ZoneDataControl.Instance.curr_zonedata;
			int interior_model_chunkX = curr_zonedata.interior_model_chunkX;
			int interior_model_chunkZ = curr_zonedata.interior_model_chunkZ;
			int interior_model_innerX = curr_zonedata.interior_model_innerX;
			int interior_model_innerZ = curr_zonedata.interior_model_innerZ;
			float x = (float)(int)GameController.Instance.player.transform.position.x - 0.5f;
			float z = (float)(int)GameController.Instance.player.transform.position.z + 0.5f;
			Vector3 vector = new Vector3(x - ((float)(interior_model_chunkX * 10 + interior_model_innerX) + 0.5f), 0f, z - ((float)(interior_model_chunkZ * 10 + interior_model_innerZ) + 0.5f));
			ShowNotif("<color=#00ff00>ORIGIN_DIFF=" + vector.ToString() + "</color>");
			return;
		}
		case "mod":
			MapEditorControl.Instance.editing_map = true;
			SceneManager.LoadSceneAsync("Game");
			ShowNotif("<color=#00ff00>ENTERING MAP EDITOR</color>");
			return;
		case "day":
			GameController.time_of_day = 0.3f;
			ShowNotif("<color=#00ff00>DAY ENABLED</color>");
			return;
		case "night":
			GameController.time_of_day = 0.8f;
			ShowNotif("<color=#00ff00>NIGHT ENABLED</color>");
			return;
		case "force_floor_model":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT FORCE FLOOR IN MENU");
				return;
			}
			string chunkString = ChunkControl.Instance.GetChunkString(GameController.Instance.player.transform.position);
			string path = System.IO.Path.Combine(Application.dataPath + System.IO.Path.DirectorySeparatorChar + "SYNCHRONOUS/TextFiles" + System.IO.Path.DirectorySeparatorChar + DevBuildControl.quest_scenics_folder_, chunkString + ".txt");
			ForcedChunkData forcedChunkData = ChunkControl.Instance.TryLoadForcedChunkData_(ChunkControl.Instance.GetForcedChunkDataFilePath(chunkString));
			int num2 = int.Parse(text.Substring(num + 1, text.Length - (num + 1)), Startup.parse_culture);
			forcedChunkData.forced_floor_model = num2;
			Save(path, forcedChunkData);
			ChunkControl.Instance.DevRebuildChunkAt(GameController.Instance.player.transform.position);
			ShowNotif("<color=#00ff00>SET FLOOR_MODEL=" + num2 + " FOR " + chunkString + "</color>");
			return;
		}
		case "force_floor_rot":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT FORCE FLOOR IN MENU");
				return;
			}
			string chunkString = ChunkControl.Instance.GetChunkString(GameController.Instance.player.transform.position);
			string path = System.IO.Path.Combine(Application.dataPath + System.IO.Path.DirectorySeparatorChar + "SYNCHRONOUS/TextFiles" + System.IO.Path.DirectorySeparatorChar + DevBuildControl.quest_scenics_folder_, chunkString + ".txt");
			ForcedChunkData forcedChunkData = ChunkControl.Instance.TryLoadForcedChunkData_(ChunkControl.Instance.GetForcedChunkDataFilePath(chunkString));
			int num2 = int.Parse(text.Substring(num + 1, text.Length - (num + 1)), Startup.parse_culture);
			forcedChunkData.forced_floor_rot = num2;
			Save(path, forcedChunkData);
			ChunkControl.Instance.DevRebuildChunkAt(GameController.Instance.player.transform.position);
			ShowNotif("<color=#00ff00>SET FLOOR_ROT=" + num2 + " FOR " + chunkString + "</color>");
			return;
		}
		case "caveart_index":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT SET CAVE ART IN MENU");
				return;
			}
			string chunkString = ChunkControl.Instance.GetChunkString(GameController.Instance.player.transform.position);
			string path = System.IO.Path.Combine(Application.dataPath + System.IO.Path.DirectorySeparatorChar + "SYNCHRONOUS/TextFiles" + System.IO.Path.DirectorySeparatorChar + DevBuildControl.quest_scenics_folder_, chunkString + ".txt");
			ForcedChunkData forcedChunkData = ChunkControl.Instance.TryLoadForcedChunkData_(ChunkControl.Instance.GetForcedChunkDataFilePath(chunkString));
			int num2 = int.Parse(text.Substring(num + 1, text.Length - (num + 1)), Startup.parse_culture);
			forcedChunkData.caveart_index = num2;
			Save(path, forcedChunkData);
			ChunkControl.Instance.DevRebuildChunkAt(GameController.Instance.player.transform.position);
			ShowNotif("<color=#00ff00>SET CAVEART_INDEX=" + num2 + " FOR " + chunkString + "</color>");
			return;
		}
		case "caveart_rot":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT SET CAVE ART IN MENU");
				return;
			}
			string chunkString = ChunkControl.Instance.GetChunkString(GameController.Instance.player.transform.position);
			string path = System.IO.Path.Combine(Application.dataPath + System.IO.Path.DirectorySeparatorChar + "SYNCHRONOUS/TextFiles" + System.IO.Path.DirectorySeparatorChar + DevBuildControl.quest_scenics_folder_, chunkString + ".txt");
			ForcedChunkData forcedChunkData = ChunkControl.Instance.TryLoadForcedChunkData_(ChunkControl.Instance.GetForcedChunkDataFilePath(chunkString));
			int num2 = int.Parse(text.Substring(num + 1, text.Length - (num + 1)), Startup.parse_culture);
			forcedChunkData.caveart_rot = num2;
			Save(path, forcedChunkData);
			ChunkControl.Instance.DevRebuildChunkAt(GameController.Instance.player.transform.position);
			ShowNotif("<color=#00ff00>SET CAVEART_ROT=" + num2 + " FOR " + chunkString + "</color>");
			return;
		}
		case "force_biome":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT SET FORCED_BIOME IN MENU");
				return;
			}
			string chunkString4 = ChunkControl.Instance.GetChunkString(GameController.Instance.player.transform.position);
			string path4 = System.IO.Path.Combine(Application.dataPath + "/SYNCHRONOUS/TextFiles/" + DevBuildControl.quest_scenics_folder_, chunkString4 + ".txt");
			ForcedChunkData forcedChunkData4 = ChunkControl.Instance.TryLoadForcedChunkData_(ChunkControl.Instance.GetForcedChunkDataFilePath(chunkString4));
			int num11 = int.Parse(text.Substring(num + 1, text.Length - (num + 1)), Startup.parse_culture);
			forcedChunkData4.forced_biome = num11;
			Save(path4, forcedChunkData4);
			ChunkControl.Instance.DevRebuildChunkAt(GameController.Instance.player.transform.position);
			ShowNotif("<color=#00ff00>SET FORCED_BIOME=" + num11 + " FOR " + chunkString4 + "</color>");
			return;
		}
		case "undo_force_biome":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT SET FORCED_BIOME IN MENU");
				return;
			}
			string chunkString2 = ChunkControl.Instance.GetChunkString(GameController.Instance.player.transform.position);
			string path2 = System.IO.Path.Combine(Application.dataPath + "/SYNCHRONOUS/TextFiles/" + DevBuildControl.quest_scenics_folder_, chunkString2 + ".txt");
			ForcedChunkData forcedChunkData2 = ChunkControl.Instance.TryLoadForcedChunkData_(ChunkControl.Instance.GetForcedChunkDataFilePath(chunkString2));
			forcedChunkData2.forced_biome = -1;
			Save(path2, forcedChunkData2);
			ChunkControl.Instance.DevRebuildChunkAt(GameController.Instance.player.transform.position);
			ShowNotif("<color=#00ff00>SET FORCED_BIOME=-1 FOR " + chunkString2 + "</color>");
			return;
		}
		case "force_blank":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT SET BLANK CHUNK IN MENU");
				return;
			}
			string chunkString5 = ChunkControl.Instance.GetChunkString(GameController.Instance.player.transform.position);
			string path5 = System.IO.Path.Combine(Application.dataPath + "/SYNCHRONOUS/TextFiles/" + DevBuildControl.quest_scenics_folder_, chunkString5 + ".txt");
			ForcedChunkData forcedChunkData5 = ChunkControl.Instance.TryLoadForcedChunkData_(ChunkControl.Instance.GetForcedChunkDataFilePath(chunkString5));
			forcedChunkData5.is_blank = true;
			Save(path5, forcedChunkData5);
			ChunkControl.Instance.DevRebuildChunkAt(GameController.Instance.player.transform.position);
			ShowNotif("<color=#00ff00>SET IS_BLANK=TRUE FOR " + chunkString5 + "</color>");
			return;
		}
		case "hack":
			inventory_ctr.Instance.player_inventory[0] = new ItemCountPair("Bed", 5000);
			break;
		case "big_bonsai":
		{
			ExtraInventoryData extraInventoryData3 = new ExtraInventoryData();
			extraInventoryData3.SetShort("bonsai_age", 2000);
			InventoryItem item4 = new InventoryItem("Bonsai Tree", extraInventoryData3);
			inventory_ctr.Instance.GiveItem(item4, 1, "");
			break;
		}
		case "books":
			GiveDevBook("The Frozen Creed");
			GiveDevBook("The Seige of Shindo Castle");
			GiveDevBook("The Unyielding Wing");
			GiveDevBook("The Sandskull Creed");
			GiveDevBook("Prophecy of the Sands");
			GiveDevBook("Shadows Beneath");
			GiveDevBook("Roots of Wrath");
			GiveDevBook("The Beast Beneath the Bark");
			GiveDevBook("Flight of the Aether Dragon");
			GiveDevBook("Salt of the Sea");
			GiveDevBook("From the Deep, I Rise");
			GiveDevBook("Purists in the Rising Tide");
			GiveDevBook("The Path of the Leech");
			GiveDevBook("The Sisters' Secrets");
			GiveDevBook("A Cold Snap");
			GiveDevBook("The Fox's Hunt");
			GiveDevBook("The Autumn King's Pledge");
			GiveDevBook("A Thorn in Their Side");
			GiveDevBook("The Silent Strike");
			GiveDevBook("The Poisonous Path");
			GiveDevBook("Vanished Without a Trace");
			return;
		case "dev_zone":
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT TELEPORT IN MENU");
				return;
			}
			CustomTeleporterControl.Instance.VisuallyTeleportMainPlayer("");
			CustomTeleporterControl.Instance.DelayedTeleportTransition(-999, CustomTeleporterControl.tele_type.hardcoded);
			ShowNotif("<color=#00ff00>TELEPORTING</color>");
			return;
		case "unlock_all":
		case "unlock_doors":
		case "unlock":
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT UNLOCK DOORS IN MENU");
				return;
			}
			DevBuildControl.Instance.all_doors_unlocked = true;
			ShowNotif("<color=#00ff00>ALL DOORS UNLOCKED</color>");
			return;
		case "clear_inv":
			inventory_ctr.Instance.ClearInventory(true);
			inventory_ctr.Instance.GiveItem("Builder Tools", 1, "", false);
			ShowNotif("<color=#00ff00>INVENTORY CLEARED</color>");
			return;
		case "genomes":
		case "gene_points":
		case "genetic_points":
		case "power_points":
		case "perk_points":
		{
			int num12 = int.Parse(text.Substring(num + 1, text.Length - (num + 1)), Startup.parse_culture);
			PerkControl.Instance.genomes = num12;
			PerkControl.Instance.SaveGenomes();
			ShowNotif("<color=#00ff00>SET GENOMES=" + num12 + "</color>");
			return;
		}
		case "gems":
		{
			int num13 = int.Parse(text.Substring(num + 1, text.Length - (num + 1)), Startup.parse_culture);
			PlayerData.Instance.SetGlobalShort("GEMS", num13);
			ShowNotif("<color=#00ff00>SET GEMS=" + num13 + "</color>");
			return;
		}
		case "super_far":
			GameController.Instance.player.transform.position = new Vector3(10000f, 1f, 10000f);
			break;
		case "tp_to":
		{
			string text5 = text.Substring(6, text.Length - 6);
			int num14 = text5.IndexOf(" ");
			string text6 = text5.Substring(0, num14);
			string text7 = text5.Substring(num14 + 1, text5.Length - (num14 + 1));
			GameController.Instance.player.transform.position = new Vector3(int.Parse(text6, Startup.parse_culture) * 10, 0f, int.Parse(text7, Startup.parse_culture) * 10);
			GameController.Instance.player.GetComponent<SharedCreature>().CancelMoveto();
			ShowNotif("A='" + text6 + "', B='" + text7 + "'");
			return;
		}
		case "sprint":
		case "auto_sprint":
		case "unlock_tp":
			PopupControl.Instance.ShowMessage("Use 'dev' command instad", PopupControl.context.message);
			ShowNotif("Use 'dev' command instead");
			return;
		case "randomize":
		{
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT RANDOMIZE CREATURE IN MENU");
				return;
			}
			int level = ((!(UnityEngine.Random.value < 0.5f)) ? UnityEngine.Random.Range(1, 50) : UnityEngine.Random.Range(50, 350));
			SetLevel(level);
			GameController.Instance.player_parent_creatures = new List<string>();
			GameController.Instance.player_parent_creatures.Add(CreatureMorpher.Instance.GetRandomCreature());
			GameController.Instance.player_parent_creatures.Add(CreatureMorpher.Instance.GetRandomCreature());
			GameController.Instance.SaveParentCreaturesToDisk();
			GameController.Instance.player.GetComponent<SharedCreature>().ReplaceCreatureModel(GameController.Instance.player_parent_creatures);
			GameplayGUIControl.Instance.DrawComboText();
			ShowNotif("<color=#00ff00>RANDOMIZED CREATURE</color>");
			return;
		}
		case "nuke":
		{
			string chunkString6 = ChunkControl.Instance.GetChunkString(GameController.Instance.player.transform.position);
			if (!ChunkControl.Instance.ChunkExists(chunkString6))
			{
				break;
			}
			ChunkData chunkData = ChunkControl.Instance.GetChunkData(chunkString6);
			List<nuke_element> list = new List<nuke_element>();
			foreach (KeyValuePair<string, List<ChunkElement>> chunk_element in chunkData.chunk_elements)
			{
				int innerX = int.Parse(chunk_element.Key[0].ToString());
				int innerZ = int.Parse(chunk_element.Key[2].ToString());
				foreach (ChunkElement item5 in chunk_element.Value)
				{
					list.Add(new nuke_element
					{
						innerX = innerX,
						innerZ = innerZ,
						element = item5
					});
				}
			}
			foreach (nuke_element item6 in list)
			{
				ConstructionControl.Instance.PlayerRemoveAt(item6.element, ChunkControl.Instance.player_zone, chunkData.X, chunkData.Z, item6.innerX, item6.innerZ, ConstructionControl.remove_context.self_remove, ConstructionControl.GenerateCacheKey());
			}
			break;
		}
		case "level_up":
			BreedControl.Instance.GainExtraLevels(6);
			ShowNotif("<color=#00ff00>ENJOY YOUR LEVELS</color>");
			return;
		case "gfx":
			if (SceneManager.GetActiveScene().name != "Game")
			{
				ShowNotif("CANNOT TEST ITEMS IN MENU");
				return;
			}
			modify_gfx_obj = text.Substring(num + 1, text.Length - (num + 1));
			if (System.IO.File.Exists(Application.dataPath + "/SYNCHRONOUS/TextFiles/InventoryItems/" + modify_gfx_obj + ".txt"))
			{
				ShowNotif("<color=#00ff00>MODIFYING: '" + modify_gfx_obj + "'</color>");
			}
			else
			{
				ShowNotif("<color=#ff0000>ITEM '" + modify_gfx_obj + "' DOES NOT EXIST</color>");
			}
			return;
		}
		ShowNotif("error: '" + text + "' is an unknown command");
	}

	private void Update()
	{
		if (modify_gfx_obj != "")
		{
			if (GamepadInput.Instance.GetKeyDown(Key.Home))
			{
				ModifyItemValue("model3d_camDist", 0.1f, 2.5f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.End))
			{
				ModifyItemValue("model3d_camDist", -0.1f, 2.5f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.PageDown))
			{
				ModifyItemValue("model3d_fov", -2.5f, 60f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.PageUp))
			{
				ModifyItemValue("model3d_fov", 2.5f, 60f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.NumpadMinus))
			{
				ModifyItemValue("model3d_camHeight", 0.1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.NumpadPlus))
			{
				ModifyItemValue("model3d_camHeight", -0.1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.LeftArrow))
			{
				ModifyItemValue("model3d_xRot", -5f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.RightArrow))
			{
				ModifyItemValue("model3d_xRot", 5f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.UpArrow))
			{
				ModifyItemValue("model3d_yRot", -5f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.DownArrow))
			{
				ModifyItemValue("model3d_yRot", 5f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.Numpad6))
			{
				ModifyItemValue("model3d_recenterX", -1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.Numpad4))
			{
				ModifyItemValue("model3d_recenterX", 1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.Numpad2))
			{
				ModifyItemValue("model3d_recenterY", 1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.Numpad8))
			{
				ModifyItemValue("model3d_recenterY", -1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.NumpadMultiply))
			{
				ModifyItemValue("model3d_objRot", 1f, 0f);
			}
		}
		if (GamepadInput.Instance.GetKeyDown(Key.Backquote) && Application.isEditor)
		{
			PressConsoleButton();
		}
		if (GamepadInput.Instance.GetKeyDown(Key.UpArrow) && modify_gfx_obj == "" && console_open && prev_command_index + 1 < previous_commands.Count)
		{
			prev_command_index++;
			input.gameObject.SetActive(false);
			input.SetTextWithoutNotify(previous_commands[prev_command_index]);
			input.gameObject.SetActive(true);
		}
		if (GamepadInput.Instance.GetKeyDown(Key.DownArrow) && modify_gfx_obj == "" && console_open && prev_command_index - 1 >= -1)
		{
			prev_command_index--;
			if (prev_command_index == -1)
			{
				input.SetTextWithoutNotify("");
				input.ActivateInputField();
			}
			else
			{
				input.gameObject.SetActive(false);
				input.SetTextWithoutNotify(previous_commands[prev_command_index]);
				input.gameObject.SetActive(true);
			}
		}
		if (GamepadInput.Instance.GetKeyDown(Key.Enter) && console_open)
		{
			OnPressSubmit();
		}
	}

	public void GiveDevBook(string book_name)
	{
		ExtraInventoryData extra_data = new ExtraInventoryData();
		inventory_ctr.Instance.FillOutBookData(book_name, ref extra_data);
		InventoryItem item = new InventoryItem("Book", extra_data);
		inventory_ctr.Instance.GiveItem(item, 1, "");
	}

	private void Save(string path, ForcedChunkData forced_chunk_data)
	{
		string[] array = new string[0];
		if (System.IO.File.Exists(path))
		{
			array = System.IO.File.ReadAllLines(path);
		}
		List<string> list = new List<string>();
		bool flag = false;
		foreach (string text in array)
		{
			if (!flag && !Startup.StringNullOrWhitespace(text) && text[0] == '[')
			{
				flag = true;
			}
			if (flag)
			{
				list.Add(text);
			}
		}
		list.Insert(0, "");
		if (forced_chunk_data.forced_biome != -1)
		{
			string item = "*forced_biome=" + forced_chunk_data.forced_biome + "*";
			if (list.Count < 1)
			{
				list.Add(item);
			}
			else
			{
				list.Insert(0, item);
			}
		}
		if (forced_chunk_data.is_blank)
		{
			if (list.Count < 1)
			{
				list.Add("*is_blank=true*");
			}
			else
			{
				list.Insert(0, "*is_blank=true*");
			}
		}
		if (forced_chunk_data.forced_floor_model != -1)
		{
			string item2 = "*forced_floor_model=" + forced_chunk_data.forced_floor_model + "*";
			if (list.Count < 1)
			{
				list.Add(item2);
			}
			else
			{
				list.Insert(0, item2);
			}
		}
		if (forced_chunk_data.forced_floor_rot != -1)
		{
			string item3 = "*forced_floor_rot=" + forced_chunk_data.forced_floor_rot + "*";
			if (list.Count < 1)
			{
				list.Add(item3);
			}
			else
			{
				list.Insert(0, item3);
			}
		}
		if (forced_chunk_data.caveart_index != -1)
		{
			string item4 = "*caveart_index=" + forced_chunk_data.caveart_index + "*";
			if (list.Count < 1)
			{
				list.Add(item4);
			}
			else
			{
				list.Insert(0, item4);
			}
		}
		if (forced_chunk_data.caveart_rot != -1)
		{
			string item5 = "*caveart_rot=" + forced_chunk_data.caveart_rot + "*";
			if (list.Count < 1)
			{
				list.Add(item5);
			}
			else
			{
				list.Insert(0, item5);
			}
		}
		System.IO.File.WriteAllLines(path, list.ToArray());
	}

	private void CloseConsole()
	{
		console_open = false;
		console_parent_obj.SetActive(false);
	}

	public void SetLevel(int lvl)
	{
		GameController.Instance.OverwritePlayerLevel(lvl, "");
		GameController.Instance.SavePlayerLevelToDisk();
		GameplayGUIControl.Instance.text_playerLevel.text = "Level " + lvl;
		int num = lvl / GameController.n_stats;
		for (int i = 0; i < GameController.n_stats; i++)
		{
			GameController.Instance.player_stats[i] = num;
		}
		for (int j = 0; j < lvl - GameController.n_stats * num; j++)
		{
			GameController.Instance.player_stats[UnityEngine.Random.Range(0, GameController.n_stats)]++;
		}
		GameController.Instance.SaveAllStatsToDisk();
		GameplayGUIControl.Instance.RedrawAllStatNibs();
		GameController.Instance.player.GetComponent<SharedCreature>().ReCalcHpMaxAndHpRegen();
		GameController.Instance.player.GetComponent<Combatant>().RefillHP();
		GameServerSender.Instance.SendUpdateCreatureStats("LOCAL", "");
		GameController.Instance.skillPointsSpendable = 0;
		GameController.Instance.SaveSkillPointsSpendableToDisk();
		GameplayGUIControl.Instance.RedrawSkillPointsSpendableText();
		int num2 = UnityEngine.Random.Range(0, GameController.Instance.NextLevelExp(lvl));
		GameController.Instance.currentEXP = num2;
		GameController.Instance.visualEXP = num2;
		GameController.Instance.SaveCurrentExpToDisk();
		GameController.Instance.animate_exp_bar = true;
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("AutoGen/(Auto Gen) Perks List", ref file_exists);
		if (file_exists)
		{
			List<string> list = new List<string>();
			foreach (string item in textFileLines)
			{
				if (!Startup.StringNullOrWhitespace(item) && !PerkControl.Instance.GetPerkDataForInfoDisplay(item).not_unlockable)
				{
					PerkControl.Instance.OverwritePerkLevel(item, 0);
					PerkControl.Instance.SavePerkLevel(item);
					list.Add(item);
				}
			}
			int num3 = lvl / 6;
			if (lvl > 5)
			{
				bool flag;
				do
				{
					flag = false;
					foreach (string item2 in list)
					{
						PerkData perkDataForInfoDisplay = PerkControl.Instance.GetPerkDataForInfoDisplay(item2);
						int perkLevel = PerkControl.Instance.GetPerkLevel(item2);
						if ((perkDataForInfoDisplay.max_level != -1 && perkLevel >= perkDataForInfoDisplay.max_level) || !PerkControl.Instance.HasPrerequisiteForNextLevel(perkDataForInfoDisplay))
						{
							continue;
						}
						int num4 = PerkControl.Instance.GenomesForNextLevel(perkLevel, perkDataForInfoDisplay);
						if (num4 <= num3)
						{
							num3 -= num4;
							PerkControl.Instance.OverwritePerkLevel(item2, perkLevel + 1);
							PerkControl.Instance.SavePerkLevel(item2);
							flag = true;
							if (num3 == 0)
							{
								break;
							}
						}
					}
				}
				while (flag && num3 > 0);
			}
			PerkControl.Instance.genomes = num3;
			PerkControl.Instance.SaveGenomes();
		}
		PerkControl.Instance.RedrawEquippedPerkSlots();
	}

	public void PressConsoleButton()
	{
		if (console_open)
		{
			CloseConsole();
			return;
		}
		console_open = true;
		console_parent_obj.SetActive(true);
		main_input_obj.SetActive(true);
		error_notif_obj.SetActive(false);
		prev_command_index = -1;
		input.SetTextWithoutNotify("");
		input.ActivateInputField();
	}

	private void SaveCustomItemMesh(Mesh mesh, ExtraInventoryData extra_data)
	{
		extra_data.SetLong("n_mesh_verts", mesh.vertices.Length);
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			Vector3 vector = mesh.vertices[i];
			extra_data.SetLong("mesh_vX" + i, (int)(vector.x * 100f));
			extra_data.SetLong("mesh_vY" + i, (int)(vector.y * 100f));
			extra_data.SetLong("mesh_vZ" + i, (int)(vector.z * 100f));
		}
		extra_data.SetLong("n_mesh_tris", mesh.triangles.Length);
		for (int j = 0; j < mesh.triangles.Length; j++)
		{
			extra_data.SetLong("mesh_tri" + j, mesh.triangles[j]);
		}
		extra_data.SetLong("n_mesh_normals", mesh.normals.Length);
		for (int k = 0; k < mesh.normals.Length; k++)
		{
			Vector3 vector2 = mesh.normals[k];
			extra_data.SetLong("mesh_nX" + k, (int)(vector2.x * 100f));
			extra_data.SetLong("mesh_nY" + k, (int)(vector2.y * 100f));
			extra_data.SetLong("mesh_nZ" + k, (int)(vector2.z * 100f));
		}
		extra_data.SetLong("n_mesh_uvs", mesh.uv.Length);
		for (int l = 0; l < mesh.uv.Length; l++)
		{
			Vector2 vector3 = mesh.uv[l];
			extra_data.SetLong("mesh_uX" + l, (int)(vector3.x * 100f));
			extra_data.SetLong("mesh_uY" + l, (int)(vector3.y * 100f));
		}
	}

	private void SaveCustomGraphic(Texture2D canvas, ExtraInventoryData extra_data, string counter, string prefix)
	{
		byte[] array = canvas.EncodeToPNG();
		extra_data.SetLong(counter, array.Length);
		int num = 0;
		byte[] array2 = new byte[4];
		int num2 = 0;
		for (int i = 0; i < array.Length; i++)
		{
			array2[num2] = array[i];
			num2++;
			if (num2 == 4)
			{
				extra_data.SetLong(prefix + num, System.BitConverter.ToInt32(array2, 0));
				array2 = new byte[4];
				num2 = 0;
				num++;
			}
		}
		if (array.Length != 0 && num2 != 0)
		{
			extra_data.SetLong(prefix + num, System.BitConverter.ToInt32(array2, 0));
		}
	}

	private void ShowNotif(string str)
	{
		error_notif_obj.SetActive(true);
		error_notif_obj.transform.Find("Text").gameObject.GetComponent<Text>().text = str;
	}
}
