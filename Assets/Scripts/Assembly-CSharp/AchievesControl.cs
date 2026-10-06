using System;
using UnityEngine;
using UnityEngine.UI;

public class AchievesControl : MonoBehaviour, OrderedStart
{
	[Serializable]
	public struct achievement
	{
		public string name;

		public string description;

		public Sprite locked;

		public Sprite unlocked;
	}

	public static AchievesControl Instance;

	public achievement[] all_achivements;

	public Sprite trophy_spr;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public void DrawAchievementSlot(int slot_id, int index)
	{
		string title = all_achivements[index].name;
		string description = all_achivements[index].description;
		inventory_ctr.Instance.instantiated_crafting_slots[slot_id].LayOutCraftingSlot(title, description, 1f, true, false, false, false, false, CraftingSlot.req_placement.disable, CraftingSlot.text_area_layout.full, CraftingSlot.slots_positioning.full_size, false);
		Image graphic = inventory_ctr.Instance.instantiated_crafting_slots[slot_id].graphic;
		if (PlayerData.Instance.GetGlobalShort("achieve_" + all_achivements[index].name) == 0)
		{
			graphic.sprite = all_achivements[index].locked;
		}
		else
		{
			graphic.sprite = all_achivements[index].unlocked;
		}
	}

	public void UnlockAchievement(string achivement_key)
	{
		if (achivement_key == "test")
		{
			UnlockAchievement(all_achivements[UnityEngine.Random.Range(0, all_achivements.Length)].name);
			return;
		}
		int page = 0;
		int slot = 0;
		for (int i = 0; i < all_achivements.Length; i++)
		{
			if (all_achivements[i].name == achivement_key)
			{
				if (PlayerData.Instance.GetGlobalShort("achieve_" + achivement_key) == 0)
				{
					PlayerData.Instance.SetGlobalShort("achieve_" + achivement_key, 1);
					OnNotifClick onNotifClick = new OnNotifClick(OnNotifClick.type.achieves);
					onNotifClick.data.Add("achievement_page", page.ToString() ?? "");
					GameplayGUIControl.Instance.ShowNotif("Achievement unlocked! <color=#fff36e>" + achivement_key + "</color>    ", trophy_spr, onNotifClick);
				}
				break;
			}
			slot++;
			if (slot == 3)
			{
				slot = 0;
				page++;
			}
		}
	}

	public void OpenAchievesWindow(int skip_to_page)
	{
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.quests_and_achieves);
		WindowControl.Instance.VisuallySelectRightMiniwindowTab();
		OnRightMiniwindowTabPressed(skip_to_page);
	}

	public void OnRightMiniwindowTabPressed(int skip_to_page)
	{
		QuestControl.Instance.CloseQuestScreen();
		inventory_ctr.Instance.curr_crafting_list = null;
		inventory_ctr.Instance.goto_crafting_tab(skip_to_page);
	}
}
