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
	}

	public void PrevPage()
	{
	}

	public void RedrawAll()
	{
	}

	public void RedrawPageButtons()
	{
	}

	public void DisableNearbyObjects(Vector3 origin, float range, bool keep_player_active)
	{
	}

	public void ReEnableNearbyObjects()
	{
	}

	public void CreateButtonModels()
	{
	}

	private GameObject CreateButtonModelHolder(int i)
	{
		return null;
	}

	private GameObject CreatePopupModelHolder()
	{
		return null;
	}

	private void OnShopModelLoaded(GameObject new_model, GameObject model_holder)
	{
	}

	public Color GetColor(string bg_color)
	{
		return default(Color);
	}

	public string GetPurchaseableKey(string iap_name)
	{
		return null;
	}

	public string GetPurchaseableModelPath(string iap_name)
	{
		return null;
	}

	public string GetPurchaseableAltKey(string iap_name)
	{
		return null;
	}

	public string GetPurchaseableGemPrice(string iap_name)
	{
		return null;
	}

	public string GetPurchaseableDescription(string iap_name)
	{
		return null;
	}

	public string GetPurchaseableBgColor(string iap_name)
	{
		return null;
	}

	public string GetPurchaseableType(string iap_name)
	{
		return null;
	}

	private void LoadIapStruct(string iap_name)
	{
	}

	public string GetCashCost(string iap_name)
	{
		return null;
	}

	public int GetGemCost(string gem_price)
	{
		return 0;
	}

	public void ClickPurchaseableItem(int buttonIndex)
	{
	}

	private void BuyWithCash(string iap_key)
	{
	}

	private bool HasEnoughGems()
	{
		return false;
	}

	private void ShowNotEnoughGemsPopup()
	{
	}

	public void DestroyShopModels()
	{
	}

	public void PressShopButton()
	{
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
	}

	public void ShopFailedLoading()
	{
	}

	public void CloseLoadingMarketScreen()
	{
	}

	public void ShopFinishedLoading()
	{
	}

	public void PressRetryLoadMarket()
	{
	}

	public void CloseGemsWindow(bool redraw_models)
	{
	}

	public void ShowMarketScreen()
	{
	}

	public void HideMarketScreen()
	{
	}

	public void BuyGemsFromRevive(int amount)
	{
	}

	public void BuyItem(string product_id)
	{
	}

	private void FixedUpdate()
	{
	}

	public void OnTransactionSucceed(string definition_id)
	{
	}

	public void OnTransactionFailed()
	{
	}

	public void PressRestorePurchases()
	{
	}

	public void TryOpenShop()
	{
	}

	private void FromPopupToStore(popup_context prev_context)
	{
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
	}

	private IEnumerator DelayedBuyWithGems(string iap_key)
	{
		return null;
	}

	private void ResizeCostText(Text cost_text, bool show_gem, GameObject gem_icon)
	{
	}

	private void RedrawShopButtons()
	{
	}
}
