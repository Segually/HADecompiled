using UnityEngine;
using UnityEngine.UI;

public class MinigameMenu : MonoBehaviour
{
	public enum menu_type_t
	{
		intro = 0,
		pick_mode = 1,
		cpu_select = 2,
		wait_for_friends = 3,
		dev = 4,
		in_game_CPU = 5,
		in_game_multiplayer = 6
	}

	public enum CPU_difficulty
	{
		easy = 0,
		normal = 1,
		hard = 2,
		impossible = 3
	}

	public static MinigameMenu Instance;

	public Text loot_respawn_text;

	public GameObject menu_main;

	public GameObject menu_CPU_select;

	public GameObject notif_screen;

	public GameObject menu;

	public GameObject yes_no_buttons;

	public GameObject menu_waiting_for_friends;

	public ItemSprite reward_easy_sprite;

	public ItemSprite reward_medium_sprite;

	public ItemSprite reward_hard_sprite;

	public ItemSprite reward_impossible_sprite;

	public ItemCountPair[] rewards;

	public Text notif_text;

	public CPU_difficulty curr_difficulty;

	public menu_type_t curr_menu;

	public GameObject dev_button;

	public Image menu_bg;

	public Image notif_bg;

	public Image CPU_head;

	public Text menu_header;

	public Text CPU_name;

	public Sprite spr_gameguy_head;

	public Color monster_pool_title;

	public Color monster_pool_BG;

	public Color monster_pool_BG_notif;

	public Color karaoke_title;

	public Color karaoke_BG;

	public Color karaoke_BG_notif;

	public Color karaoke2_title;

	public Color karaoke2_BG;

	public Color karaoke2_BG_notif;

	public void ShowMenu(menu_type_t menu_type)
	{
	}

	public void ShowNotif(string text, bool show_yes_no_buttons = false, bool slow_notif = false)
	{
	}

	public void HideNotif()
	{
	}

	public void DevButtonPressed()
	{
	}

	public void PressPlayVsCpu()
	{
	}

	public void PressPlayVsFriends()
	{
	}

	public void PressDifficulty(int difficulty)
	{
	}

	public void PressPlayAgain()
	{
	}

	public void PressDontPlayAgain()
	{
	}

	public void AnmNotifComplete()
	{
	}

	public static ItemCountPair[] GenerateNewRewards()
	{
		return null;
	}

	public void TryTakeReward(int index, byte pool_or_karaoke)
	{
	}

	public void SaveRewards()
	{
	}
}
