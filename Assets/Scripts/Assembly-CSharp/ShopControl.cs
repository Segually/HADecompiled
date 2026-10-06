using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopControl : MonoBehaviour, OrderedStart
{
	[Serializable]
	public struct purchase_color_define
	{
		public string name;

		public Color col;
	}

	public enum button_color_t
	{
		none = 0,
		yes_green = 1,
		no_red = 2,
		okay_blue = 3,
		price_white = 4,
		gems_cyan = 5
	}

	private enum buy_context
	{
		error = 0,
		buy_with_cash = 1,
		buy_with_gems = 2,
		buy_with_cash_or_gems = 3
	}

	public enum req_context_t
	{
		none = 0,
		insufficient = 1,
		inquire = 2
	}

	public enum shop_state
	{
		LOADING = 0,
		load_GOOD = 1,
		load_FAILED = 2
	}

	public enum popup_context
	{
		none = 0,
		revive_get_gems_yes_no = 1,
		revive_purchase_succeed = 2,
		revive_purchase_fail = 3,
		craft_yes_no = 4,
		craft_message = 5,
		payment_options = 6,
		store_message = 7,
		store_purchase_succeed = 8,
		store_purchase_fail = 9,
		unlock_with_gems_confirm = 10,
		vendor_buy_yes_no = 11,
		vendor_sell_yes_no = 12,
		vending_machine = 13
	}

	public static ShopControl Instance;

	public Text shop_your_gems_text;

	public GameObject shop_your_gems_icon;

	public Text[] market_texts_to_translate;

	public string[] purchase_structs_ordering;

	public purchase_color_define[] purchase_color_defines;

	private bool is_shop_window_open;

	private shop_state SHOP_STATE;

	public string attempt_buy_name = "";

	public List<GameObject> models_for_buttons = new List<GameObject>();

	public GameObject model_on_popup;

	public Sprite button_BG_generic;

	public Text text_GEM_count;

	public TextMeshProUGUI text_popup;

	public Text text_popup_yes;

	public Text text_popup_no;

	public Text text_popup_okay;

	public GameObject Shop_screen;

	public GameObject popup;

	public GameObject popup_okay;

	public GameObject popup_yes;

	public GameObject popup_no;

	public GameObject restore_purchase_button;

	public GameObject free_gems_button;

	public Image popup_header_bg;

	public Image popup_header_overlay;

	public Image popup_bg_outline;

	public Sprite purchase_happy;

	public Sprite purchase_sad;

	public Sprite purchase_v_sad;

	public Color sky_color;

	public GameObject breeder_base;

	public GameObject mutation_scroll_white;

	public GameObject button_nextpage;

	public GameObject button_prevpage;

	public List<GameObject> all_temporarily_disabled = new List<GameObject>();

	public Transform[] button_model_positions;

	private Dictionary<string, Dictionary<string, string>> loaded_purchaseable_structs = new Dictionary<string, Dictionary<string, string>>();

	public Sprite ring_sprite;

	public GameObject[] sparkles;

	public bool popup_open;

	public GameObject popup_close_button;

	public GameObject popup_slider;

	public ItemSprite popup_item;

	private bool pressing_how_many_slider;

	public RectTransform slider;

	public Text slider_text;

	private int slider_max_buy;

	private List<GameObject> instantiated_slider_bg_nums = new List<GameObject>();

	public GameObject slider_bg_number_prefab;

	public Image slider_line;

	public ItemSprite req_1_item_sprite;

	public ItemSprite req_2_item_sprite;

	public Text req_1_text;

	public Text req_2_text;

	public Color popup_button_col_yes_bg;

	public Color popup_button_col_yes_outline;

	public Color popup_button_col_yes_text;

	public Color popup_button_col_no_bg;

	public Color popup_button_col_no_outline;

	public Color popup_button_col_no_text;

	public Color popup_button_col_okay_bg;

	public Color popup_button_col_okay_outline;

	public Color popup_button_col_okay_text;

	public Color popup_button_col_price_bg;

	public Color popup_button_col_price_outline;

	public Color popup_button_col_price_text;

	public Color popup_button_col_gem_bg;

	public Color popup_button_col_gem_outline;

	public Color popup_button_col_gem_text;

	public bool on_pressed_buy;

	public popup_context curr_popup_context;

	private bool blobble_gem_text_on_press_OKAY;

	public GameObject[] shop_buttons;

	public Image[] button_backdrops;

	public Text[] shop_costs;

	public Text[] shop_descriptions;

	public Text[] shop_titles;

	public GameObject[] button_gem_icons;

	public Image[] button_rects;

	public GameObject model_ads_removed;

	public Color col_rect_ads_removed;

	public Color col_rect_normal;

	public Color col_button_ads_removed;

	public Color col_title_ads_removed;

	public Color col_description_ads_removed;

	private int shopPage;

	void OrderedStart.Start_0()
	{
		Instance = this;
	}

	void OrderedStart.Start_1()
	{
		InAppPurchaseControl.Instance.SignalToGame();
		foreach (Text text in market_texts_to_translate)
		{
			text.text = TranslationControl.Instance.TranslateGeneral(text.text, "Market");
		}
		shop_your_gems_icon.transform.localPosition = new Vector3(shop_your_gems_text.transform.localPosition.x + shop_your_gems_text.rectTransform.sizeDelta.x * 0.5f - shop_your_gems_text.preferredWidth - 55f, shop_your_gems_icon.transform.localPosition.y, 0f);
		popup_slider.SetActive(false);
	}

	public static string RandomString()
	{
		string text = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
		string text2 = "";
		for (int i = 0; i < 9; i++)
		{
			text2 += text[UnityEngine.Random.Range(0, text.Length)];
		}
		return text2;
	}

	public void NextPage()
	{
		if (PopupControl.Instance.popup_open)
		{
			return;
		}
		shopPage++;
		RedrawAll();
	}

	public void PrevPage()
	{
		if (PopupControl.Instance.popup_open)
		{
			return;
		}
		shopPage--;
		RedrawAll();
	}

	public void RedrawAll()
	{
		DestroyShopModels();
		RedrawShopButtons();
		CreateButtonModels();
		RedrawPageButtons();
	}

	public void RedrawPageButtons()
	{
		button_prevpage.SetActive(shopPage != 0);
		button_nextpage.SetActive(shopPage * 3 + 3 < purchase_structs_ordering.Length);
	}

	public void DisableNearbyObjects(Vector3 origin, float range, bool keep_player_active)
	{
		List<GameObject> disabled = new List<GameObject>();
		if (Vector3.Distance(origin, breeder_base.transform.position) < range)
		{
			if (!all_temporarily_disabled.Contains(breeder_base)) disabled.Add(breeder_base);
			if (!all_temporarily_disabled.Contains(GameController.Instance.scenic_elevator)) disabled.Add(GameController.Instance.scenic_elevator);
		}
		foreach (KeyValuePair<string, GameObject> combatant in MobControl.Instance.active_combatants)
		{
			GameObject obj = combatant.Value;
			if (obj != null && obj.GetComponent<SharedCreature>() != null)
			{
				if (keep_player_active && obj == GameController.Instance.player) continue;
				if (!all_temporarily_disabled.Contains(obj)) disabled.Add(obj);
			}
		}
		ChunkControl.Instance.TemporarilyDisableChunkObjects(origin, range, disabled);
		foreach (GameObject obj in disabled)
		{
			obj.SetActive(false);
			all_temporarily_disabled.Add(obj);
		}
	}

	public void ReEnableNearbyObjects()
	{
		foreach (GameObject obj in all_temporarily_disabled)
		{
			if (obj == null) continue;
			obj.SetActive(true);
			SharedCreature creature = obj.GetComponent<SharedCreature>();
			if (creature != null && creature.is_local_mob)
			{
				if (GameController.Instance.player != null && obj == GameController.Instance.player) continue;
				obj.GetComponent<SharedCreature>().ReEnable();
				obj.GetComponent<CreatureBrain>().ReEnable();
			}
		}
		all_temporarily_disabled.Clear();
	}

	public void CreateButtonModels()
	{
		for (int i = 0; i < 3; i++)
		{
			int index = i + shopPage * 3;
			if (index < purchase_structs_ordering.Length)
			{
				string name = purchase_structs_ordering[index];
				string key = GetPurchaseableKey(name);
				string path = GetPurchaseableModelPath(name);
				if (key == "no_ads_subscr" && !AdvertControl.Instance.AdsActive())
				{
					GameObject holder = CreateButtonModelHolder(i);
					OnShopModelLoaded(UnityEngine.Object.Instantiate(model_ads_removed), holder);
				}
				else
				{
					GameObject holder = CreateButtonModelHolder(i);
					if (!Startup.StringNullOrWhitespace(path))
					{
						GameObject check_not_null = holder;
						ResourceControl.Instance.AsyncInstantiateShopModel(path, delegate(GameObject new_model)
						{
							if (check_not_null == null)
							{
								UnityEngine.Object.Destroy(new_model);
								return;
							}
							OnShopModelLoaded(new_model, holder);
						});
					}
					else
					{
						OnShopModelLoaded(new GameObject("Error"), holder);
					}
				}
			}
		}
		foreach (GameObject sparkle in sparkles)
		{
			sparkle.SetActive(true);
		}
	}

	private GameObject CreateButtonModelHolder(int i)
	{
		GameObject holder = new GameObject("model holder");
		holder.transform.SetParent(WindowControl.Instance.gui_canvas.transform);
		holder.transform.position = button_model_positions[i].position;
		holder.transform.localPosition += Vector3.back * 56f;
		holder.transform.localPosition += Vector3.up * 3f;
		holder.transform.localScale = Vector3.one * 10.5f;
		models_for_buttons.Add(holder);
		return holder;
	}

	private GameObject CreatePopupModelHolder()
	{
		GameObject holder = new GameObject("model holder");
		holder.transform.SetParent(WindowControl.Instance.gui_canvas.transform);
		holder.transform.position = popup_header_bg.transform.position;
		holder.transform.localPosition += Vector3.back * 123f;
		holder.transform.localScale = Vector3.one * 10.5f;
		model_on_popup = holder;
		return holder;
	}

	private void OnShopModelLoaded(GameObject new_model, GameObject model_holder)
	{
		new_model.transform.SetParent(model_holder.transform);
		new_model.transform.localPosition = Vector3.zero;
		new_model.transform.localScale = Vector3.one;
		new_model.transform.localRotation = Quaternion.identity;
	}

	public Color GetColor(string bg_color)
	{
		for (int i = 0; i < purchase_color_defines.Length; i++)
		{
			if (purchase_color_defines[i].name == bg_color)
			{
				return purchase_color_defines[i].col;
			}
		}
		return purchase_color_defines[0].col;
	}

	public string GetPurchaseableKey(string iap_name)
	{
		if (!loaded_purchaseable_structs.ContainsKey(iap_name))
		{
			LoadIapStruct(iap_name);
		}
		return loaded_purchaseable_structs[iap_name].ContainsKey("IAP_KEY") ? loaded_purchaseable_structs[iap_name]["IAP_KEY"] : "";
	}

	public string GetPurchaseableModelPath(string iap_name)
	{
		if (!loaded_purchaseable_structs.ContainsKey(iap_name))
		{
			LoadIapStruct(iap_name);
		}
		return loaded_purchaseable_structs[iap_name].ContainsKey("Model_path") ? loaded_purchaseable_structs[iap_name]["Model_path"] : "";
	}

	public string GetPurchaseableAltKey(string iap_name)
	{
		if (!loaded_purchaseable_structs.ContainsKey(iap_name))
		{
			LoadIapStruct(iap_name);
		}
		return loaded_purchaseable_structs[iap_name].ContainsKey("Alt_IAP_KEY") ? loaded_purchaseable_structs[iap_name]["Alt_IAP_KEY"] : "";
	}

	public string GetPurchaseableGemPrice(string iap_name)
	{
		if (!loaded_purchaseable_structs.ContainsKey(iap_name))
		{
			LoadIapStruct(iap_name);
		}
		return loaded_purchaseable_structs[iap_name].ContainsKey("Gem_price") ? loaded_purchaseable_structs[iap_name]["Gem_price"] : "";
	}

	public string GetPurchaseableDescription(string iap_name)
	{
		if (!loaded_purchaseable_structs.ContainsKey(iap_name))
		{
			LoadIapStruct(iap_name);
		}
		return loaded_purchaseable_structs[iap_name].ContainsKey("Description") ? loaded_purchaseable_structs[iap_name]["Description"] : "";
	}

	public string GetPurchaseableBgColor(string iap_name)
	{
		if (!loaded_purchaseable_structs.ContainsKey(iap_name))
		{
			LoadIapStruct(iap_name);
		}
		return loaded_purchaseable_structs[iap_name].ContainsKey("Bg_color") ? loaded_purchaseable_structs[iap_name]["Bg_color"] : "";
	}

	public string GetPurchaseableType(string iap_name)
	{
		if (!loaded_purchaseable_structs.ContainsKey(iap_name))
		{
			LoadIapStruct(iap_name);
		}
		return loaded_purchaseable_structs[iap_name].ContainsKey("Type") ? loaded_purchaseable_structs[iap_name]["Type"] : "";
	}

	private void LoadIapStruct(string iap_name)
	{
		Dictionary<string, string> values = new Dictionary<string, string>();
		bool file_exists = false;
		List<string> lines = ResourceControl.Instance.GetTextFileLines("PurchaseStructs/" + iap_name, ref file_exists);
		if (file_exists)
		{
			foreach (string line in lines)
			{
				if (!Startup.StringNullOrWhitespace(line))
				{
					int index = line.IndexOf('=');
					if (index != -1)
					{
						values.Add(line.Substring(0, index - 1), line.Substring(index + 2, line.Length - (index + 2)));
					}
				}
			}
		}
		loaded_purchaseable_structs.Add(iap_name, values);
	}

	public string GetCashCost(string iap_name)
	{
		string key = GetPurchaseableKey(iap_name);
		string alt_key = GetPurchaseableAltKey(iap_name);
		if (InAppPurchaseControl.Instance.price_infos.ContainsKey(key))
		{
			return InAppPurchaseControl.Instance.price_infos[key];
		}
		if (!Startup.StringNullOrWhitespace(alt_key) && InAppPurchaseControl.Instance.price_infos.ContainsKey(alt_key))
		{
			return InAppPurchaseControl.Instance.price_infos[alt_key];
		}
		return "";
	}

	public int GetGemCost(string gem_price)
	{
		if (gem_price == "cheap")
		{
			return 7;
		}
		if (gem_price == "medium")
		{
			return 15;
		}
		return 0;
	}

	public void ClickPurchaseableItem(int buttonIndex)
	{
		if (PopupControl.Instance.popup_open) return;
		int index = shopPage * 3 + buttonIndex;
		string name = purchase_structs_ordering[index];
		string key = GetPurchaseableKey(name);
		string alt = GetPurchaseableAltKey(name);
		string type = GetPurchaseableType(name);
		int gems = GetGemCost(GetPurchaseableGemPrice(name));
		string cash = GetCashCost(name);
		Color color = GetColor(GetPurchaseableBgColor(name));
		string path = GetPurchaseableModelPath(name);
		if (!InAppPurchaseControl.Instance.IsPurchased(key, alt))
		{
			if (gems == 0 && cash == "") return;
			AudioControl.Instance.PlayGenericClick();
			if (key == "doCOMPANION")
			{
				if (GameServerConnector.Instance.FullyInGame())
				{
					if (GameServerReceiver.Instance.max_companions == 0)
					{
						PopupControl.Instance.ShowMessage("Companions are not allowed on this server.", PopupControl.context.message);
						return;
					}
					if (GameServerReceiver.Instance.max_companions < CompanionController.Instance.active_companions.Count + 1)
					{
						PopupControl.Instance.ShowMessage("You may only have " + GameServerReceiver.Instance.max_companions + " companions on this server.", PopupControl.context.message);
						return;
					}
				}
				if (CompanionController.Instance.active_companions.Count >= 2)
				{
					PopupControl.Instance.ShowMessage("You can only have 2 followers at a time!", PopupControl.context.message);
					return;
				}
			}
			if (gems == 0 && cash != "")
			{
				BuyItem(key);
				return;
			}
			if (gems != 0 && cash == "")
			{
				attempt_buy_name = purchase_structs_ordering[index];
				if (HasEnoughGems()) BuyUsingGems();
				else ShowNotEnoughGemsPopup();
				return;
			}
			if (gems == 0 || cash == "") return;
			GameObject holder = CreatePopupModelHolder();
			if (!Startup.StringNullOrWhitespace(path))
			{
				GameObject check_not_null = holder;
				ResourceControl.Instance.AsyncInstantiateShopModel(path, delegate(GameObject new_model)
				{
					if (check_not_null == null)
					{
						UnityEngine.Object.Destroy(new_model);
						return;
					}
					OnShopModelLoaded(new_model, holder);
				});
			}
			else OnShopModelLoaded(new GameObject("Error"), holder);
			attempt_buy_name = purchase_structs_ordering[index];
			ShowShopPopup(TranslationControl.Instance.TranslateGeneral("Pay with cash", "Market"), button_color_t.price_white, TranslationControl.Instance.TranslateGeneral("Use gems", "Market"), button_color_t.gems_cyan, true, "<color=#eeeeee>" + TranslationControl.Instance.TranslateGeneral("How would you like to get", "Market") + "</color>\n" + TranslationControl.Instance.TranslateGeneral(name, "Market"), color, button_BG_generic, Color.white, ring_sprite, new Color(1f, 1f, 1f, 0.6f), color, popup_context.payment_options);
			DestroyShopModels();
		}
		else if (type == "subscription")
		{
			if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
				PopupControl.Instance.on_yes_pressed = delegate { Application.OpenURL("https://support.google.com/googleplay/answer/7018481"); };
			else if (Application.platform == RuntimePlatform.IPhonePlayer)
				PopupControl.Instance.on_yes_pressed = delegate { Application.OpenURL("https://support.apple.com/en-ca/HT202039"); };
			PopupControl.Instance.ShowYesNo("If you would like to remove a subscription,\nyou can do so by pressing 'Manage' below.", "MANAGE", "Cancel", PopupControl.context.yesno_ACTION);
		}
	}

	private void BuyWithCash(string iap_key)
	{
		PopupControl.Instance.ShowConnecting("Processing Transaction", PopupControl.context.loading_NO_TIMEOUT);
		on_pressed_buy = true;
		AdvertControl.Instance.DONT_DISCONNECT = true;
		InAppPurchaseControl.Instance.BuyProductID(iap_key);
	}

	private bool HasEnoughGems()
	{
		int cost = GetGemCost(GetPurchaseableGemPrice(attempt_buy_name));
		return cost <= PlayerData.Instance.GetGlobalShort("GEMS");
	}

	private void ShowNotEnoughGemsPopup()
	{
		PopupControl.Instance.ShowMessage(TranslationControl.Instance.TranslateGeneral("You don't have enough gems!", "Market"), PopupControl.context.message);
	}

	public void DestroyShopModels()
	{
		foreach (GameObject model in models_for_buttons)
		{
			UnityEngine.Object.Destroy(model);
		}
		models_for_buttons.Clear();
		foreach (GameObject sparkle in sparkles)
		{
			sparkle.SetActive(false);
		}
	}

	public void PressShopButton()
	{
		PopupControl.Instance.SetButtonWasPressed();
		if (WindowControl.Instance.CanOpenGenericWindow())
		{
			WindowControl.Instance.DoOpenGenericWindow();
			TryOpenShop();
		}
	}

	public void ShowShopPopup(string YES_str, button_color_t YES_col, string NO_str, button_color_t NO_col, bool show_close, string TEXT, Color textColor, InventoryItem item, int count, Color outlineColor, popup_context context, int slider_max_buy = -1, InventoryItem req_1_item = null, int req_1_count = -1, InventoryItem req_2_item = null, int req_2_count = -1, req_context_t req_context = req_context_t.none)
	{
		this.slider_max_buy = slider_max_buy;
		bool tall_window = slider_max_buy != -1 || req_1_item != null || req_2_item != null;
		GenericRedrawPopup(YES_str, YES_col, NO_str, NO_col, show_close, TEXT, textColor, outlineColor, context, tall_window, req_1_item, req_1_count, req_2_item, req_2_count, req_context);
		popup_item.gameObject.SetActive(true);
		popup_header_bg.gameObject.SetActive(false);
		popup_header_overlay.gameObject.SetActive(false);
		switch (context)
		{
		case popup_context.vendor_sell_yes_no:
			popup_item.RedrawAsSellPopupHeader(item, count);
			break;
		case popup_context.vendor_buy_yes_no:
			popup_item.RedrawAsBuyPopupHeader(item, count);
			break;
		default:
			popup_item.RedrawBasic(item, count);
			break;
		}
	}

	public void ShowShopPopup(string YES_str, button_color_t YES_col, string NO_str, button_color_t NO_col, bool show_close, string TEXT, Color textColor, Sprite header_bg_sprite, Color header_bg_col, Sprite header_overlay_sprite, Color header_overlay_col, Color outlineColor, popup_context context, int slider_max_buy = -1, InventoryItem req_1_item = null, int req_1_count = -1, InventoryItem req_2_item = null, int req_2_count = -1, req_context_t req_context = req_context_t.none)
	{
		this.slider_max_buy = slider_max_buy;
		bool tall_window = slider_max_buy != -1 || req_1_item != null || req_2_item != null;
		GenericRedrawPopup(YES_str, YES_col, NO_str, NO_col, show_close, TEXT, textColor, outlineColor, context, tall_window, req_1_item, req_1_count, req_2_item, req_2_count, req_context);
		popup_item.gameObject.SetActive(false);
		popup_header_bg.gameObject.SetActive(true);
		popup_header_bg.sprite = header_bg_sprite;
		popup_header_bg.color = header_bg_col;
		if (header_overlay_sprite != null)
		{
			popup_header_overlay.gameObject.SetActive(true);
			popup_header_overlay.sprite = header_overlay_sprite;
			popup_header_overlay.color = header_overlay_col;
		}
		else
		{
			popup_header_overlay.gameObject.SetActive(false);
		}
	}

	public void PressHowManySlider()
	{
		pressing_how_many_slider = true;
	}

	public void ReleaseHowManySlider()
	{
		pressing_how_many_slider = false;
	}

	private void Update()
	{
		if (pressing_how_many_slider)
		{
			float x = GamepadInput.Instance.GetMousePosition().x;
			int width = Screen.width;
			float scaleFactor = WindowControl.Instance.gui_canvas.scaleFactor;
			float num = slider_line.rectTransform.sizeDelta.x * 0.5f - slider.sizeDelta.x * 0.5f;
			float value = (x - (float)width * 0.5f) / scaleFactor;
			TryRedrawSlider(Mathf.InverseLerp(0f - num, num, value), false);
		}
	}

	private void TryRedrawSlider(float p, bool force_redraw)
	{
		int num = (int)Mathf.Round(Mathf.Lerp(1f, slider_max_buy, p));
		if (num != inventory_ctr.Instance.trying_to_craft.count || force_redraw)
		{
			inventory_ctr.Instance.trying_to_craft = new ItemCountPair(inventory_ctr.Instance.trying_to_craft.item, num);
			if (curr_popup_context == popup_context.vendor_sell_yes_no)
			{
				popup_item.RedrawAsSellPopupHeader(inventory_ctr.Instance.trying_to_craft.item, inventory_ctr.Instance.trying_to_craft.count);
			}
			else if (curr_popup_context == popup_context.vendor_buy_yes_no)
			{
				popup_item.RedrawAsBuyPopupHeader(inventory_ctr.Instance.trying_to_craft.item, inventory_ctr.Instance.trying_to_craft.count);
			}
			float t = ((slider_max_buy != 1) ? Mathf.Clamp01(((float)inventory_ctr.Instance.trying_to_craft.count - 1f) / ((float)slider_max_buy - 1f)) : 0f);
			float num2 = slider_line.rectTransform.sizeDelta.x * 0.5f - slider.sizeDelta.x * 0.5f;
			slider.anchoredPosition = new Vector2(t * (num2 + num2) - num2, 0f);
			slider_text.text = inventory_ctr.Instance.trying_to_craft.count.ToString() ?? "";
		}
	}

	private void GenericRedrawPopup(string YES_str, button_color_t YES_col, string NO_str, button_color_t NO_col, bool show_close, string TEXT, Color textColor, Color outlineColor, popup_context context, bool tall_window, InventoryItem req_1_item, int req_1_count, InventoryItem req_2_item, int req_2_count, req_context_t req_context)
	{
		popup_open = true;
		popup.SetActive(true);
		WindowControl.Instance.close_button.SetActive(false);
		if (NO_str != "")
		{
			popup_okay.SetActive(false);
			popup_yes.SetActive(true);
			popup_no.SetActive(true);
			text_popup_yes.text = YES_str;
			text_popup_no.text = NO_str;
			if (YES_col != button_color_t.none)
			{
				ColorizeShopPopupButton(popup_yes, text_popup_yes, YES_col);
			}
			if (NO_col != button_color_t.none)
			{
				ColorizeShopPopupButton(popup_no, text_popup_no, NO_col);
			}
		}
		else
		{
			popup_okay.SetActive(true);
			popup_yes.SetActive(false);
			popup_no.SetActive(false);
			text_popup_okay.text = YES_str;
			if (YES_col != button_color_t.none)
			{
				ColorizeShopPopupButton(popup_okay, text_popup_okay, YES_col);
			}
		}
		popup_close_button.SetActive(show_close);
		text_popup.text = TEXT;
		text_popup.color = textColor;
		popup_bg_outline.color = outlineColor;
		curr_popup_context = context;
		popup_slider.SetActive(slider_max_buy != -1);
		float y;
		if (!tall_window)
		{
			text_popup.rectTransform.anchoredPosition = new Vector2(0f, -69.7f);
			text_popup.rectTransform.sizeDelta = new Vector2(297f, 61f);
			y = 180f;
		}
		else
		{
			text_popup.rectTransform.anchoredPosition = new Vector2(0f, -54.4f);
			text_popup.rectTransform.sizeDelta = new Vector2(297f, 35f);
			y = 185f;
		}
		if (slider_max_buy != -1)
		{
			float y2 = slider_line.rectTransform.sizeDelta.y;
			float x;
			switch (slider_max_buy)
			{
			case 4:
				x = 195f;
				break;
			case 3:
				x = 140f;
				break;
			case 2:
				x = 100f;
				break;
			default:
				x = 250f;
				break;
			}
			slider_line.rectTransform.sizeDelta = new Vector2(x, y2);
			float x2 = Mathf.Max(slider_line.rectTransform.sizeDelta.x / (float)slider_max_buy, 45f);
			slider.sizeDelta = new Vector2(x2, slider.sizeDelta.y);
			float num = slider_line.rectTransform.sizeDelta.x * 0.5f - slider.sizeDelta.x * 0.5f;
			foreach (GameObject instantiated_slider_bg_num in instantiated_slider_bg_nums)
			{
				UnityEngine.Object.Destroy(instantiated_slider_bg_num);
			}
			instantiated_slider_bg_nums.Clear();
			int num2 = ((slider_max_buy > 5) ? 6 : slider_max_buy);
			for (int i = 0; i < num2; i++)
			{
				float num3 = ((num2 - 1 != 0) ? Mathf.Clamp01((float)i / (float)(num2 - 1)) : 0f);
				GameObject gameObject = UnityEngine.Object.Instantiate(slider_bg_number_prefab);
				gameObject.transform.SetParent(slider_bg_number_prefab.transform.parent);
				gameObject.transform.localPosition = slider_bg_number_prefab.transform.localPosition;
				gameObject.transform.localRotation = slider_bg_number_prefab.transform.localRotation;
				gameObject.transform.localScale = slider_bg_number_prefab.transform.localScale;
				((RectTransform)gameObject.transform).anchoredPosition = new Vector2((num + num) * num3 - num, 0f);
				gameObject.transform.SetAsFirstSibling();
				gameObject.SetActive(true);
				if (i == 0)
				{
					gameObject.GetComponent<Text>().text = "1";
				}
				else if (i == num2 - 1)
				{
					gameObject.GetComponent<Text>().text = slider_max_buy.ToString() ?? "";
				}
				else
				{
					gameObject.GetComponent<Text>().text = ((int)(Mathf.Max(num3, 0f) * ((float)slider_max_buy - 1f) + 1f)).ToString() ?? "";
				}
				instantiated_slider_bg_nums.Add(gameObject);
			}
			TryRedrawSlider((slider_max_buy != 1) ? Mathf.Clamp01(((float)inventory_ctr.Instance.trying_to_craft.count - 1f) / ((float)slider_max_buy - 1f)) : 0f, true);
		}
		bool flag;
		bool flag2;
		if (req_1_item == null && req_2_item == null)
		{
			flag = false;
			flag2 = false;
		}
		else if (req_1_item != null && req_2_item == null)
		{
			flag = true;
			flag2 = false;
		}
		else if (req_1_item != null && req_2_item != null)
		{
			flag = true;
			flag2 = req_2_item.item_name != "";
		}
		else
		{
			flag = false;
			flag2 = false;
		}
		if (flag)
		{
			req_1_item_sprite.gameObject.SetActive(true);
			req_1_text.gameObject.SetActive(true);
			req_1_item_sprite.RedrawBasicHideCount(req_1_item, req_1_count);
			req_1_text.text = "x" + req_1_count + " " + inventory_ctr.Instance.GetFullItemName(req_1_item) + ((req_context == req_context_t.inquire) ? "?" : "");
			switch (req_context)
			{
			case req_context_t.insufficient:
				req_1_text.color = new Color(1f, 0.23137255f, 0.23137255f, 1f);
				break;
			case req_context_t.none:
				req_1_text.color = new Color(1f, 1f, 1f, 1f);
				break;
			case req_context_t.inquire:
				req_1_text.color = new Color(0.2901961f, 1f, 0.5137255f, 1f);
				break;
			}
		}
		else
		{
			req_1_item_sprite.gameObject.SetActive(false);
			req_1_text.gameObject.SetActive(false);
		}
		if (!flag2)
		{
			req_2_item_sprite.gameObject.SetActive(false);
			req_2_text.gameObject.SetActive(false);
			if (flag)
			{
				float num4 = Mathf.Min(req_1_text.preferredWidth, 120f);
				req_1_text.transform.localPosition = new Vector3(num4 * 0.5f + (num4 + 3f + 28f) * -0.5f, req_1_text.transform.localPosition.y, 0f);
				req_1_item_sprite.transform.localPosition = req_1_text.transform.localPosition + Vector3.right * (num4 * 0.5f + 3f + 14f);
			}
		}
		else
		{
			req_2_item_sprite.gameObject.SetActive(true);
			req_2_text.gameObject.SetActive(true);
			req_2_item_sprite.RedrawBasicHideCount(req_2_item, req_2_count);
			req_2_text.text = "x" + req_2_count + " " + inventory_ctr.Instance.GetFullItemName(req_2_item) + ((req_context == req_context_t.inquire) ? "?" : "");
			switch (req_context)
			{
			case req_context_t.insufficient:
				req_2_text.color = new Color(1f, 0.23137255f, 0.23137255f, 1f);
				break;
			case req_context_t.none:
				req_2_text.color = new Color(1f, 1f, 1f, 1f);
				break;
			case req_context_t.inquire:
				req_2_text.color = new Color(0.2901961f, 1f, 0.5137255f, 1f);
				break;
			}
			if (flag)
			{
				float num5 = Mathf.Min(req_1_text.preferredWidth, 120f);
				float num6 = Mathf.Min(req_2_text.preferredWidth, 120f);
				float num7 = (num5 + 3f + 28f + 17f + num6 + 3f + 28f) * 0.5f;
				req_1_text.transform.localPosition = new Vector3(num5 * 0.5f - num7, req_1_text.transform.localPosition.y, 0f);
				req_1_item_sprite.transform.localPosition = req_1_text.transform.localPosition + Vector3.right * (num5 * 0.5f + 3f + 14f);
				req_2_text.transform.localPosition = new Vector3(num7 - 28f - 3f - num6 * 0.5f, req_2_text.transform.localPosition.y, 0f);
				req_2_item_sprite.transform.localPosition = req_2_text.transform.localPosition + Vector3.right * (num6 * 0.5f + 3f + 14f);
			}
		}
		popup_bg_outline.rectTransform.sizeDelta = new Vector2(popup_bg_outline.rectTransform.sizeDelta.x, y);
	}

	private void ColorizeShopPopupButton(GameObject button, Text text, button_color_t col)
	{
		switch (col)
		{
		case button_color_t.yes_green:
			button.GetComponent<Image>().color = popup_button_col_yes_bg;
			button.GetComponent<Outline>().effectColor = popup_button_col_yes_outline;
			text.color = popup_button_col_yes_text;
			break;
		case button_color_t.no_red:
			button.GetComponent<Image>().color = popup_button_col_no_bg;
			button.GetComponent<Outline>().effectColor = popup_button_col_no_outline;
			text.color = popup_button_col_no_text;
			break;
		case button_color_t.okay_blue:
			button.GetComponent<Image>().color = popup_button_col_okay_bg;
			button.GetComponent<Outline>().effectColor = popup_button_col_okay_outline;
			text.color = popup_button_col_okay_text;
			break;
		case button_color_t.price_white:
			button.GetComponent<Image>().color = popup_button_col_price_bg;
			button.GetComponent<Outline>().effectColor = popup_button_col_price_outline;
			text.color = popup_button_col_price_text;
			break;
		case button_color_t.gems_cyan:
			button.GetComponent<Image>().color = popup_button_col_gem_bg;
			button.GetComponent<Outline>().effectColor = popup_button_col_gem_outline;
			text.color = popup_button_col_gem_text;
			break;
		}
	}

	public void PressFreeGemsButton()
	{
		if (PopupControl.Instance.popup_open)
		{
			return;
		}
		AudioControl.Instance.PlayGenericClick();
		PopupControl.Instance.ShowRewardAskPopup(AdvertControl.reward_ad_type.on_free_gems_button);
	}

	public void ShopFailedLoading()
	{
		SHOP_STATE = shop_state.load_FAILED;
		if (is_shop_window_open)
		{
			PopupControl.Instance.ShowYesNo("Could not connect to Market.\nTry again?", "Yes", "No", PopupControl.context.failed_init_market);
		}
	}

	public void CloseLoadingMarketScreen()
	{
		GameplayGUIControl.Instance.ShowGameplayGui();
		GameController.Instance.UNPAUSE_GAME();
		is_shop_window_open = false;
	}

	public void ShopFinishedLoading()
	{
		SHOP_STATE = shop_state.load_GOOD;
		if (is_shop_window_open)
		{
			ShowMarketScreen();
			PopupControl.Instance.HideAll();
		}
	}

	public void PressRetryLoadMarket()
	{
		InAppPurchaseControl.Instance.InitializePurchasing();
		SHOP_STATE = shop_state.LOADING;
		PopupControl.Instance.ShowConnecting("Loading Mutant Market", PopupControl.context.loading_market);
	}

	public void CloseGemsWindow(bool redraw_models)
	{
		AudioControl.Instance.PlayGenericClick();
		WindowPrefabsControl.Instance.DestroyScreen("Shop-getgems");
		GameController.Instance.revive_cancel();
	}

	public void ShowMarketScreen()
	{
		Shop_screen.SetActive(true);
		free_gems_button.SetActive(true);
		restore_purchase_button.SetActive(true);
		text_GEM_count.text = PlayerData.Instance.GetGlobalShort("GEMS").ToString() ?? "";
		RedrawShopButtons();
		CreateButtonModels();
		WindowControl.Instance.OpenWindow(WindowControl.window_type_t.mutant_market);
	}

	public void HideMarketScreen()
	{
		is_shop_window_open = false;
		DestroyShopModels();
		Shop_screen.SetActive(false);
		free_gems_button.SetActive(false);
		restore_purchase_button.SetActive(false);
	}

	public void BuyGemsFromRevive(int amount)
	{
		WindowPrefabsControl.Instance.DestroyScreen("Shop-getgems");
		WindowControl.Instance.close_button.SetActive(false);
		WindowControl.Instance.curr_window = WindowControl.window_type_t.none;
		if (amount == 30)
		{
			BuyItem("30gems");
		}
		else if (amount == 60)
		{
			BuyItem("60gems");
		}
	}

	public void BuyItem(string product_id)
	{
		PopupControl.Instance.ShowConnecting("Processing Transaction", PopupControl.context.loading_NO_TIMEOUT);
		on_pressed_buy = true;
		AdvertControl.Instance.DONT_DISCONNECT = true;
		InAppPurchaseControl.Instance.BuyProductID(product_id);
	}

	private void FixedUpdate()
	{
		if (is_shop_window_open)
		{
			text_GEM_count.color = Color.Lerp(text_GEM_count.color, Color.white, Time.fixedDeltaTime * 0.5f);
			text_GEM_count.transform.localScale = Vector3.Lerp(text_GEM_count.transform.localScale, Vector3.one * 1.2f, Time.fixedDeltaTime);
		}
	}

	public void OnTransactionSucceed(string definition_id)
	{
		PopupControl.Instance.HideAll();
		if (definition_id.Equals("30gems", StringComparison.Ordinal))
		{
			PlayerData.Instance.SetGlobalShort("GEMS", (short)(PlayerData.Instance.GetGlobalShort("GEMS") + 30));
			blobble_gem_text_on_press_OKAY = true;
		}
		else if (definition_id.Equals("60gems", StringComparison.Ordinal))
		{
			PlayerData.Instance.SetGlobalShort("GEMS", (short)(PlayerData.Instance.GetGlobalShort("GEMS") + 60));
			blobble_gem_text_on_press_OKAY = true;
		}
		else if (definition_id.Equals("no_ads_subscr", StringComparison.Ordinal))
			AdvertControl.Instance.subscribed_to_remove_ads = true;
		else if (definition_id == "doCOMPANION")
		{
			WindowControl.Instance.close_button.SetActive(false);
			WindowControl.Instance.curr_window = WindowControl.window_type_t.none;
			HideMarketScreen();
			CompanionController.Instance.CreateAnimatedEgg(4, true, "", "");
			if (ChunkControl.Instance.player_zone == "overworld") DisableNearbyObjects(CompanionController.Instance.EGG.transform.position, 7f, false);
			return;
		}
		else if (definition_id == "doMUTATE")
		{
			WindowControl.Instance.close_button.SetActive(false);
			WindowControl.Instance.curr_window = WindowControl.window_type_t.none;
			HideMarketScreen();
			BreedControl.Instance.gameObject.SetActive(true);
			BreedControl.Instance.TransitionBackToBreeder(BreedControl.breeder_transition.on_mutation);
			mutation_scroll_white.SetActive(true);
			mutation_scroll_white.GetComponent<CanvasGroup>().alpha = 1f;
			if (ChunkControl.Instance.player_zone == "overworld") DisableNearbyObjects(BreedControl.Instance.spawn_mutant.position, 7f, true);
			return;
		}
		else
		{
			bool found = false;
			foreach (string name in purchase_structs_ordering)
			{
				string key = GetPurchaseableKey(name);
				string alt = GetPurchaseableAltKey(name);
				string type = GetPurchaseableType(name);
				if ((definition_id.Equals(key, StringComparison.Ordinal) || (!Startup.StringNullOrWhitespace(alt) && definition_id.Equals(alt, StringComparison.Ordinal))) && type == "permanent")
				{
					PlayerData.Instance.SetGlobalShort(key, 1);
					found = true;
					break;
				}
			}
			if (!found)
			{
				OnTransactionFailed();
				return;
			}
		}
		if (!on_pressed_buy) return;
		on_pressed_buy = false;
		popup_context context = is_shop_window_open ? popup_context.store_purchase_succeed : popup_context.revive_purchase_succeed;
		ShowShopPopup("OKAY", button_color_t.okay_blue, "", button_color_t.none, false, TranslationControl.Instance.TranslateGeneral("THANK YOU for supporting us!", "Market") + "\n<color=#eeeeee>" + TranslationControl.Instance.TranslateGeneral("Your generosity helps us improve the game!", "Market") + "</color>", new Color(0.8666667f, 0.9411765f, 0.16078432f, 1f), purchase_happy, Color.white, null, Color.white, new Color(0.5294118f, 0.50980395f, 0.29803923f, 1f), context);
		if (is_shop_window_open) DestroyShopModels();
		GameController.Instance.sound_levelButton();
	}

	public void OnTransactionFailed()
	{
		if (!is_shop_window_open) return;
		PopupControl.Instance.HideAll();
		popup_context context = is_shop_window_open ? popup_context.store_purchase_fail : popup_context.revive_purchase_fail;
		ShowShopPopup("OKAY", button_color_t.okay_blue, "", button_color_t.none, false, "<color=#00aaff>OOPS!</color> Something went wrong.\nThe transaction did not complete.", Color.white, purchase_sad, Color.white, null, Color.white, new Color(0.29803923f, 0.50980395f, 0.5294118f, 1f), context);
		if (is_shop_window_open) DestroyShopModels();
	}

	public void PressRestorePurchases()
	{
		InAppPurchaseControl.Instance.RestoreSubscription();
	}

	public void TryOpenShop()
	{
		AudioControl.Instance.PlayGenericClick();
		is_shop_window_open = true;
		switch (SHOP_STATE)
		{
		case shop_state.LOADING:
			PopupControl.Instance.ShowConnecting("Loading Mutant Market", PopupControl.context.loading_market);
			break;
		case shop_state.load_GOOD:
			ShowMarketScreen();
			break;
		case shop_state.load_FAILED:
			PopupControl.Instance.ShowYesNo("Could not connect to Market.\nTry again?", "Yes", "No", PopupControl.context.failed_init_market);
			break;
		}
	}

	private void FromPopupToStore(popup_context prev_context)
	{
		if (model_on_popup != null)
		{
			UnityEngine.Object.Destroy(model_on_popup);
		}
		RedrawShopButtons();
		CreateButtonModels();
		WindowControl.Instance.close_button.SetActive(true);
	}

	public void PopupYesPressed()
	{
		popup_open = false;
		popup.SetActive(false);
		popup_context popup_context = curr_popup_context;
		curr_popup_context = popup_context.none;
		switch (popup_context)
		{
		case popup_context.revive_get_gems_yes_no:
			GameController.Instance.revive_gems_accept();
			break;
		case popup_context.craft_yes_no:
			inventory_ctr.Instance.BeginCraftAnimation();
			break;
		case popup_context.payment_options:
			FromPopupToStore(popup_context);
			BuyItem(GetPurchaseableKey(attempt_buy_name));
			break;
		case popup_context.vendor_buy_yes_no:
			inventory_ctr.Instance.AcceptBuy();
			break;
		case popup_context.vendor_sell_yes_no:
			inventory_ctr.Instance.CompleteSell();
			break;
		case popup_context.vending_machine:
			VendingMachineControl.Instance.PressAcceptBuy();
			break;
		}
	}

	public void PopupNoPressed()
	{
		popup_open = false;
		popup.SetActive(false);
		popup_context popup_context = curr_popup_context;
		curr_popup_context = popup_context.none;
		switch (popup_context)
		{
		case popup_context.payment_options:
		{
			if (!HasEnoughGems())
			{
				FromPopupToStore(popup_context);
				ShowNotEnoughGemsPopup();
				break;
			}
			string purchaseableKey = GetPurchaseableKey(attempt_buy_name);
			Color color = GetColor(GetPurchaseableBgColor(attempt_buy_name));
			if (purchaseableKey == "doMUTATE" || purchaseableKey == "doCOMPANION")
			{
				FromPopupToStore(popup_context);
				BuyUsingGems();
			}
			else
			{
				ShowShopPopup(TranslationControl.Instance.TranslateGeneral("OKAY", "Market"), button_color_t.yes_green, "", button_color_t.none, true, TranslationControl.Instance.TranslateGeneral("NOTE: Game features unlocked using Gems cannot be restored if you get a new phone/tablet, or if your game somehow becomes corrupted", "Market"), color, button_BG_generic, new Color(1f, 1f, 1f, 1f), ring_sprite, new Color(1f, 1f, 1f, 0.6f), color, popup_context.unlock_with_gems_confirm);
			}
			break;
		}
		case popup_context.revive_get_gems_yes_no:
			GameController.Instance.revive_cancel();
			break;
		}
	}

	public void PopupOkayPressed()
	{
		AudioControl.Instance.PlayGenericClick();
		popup_open = false;
		popup.SetActive(false);
		popup_context popup_context = curr_popup_context;
		curr_popup_context = popup_context.none;
		switch (popup_context)
		{
		case popup_context.revive_purchase_succeed:
			GameController.Instance.ReviveAccepted();
			break;
		case popup_context.revive_purchase_fail:
			GameController.Instance.revive_cancel();
			break;
		case popup_context.store_purchase_succeed:
			if (blobble_gem_text_on_press_OKAY)
			{
				BlobbleGemText();
				blobble_gem_text_on_press_OKAY = false;
			}
			FromPopupToStore(popup_context);
			break;
		case popup_context.store_message:
		case popup_context.store_purchase_fail:
			FromPopupToStore(popup_context);
			break;
		case popup_context.unlock_with_gems_confirm:
			FromPopupToStore(popup_context);
			BuyUsingGems();
			break;
		}
	}

	private void BuyUsingGems()
	{
		string key = GetPurchaseableKey(attempt_buy_name);
		int cost = GetGemCost(GetPurchaseableGemPrice(attempt_buy_name));
		PlayerData.Instance.SetGlobalShort("GEMS", PlayerData.Instance.GetGlobalShort("GEMS") - cost);
		text_GEM_count.text = PlayerData.Instance.GetGlobalShort("GEMS").ToString() ?? "";
		StartCoroutine(DelayedBuyWithGems(key));
	}

	public void PressPopupClose()
	{
		AudioControl.Instance.PlayGenericClick();
		popup_open = false;
		popup.SetActive(false);
		popup_context popup_context = curr_popup_context;
		curr_popup_context = popup_context.none;
		if (popup_context == popup_context.unlock_with_gems_confirm || popup_context == popup_context.payment_options)
		{
			FromPopupToStore(popup_context);
		}
	}

	public void BlobbleGemText()
	{
		text_GEM_count.text = ((int)PlayerData.Instance.GetGlobalShort("GEMS")).ToString() ?? "";
		text_GEM_count.color = Color.green;
		text_GEM_count.transform.localScale = Vector3.one * 4.5f;
	}

	private IEnumerator DelayedBuyWithGems(string iap_key)
	{
		PopupControl.Instance.ShowConnecting("Unlocking using gems", PopupControl.context.loading_NO_TIMEOUT);
		on_pressed_buy = true;
		yield return new WaitForSeconds(0.3f);
		OnTransactionSucceed(iap_key);
	}

	private void ResizeCostText(Text cost_text, bool show_gem, GameObject gem_icon)
	{
		float icon_width = show_gem ? 50f : 0f;
		for (int size = 59; size > 10; size -= 2)
		{
			cost_text.fontSize = size;
			if (icon_width + cost_text.preferredWidth <= 250f)
			{
				break;
			}
		}
		if (show_gem)
		{
			cost_text.transform.localPosition = new Vector3(cost_text.preferredWidth * 0.5f - (icon_width + cost_text.preferredWidth) * 0.5f, cost_text.transform.localPosition.y, 0f);
			gem_icon.transform.localPosition = new Vector3((icon_width + cost_text.preferredWidth) * 0.5f - 20f, gem_icon.transform.localPosition.y, 0f);
			gem_icon.SetActive(true);
		}
		else
		{
			cost_text.transform.localPosition = new Vector3(0f, cost_text.transform.localPosition.y, 0f);
			gem_icon.SetActive(false);
		}
	}

	private void RedrawShopButtons()
	{
		for (int i = 0; i < 3; i++)
		{
			int index = i + shopPage * 3;
			if (index >= purchase_structs_ordering.Length)
			{
				if (i == 0)
				{
					shopPage--;
					RedrawShopButtons();
					return;
				}
				shop_buttons[i].SetActive(false);
				continue;
			}
			shop_buttons[i].SetActive(true);
			string name = purchase_structs_ordering[index];
			string key = GetPurchaseableKey(name);
			string alt_key = GetPurchaseableAltKey(name);
			string description = GetPurchaseableDescription(name);
			Color color = GetColor(GetPurchaseableBgColor(name));
			int gem_cost = GetGemCost(GetPurchaseableGemPrice(name));
			string cash_cost = GetCashCost(name);
			string type = GetPurchaseableType(name);
			if (InAppPurchaseControl.Instance.IsPurchased(key, alt_key))
			{
				button_rects[i].color = col_rect_ads_removed;
				button_backdrops[i].color = col_button_ads_removed;
				string status = type == "subscription" && PlayerData.Instance.GetGlobalShort("no_ads") != 1 ? "Subscribed" : "Owned";
				shop_costs[i].text = "<color=#00ff00>" + TranslationControl.Instance.TranslateGeneral(status, "Market") + "</color>";
				ResizeCostText(shop_costs[i], false, button_gem_icons[i]);
				shop_descriptions[i].text = TranslationControl.Instance.TranslateGeneral(description, "Market");
				shop_descriptions[i].color = col_description_ads_removed;
				shop_titles[i].text = TranslationControl.Instance.TranslateGeneral(name, "Market");
				shop_titles[i].color = col_title_ads_removed;
			}
			else
			{
				button_rects[i].color = col_rect_normal;
				button_backdrops[i].color = color;
				bool show_gem;
				if (gem_cost != 0 && cash_cost == "")
				{
					shop_costs[i].text = "<color=#2DFFF6>" + gem_cost + "</color>";
					show_gem = true;
				}
				else if (gem_cost == 0 && cash_cost != "")
				{
					shop_costs[i].text = cash_cost;
					show_gem = false;
				}
				else if (gem_cost != 0 && cash_cost != "")
				{
					shop_costs[i].text = cash_cost + "<color=#aaaaaa> " + TranslationControl.Instance.TranslateGeneral("or", "Market") + " </color><color=#2DFFF6>" + gem_cost + "</color>";
					show_gem = true;
				}
				else
				{
					shop_costs[i].text = "";
					show_gem = false;
				}
				if (gem_cost == 0 && cash_cost == "")
				{
					button_gem_icons[i].SetActive(false);
				}
				else
				{
					ResizeCostText(shop_costs[i], show_gem, button_gem_icons[i]);
				}
				shop_descriptions[i].text = TranslationControl.Instance.TranslateGeneral(description, "Market");
				shop_descriptions[i].color = Color.white;
				shop_titles[i].text = TranslationControl.Instance.TranslateGeneral(name, "Market");
				shop_titles[i].color = color;
			}
		}
		if (PlayerData.Instance.GetGlobalShort("GEMS") != 0)
		{
			text_GEM_count.text = PlayerData.Instance.GetGlobalShort("GEMS").ToString() ?? "";
		}
	}
}
