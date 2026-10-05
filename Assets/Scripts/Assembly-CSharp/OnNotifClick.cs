using System.Collections.Generic;

public class OnNotifClick
{
	public enum type
	{
		none = 0,
		achieves = 1,
		friends_list_general = 2,
		teleporters = 3,
		quests = 4,
		certain_friend = 5,
		new_gift = 6
	}

	public Dictionary<string, string> data;

	public type on_click_type;

	public OnNotifClick(type on_click_type)
	{
	}

	public void OnClick()
	{
	}
}
