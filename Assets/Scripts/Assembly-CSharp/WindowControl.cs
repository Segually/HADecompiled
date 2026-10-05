using System;
using UnityEngine;
using UnityEngine.UI;

public class WindowControl : MonoBehaviour, OrderedStart
{
	[Serializable]
	public struct miniwindow_layout
	{
		public string name;

		public Color background_col;

		public Color border_col;

		public Color header_L_col;

		public Color header_R_col;

		public Color header_sel_col;

		public Color header_desel_col;

		public Color left_right_button_col;

		public Color crafting_buttons_color;
	}

	public enum tab
	{
		left = 0,
		right = 1
	}

	public enum window_type_t
	{
		none = 0,
		dialogue = 1,
		perkmanage = 2,
		mutant_market = 3,
		buy_gems_revive = 4,
		pool_game = 5,
		karaoke_game = 6
	}

	public enum miniwindow_type_t
	{
		none = 0,
		inventory_and_crafting = 1,
		quests_and_achieves = 2,
		teleport = 3,
		musicbox = 4,
		companion_commands = 5,
		chat = 6,
		painting = 7,
		inventory_and_container = 8,
		inventory_and_merchant = 9,
		land_claim = 10,
		lock_screen = 11,
		edit_navpost = 12,
		edit_merchant_sign = 13,
		new_friends_list = 14,
		report_object = 15,
		vending_machine = 16,
		battle_royale_start_splash = 17
	}

	public static WindowControl Instance;

	public GameObject close_button;

	public Image miniwindow_backdrop;

	public Image miniwindow_header_L_bg;

	public Image miniwindow_header_R_bg;

	public Image miniwindow_crafting_pageswitcher_L;

	public Image miniwindow_crafting_pageswitcher_R;

	public Text miniwindow_header_L;

	public Text miniwindow_header_R;

	public Canvas gui_canvas;

	public GameObject miniwindow;

	public miniwindow_layout[] miniwindow_layouts;

	public string layout_str;

	public tab curr_miniwindow_tab_selected;

	public window_type_t curr_window;

	public miniwindow_type_t curr_miniwindow;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public void ColorizeMiniwindow(string t)
	{
	}

	public void HideMiniwindowHeaders()
	{
	}

	public void ShowMiniwindowHeaders()
	{
	}

	public miniwindow_layout GetMiniwindowLayout(string t)
	{
		return default(miniwindow_layout);
	}

	public void OnPressLeftMiniwindowTab()
	{
	}

	public void VisuallySelectLeftMiniwindowTab()
	{
	}

	public void OnPressRightMiniwindowTab()
	{
	}

	public void VisuallySelectRightMiniwindowTab()
	{
	}

	private void UpdateTabColors()
	{
	}

	public bool ShouldRecreateOverheads()
	{
		if (curr_window == window_type_t.none && curr_miniwindow == miniwindow_type_t.none && BreedControl.Instance.state_t == BreedControl.state.none && DialogueControl.Instance.focus_type == DialogueControl.focus_type_t.none)
		{
			return true;
		}
		return false;
	}

	public bool ImportantWindowsOpen()
	{
		return false;
	}

	public void CloseAllWindows()
	{
	}

	public bool OpenMiniwindow(miniwindow_type_t miniwindow_t)
	{
		return false;
	}

	private void OnOpenMiniWindow(miniwindow_type_t type)
	{
	}

	public void ClickMiniwindowX()
	{
	}

	public void CloseMiniwindow(bool unpause_and_show_GUI)
	{
	}

	private void OnCloseMiniwindow(miniwindow_type_t to_close)
	{
	}

	public void SwitchMiniwindow(miniwindow_type_t type)
	{
	}

	public void OpenWindow(window_type_t window_type)
	{
		ConstructionControl.Instance.DONE_placing_button.SetActive(false);
		curr_window = window_type;
		close_button.SetActive(true);
		close_button.transform.SetAsLastSibling();
	}

	public bool CanOpenGenericWindow()
	{
		if (GameController.Instance.level_up_animation_playing || GameController.Instance.player == null)
		{
			return false;
		}
		return ConstructionControl.Instance.done_button_context == ConstructionControl.button_state.none;
	}

	public void DoOpenGenericWindow()
	{
		GameController.Instance.PAUSE_GAME();
		if (GameController.Instance.player != null)
		{
			GameController.Instance.player.GetComponent<CreatureBrainLocalPlayer>().StopEverything();
		}
		GameController.Instance.HideTargetCircle();
		GameController.Instance.DestroyAllOverheads();
		GameplayGUIControl.Instance.HideGameplayGui();
		MusicBoxControl.Instance.HideAllNotes();
	}

	public void PressClose()
	{
		OnClose();
	}

	private void OnClose()
	{
		if (PopupControl.Instance.popup_open)
		{
			return;
		}
		bool flag;
		switch (curr_window)
		{
		case window_type_t.none:
			return;
		case window_type_t.dialogue:
			DialogueControl.Instance.ExitDialogue(true);
			flag = true;
			break;
		case window_type_t.perkmanage:
			PerkControl.Instance.ClosePerkManageScreen();
			flag = true;
			break;
		case window_type_t.mutant_market:
			AudioControl.Instance.PlayGenericClick();
			ShopControl.Instance.HideMarketScreen();
			flag = true;
			break;
		case window_type_t.buy_gems_revive:
			ShopControl.Instance.CloseGemsWindow(false);
			flag = false;
			break;
		case window_type_t.pool_game:
			GameServerSender.Instance.SendExitMinigame();
			if (PoolGameControl.Instance != null && PoolGameControl.Instance.show_ad_on_close)
			{
				AdvertControl.Instance.TryShowInterstitialAd(AdvertControl.ad_context.FORCED);
			}
			GameServerSender.Instance.SendReleaseInteractingObject();
			GameController.Instance.DestroyPoolScreen();
			flag = true;
			break;
		case window_type_t.karaoke_game:
			if (KaraokeControl.Instance != null && KaraokeControl.Instance.show_ad_on_close)
			{
				AdvertControl.Instance.TryShowInterstitialAd(AdvertControl.ad_context.FORCED);
			}
			GameServerSender.Instance.SendReleaseInteractingObject();
			GameController.Instance.DestroyKaraokeScreens();
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		close_button.SetActive(false);
		PopupControl.Instance.SetButtonWasPressed();
		curr_window = window_type_t.none;
		if (flag)
		{
			GameController.Instance.GiveAllOverheads();
			GameplayGUIControl.Instance.ShowGameplayGui();
			GameController.Instance.UNPAUSE_GAME();
			GameController.Instance.ForgetInteractingElement();
		}
	}

	public void TryShowAdOnLevelupScreenAppear()
	{
		GameController.Instance.AttemptAdOnLevelup();
	}
}
