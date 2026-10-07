using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class QuestControl : MonoBehaviour, OrderedStart
{
	public enum quest_status
	{
		not_started = 0,
		in_progress = 1,
		complete = 2
	}

	public struct parsed_position
	{
		public string zone;

		public int chunkX;

		public int chunkZ;

		public int innerX;

		public int innerZ;
	}

	public enum sell_quest_item_result
	{
		fail = 0,
		new_collected = 1,
		all_collected = 2
	}

	public static QuestControl Instance;

	// Changed so it works on any computer. The commented-out line below is how it looked on the developer's machine.
	public static string quest_cache_path = Directory.CreateDirectory("Hybrid Animals Stuff/quest_scenics_CACHE").FullName;
	// public static string quest_cache_path = "C:\\Hybrid Animals Stuff\\quest_scenics_CACHE";

	public Dictionary<string, Quest> loaded_quests = new Dictionary<string, Quest>();

	public GameObject quest_nib_prefab;

	private List<GameObject> instantiated_quest_nibs = new List<GameObject>();

	public string show_quest_complete_popup_after = "";

	private List<string> quest_names = new List<string>();

	public string quest_notif_on_exit_dialogue = "";

	public bool doing_time_trial;

	public bool redraw_companions_on_scene_change;

	public List<string> killGoal_mobs_killed = new List<string>();

	public string quest_perk_reapply_key = "";

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
		if (this != Instance) return;
		quest_names.Add("Colors of the Sea");
		quest_names.Add("The Bandit King");
		quest_names.Add("The Fossil Record");
		quest_names.Add("Critter Crush");
		quest_names.Add("Dark and Forgotten Magic");
		TryRecacheQuests();
		RevertQuestIfNecessary();
	}

	public static void ScanQuestDataForChanges(string[] cached_files_full, string[] game_files_full)
	{
		List<string> deleted = new List<string>();
		foreach (string file in cached_files_full) deleted.Add(Path.GetFileNameWithoutExtension(file));
		foreach (string file in game_files_full)
		{
			if (Path.GetExtension(file) == ".meta") continue;
			string name = Path.GetFileNameWithoutExtension(file);
			if (name == "(Auto Gen) npc_home_data") continue;
			deleted.Remove(name);
			List<string> lines = new List<string>(File.ReadAllLines(file));
			int version_index = -1;
			for (int i = 0; i < lines.Count; i++)
			{
				if (!lines[i].Contains("auto_version")) continue;
				version_index = i;
				break;
			}
			string cached = quest_cache_path + Path.DirectorySeparatorChar + name + ".txt";
			if (version_index == -1)
			{
				File.WriteAllLines(cached, lines);
				lines.Insert(0, "*auto_version=1*");
				File.WriteAllLines(file, lines);
				Debug.Log("New file detected '" + name + "'");
				continue;
			}
			string version_line = lines[version_index];
			int version = int.Parse(version_line.Substring(14, version_line.Length - 15), Startup.parse_culture);
			lines.RemoveAt(version_index);
			if (!File.Exists(cached))
			{
				File.WriteAllLines(cached, lines);
				Debug.Log("New file detected '" + name + "'");
				continue;
			}
			List<string> old = new List<string>(File.ReadAllLines(cached));
			bool changed = old.Count != lines.Count;
			if (!changed)
			{
				for (int i = 0; i < old.Count; i++)
				{
					if (old[i] == lines[i]) continue;
					changed = true;
					break;
				}
			}
			if (!changed) continue;
			File.WriteAllLines(cached, lines);
			lines.Insert(0, "*auto_version=" + (version + 1) + "*");
			File.WriteAllLines(file, lines);
			Debug.Log("Change detected in '" + name + "'");
		}
		foreach (string name in deleted)
		{
			File.Delete(quest_cache_path + Path.DirectorySeparatorChar + name + ".txt");
			Debug.Log("File deletion detected '" + name + "'");
		}
	}

	public Quest GetQuest(string quest_name)
	{
		if (loaded_quests.ContainsKey(quest_name)) return loaded_quests[quest_name];
		Quest quest = ResourceControl.Instance.LoadQuest(quest_name);
		loaded_quests.Add(quest_name, quest);
		return quest;
	}

	public void RevertQuestIfNecessary()
	{
		string slotString = PlayerData.Instance.GetSlotString("revert_quest_name", PlayerData.filename_t.global_quest_data);
		short slotShort = PlayerData.Instance.GetSlotShort("revert_quest_to_step", PlayerData.filename_t.global_quest_data);
		if (slotString != "")
		{
			if (doing_time_trial)
			{
				WindowPrefabsControl.Instance.DestroyScreen("QUEST COUNTDOWN");
				WindowPrefabsControl.Instance.DestroyScreen("QUEST KILL COUNT");
				doing_time_trial = false;
			}
			SetQuestProgress(slotString, slotShort);
			PlayerData.Instance.SetSlotString("revert_quest_name", "", PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotShort("revert_quest_to_step", 0, PlayerData.filename_t.global_quest_data);
		}
	}

	public void OpenQuestWindow()
	{
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.quests_and_achieves);
		WindowControl.Instance.VisuallySelectLeftMiniwindowTab();
		OnLeftMiniwindowTabPressed();
	}

	public void OnLeftMiniwindowTabPressed()
	{
		inventory_ctr.Instance.HideCraftingTab();
		WindowPrefabsControl.Instance.CreateScreen("QUESTS", WindowPrefabsControl.build_into_t.mini_window);
		for (int i = 0; i < quest_names.Count; i++)
		{
			string quest_name = quest_names[i];
			GetQuestProgress(quest_name);
			string cached_status = PlayerData.Instance.GetSlotString(quest_name + "_cached_status", PlayerData.filename_t.global_quest_data);
			string cached_translation = PlayerData.Instance.GetSlotString(quest_name + "_cached_translation", PlayerData.filename_t.global_quest_data);
			quest_status status;
			if (cached_status == "complete")
			{
				status = quest_status.complete;
			}
			else if (cached_status == "not started")
			{
				status = quest_status.not_started;
			}
			else
			{
				status = quest_status.in_progress;
			}
			CreateQuestNib(quest_name, cached_translation, status, i);
		}
		Scrollable scrollable = WindowPrefabsControl.Instance.GetScreen("QUESTS").GetComponent<Scrollable>();
		scrollable.SetScrollAreaMaxY(instantiated_quest_nibs.Count < 4 ? 70f : instantiated_quest_nibs.Count * 114 - 342);
	}

	private void TryRecacheQuests()
	{
		string cached = PlayerData.Instance.GetSlotString("cached_language", PlayerData.filename_t.global_quest_data);
		string language = "";
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.English: language = "English"; break;
		case TranslationControl.languages.Russian: language = "Russian"; break;
		case TranslationControl.languages.Portuguese: language = "Portuguese"; break;
		case TranslationControl.languages.Indonesian: language = "Indonesian"; break;
		case TranslationControl.languages.Spanish: language = "Spanish"; break;
		case TranslationControl.languages.Thai: language = "Thai"; break;
		}
		if (cached == language) return;
		for (int i = 0; i < quest_names.Count; i++)
		{
			string name = quest_names[i];
			int progress = GetQuestProgress(name);
			Quest quest = GetQuest(name);
			string status = progress == quest.steps.Count ? "complete" : progress == 0 ? "not started" : "in progress";
			PlayerData.Instance.SetSlotString(name + "_cached_status", status, PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotString(name + "_cached_translation", quest.display_name, PlayerData.filename_t.global_quest_data);
		}
		PlayerData.Instance.SetSlotString("cached_language", language, PlayerData.filename_t.global_quest_data);
	}

	private void TryAddTempCompanions(string quest_name, int progress)
	{
		if (CompanionController.Instance.active_companions.Count != 0) return;
		Dictionary<string, string> step = GetQuest(quest_name).steps[progress];
		int count = int.Parse(step["n_TempCompanions"], Startup.parse_culture);
		for (int i = 0; i < count; i++)
		{
			string prefix = "TempCompanion" + i;
			string name = step[prefix + "_name"];
			string creatureA = step[prefix + "_creatureA"];
			string creatureB = step[prefix + "_creatureB"];
			float scale = float.Parse(step[prefix + "_levelScale"], Startup.parse_culture);
			string hat = step[prefix + "_hat"];
			string body = step[prefix + "_body"];
			string hand = step[prefix + "_hand"];
			string hat_paint = step.ContainsKey(prefix + "_hat_paint") ? step[prefix + "_hat_paint"] : "";
			string body_paint = step.ContainsKey(prefix + "_armor_paint") ? step[prefix + "_armor_paint"] : "";
			string hand_paint = step.ContainsKey(prefix + "_hand_paint") ? step[prefix + "_hand_paint"] : "";
			ExtraInventoryData hat_data = new ExtraInventoryData();
			hat_data.SetString("paint", hat_paint);
			InventoryItem hat_item = new InventoryItem(hat, hat_data);
			ExtraInventoryData body_data = new ExtraInventoryData();
			body_data.SetString("paint", body_paint);
			InventoryItem body_item = new InventoryItem(body, body_data);
			ExtraInventoryData hand_data = new ExtraInventoryData();
			hand_data.SetString("paint", hand_paint);
			InventoryItem hand_item = new InventoryItem(hand, hand_data);
			CompanionController.Instance.AddTempCompanion(creatureA, creatureB, (int)(scale * GameController.Instance.playerLevel), name, hat_item, body_item, hand_item);
		}
	}

	public void CloseQuestScreen()
	{
		WindowPrefabsControl.Instance.DestroyScreen("QUESTS");
		instantiated_quest_nibs.Clear();
	}

	public int GetQuestProgress(string quest_name)
	{
		return PlayerData.Instance.GetSlotShort(quest_name + "_progress", PlayerData.filename_t.global_quest_data);
	}

	private void TryDestroyTempCompanions(Dictionary<string, string> step)
	{
		if (step.ContainsKey("keep_TempCompanions") && step["keep_TempCompanions"] == "true") return;
		CompanionController.Instance.DestroyTempCompanions();
	}

	public void SetQuestProgress(string quest_name, int set_progress_to, bool on_revert = false, bool on_teleport_finished = false)
	{
		Quest quest = GetQuest(quest_name);
		if (set_progress_to >= quest.steps.Count)
		{
			PlayerData.Instance.SetSlotShort(quest_name + "_progress", set_progress_to, PlayerData.filename_t.global_quest_data);
			CompanionController.Instance.DestroyTempCompanions();
			killGoal_mobs_killed.Clear();
			PlayerData.Instance.SetSlotString(quest_name + "_cached_status", "complete", PlayerData.filename_t.global_quest_data);
			quest_notif_on_exit_dialogue = "";
			show_quest_complete_popup_after = quest_name;
			if (quest.repeat_hours == -1) return;
			DateTime restart = DateTime.UtcNow.AddHours(quest.repeat_hours);
			PlayerData.Instance.SetSlotShort(quest_name + "_restart_dateSet", 1, PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotShort(quest_name + "_restart_second", restart.Second, PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotShort(quest_name + "_restart_minute", restart.Minute, PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotShort(quest_name + "_restart_hour", restart.Hour, PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotShort(quest_name + "_restart_day", restart.Day, PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotShort(quest_name + "_restart_month", restart.Month, PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotShort(quest_name + "_restart_year", restart.Year, PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotShort(quest_name + "_completions", PlayerData.Instance.GetSlotShort(quest_name + "_completions", PlayerData.filename_t.global_quest_data) + 1, PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotString("revert_quest_name", "", PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotShort("revert_quest_to_step", 0, PlayerData.filename_t.global_quest_data);
			return;
		}
		Dictionary<string, string> step = quest.steps[set_progress_to];
		if (step.ContainsKey("Instantly teleport to") && !on_teleport_finished)
		{
			TransitionControl.Instance.BeginQuestProgressionTransition(quest_name, set_progress_to);
			return;
		}
		int previous = GetQuestProgress(quest_name);
		PlayerData.Instance.SetSlotShort(quest_name + "_progress", set_progress_to, PlayerData.filename_t.global_quest_data);
		TryDestroyTempCompanions(step);
		killGoal_mobs_killed.Clear();
		Sprite icon = DevBuildControl.Instance.quest_updated_ico;
		string notification = step.ContainsKey("Override_notif_text") ? step["Override_notif_text"] : previous == 0 ? "Quest Started!" : "Quest Updated!";
		PlayerData.Instance.SetSlotString(quest_name + "_cached_status", set_progress_to == 0 ? "not started" : "in progress", PlayerData.filename_t.global_quest_data);
		if (WindowControl.Instance.curr_window == WindowControl.window_type_t.dialogue) quest_notif_on_exit_dialogue = notification;
		else GameplayGUIControl.Instance.ShowNotif(notification, icon, new OnNotifClick(OnNotifClick.type.quests));
		if (GetStepType(quest_name, set_progress_to) == "Collect Items") PickItemsToCollect(quest_name, set_progress_to);
		ChunkControl.Instance.AppearQuestMobs(quest_name, set_progress_to);
		if (step.ContainsKey("On logout revert to step"))
		{
			int revert = int.Parse(step["On logout revert to step"], Startup.parse_culture);
			PlayerData.Instance.SetSlotString("revert_quest_name", quest_name, PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotShort("revert_quest_to_step", revert, PlayerData.filename_t.global_quest_data);
		}
		else
		{
			PlayerData.Instance.SetSlotString("revert_quest_name", "", PlayerData.filename_t.global_quest_data);
			PlayerData.Instance.SetSlotShort("revert_quest_to_step", 0, PlayerData.filename_t.global_quest_data);
		}
		if (step.ContainsKey("Time limit"))
		{
			doing_time_trial = true;
			int limit = int.Parse(step["Time limit"], Startup.parse_culture);
			int kills = int.Parse(step["Goal kills"], Startup.parse_culture);
			WindowPrefabsControl.Instance.CreateScreen("QUEST COUNTDOWN", WindowPrefabsControl.build_into_t.GAME_CTR);
			WindowPrefabsControl.Instance.GetScreen("QUEST COUNTDOWN").GetComponent<QuestCountdown>().Init();
			WindowPrefabsControl.Instance.GetScreen("QUEST COUNTDOWN").GetComponent<QuestCountdown>().StartQuestTimerCountDown(limit, kills, quest_name, set_progress_to, 4, 3);
		}
		else doing_time_trial = false;
		CompanionController.Instance.max_personal_companions_right_now = step.ContainsKey("Disable companions") && step["Disable companions"] == "true" ? 0 : 2;
		if (step.ContainsKey("n_TempCompanions")) TryAddTempCompanions(quest_name, set_progress_to);
		if (quest_perk_reapply_key != "")
		{
			if (GameController.Instance.player != null) GameController.Instance.player.GetComponent<PerkReceiver>().RemovePerk(quest_perk_reapply_key);
			quest_perk_reapply_key = "";
		}
		if (step.ContainsKey("Apply perk to player"))
		{
			string key = step["Apply perk to player"];
			PerkData perk = PerkControl.Instance.ClonePerkForCasting(key, GameController.Instance.player);
			PerkControl.Instance.ApplyInitialCastOnto(perk, 1, "LOCAL", 1, GameController.Instance.player);
			quest_perk_reapply_key = key;
		}
		if (!step.ContainsKey("continue_soundtrack") || step["continue_soundtrack"] != "true")
		{
			if (step.ContainsKey("Soundtrack"))
			{
				if (!Instance.doing_time_trial) AudioControl.Instance.PlayBattleMusic(step["Soundtrack"]);
				else AudioControl.Instance.PauseBackgroundMusic();
			}
			else AudioControl.Instance.EndBattleMusic();
		}
		if (step.ContainsKey("n_Battle_Containers"))
		{
			int containers = int.Parse(step["n_Battle_Containers"], Startup.parse_culture);
			for (int i = 0; i < containers; i++)
			{
				BasketContents contents = new BasketContents();
				int count = UnityEngine.Random.Range(1, 3);
				for (int j = 0; j < count; j++)
				{
					float roll = UnityEngine.Random.value;
					string item = "Bones";
					int quantity = 1;
					if (roll >= 0.333f)
					{
						float second = UnityEngine.Random.value;
						if (roll >= 0.8f) item = second < 0.66f ? "Jam" : "Super Jam";
						else
						{
							item = second < 0.333 ? "Toasted Nuts" : second < 0.666 ? "Berry" : "Brown Mushroom";
							quantity = UnityEngine.Random.Range(1, 3);
						}
					}
					contents[j] = new ItemCountPair(item, quantity);
				}
				int container = int.Parse(step["BattleContainer" + i], Startup.parse_culture);
				contents.SaveToAllAsContainer(container);
				PlayerData.Instance.SetSlotShort("NPCchest_" + container, 1, PlayerData.filename_t.generated_NPC_chests);
			}
		}
	}

	private void CreateQuestNib(string quest_key, string quest_display_name, quest_status status, int i)
	{
		GameObject nib = Instantiate(quest_nib_prefab);
		nib.transform.SetParent(WindowPrefabsControl.Instance.GetScreen("QUESTS").GetComponent<Scrollable>().scrollwheel_parent.transform);
		nib.transform.localScale = Vector3.one;
		nib.transform.localRotation = Quaternion.identity;
		nib.transform.localPosition = new Vector3(0f, 263 - i * 114, 0f);
		nib.GetComponent<QuestNib>().index = i;
		nib.transform.Find("quest name").GetComponent<Text>().text = quest_display_name;
		if (status == quest_status.complete)
		{
			int count = PlayerData.Instance.GetSlotShort(quest_key + "_completions", PlayerData.filename_t.global_quest_data);
			string completed = TranslationControl.Instance.TranslateGeneral("COMPLETED", "GUI");
			nib.transform.Find("quest progress").GetComponent<Text>().text = count < 2 ? completed : completed + " (x" + count + ")";
			nib.transform.Find("quest progress").GetComponent<Text>().color = new Color(0.27f, 1f, 0.25f, 1f);
			nib.transform.Find("background").GetComponent<Image>().color = new Color(0.47f, 0.59f, 0.44f, 0.85f);
			nib.transform.Find("icon inner").GetComponent<Image>().color = new Color(0.27f, 1f, 0.25f, 1f);
			nib.transform.Find("quest name").GetComponent<Text>().color = Color.white;
			if (IsQuestReadyToRepeat(quest_key)) nib.transform.Find("repeat star").gameObject.SetActive(true);
		}
		else if (status == quest_status.in_progress)
		{
			WindowControl.miniwindow_layout layout = WindowControl.Instance.GetMiniwindowLayout("achieves");
			nib.transform.Find("quest progress").GetComponent<Text>().text = TranslationControl.Instance.TranslateGeneral("In Progress", "GUI");
			nib.transform.Find("quest progress").GetComponent<Text>().color = layout.header_L_col;
			nib.transform.Find("background").GetComponent<Image>().color = new Color(0.5f, 0.47f, 0.32f, 0.8f);
			nib.transform.Find("icon inner").GetComponent<Image>().color = layout.header_L_col;
			nib.transform.Find("quest name").GetComponent<Text>().color = new Color(1f, 1f, 0.25f, 1f);
		}
		else if (status == quest_status.not_started)
		{
			nib.transform.Find("quest progress").GetComponent<Text>().text = TranslationControl.Instance.TranslateGeneral("Not Started", "GUI");
			nib.transform.Find("quest progress").GetComponent<Text>().color = new Color(1f, 0.697f, 0.254f, 1f);
		}
		instantiated_quest_nibs.Add(nib);
	}

	public void SucceedClickQuestNib(int index)
	{
		string name = quest_names[index];
		Quest quest = GetQuest(name);
		int progress = GetQuestProgress(name);
		if (progress == quest.steps.Count)
		{
			ShowQuestCompletePopup(name);
			return;
		}
		Dictionary<string, string> step = quest.steps[progress];
		string hint = GetStepHint(name, progress);
		string type = GetStepType(name, progress);
		if (type == "Collect Items")
		{
			List<InventoryItem> remaining = GetCollectablesRemaining(name, progress);
			if (hint.Contains("[ITEMS]"))
			{
				string items = "";
				for (int i = 0; i < remaining.Count; i++) items += (i == 0 ? "a " : i == remaining.Count - 1 ? ", and a " : ", a ") + remaining[i].item_name;
				hint = hint.Replace("[ITEMS]", items);
			}
			else if (hint.Contains("[N_ITEMS]")) hint = hint.Replace("[N_ITEMS]", remaining.Count.ToString() ?? "");
			PopupControl.Instance.ShowMessage("<color=#999999>" + quest.display_name + "</color>\n" + hint, PopupControl.context.message, remaining);
			return;
		}
		if (type == "Kill All Mobs" || type == "Kill As Many Mobs As Possible")
		{
			if (hint.Contains("[N_MOBS]")) hint = hint.Replace("[N_MOBS]", (int.Parse(step["Goal kills"], Startup.parse_culture) - killGoal_mobs_killed.Count).ToString() ?? "");
			else if (hint.Contains("[GOAL_KILLS]")) hint = hint.Replace("[GOAL_KILLS]", int.Parse(step["Goal kills"], Startup.parse_culture).ToString() ?? "");
		}
		else if (type == "Collect Identical Items" && step.ContainsKey("N_identical_collect") && int.Parse(step["N_identical_collect"], Startup.parse_culture) == 2 && step.ContainsKey("CollectIdenticalItem"))
		{
			List<InventoryItem> items = new List<InventoryItem>();
			items.Add(new InventoryItem(step["CollectIdenticalItem"]));
			items.Add(new InventoryItem("DEBUG-quest-header-equals"));
			items.Add(new InventoryItem(step["CollectIdenticalItem"]));
			PopupControl.Instance.ShowMessage("<color=#999999>" + quest.display_name + "</color>\n" + hint, PopupControl.context.message, items);
			return;
		}
		PopupControl.Instance.ShowMessage("<color=#999999>" + quest.display_name + "</color>\n" + hint, PopupControl.context.message);
	}

	public void ShowQuestCompletePopup(string quest_name)
	{
		Quest quest = GetQuest(quest_name);
		bool date_set = PlayerData.Instance.GetSlotShort(quest_name + "_restart_dateSet", PlayerData.filename_t.global_quest_data) == 1;
		bool ready = date_set && IsQuestReadyToRepeat(quest_name);
		string text = "<color=#aaaaaa>" + quest.display_name + "</color>\n<i><size=31><color=#24ff53>" + TranslationControl.Instance.TranslateGeneral("Quest Completed!", "GUI") + "</color></size></i>\n\n" + quest.complete_text;
		if (date_set)
		{
			if (ready) text += "\n<size=17><color=#fff533>(!) " + TranslationControl.Instance.TranslateGeneral("You can do this quest again for a new reward!", "GUI") + "</color></size>";
			else text += "\n<size=16><color=#aaaaaa>" + TranslationControl.Instance.TranslateGeneral("You can do this quest again for a new reward in:", "GUI") + " </color></size><color=#fff533>" + GetRepeatTimeRemainingString(quest_name) + "</color>";
		}
		PopupControl.Instance.ShowMessage(text, PopupControl.context.message);
	}

	public bool IsQuestReadyToRepeat(string quest_name)
	{
		int second = PlayerData.Instance.GetSlotShort(quest_name + "_restart_second", PlayerData.filename_t.global_quest_data);
		int minute = PlayerData.Instance.GetSlotShort(quest_name + "_restart_minute", PlayerData.filename_t.global_quest_data);
		int hour = PlayerData.Instance.GetSlotShort(quest_name + "_restart_hour", PlayerData.filename_t.global_quest_data);
		int day = PlayerData.Instance.GetSlotShort(quest_name + "_restart_day", PlayerData.filename_t.global_quest_data);
		int month = PlayerData.Instance.GetSlotShort(quest_name + "_restart_month", PlayerData.filename_t.global_quest_data);
		int year = PlayerData.Instance.GetSlotShort(quest_name + "_restart_year", PlayerData.filename_t.global_quest_data);
		if (second == 0 && minute == 0 && hour == 0 && day == 0 && month == 0 && year == 0) return false;
		DateTime restart = new DateTime(year, month, day, hour, minute, second);
		if ((restart - DateTime.UtcNow).TotalSeconds >= 0.0) return false;
		Quest quest = GetQuest(quest_name);
		return GetQuestProgress(quest_name) == quest.steps.Count;
	}

	private string GetRepeatTimeRemainingString(string quest_name)
	{
		int second = PlayerData.Instance.GetSlotShort(quest_name + "_restart_second", PlayerData.filename_t.global_quest_data);
		int minute = PlayerData.Instance.GetSlotShort(quest_name + "_restart_minute", PlayerData.filename_t.global_quest_data);
		int hour = PlayerData.Instance.GetSlotShort(quest_name + "_restart_hour", PlayerData.filename_t.global_quest_data);
		int day = PlayerData.Instance.GetSlotShort(quest_name + "_restart_day", PlayerData.filename_t.global_quest_data);
		int month = PlayerData.Instance.GetSlotShort(quest_name + "_restart_month", PlayerData.filename_t.global_quest_data);
		int year = PlayerData.Instance.GetSlotShort(quest_name + "_restart_year", PlayerData.filename_t.global_quest_data);
		if (second == 0 && minute == 0 && hour == 0 && day == 0 && month == 0 && year == 0) return "???";
		DateTime restart = new DateTime(year, month, day, hour, minute, second);
		int days = (int)(restart - DateTime.UtcNow).TotalDays;
		int hours = (int)(restart - DateTime.UtcNow).TotalHours - days * 24;
		int minutes = (int)(restart - DateTime.UtcNow).TotalMinutes - days * 1440 - hours * 60;
		int seconds = (int)(restart - DateTime.UtcNow).TotalSeconds - days * 86400 - hours * 3600 - minutes * 60;
		if (days >= 2) return days + " days";
		if (days == 1) return "1 day, " + hours + " hours";
		if (hours >= 2) return hours + " hours";
		if (hours == 1) return "1 hour";
		if (minutes >= 1) return minutes + " minutes";
		return seconds + " seconds";
	}

	private string GetStepHint(string quest_name, int progress)
	{
		Dictionary<string, string> step = GetQuest(quest_name).steps[progress];
		return step.ContainsKey("Hint") ? step["Hint"] : "ERROR: NO HINT FOUND";
	}

	private string GetStepType(string quest_name, int progress)
	{
		Dictionary<string, string> step = GetQuest(quest_name).steps[progress];
		return step.ContainsKey("Step type") ? step["Step type"] : "";
	}

	public void TryNoteQuestMobKilled(string zone, int chunkX, int chunkZ, int innerX, int innerZ, InventoryItem item, string combat_name)
	{
		if (item.GetString("is_killGoal_mob") != "true") return;
		string name = item.GetString("associated_quest_name");
		short step = item.GetShort("associated_quest_step");
		if (Instance.GetQuestProgress(name) != step || killGoal_mobs_killed.Contains(combat_name)) return;
		killGoal_mobs_killed.Add(combat_name);
	}

	public void CheckIfAllQuestMobsKilled(string zone, int chunkX, int chunkZ, int innerX, int innerZ, InventoryItem item, string combat_name)
	{
		if (item.GetString("is_killGoal_mob") != "true") return;
		string name = item.GetString("associated_quest_name");
		int step = item.GetShort("associated_quest_step");
		if (Instance.GetQuestProgress(name) != step || GetStepType(name, step) != "Kill All Mobs") return;
		int goal = int.Parse(Instance.GetQuest(name).steps[step]["Goal kills"], Startup.parse_culture);
		if (killGoal_mobs_killed.Count == goal) SetQuestProgress(name, step + 1);
	}

	public List<parsed_position> ParseQuestPositions(string quest_name, int progress, string parse_prefix)
	{
		List<parsed_position> positions = new List<parsed_position>();
		Dictionary<string, string> step = GetQuest(quest_name).steps[progress];
		List<string> strings = new List<string>();
		foreach (KeyValuePair<string, string> entry in step)
			if (entry.Key.Contains(parse_prefix)) strings.Add(entry.Value);
		foreach (string position in strings) positions.Add(ParsePosition(position));
		return positions;
	}

	public parsed_position ParsePosition(string position_str)
	{
		int comma1 = -1, comma2 = -1, space1 = -1, closing = -1, space2 = -1, space3 = -1;
		for (int i = 0; i < position_str.Length; i++)
		{
			if (comma1 == -1) { if (position_str[i] == ',') comma1 = i; }
			else if (comma2 == -1) { if (position_str[i] == ',') comma2 = i; }
			else if (space1 == -1) { if (position_str[i] == ' ') space1 = i; }
			else if (closing == -1) { if (position_str[i] == ')') closing = i; }
			else if (space2 == -1) { if (position_str[i] == ' ') space2 = i; }
			else if (space3 == -1) { if (position_str[i] == ' ') space3 = i; }
		}
		parsed_position position = new parsed_position();
		position.zone = position_str.Substring(6, comma1 - 6);
		position.chunkX = int.Parse(position_str.Substring(comma1 + 1, comma2 - comma1 - 1), Startup.parse_culture);
		position.chunkZ = int.Parse(position_str.Substring(space1 + 1, closing - space1 - 1), Startup.parse_culture);
		position.innerX = int.Parse(position_str.Substring(space2 + 1, space3 - space2 - 1), Startup.parse_culture);
		position.innerZ = int.Parse(position_str.Substring(space3 + 1, position_str.Length - space3 - 1), Startup.parse_culture);
		return position;
	}

	private void PickItemsToCollect(string quest_name, int progress)
	{
		Dictionary<string, string> step = GetQuest(quest_name).steps[progress];
		if (!step.ContainsKey("N_collect")) return;
		int count = int.Parse(step["N_collect"], Startup.parse_culture);
		List<InventoryItem> items = GetAllCollectables(quest_name, progress);
		int remove = items.Count - count;
		for (int i = 0; i < remove; i++)
		{
			int index = UnityEngine.Random.Range(0, items.Count);
			InventoryItem item = items[index];
			items.RemoveAt(index);
			PlayerData.Instance.SetSlotShort(quest_name + "_step" + progress + "_" + item.item_name + "_isCollected", 1, PlayerData.filename_t.global_quest_data);
		}
		foreach (InventoryItem item in items) PlayerData.Instance.SetSlotShort(quest_name + "_step" + progress + "_" + item.item_name + "_isCollected", 0, PlayerData.filename_t.global_quest_data);
	}

	public List<InventoryItem> GetCollectablesRemaining(string quest_name, int progress)
	{
		List<InventoryItem> all = GetAllCollectables(quest_name, progress);
		List<InventoryItem> remaining = new List<InventoryItem>();
		foreach (InventoryItem item in all)
			if (PlayerData.Instance.GetSlotShort(quest_name + "_step" + progress + "_" + item.item_name + "_isCollected", PlayerData.filename_t.global_quest_data) == 0) remaining.Add(item);
		return remaining;
	}

	public List<InventoryItem> GetAllCollectables(string quest_name, int progress)
	{
		Dictionary<string, string> step = GetQuest(quest_name).steps[progress];
		List<InventoryItem> items = new List<InventoryItem>();
		foreach (KeyValuePair<string, string> entry in step)
			if (entry.Key.Contains("PossibleCollect")) items.Add(new InventoryItem(entry.Value));
		return items;
	}

	public sell_quest_item_result TrySellQuestItemToNPC(string quest_name, int curr_progress, int set_progres_if_all_collected, InventoryItem item)
	{
		if (GetQuestProgress(quest_name) != curr_progress) return sell_quest_item_result.fail;
		Dictionary<string, string> step = GetQuest(quest_name).steps[curr_progress];
		List<InventoryItem> remaining = GetCollectablesRemaining(quest_name, curr_progress);
		if (!remaining.Contains(item)) return sell_quest_item_result.fail;
		PlayerData.Instance.SetSlotShort(quest_name + "_step" + curr_progress + "_" + item.item_name + "_isCollected", 1, PlayerData.filename_t.global_quest_data);
		quest_notif_on_exit_dialogue = "Quest Updated!";
		if (remaining.Count == 1)
		{
			SetQuestProgress(quest_name, set_progres_if_all_collected);
			return sell_quest_item_result.all_collected;
		}
		return sell_quest_item_result.new_collected;
	}

	public bool HasAnyQuestItemsInInventory(string quest_name, int progress)
	{
		string type = GetStepType(quest_name, progress);
		if (type == "Collect Items")
		{
			List<InventoryItem> remaining = GetCollectablesRemaining(quest_name, progress);
			foreach (int slot in inventory_ctr.Instance.player_inventory.FilledSlots())
				if (remaining.Contains(inventory_ctr.Instance.player_inventory[slot].item)) return true;
		}
		else if (type == "Collect Identical Items")
		{
			Dictionary<string, string> step = GetQuest(quest_name).steps[progress];
			if (!step.ContainsKey("CollectIdenticalItem")) return false;
			string name = step["CollectIdenticalItem"];
			foreach (int slot in inventory_ctr.Instance.player_inventory.FilledSlots())
				if (inventory_ctr.Instance.player_inventory[slot].item.item_name == name) return true;
		}
		return false;
	}

	public bool HasAllQuestItemsInInventory(string quest_name, int progress)
	{
		string type = GetStepType(quest_name, progress);
		if (type == "Collect Items" || type != "Collect Identical Items") return false;
		Dictionary<string, string> step = GetQuest(quest_name).steps[progress];
		if (!step.ContainsKey("N_identical_collect") || !step.ContainsKey("CollectIdenticalItem")) return false;
		string name = step["CollectIdenticalItem"];
		int count = int.Parse(step["N_identical_collect"], Startup.parse_culture);
		List<string> matches = new List<string>();
		foreach (int slot in inventory_ctr.Instance.player_inventory.FilledSlots())
		{
			InventoryItem item = inventory_ctr.Instance.player_inventory[slot].item;
			if (item.item_name != name) continue;
			string monster = item.GetString("fossil_monster");
			if (!matches.Contains(monster))
			{
				for (int i = 0; i < count - 1; i++) matches.Add(monster);
			}
			else
			{
				matches.Remove(monster);
				if (!matches.Contains(monster)) return true;
			}
		}
		return false;
	}

	public void TakeQuestItems(string quest_name, int progress, int set_progress_if_all_collected)
	{
		string type = GetStepType(quest_name, progress);
		if (type == "Collect Items")
		{
			List<InventoryItem> remaining = GetCollectablesRemaining(quest_name, progress);
			foreach (int slot in inventory_ctr.Instance.player_inventory.FilledSlots())
			{
				ItemCountPair pair = inventory_ctr.Instance.player_inventory[slot];
				if (!remaining.Contains(pair.item)) continue;
				remaining.Remove(pair.item);
				PlayerData.Instance.SetSlotShort(quest_name + "_step" + progress + "_" + pair.item.item_name + "_isCollected", 1, PlayerData.filename_t.global_quest_data);
				inventory_ctr.Instance.player_inventory[slot] = new ItemCountPair("", 0);
			}
			if (remaining.Count == 0) SetQuestProgress(quest_name, set_progress_if_all_collected);
		}
		else if (type == "Collect Identical Items")
		{
			Dictionary<string, string> step = GetQuest(quest_name).steps[progress];
			string name = step["CollectIdenticalItem"];
			int count = int.Parse(step["N_identical_collect"], Startup.parse_culture);
			List<string> matches = new List<string>();
			string monster = "";
			foreach (int slot in inventory_ctr.Instance.player_inventory.FilledSlots())
			{
				InventoryItem item = inventory_ctr.Instance.player_inventory[slot].item;
				if (item.item_name != name) continue;
				monster = item.GetString("fossil_monster");
				if (!matches.Contains(monster))
				{
					for (int i = 0; i < count - 1; i++) matches.Add(monster);
				}
				else
				{
					matches.Remove(monster);
					if (!matches.Contains(monster)) break;
				}
			}
			ExtraInventoryData data = new ExtraInventoryData();
			data.SetString("fossil_monster", monster);
			InventoryItem fossil = new InventoryItem("Fossil", data);
			foreach (int slot in inventory_ctr.Instance.player_inventory.FilledSlots())
			{
				if (inventory_ctr.Instance.player_inventory[slot].item != fossil) continue;
				inventory_ctr.Instance.player_inventory[slot] = new ItemCountPair("", 0);
				count--;
				if (count == 0) break;
			}
			SetQuestProgress(quest_name, set_progress_if_all_collected);
		}
		else return;
		quest_notif_on_exit_dialogue = "Quest Updated!";
	}
}
