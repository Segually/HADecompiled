using System;
using UnityEngine;

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
	}

	public void UnlockAchievement(string achivement_key)
	{
		if (achivement_key == "test")
		{
			UnlockAchievement(all_achivements[UnityEngine.Random.Range(0, all_achivements.Length)].name);
			return;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < all_achivements.Length; i++)
		{
			if (all_achivements[i].name == achivement_key)
			{
				if (PlayerData.Instance.GetGlobalShort("achieve_" + achivement_key) == 0)
				{
					PlayerData.Instance.SetGlobalShort("achieve_" + achivement_key, 1);
					OnNotifClick onNotifClick = new OnNotifClick(OnNotifClick.type.achieves);
					onNotifClick.data.Add("achievement_page", num.ToString() ?? "");
					GameplayGUIControl.Instance.ShowNotif("Achievement unlocked! <color=#fff36e>" + achivement_key + "</color>    ", trophy_spr, onNotifClick);
				}
				break;
			}
			num2++;
			if (num2 == 3)
			{
				num2 = 0;
				num++;
			}
		}
	}

	public void OpenAchievesWindow(int skip_to_page)
	{
	}

	public void OnRightMiniwindowTabPressed(int skip_to_page)
	{
	}
}
