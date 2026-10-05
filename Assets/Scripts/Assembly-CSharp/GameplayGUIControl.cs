using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayGUIControl : MonoBehaviour, OrderedStart
{
	public enum GUI_layout_t
	{
		standard_gameplay = 0,
		dev_interface = 1
	}

	private enum notif_index
	{
		neither_in_use = 0,
		notif1_in_use = 1,
		both_in_use = 2
	}

	public static GameplayGUIControl Instance;

	public GUI_layout_t curr_GUI;

	public Text[] GUI_texts_to_translate;

	public GameObject top_left_buttons;

	public GameObject levelbar;

	public GameObject top_right_buttons;

	public GameObject perkbuttons;

	public GameObject bottom_left_buttons;

	public GameObject teleport_button;

	public GameObject chat_button;

	public GameObject end_sit_button;

	private Vector3 market_button_start_position;

	private Vector3 inventory_button_start_position;

	private Vector3 friends_button_start_position;

	public GameObject market_button;

	public GameObject inventory_button;

	public GameObject friends_button;

	public GameObject quests_button;

	public GameObject distance_display;

	public Text txt_GUI_player_combo;

	public Text coordinateDisplay;

	public Text coordinateDisplay2;

	public Text text_playerLevel;

	public Text auto_saving_text;

	public MeshRenderer teleporter_icon;

	public Material mat_teleport_icon_active;

	public Material mat_teleport_icon_inactive;

	public Image expBarYellow;

	public Text text_skillPoints;

	public Image notif_1_obj;

	public Image notif_2_obj;

	public Image notif_1_overlay;

	public Image notif_2_overlay;

	public ItemSprite notif_1_item;

	public ItemSprite notif_2_item;

	public Text notif_1_text;

	public Text notif_2_text;

	public GameObject notif_1_BG;

	public GameObject notif_2_BG;

	private InventoryItem item_2 = new InventoryItem("");

	private OnNotifClick on_A_click;

	private OnNotifClick on_B_click;

	public List<GameObject>[] instantiated_stat_nibs;

	private Vector2 notif_1_start_pos;

	private Vector2 notif_2_start_pos;

	private float notif_1_life;

	private float notif_2_life;

	private float pad = 45f;

	void OrderedStart.Start_0()
	{
		Instance = this;
		instantiated_stat_nibs = new List<GameObject>[GameController.n_stats];
		for (int i = 0; i < GameController.n_stats; i++)
		{
			instantiated_stat_nibs[i] = new List<GameObject>();
		}
	}

	void OrderedStart.Start_1()
	{
		if (Screen.safeArea.x != 0f)
		{
			top_left_buttons.transform.localPosition += Vector3.right * 13f;
			bottom_left_buttons.transform.localPosition += Vector3.right * 13f;
		}
		market_button_start_position = market_button.transform.localPosition;
		inventory_button_start_position = inventory_button.transform.localPosition;
		friends_button_start_position = friends_button.transform.localPosition;
		notif_1_life = 0f;
		notif_2_life = 0f;
		Text[] gUI_texts_to_translate = GUI_texts_to_translate;
		foreach (Text text in gUI_texts_to_translate)
		{
			text.text = TranslationControl.Instance.TranslateGeneral(text.text, "GUI");
		}
		notif_1_start_pos = ((RectTransform)notif_1_obj.transform).anchoredPosition;
		notif_2_start_pos = ((RectTransform)notif_2_obj.transform).anchoredPosition;
	}

	public void ShowGameplayGui()
	{
		top_left_buttons.SetActive(true);
		top_right_buttons.SetActive(true);
		bottom_left_buttons.SetActive(true);
		chat_button.SetActive(GameServerConnector.Instance.FullyInGame());
		if (curr_GUI == GUI_layout_t.dev_interface)
		{
			market_button.SetActive(false);
			inventory_button.SetActive(false);
			friends_button.SetActive(false);
			teleport_button.SetActive(true);
			quests_button.SetActive(false);
			distance_display.SetActive(true);
			perkbuttons.SetActive(false);
			levelbar.SetActive(false);
			WindowPrefabsControl.Instance.CreateScreen("dev - top left", WindowPrefabsControl.build_into_t.GAME_CTR);
			WindowPrefabsControl.Instance.CreateScreen("dev - bottom center", WindowPrefabsControl.build_into_t.GAME_CTR);
			DevBuildControl.Instance.RedrawBanditButtons();
		}
		else if (curr_GUI == GUI_layout_t.standard_gameplay)
		{
			market_button.SetActive(true);
			inventory_button.SetActive(true);
			friends_button.SetActive(true);
			market_button.transform.localPosition = market_button_start_position;
			inventory_button.transform.localPosition = inventory_button_start_position;
			friends_button.transform.localPosition = friends_button_start_position;
			teleport_button.SetActive(true);
			quests_button.SetActive(true);
			distance_display.SetActive(true);
			perkbuttons.SetActive(true);
			levelbar.SetActive(true);
			WindowPrefabsControl.Instance.DestroyScreen("dev - top left");
			WindowPrefabsControl.Instance.DestroyScreen("dev - bottom center");
		}
		((RectTransform)notif_1_obj.transform).anchoredPosition = notif_1_start_pos;
		((RectTransform)notif_2_obj.transform).anchoredPosition = notif_2_start_pos;
		if (GameController.Instance.player != null && GameController.Instance.player.GetComponent<SharedCreature>().snapped_to_chair_obj)
		{
			end_sit_button.SetActive(true);
		}
		if (QuestControl.Instance.doing_time_trial)
		{
			WindowPrefabsControl.Instance.GetScreen("QUEST COUNTDOWN").GetComponent<QuestCountdown>().game_elements.alpha = 1f;
			if (WindowPrefabsControl.Instance.GetScreen("QUEST KILL COUNT") != null)
			{
				WindowPrefabsControl.Instance.GetScreen("QUEST KILL COUNT").SetActive(true);
			}
		}
	}

	public void HideGameplayGui()
	{
		top_left_buttons.SetActive(false);
		top_right_buttons.SetActive(false);
		bottom_left_buttons.SetActive(false);
		levelbar.SetActive(false);
		perkbuttons.SetActive(false);
		end_sit_button.SetActive(false);
		WindowPrefabsControl.Instance.DestroyScreen("dev - top left");
		WindowPrefabsControl.Instance.DestroyScreen("dev - bottom center");
		if (QuestControl.Instance.doing_time_trial)
		{
			WindowPrefabsControl.Instance.GetScreen("QUEST COUNTDOWN").GetComponent<QuestCountdown>().game_elements.alpha = 0f;
			if (WindowPrefabsControl.Instance.GetScreen("QUEST KILL COUNT") != null)
			{
				WindowPrefabsControl.Instance.GetScreen("QUEST KILL COUNT").SetActive(false);
			}
		}
		levelbar.GetComponent<Animation>().Stop();
		GameController.Instance.level_up_animation_playing = false;
	}

	public void RedrawSkillPointsSpendableText()
	{
		if (GameController.Instance.skillPointsSpendable == 0)
		{
			text_skillPoints.text = "<color=#cccccc>0</color> <size=33><color=#cccccc>POINTS</color></size>";
		}
		else
		{
			text_skillPoints.text = GameController.Instance.skillPointsSpendable + " <size=33><color=#cccccc>POINTS</color></size>";
		}
	}

	public void RedrawAllStatNibs()
	{
		for (int i = 0; i < GameController.n_stats; i++)
		{
			foreach (GameObject item in instantiated_stat_nibs[i])
			{
				Object.Destroy(item);
			}
			instantiated_stat_nibs[i].Clear();
		}
		for (int j = 0; j < GameController.n_stats; j++)
		{
			int num = GameController.Instance.player_stats[j];
			for (int num2 = num % 6; num2 > 0; num2--)
			{
				CreateStatNib(j);
			}
			RedrawStatName(j, num);
		}
	}

	public void CreateStatNib(int index)
	{
		GameObject gameObject = Object.Instantiate(GameController.Instance.type_levelMeter_nib);
		instantiated_stat_nibs[index].Add(gameObject);
		gameObject.transform.SetParent(GameController.Instance.stat_meters[index].transform);
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = new Vector3((float)(instantiated_stat_nibs[index].Count * 40 - 40) + 61.5f, -11.3f, 0f);
		gameObject.GetComponent<Image>().color = GameController.Instance.nibColors[index];
	}

	public void RedrawStatName(int meter_index, int set_to)
	{
		string text;
		switch (meter_index)
		{
		case 0:
			text = "Mining";
			break;
		case 1:
			text = "Health Recharge";
			break;
		case 2:
			text = "Health";
			break;
		case 3:
			text = "Attack";
			break;
		case 4:
			text = "Accuracy";
			break;
		case 5:
			text = "Crafting";
			break;
		case 6:
			text = "Power Recharge";
			break;
		case 7:
			text = "Dodge";
			break;
		default:
			text = "?";
			break;
		}
		Text component = GameController.Instance.stat_meters[meter_index].transform.Find("skill_title").gameObject.GetComponent<Text>();
		if (set_to == 0)
		{
			component.text = "<color=#ffffff>" + text + "</color>";
		}
		else
		{
			component.text = "<color=#ffffff>" + text + " ~</color> " + set_to;
		}
	}

	private bool OnlyShowOneNotif()
	{
		return false;
	}

	public void DisableTeleporterButton()
	{
	}

	public void EnableTeleporterButton()
	{
		CustomTeleporterControl.Instance.disable_teleport_button = false;
		teleport_button.GetComponent<CanvasGroup>().alpha = 1f;
		teleporter_icon.material = mat_teleport_icon_active;
	}

	public void UpdateDistanceDisplay()
	{
		if (ZoneDataControl.Instance.curr_zonedata.house_item.GetString("quest_miniworld") == "true")
		{
			coordinateDisplay.text = "???";
			coordinateDisplay2.text = "???";
			return;
		}
		if (GameController.Instance.player != null)
		{
			float num = GameController.Instance.DepthAt(GameController.Instance.player.transform.position);
			coordinateDisplay.text = num.ToString("F0") + "<size=21> M</size>";
		}
		coordinateDisplay2.text = ChunkControl.Instance.player_chunk_X + ", " + ChunkControl.Instance.player_chunk_Z;
	}

	public void DrawComboText()
	{
		GameObject gameObject = GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel.gameObject;
		string text = "";
		if (gameObject.GetComponent<LiteModel>().original.creatures_that_made_me_TRANSLATED.Count == 2)
		{
			text = GameController.first_upper(gameObject.GetComponent<LiteModel>().original.creatures_that_made_me_TRANSLATED[0]) + " + " + GameController.first_upper(gameObject.GetComponent<LiteModel>().original.creatures_that_made_me_TRANSLATED[1]);
		}
		txt_GUI_player_combo.text = text;
	}

	public void ClickNotifA()
	{
	}

	public void ClickNotifB()
	{
	}

	private void SetNotifGraphic(int i, InventoryItem item)
	{
		switch (i)
		{
		case 1:
			notif_1_BG.SetActive(false);
			notif_1_overlay.gameObject.SetActive(false);
			notif_1_item.gameObject.SetActive(true);
			notif_1_item.RedrawBasicIgnorePremium(item, 1);
			break;
		case 2:
			notif_2_BG.gameObject.SetActive(false);
			notif_2_overlay.gameObject.SetActive(false);
			notif_2_item.gameObject.SetActive(true);
			notif_2_item.RedrawBasicIgnorePremium(item, 1);
			item_2 = item;
			break;
		}
	}

	private void SetNotifGraphic(int i, Sprite sprite)
	{
		switch (i)
		{
		case 1:
			if (sprite != null)
			{
				notif_1_BG.SetActive(true);
				notif_1_overlay.gameObject.SetActive(true);
				notif_1_overlay.sprite = sprite;
			}
			else
			{
				notif_1_BG.SetActive(false);
				notif_1_overlay.gameObject.SetActive(false);
			}
			notif_1_item.gameObject.SetActive(false);
			break;
		case 2:
			if (sprite != null)
			{
				notif_2_BG.SetActive(true);
				notif_2_overlay.gameObject.SetActive(true);
				notif_2_overlay.sprite = sprite;
			}
			else
			{
				notif_2_BG.SetActive(false);
				notif_2_overlay.gameObject.SetActive(false);
			}
			notif_2_item.gameObject.SetActive(false);
			break;
		}
	}

	public void HideAllNotifs()
	{
	}

	private void BumpNotif()
	{
		bool activeInHierarchy = notif_2_item.gameObject.activeInHierarchy;
		GameObject gameObject = notif_1_item.gameObject;
		if (activeInHierarchy)
		{
			gameObject.SetActive(true);
			notif_1_item.RedrawBasicIgnorePremium(item_2, 1);
		}
		else
		{
			gameObject.SetActive(false);
		}
		notif_1_BG.gameObject.SetActive(notif_2_BG.gameObject.activeInHierarchy);
		bool activeInHierarchy2 = notif_2_overlay.gameObject.activeInHierarchy;
		GameObject gameObject2 = notif_1_overlay.gameObject;
		if (activeInHierarchy2)
		{
			gameObject2.SetActive(true);
			notif_1_overlay.sprite = notif_2_overlay.sprite;
		}
		else
		{
			gameObject2.SetActive(false);
		}
		notif_1_text.text = notif_2_text.text;
		notif_1_obj.rectTransform.sizeDelta = notif_2_obj.rectTransform.sizeDelta;
		notif_1_obj.gameObject.GetComponent<Animation>().Stop();
		notif_1_obj.transform.localScale = Vector3.one;
		notif_1_obj.GetComponent<CanvasGroup>().alpha = 1f;
		notif_2_obj.GetComponent<Animation>().Stop();
		notif_2_obj.gameObject.SetActive(false);
		notif_1_life = notif_2_life;
		on_A_click = on_B_click;
	}

	public void ShowNotif(string text, OnNotifClick on_click)
	{
		ShowNotif(text, (Sprite)null, on_click);
	}

	public void ShowNotif(string str, InventoryItem item, int count, OnNotifClick on_click)
	{
		float num = notif_1_life;
		ShowNotif(str, (Sprite)null, on_click);
		SetNotifGraphic((num == 0f) ? 1 : 2, item);
	}

	public void ShowNotif(string str, Sprite notif_img, OnNotifClick on_click)
	{
		if (notif_1_life == 0f)
		{
			notif_1_obj.gameObject.SetActive(true);
			notif_1_obj.GetComponent<Animation>().Stop();
			notif_1_obj.GetComponent<Animation>().Play("show-notif");
			notif_1_text.text = str;
			float preferredWidth = notif_1_text.preferredWidth;
			RectTransform rectTransform = notif_1_obj.rectTransform;
			float x = Mathf.Clamp(preferredWidth + pad, 180f, 400f);
			rectTransform.sizeDelta = new Vector2(x, notif_1_obj.rectTransform.sizeDelta.y);
			if (notif_img != null)
			{
				SetNotifGraphic(1, notif_img);
			}
			else
			{
				notif_1_BG.gameObject.SetActive(false);
				notif_1_overlay.gameObject.SetActive(false);
				notif_1_item.gameObject.SetActive(false);
			}
			notif_1_life = 420f;
			on_A_click = on_click;
			return;
		}
		if (notif_2_life != 0f)
		{
			BumpNotif();
		}
		notif_2_obj.gameObject.SetActive(true);
		notif_2_obj.GetComponent<Animation>().Stop();
		notif_2_obj.GetComponent<Animation>().Play("show-notif");
		notif_2_text.text = str;
		float preferredWidth2 = notif_2_text.preferredWidth;
		RectTransform rectTransform2 = notif_2_obj.rectTransform;
		float x2 = Mathf.Clamp(preferredWidth2 + pad, 180f, 400f);
		rectTransform2.sizeDelta = new Vector2(x2, notif_2_obj.rectTransform.sizeDelta.y);
		if (notif_img != null)
		{
			SetNotifGraphic(2, notif_img);
		}
		else
		{
			notif_2_BG.gameObject.SetActive(false);
			notif_2_overlay.gameObject.SetActive(false);
			notif_2_item.gameObject.SetActive(false);
		}
		notif_2_life = 420f;
		on_B_click = on_click;
	}

	private notif_index GetEmptyNotifSlot()
	{
		return default(notif_index);
	}

	private void FixedUpdate()
	{
		if (notif_1_life > 0f)
		{
			notif_1_life -= 1f;
			if (notif_1_life == 30f)
			{
				notif_1_obj.GetComponent<Animation>().Stop();
				notif_1_obj.GetComponent<Animation>().Play("hide-notif");
			}
			else if (notif_1_life == 1f)
			{
				if (notif_2_life == 0f)
				{
					notif_1_obj.GetComponent<Animation>().Stop();
					notif_1_obj.gameObject.SetActive(false);
				}
				else
				{
					BumpNotif();
					notif_2_life = 0f;
				}
			}
		}
		if (notif_2_life > 0f)
		{
			notif_2_life -= 1f;
			if (notif_2_life == 30f)
			{
				notif_2_obj.GetComponent<Animation>().Stop();
				notif_2_obj.GetComponent<Animation>().Play("hide-notif");
			}
			else if (notif_2_life == 1f)
			{
				notif_2_obj.GetComponent<Animation>().Stop();
				notif_2_obj.gameObject.SetActive(false);
			}
		}
	}
}
