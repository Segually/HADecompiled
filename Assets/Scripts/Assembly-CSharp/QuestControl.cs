using System.Collections.Generic;
using UnityEngine;

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

	public static string quest_cache_path;

	public Dictionary<string, Quest> loaded_quests;

	public GameObject quest_nib_prefab;

	private List<GameObject> instantiated_quest_nibs;

	public string show_quest_complete_popup_after;

	private List<string> quest_names;

	public string quest_notif_on_exit_dialogue;

	public bool doing_time_trial;

	public bool redraw_companions_on_scene_change;

	public List<string> killGoal_mobs_killed;

	public string quest_perk_reapply_key;

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

	public static void ScanQuestDataForChanges(string[] cached_files_full, string[] game_files_full)
	{
	}

	public Quest GetQuest(string quest_name)
	{
		return null;
	}

	public void RevertQuestIfNecessary()
	{
	}

	public void OpenQuestWindow()
	{
	}

	public void OnLeftMiniwindowTabPressed()
	{
	}

	private void TryRecacheQuests()
	{
	}

	private void TryAddTempCompanions(string quest_name, int progress)
	{
	}

	public void CloseQuestScreen()
	{
	}

	public int GetQuestProgress(string quest_name)
	{
		return 0;
	}

	private void TryDestroyTempCompanions(Dictionary<string, string> step)
	{
	}

	public void SetQuestProgress(string quest_name, int set_progress_to, bool on_revert = false, bool on_teleport_finished = false)
	{
	}

	private void CreateQuestNib(string quest_key, string quest_display_name, quest_status status, int i)
	{
	}

	public void SucceedClickQuestNib(int index)
	{
	}

	public void ShowQuestCompletePopup(string quest_name)
	{
	}

	public bool IsQuestReadyToRepeat(string quest_name)
	{
		return false;
	}

	private string GetRepeatTimeRemainingString(string quest_name)
	{
		return null;
	}

	private string GetStepHint(string quest_name, int progress)
	{
		return null;
	}

	private string GetStepType(string quest_name, int progress)
	{
		return null;
	}

	public void TryNoteQuestMobKilled(string zone, int chunkX, int chunkZ, int innerX, int innerZ, InventoryItem item, string combat_name)
	{
	}

	public void CheckIfAllQuestMobsKilled(string zone, int chunkX, int chunkZ, int innerX, int innerZ, InventoryItem item, string combat_name)
	{
	}

	public List<parsed_position> ParseQuestPositions(string quest_name, int progress, string parse_prefix)
	{
		return null;
	}

	public parsed_position ParsePosition(string position_str)
	{
		return default(parsed_position);
	}

	private void PickItemsToCollect(string quest_name, int progress)
	{
	}

	public List<InventoryItem> GetCollectablesRemaining(string quest_name, int progress)
	{
		return null;
	}

	public List<InventoryItem> GetAllCollectables(string quest_name, int progress)
	{
		return null;
	}

	public sell_quest_item_result TrySellQuestItemToNPC(string quest_name, int curr_progress, int set_progres_if_all_collected, InventoryItem item)
	{
		return default(sell_quest_item_result);
	}

	public bool HasAnyQuestItemsInInventory(string quest_name, int progress)
	{
		return false;
	}

	public bool HasAllQuestItemsInInventory(string quest_name, int progress)
	{
		return false;
	}

	public void TakeQuestItems(string quest_name, int progress, int set_progress_if_all_collected)
	{
	}
}
