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
	}

	public void OpenAchievesWindow(int skip_to_page)
	{
	}

	public void OnRightMiniwindowTabPressed(int skip_to_page)
	{
	}
}
