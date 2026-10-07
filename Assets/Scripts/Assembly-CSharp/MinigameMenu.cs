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

	public ItemCountPair[] rewards = new ItemCountPair[0];

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
		dev_button.SetActive(Application.isEditor);
		GetComponent<Animation>().Stop();
		menu_main.SetActive(menu_type == menu_type_t.pick_mode);
		menu_CPU_select.SetActive(menu_type == menu_type_t.cpu_select);
		menu_waiting_for_friends.SetActive(menu_type == menu_type_t.wait_for_friends);
		if (menu_type == menu_type_t.intro)
		{
			menu.SetActive(false);
		}
		else
		{
			GetComponent<Animation>().Play("pool-show-menu");
		}
		if (menu_type == menu_type_t.cpu_select && rewards.Length != 0)
		{
			reward_easy_sprite.RedrawAsMinigameReward(rewards[0].item, rewards[0].count);
			reward_medium_sprite.RedrawAsMinigameReward(rewards[1].item, rewards[1].count);
			reward_hard_sprite.RedrawAsMinigameReward(rewards[2].item, rewards[2].count);
			reward_impossible_sprite.RedrawAsMinigameReward(rewards[3].item, rewards[3].count);
		}
		curr_menu = menu_type;
	}

	public void ShowNotif(string text, bool show_yes_no_buttons = false, bool slow_notif = false)
	{
		menu.SetActive(false);
		notif_text.text = text;
		if (show_yes_no_buttons)
		{
			GetComponent<Animation>().Stop();
			GetComponent<Animation>().Play("pool-notif3");
			yes_no_buttons.SetActive(true);
		}
		else
		{
			GetComponent<Animation>().Stop();
			GetComponent<Animation>().Play(slow_notif ? "pool-notif4" : "pool-notif2");
			yes_no_buttons.SetActive(false);
		}
	}

	public void HideNotif()
	{
		notif_screen.SetActive(false);
	}

	public void DevButtonPressed()
	{
		if (KaraokeControl.Instance != null)
		{
			KaraokeControl.Instance.Dev_Initialize();
		}
	}

	public void PressPlayVsCpu()
	{
		ShowMenu(menu_type_t.cpu_select);
	}

	public void PressPlayVsFriends()
	{
		if (PoolGameControl.Instance != null)
		{
			if (GameServerConnector.Instance.FullyInGame())
			{
				ShowMenu(menu_type_t.wait_for_friends);
			}
			else
			{
				PopupControl.Instance.ShowMessage("To play with someone, you must be in the same world in Multiplayer");
			}
		}
		else if (KaraokeControl.Instance != null)
		{
			PopupControl.Instance.ShowMessage("Coming soon!");
		}
	}

	public void PressDifficulty(int difficulty)
	{
		curr_difficulty = (CPU_difficulty)difficulty;
		if (PoolGameControl.Instance != null)
		{
			PoolGameControl.Instance.StartCPUGame();
		}
		else if (KaraokeControl.Instance != null)
		{
			KaraokeControl.Instance.StartRound();
		}
	}

	public void PressPlayAgain()
	{
		if (PoolGameControl.Instance != null)
		{
			PoolGameControl.Instance.OnPlayAgain();
		}
		else if (KaraokeControl.Instance != null)
		{
			KaraokeControl.Instance.PressPlayAgain();
		}
	}

	public void PressDontPlayAgain()
	{
		WindowControl.Instance.PressClose();
	}

	public void AnmNotifComplete()
	{
		if (PoolGameControl.Instance != null)
		{
			PoolGameControl.Instance.AnmNotifComplete();
		}
		else if (KaraokeControl.Instance != null)
		{
			KaraokeControl.Instance.AnmNotifComplete();
		}
	}

	public static ItemCountPair[] GenerateNewRewards()
	{
		return new ItemCountPair[4]
		{
			LootControl.Instance.GetSingleLoot(Random.Range(40, 70)),
			LootControl.Instance.GetSingleLoot(Random.Range(200, 300)),
			LootControl.Instance.GetSingleLoot(Random.Range(300, 600)),
			LootControl.Instance.GetSingleLoot(Random.Range(600, 3000))
		};
	}

	public void TryTakeReward(int index, byte pool_or_karaoke)
	{
		if (!(rewards[index].item.item_name != ""))
		{
			return;
		}
		if (!inventory_ctr.Instance.CanReceiveItem(rewards[index].item, rewards[index].count))
		{
			PopupControl.Instance.ShowMessage("<color=#888888>Can't receive item</color>\n<color=#ff0000>Inventory Full!</color>", PopupControl.context.message, rewards[index]);
			return;
		}
		inventory_ctr.Instance.GiveItem(rewards[index].item, rewards[index].count, "");
		PopupControl.Instance.ShowMessage("<color=#2ebdff>Received Item!</color>\nx" + rewards[index].count + " " + rewards[index].item.item_name, PopupControl.context.message, rewards[index]);
		rewards[index] = new ItemCountPair("", 0);
		SaveRewards();
	}

	public void SaveRewards()
	{
		GameController.Instance.interacting_element_item.GetExtraDataCopy();
		InventoryItem new_item = ChunkControl.Instance.EncodeItemListIntoItem("rewards_list", rewards, GameController.Instance.interacting_element_item);
		ConstructionControl.Instance.PlayerReplaceInteracting(new_item, true);
	}
}
