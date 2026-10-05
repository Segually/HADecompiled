using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupControl : MonoBehaviour, OrderedStart
{
	public enum context
	{
		none = 0,
		message = 1,
		message_unpause_on_okay = 2,
		reward_ad_ask = 3,
		reward_ad_open = 4,
		reward_ad_skipped = 5,
		delete_1 = 6,
		delete_2 = 7,
		delete_0 = 8,
		failed_init_market = 9,
		loading_market = 10,
		accept_Terms = 11,
		quest_reward = 12,
		yesno_ACTION = 13,
		loading_NO_TIMEOUT = 14,
		sign_etc = 15,
		initial_load_world = 16
	}

	public static PopupControl Instance;

	private bool button_was_pressed_;

	private TouchScreenKeyboard keyboard;

	public GameObject black_background;

	public GameObject connecting_screen;

	public GameObject message_screen;

	public GameObject okay_button;

	public GameObject yesno_buttons;

	public GameObject reward_screen;

	public GameObject reward_collect_screen;

	public Animation loading_bubbles;

	public Text message_Text;

	public Text connecting_Text;

	public Text yes_text;

	public Text no_text;

	public Text okay_text;

	public bool popup_open;

	public string error_log = "";

	public GameObject item_sprite_prefab;

	private List<GameObject> header_item_sprites = new List<GameObject>();

	public Action on_yes_pressed;

	public Action on_no_pressed;

	public context curr_popup;

	private int cached_terms_version = -1;

	private IEnumerator show_cancel_coroutine;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
		if (this == Instance)
		{
			UnityEngine.Object.DontDestroyOnLoad(black_background);
		}
	}

	public void SetButtonWasPressed()
	{
		button_was_pressed_ = true;
		if (GameController.Instance != null)
		{
			GameController.Instance.resume_input_on_next_click = true;
		}
	}

	public bool GetButtonWasPressed()
	{
		return button_was_pressed_;
	}

	public void ClickEditSignText()
	{
		SetButtonWasPressed();
		if (message_Text.text == "<color=#a5a5a5>[Click to add text]</color>" && !Application.isEditor)
		{
			keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
		}
	}

	public void OnFinishedEditSign(string message)
	{
		ExtraInventoryData extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
		extraDataCopy.SetString("sign_text", message);
		InventoryItem new_item = new InventoryItem(GameController.Instance.interacting_element_item.item_name, extraDataCopy);
		ConstructionControl.Instance.PlayerReplaceInteracting(new_item, true);
		ShowMessage(message);
	}

	private void Update()
	{
		if (button_was_pressed_ && !GamepadInput.Instance.GetMouseButton())
		{
			button_was_pressed_ = false;
		}
		if (keyboard == null)
		{
			return;
		}
		if (keyboard.status == TouchScreenKeyboard.Status.Done)
		{
			OnFinishedEditSign(keyboard.text);
			keyboard = null;
		}
		else if (keyboard.status == TouchScreenKeyboard.Status.Canceled || keyboard.status == TouchScreenKeyboard.Status.LostFocus)
		{
			keyboard = null;
		}
	}

	public void ShowMessage(string message, context popup_context, InventoryItem item)
	{
		ShowMessage(message, popup_context, new ItemCountPair(item, 1));
	}

	public void ShowMessage(string message, context popup_context, ItemCountPair pair)
	{
		ShowMessage(message, popup_context);
		CreateHeaderItemSprite(pair.item, pair.count, 0, 1);
	}

	public void ShowMessage(string message, context popup_context, List<InventoryItem> items)
	{
		List<ItemCountPair> list = new List<ItemCountPair>();
		foreach (InventoryItem item in items)
		{
			list.Add(new ItemCountPair(item, 1));
		}
		ShowMessage(message, popup_context, list);
	}

	public void ShowMessage(string message, context popup_context, List<ItemCountPair> pairs)
	{
		if (pairs.Count == 1)
		{
			ShowMessage(message, popup_context, pairs[0]);
			return;
		}
		if (pairs.Count == 0)
		{
			ShowMessage(message, popup_context);
			return;
		}
		ShowMessage(message, popup_context);
		for (int i = 0; i < pairs.Count; i++)
		{
			CreateHeaderItemSprite(pairs[i].item, pairs[i].count, i, pairs.Count);
		}
	}

	public void ShowMessage(string message, context popup_context = context.message)
	{
		if (popup_context == context.accept_Terms || curr_popup != context.accept_Terms)
		{
			HideAll();
			if (SceneManager.GetActiveScene().name == "Menu" && MenuController.Instance.in_sumenu)
			{
				MenuController.Instance.Cancel();
			}
			popup_open = true;
			black_background.SetActive(true);
			message_screen.SetActive(true);
			okay_button.SetActive(true);
			okay_text.text = "OKAY";
			message_Text.text = message;
			curr_popup = popup_context;
			DestroyAllHeaderSprites();
		}
	}

	public void ShowYesNo(string message, string Yes, string No, context popup_context, InventoryItem obj, int count)
	{
		ShowYesNo(message, Yes, No, popup_context);
		CreateHeaderItemSprite(obj, count, 0, 1);
	}

	public void ShowYesNo(string message, string Yes, string No, context popup_context)
	{
		if (popup_context == context.accept_Terms || curr_popup != context.accept_Terms)
		{
			HideAll();
			if (SceneManager.GetActiveScene().name == "Menu" && MenuController.Instance.in_sumenu)
			{
				MenuController.Instance.Cancel();
			}
			popup_open = true;
			black_background.SetActive(true);
			message_screen.SetActive(true);
			message_Text.text = message;
			yesno_buttons.SetActive(true);
			yes_text.text = Yes;
			no_text.text = No;
			curr_popup = popup_context;
			DestroyAllHeaderSprites();
		}
	}

	private void CreateHeaderItemSprite(InventoryItem item, int count, int x_index, int x_total)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(item_sprite_prefab);
		gameObject.transform.SetParent(message_Text.gameObject.transform.parent);
		gameObject.transform.localPosition = Vector2.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one * 0.435f;
		header_item_sprites.Add(gameObject);
		ItemSprite component = gameObject.GetComponent<ItemSprite>();
		component.RedrawBasic(item, count);
		component.gameObject.SetActive(true);
		float y = message_Text.transform.localPosition.y;
		float preferredHeight = message_Text.preferredHeight;
		component.transform.localPosition = new Vector3((float)(x_index * 80) - (float)(x_total * 40 - 40), y + preferredHeight * 0.5f + 50f, 0f);
	}

	private void DestroyAllHeaderSprites()
	{
		foreach (GameObject header_item_sprite in header_item_sprites)
		{
			UnityEngine.Object.Destroy(header_item_sprite);
		}
		header_item_sprites.Clear();
	}

	public void ShowConnecting(string connect_string)
	{
		ShowConnecting(connect_string, context.none);
	}

	public void ShowConnecting(string connect_string, context popup_context)
	{
		if (curr_popup != context.accept_Terms)
		{
			HideAll();
			popup_open = true;
			black_background.SetActive(true);
			connecting_Text.text = connect_string;
			connecting_screen.SetActive(true);
			loading_bubbles.Play();
			curr_popup = popup_context;
			if (show_cancel_coroutine != null)
			{
				StopCoroutine(show_cancel_coroutine);
			}
			show_cancel_coroutine = ShowCancel();
			StartCoroutine(show_cancel_coroutine);
		}
	}

	public void ShowRewardAskPopup(AdvertControl.reward_ad_type reward_ad_type_t)
	{
		AdvertControl.Instance.reward_ad_type_t = reward_ad_type_t;
		GameController.Instance.PAUSE_GAME();
		ShowMessage("", context.reward_ad_ask);
		reward_screen.SetActive(true);
	}

	public void ShowRewardCompletePopup()
	{
		curr_popup = context.reward_ad_open;
		popup_open = true;
		black_background.SetActive(true);
		reward_collect_screen.SetActive(true);
		RewardsControl.Instance.PlayMysteryBoxAppear();
	}

	public void ShowTermsPopup()
	{
		int num = CurrAcceptedTermVersion();
		string text;
		if (num == 0)
		{
			text = "Terms of Use and Privacy Policy\n\n<size=14>Accept Version ";
		}
		else
		{
			if (MenuController.curr_terms_version <= num)
			{
				return;
			}
			text = "Updated Terms of Use and Privacy Policy\n\n<size=14>Accept Version ";
		}
		ShowYesNo(text + MenuController.curr_terms_version + " of our Terms of Use and Privacy Policy?</size>\n<size=16><color=#999999>https://www.abstractsoftwares.com/hybrid-animals-terms</color></size>", "Yes", "No", context.accept_Terms);
	}

	private void OnPressYes()
	{
		if (button_was_pressed_)
		{
			return;
		}
		SetButtonWasPressed();
		context context = curr_popup;
		switch (context)
		{
		case context.delete_1:
			MenuController.Instance.StartDeletingSlot(1);
			return;
		case context.delete_2:
			MenuController.Instance.StartDeletingSlot(2);
			return;
		case context.delete_0:
			MenuController.Instance.StartDeletingSlot(0);
			return;
		case context.failed_init_market:
			ShopControl.Instance.PressRetryLoadMarket();
			return;
		case context.accept_Terms:
		{
			List<string> list = new List<string>();
			list.Add("Version accepted: " + MenuController.curr_terms_version);
			list.Add("Date accepted: " + DateTime.Now.ToString());
			File.WriteAllLines(Path.Combine(Startup.persistentDataPath, "Terms.txt"), list.ToArray());
			cached_terms_version = MenuController.curr_terms_version;
			break;
		}
		}
		AudioControl.Instance.PlayGenericClick();
		HideAll();
		if (context == context.yesno_ACTION)
		{
			if (on_yes_pressed != null)
			{
				on_yes_pressed();
			}
			on_yes_pressed = null;
			on_no_pressed = null;
		}
	}

	private void OnPressNo()
	{
		if (button_was_pressed_)
		{
			return;
		}
		SetButtonWasPressed();
		context context = curr_popup;
		if ((uint)(context - 6) < 3u)
		{
			HideAll();
			MenuController.Instance.PressPlay(false);
		}
		else
		{
			switch (context)
			{
			case context.failed_init_market:
				ShopControl.Instance.CloseLoadingMarketScreen();
				AudioControl.Instance.PlayGenericClick();
				HideAll();
				return;
			default:
				AudioControl.Instance.PlayGenericClick();
				HideAll();
				if (context == context.yesno_ACTION)
				{
					if (on_no_pressed != null)
					{
						on_no_pressed();
					}
					on_yes_pressed = null;
					on_no_pressed = null;
				}
				return;
			case context.accept_Terms:
				break;
			}
			yesno_buttons.SetActive(false);
			ShowMessage("You must accept to play.", context.accept_Terms);
		}
		AudioControl.Instance.PlayGenericClick();
	}

	private void OnPressOkay()
	{
		if (button_was_pressed_)
		{
			return;
		}
		SetButtonWasPressed();
		bool flag = true;
		switch (curr_popup)
		{
		case context.message_unpause_on_okay:
			GameController.Instance.UNPAUSE_GAME();
			break;
		case context.reward_ad_ask:
			AdvertControl.Instance.TryShowRewardAd();
			flag = false;
			break;
		case context.reward_ad_open:
			if (AdvertControl.Instance.reward_ad_type_t != AdvertControl.reward_ad_type.on_levelup && WindowControl.Instance.curr_window == (WindowControl.window_type_t)0 && WindowControl.Instance.curr_miniwindow == (WindowControl.miniwindow_type_t)0 && ConstructionControl.Instance.done_button_context == (ConstructionControl.button_state)0)
			{
				GameplayGUIControl.Instance.ShowGameplayGui();
				GameController.Instance.UNPAUSE_GAME();
			}
			RewardsControl.Instance.PressRewardOkay();
			break;
		case context.reward_ad_skipped:
			if (AdvertControl.Instance.reward_ad_type_t != AdvertControl.reward_ad_type.on_levelup && WindowControl.Instance.curr_window == (WindowControl.window_type_t)0 && WindowControl.Instance.curr_miniwindow == (WindowControl.miniwindow_type_t)0 && ConstructionControl.Instance.done_button_context == (ConstructionControl.button_state)0)
			{
				GameplayGUIControl.Instance.ShowGameplayGui();
				GameController.Instance.UNPAUSE_GAME();
			}
			break;
		case context.loading_market:
			ShopControl.Instance.CloseLoadingMarketScreen();
			break;
		case context.accept_Terms:
			okay_button.SetActive(false);
			ShowTermsPopup();
			flag = false;
			break;
		case context.quest_reward:
			QuestControl.Instance.ShowQuestCompletePopup(QuestControl.Instance.show_quest_complete_popup_after);
			QuestControl.Instance.show_quest_complete_popup_after = "";
			flag = false;
			break;
		case context.sign_etc:
			if (GameServerConnector.Instance.FullyInGame())
			{
				GameServerReceiver.Instance.HideReportObjectButton();
			}
			break;
		case context.initial_load_world:
			TransitionControl.Instance.EndTransitionNow(false);
			break;
		}
		AudioControl.Instance.PlayGenericClick();
		if (flag)
		{
			HideAll();
		}
	}

	public void GotoGamePage()
	{
		if (Application.platform == RuntimePlatform.Android || Application.isEditor)
		{
			Application.OpenURL("https://play.google.com/store/apps/details?id=com.abstractsoft.hybridanimals");
		}
		else if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
		{
			Application.OpenURL("https://apps.apple.com/us/app/hybrid-animals/id1127937062");
		}
	}

	public int CurrAcceptedTermVersion()
	{
		if (cached_terms_version == -1)
		{
			string path = Path.Combine(Startup.persistentDataPath, "Terms.txt");
			if (!File.Exists(path))
			{
				cached_terms_version = 0;
			}
			else
			{
				cached_terms_version = int.Parse(File.ReadAllLines(path)[0].Replace("Version accepted: ", ""), Startup.parse_culture);
			}
		}
		return cached_terms_version;
	}

	public IEnumerator ShowCancel()
	{
		yield return new WaitForSeconds(12f);
		if (curr_popup != context.loading_NO_TIMEOUT)
		{
			okay_button.SetActive(true);
			okay_text.text = "CANCEL";
		}
	}

	public void HideAll()
	{
		if (curr_popup != context.accept_Terms || CurrAcceptedTermVersion() == MenuController.curr_terms_version)
		{
			popup_open = false;
			curr_popup = context.none;
			connecting_screen.SetActive(false);
			black_background.SetActive(false);
			yesno_buttons.SetActive(false);
			message_screen.SetActive(false);
			okay_button.SetActive(false);
			reward_collect_screen.SetActive(false);
			reward_screen.SetActive(false);
			DestroyAllHeaderSprites();
			if (show_cancel_coroutine != null)
			{
				StopCoroutine(show_cancel_coroutine);
				show_cancel_coroutine = null;
			}
		}
	}

	public void PressOkay()
	{
		OnPressOkay();
	}

	public void PressNo()
	{
		OnPressNo();
	}

	public void PressYes()
	{
		OnPressYes();
	}
}
