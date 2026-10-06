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

	public string layout_str = "";

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
		layout_str = t;
		miniwindow_layout miniwindowLayout = GetMiniwindowLayout(t);
		miniwindow_backdrop.color = miniwindowLayout.background_col;
		miniwindow_header_L.color = miniwindowLayout.header_L_col;
		miniwindow_header_R.color = miniwindowLayout.header_R_col;
		miniwindow_crafting_pageswitcher_L.color = miniwindowLayout.left_right_button_col;
		miniwindow_crafting_pageswitcher_R.color = miniwindowLayout.left_right_button_col;
		for (int i = 0; i < 3; i++)
		{
			inventory_ctr.Instance.instantiated_crafting_slots[i].GetComponent<Image>().color = miniwindowLayout.crafting_buttons_color;
		}
		UpdateTabColors();
	}

	public void HideMiniwindowHeaders()
	{
		miniwindow_header_L_bg.gameObject.SetActive(false);
		miniwindow_header_R_bg.gameObject.SetActive(false);
	}

	public void ShowMiniwindowHeaders()
	{
		miniwindow_header_L_bg.gameObject.SetActive(true);
		miniwindow_header_R_bg.gameObject.SetActive(true);
	}

	public miniwindow_layout GetMiniwindowLayout(string t)
	{
		for (int i = 0; i < miniwindow_layouts.Length; i++)
		{
			if (miniwindow_layouts[i].name == t)
			{
				return miniwindow_layouts[i];
			}
		}
		return miniwindow_layouts[0];
	}

	public void OnPressLeftMiniwindowTab()
	{
		if (curr_miniwindow_tab_selected == tab.left)
		{
			return;
		}
		curr_miniwindow_tab_selected = tab.left;
		UpdateTabColors();
		switch (curr_miniwindow)
		{
		case miniwindow_type_t.inventory_and_crafting:
		case miniwindow_type_t.inventory_and_container:
		case miniwindow_type_t.inventory_and_merchant:
			inventory_ctr.Instance.OnLeftMiniwindowTabClicked();
			break;
		case miniwindow_type_t.quests_and_achieves:
			QuestControl.Instance.OnLeftMiniwindowTabPressed();
			break;
		case miniwindow_type_t.teleport:
			CustomTeleporterControl.Instance.OnLeftMiniwindowTabPressed(CustomTeleporterControl.Instance.curr_hardcoded_teleporters_page);
			break;
		case miniwindow_type_t.new_friends_list:
			FriendServerInterface.Instance.ChangeFriendScreen(FriendServerInterface.friend_window_screen.friend_list);
			break;
		}
	}

	public void VisuallySelectLeftMiniwindowTab()
	{
		curr_miniwindow_tab_selected = tab.left;
		UpdateTabColors();
	}

	public void OnPressRightMiniwindowTab()
	{
		if (curr_miniwindow_tab_selected == tab.right)
		{
			return;
		}
		curr_miniwindow_tab_selected = tab.right;
		UpdateTabColors();
		switch (curr_miniwindow)
		{
		case miniwindow_type_t.inventory_and_crafting:
		case miniwindow_type_t.inventory_and_container:
		case miniwindow_type_t.inventory_and_merchant:
			inventory_ctr.Instance.OnRightMiniwindowTabClicked();
			break;
		case miniwindow_type_t.quests_and_achieves:
			AchievesControl.Instance.OnRightMiniwindowTabPressed(0);
			break;
		case miniwindow_type_t.teleport:
			CustomTeleporterControl.Instance.OnRightMiniwindowTabPressed();
			break;
		case miniwindow_type_t.new_friends_list:
			FriendServerInterface.Instance.PressPubServerTab();
			break;
		}
	}

	public void VisuallySelectRightMiniwindowTab()
	{
		curr_miniwindow_tab_selected = tab.right;
		UpdateTabColors();
	}

	private void UpdateTabColors()
	{
		if (curr_miniwindow_tab_selected == tab.right)
		{
			miniwindow_header_L_bg.color = GetMiniwindowLayout(layout_str).header_desel_col;
			miniwindow_header_R_bg.color = GetMiniwindowLayout(layout_str).header_sel_col;
		}
		else
		{
			miniwindow_header_L_bg.color = GetMiniwindowLayout(layout_str).header_sel_col;
			miniwindow_header_R_bg.color = GetMiniwindowLayout(layout_str).header_desel_col;
		}
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
		switch (curr_miniwindow)
		{
		case miniwindow_type_t.inventory_and_crafting:
		case miniwindow_type_t.musicbox:
		case miniwindow_type_t.inventory_and_container:
		case miniwindow_type_t.inventory_and_merchant:
			return false;
		default:
			if (curr_miniwindow == miniwindow_type_t.none && curr_window == window_type_t.none && ConstructionControl.Instance.done_button_context == ConstructionControl.button_state.none && !PopupControl.Instance.popup_open && !QuestControl.Instance.doing_time_trial && BreedControl.Instance.state_t == BreedControl.state.none)
			{
				return GameController.Instance.level_up_animation_playing;
			}
			return true;
		}
	}

	public void CloseAllWindows()
	{
		if (curr_window == window_type_t.none && curr_miniwindow == miniwindow_type_t.none && ConstructionControl.Instance.done_button_context == ConstructionControl.button_state.none)
		{
			return;
		}
		if (ShopControl.Instance.popup_open)
		{
			ShopControl.Instance.PressPopupClose();
		}
		if (ConstructionControl.Instance.done_button_context != ConstructionControl.button_state.none)
		{
			ConstructionControl.Instance.DonePlacing(true, true);
		}
		if (curr_miniwindow != miniwindow_type_t.none)
		{
			CloseMiniwindow(true);
		}
		else if (curr_window == window_type_t.dialogue || curr_window == window_type_t.perkmanage || curr_window == window_type_t.mutant_market || curr_window == window_type_t.pool_game || curr_window == window_type_t.karaoke_game)
		{
			OnClose();
		}
	}

	public bool OpenMiniwindow(miniwindow_type_t miniwindow_t)
	{
		if (!Instance.CanOpenGenericWindow())
		{
			return false;
		}
		Instance.DoOpenGenericWindow();
		curr_miniwindow = miniwindow_t;
		PopupControl.Instance.SetButtonWasPressed();
		AudioControl.Instance.PlayGenericClick();
		miniwindow.SetActive(true);
		OnOpenMiniWindow(curr_miniwindow);
		ConstructionControl.Instance.DONE_placing_button.SetActive(false);
		return true;
	}

	private void OnOpenMiniWindow(miniwindow_type_t type)
	{
		switch (type)
		{
		case miniwindow_type_t.inventory_and_crafting:
		case miniwindow_type_t.inventory_and_container:
		case miniwindow_type_t.inventory_and_merchant:
		case miniwindow_type_t.land_claim:
		case miniwindow_type_t.lock_screen:
		case miniwindow_type_t.edit_navpost:
		case miniwindow_type_t.edit_merchant_sign:
		case miniwindow_type_t.vending_machine:
			ColorizeMiniwindow("inventory");
			ShowMiniwindowHeaders();
			break;
		case miniwindow_type_t.quests_and_achieves:
			miniwindow_header_L.text = "Quests";
			miniwindow_header_R.text = "Achievements";
			ColorizeMiniwindow("achieves");
			ShowMiniwindowHeaders();
			break;
		case miniwindow_type_t.teleport:
			miniwindow_header_L.text = "Teleport";
			miniwindow_header_R.text = "My Portals";
			ColorizeMiniwindow("teleport");
			ShowMiniwindowHeaders();
			break;
		case miniwindow_type_t.musicbox:
			ColorizeMiniwindow("musicbox");
			HideMiniwindowHeaders();
			break;
		case miniwindow_type_t.companion_commands:
		case miniwindow_type_t.report_object:
		case miniwindow_type_t.battle_royale_start_splash:
			ColorizeMiniwindow("companions");
			HideMiniwindowHeaders();
			break;
		case miniwindow_type_t.chat:
			ColorizeMiniwindow("chat");
			HideMiniwindowHeaders();
			break;
		case miniwindow_type_t.painting:
			ColorizeMiniwindow("paint");
			HideMiniwindowHeaders();
			break;
		case miniwindow_type_t.new_friends_list:
			ColorizeMiniwindow("online");
			HideMiniwindowHeaders();
			break;
		default:
			ShowMiniwindowHeaders();
			break;
		}
	}

	public void ClickMiniwindowX()
	{
		if ((curr_miniwindow == miniwindow_type_t.inventory_and_crafting || curr_miniwindow == miniwindow_type_t.inventory_and_container || curr_miniwindow == miniwindow_type_t.inventory_and_merchant) && inventory_ctr.Instance.angular_animation_playing)
		{
			return;
		}
		CloseMiniwindow(true);
	}

	public void CloseMiniwindow(bool unpause_and_show_GUI)
	{
		miniwindow_type_t miniwindow_type_t = curr_miniwindow;
		OnCloseMiniwindow(miniwindow_type_t);
		AudioControl.Instance.PlayGenericClick();
		miniwindow.SetActive(false);
		PopupControl.Instance.SetButtonWasPressed();
		curr_miniwindow = miniwindow_type_t.none;
		if (miniwindow_type_t == miniwindow_type_t.inventory_and_merchant && inventory_ctr.Instance.enter_dialogue_on_close != -1)
		{
			DialogueControl.Instance.ReEnterDialogue(inventory_ctr.Instance.enter_dialogue_on_close);
			inventory_ctr.Instance.enter_dialogue_on_close = -1;
		}
		else if (unpause_and_show_GUI)
		{
			GameController.Instance.GiveAllOverheads();
			GameplayGUIControl.Instance.ShowGameplayGui();
			GameController.Instance.UNPAUSE_GAME();
			GameController.Instance.ForgetInteractingElement();
		}
	}

	private void OnCloseMiniwindow(miniwindow_type_t to_close)
	{
		switch (to_close)
		{
		case miniwindow_type_t.inventory_and_crafting:
		case miniwindow_type_t.inventory_and_container:
		case miniwindow_type_t.inventory_and_merchant:
			inventory_ctr.Instance.OnClose();
			break;
		case miniwindow_type_t.quests_and_achieves:
			inventory_ctr.Instance.HideCraftingTab();
			QuestControl.Instance.CloseQuestScreen();
			break;
		case miniwindow_type_t.teleport:
			CustomTeleporterControl.Instance.save_window_open = false;
			CustomTeleporterControl.Instance.in_search_screen = false;
			inventory_ctr.Instance.HideCraftingTab();
			WindowPrefabsControl.Instance.DestroyScreen("Teleport-nonebuilt");
			WindowPrefabsControl.Instance.DestroyScreen("Teleport-save");
			WindowPrefabsControl.Instance.DestroyScreen("Teleport-search");
			CustomTeleporterControl.Instance.tele_search_button.SetActive(false);
			break;
		case miniwindow_type_t.musicbox:
			MusicBoxControl.Instance.CloseMusicBox();
			break;
		case miniwindow_type_t.companion_commands:
			WindowPrefabsControl.Instance.DestroyScreen("COMPANION-commands");
			WindowPrefabsControl.Instance.DestroyScreen("COMPANION-rename");
			WindowPrefabsControl.Instance.DestroyScreen("COMPANION-advanced");
			WindowPrefabsControl.Instance.DestroyScreen("COMPANION-merchant");
			break;
		case miniwindow_type_t.chat:
			WindowPrefabsControl.Instance.DestroyScreen("CHAT");
			break;
		case miniwindow_type_t.painting:
			if (PaintingControl.Instance != null)
			{
				PaintingControl.Instance.save_screen_open = false;
				PaintingControl.Instance.load_screen_open = false;
			}
			WindowPrefabsControl.Instance.DestroyScreen("PAINTING");
			inventory_ctr.Instance.HideInventoryTab(false);
			WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-pickItem");
			break;
		case miniwindow_type_t.land_claim:
			LandClaimControl.Instance.OnClose();
			break;
		case miniwindow_type_t.lock_screen:
			LockControl.Instance.OnClose();
			break;
		case miniwindow_type_t.edit_navpost:
			WindowPrefabsControl.Instance.DestroyScreen("NAV POST");
			break;
		case miniwindow_type_t.edit_merchant_sign:
			WindowPrefabsControl.Instance.DestroyScreen("MERCHANT SIGN");
			break;
		case miniwindow_type_t.new_friends_list:
			FriendServerInterface.Instance.OnCloseFriendScreen();
			break;
		case miniwindow_type_t.report_object:
			WindowPrefabsControl.Instance.DestroyScreen("FRIENDS-report-finalize_obj");
			WindowPrefabsControl.Instance.DestroyScreen("FRIENDS-report-doubleCheck-obj");
			WindowPrefabsControl.Instance.DestroyScreen("FRIENDS-report-doubleCheck-obj-self");
			break;
		case miniwindow_type_t.vending_machine:
			WindowPrefabsControl.Instance.DestroyScreen("VENDING MACHINE");
			WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-pickItem");
			inventory_ctr.Instance.HideInventoryTab(false);
			GameServerSender.Instance.SendReleaseInteractingObject();
			break;
		case miniwindow_type_t.battle_royale_start_splash:
			WindowPrefabsControl.Instance.DestroyScreen("battle royale intro splash");
			break;
		}
	}

	public void SwitchMiniwindow(miniwindow_type_t type)
	{
		OnCloseMiniwindow(curr_miniwindow);
		curr_miniwindow = type;
		OnOpenMiniWindow(type);
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
