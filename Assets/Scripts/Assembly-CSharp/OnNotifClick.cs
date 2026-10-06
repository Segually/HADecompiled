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

	public Dictionary<string, string> data = new Dictionary<string, string>();

	public type on_click_type;

	public OnNotifClick(type on_click_type)
	{
		this.on_click_type = on_click_type;
	}

	public void OnClick()
	{
		if (WindowControl.Instance.curr_window != WindowControl.window_type_t.none || WindowControl.Instance.curr_miniwindow != WindowControl.miniwindow_type_t.none || TransitionControl.Instance.is_transition_playing)
		{
			return;
		}
		switch (on_click_type)
		{
		case type.achieves:
			if (data.ContainsKey("achievement_page"))
			{
				AchievesControl.Instance.OpenAchievesWindow(int.Parse(data["achievement_page"], Startup.parse_culture));
			}
			else
			{
				AchievesControl.Instance.OpenAchievesWindow(0);
			}
			break;
		case type.friends_list_general:
			FriendServerInterface.Instance.PressedFriendsButton();
			break;
		case type.teleporters:
			if (data.ContainsKey("skip_to_hard_code_page"))
			{
				CustomTeleporterControl.Instance.OpenTeleportWindow(int.Parse(data["skip_to_hard_code_page"], Startup.parse_culture));
			}
			else
			{
				CustomTeleporterControl.Instance.OpenTeleportWindow(-1);
			}
			break;
		case type.quests:
			GameController.Instance.PressViewAchievements();
			break;
		case type.certain_friend:
			if (data.ContainsKey("friend_username_lower"))
			{
				FriendServerInterface.Instance.DirectOpenFriend(data["friend_username_lower"]);
			}
			else
			{
				FriendServerInterface.Instance.PressedFriendsButton();
			}
			break;
		case type.new_gift:
			FriendServerInterface.Instance.PressGiftNotif();
			break;
		}
	}
}
