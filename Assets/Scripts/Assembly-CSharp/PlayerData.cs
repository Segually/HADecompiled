using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerData : MonoBehaviour, OrderedStart
{
	public enum filename_t
	{
		general = 0,
		the_inventory = 1,
		perks = 2,
		playerpos = 3,
		teleporters = 4,
		active_companions = 5,
		dead_companions = 6,
		suspended_companions = 7,
		global_quest_data = 8,
		generated_NPC_chests = 9
	}

	public static PlayerData Instance;

	public int SLOT;

	private SlotFilesGroup global_vals;

	private SlotFilesGroup slot0_files;

	private SlotFilesGroup slot1_files;

	private SlotFilesGroup slot2_files;

	public int slot_being_deleted;

	private bool currently_deleting;

	private const int file_iterator_timer = 10;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
		if (this == Instance)
		{
			global_vals = new SlotFilesGroup("General");
			slot0_files = new SlotFilesGroup("Slot_0");
			slot1_files = new SlotFilesGroup("Slot_1");
			slot2_files = new SlotFilesGroup("Slot_2");
			StartCoroutine(ClearUnusedFilesCoroutine());
		}
	}

	public string GetCurrentSlotFolder()
	{
		switch (SLOT)
		{
		case 0:
			return "Slot_0";
		case 1:
			return "Slot_1";
		case 2:
			return "Slot_2";
		default:
			return "";
		}
	}

	public void AsyncSaveAll(Action on_complete, bool show_connecting_percent, bool on_return_to_menu)
	{
		StartCoroutine(AsyncSaveAllCoroutine(on_complete, show_connecting_percent, on_return_to_menu));
	}

	private IEnumerator AsyncSaveAllCoroutine(Action on_complete, bool show_connecting_percent, bool on_return_to_menu)
	{
		SlotFilesGroup group = GetSlotFilesGroup(SLOT);
		List<string> slot_files_to_save = new List<string>();
		foreach (KeyValuePair<string, SingleFile> loaded_file in group.loaded_files)
		{
			slot_files_to_save.Add(loaded_file.Key);
		}
		float n_saved = 0f;
		foreach (string item in slot_files_to_save)
		{
			if (!group.loaded_files.ContainsKey(item))
			{
				continue;
			}
			if (group.loaded_files[item].edited_since_load)
			{
				group.SaveSlotFile(item);
			}
			n_saved += 1f;
			if (n_saved % 10f == 0f)
			{
				if (show_connecting_percent)
				{
					PopupControl.Instance.connecting_Text.text = "Saving Game (" + (int)(n_saved / (float)slot_files_to_save.Count * 100f) + "%)";
				}
				yield return new WaitForEndOfFrame();
			}
		}
		if (on_return_to_menu)
		{
			group.loaded_files.Clear();
		}
		if (on_complete != null)
		{
			on_complete();
		}
	}

	private IEnumerator ClearUnusedFilesCoroutine()
	{
		float check_timespan = 1f;
		int iterations_til_periodic_autosave = 75;
		while (true)
		{
			for (int i = 0; i < iterations_til_periodic_autosave; i++)
			{
				yield return new WaitForSeconds(check_timespan);
				if (!currently_deleting)
				{
					slot0_files.DeloadUnusedFiles();
					slot1_files.DeloadUnusedFiles();
					slot2_files.DeloadUnusedFiles();
				}
			}
			yield return new WaitForSeconds(check_timespan);
			if (!currently_deleting)
			{
				Action on_complete = null;
				if (SceneManager.GetActiveScene().name == "Game")
				{
					GameplayGUIControl.Instance.auto_saving_text.gameObject.SetActive(true);
					on_complete = TryDisableAutosaveText;
				}
				StartCoroutine(AsyncSaveAllCoroutine(on_complete, false, false));
			}
		}
	}

	private void TryDisableAutosaveText()
	{
		StartCoroutine(TryDisableAutosaveTextCoroutine());
	}

	private IEnumerator TryDisableAutosaveTextCoroutine()
	{
		yield return new WaitForSeconds(2f);
		if (SceneManager.GetActiveScene().name == "Game")
		{
			GameplayGUIControl.Instance.auto_saving_text.gameObject.SetActive(false);
		}
	}

	public SlotFilesGroup GetSlotFilesGroup(int slot)
	{
		if (slot == -1)
		{
			slot = SLOT;
		}
		switch (slot)
		{
		case 2:
			return slot2_files;
		case 1:
			return slot1_files;
		case 0:
			return slot0_files;
		default:
			return null;
		}
	}

	public void SaveAllOnApplicationQuit()
	{
		slot0_files.InstantlySaveAll();
		slot1_files.InstantlySaveAll();
		slot2_files.InstantlySaveAll();
	}

	public void DeleteSlot(int index, Action on_complete)
	{
		slot_being_deleted = index;
		StartCoroutine(DeleteSlotCoroutine(index, on_complete));
	}

	private IEnumerator DeleteSlotCoroutine(int index, Action on_complete)
	{
		SlotFilesGroup group = GetSlotFilesGroup(index);
		currently_deleting = true;
		string[] files_to_delete = group.FindRelatedFiles();
		float n_deleted = 0f;
		string[] array = files_to_delete;
		for (int i = 0; i < array.Length; i++)
		{
			File.Delete(array[i]);
			n_deleted += 1f;
			if (n_deleted % 10f == 0f)
			{
				PopupControl.Instance.connecting_Text.text = "Deleting (" + (int)(n_deleted / (float)files_to_delete.Length * 100f) + "%)";
				yield return new WaitForEndOfFrame();
			}
		}
		currently_deleting = false;
		group.loaded_files.Clear();
		on_complete();
	}

	public void DeleteOneSlotFile(string generic_file_name)
	{
		SlotFilesGroup slotFilesGroup;
		switch (SLOT)
		{
		case 2:
			slotFilesGroup = slot2_files;
			break;
		case 1:
			slotFilesGroup = slot1_files;
			break;
		case 0:
			slotFilesGroup = slot0_files;
			break;
		default:
			return;
		}
		slotFilesGroup?.DeleteOneFile(generic_file_name);
	}

	public SingleFile TryLoadFromDiskWithNoExtension(string full_path)
	{
		if (!File.Exists(full_path))
		{
			if (full_path.Length < 2)
			{
				return null;
			}
			full_path = full_path.Substring(0, full_path.Length - 1);
			if (!File.Exists(full_path))
			{
				return null;
			}
		}
		byte[] bytes = File.ReadAllBytes(full_path);
		SingleFile singleFile = new SingleFile();
		singleFile.LoadFromBytes(bytes);
		return singleFile;
	}

	public SingleFile TryLoadFromDiskWithBytesExtension(string full_path)
	{
		bool file_exists = false;
		byte[] bytesFileBytes = ResourceControl.Instance.GetBytesFileBytes(full_path, ref file_exists);
		if (!file_exists)
		{
			return null;
		}
		SingleFile singleFile = new SingleFile();
		singleFile.LoadFromBytes(bytesFileBytes, true);
		return singleFile;
	}

	public void SetSlotShort(string key, float value, filename_t filename, string filesegment = "default", int slot = -1)
	{
		SetSlotShort(key, (short)value, GetFilenameString(filename), filesegment, slot);
	}

	public void SetSlotShort(string key, float value, string filename, string filesegment = "default", int slot = -1)
	{
		SetSlotShort(key, (short)value, filename, filesegment, slot);
	}

	public void SetSlotShort(string key, byte value, filename_t filename, string filesegment = "default", int slot = -1)
	{
		SetSlotShort(key, (short)value, GetFilenameString(filename), filesegment, slot);
	}

	public void SetSlotShort(string key, byte value, string filename, string filesegment = "default", int slot = -1)
	{
		SetSlotShort(key, (short)value, filename, filesegment, slot);
	}

	public void SetSlotShort(string key, int value, filename_t filename, string filesegment = "default", int slot = -1)
	{
		SetSlotShort(key, (short)value, GetFilenameString(filename), filesegment, slot);
	}

	public void SetSlotShort(string key, int value, string filename, string filesegment = "default", int slot = -1)
	{
		SetSlotShort(key, (short)value, filename, filesegment, slot);
	}

	public void SetSlotShort(string key, short value, filename_t filename, string filesegment = "default", int slot = -1)
	{
		SetSlotShort(key, value, GetFilenameString(filename), filesegment, slot);
	}

	public void SetSlotShort(string key, short value, string filename, string filesegment = "default", int slot = -1)
	{
		GetSlotGroupOrThrow(slot).GetFile(filename).SetShort(key, value, filesegment);
	}

	public void SetSlotString(string key, string value, filename_t filename, string filesegment = "default", int slot = -1)
	{
		SetSlotString(key, value, GetFilenameString(filename), filesegment, slot);
	}

	public void SetSlotString(string key, string value, string filename, string filesegment = "default", int slot = -1)
	{
		GetSlotGroupOrThrow(slot).GetFile(filename).SetString(key, value, filesegment);
	}

	public void SetSlotLong(string key, float value, filename_t filename, string filesegment = "default", int slot = -1)
	{
		SetSlotLong(key, (int)value, GetFilenameString(filename), filesegment, slot);
	}

	public void SetSlotLong(string key, float value, string filename, string filesegment = "default", int slot = -1)
	{
		SetSlotLong(key, (int)value, filename, filesegment, slot);
	}

	public void SetSlotLong(string key, byte value, filename_t filename, string filesegment = "default", int slot = -1)
	{
		SetSlotLong(key, (int)value, GetFilenameString(filename), filesegment, slot);
	}

	public void SetSlotLong(string key, byte value, string filename, string filesegment = "default", int slot = -1)
	{
		SetSlotLong(key, (int)value, filename, filesegment, slot);
	}

	public void SetSlotLong(string key, short value, filename_t filename, string filesegment = "default", int slot = -1)
	{
		SetSlotLong(key, (int)value, GetFilenameString(filename), filesegment, slot);
	}

	public void SetSlotLong(string key, short value, string filename, string filesegment = "default", int slot = -1)
	{
		SetSlotLong(key, (int)value, filename, filesegment, slot);
	}

	public void SetSlotLong(string key, int value, filename_t filename, string filesegment = "default", int slot = -1)
	{
		SetSlotLong(key, value, GetFilenameString(filename), filesegment, slot);
	}

	public void SetSlotLong(string key, int value, string filename, string filesegment = "default", int slot = -1)
	{
		GetSlotGroupOrThrow(slot).GetFile(filename).SetLong(key, value, filesegment);
	}

	public string GetSlotString(string key, filename_t filename, string filesegment = "default", int slot = -1)
	{
		return GetSlotString(key, GetFilenameString(filename), filesegment, slot);
	}

	public string GetSlotString(string key, string filename, string filesegment = "default", int slot = -1)
	{
		return GetSlotGroupOrThrow(slot).GetFile(filename).GetStringValue(key, filesegment);
	}

	public short GetSlotShort(string key, filename_t filename, string filesegment = "default", int slot = -1)
	{
		return GetSlotShort(key, GetFilenameString(filename), filesegment, slot);
	}

	public short GetSlotShort(string key, string filename, string filesegment = "default", int slot = -1)
	{
		return GetSlotGroupOrThrow(slot).GetFile(filename).GetShortValue(key, filesegment);
	}

	public int GetSlotLong(string key, filename_t filename, string filesegment = "default", int slot = -1)
	{
		return GetSlotLong(key, GetFilenameString(filename), filesegment, slot);
	}

	public int GetSlotLong(string key, string filename, string filesegment = "default", int slot = -1)
	{
		return GetSlotGroupOrThrow(slot).GetFile(filename).GetLongValue(key, filesegment);
	}

	private SlotFilesGroup GetSlotGroupOrThrow(int slot)
	{
		if (slot == -1)
		{
			slot = SLOT;
		}
		switch (slot)
		{
		case 2:
			return slot2_files;
		case 1:
			return slot1_files;
		case 0:
			return slot0_files;
		default:
			throw new NullReferenceException();
		}
	}

	public short GetGlobalShort(string key)
	{
		return global_vals.GetFile("general_").GetShortValue(key);
	}

	public string GetGlobalString(string key)
	{
		return global_vals.GetFile("general_").GetStringValue(key);
	}

	public int GetGlobalLong(string key)
	{
		return global_vals.GetFile("general_").GetLongValue(key);
	}

	public void SetGlobalShort(string key, float value)
	{
		SetGlobalShort(key, (short)value);
	}

	public void SetGlobalShort(string key, byte value)
	{
		SetGlobalShort(key, (short)value);
	}

	public void SetGlobalShort(string key, int value)
	{
		global_vals.GetFile("general_").SetShort(key, (short)value);
		global_vals.InstantlySaveAll();
	}

	public void SetGlobalShort(string key, short value)
	{
		global_vals.GetFile("general_").SetShort(key, value);
		global_vals.InstantlySaveAll();
	}

	public void SetGlobalString(string key, string value)
	{
		global_vals.GetFile("general_").SetString(key, value);
		global_vals.InstantlySaveAll();
	}

	public void SetGlobalLong(string key, float value)
	{
		SetGlobalLong(key, (int)value);
	}

	public void SetGlobalLong(string key, byte value)
	{
		SetGlobalLong(key, (int)value);
	}

	public void SetGlobalLong(string key, short value)
	{
		SetGlobalLong(key, (int)value);
	}

	public void SetGlobalLong(string key, int value)
	{
		global_vals.GetFile("general_").SetLong(key, value);
		global_vals.InstantlySaveAll();
	}

	public string GetFilenameString(filename_t filename)
	{
		switch (filename)
		{
		case filename_t.the_inventory:
			return "the_inventory";
		case filename_t.perks:
			return "perks";
		case filename_t.playerpos:
			return "playerpos";
		case filename_t.teleporters:
			return "teleporters";
		case filename_t.active_companions:
			return "active_companions";
		case filename_t.dead_companions:
			return "dead_companions";
		case filename_t.suspended_companions:
			return "suspended_companions";
		case filename_t.global_quest_data:
			return "global_quest_data";
		case filename_t.generated_NPC_chests:
			return "generated_npc_chests";
		default:
			return "general";
		}
	}

	public void DeleteGemsEtc()
	{
		if (global_vals.loaded_files.ContainsKey("general_"))
		{
			global_vals.loaded_files.Remove("general_");
		}
		string path = Path.Combine(Startup.persistentDataPath + Path.DirectorySeparatorChar + "General", "general_");
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		PlayerPrefs.DeleteAll();
		string path2 = Path.Combine(Startup.persistentDataPath, "Terms.txt");
		if (File.Exists(path2))
		{
			File.Delete(path2);
		}
	}
}
