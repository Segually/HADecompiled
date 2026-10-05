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

	public string attempt_buy_name;

	public List<GameObject> models_for_buttons;

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

	public List<GameObject> all_temporarily_disabled;

	public Transform[] button_model_positions;

	private Dictionary<string, Dictionary<string, string>> loaded_purchaseable_structs;

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

	private List<GameObject> instantiated_slider_bg_nums;

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
	}

	public void ShowShopPopup(string YES_str, button_color_t YES_col, string NO_str, button_color_t NO_col, bool show_close, string TEXT, Color textColor, Sprite header_bg_sprite, Color header_bg_col, Sprite header_overlay_sprite, Color header_overlay_col, Color outlineColor, popup_context context, int slider_max_buy = -1, InventoryItem req_1_item = null, int req_1_count = -1, InventoryItem req_2_item = null, int req_2_count = -1, req_context_t req_context = req_context_t.none)
	{
	}

	public void PressHowManySlider()
	{
	}

	public void ReleaseHowManySlider()
	{
	}

	private void Update()
	{
	}

	private void TryRedrawSlider(float p, bool force_redraw)
	{
	}

	private void GenericRedrawPopup(string YES_str, button_color_t YES_col, string NO_str, button_color_t NO_col, bool show_close, string TEXT, Color textColor, Color outlineColor, popup_context context, bool tall_window, InventoryItem req_1_item, int req_1_count, InventoryItem req_2_item, int req_2_count, req_context_t req_context)
	{
	}

	private void ColorizeShopPopupButton(GameObject button, Text text, button_color_t col)
	{
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
	}

	public void PopupNoPressed()
	{
	}

	public void PopupOkayPressed()
	{
	}

	private void BuyUsingGems()
	{
	}

	public void PressPopupClose()
	{
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
