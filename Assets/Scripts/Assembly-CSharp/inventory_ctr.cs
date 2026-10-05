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

	public static float crafting_bonus_multiplier = 0.13f;

	public Material mobile_diffuse;

	public Material mobile_diffuse_color;

	public Material mobile_diffuse_colorONLY;

	public Material default_particles;

	public bool angular_animation_playing;

	public ItemCountPair trying_to_craft;

	public static int p2_begin_ = 20;

	public static int n_slots_per_page_ = 15;

	private BasketContents curr_container;

	public Text text_loot_respawn_in;

	public static int hand_index = 15;

	public static int hat_index = 16;

	public static int body_index = 17;

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
		standard_slots_position = slot_parent.transform.localPosition;
		ClearInventory(false);
		create_buttons_and_slots();
		RedrawPageSwitchers(1);
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
		if (item.GetString("custom_type") != "")
		{
			string text = item.GetString("custom_type");
			if (text == "hat")
			{
				return inv_type_t.helmet;
			}
			if (text == "furniture")
			{
				return inv_type_t.place_in_world;
			}
			return inv_type_t.none;
		}
		string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item.item_name, "Type");
		if (stringFromItemFile == "Armor")
		{
			return inv_type_t.armor;
		}
		if (stringFromItemFile == "Consumable")
		{
			return inv_type_t.consumable;
		}
		if (stringFromItemFile == "Helmet")
		{
			return inv_type_t.helmet;
		}
		if (stringFromItemFile == "Holdable")
		{
			return inv_type_t.holdable;
		}
		if (stringFromItemFile == "Place_in_world")
		{
			return inv_type_t.place_in_world;
		}
		if (stringFromItemFile == "Tool")
		{
			return inv_type_t.tool;
		}
		return inv_type_t.none;
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
		int intFromItemFile = ResourceControl.Instance.GetIntFromItemFile(item_name, "Max_stack");
		if (intFromItemFile == 0)
		{
			return 1;
		}
		return intFromItemFile;
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
		GameObject gameObject = new GameObject("CUSTOM MODEL");
		gameObject.AddComponent<MeshFilter>();
		gameObject.AddComponent<MeshRenderer>();
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<Vector3> list3 = new List<Vector3>();
		List<Vector2> list4 = new List<Vector2>();
		int @long = item.GetLong("n_mesh_verts");
		for (int i = 0; i < @long; i++)
		{
			list.Add(new Vector3((float)item.GetLong("mesh_vX" + i) / 100f, (float)item.GetLong("mesh_vY" + i) / 100f, (float)item.GetLong("mesh_vZ" + i) / 100f));
		}
		int long2 = item.GetLong("n_mesh_tris");
		for (int j = 0; j < long2; j++)
		{
			list2.Add(item.GetLong("mesh_tri" + j));
		}
		int long3 = item.GetLong("n_mesh_normals");
		for (int k = 0; k < long3; k++)
		{
			list3.Add(new Vector3((float)item.GetLong("mesh_nX" + k) / 100f, (float)item.GetLong("mesh_nY" + k) / 100f, (float)item.GetLong("mesh_nZ" + k) / 100f));
		}
		int long4 = item.GetLong("n_mesh_uvs");
		for (int l = 0; l < long4; l++)
		{
			list4.Add(new Vector2((float)item.GetLong("mesh_uX" + l) / 100f, (float)item.GetLong("mesh_uY" + l) / 100f));
		}
		Mesh mesh = new Mesh();
		mesh.vertices = list.ToArray();
		mesh.triangles = list2.ToArray();
		mesh.normals = list3.ToArray();
		mesh.uv = list4.ToArray();
		gameObject.GetComponent<MeshFilter>().mesh = mesh;
		gameObject.GetComponent<MeshRenderer>().material = new Material(mobile_diffuse);
		int long5 = item.GetLong("n_tex_bts");
		byte[] array = new byte[long5];
		byte[] array2 = null;
		int num = 0;
		int num2 = 0;
		for (int m = 0; m < long5; m++)
		{
			if (array2 == null)
			{
				array2 = BitConverter.GetBytes(item.GetLong("Tb" + num2));
			}
			array[m] = array2[num];
			num++;
			if (num == 4)
			{
				num = 0;
				array2 = null;
				num2++;
			}
		}
		Texture2D texture2D = new Texture2D(64, 64);
		texture2D.LoadImage(array);
		gameObject.GetComponent<MeshRenderer>().material.mainTexture = texture2D;
		if (item.GetShort("has_particles") != 1)
		{
			return gameObject;
		}
		GameObject gameObject2 = new GameObject("CUSTOM PARTICLES");
		gameObject2.transform.SetParent(gameObject.transform);
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localScale = Vector3.one;
		gameObject2.transform.localRotation = Quaternion.identity;
		ParticleSystem particleSystem = gameObject2.AddComponent<ParticleSystem>();
		ParticleSystem.MainModule main = particleSystem.main;
		main.startSpeed = (float)item.GetShort("particle_speed") / 10f;
		main.startLifetime = (float)item.GetShort("particle_lifetime") / 10f;
		main.startSize = (float)item.GetShort("particle_size") / 10f;
		if (item.GetString("particle_space") == "world")
		{
			main.simulationSpace = ParticleSystemSimulationSpace.World;
		}
		Color color = new Color((float)item.GetShort("particle_color1_r") / 255f, (float)item.GetShort("particle_color1_g") / 255f, (float)item.GetShort("particle_color1_b") / 255f);
		Color color2 = new Color((float)item.GetShort("particle_color2_r") / 255f, (float)item.GetShort("particle_color2_g") / 255f, (float)item.GetShort("particle_color2_b") / 255f);
		if (color != Color.black || color2 != Color.black)
		{
			if (color2 != Color.black)
			{
				main.startColor = new ParticleSystem.MinMaxGradient(color, color2);
			}
			else
			{
				main.startColor = color;
			}
		}
		ParticleSystem.ShapeModule shape = particleSystem.shape;
		if (item.GetString("particle_shape_type") == "sphere")
		{
			shape.shapeType = ParticleSystemShapeType.Sphere;
		}
		shape.radius = (float)item.GetShort("particle_shape_radius") / 10f;
		particleSystem.GetComponent<ParticleSystemRenderer>().material = default_particles;
		return gameObject;
	}

	public void NestedApplyMaterial(GameObject G, Material mat)
	{
		MeshRenderer component = G.GetComponent<MeshRenderer>();
		if (component != null)
		{
			Material[] materials = component.materials;
			for (int i = 0; i < materials.Length; i++)
			{
				materials[i] = mat;
			}
			component.materials = materials;
		}
		for (int j = 0; j < G.transform.childCount; j++)
		{
			NestedApplyMaterial(G.transform.GetChild(j).gameObject, mat);
		}
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
		player_inventory = new BasketContents();
		if (save)
		{
			player_inventory.SaveAllAsInventory();
		}
	}

	public void crafting_inc_page(int dir)
	{
	}

	public void LoadInventory()
	{
		player_inventory = new BasketContents();
		player_inventory.LoadFromDiskAsInventory();
		GameController.Instance.player.GetComponent<SharedCreature>().hand_ = player_inventory[hand_index].item;
		GameController.Instance.player.GetComponent<SharedCreature>().hat_ = player_inventory[hat_index].item;
		GameController.Instance.player.GetComponent<SharedCreature>().body_ = player_inventory[body_index].item;
		GameController.Instance.player.GetComponent<SharedCreature>().OnEquipmentChanged();
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
