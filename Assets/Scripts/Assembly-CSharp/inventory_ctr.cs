using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class inventory_ctr : MonoBehaviour, OrderedStart
{
	public enum sound_on_use
	{
		default_clink = 0,
		slurp = 1,
		crunch = 2
	}

	public enum sound_on_craft
	{
		default_clink = 0,
		bubbles = 1
	}

	public enum inv_type_t
	{
		none = 0,
		place_in_world = 1,
		armor = 2,
		helmet = 3,
		holdable = 4,
		tool = 5,
		consumable = 6
	}

	private struct craft_mixer_slot
	{
		public int index;

		public int deduct_amount;
	}

	public enum angular_sound_t
	{
		default_ = 0,
		sell = 1
	}

	public enum container_style_t
	{
		world_container = 0,
		companion_pockets = 1,
		trading_table = 2
	}

	public enum ptype
	{
		firstPage = 0,
		secondPage = 1,
		thirdPage = 2,
		eitherPage = 3
	}

	public enum fusion_button
	{
		hide = 0,
		start = 1,
		hatch = 2
	}

	public enum slots_positionings
	{
		show_15_centered = 0,
		show_15_left = 1,
		show_12 = 2,
		show_9 = 3,
		show_9_tradingTable = 4,
		show_3_fuser = 5,
		show_3_statue = 6,
		show_2 = 7,
		show_1_fuser = 8,
		show_1_wepDisplay = 9
	}

	[Serializable]
	public struct hover_button_col
	{
		public string name;

		public string text;

		public Color button_col;

		public Color button_trim_col;
	}

	public enum background_strip_layout
	{
		normal = 0,
		higher = 1
	}

	public static inventory_ctr Instance;

	public GameObject prefab_item_bg;

	public GameObject prefab_item_graphic;

	public GameObject prefab_item_overlay;

	public GameObject prefab_item_count;

	public GameObject prefab_item_locked;

	public GameObject prefab_item_requirement;

	public GameObject prefab_item_sprite;

	public GameObject prefab_model3d_graphic;

	public GameObject prefab_item_loading;

	public GameObject prefab_item_dmg_indicator;

	public GameObject prefab_item_shield_indicator;

	public static float crafting_bonus_multiplier;

	public Material mobile_diffuse;

	public Material mobile_diffuse_color;

	public Material mobile_diffuse_colorONLY;

	public Material default_particles;

	public bool angular_animation_playing;

	public ItemCountPair trying_to_craft;

	public static int p2_begin_;

	public static int n_slots_per_page_;

	private BasketContents curr_container;

	public Text text_loot_respawn_in;

	public static int hand_index;

	public static int hat_index;

	public static int body_index;

	private int trash_index;

	private int page_1_index;

	private int page_2_index;

	private int page_3_index;

	public GameObject equip_hand_slot;

	public GameObject equip_hat_slot;

	public GameObject equip_body_slot;

	public Sprite blank_hand_sprite;

	public Sprite blank_hat_sprite;

	public Sprite blank_body_sprite;

	public Sprite sprite_blank_bg;

	public Color col_background_normal;

	public Color col_background_fuser;

	public Sprite spr_fuser_eggSlot;

	public Sprite spr_fuser_fuelSlot;

	public Sprite spr_weapondisplay_empty;

	public Sprite spr_mystery_egg;

	public bool hover_clicked;

	public bool is_dragging;

	private Vector2 initial_press;

	private bool test_for_dragging;

	public BasketContents player_inventory;

	private Vector2 INV_start_slots;

	private float INV_slot_spacing;

	private Vector2 CRAFT_start_slots;

	private float CRAFT_spacing;

	private int drag_threshold;

	public Transform slot_parent;

	public Transform craft_slot_parent;

	public List<CraftingSlot> instantiated_crafting_slots;

	public List<GameObject> instantiated_inv_slots;

	public GameObject craft_slot_type;

	public GameObject inv_slot_type;

	public GameObject crafting_page_left;

	public GameObject crafting_page_right;

	public GameObject type_particle_mixer;

	private List<craft_mixer_slot> crafting_mixer_slots;

	public Color default_angular_col;

	public Color getitem_angular_col;

	public container_style_t container_style;

	public int buy_index;

	public Mesh CUSTOM_ITEM_MODEL;

	private GameObject mouse_down_slot;

	public GameObject invPAGE1;

	public GameObject invPAGE2;

	public GameObject invPAGE3;

	private int refresh_loot_counter_i;

	public ItemSprite drag_item_sprite;

	public Transform for_slots;

	private Vector2 standard_slots_position;

	public GameObject gui_loot_respawn_time;

	public new_craft_list curr_crafting_list;

	public List<string> curr_buyback_list;

	private Dictionary<string, new_craft_list> loaded_craft_lists;

	public GameObject advanced_button;

	public GameObject egg_fuser_text;

	public GameObject start_fuser_button;

	public Text txt_fuser_startbutton;

	public int page_inventory;

	private int page_container;

	public GameObject pageSwitchers;

	public Text text_page1;

	public Text text_page2;

	public Image invp1;

	public Image invp2;

	public Image invp3;

	public Color col_fuser_startbutton;

	public Color col_fuser_hatchbutton;

	public Sprite spr_fuser_startbutton;

	public Sprite spr_fuser_hatchbutton;

	public Color premium_inv_color;

	public Color premium_cannot_craft;

	public Color col_inv_unclickable;

	public Color fusion_inprogress_slot_color;

	public GameObject land_claim_particle_prefab;

	public GameObject land_claim_particle_prefab_2;

	public Sprite record_center_sprite;

	public RectTransform crafting_background_strip;

	public int craft_PAGE;

	public GameObject hover_click;

	public GameObject hover_nib;

	public GameObject selector;

	public Animation hover_anm;

	public RectTransform hover_panel;

	public RectTransform hover_item_parent;

	public Text hover_text_title;

	public Text hover_text_desc;

	public GameObject hover_button;

	public GameObject hover_button_2;

	public Text hover_button_text;

	public Text hover_button_2_text;

	public Image hover_button_inner;

	public Image hover_button_2_inner;

	public hover_button_col[] hover_button_cols;

	private List<string> hover_options;

	private InventoryItem split_obj;

	private int og_split;

	private int split_1;

	private int split_2;

	private int split_inv_index;

	public GameObject prev_clicked_inv;

	private int prev_clicked_inv_i;

	public GameObject trash_bin;

	public ItemSprite hover_sprite;

	public Color fossil_col;

	public bool PLACING_OBJECT_OR_USING_TOOL;

	public InventoryItem ITEM_USING;

	public GameObject angular;

	private int sell_index;

	public int enter_dialogue_on_close;

	public GameObject inventory_Tab;

	public GameObject crafting_Tab;

	private IEnumerator CASCADE_CRAFT_BUTS;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public void Reorder(List<GameObject> objects, Transform T)
	{
	}

	private void ReorderRecursive(List<GameObject> nulls_removed, int d, Transform T)
	{
	}

	public void PressBackOnSelectScreen()
	{
	}

	public bool IsItemPaintable(string item_name)
	{
		return false;
	}

	public inv_type_t GetItemType(InventoryItem item)
	{
		return default(inv_type_t);
	}

	public string GetItemToolRequiredToMove(string item_name)
	{
		return null;
	}

	public bool GetItemBool(string item_name, string key)
	{
		return ResourceControl.Instance.GetStringFromItemFile(item_name, key) == "true";
	}

	public string GetItemWorldObjPath(string item_name)
	{
		if (DevBuildControl.Instance.view_dev_objects)
		{
			string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item_name, "DEBUG_World_obj_path");
			if (stringFromItemFile != "")
			{
				return stringFromItemFile;
			}
		}
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "World_obj_path");
	}

	public string GetItemSpritePath(string item_name)
	{
		return null;
	}

	public sound_on_craft GetItemSoundOnCraft(string item_name)
	{
		return default(sound_on_craft);
	}

	public sound_on_use GetItemSoundOnUse(string item_name)
	{
		return default(sound_on_use);
	}

	public int GetItemCraftingLevelRequired(string item_name)
	{
		return 0;
	}

	public string GetItemCraftingIngredientA(string item_name)
	{
		return null;
	}

	public int GetItem_nCraft(string item_name)
	{
		return 0;
	}

	public int GetItemCraftingIngredientA_count(string item_name)
	{
		return 0;
	}

	public string GetCraftingAltIngredientA(string item_name)
	{
		return null;
	}

	public int GetItemCraftingIngredientB_count(string item_name)
	{
		return 0;
	}

	public string GetItemCraftingIngredientB(string item_name)
	{
		return null;
	}

	public int GetItemCraftingAltIngredientA_count(string item_name)
	{
		return 0;
	}

	public string GetItemIapKeyRequired(InventoryItem item)
	{
		return null;
	}

	public string GetItemEquipReqStat(string item_name)
	{
		return null;
	}

	public int GetItemEquipReqLvl(string item_name)
	{
		return 0;
	}

	public int GetItemMaxStack(string item_name)
	{
		return 0;
	}

	public string GetItemDescription(string item_name)
	{
		return null;
	}

	public string GetItemOverwriteName(string item_name)
	{
		return null;
	}

	public string GetFullItemName(InventoryItem item)
	{
		return null;
	}

	public GameObject GenerateCustomModel(InventoryItem item)
	{
		return null;
	}

	public void NestedApplyMaterial(GameObject G, Material mat)
	{
	}

	public bool is_locked_by_player(InventoryItem item)
	{
		return false;
	}

	public int BonsaiBonus(int bonsai_age)
	{
		return 0;
	}

	public void NewRedrawLootRespawnText(string respawn_prefix, InventoryItem item)
	{
	}

	public void RedrawEggFuserText(InventoryItem item)
	{
	}

	public void RedrawMinigameRespawn()
	{
	}

	public void ClearInventory(bool save)
	{
	}

	public void crafting_inc_page(int dir)
	{
	}

	public void LoadInventory()
	{
	}

	public void BeginCraftAnimation()
	{
	}

	public void AcceptBuy()
	{
	}

	public void ShowDelayedBuySuccessNotif(ItemCountPair received)
	{
	}

	private IEnumerator DelayedBuySuccessNotif(ItemCountPair received)
	{
		return null;
	}

	public void DeductCoins(int amount)
	{
	}

	private IEnumerator mix_items()
	{
		return null;
	}

	private BasketContents ClonePlayerInventory()
	{
		return null;
	}

	private int TestCanCraftItem(string item_name, int count, string reqA_item, int reqA_n_remaining, string reqB_item = "", int reqB_n_remaining = 0)
	{
		return 0;
	}

	public void show_angular(Vector3 localPos, Color angular_col, angular_sound_t angular_sound = angular_sound_t.default_)
	{
	}

	private bool ShouldContainerGenerateLoot(InventoryItem container_item)
	{
		return false;
	}

	public void TryOpenWorldContainer(InventoryItem container_item, int container_rot, int innerX, int innerZ, int chunkX, int chunkZ)
	{
	}

	public void SucceedOpenWorldContainer(int container_id, BasketContents contents)
	{
	}

	public void SucceedOpenCompanionPockets(ActiveCompanion companion)
	{
	}

	public void SucceedOpenTradingTable()
	{
	}

	public void AddChestRespawnAndRedraw()
	{
	}

	public int GetPlayerItemCount(string item_name)
	{
		return 0;
	}

	public bool iap_allowed(InventoryItem item)
	{
		return false;
	}

	public void iap_deny_popup(InventoryItem item, string verb)
	{
	}

	private int GetMaxBuy(InventoryItem item, int og_max)
	{
		return 0;
	}

	public void PressBuyItem(int button_index)
	{
	}

	public void PressCraftItem(int button_index)
	{
	}

	public InventoryItem AdjustMouseItemData(InventoryItem item)
	{
		return null;
	}

	public void DragOntoSlot(int drag_onto_index, GameObject drag_onto_obj)
	{
	}

	public void FillOutBookData(string book_name, ref ExtraInventoryData extra_data)
	{
	}

	private void LayOutEggFuser()
	{
	}

	public void UpdateManaMods(string hat_item)
	{
	}

	public void EquipIfPossible(int slot_index)
	{
	}

	public void UnequipIfPossible(int inv_index)
	{
	}

	public void click_DONE_placing()
	{
	}

	public string GetPaintFromItemOrUseDefault(InventoryItem item)
	{
		string text = item.GetString("paint");
		if (Startup.StringNullOrWhitespace(text))
		{
			string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item.item_name, "Default Coloring");
			text = "";
			if (!Startup.StringNullOrWhitespace(stringFromItemFile))
			{
				text = stringFromItemFile;
			}
		}
		return text;
	}

	public string GetLayoutItemFromItem(string item_name)
	{
		string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item_name, "CopyPaintLayout");
		if (!Startup.StringNullOrWhitespace(stringFromItemFile))
		{
			return stringFromItemFile;
		}
		return item_name;
	}

	public string GetStampFromItem(InventoryItem item)
	{
		string text = item.GetString("stamp");
		if (Startup.StringNullOrWhitespace(text))
		{
			string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item.item_name, "Default Stamp");
			text = "";
			if (!Startup.StringNullOrWhitespace(stringFromItemFile))
			{
				text = stringFromItemFile;
			}
		}
		return text;
	}

	public bool check_req_skill_to_equip(string skill_key, int min_skill_lvl)
	{
		return false;
	}

	public void DropExtraItemDataOnPickup(InventoryItem item)
	{
	}

	public static string BiomeIdToCaveEntrance(int biome_id)
	{
		return null;
	}

	public InventoryItem FinalizeItemBeforePutDown(InventoryItem item, ChunkData chunk_data, int innerX, int innerZ)
	{
		return null;
	}

	public bool HasItem(InventoryItem item)
	{
		return false;
	}

	public void GrabNext(InventoryItem item)
	{
	}

	private void Update()
	{
	}

	private bool TestEquipAllowed(int the_slot, inv_type_t type, int nearest_ind, int drag_from, InventoryItem itemA, InventoryItem itemB, int companion_level)
	{
		return false;
	}

	public void CancelDragging()
	{
	}

	private void UpdateLootChestVisual()
	{
	}

	private void FixedUpdate()
	{
	}

	private void position_drag_at_mouse()
	{
	}

	public List<int> GetEmptyNonEquipmentInventorySlots(BasketContents contents)
	{
		return null;
	}

	public int HowManyCanAfford(InventoryItem item)
	{
		return 0;
	}

	public int HowManyCanReceive(string item_name)
	{
		return 0;
	}

	public bool CanReceiveItem(string item_name, int n_give, bool show_full_notif = false, BasketContents hypothetical_inventory = null, bool add_to_crafting_mixers = false)
	{
		return false;
	}

	public bool CanReceiveItem(InventoryItem item, int n_give, bool show_full_notif = false, BasketContents hypothetical_inventory = null, bool add_to_crafting_mixers = false)
	{
		return false;
	}

	public void GiveItem(string item_name, int count, string fn_validator, bool visual = true)
	{
	}

	public void GiveItem(InventoryItem item, int count, string fn_validator, bool visual = true)
	{
	}

	private void create_buttons_and_slots()
	{
	}

	public void OpenInventoryAndCrafting(new_craft_list list, InventoryItem item)
	{
	}

	public void OpenInventoryAndCrafting(WindowControl.tab initial_tab, new_craft_list list, string non_inv_tab_name, string inv_tab_name = "INVENTORY")
	{
	}

	public void OpenInventoryAndContainer(string non_inv_tab_name)
	{
	}

	public void OpenInventoryAndMerchant(WindowControl.tab initial_tab, new_craft_list list)
	{
	}

	public void OnLeftMiniwindowTabClicked()
	{
	}

	public void OnRightMiniwindowTabClicked()
	{
	}

	public void EquipmentSlotsSetActive(bool val)
	{
	}

	public void open_buy()
	{
	}

	public void open_sell()
	{
	}

	private void open_merchant_window(WindowControl.tab tab)
	{
	}

	public void press_inv_button()
	{
	}

	public new_craft_list GetCraftList(string craft_list_file_name)
	{
		return null;
	}

	public List<int> GetPermittedContainerSlots(container_style_t style, string item_name, ptype page)
	{
		return null;
	}

	private int ToContainer(InventoryItem item, int curr_count, ptype page_type)
	{
		return 0;
	}

	private int ToInventory(InventoryItem item, int curr_count, ptype page_type)
	{
		return 0;
	}

	private int AddToPreExistingInventoryStacks(InventoryItem item, int curr_count, int startIndex, int endIndex)
	{
		return 0;
	}

	private int AddToEmptyInventoryStacks(InventoryItem item, int curr_count, int startIndex, int endIndex)
	{
		return 0;
	}

	public void inv_page_switch(int new_page_id)
	{
	}

	private void RedrawPageSwitchers(int new_page_id)
	{
	}

	public void RedrawInventorySlots()
	{
	}

	public void RedrawContainerSlots()
	{
	}

	public Texture2D LoadPaintingFrame(InventoryItem item, string counter, string prefix)
	{
		int @long = item.GetLong(counter);
		byte[] array = new byte[@long];
		int num = 0;
		byte[] array2 = null;
		int num2 = 0;
		for (int i = 0; i < @long; i++)
		{
			if (array2 == null)
			{
				array2 = BitConverter.GetBytes(item.GetLong(prefix + num));
			}
			array[i] = array2[num2];
			num2++;
			if (num2 == 4)
			{
				num2 = 0;
				array2 = null;
				num++;
			}
		}
		Texture2D texture2D = new Texture2D(PaintingControl.n_horizontal_segs * 32, PaintingControl.n_vertical_segs * 32);
		texture2D.filterMode = FilterMode.Point;
		texture2D.LoadImage(array);
		return texture2D;
	}

	public int NumPlayerPages()
	{
		return 0;
	}

	public void goto_findBlankRecord()
	{
	}

	public void PressStartEggFusion()
	{
	}

	public void jump_to_page(int page)
	{
	}

	public void ShowRespawnTimer(string prefix)
	{
	}

	public void HideRespawnTimer()
	{
	}

	public void goto_crafting_tab(int start_page)
	{
	}

	public void LayOutInvSlots(bool show_respawn_timer, bool show_trash_bin, slots_positionings slot_positioning, bool show_equipment_slots, int n_pages, bool show_advanced_button, string fuser_text, fusion_button fusion_button_layout, string page1_text = "PAGE 1", string page2_text = "PAGE 2")
	{
	}

	public void RedrawCraftingSlotButtons()
	{
	}

	private void DrawCraftingSlotPart1(int slot_id, int index)
	{
	}

	private void DrawCraftingSlotPart2(int slot_id, int index)
	{
	}

	public void hide_selector()
	{
	}

	public void click_hover_option_A()
	{
	}

	public void click_hover_option_B()
	{
	}

	public void click_hover_nullspace()
	{
	}

	private void do_option(string action)
	{
	}

	private void RedrawSplitGraphic()
	{
	}

	public void edited_split_1()
	{
	}

	public void edited_split_2()
	{
	}

	public void press_split_accept()
	{
	}

	public void press_split_cancel()
	{
	}

	private void attempt_store_item(int index)
	{
	}

	private void attempt_take_item(int index, bool do_show_angular)
	{
	}

	public void OnClose()
	{
	}

	private void create_hover(int index)
	{
	}

	private int GetTrueInventoryIndex(int inventory_slot_id)
	{
		return 0;
	}

	private int GetTrueContainerIndex(int container_slot_id)
	{
		return 0;
	}

	private int GetTrueIndex(int page, int slot_id)
	{
		return 0;
	}

	public void slot_mouse_down(int index, GameObject mouse_down_slot)
	{
	}

	public void CompleteSell()
	{
	}

	private int GetTotalNumberOfItemInInventory(InventoryItem item)
	{
		return 0;
	}

	private IEnumerator ON_DOUBLE_CLICK(int inventory_slot_id, string action, GameObject mouse_down_slot)
	{
		return null;
	}

	public void LayOutCraftingTab(background_strip_layout background_strip_layout_)
	{
	}

	public void HideCraftingTab()
	{
	}

	public void ShowInventoryTab(bool also_enable_top_buttons)
	{
	}

	public void HideInventoryTab(bool also_hide_top_buttons)
	{
	}

	public bool page_exists(int mod)
	{
		return false;
	}

	private void do_cascade_craft_buttons(int dir)
	{
	}

	private IEnumerator cascade_craft_buts(int dir)
	{
		return null;
	}
}
