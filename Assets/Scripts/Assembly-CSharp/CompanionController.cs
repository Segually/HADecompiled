using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompanionController : MonoBehaviour, OrderedStart
{
	public static CompanionController Instance;

	public Image healthbar_0;

	public Image healthbar_1;

	public Image healthbar_bg_0;

	public Image healthbar_bg_1;

	public List<ActiveCompanion> active_companions = new List<ActiveCompanion>();

	public GameObject type_animatedEgg;

	public GameObject EGG;

	public string selected_companion_name;

	public Color col_behaviour_tab_selected;

	public Color col_behaviour_tab_deselected;

	private int curr_wait_icon_selected;

	private int behaviour_tab_selected;

	private bool attack_XP_orbs_selected = true;

	public Color col_happy_icon_bg;

	public Color col_happy_icon_text;

	public Color col_happy_icon_bar;

	public Color col_info_icon_bg;

	public Color col_info_icon_text;

	public Color col_info_icon_bar;

	public int max_personal_companions_right_now = 2;

	public List<GameObject> trail_nodes__ = new List<GameObject>();

	public static int max_trail_nodes = 2;

	public GameObject companion_nib_0;

	public GameObject companion_nib_1;

	private int curr_companion_page;

	public Color col_switcher_YES;

	public Color col_switcher_NO;

	public static string default_wait_message1 = "Hello!";

	public static string default_wait_message2 = "Shall I come with you?";

	public static string default_statue_message1 = "It's a statue of somebody ...";

	public static string default_statue_message2 = "... I wonder who it is";

	public static string default_guard_message1 = "Hey boss!";

	public static string default_guard_message2 = "Is my guard duty finished?";

	public static string default_merchant_message1 = "Greetings.";

	public static string default_merchant_message2 = "Can I interest you in some items?";

	public Sprite companion_died_ico;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		List<InventoryItem> items = LoadCompanionList(PlayerData.filename_t.active_companions);
		for (int i = 0; i < items.Count; i++)
		{
			ActiveCompanion companion = new ActiveCompanion();
			companion.companion_item = items[i];
			active_companions.Add(companion);
			active_companions[i].hatch_index = i;
		}
	}

	public static int WaitIconIdToStateIconId(int wait_icon_id)
	{
		return wait_icon_id == 1 ? 3 : 2;
	}

	public Color GetTextColorFromIcon(int icon_id)
	{
		switch (icon_id)
		{
		case 3:
			return col_info_icon_text;
		case 2:
			return col_happy_icon_text;
		default:
			return default(Color);
		}
	}

	public void PressGuard()
	{
		PopupControl.Instance.SetButtonWasPressed();
		ActiveCompanion currSelectedCompanion = GetCurrSelectedCompanion();
		if (!currSelectedCompanion.is_temp_companion)
		{
			WindowControl.Instance.CloseMiniwindow(false);
			PopupControl.Instance.SetButtonWasPressed();
			ExtraInventoryData extraDataCopy = currSelectedCompanion.companion_item.GetExtraDataCopy();
			extraDataCopy.SetString("companion_mode", "guard");
			InventoryItem item = new InventoryItem(currSelectedCompanion.companion_item.item_name, extraDataCopy);
			ConstructionControl.Instance.EnterBuildMode(item, Vector3.zero, 0, false);
		}
		else
		{
			PopupControl.Instance.ShowMessage(currSelectedCompanion.companion_name + " cannot guard");
		}
	}

	public bool WasMyGuard(string owner_name)
	{
		if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
		{
			return owner_name == PlayerData.Instance.GetGlobalString("username_lower");
		}
		if (!(owner_name == "ME"))
		{
			return owner_name == PlayerData.Instance.GetGlobalString("username_lower");
		}
		return true;
	}

	public void OnGuardDie(string mob_name, string owner_name)
	{
		if (WasMyGuard(owner_name))
		{
			GameplayGUIControl.Instance.ShowNotif("<color=#aaaaaa>" + mob_name + " died</color>", companion_died_ico, new OnNotifClick(OnNotifClick.type.none));
		}
	}

	public void AddDeadCompanion(InventoryItem companion_item)
	{
		List<InventoryItem> items = LoadCompanionList(PlayerData.filename_t.dead_companions);
		if (items.Count > 14) items.RemoveAt(7);
		items.Add(companion_item);
		SaveCompanionList(items, PlayerData.filename_t.dead_companions);
	}

	public void SetCompanionGuiHealth(int index, float percentage)
	{
		if (index > 1) return;
		Image health = index == 0 ? healthbar_0 : index == 1 ? healthbar_1 : null;
		health.rectTransform.sizeDelta = new Vector2(percentage / 100f * healthbar_bg_0.rectTransform.sizeDelta.x, health.rectTransform.sizeDelta.y);
		health.rectTransform.anchoredPosition = new Vector2(health.rectTransform.sizeDelta.x * 0.5f, 0f);
	}

	public void MoveCompanionsToPlayerPosition()
	{
		ClearTrailNodes();
		Vector3 vector = ((!(GameController.Instance.player != null)) ? GameController.Instance.prev_player_pos : GameController.Instance.player.transform.position);
		int num = ZoneDataControl.Instance.curr_zonedata.outer_item_rot;
		if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			num = ((num + 1 < 4) ? (num + 1) : (num - 3));
		}
		foreach (ActiveCompanion active_companion in active_companions)
		{
			if (!(active_companion.obj != null))
			{
				continue;
			}
			if (active_companion.hatch_index < 2)
			{
				switch (num)
				{
				case 0:
				case 2:
					if (active_companion.hatch_index == 1)
					{
						active_companion.obj.transform.position = vector + new Vector3(0f, 0f, 1.3f);
					}
					else if (active_companion.hatch_index == 0)
					{
						active_companion.obj.transform.position = vector + new Vector3(0f, 0f, -1.3f);
					}
					break;
				case 1:
				case 3:
					if (active_companion.hatch_index == 1)
					{
						active_companion.obj.transform.position = vector + new Vector3(1.3f, 0f, 0f);
					}
					else if (active_companion.hatch_index == 0)
					{
						active_companion.obj.transform.position = vector + new Vector3(-1.3f, 0f, 0f);
					}
					break;
				}
			}
			else
			{
				active_companion.obj.transform.position = vector + Vector3.up;
			}
			active_companion.obj.GetComponent<CreatureBrainCompanion>().ForgetTargetsAndFollowPlayer();
		}
	}

	public void PressCompanionButton(int index)
	{
		PopupControl.Instance.SetButtonWasPressed();
		if (index < 0)
		{
			return;
		}
		if (index == 1)
		{
			if (active_companions.Count < 2)
			{
				return;
			}
		}
		else if (index == 0)
		{
			if (active_companions.Count < 1)
			{
				return;
			}
		}
		else if (active_companions.Count < 3)
		{
			return;
		}
		selected_companion_name = active_companions[index].companion_name;
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.companion_commands);
		WindowPrefabsControl.Instance.CreateScreen("COMPANION-commands", WindowPrefabsControl.build_into_t.mini_window);
		curr_companion_page = 0;
		RedrawPage(0);
		ActiveCompanion activeCompanion = active_companions[index];
		WindowPrefabsControl.Instance.GetTextLegacy("COMPANION-commands", "creature-name-header").text = activeCompanion.companion_name.ToUpper();
		WindowPrefabsControl.Instance.GetTextLegacy("COMPANION-commands", "creature-level").text = "Level " + activeCompanion.level;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button0").GetComponent<CanvasGroup>().alpha = 1f;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button1").GetComponent<CanvasGroup>().alpha = 1f;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button4").GetComponent<CanvasGroup>().alpha = 0.4f;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button5").GetComponent<CanvasGroup>().alpha = 0.4f;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button8").GetComponent<CanvasGroup>().alpha = 0.4f;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button11").GetComponent<CanvasGroup>().alpha = 0.4f;
		float alpha = (activeCompanion.is_temp_companion ? 0.4f : 1f);
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button2").GetComponent<CanvasGroup>().alpha = alpha;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button3").GetComponent<CanvasGroup>().alpha = alpha;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button7").GetComponent<CanvasGroup>().alpha = alpha;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button9").GetComponent<CanvasGroup>().alpha = alpha;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button10").GetComponent<CanvasGroup>().alpha = alpha;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button6").GetComponent<CanvasGroup>().alpha = alpha;
		for (int i = 0; i < 12; i++)
		{
			string text = WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button" + i).transform.Find("Text").GetComponent<Text>().text;
			WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "button" + i).transform.Find("Text").GetComponent<Text>().text = TranslationControl.Instance.TranslateGeneral(text, "GUI");
		}
		string text2 = WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "pageButtonL").transform.Find("Text").GetComponent<Text>().text;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "pageButtonL").transform.Find("Text").GetComponent<Text>().text = TranslationControl.Instance.TranslateGeneral(text2, "GUI");
		string text3 = WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "pageButtonR").transform.Find("Text").GetComponent<Text>().text;
		WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "pageButtonR").transform.Find("Text").GetComponent<Text>().text = TranslationControl.Instance.TranslateGeneral(text3, "GUI");
		List<string> list = new List<string>();
		list.Add(activeCompanion.creature_A);
		list.Add(activeCompanion.creature_B);
		GameObject hybridLite = CreatureMorpher.Instance.GetHybridLite(list);
		hybridLite.GetComponent<LiteModel>().animation_choppiness = GraphicsControl.Instance.SpecialAnimationChoppiness();
		hybridLite.transform.SetParent(WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "model-go-here").transform);
		hybridLite.transform.localRotation = Quaternion.Euler(0f, 220f, 0f);
		hybridLite.transform.localPosition = Vector3.zero;
		if (activeCompanion.hat_.item_name != "" && inventory_ctr.Instance.GetItemType(activeCompanion.hat_) == inventory_ctr.inv_type_t.helmet)
		{
			hybridLite.GetComponent<LiteModel>().ApplyHat(activeCompanion.hat_, null);
		}
		if (activeCompanion.body_.item_name != "" && inventory_ctr.Instance.GetItemType(activeCompanion.body_) == inventory_ctr.inv_type_t.armor)
		{
			hybridLite.GetComponent<LiteModel>().ApplyArmor(activeCompanion.body_, null);
		}
		if (activeCompanion.hand_.item_name != "" && inventory_ctr.Instance.GetItemType(activeCompanion.hand_) == inventory_ctr.inv_type_t.holdable)
		{
			hybridLite.GetComponent<LiteModel>().ApplyWeapon(activeCompanion.hand_, null);
		}
		hybridLite.transform.localScale = Vector3.one * 170f;
		hybridLite.GetComponent<LiteModel>().StartAnimation(0);
		ItemSprite.RecursiveApplyLayer(hybridLite.transform, LayerMask.NameToLayer("GUI-lighting"), false);
		WindowPrefabsControl.Instance.GetTextLegacy("COMPANION-commands", "creature-constituents").text = hybridLite.GetComponent<LiteModel>().original.creatures_that_made_me_TRANSLATED[0] + "+" + hybridLite.GetComponent<LiteModel>().original.creatures_that_made_me_TRANSLATED[1];
		Image image = WindowPrefabsControl.Instance.GetImage("COMPANION-commands", "exp-fg");
		float num = WindowPrefabsControl.Instance.GetImage("COMPANION-commands", "exp-bg").rectTransform.sizeDelta.x * ((float)activeCompanion.curr_exp / (float)activeCompanion.next_exp);
		image.rectTransform.sizeDelta = new Vector2(num, image.rectTransform.sizeDelta.y);
		image.rectTransform.anchoredPosition = new Vector2(num * 0.5f, 0f);
	}

	public void RedrawCompanionNibs()
	{
		companion_nib_0.SetActive(false);
		companion_nib_1.SetActive(false);
		if (active_companions.Count > 0 && active_companions[0].obj != null)
		{
			companion_nib_0.SetActive(true);
			companion_nib_0.transform.Find("name").GetComponent<Text>().text = active_companions[0].companion_name;
			Image component = companion_nib_0.transform.Find("happy").GetComponent<Image>();
			Sprite[] overhead_logos = DevBuildControl.Instance.overhead_logos;
			int wait_icon = active_companions[0].wait_icon;
			component.sprite = overhead_logos[(wait_icon != 0) ? ((wait_icon == 1) ? 3 : 2) : 2];
			if (active_companions[0].wait_icon == 0)
			{
				companion_nib_0.GetComponent<Image>().color = col_happy_icon_bg;
				companion_nib_0.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_happy_icon_bar;
			}
			else if (active_companions[0].wait_icon == 1)
			{
				companion_nib_0.GetComponent<Image>().color = col_info_icon_bg;
				companion_nib_0.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_info_icon_bar;
			}
			int wait_iconb = active_companions[0].wait_icon;
			companion_nib_0.transform.Find("name").GetComponent<Text>().color = ((wait_iconb == 1) ? col_info_icon_text : col_happy_icon_text);
		}
		if (active_companions.Count > 1 && active_companions[1].obj != null)
		{
			companion_nib_1.SetActive(true);
			companion_nib_1.transform.Find("name").GetComponent<Text>().text = active_companions[1].companion_name;
			Image component2 = companion_nib_1.transform.Find("happy").GetComponent<Image>();
			Sprite[] overhead_logos2 = DevBuildControl.Instance.overhead_logos;
			int wait_icon2 = active_companions[1].wait_icon;
			component2.sprite = overhead_logos2[(wait_icon2 != 0) ? ((wait_icon2 == 1) ? 3 : 2) : 2];
			if (active_companions[1].wait_icon == 0)
			{
				companion_nib_1.GetComponent<Image>().color = col_happy_icon_bg;
				companion_nib_1.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_happy_icon_bar;
			}
			else if (active_companions[1].wait_icon == 1)
			{
				companion_nib_1.GetComponent<Image>().color = col_info_icon_bg;
				companion_nib_1.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_info_icon_bar;
			}
			int wait_icon2b = active_companions[1].wait_icon;
			companion_nib_1.transform.Find("name").GetComponent<Text>().color = ((wait_icon2b == 1) ? col_info_icon_text : col_happy_icon_text);
		}
	}

	public void PrevPage()
	{
		if (curr_companion_page != 0)
		{
			RedrawPage(-1);
		}
	}

	public void NextPage()
	{
		if (curr_companion_page != 1)
		{
			RedrawPage(1);
		}
	}

	private void RedrawPage(int dir)
	{
		switch (curr_companion_page)
		{
		case 0:
			WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "page1").SetActive(false);
			break;
		case 1:
			WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "page2").SetActive(false);
			break;
		case 2:
			WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "page3").SetActive(false);
			break;
		}
		curr_companion_page += dir;
		if (curr_companion_page == 1)
		{
			WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "page2").SetActive(true);
			WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "pageButtonL").GetComponent<CanvasGroup>().alpha = 1f;
			WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "pageButtonR").GetComponent<CanvasGroup>().alpha = 0.4f;
		}
		else if (curr_companion_page == 0)
		{
			WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "page1").SetActive(true);
			WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "pageButtonL").GetComponent<CanvasGroup>().alpha = 0.4f;
			WindowPrefabsControl.Instance.GetObject("COMPANION-commands", "pageButtonR").GetComponent<CanvasGroup>().alpha = 1f;
		}
	}

	public void PressCommandAttack()
	{
		WindowControl.Instance.CloseMiniwindow(false);
		PopupControl.Instance.SetButtonWasPressed();
		GameController.Instance.is_picking_companion_target = true;
		ConstructionControl.Instance.click_to_place.SetActive(true);
		ConstructionControl.Instance.click_to_place_txt.text = "Pick a target";
		ConstructionControl.Instance.ShowDoneButton("CANCEL", ConstructionControl.button_state.COMPANION_ATTACK);
	}

	public void PressCommandWalk()
	{
		WindowControl.Instance.CloseMiniwindow(false);
		PopupControl.Instance.SetButtonWasPressed();
		GameController.Instance.is_picking_companion_walk_location = true;
		ConstructionControl.Instance.click_to_place.SetActive(true);
		ConstructionControl.Instance.click_to_place_txt.text = "Pick a location";
		ConstructionControl.Instance.ShowDoneButton("CANCEL", ConstructionControl.button_state.COMPANION_MOVE);
	}

	public void PressCommandItems()
	{
		PopupControl.Instance.SetButtonWasPressed();
		ActiveCompanion currSelectedCompanion = GetCurrSelectedCompanion();
		if (!currSelectedCompanion.is_temp_companion)
		{
			WindowPrefabsControl.Instance.DestroyScreen("COMPANION-commands");
			inventory_ctr.Instance.SucceedOpenCompanionPockets(currSelectedCompanion);
		}
		else
		{
			PopupControl.Instance.ShowMessage(currSelectedCompanion.companion_name + " cannot hold items");
		}
	}

	public void PressCommandWait()
	{
		PopupControl.Instance.SetButtonWasPressed();
		ActiveCompanion currSelectedCompanion = GetCurrSelectedCompanion();
		if (!currSelectedCompanion.is_temp_companion)
		{
			WindowControl.Instance.CloseMiniwindow(false);
			PopupControl.Instance.SetButtonWasPressed();
			ExtraInventoryData extraDataCopy = currSelectedCompanion.companion_item.GetExtraDataCopy();
			extraDataCopy.SetString("companion_mode", "wait");
			InventoryItem item = new InventoryItem(currSelectedCompanion.companion_item.item_name, extraDataCopy);
			ConstructionControl.Instance.EnterBuildMode(item, Vector3.zero, 0, false);
		}
		else
		{
			PopupControl.Instance.ShowMessage(currSelectedCompanion.companion_name + " cannot wait");
		}
	}

	public void PressCommandRename()
	{
		PopupControl.Instance.SetButtonWasPressed();
		ActiveCompanion currSelectedCompanion = GetCurrSelectedCompanion();
		if (!currSelectedCompanion.is_temp_companion)
		{
			WindowPrefabsControl.Instance.GetScreen("COMPANION-commands").gameObject.SetActive(false);
			WindowPrefabsControl.Instance.CreateScreen("COMPANION-rename", WindowPrefabsControl.build_into_t.mini_window);
			WindowPrefabsControl.Instance.GetTextLegacy("COMPANION-rename", "desc-text").text = "Type a new name for '" + selected_companion_name + "'";
		}
		else
		{
			PopupControl.Instance.ShowMessage(currSelectedCompanion.companion_name + " cannot be renamed");
		}
	}

	public void PressCommandAdvanced()
	{
		PopupControl.Instance.SetButtonWasPressed();
		ActiveCompanion currSelectedCompanion = GetCurrSelectedCompanion();
		if (!currSelectedCompanion.is_temp_companion)
		{
			WindowPrefabsControl.Instance.GetScreen("COMPANION-commands").gameObject.SetActive(false);
			WindowPrefabsControl.Instance.CreateScreen("COMPANION-advanced", WindowPrefabsControl.build_into_t.mini_window);
			for (int i = 0; i < 11; i++)
			{
				string text = WindowPrefabsControl.Instance.GetTextLegacy("COMPANION-advanced", "translate" + i).GetComponent<Text>().text;
				WindowPrefabsControl.Instance.GetTextLegacy("COMPANION-advanced", "translate" + i).GetComponent<Text>().text = TranslationControl.Instance.TranslateGeneral(text, "CompanionsEtc");
			}
			behaviour_tab_selected = 0;
			WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "wait_message_1").GetComponent<InputField>().SetTextWithoutNotify(currSelectedCompanion.wait_message1);
			WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "wait_message_2").GetComponent<InputField>().SetTextWithoutNotify(currSelectedCompanion.wait_message2);
			WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "wait_message_3").GetComponent<InputField>().SetTextWithoutNotify(currSelectedCompanion.wait_message3);
			WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "wait_message_4").GetComponent<InputField>().SetTextWithoutNotify(currSelectedCompanion.wait_message4);
			WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "guard_message_1").GetComponent<InputField>().SetTextWithoutNotify(currSelectedCompanion.guard_message1);
			WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "guard_message_2").GetComponent<InputField>().SetTextWithoutNotify(currSelectedCompanion.guard_message2);
			curr_wait_icon_selected = currSelectedCompanion.wait_icon;
			RedrawWaitLogo();
			attack_XP_orbs_selected = currSelectedCompanion.attack_xp_orbs;
			RedrawAttackExpOrbSwitcher();
		}
		else
		{
			PopupControl.Instance.ShowMessage(currSelectedCompanion.companion_name + " cannot change behaviour");
		}
	}

	public void PressOptionAttackExpOrb()
	{
		attack_XP_orbs_selected = !attack_XP_orbs_selected;
		RedrawAttackExpOrbSwitcher();
	}

	private void RedrawAttackExpOrbSwitcher()
	{
		if (attack_XP_orbs_selected)
		{
			WindowPrefabsControl.Instance.GetImage("COMPANION-advanced", "option_xp_orb_button").color = col_switcher_YES;
			WindowPrefabsControl.Instance.GetTextLegacy("COMPANION-advanced", "option_xp_orb_text").text = "YES";
		}
		else
		{
			WindowPrefabsControl.Instance.GetImage("COMPANION-advanced", "option_xp_orb_button").color = col_switcher_NO;
			WindowPrefabsControl.Instance.GetTextLegacy("COMPANION-advanced", "option_xp_orb_text").text = "NO";
		}
	}

	public void PressAdvancedTab(int index)
	{
		WindowPrefabsControl.Instance.GetImage("COMPANION-advanced", "button" + behaviour_tab_selected).color = col_behaviour_tab_deselected;
		WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "tab" + behaviour_tab_selected).SetActive(false);
		behaviour_tab_selected = index;
		WindowPrefabsControl.Instance.GetImage("COMPANION-advanced", "button" + behaviour_tab_selected).color = col_behaviour_tab_selected;
		WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "tab" + behaviour_tab_selected).SetActive(true);
	}

	public void PressChangeCompanionWaitIcon(int dir)
	{
		curr_wait_icon_selected += dir;
		if (curr_wait_icon_selected < 0)
		{
			curr_wait_icon_selected = 1;
		}
		else if (curr_wait_icon_selected >= 2)
		{
			curr_wait_icon_selected = 0;
		}
		RedrawWaitLogo();
	}

	private void RedrawWaitLogo()
	{
		int num = ((curr_wait_icon_selected != 0) ? ((curr_wait_icon_selected == 1) ? 3 : 2) : 2);
		WindowPrefabsControl.Instance.GetImage("COMPANION-advanced", "wait_icon").sprite = DevBuildControl.Instance.overhead_logos[num];
	}

	public void PressBackOnAdvanced(bool save)
	{
		if (save)
		{
			string text = WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "wait_message_1").GetComponent<InputField>().text;
			string text2 = WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "wait_message_2").GetComponent<InputField>().text;
			string text3 = WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "wait_message_3").GetComponent<InputField>().text;
			string text4 = WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "wait_message_4").GetComponent<InputField>().text;
			string text5 = WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "guard_message_1").GetComponent<InputField>().text;
			string text6 = WindowPrefabsControl.Instance.GetObject("COMPANION-advanced", "guard_message_2").GetComponent<InputField>().text;
			ActiveCompanion currSelectedCompanion = GetCurrSelectedCompanion();
			ExtraInventoryData extraDataCopy = currSelectedCompanion.companion_item.GetExtraDataCopy();
			extraDataCopy.SetString("wait_message1", text);
			extraDataCopy.SetString("wait_message2", text2);
			extraDataCopy.SetString("wait_message3", text3);
			extraDataCopy.SetString("wait_message4", text4);
			extraDataCopy.SetString("guard_message1", text5);
			extraDataCopy.SetString("guard_message2", text6);
			extraDataCopy.SetShort("npc_icon", curr_wait_icon_selected);
			extraDataCopy.SetShort("attack_xp_orbs", attack_XP_orbs_selected ? 1 : 0);
			currSelectedCompanion.companion_item = new InventoryItem(currSelectedCompanion.companion_item.item_name, extraDataCopy);
			SaveActiveCompanions();
			int wait_icon = currSelectedCompanion.wait_icon;
			int num = ((wait_icon != 0) ? ((wait_icon == 1) ? 3 : 2) : 2);
			currSelectedCompanion.obj.GetComponent<SharedCreature>().icon_id = num;
			currSelectedCompanion.obj.GetComponent<SharedCreature>().creature_type_col = ((num == 3) ? col_info_icon_text : col_happy_icon_text);
			currSelectedCompanion.obj.GetComponent<SharedCreature>().RedrawCreatureText();
			currSelectedCompanion.obj.GetComponent<SharedCreature>().RedrawIcon(num);
			RedrawCompanionNibs();
		}
		WindowPrefabsControl.Instance.GetScreen("COMPANION-commands").gameObject.SetActive(true);
		WindowPrefabsControl.Instance.DestroyScreen("COMPANION-advanced");
	}

	public void PressCommandComingSoon()
	{
		PopupControl.Instance.ShowMessage("Coming soon!");
	}

	public void PressCommandMerchant()
	{
		PopupControl.Instance.SetButtonWasPressed();
		ActiveCompanion currSelectedCompanion = GetCurrSelectedCompanion();
		if (!currSelectedCompanion.is_temp_companion)
		{
			WindowPrefabsControl.Instance.GetScreen("COMPANION-commands").gameObject.SetActive(false);
			WindowPrefabsControl.Instance.CreateScreen("COMPANION-merchant", WindowPrefabsControl.build_into_t.mini_window);
		}
		else
		{
			PopupControl.Instance.ShowMessage(currSelectedCompanion.companion_name + " cannot become a Merchant");
		}
	}

	public void PressBackOnRename()
	{
		WindowPrefabsControl.Instance.GetScreen("COMPANION-commands").gameObject.SetActive(true);
		WindowPrefabsControl.Instance.DestroyScreen("COMPANION-rename");
	}

	public void PressAcceptOnRename()
	{
		string input_text = WindowPrefabsControl.Instance.GetTextLegacy("COMPANION-rename", "new-name-text").text;
		if (Startup.StringNullOrWhitespace(input_text))
		{
			return;
		}
		if (PlayerData.Instance.GetGlobalShort("GEMS") < 2)
		{
			PopupControl.Instance.ShowMessage("Cannot rename companion\n<color=#ff3b29>You don't have enough gems!</color>");
			return;
		}
		PopupControl.Instance.on_yes_pressed = delegate
		{
			RenameCompanionManually(input_text);
			PressBackOnRename();
		};
		PopupControl.Instance.ShowYesNo("Spend <color=#38b9ff>2 gems</color> to rename\n" + selected_companion_name + " to <color=#ffdb38>" + input_text + "</color>?", "Yes", "No", PopupControl.context.yesno_ACTION);
	}

	public void RenameCompanionManually(string rename_to)
	{
		WindowPrefabsControl.Instance.GetTextLegacy("COMPANION-commands", "creature-name-header").text = rename_to.ToUpper();
		ActiveCompanion currSelectedCompanion = GetCurrSelectedCompanion();
		ExtraInventoryData extraDataCopy = currSelectedCompanion.companion_item.GetExtraDataCopy();
		extraDataCopy.SetString("npc_display_name", rename_to);
		currSelectedCompanion.companion_item = new InventoryItem(currSelectedCompanion.companion_item.item_name, extraDataCopy);
		selected_companion_name = rename_to;
		Color overheadNameColor = MobControl.Instance.GetOverheadNameColor(currSelectedCompanion.creature_A + currSelectedCompanion.creature_B);
		currSelectedCompanion.obj.GetComponent<SharedCreature>().AssignOverheadName(rename_to, overheadNameColor);
		SaveActiveCompanions();
		RedrawCompanionNibs();
		GameServerSender.Instance.SendRenameCompanion(currSelectedCompanion.combat_name, rename_to);
		short globalShort = PlayerData.Instance.GetGlobalShort("GEMS");
		PlayerData.Instance.SetGlobalShort("GEMS", globalShort - 2);
	}

	public void CreateSingleCompanion(ActiveCompanion companion)
	{
		Vector3 position = GameController.Instance.player != null ? GameController.Instance.player.transform.position : GameController.Instance.prev_player_pos;
		if (companion.hatch_index == 0) position += Vector3.left * 1.8f;
		else if (companion.hatch_index == 1) position += Vector3.forward * 1.8f;
		else position += Vector3.right * 1.8f * (companion.hatch_index - 1);
		CreateSingleCompanion(companion, position);
	}

	public void CreateSingleCompanion(ActiveCompanion companion, Vector3 V)
	{
		MobControl.Instance.SpawnCompanion(V, companion);
		SetCompanionGuiHealth(companion.hatch_index, 100f);
		RedrawCompanionNibs();
	}

	public void CompanionPocketsClosed(BasketContents companion_pockets)
	{
		ActiveCompanion currSelectedCompanion = GetCurrSelectedCompanion();
		if (currSelectedCompanion == null)
		{
			return;
		}
		ItemCountPair[] array = new ItemCountPair[inventory_ctr.n_slots_per_page_];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = companion_pockets[i];
		}
		currSelectedCompanion.companion_item = ChunkControl.Instance.EncodeItemListIntoItem("pockets", array, currSelectedCompanion.companion_item);
		SaveActiveCompanions();
		currSelectedCompanion.obj.GetComponent<SharedCreature>().hat_ = currSelectedCompanion.hat_;
		currSelectedCompanion.obj.GetComponent<SharedCreature>().body_ = currSelectedCompanion.body_;
		currSelectedCompanion.obj.GetComponent<SharedCreature>().hand_ = currSelectedCompanion.hand_;
		currSelectedCompanion.obj.GetComponent<SharedCreature>().OnEquipmentChanged();
		GameServerSender.Instance.SendCompanionChangeEquip(currSelectedCompanion.combat_name, currSelectedCompanion.hat_, currSelectedCompanion.body_, currSelectedCompanion.hand_);
	}

	public void DeleteAllActiveCompanions()
	{
		active_companions.Clear();
		SaveActiveCompanions();
	}

	public void DestroyActiveCompanion(ActiveCompanion companion)
	{
		if (companion == null) return;
		RemoveActiveCompanionAt(companion.hatch_index);
		if (companion.obj != null) Object.Destroy(companion.obj);
		RedrawCompanionNibs();
		GameServerSender.Instance.SendDestroyCompanion(companion.combat_name);
	}

	public ActiveCompanion GetCurrSelectedCompanion()
	{
		foreach (ActiveCompanion companion in active_companions)
			if (companion.companion_name == selected_companion_name) return companion;
		return null;
	}

	public void RecreateAllCompanions()
	{
		List<InventoryItem> waiting = LoadCompanionList(PlayerData.filename_t.suspended_companions);
		List<ActiveCompanion> remove = new List<ActiveCompanion>();
		int count = 0;
		foreach (ActiveCompanion companion in active_companions)
		{
			if (count >= max_personal_companions_right_now)
			{
				remove.Add(companion);
				waiting.Add(companion.companion_item);
			}
			count++;
		}
		foreach (ActiveCompanion companion in remove) DestroyActiveCompanion(companion);
		if (count < max_personal_companions_right_now && waiting.Count > 0)
		{
			List<InventoryItem> added = new List<InventoryItem>();
			foreach (InventoryItem item in waiting)
			{
				ActiveCompanion companion = new ActiveCompanion();
				companion.companion_item = item;
				companion.hatch_index = active_companions.Count;
				active_companions.Add(companion);
				added.Add(item);
				if (active_companions.Count >= max_personal_companions_right_now) break;
			}
			foreach (InventoryItem item in added) waiting.Remove(item);
		}
		SaveActiveCompanions();
		SaveCompanionList(waiting, PlayerData.filename_t.suspended_companions);
		foreach (ActiveCompanion companion in active_companions)
			if (companion.obj == null) CreateSingleCompanion(companion);
	}

	public void AcceptCompanionFollow()
	{
		int interacting_element_chunkX = GameController.Instance.interacting_element_chunkX;
		int interacting_element_chunkZ = GameController.Instance.interacting_element_chunkZ;
		int interacting_element_innerX = GameController.Instance.interacting_element_innerX;
		int interacting_element_innerZ = GameController.Instance.interacting_element_innerZ;
		string chunkString = ChunkControl.Instance.GetChunkString(ChunkControl.Instance.player_zone, interacting_element_chunkX, interacting_element_chunkZ);
		if (ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
			ChunkElement remove_element = new ChunkElement(interacting_element_item, GameController.Instance.interacting_element_rot);
			ConstructionControl.Instance.PlayerRemoveAt(remove_element, ChunkControl.Instance.player_zone, interacting_element_chunkX, interacting_element_chunkZ, interacting_element_innerX, interacting_element_innerZ, ConstructionControl.remove_context.self_remove, ConstructionControl.GenerateCacheKey());
			ActiveCompanion activeCompanion = new ActiveCompanion();
			activeCompanion.companion_item = interacting_element_item;
			activeCompanion.hatch_index = active_companions.Count;
			active_companions.Add(activeCompanion);
			SaveActiveCompanions();
			CreateSingleCompanion(activeCompanion, new Vector3((float)(interacting_element_innerX + interacting_element_chunkX * 10) + 0.5f, SharedCreature.H, (float)(interacting_element_innerZ + interacting_element_chunkZ * 10) + 0.5f));
			GameServerSender.Instance.SendCreatedLocalMob(activeCompanion.combat_name);
		}
	}

	public void AddTempCompanion(string creatureA, string creatureB, int start_lvl, string companion_name, InventoryItem hat_, InventoryItem body_, InventoryItem hand_)
	{
		ActiveCompanion companion = new ActiveCompanion();
		int next_exp = GameController.Instance.NextLevelExp(start_lvl);
		string combat_name = ShopControl.RandomString();
		companion.companion_item = ActiveCompanion.CreateNewItem(creatureA, creatureB, start_lvl, 0, next_exp, combat_name, companion_name, 0, "", "", "", "", "", "", 0, false, hand_, hat_, body_, "", "");
		companion.hatch_index = active_companions.Count;
		companion.is_temp_companion = true;
		active_companions.Add(companion);
		CreateSingleCompanion(companion);
	}

	public void DestroyTempCompanions()
	{
		int num = 0;
		foreach (ActiveCompanion active_companion in active_companions)
		{
			num += (active_companion.is_temp_companion ? 1 : 0);
		}
		for (int i = 0; i < num; i++)
		{
			int j;
			for (j = 0; !active_companions[j].is_temp_companion; j++)
			{
			}
			ActiveCompanion activeCompanion = active_companions[j];
			RemoveActiveCompanionAt(activeCompanion.hatch_index);
			if (activeCompanion.obj != null)
			{
				Object.Destroy(activeCompanion.obj);
			}
		}
	}

	public void AcceptFreeCompanion()
	{
		int interacting_element_chunkX = GameController.Instance.interacting_element_chunkX;
		int interacting_element_chunkZ = GameController.Instance.interacting_element_chunkZ;
		int interacting_element_innerX = GameController.Instance.interacting_element_innerX;
		int interacting_element_innerZ = GameController.Instance.interacting_element_innerZ;
		string creatureA = GameController.Instance.interacting_element_item.GetString("creature_A");
		string creatureB = GameController.Instance.interacting_element_item.GetString("creature_B");
		string companion_name = GameController.Instance.interacting_element_item.GetString("npc_display_name");
		string combat_name = ShopControl.RandomString();
		int newUniqueId = ConstructionControl.Instance.GetNewUniqueId(true);
		string item_name = GameController.Instance.interacting_element_item.GetString("hat");
		string value = GameController.Instance.interacting_element_item.GetString("hat_paint");
		string item_name2 = GameController.Instance.interacting_element_item.GetString("body");
		string value2 = GameController.Instance.interacting_element_item.GetString("armor_paint");
		ExtraInventoryData extraInventoryData = new ExtraInventoryData();
		extraInventoryData.SetString("paint", value);
		InventoryItem start_hat = new InventoryItem(item_name, extraInventoryData);
		ExtraInventoryData extraInventoryData2 = new ExtraInventoryData();
		extraInventoryData2.SetString("paint", value2);
		InventoryItem start_armor = new InventoryItem(item_name2, extraInventoryData2);
		ActiveCompanion activeCompanion = new ActiveCompanion();
		activeCompanion.companion_item = ActiveCompanion.CreateNewItem(creatureA, creatureB, 10, 0, GameController.Instance.NextLevelExp(10), combat_name, companion_name, newUniqueId, TranslationControl.Instance.TranslateGeneral(default_wait_message1, "CompanionsEtc"), TranslationControl.Instance.TranslateGeneral(default_wait_message2, "CompanionsEtc"), "", "", TranslationControl.Instance.TranslateGeneral(default_guard_message1, "CompanionsEtc"), TranslationControl.Instance.TranslateGeneral(default_guard_message2, "CompanionsEtc"), 0, false, new InventoryItem(""), start_hat, start_armor, TranslationControl.Instance.TranslateGeneral(default_merchant_message1, "Merchants"), TranslationControl.Instance.TranslateGeneral(default_merchant_message2, "Merchants"));
		activeCompanion.hatch_index = active_companions.Count;
		active_companions.Add(activeCompanion);
		SaveActiveCompanions();
		CreateSingleCompanion(activeCompanion, new Vector3(interacting_element_innerX + interacting_element_chunkX * 10, SharedCreature.H, interacting_element_innerZ + interacting_element_chunkZ * 10));
		ChunkControl.Instance.GetChunkString(ChunkControl.Instance.player_zone, interacting_element_chunkX, interacting_element_chunkZ);
		ChunkElement remove_element = new ChunkElement(GameController.Instance.interacting_element_item, GameController.Instance.interacting_element_rot);
		ConstructionControl.Instance.PlayerRemoveAt(remove_element, ChunkControl.Instance.player_zone, interacting_element_chunkX, interacting_element_chunkZ, interacting_element_innerX, interacting_element_innerZ, ConstructionControl.remove_context.self_remove, ConstructionControl.GenerateCacheKey());
		GameServerSender.Instance.SendCreatedLocalMob(activeCompanion.combat_name);
	}

	private void SaveCompanionList(List<InventoryItem> companion_item_list, PlayerData.filename_t filename_t)
	{
		string filename = PlayerData.Instance.GetFilenameString(filename_t);
		PlayerData.Instance.SetSlotShort("n_entries", companion_item_list.Count, filename_t, "default", -1);
		for (int i = 0; i < companion_item_list.Count; i++) companion_item_list[i].SaveToDisk(filename, "companion_" + i, "default");
	}

	public List<InventoryItem> LoadCompanionList(PlayerData.filename_t filename_t)
	{
		List<InventoryItem> items = new List<InventoryItem>();
		string filename = PlayerData.Instance.GetFilenameString(filename_t);
		short count = PlayerData.Instance.GetSlotShort("n_entries", filename, "default", -1);
		for (int i = 0; i < count; i++) items.Add(InventoryItem.LoadFromDisk("companion_" + i, filename, "default"));
		return items;
	}

	public void RenameCompanionOnHatch(string input)
	{
		BreedControl.Instance.text_result_name.text = input;
		ActiveCompanion companion = active_companions[active_companions.Count - 1];
		ExtraInventoryData data = companion.companion_item.GetExtraDataCopy();
		data.SetString("npc_display_name", input);
		companion.companion_item = new InventoryItem(companion.companion_item.item_name, data);
		SaveActiveCompanions();
		Color color = MobControl.Instance.GetOverheadNameColor(companion.creature_A + companion.creature_B);
		companion.obj.GetComponent<SharedCreature>().AssignOverheadName(input, color);
		RedrawCompanionNibs();
		GameServerSender.Instance.SendRenameCompanion(companion.combat_name, input);
	}

	public void SaveActiveCompanions()
	{
		List<InventoryItem> items = new List<InventoryItem>();
		foreach (ActiveCompanion companion in active_companions)
			if (!companion.is_temp_companion) items.Add(companion.companion_item);
		SaveCompanionList(items, PlayerData.filename_t.active_companions);
	}

	private void FixedUpdate()
	{
		if (GameController.Instance.player == null) return;
		if (trail_nodes__.Count == 0)
		{
			GameObject node = new GameObject("trail-node");
			node.transform.position = GameController.Instance.player.transform.position;
			trail_nodes__.Add(node);
			return;
		}
		if (Vector3.Distance(GameController.Instance.player.transform.position, trail_nodes__[0].transform.position) <= 1.3f) return;
		GameObject next = new GameObject("trail-node");
		next.transform.position = GameController.Instance.player.transform.position;
		trail_nodes__.Insert(0, next);
		if (trail_nodes__.Count > active_companions.Count * 2 + 2)
		{
			GameObject last = trail_nodes__[trail_nodes__.Count - 1];
			trail_nodes__.Remove(last);
			Object.Destroy(last);
		}
	}

	public void ClearTrailNodes()
	{
		foreach (GameObject item in trail_nodes__)
		{
			UnityEngine.Object.Destroy(item);
		}
		trail_nodes__.Clear();
	}

	public void CreateAnimatedEgg(int critterLevel, bool paid, string animal1 = "", string animal2 = "")
	{
		Quaternion rotation = Quaternion.Euler(0f, 268f, 0f);
		GameController.Instance.player.transform.rotation = rotation;
		GameController.Instance.player.GetComponent<SharedCreature>().SnapSpotterRotation(rotation);
		EGG = Object.Instantiate(type_animatedEgg);
		int index = active_companions.Count * 2 + 1;
		if (index >= trail_nodes__.Count) index = trail_nodes__.Count - 1;
		Vector3 position = trail_nodes__[index].transform.position;
		position.y = 0.1f;
		EGG.transform.position = position;
		StartCoroutine(AnimatedEggCoroutine(EGG, critterLevel, animal1, animal2));
		BreedControl.Instance.paid_companion = paid;
		BreedControl.Instance.state_t = BreedControl.state.view_companion_hatch;
		BreedControl.Instance.gameObject.SetActive(true);
		BreedControl.Instance.TransitionBackToBreeder(BreedControl.breeder_transition.on_companion);
	}

	private IEnumerator AnimatedEggCoroutine(GameObject EGG, int critterLevel, string animal1 = "", string animal2 = "")
	{
		BreedControl.Instance.view_result_rotate = true;
		yield return new WaitForSeconds(1f);
		EGG.GetComponent<Animation>().Play();
		yield return new WaitForSeconds(2.5f);
		EGG.GetComponent<CompanionEgg>().VisuallyCrackEgg();
		GameController.Instance.GetComponent<GameController>().sound_crackshell();
		GameController.Instance.GetComponent<GameController>().animation_sound_levelScreenAppear();
		string first = "";
		string path = "";
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.English: path = "Lists/Companions - English Names"; break;
		case TranslationControl.languages.Russian: path = "lang-Russian/Russian-CommonNames"; break;
		case TranslationControl.languages.Portuguese: path = "lang-Portuguese/Portuguese-CommonNames"; break;
		case TranslationControl.languages.Indonesian: path = "lang-Indonesian/Indonesian-CommonNames"; break;
		case TranslationControl.languages.Spanish: path = "lang-Spanish/Spanish-CommonNames"; break;
		case TranslationControl.languages.Thai: path = "lang-Thai/Thai-CommonNames"; break;
		}
		bool exists = false;
		List<string> lines = ResourceControl.Instance.GetTextFileLines(path, ref exists);
		if (exists)
		{
			List<string> names = new List<string>();
			foreach (string line in lines) if (!Startup.StringNullOrWhitespace(line)) names.Add(line);
			first = names[Random.Range(0, names.Count)];
		}
		string descriptor = "";
		exists = false;
		lines = ResourceControl.Instance.GetTextFileLines("Lists/Companions - Descriptors", ref exists);
		if (exists)
		{
			List<string> names = new List<string>();
			foreach (string line in lines) if (!Startup.StringNullOrWhitespace(line)) names.Add(line);
			descriptor = names[Random.Range(0, names.Count)];
			if (TranslationControl.Instance.use_language != TranslationControl.languages.English) descriptor = TranslationControl.Instance.TranslateGeneral(descriptor, "CommonDescriptors");
		}
		string name = "???";
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.English:
		case TranslationControl.languages.Russian: name = descriptor + " " + first; break;
		case TranslationControl.languages.Portuguese:
		case TranslationControl.languages.Indonesian:
		case TranslationControl.languages.Spanish: name = first + " " + descriptor; break;
		case TranslationControl.languages.Thai: name = first + descriptor; break;
		}
		BreedControl.Instance.SetBannerText(name, "");
		BreedControl.Instance.ShowBanners(BreedControl.banner_type.companion);
		if (animal1 == "") animal1 = CreatureMorpher.Instance.GetRandomCreature();
		if (animal2 == "") animal2 = CreatureMorpher.Instance.GetRandomCreature();
		string combat = ShopControl.RandomString();
		int pocket = ConstructionControl.Instance.GetNewUniqueId(true);
		ActiveCompanion companion = new ActiveCompanion();
		companion.companion_item = ActiveCompanion.CreateNewItem(animal1, animal2, 5, 0, GameController.Instance.NextLevelExp(5), combat, name, pocket, TranslationControl.Instance.TranslateGeneral(default_wait_message1, "CompanionsEtc"), TranslationControl.Instance.TranslateGeneral(default_wait_message2, "CompanionsEtc"), "", "", TranslationControl.Instance.TranslateGeneral(default_guard_message1, "CompanionsEtc"), TranslationControl.Instance.TranslateGeneral(default_guard_message2, "CompanionsEtc"), 0, false, new InventoryItem(""), new InventoryItem(""), new InventoryItem(""), TranslationControl.Instance.TranslateGeneral(default_merchant_message1, "Merchants"), TranslationControl.Instance.TranslateGeneral(default_merchant_message2, "Merchants"));
		companion.hatch_index = active_companions.Count;
		active_companions.Add(companion);
		SaveActiveCompanions();
		CreateSingleCompanion(companion, EGG.transform.position);
		GameServerSender.Instance.SendCreatedLocalMob(combat);
		yield return new WaitForSeconds(0.02f);
		BreedControl.Instance.AdjustCamHeightToCreatureHeight(companion.obj.GetComponent<SharedCreature>().myCreatureModel.gameObject, false);
		companion.obj.GetComponent<SharedCreature>().myCreatureModel.StartAnimation(0, -1f);
		yield return new WaitForSeconds(2.5f);
	}

	public void IncreaseCompanionExp(int amount, ActiveCompanion the_companion)
	{
		int exp = the_companion.curr_exp + amount;
		int next = the_companion.next_exp;
		int level = the_companion.level;
		if (exp >= next)
		{
			do
			{
				the_companion.obj.GetComponent<SharedCreature>().level++;
				level++;
				int new_next = GameController.Instance.NextLevelExp(level);
				the_companion.obj.GetComponent<SharedCreature>().ReCalcHpMaxAndHpRegen();
				the_companion.obj.GetComponent<Combatant>().GainSomeHPOnLevelup();
				exp -= next;
				GameServerSender.Instance.SendUpdateCreatureStats(the_companion.obj.GetComponent<Combatant>().combat_name, "");
				next = new_next;
			} while (exp >= next);
			the_companion.obj.GetComponent<SharedCreature>().ShowLevelupParticles(true, 0.6f);
			the_companion.obj.GetComponent<SharedCreature>().RedrawLevelText();
		}
		ExtraInventoryData data = the_companion.companion_item.GetExtraDataCopy();
		data.SetShort("curr_exp", exp);
		data.SetShort("next_exp", next);
		data.SetLong("level", level);
		the_companion.companion_item = new InventoryItem(the_companion.companion_item.item_name, data);
		SaveActiveCompanions();
	}

	public void CompanionDeath(ActiveCompanion companion)
	{
		if (!companion.is_temp_companion) AddDeadCompanion(companion.companion_item);
		RemoveActiveCompanionAt(companion.hatch_index);
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.companion_commands) WindowControl.Instance.CloseMiniwindow(true);
		GameplayGUIControl.Instance.ShowNotif("<color=#aaaaaa>" + companion.companion_name + " died</color>", companion_died_ico, new OnNotifClick(OnNotifClick.type.none));
		RedrawCompanionNibs();
	}

	private void RemoveActiveCompanionAt(int X)
	{
		for (int i = X + 1; i < active_companions.Count; i++)
		{
			active_companions[i].hatch_index--;
		}
		active_companions.RemoveAt(X);
		SaveActiveCompanions();
	}
}
