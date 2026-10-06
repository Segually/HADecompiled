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

	public ItemCountPair trying_to_craft = new ItemCountPair("", 0);

	public static int p2_begin_ = 20;

	public static int n_slots_per_page_ = 15;

	private BasketContents curr_container = new BasketContents();

	public Text text_loot_respawn_in;

	public static int hand_index = 15;

	public static int hat_index = 16;

	public static int body_index = 17;

	private int trash_index = -2;

	private int page_1_index = -10;

	private int page_2_index = -11;

	private int page_3_index = -12;

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

	public BasketContents player_inventory = new BasketContents();

	private Vector2 INV_start_slots = new Vector2(-377f, 191f);

	private float INV_slot_spacing = 190f;

	private Vector2 CRAFT_start_slots = new Vector2(-307f, -55f);

	private float CRAFT_spacing = 290f;

	private int drag_threshold = 15;

	public Transform slot_parent;

	public Transform craft_slot_parent;

	public List<CraftingSlot> instantiated_crafting_slots = new List<CraftingSlot>();

	public List<GameObject> instantiated_inv_slots = new List<GameObject>();

	public GameObject craft_slot_type;

	public GameObject inv_slot_type;

	public GameObject crafting_page_left;

	public GameObject crafting_page_right;

	public GameObject type_particle_mixer;

	private List<craft_mixer_slot> crafting_mixer_slots = new List<craft_mixer_slot>();

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

	public List<string> curr_buyback_list = new List<string>();

	private Dictionary<string, new_craft_list> loaded_craft_lists = new Dictionary<string, new_craft_list>();

	public GameObject advanced_button;

	public GameObject egg_fuser_text;

	public GameObject start_fuser_button;

	public Text txt_fuser_startbutton;

	public int page_inventory = 1;

	private int page_container = 1;

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

	private List<string> hover_options = new List<string>();

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

	public InventoryItem ITEM_USING = new InventoryItem("");

	public GameObject angular;

	private int sell_index;

	public int enter_dialogue_on_close = -1;

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
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < objects.Count; i++)
		{
			if (objects[i] != null)
			{
				list.Add(objects[i]);
			}
		}
		if (list.Count != 0)
		{
			ReorderRecursive(list, 0, T);
		}
	}

	private void ReorderRecursive(List<GameObject> nulls_removed, int d, Transform T)
	{
		if (d >= 50)
		{
			return;
		}
		int childCount = T.childCount;
		int num = 0;
		for (int i = 0; i < childCount; i++)
		{
			GameObject gameObject = T.GetChild(i).gameObject;
			if (nulls_removed.Contains(gameObject))
			{
				if (nulls_removed.IndexOf(gameObject) != num)
				{
					gameObject.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex() - 1);
					ReorderRecursive(nulls_removed, d + 1, T);
					break;
				}
				num++;
			}
		}
	}

	public void PressBackOnSelectScreen()
	{
		if (MusicBoxControl.Instance.save_screen_open || MusicBoxControl.Instance.load_screen_open)
		{
			MusicBoxControl.Instance.press_back_on_select_record();
		}
		else if (PaintingControl.Instance != null)
		{
			if (PaintingControl.Instance.load_screen_open || PaintingControl.Instance.save_screen_open)
			{
				PaintingControl.Instance.PressBackOnSaveLoad();
			}
		}
		else if (VendingMachineControl.Instance != null)
		{
			VendingMachineControl.Instance.PressBackOnSelectItem();
		}
	}

	public bool IsItemPaintable(string item_name)
	{
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "paintable_by_player") == "true";
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
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "tool_required_to_move");
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
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "Inventory_sprite_path");
	}

	public sound_on_craft GetItemSoundOnCraft(string item_name)
	{
		if (ResourceControl.Instance.GetStringFromItemFile(item_name, "Sound on Craft") == "Bubbles")
		{
			return sound_on_craft.bubbles;
		}
		return sound_on_craft.default_clink;
	}

	public sound_on_use GetItemSoundOnUse(string item_name)
	{
		string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item_name, "Sound on Use");
		if (stringFromItemFile == "Crunch")
		{
			return sound_on_use.crunch;
		}
		if (stringFromItemFile == "Slurp")
		{
			return sound_on_use.slurp;
		}
		return sound_on_use.default_clink;
	}

	public int GetItemCraftingLevelRequired(string item_name)
	{
		return ResourceControl.Instance.GetIntFromItemFile(item_name, "Craft_required_lvl");
	}

	public string GetItemCraftingIngredientA(string item_name)
	{
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "Crafting_ingredient_A");
	}

	public int GetItem_nCraft(string item_name)
	{
		int intFromItemFile = ResourceControl.Instance.GetIntFromItemFile(item_name, "Crafting_makes");
		if (intFromItemFile == 0)
		{
			return 1;
		}
		return intFromItemFile;
	}

	public int GetItemCraftingIngredientA_count(string item_name)
	{
		return ResourceControl.Instance.GetIntFromItemFile(item_name, "Crafting_ingredient_A_count");
	}

	public string GetCraftingAltIngredientA(string item_name)
	{
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "Alt_crafting_ingredient_A");
	}

	public int GetItemCraftingIngredientB_count(string item_name)
	{
		return ResourceControl.Instance.GetIntFromItemFile(item_name, "Crafting_ingredient_B_count");
	}

	public string GetItemCraftingIngredientB(string item_name)
	{
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "Crafting_ingredient_B");
	}

	public int GetItemCraftingAltIngredientA_count(string item_name)
	{
		return ResourceControl.Instance.GetIntFromItemFile(item_name, "Alt_crafting_ingredient_A_count");
	}

	public string GetItemIapKeyRequired(InventoryItem item)
	{
		return ResourceControl.Instance.GetStringFromItemFile(item.item_name, "IAP_required");
	}

	public string GetItemEquipReqStat(string item_name)
	{
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "Equip_required_stat");
	}

	public int GetItemEquipReqLvl(string item_name)
	{
		return ResourceControl.Instance.GetIntFromItemFile(item_name, "Equip_required_stat_lvl");
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
		if (TranslationControl.Instance.use_language != TranslationControl.languages.English)
		{
			string text = "";
			switch (TranslationControl.Instance.use_language)
			{
			case TranslationControl.languages.Russian:
				text = ResourceControl.Instance.GetStringFromItemFile(item_name, "Description_RUS");
				break;
			case TranslationControl.languages.Portuguese:
				text = ResourceControl.Instance.GetStringFromItemFile(item_name, "Description_POR");
				break;
			case TranslationControl.languages.Indonesian:
				text = ResourceControl.Instance.GetStringFromItemFile(item_name, "Description_IND");
				break;
			case TranslationControl.languages.Spanish:
				text = ResourceControl.Instance.GetStringFromItemFile(item_name, "Description_SPN");
				break;
			case TranslationControl.languages.Thai:
				text = ResourceControl.Instance.GetStringFromItemFile(item_name, "Description_TAI");
				break;
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
		}
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "Description");
	}

	public string GetItemOverwriteName(string item_name)
	{
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "Overwrite_name");
	}

	public string GetFullItemName(InventoryItem item)
	{
		if (item.item_name == "Egg" || item.item_name == "Fossil")
		{
			string text = "";
			if (item.item_name == "Egg")
			{
				text = item.GetString("egg_monster");
			}
			else if (item.item_name == "Fossil")
			{
				text = item.GetString("fossil_monster");
			}
			if (text.Length < 1)
			{
				return item.item_name;
			}
			return text[0].ToString().ToUpper() + text.Substring(1, text.Length - 1) + " " + item.item_name;
		}
		if (item.item_name == "Book")
		{
			string text2 = TranslationControl.Instance.TranslateItemName("Book");
			string text3 = item.GetString("dev_book_title");
			if (!string.IsNullOrWhiteSpace(text3))
			{
				if (TranslationControl.Instance.use_language != TranslationControl.languages.English)
				{
					Dictionary<string, string> dictionary = BookControl.LoadDevBook(text3);
					if (dictionary.ContainsKey("book name - " + BookControl.GetBookLanguage()))
					{
						text3 = dictionary["book name - " + BookControl.GetBookLanguage()];
					}
				}
				return text2 + " <i>(" + text3 + ")</i>";
			}
			string text4 = item.GetString("book_title");
			if (string.IsNullOrWhiteSpace(text4))
			{
				return text2 + " <i>(Empty)</i>";
			}
			return text2 + " <i>(" + text4 + ")</i>";
		}
		if (item.item_name == "Key")
		{
			string text5 = item.GetString("key_name");
			if (text5.Contains("BanditCamp"))
			{
				BanditCampInstance banditCampInstanceByName = BanditCampsControl.Instance.GetBanditCampInstanceByName(text5);
				if (banditCampInstanceByName != null)
				{
					return banditCampInstanceByName.GetRandomizedBossEntry("BOSS_NAME") + "'s Key";
				}
				return "Boss Key";
			}
			if (text5 == "KW_basement")
			{
				return "Key to King Wing's Basement";
			}
			if (text5 == "VA_chest")
			{
				return "Key to Vitamin Ape's Chest";
			}
			if (text5 == "Nelly")
			{
				return "Key to Nelly's Chest";
			}
			return item.item_name;
		}
		if (item.item_name == "Trophy")
		{
			if (Startup.StringNullOrWhitespace(item.GetString("trophy_name")))
			{
				return "Trophy";
			}
			return item.GetString("trophy_name");
		}
		if (TranslationControl.Instance.use_language != TranslationControl.languages.English)
		{
			return TranslationControl.Instance.TranslateItemName(item.item_name);
		}
		if (GetItemOverwriteName(item.item_name) == "")
		{
			return item.item_name;
		}
		return GetItemOverwriteName(item.item_name);
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
		if (!DevBuildControl.Instance.all_doors_unlocked)
		{
			string text = item.GetString("password");
			if (text != "" && text != "DEV_LOCKED")
			{
				return true;
			}
		}
		return false;
	}

	public int BonsaiBonus(int bonsai_age)
	{
		return (int)((float)bonsai_age * 0.5f);
	}

	public void NewRedrawLootRespawnText(string respawn_prefix, InventoryItem item)
	{
		if (!item.HasActiveOrExpiredRespawn(respawn_prefix))
		{
			text_loot_respawn_in.text = "???";
			return;
		}
		DateTime respawnDateTime = item.GetRespawnDateTime(respawn_prefix);
		DateTime utcNow = DateTime.UtcNow;
		int num = (int)(respawnDateTime - utcNow).TotalHours;
		int num2 = (int)(respawnDateTime - utcNow).TotalMinutes - num * 60;
		int num3 = (int)(respawnDateTime - utcNow).TotalSeconds - num * 3600 - num2 * 60;
		if (num > 0)
		{
			text_loot_respawn_in.text = num + "h " + num2 + "m " + num3 + "s";
		}
		else if (num2 > 0)
		{
			text_loot_respawn_in.text = num2 + "m " + num3 + "s";
		}
		else if (num3 > 0)
		{
			text_loot_respawn_in.text = num3 + "s";
		}
		else
		{
			text_loot_respawn_in.text = "0s";
		}
	}

	public void RedrawEggFuserText(InventoryItem item)
	{
		if (!item.HasActiveOrExpiredRespawn("fuser_spawn"))
		{
			egg_fuser_text.GetComponent<Text>().text = "<size=40><color=#00ff00>Mixing eggs...</color>\n<color=#7B7B7B>Done in </color><color=#ffffff>???</color></size>";
			return;
		}
		DateTime respawnDateTime = item.GetRespawnDateTime("fuser_spawn");
		DateTime utcNow = DateTime.UtcNow;
		int num = (int)(respawnDateTime - utcNow).TotalHours;
		int num2 = (int)(respawnDateTime - utcNow).TotalMinutes - num * 60;
		int num3 = (int)(respawnDateTime - utcNow).TotalSeconds - num * 3600 - num2 * 60;
		if (num2 > 0)
		{
			egg_fuser_text.GetComponent<Text>().text = "<size=40><color=#00ff00>Mixing eggs...</color>\n<color=#7B7B7B>Done in </color><color=#ffffff>" + num2 + "m " + num3 + "s</color></size>";
		}
		else if (num3 > 0)
		{
			egg_fuser_text.GetComponent<Text>().text = "<size=40><color=#00ff00>Mixing eggs...</color>\n<color=#7B7B7B>Done in </color><color=#ffffff>" + num3 + "s</color></size>";
		}
		else
		{
			LayOutEggFuser();
			RedrawContainerSlots();
		}
	}

	public void RedrawMinigameRespawn()
	{
		if (!GameController.Instance.interacting_element_item.HasActiveOrExpiredRespawn("rewards_respawn"))
		{
			MinigameMenu.Instance.loot_respawn_text.text = "???";
			return;
		}
		DateTime respawnDateTime = GameController.Instance.interacting_element_item.GetRespawnDateTime("rewards_respawn");
		DateTime utcNow = DateTime.UtcNow;
		int num = (int)(respawnDateTime - utcNow).TotalDays;
		int num2 = (int)(respawnDateTime - utcNow).TotalHours - num * 24;
		int num3 = (int)(respawnDateTime - utcNow).TotalMinutes - num * 1440 - num2 * 60;
		int num4 = (int)(respawnDateTime - utcNow).TotalSeconds - num * 86400 - num2 * 3600 - num3 * 60;
		if (num2 > 0)
		{
			MinigameMenu.Instance.loot_respawn_text.text = "<color=#bbbbbb>Prizes change in:</color> " + num2 + " hours, " + num3 + " mins";
		}
		else if (num3 > 0)
		{
			MinigameMenu.Instance.loot_respawn_text.text = "<color=#bbbbbb>Prizes change in:</color> " + num3 + " mins, " + num4 + " seconds";
		}
		else if (num4 > 0)
		{
			MinigameMenu.Instance.loot_respawn_text.text = "<color=#bbbbbb>Prizes change in:</color> " + num4 + " seconds";
		}
		else
		{
			MinigameMenu.Instance.loot_respawn_text.text = "<color=#bbbbbb>Prizes change in:</color> 0 seconds";
		}
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
		AudioControl.Instance.PlayGenericClick();
		do_cascade_craft_buttons(dir);
		switch (dir)
		{
		case -1:
			crafting_page_left.GetComponent<Animation>().Stop();
			crafting_page_left.GetComponent<Animation>().Play();
			break;
		case 1:
			crafting_page_right.GetComponent<Animation>().Stop();
			crafting_page_right.GetComponent<Animation>().Play();
			break;
		}
		if (GameServerConnector.Instance.FullyInGame() && WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.teleport && WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.right && !GameServerConnector.Instance.is_host)
		{
			PopupControl.Instance.ShowConnecting("Loading teleporters");
			if (!CustomTeleporterControl.Instance.in_search_screen)
			{
				GameServerSender.Instance.RequestPageOfTeleportersByPageNumber(craft_PAGE + dir, false);
			}
			else
			{
				GameServerSender.Instance.RequestPageOfTeleportersByPageNumber(CustomTeleporterControl.Instance.search_page + dir, true);
			}
			return;
		}
		if (page_exists(dir))
		{
			craft_PAGE += dir;
			GameServerConnector.Instance.FullyInGame();
			if (curr_crafting_list != null)
			{
				curr_crafting_list.saved_page += dir;
			}
			else if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.teleport && WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.left)
			{
				CustomTeleporterControl.Instance.curr_hardcoded_teleporters_page += dir;
			}
			RedrawCraftingSlotButtons();
		}
		crafting_page_left.SetActive(page_exists(-1));
		crafting_page_right.SetActive(page_exists(1));
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
		WindowControl.Instance.VisuallySelectLeftMiniwindowTab();
		OnLeftMiniwindowTabClicked();
		StartCoroutine(mix_items());
	}

	public void AcceptBuy()
	{
		int buyPrice = MerchantControl.Instance.GetBuyPrice(trying_to_craft.item);
		DeductCoins(trying_to_craft.count * buyPrice);
		GiveItem(trying_to_craft.item, trying_to_craft.count, "");
		curr_crafting_list.items[buy_index] = new ItemCountPair(curr_crafting_list.items[buy_index].item, Mathf.Max(curr_crafting_list.items[buy_index].count - trying_to_craft.count, 0));
		InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
		InventoryItem new_item = ChunkControl.Instance.EncodeItemListIntoItem("sell_list_" + interacting_element_item.GetString("merchant_type"), curr_crafting_list.items, interacting_element_item);
		ConstructionControl.Instance.PlayerReplaceInteracting(new_item, true);
		MerchantControl.Instance.DrawMerchantSlot(buy_index - craft_PAGE * 3, buy_index);
		StartCoroutine(DelayedBuySuccessNotif(trying_to_craft));
	}

	public void ShowDelayedBuySuccessNotif(ItemCountPair received)
	{
		StartCoroutine(DelayedBuySuccessNotif(received));
	}

	private IEnumerator DelayedBuySuccessNotif(ItemCountPair received)
	{
		yield return new WaitForSeconds(0.13f);
		string text = ((received.item.item_name == "Saved Record") ? DevBuildControl.Instance.NPC_musicboxes[received.item.GetShort("npc_record_index")].sale_name : ((!(received.item.item_name == "Painting")) ? GetFullItemName(received.item) : DevBuildControl.Instance.NPC_paintings[received.item.GetShort("dev_painting_id")].name));
		string text2 = TranslationControl.Instance.TranslateGeneral("Received XYZ!", "GUI");
		string newValue = ((received.count != 1) ? ("x" + received.count + " " + text) : text);
		string tEXT = "<color=#eeee55>" + text + "</color>\n" + text2.Replace("XYZ", newValue);
		ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, tEXT, new Color(0.45f, 0.92f, 1f, 1f), received.item, received.count, new Color(0.26f, 0.6f, 0.79f, 1f), ShopControl.popup_context.craft_message);
		GameController.Instance.sound_buy();
	}

	public void DeductCoins(int amount)
	{
		player_inventory.RemoveItemByName("Coins", amount);
	}

	private IEnumerator mix_items()
	{
		angular_animation_playing = true;
		yield return new WaitForSeconds(0.1f);
		int index = crafting_mixer_slots[0].index;
		int slot_instance_index = -1;
		if ((uint)(index - 15) < 5u)
		{
			slot_instance_index = index;
		}
		else if (index >= 0 && index < n_slots_per_page_)
		{
			if (page_inventory != 1)
			{
				inv_page_switch(1);
			}
			slot_instance_index = index;
		}
		else if (index >= p2_begin_ && index < n_slots_per_page_ + p2_begin_)
		{
			if (page_inventory != 2)
			{
				inv_page_switch(2);
			}
			slot_instance_index = index - p2_begin_;
		}
		List<GameObject> floating_mix_icons = new List<GameObject>();
		for (int i = 0; i < crafting_mixer_slots.Count; i++)
		{
			if (crafting_mixer_slots[i].index != index || crafting_mixer_slots[i].deduct_amount != 0)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(type_particle_mixer);
				gameObject.transform.SetParent(slot_parent);
				gameObject.transform.localRotation = Quaternion.identity;
				if (page_inventory == 2)
				{
					if (crafting_mixer_slots[i].index > n_slots_per_page_)
					{
						gameObject.transform.localPosition = instantiated_inv_slots[crafting_mixer_slots[i].index - p2_begin_].transform.localPosition;
					}
					else
					{
						gameObject.transform.localPosition = invPAGE1.transform.localPosition;
					}
				}
				else if (page_inventory == 1)
				{
					if (crafting_mixer_slots[i].index < p2_begin_)
					{
						gameObject.transform.localPosition = instantiated_inv_slots[crafting_mixer_slots[i].index].transform.localPosition;
					}
					else
					{
						gameObject.transform.localPosition = invPAGE2.transform.localPosition;
					}
				}
				gameObject.transform.localScale = Vector3.one;
				floating_mix_icons.Add(gameObject);
				gameObject.transform.GetChild(0).GetComponent<ItemSprite>().RedrawBasic(player_inventory[crafting_mixer_slots[i].index].item, player_inventory[crafting_mixer_slots[i].index].count);
			}
			int index2 = crafting_mixer_slots[i].index;
			player_inventory[index2] = new ItemCountPair(player_inventory[index2].item, player_inventory[index2].count - crafting_mixer_slots[i].deduct_amount);
			if (player_inventory[crafting_mixer_slots[i].index].count >= 1)
			{
				continue;
			}
			player_inventory[crafting_mixer_slots[i].index] = new ItemCountPair("", 0);
			if (page_inventory == 2)
			{
				if (crafting_mixer_slots[i].index >= p2_begin_)
				{
					int num = crafting_mixer_slots[i].index - p2_begin_;
					instantiated_inv_slots[num].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryNormal("", 0, num);
				}
			}
			else if (page_inventory == 1 && crafting_mixer_slots[i].index <= n_slots_per_page_)
			{
				int index3 = crafting_mixer_slots[i].index;
				instantiated_inv_slots[index3].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryNormal("", 0, index3);
			}
		}
		Vector3 target_position = instantiated_inv_slots[slot_instance_index].transform.localPosition;
		for (float i2 = 0f; i2 < 10f; i2 += 1f)
		{
			foreach (GameObject item in floating_mix_icons)
			{
				item.transform.localPosition = Vector3.Lerp(item.transform.localPosition, target_position, i2 * 0.125f);
			}
			yield return new WaitForEndOfFrame();
		}
		foreach (GameObject item2 in floating_mix_icons)
		{
			UnityEngine.Object.Destroy(item2);
		}
		GiveItem(trying_to_craft.item, trying_to_craft.count, "", false);
		instantiated_inv_slots[slot_instance_index].GetComponent<InventorySlotObject>().animated_segment.Play("inventory_craft_oncomplete");
		RedrawInventorySlots();
		show_angular(target_position, default_angular_col);
		if (GetItemSoundOnCraft(trying_to_craft.item.item_name) == sound_on_craft.bubbles)
		{
			AudioControl.Instance.Play(AudioControl.Instance.sfx_bubbles_crafting);
		}
		yield return new WaitForSeconds(0.5f);
		angular_animation_playing = false;
	}

	private BasketContents ClonePlayerInventory()
	{
		BasketContents basketContents = new BasketContents();
		foreach (int item in player_inventory.FilledSlots())
		{
			basketContents[item] = new ItemCountPair(player_inventory[item].item, player_inventory[item].count);
		}
		return basketContents;
	}

	private int TestCanCraftItem(string item_name, int count, string reqA_item, int reqA_n_remaining, string reqB_item = "", int reqB_n_remaining = 0)
	{
		BasketContents basketContents = ClonePlayerInventory();
		List<craft_mixer_slot> list = new List<craft_mixer_slot>();
		foreach (int item in basketContents.FilledSlots())
		{
			if (reqA_n_remaining != 0 && basketContents[item].item.item_name == reqA_item)
			{
				int num = ((basketContents[item].count <= reqA_n_remaining) ? basketContents[item].count : reqA_n_remaining);
				basketContents[item] = new ItemCountPair(basketContents[item].item, basketContents[item].count - num);
				list.Add(new craft_mixer_slot
				{
					index = item,
					deduct_amount = num
				});
				reqA_n_remaining = Mathf.Max(reqA_n_remaining - num, 0);
			}
			else if (reqB_item != "" && reqB_n_remaining != 0 && basketContents[item].item.item_name == reqB_item)
			{
				int num2 = ((basketContents[item].count <= reqB_n_remaining) ? basketContents[item].count : reqB_n_remaining);
				basketContents[item] = new ItemCountPair(basketContents[item].item, basketContents[item].count - num2);
				list.Add(new craft_mixer_slot
				{
					index = item,
					deduct_amount = num2
				});
				reqB_n_remaining = Mathf.Max(reqB_n_remaining - num2, 0);
			}
			if (reqB_item == "")
			{
				if (reqA_n_remaining == 0)
				{
					goto IL_done;
				}
			}
			else if (reqA_n_remaining == 0 && reqB_n_remaining == 0)
			{
				goto IL_done;
			}
		}
		return 1;
		IL_done:
		crafting_mixer_slots.Clear();
		if (!CanReceiveItem(item_name, count, false, basketContents, true))
		{
			return 2;
		}
		foreach (craft_mixer_slot item2 in list)
		{
			crafting_mixer_slots.Add(item2);
		}
		return 0;
	}

	public void show_angular(Vector3 localPos, Color angular_col, angular_sound_t angular_sound = angular_sound_t.default_)
	{
		angular.SetActive(true);
		angular.transform.localPosition = localPos;
		angular.GetComponent<Animation>().Play();
		angular.transform.Find("Image (1)").GetComponent<Image>().color = angular_col;
		angular.transform.SetAsLastSibling();
		switch (angular_sound)
		{
		case angular_sound_t.sell:
			GameController.Instance.sound_sell();
			break;
		case angular_sound_t.default_:
			GameController.Instance.sound_ding();
			break;
		}
	}

	private bool ShouldContainerGenerateLoot(InventoryItem container_item)
	{
		if (container_item.HasActiveRespawn("loot_spawn"))
		{
			return false;
		}
		if (container_item.GetString("bandit_camp_instance") != "")
		{
			return !BanditCampsControl.Instance.GetBanditCampInstanceByName(container_item.GetString("bandit_camp_instance")).flag_destroyed;
		}
		return true;
	}

	public void TryOpenWorldContainer(InventoryItem container_item, int container_rot, int innerX, int innerZ, int chunkX, int chunkZ)
	{
		string item_name = GameController.Instance.interacting_element_item.item_name;
		int @long = GameController.Instance.interacting_element_item.GetLong("basket_id");
		if (GameServerConnector.Instance.FullyInGame())
		{
			string obj_str = ChunkControl.Instance.player_zone + "," + chunkX + "," + chunkZ + "," + innerX + "," + innerZ;
			if (!GameServerInterface.Instance.AnyoneUsing(obj_str))
			{
				GameServerSender.chest_request_type chest_request_type_t;
				if (item_name == "Gold Chest" || item_name == "Titanium Chest" || item_name == "Cave Basket" || item_name == "Cave Chest" || item_name == "Sky Chest" || item_name == "Loot Chest" || item_name == "Loot Basket" || item_name == "Boss Chest")
				{
					chest_request_type_t = (((!(item_name == "Sky Chest")) ? ShouldContainerGenerateLoot(container_item) : (container_item.GetShort("loot_generated") == 0)) ? GameServerSender.chest_request_type.unopened_loot : GameServerSender.chest_request_type.normal);
				}
				else
				{
					chest_request_type_t = ((item_name == "Egg Fuser") ? GameServerSender.chest_request_type.egg_fuser : GameServerSender.chest_request_type.normal);
				}
				GameServerSender.Instance.SendRequestCurrContainer(chest_request_type_t, "");
			}
			else
			{
				PopupControl.Instance.ShowMessage("Can't open " + item_name + "\nSomeone else is using it right now!", PopupControl.context.message);
			}
			return;
		}
		if ((item_name == "Sky Chest") ? (container_item.GetShort("loot_generated") == 0) : ((item_name == "Gold Chest" || item_name == "Titanium Chest" || item_name == "Cave Basket" || item_name == "Cave Chest" || item_name == "Loot Basket" || item_name == "Loot Chest" || item_name == "Boss Chest") && ShouldContainerGenerateLoot(container_item)))
		{
			LootControl.Instance.GenerateLootChest(GameController.Instance.interacting_element_item).SaveToAllAsContainer(@long);
			AddChestRespawnAndRedraw();
		}
		BasketContents basketContents = new BasketContents();
		basketContents.LoadFromDiskAsContainer(@long);
		SucceedOpenWorldContainer(@long, basketContents);
	}

	public void SucceedOpenWorldContainer(int container_id, BasketContents contents)
	{
		curr_container = contents;
		container_style = container_style_t.world_container;
		page_container = 1;
		string item_name = GameController.Instance.interacting_element_item.item_name;
		if (item_name == "Gold Chest" || item_name == "Titanium Chest" || item_name == "Egg Fuser" || item_name == "Cave Basket" || item_name == "Cave Chest" || item_name == "Loot Basket" || item_name == "Loot Chest" || item_name == "Boss Chest")
		{
			text_loot_respawn_in.text = "???";
		}
		OpenInventoryAndContainer(TranslationControl.Instance.TranslateItemName(item_name));
	}

	public void SucceedOpenCompanionPockets(ActiveCompanion companion)
	{
		container_style = container_style_t.companion_pockets;
		curr_container = companion.GetPocketContents();
		page_container = 1;
		OpenInventoryAndContainer(companion.companion_name);
	}

	public void SucceedOpenTradingTable()
	{
		curr_container = new BasketContents();
		container_style = container_style_t.trading_table;
		page_container = 1;
		OpenInventoryAndContainer(TranslationControl.Instance.TranslateItemName("Trading Table"));
	}

	public void AddChestRespawnAndRedraw()
	{
		InventoryItem new_item;
		if (GameController.Instance.interacting_element_item.item_name != "Sky Chest")
		{
			DateTime uTC_when = DateTime.UtcNow.AddSeconds(0.0).AddMinutes(0.0).AddHours(2.0);
			new_item = ChunkControl.Instance.EncodeRespawnIntoItem("loot_spawn", GameController.Instance.interacting_element_item, uTC_when);
		}
		else
		{
			ExtraInventoryData extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
			extraDataCopy.SetShort("loot_generated", 1);
			new_item = new InventoryItem("Sky Chest", extraDataCopy);
		}
		ConstructionControl.Instance.PlayerReplaceInteracting(new_item, true);
	}

	public int GetPlayerItemCount(string item_name)
	{
		int num = 0;
		foreach (int item in player_inventory.FilledSlots())
		{
			if (player_inventory[item].item.item_name == item_name)
			{
				num += player_inventory[item].count;
			}
		}
		return num;
	}

	public bool iap_allowed(InventoryItem item)
	{
		if (PlayerData.Instance.GetGlobalShort("dev_mode") != 1)
		{
			string itemIapKeyRequired = GetItemIapKeyRequired(item);
			if (itemIapKeyRequired != "")
			{
				return PlayerData.Instance.GetGlobalShort(itemIapKeyRequired) == 1;
			}
		}
		return true;
	}

	public void iap_deny_popup(InventoryItem item, string verb)
	{
		for (int i = 0; i < ShopControl.Instance.purchase_structs_ordering.Length; i++)
		{
			string text = ShopControl.Instance.purchase_structs_ordering[i];
			if (ShopControl.Instance.GetPurchaseableKey(text) == GetItemIapKeyRequired(item))
			{
				string text2 = TranslationControl.Instance.TranslateGeneral("Premium Item", "Market");
				string newValue = "<color=#62CBFF>" + TranslationControl.Instance.TranslateGeneral(text, "Market") + "</color>";
				string newValue2 = TranslationControl.Instance.TranslateGeneral(verb, "Market");
				string text3 = TranslationControl.Instance.TranslateGeneral("You must get the IAP expansion from the Market to VRB this.", "Market").Replace("VRB", newValue2).Replace("IAP", newValue);
				PopupControl.Instance.ShowMessage("<color=#888888>" + text2 + "</color>\n" + text3, PopupControl.context.message);
				break;
			}
		}
	}

	private int GetMaxBuy(InventoryItem item, int og_max)
	{
		int num = HowManyCanReceive(item.item_name);
		int num2 = HowManyCanAfford(item);
		if (num2 <= num)
		{
			num = num2;
		}
		if (og_max <= num)
		{
			num = og_max;
		}
		return num;
	}

	public void PressBuyItem(int button_index)
	{
		InventoryItem item = curr_crafting_list.items[craft_PAGE * 3 + button_index].item;
		int num = curr_crafting_list.items[craft_PAGE * 3 + button_index].count;
		string text = GetFullItemName(item);
		if (!iap_allowed(item))
		{
			iap_deny_popup(item, "buy");
			return;
		}
		if (num < 1)
		{
			ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, "<color=#eeee55>" + text + "</color>\n" + TranslationControl.Instance.TranslateGeneral("Out of stock. Come back tomorrow!", "GUI"), new Color(0.74509805f, 0.74509805f, 0.74509805f, 1f), item, 1, new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f), ShopControl.popup_context.craft_message);
			return;
		}
		int playerItemCount = GetPlayerItemCount("Coins");
		if (item.item_name == "Saved Record")
		{
			text = DevBuildControl.Instance.NPC_musicboxes[item.GetShort("npc_record_index")].sale_name;
		}
		else if (item.item_name == "Painting")
		{
			text = DevBuildControl.Instance.NPC_paintings[item.GetShort("dev_painting_id")].name;
		}
		int buyPrice = MerchantControl.Instance.GetBuyPrice(item);
		if (playerItemCount == -1 || buyPrice == -1)
		{
			ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, "ERROR", new Color(0.9411765f, 0.3019608f, 0.3019608f, 1f), item, 1, new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f), ShopControl.popup_context.craft_message);
			return;
		}
		if (!CanReceiveItem(item, 1))
		{
			ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, "<color=#eeee55>" + text + "</color>\n" + TranslationControl.Instance.TranslateGeneral("Inventory Full!", "GUI"), new Color(0.9411765f, 0.3019608f, 0.3019608f, 1f), item, 1, new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f), ShopControl.popup_context.craft_message);
			return;
		}
		if (buyPrice > playerItemCount)
		{
			ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, "<color=#eeee55>" + text + "</color>\n" + TranslationControl.Instance.TranslateGeneral("Not enough coins!", "GUI"), new Color(0.9411765f, 0.3019608f, 0.3019608f, 1f), item, 1, new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f), ShopControl.popup_context.craft_message);
			return;
		}
		int maxBuy = GetMaxBuy(item, num);
		buy_index = craft_PAGE * 3 + button_index;
		trying_to_craft = new ItemCountPair(item, 1);
		if (maxBuy != 1)
		{
			ShopControl.Instance.ShowShopPopup("ACCEPT", ShopControl.button_color_t.yes_green, "CANCEL", ShopControl.button_color_t.no_red, false, TranslationControl.Instance.TranslateGeneral("Buy how many?", "GUI"), new Color(1f, 1f, 1f, 1f), item, 1, new Color(0.2980392f, 0.50980395f, 0.5294118f, 1f), ShopControl.popup_context.vendor_buy_yes_no, maxBuy);
			return;
		}
		string tEXT = TranslationControl.Instance.TranslateGeneral("Buy XYZ?", "GUI").Replace("XYZ", "<color=#eeee55>" + text + "</color>");
		ShopControl.Instance.ShowShopPopup("YES", ShopControl.button_color_t.yes_green, "CANCEL", ShopControl.button_color_t.no_red, false, tEXT, new Color(1f, 1f, 1f, 1f), item, 1, new Color(0.29803923f, 0.50980395f, 0.5294118f, 1f), ShopControl.popup_context.vendor_buy_yes_no);
	}

	public void PressCraftItem(int button_index)
	{
		InventoryItem item = curr_crafting_list.items[craft_PAGE * 3 + button_index].item;
		int count = curr_crafting_list.items[craft_PAGE * 3 + button_index].count;
		string fullItemName = GetFullItemName(item);
		if (!iap_allowed(item))
		{
			iap_deny_popup(item, "craft");
			return;
		}
		if (GameController.Instance.player_stats[5] < GetItemCraftingLevelRequired(item.item_name))
		{
			ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, "<color=#eeee55>" + fullItemName + "</color>\n" + TranslationControl.Instance.TranslateGeneral("Your crafting skill is too low to build this!", "GUI"), new Color(0.9411765f, 0.3019608f, 0.3019608f, 1f), item, count, new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f), ShopControl.popup_context.craft_message);
			return;
		}
		string item_name = curr_crafting_list.items[craft_PAGE * 3 + button_index].item.item_name;
		int count2 = curr_crafting_list.items[craft_PAGE * 3 + button_index].count;
		string itemCraftingIngredientA = GetItemCraftingIngredientA(item_name);
		int itemCraftingIngredientA_count = GetItemCraftingIngredientA_count(item_name);
		string itemCraftingIngredientB = GetItemCraftingIngredientB(item_name);
		int itemCraftingIngredientB_count = GetItemCraftingIngredientB_count(item_name);
		int num = TestCanCraftItem(item_name, count2, itemCraftingIngredientA, itemCraftingIngredientA_count, itemCraftingIngredientB, itemCraftingIngredientB_count);
		if (num == 1)
		{
			string craftingAltIngredientA = GetCraftingAltIngredientA(item_name);
			int itemCraftingAltIngredientA_count = GetItemCraftingAltIngredientA_count(item_name);
			if (!Startup.StringNullOrWhitespace(craftingAltIngredientA))
			{
				num = TestCanCraftItem(item_name, count2, craftingAltIngredientA, itemCraftingAltIngredientA_count);
			}
		}
		switch (num)
		{
		case 2:
			ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, "<color=#eeee55>" + fullItemName + "</color>\n" + TranslationControl.Instance.TranslateGeneral("Not enough inventory space", "GUI"), new Color(0.9411765f, 0.3019608f, 0.3019608f, 1f), item, count, new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f), ShopControl.popup_context.craft_message);
			break;
		case 1:
			ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, "<color=#eeee55>" + TranslationControl.Instance.TranslateGeneral("To build this, you need", "GUI") + "</color>", new Color(0.9411765f, 0.3019608f, 0.3019608f, 1f), item, count, new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f), ShopControl.popup_context.craft_message, -1, new InventoryItem(itemCraftingIngredientA), itemCraftingIngredientA_count, new InventoryItem(itemCraftingIngredientB), itemCraftingIngredientB_count, ShopControl.req_context_t.insufficient);
			break;
		case 0:
		{
			trying_to_craft = new ItemCountPair(item, count);
			string tEXT = TranslationControl.Instance.TranslateGeneral("Build a XYZ?", "GUI").Replace("XYZ", "<color=#eeee55>" + fullItemName + "</color>");
			ShopControl.Instance.ShowShopPopup("YES", ShopControl.button_color_t.yes_green, "CANCEL", ShopControl.button_color_t.no_red, false, tEXT, new Color(1f, 1f, 1f, 1f), item, count, new Color(0.29803923f, 0.50980395f, 0.5294118f, 1f), ShopControl.popup_context.craft_yes_no);
			break;
		}
		}
	}

	public InventoryItem AdjustMouseItemData(InventoryItem item)
	{
		if (item.item_name == "Armor Display" || item.item_name == "Custom Statue")
		{
			if (item.GetString("creature_A") != "")
			{
				return item;
			}
			string value = "human";
			string value2 = "human";
			if (GameController.Instance.player != null)
			{
				LiteModel myCreatureModel = GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel;
				value = myCreatureModel.original.creatures_that_made_me[0];
				value2 = myCreatureModel.original.creatures_that_made_me[1];
			}
			ExtraInventoryData extraDataCopy = item.GetExtraDataCopy();
			extraDataCopy.SetString("creature_A", value);
			extraDataCopy.SetString("creature_B", value2);
			return new InventoryItem(item.item_name, extraDataCopy);
		}
		if (item.item_name == "Companion")
		{
			if (CompanionController.Instance.GetCurrSelectedCompanion() == null)
			{
				return item;
			}
			string value3 = ((PlayerData.Instance.GetGlobalString("username_lower") != "") ? PlayerData.Instance.GetGlobalString("username_lower") : "ME");
			ExtraInventoryData extraDataCopy2 = item.GetExtraDataCopy();
			extraDataCopy2.SetString("companion_owner", value3);
			return new InventoryItem(item.item_name, extraDataCopy2);
		}
		if (item.item_name == "3-day Land Claim")
		{
			DateTime uTC_when = DateTime.UtcNow;
			uTC_when = uTC_when.AddSeconds(LandClaimControl.three_day_land_claim_add_seconds);
			uTC_when = uTC_when.AddDays(LandClaimControl.three_day_land_claim_add_days);
			return ChunkControl.Instance.EncodeRespawnIntoItem("landclaim_spawn", item, uTC_when);
		}
		if (item.item_name == "8-day Land Claim")
		{
			DateTime uTC_when2 = DateTime.UtcNow;
			uTC_when2 = uTC_when2.AddSeconds(LandClaimControl.eight_day_land_claim_add_seconds);
			uTC_when2 = uTC_when2.AddDays(LandClaimControl.eight_day_land_claim_add_days);
			return ChunkControl.Instance.EncodeRespawnIntoItem("landclaim_spawn", item, uTC_when2);
		}
		if (item.item_name == "Admin Land Claim")
		{
			DateTime uTC_when3 = DateTime.UtcNow;
			uTC_when3 = uTC_when3.AddSeconds(LandClaimControl.admin_land_claim_add_seconds);
			uTC_when3 = uTC_when3.AddDays(LandClaimControl.admin_land_claim_add_days);
			return ChunkControl.Instance.EncodeRespawnIntoItem("landclaim_spawn", item, uTC_when3);
		}
		if (item.item_name == "Boss Spawner - Shindeon" || item.item_name == "Boss Spawner - Yandeon")
		{
			DateTime uTC_when4 = DateTime.UtcNow;
			uTC_when4 = uTC_when4.AddSeconds(60.0);
			return ChunkControl.Instance.EncodeRespawnIntoItem("mob_spawn", item, uTC_when4);
		}
		return item;
	}

	public void DragOntoSlot(int drag_onto_index, GameObject drag_onto_obj)
	{
		int index = mouse_down_slot.GetComponent<InventorySlotObject>().index;
		if (drag_onto_index == trash_index)
		{
			player_inventory[GetTrueIndex(page_inventory, index)] = new ItemCountPair("", 0);
			RedrawInventorySlots();
			return;
		}
		if (drag_onto_index == page_1_index || drag_onto_index == page_2_index || drag_onto_index == page_3_index)
		{
			if (WindowControl.Instance.curr_miniwindow != WindowControl.miniwindow_type_t.inventory_and_container || WindowControl.Instance.curr_miniwindow_tab_selected != WindowControl.tab.right)
			{
				int trueIndex = GetTrueIndex(page_inventory, index);
				int count = player_inventory[trueIndex].count;
				int num = ToInventory(player_inventory[trueIndex].item, player_inventory[trueIndex].count, (page_1_index != drag_onto_index) ? ptype.secondPage : ptype.firstPage);
				if (num == 0)
				{
					player_inventory[trueIndex] = new ItemCountPair("", 0);
				}
				else if (num != count)
				{
					player_inventory[trueIndex] = new ItemCountPair(player_inventory[trueIndex].item, num);
				}
				RedrawInventorySlots();
				return;
			}
			int trueIndex2 = GetTrueIndex(page_container, index);
			int count2 = curr_container[trueIndex2].count;
			int num2 = -1;
			if (page_1_index == drag_onto_index)
			{
				num2 = ToContainer(curr_container[trueIndex2].item, curr_container[trueIndex2].count, ptype.firstPage);
			}
			else if (page_2_index == drag_onto_index)
			{
				num2 = ToContainer(curr_container[trueIndex2].item, curr_container[trueIndex2].count, ptype.secondPage);
			}
			else if (page_3_index == drag_onto_index)
			{
				num2 = ToContainer(curr_container[trueIndex2].item, curr_container[trueIndex2].count, ptype.thirdPage);
			}
			if (num2 == 0)
			{
				curr_container[trueIndex2] = new ItemCountPair("", 0);
			}
			else if (num2 != count2)
			{
				curr_container[trueIndex2] = new ItemCountPair(curr_container[trueIndex2].item, num2);
			}
			RedrawContainerSlots();
			return;
		}
		switch (drag_onto_index)
		{
		case -5:
			attempt_take_item(GetTrueIndex(page_container, index), false);
			if (GameController.Instance.interacting_element_item.item_name == "Egg Fuser")
			{
				LayOutEggFuser();
			}
			RedrawContainerSlots();
			return;
		case -4:
			attempt_store_item(GetTrueIndex(page_inventory, index));
			RedrawInventorySlots();
			return;
		}
		ItemCountPair itemCountPair = ((WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container && WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.right) ? curr_container[GetTrueIndex(page_container, drag_onto_index)] : ((drag_onto_index != hand_index && drag_onto_index != hat_index && drag_onto_index != body_index) ? player_inventory[GetTrueIndex(page_inventory, drag_onto_index)] : player_inventory[drag_onto_index]));
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container && WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.right)
		{
			int trueIndex3 = GetTrueIndex(page_container, drag_onto_index);
			int trueIndex4 = GetTrueIndex(page_container, index);
			curr_container[trueIndex3] = curr_container[trueIndex4];
			curr_container[trueIndex4] = itemCountPair;
			if (itemCountPair.item.item_name != "")
			{
				mouse_down_slot.GetComponent<InventorySlotObject>().animated_segment.Play("inventory_craft_oncomplete");
			}
			drag_onto_obj.GetComponent<InventorySlotObject>().animated_segment.Play("inventory_craft_oncomplete");
		}
		else
		{
			int trueIndex5 = GetTrueIndex(page_inventory, drag_onto_index);
			int trueIndex6 = GetTrueIndex(page_inventory, index);
			if (itemCountPair.item == player_inventory[trueIndex6].item && GetItemMaxStack(itemCountPair.item.item_name) >= 2 && itemCountPair.count != GetItemMaxStack(itemCountPair.item.item_name) && player_inventory[trueIndex6].count != GetItemMaxStack(player_inventory[trueIndex6].item.item_name))
			{
				int num3 = GetItemMaxStack(player_inventory[trueIndex5].item.item_name) - player_inventory[trueIndex5].count;
				if (num3 < player_inventory[trueIndex6].count)
				{
					player_inventory[trueIndex5] = new ItemCountPair(player_inventory[trueIndex5].item, player_inventory[trueIndex5].count + num3);
					player_inventory[trueIndex6] = new ItemCountPair(player_inventory[trueIndex6].item, player_inventory[trueIndex6].count - num3);
				}
				else
				{
					player_inventory[trueIndex5] = new ItemCountPair(player_inventory[trueIndex5].item, player_inventory[trueIndex6].count + player_inventory[trueIndex5].count);
					player_inventory[trueIndex6] = new ItemCountPair("", 0);
				}
			}
			else
			{
				player_inventory[trueIndex5] = player_inventory[trueIndex6];
				player_inventory[trueIndex6] = itemCountPair;
				if (itemCountPair.item.item_name != "")
				{
					mouse_down_slot.GetComponent<InventorySlotObject>().animated_segment.Play("inventory_craft_oncomplete");
				}
				drag_onto_obj.GetComponent<InventorySlotObject>().animated_segment.Play("inventory_craft_oncomplete");
			}
		}
		if (WindowControl.Instance.curr_miniwindow != WindowControl.miniwindow_type_t.inventory_and_container || WindowControl.Instance.curr_miniwindow_tab_selected != WindowControl.tab.right)
		{
			RedrawInventorySlots();
			return;
		}
		if (GameController.Instance.interacting_element_item.item_name == "Egg Fuser")
		{
			LayOutEggFuser();
		}
		RedrawContainerSlots();
	}

	public void FillOutBookData(string book_name, ref ExtraInventoryData extra_data)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (book_name == "")
		{
			return;
		}
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("Books/" + book_name, ref file_exists);
		if (file_exists)
		{
			List<string> list = new List<string>();
			string text = null;
			foreach (string item in textFileLines)
			{
				if (string.IsNullOrWhiteSpace(item))
				{
					continue;
				}
				if (item.StartsWith("[") && item.EndsWith("]"))
				{
					if (text != null)
					{
						dictionary[text] = string.Join(" ", list);
					}
					text = item.Trim('[', ']');
					list.Clear();
				}
				else
				{
					list.Add(item);
				}
			}
			if (text != null)
			{
				dictionary[text] = string.Join(" ", list);
			}
		}
		extra_data.SetString("dev_book_title", book_name);
		if (dictionary.ContainsKey("paint"))
		{
			extra_data.SetString("paint", dictionary["paint"]);
		}
		if (dictionary.ContainsKey("stamp"))
		{
			extra_data.SetString("stamp", dictionary["stamp"]);
		}
	}

	private void LayOutEggFuser()
	{
		if (GameController.Instance.interacting_element_item.GetShort("started_fusion") == 1)
		{
			if (!GameController.Instance.interacting_element_item.HasActiveRespawn("fuser_spawn"))
			{
				LayOutInvSlots(false, false, slots_positionings.show_1_fuser, false, 1, false, "", fusion_button.hatch);
			}
			else
			{
				LayOutInvSlots(false, false, slots_positionings.show_3_fuser, false, 1, false, "<size=40><color=#00ff00>Mixing eggs...</color>\n<color=#7B7B7B>Done in </color><color=#ffffff>???</color></size>", fusion_button.hide);
			}
		}
		else if (curr_container[5].item.item_name == "Egg" && curr_container[7].item.item_name == "Egg" && curr_container[11].item.item_name == "Uranium Bar")
		{
			LayOutInvSlots(false, false, slots_positionings.show_3_fuser, false, 1, false, "", fusion_button.start);
		}
		else
		{
			LayOutInvSlots(false, false, slots_positionings.show_3_fuser, false, 1, false, "Insert two eggs and one uranium bar\nto create a hybrid animal pet", fusion_button.hide);
		}
	}

	public void UpdateManaMods(string hat_item)
	{
		float mana_modifier;
		switch (hat_item)
		{
		case "Blue Wizard Hat":
		case "Purple Wizard Hat":
		case "Grey Wizard Hat":
			mana_modifier = 1.25f;
			break;
		case "Black Wizard Hat":
		case "Storm Wizard Hat":
			mana_modifier = 1.5f;
			break;
		case "White Wizard Hat":
			mana_modifier = 2f;
			break;
		default:
			mana_modifier = 1f;
			break;
		}
		float mana_available = PerkControl.Instance.mana_available;
		PerkControl.Instance.mana_modifier = mana_modifier;
		float num = PerkControl.Instance.MaxManaWithModifiers();
		if (mana_available > num)
		{
			PerkControl.Instance.mana_available = num;
		}
		PerkControl.Instance.UpdateManaVisual();
		PerkControl.Instance.UpdateMainSlotsColor();
	}

	public void EquipIfPossible(int slot_index)
	{
		if (slot_index == hand_index)
		{
			show_angular(instantiated_inv_slots[hand_index].transform.localPosition, default_angular_col);
			GameController.Instance.player.GetComponent<SharedCreature>().hand_ = player_inventory[hand_index].item;
			GameServerSender.Instance.SendChangeEquipment(2, player_inventory[hand_index].item);
		}
		else if (slot_index == hat_index)
		{
			show_angular(instantiated_inv_slots[hat_index].transform.localPosition, default_angular_col);
			GameController.Instance.player.GetComponent<SharedCreature>().hat_ = player_inventory[hat_index].item;
			GameServerSender.Instance.SendChangeEquipment(0, player_inventory[hat_index].item);
		}
		else if (slot_index == body_index)
		{
			show_angular(instantiated_inv_slots[body_index].transform.localPosition, default_angular_col);
			GameController.Instance.player.GetComponent<SharedCreature>().body_ = player_inventory[body_index].item;
			GameServerSender.Instance.SendChangeEquipment(1, player_inventory[body_index].item);
		}
		if (slot_index == hand_index || slot_index == hat_index || slot_index == body_index)
		{
			GameController.Instance.player.GetComponent<SharedCreature>().OnEquipmentChanged();
		}
	}

	public void UnequipIfPossible(int inv_index)
	{
		if (inv_index == hand_index)
		{
			GameServerSender.Instance.SendChangeEquipment(2, new InventoryItem(""));
			GameController.Instance.player.GetComponent<SharedCreature>().hand_ = new InventoryItem("");
		}
		else if (inv_index == hat_index)
		{
			GameServerSender.Instance.SendChangeEquipment(0, new InventoryItem(""));
			GameController.Instance.player.GetComponent<SharedCreature>().hat_ = new InventoryItem("");
		}
		else if (inv_index == body_index)
		{
			GameServerSender.Instance.SendChangeEquipment(1, new InventoryItem(""));
			GameController.Instance.player.GetComponent<SharedCreature>().body_ = new InventoryItem("");
		}
		if (inv_index == hand_index || inv_index == hat_index || inv_index == body_index)
		{
			GameController.Instance.player.GetComponent<SharedCreature>().OnEquipmentChanged();
		}
	}

	public void click_DONE_placing()
	{
		ConstructionControl.Instance.DonePlacing(true, true);
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
		if (skill_key == "Crafting")
		{
			return true;
		}
		if (skill_key == "Attack")
		{
			return min_skill_lvl <= GameController.Instance.player_stats[3];
		}
		if (skill_key == "Health")
		{
			return min_skill_lvl <= GameController.Instance.player_stats[2];
		}
		if (skill_key == "Mining")
		{
			return min_skill_lvl <= GameController.Instance.player_stats[0];
		}
		return true;
	}

	public void DropExtraItemDataOnPickup(InventoryItem item)
	{
		ExtraInventoryData extraDataCopy = item.GetExtraDataCopy();
		extraDataCopy.SetString("tag", "");
		extraDataCopy.SetString("password", "");
		extraDataCopy.SetString("bandit_camp_instance", "");
		if (InventoryUtils.UsesShackId(item.item_name))
		{
			extraDataCopy.SetLong("shack_id", 0);
			extraDataCopy.SetShort("outer_item_chunkX", 0);
			extraDataCopy.SetShort("outer_item_chunkZ", 0);
			extraDataCopy.SetShort("outer_item_innerX", 0);
			extraDataCopy.SetShort("outer_item_innerZ", 0);
			extraDataCopy.SetShort("depth", 0);
			if (item.item_name == "Magic Bean" || item.item_name == "Spooky Well")
			{
				extraDataCopy.SetShort("dont_build_start_area", 1);
			}
		}
		if (InventoryUtils.UsesBasketId(item))
		{
			extraDataCopy.SetLong("basket_id", 0);
			if (item.item_name == "Armor Display")
			{
				extraDataCopy.SetString("hat", "");
				extraDataCopy.SetString("body", "");
				extraDataCopy.ClearSubItem("hat");
				extraDataCopy.ClearSubItem("body");
			}
			else if (item.item_name == "Custom Statue")
			{
				extraDataCopy.SetString("hat", "");
				extraDataCopy.SetString("body", "");
				extraDataCopy.SetString("wep", "");
				extraDataCopy.ClearSubItem("hat");
				extraDataCopy.ClearSubItem("body");
				extraDataCopy.ClearSubItem("wep");
			}
			else if (item.item_name == "Weapon Display")
			{
				extraDataCopy.SetString("wep", "");
				extraDataCopy.ClearSubItem("wep");
			}
			else if (item.item_name == "Large Weapon Display")
			{
				extraDataCopy.SetString("wep", "");
				extraDataCopy.SetString("wep2", "");
				extraDataCopy.ClearSubItem("wep");
				extraDataCopy.ClearSubItem("wep2");
			}
		}
		if (InventoryUtils.IsStringItem(item.item_name))
		{
			extraDataCopy.SetShort("model1_chunkX", 0);
			extraDataCopy.SetShort("model1_chunkZ", 0);
			extraDataCopy.SetShort("model1_innerX", 0);
			extraDataCopy.SetShort("model1_innerZ", 0);
		}
		string item_name = item.item_name;
		if (item_name == "Egg Fuser")
		{
			if (item.GetShort("started_fusion") == 1)
			{
				extraDataCopy.SetString("has_respawn_fuser_spawn", "");
				extraDataCopy.SetShort("started_fusion", 0);
			}
		}
		else if (item_name == "Vending Machine")
		{
			string @string = item.GetString("paint");
			string string2 = item.GetString("stamp");
			extraDataCopy = new ExtraInventoryData();
			extraDataCopy.SetString("paint", @string);
			extraDataCopy.SetString("stamp", string2);
		}
		GiveItem(new InventoryItem(item.item_name, extraDataCopy), 1, null);
	}

	public static string BiomeIdToCaveEntrance(int biome_id)
	{
		switch (biome_id)
		{
		case 1:
			return "Snow Cave Entrance";
		case 2:
			return "Desert Cave Entrance";
		case 3:
			return "Evergreen Cave Entrance";
		case 4:
		case 5:
			return "Ocean Cave Entrance";
		case 6:
		case 7:
			return "Swamp Cave Entrance";
		default:
			return "Grass Cave Entrance";
		}
	}

	public InventoryItem FinalizeItemBeforePutDown(InventoryItem item, ChunkData chunk_data, int innerX, int innerZ)
	{
		string item_name = item.item_name;
		if (item.item_name == "Personal Mine")
		{
			item_name = BiomeIdToCaveEntrance(chunk_data.biome);
		}
		ExtraInventoryData extraDataCopy = item.GetExtraDataCopy();
		if (InventoryUtils.UsesShackId(item.item_name))
		{
			extraDataCopy.SetShort("outer_item_chunkX", chunk_data.X);
			extraDataCopy.SetShort("outer_item_chunkZ", chunk_data.Z);
			extraDataCopy.SetShort("outer_item_innerX", innerX);
			extraDataCopy.SetShort("outer_item_innerZ", innerZ);
			int num = ZoneDataControl.Instance.curr_zonedata.house_item.GetShort("depth");
			if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
			{
				num += 3;
			}
			else if (item.item_name == "Upstairs Room")
			{
				num--;
			}
			else if (item.item_name == "Underground Room")
			{
				num++;
			}
			extraDataCopy.SetShort("depth", num);
			if (item.GetLong("shack_id") == 0)
			{
				extraDataCopy.SetLong("shack_id", ConstructionControl.Instance.GetNewUniqueId());
			}
		}
		else if (InventoryUtils.UsesBasketId(ITEM_USING))
		{
			if (item.GetLong("basket_id") == 0)
			{
				extraDataCopy.SetLong("basket_id", ConstructionControl.Instance.GetNewUniqueId());
			}
		}
		else if (item.item_name == "Teleporter")
		{
			extraDataCopy.SetString("teleporter_name", CustomTeleporterControl.Instance.GetNewTeleporterName());
		}
		else if (item.item_name == "Vending Machine")
		{
			extraDataCopy.SetString("vending_machine_owner", PlayerData.Instance.GetGlobalString("username_lower"));
		}
		return new InventoryItem(item_name, extraDataCopy);
	}

	public bool HasItem(InventoryItem item)
	{
		foreach (int item2 in player_inventory.FilledSlots())
		{
			if (player_inventory[item2].item == item)
			{
				return true;
			}
		}
		return false;
	}

	public void GrabNext(InventoryItem item)
	{
		foreach (int item2 in player_inventory.FilledSlots())
		{
			if (player_inventory[item2].item == item)
			{
				ITEM_USING = AdjustMouseItemData(player_inventory[item2].item);
				player_inventory[item2] = new ItemCountPair(player_inventory[item2].item, player_inventory[item2].count - 1);
				break;
			}
		}
	}

	private void Update()
	{
		if (GamepadInput.Instance.GetMouseButtonDown() && !hover_clicked)
		{
			hide_selector();
		}
		if (hover_clicked)
		{
			hover_clicked = false;
		}
		if (GamepadInput.Instance.GetMouseButtonUp())
		{
			if (test_for_dragging)
			{
				create_hover(mouse_down_slot.GetComponent<InventorySlotObject>().index);
				test_for_dragging = false;
			}
			else if (is_dragging)
			{
				GameObject gameObject = null;
				int num = -1;
				float num2 = float.MaxValue;
				for (int i = 0; i < instantiated_inv_slots.Count; i++)
				{
					if (instantiated_inv_slots[i].gameObject.activeInHierarchy)
					{
						float num3 = Vector2.Distance(drag_item_sprite.transform.parent.position, instantiated_inv_slots[i].transform.position);
						if (num3 < num2)
						{
							gameObject = instantiated_inv_slots[i];
							num2 = num3;
							num = i;
						}
					}
				}
				bool flag = false;
				if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container)
				{
					if (WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.right)
					{
						float num4 = Vector2.Distance(drag_item_sprite.transform.parent.position, WindowControl.Instance.miniwindow_header_L_bg.transform.position);
						if (num4 < num2)
						{
							gameObject = WindowControl.Instance.miniwindow_header_L_bg.gameObject;
							num = -5;
							flag = true;
							num2 = num4;
						}
					}
					if (WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.left)
					{
						float num5 = Vector2.Distance(drag_item_sprite.transform.parent.position, WindowControl.Instance.miniwindow_header_R_bg.transform.position);
						if (num5 < num2)
						{
							gameObject = WindowControl.Instance.miniwindow_header_R_bg.gameObject;
							num = -4;
							flag = true;
							num2 = num5;
						}
					}
				}
				if (trash_bin.activeInHierarchy)
				{
					float num6 = Vector2.Distance(drag_item_sprite.transform.parent.position, trash_bin.transform.position);
					if (num6 < num2)
					{
						num = trash_index;
						gameObject = trash_bin;
						flag = true;
						num2 = num6;
					}
				}
				int num7 = ((WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container && WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.right) ? page_container : page_inventory);
				if (pageSwitchers.activeInHierarchy)
				{
					if (num7 != 1)
					{
						float num8 = Vector2.Distance(drag_item_sprite.transform.parent.position, invPAGE1.transform.position);
						if (num8 < num2)
						{
							num = page_1_index;
							gameObject = invPAGE1;
							flag = true;
							num2 = num8;
						}
					}
					if (num7 != 2)
					{
						float num9 = Vector2.Distance(drag_item_sprite.transform.parent.position, invPAGE2.transform.position);
						if (num9 < num2)
						{
							num = page_2_index;
							gameObject = invPAGE2;
							flag = true;
							num2 = num9;
						}
					}
					if (num7 != 3)
					{
						float num10 = Vector2.Distance(drag_item_sprite.transform.parent.position, invPAGE3.transform.position);
						if (num10 < num2)
						{
							num = page_3_index;
							gameObject = invPAGE3;
							flag = true;
							num2 = num10;
						}
					}
				}
				bool flag2;
				if (flag)
				{
					flag2 = true;
				}
				else
				{
					int index = mouse_down_slot.GetComponent<InventorySlotObject>().index;
					int the_slot;
					int the_slot2;
					int the_slot3;
					int companion_level;
					InventoryItem item;
					InventoryItem item2;
					if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container && WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.right)
					{
						if (container_style == container_style_t.companion_pockets)
						{
							ActiveCompanion currSelectedCompanion = CompanionController.Instance.GetCurrSelectedCompanion();
							companion_level = ((currSelectedCompanion == null) ? 1 : currSelectedCompanion.level);
							the_slot = 13;
							the_slot2 = 3;
							the_slot3 = 8;
						}
						else
						{
							the_slot = -1;
							the_slot2 = -1;
							the_slot3 = -1;
							companion_level = -1;
						}
						item = curr_container[GetTrueIndex(page_container, index)].item;
						item2 = curr_container[GetTrueIndex(page_container, num)].item;
					}
					else
					{
						the_slot = hand_index;
						the_slot2 = hat_index;
						the_slot3 = body_index;
						item = player_inventory[GetTrueIndex(page_inventory, index)].item;
						item2 = player_inventory[GetTrueIndex(page_inventory, num)].item;
						companion_level = -1;
					}
					flag2 = TestEquipAllowed(the_slot, inv_type_t.holdable, num, index, item, item2, companion_level) && TestEquipAllowed(the_slot2, inv_type_t.helmet, num, index, item, item2, companion_level) && TestEquipAllowed(the_slot3, inv_type_t.armor, num, index, item, item2, companion_level);
				}
				if (flag2 && mouse_down_slot != gameObject)
				{
					DragOntoSlot(num, gameObject);
				}
				else if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container && WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.right)
				{
					RedrawContainerSlots();
				}
				else
				{
					RedrawInventorySlots();
				}
				CancelDragging();
			}
		}
		if (!test_for_dragging || !(Vector3.Distance(GamepadInput.Instance.GetMousePosition(), initial_press) > (float)drag_threshold))
		{
			return;
		}
		hide_selector();
		int index2 = mouse_down_slot.GetComponent<InventorySlotObject>().index;
		ItemCountPair itemCountPair = null;
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container)
		{
			if (WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.right)
			{
				itemCountPair = curr_container[GetTrueIndex(page_container, index2)];
				mouse_down_slot.GetComponent<InventorySlotObject>().item_sprite.RedrawAsContainer(new InventoryItem(""), 0, index2);
			}
			else
			{
				itemCountPair = player_inventory[GetTrueIndex(page_inventory, index2)];
				mouse_down_slot.GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryNormal("", 0, index2);
			}
		}
		else
		{
			itemCountPair = player_inventory[GetTrueIndex(page_inventory, index2)];
			if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_crafting)
			{
				mouse_down_slot.GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryNormal("", 0, index2);
			}
			else if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_merchant)
			{
				mouse_down_slot.GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventorySellItems(new InventoryItem(""), 0, index2);
			}
		}
		position_drag_at_mouse();
		drag_item_sprite.transform.parent.gameObject.SetActive(true);
		drag_item_sprite.RedrawBasic(itemCountPair.item, itemCountPair.count);
		is_dragging = true;
		test_for_dragging = false;
	}

	private bool TestEquipAllowed(int the_slot, inv_type_t type, int nearest_ind, int drag_from, InventoryItem itemA, InventoryItem itemB, int companion_level)
	{
		if (nearest_ind == the_slot)
		{
			if (GetItemType(itemA) != type)
			{
				return false;
			}
			if (companion_level == -1)
			{
				if (check_req_skill_to_equip(GetItemEquipReqStat(itemA.item_name), GetItemEquipReqLvl(itemA.item_name)))
				{
					return true;
				}
				if (PlayerData.Instance.GetGlobalShort("dev_mode") != 0)
				{
					return true;
				}
				PopupControl.Instance.ShowMessage("Your <color=#66d8ff>" + GetItemEquipReqStat(itemA.item_name) + "</color> skill must be <color=#f44242>" + GetItemEquipReqLvl(itemA.item_name) + "</color> or greater to equip this.", PopupControl.context.message, itemA);
				return false;
			}
			int num = GetItemEquipReqLvl(itemA.item_name) * 3;
			if (num <= companion_level)
			{
				return true;
			}
			if (PlayerData.Instance.GetGlobalShort("dev_mode") != 0)
			{
				return true;
			}
			PopupControl.Instance.ShowMessage("Your companion must be <color=#f44242>Level " + num + "</color> to equip this.", PopupControl.context.message, itemA);
			return false;
		}
		if (drag_from != the_slot)
		{
			return true;
		}
		if (trash_index == nearest_ind || page_1_index == nearest_ind || page_2_index == nearest_ind || page_3_index == nearest_ind)
		{
			return true;
		}
		if (itemB.item_name != "" && GetItemType(itemB) != type)
		{
			return false;
		}
		if (GetItemType(itemB) != type)
		{
			return true;
		}
		if (companion_level == -1)
		{
			if (check_req_skill_to_equip(GetItemEquipReqStat(itemB.item_name), GetItemEquipReqLvl(itemB.item_name)))
			{
				return true;
			}
			PopupControl.Instance.ShowMessage("Your <color=#66d8ff>" + GetItemEquipReqStat(itemA.item_name) + "</color> skill must be <color=#f44242>" + GetItemEquipReqLvl(itemA.item_name) + "</color> or greater to equip this.", PopupControl.context.message, itemA);
			return false;
		}
		int num2 = GetItemEquipReqLvl(itemB.item_name) * 3;
		if (num2 <= companion_level)
		{
			return true;
		}
		PopupControl.Instance.ShowMessage("Your companion must be <color=#f44242>Level " + num2 + "</color> to equip this.", PopupControl.context.message, itemB);
		return false;
	}

	public void CancelDragging()
	{
		is_dragging = false;
		test_for_dragging = false;
		drag_item_sprite.transform.parent.gameObject.SetActive(false);
	}

	private void UpdateLootChestVisual()
	{
		string item_name = GameController.Instance.interacting_element_item.item_name;
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container)
		{
			if (item_name == "Gold Chest" || item_name == "Titanium Chest" || item_name == "Cave Basket" || item_name == "Cave Chest" || item_name == "Loot Basket" || item_name == "Loot Chest" || item_name == "Boss Chest")
			{
				NewRedrawLootRespawnText("loot_spawn", GameController.Instance.interacting_element_item);
			}
			else if (item_name == "Egg Fuser" && GameController.Instance.interacting_element_item.GetShort("started_fusion") == 1)
			{
				RedrawEggFuserText(GameController.Instance.interacting_element_item);
			}
		}
		else if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_merchant)
		{
			NewRedrawLootRespawnText(GameController.Instance.interacting_element_item.GetString("merchant_type"), GameController.Instance.interacting_element_item);
		}
		else if (LandClaimControl.Instance.land_screen_context == LandClaimControl.land_screen_context_t.open_active)
		{
			LandClaimControl.Instance.RedrawTimeRemaining();
		}
		else if (MinigameMenu.Instance != null && MinigameMenu.Instance.menu_CPU_select.activeInHierarchy)
		{
			RedrawMinigameRespawn();
		}
	}

	private void FixedUpdate()
	{
		if (player_inventory.values_edited)
		{
			player_inventory.SaveAllAsInventory();
			player_inventory.values_edited = false;
		}
		if (refresh_loot_counter_i <= 0)
		{
			UpdateLootChestVisual();
			refresh_loot_counter_i = 50;
		}
		refresh_loot_counter_i--;
		if (is_dragging)
		{
			position_drag_at_mouse();
		}
	}

	private void position_drag_at_mouse()
	{
		Vector3 mousePosition = GamepadInput.Instance.GetMousePosition();
		drag_item_sprite.transform.parent.localPosition = new Vector3((mousePosition.x - (float)Screen.width * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor, (mousePosition.y - (float)Screen.height * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor, 0f);
	}

	public List<int> GetEmptyNonEquipmentInventorySlots(BasketContents contents)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < n_slots_per_page_; i++)
		{
			list.Add(i);
		}
		if (PlayerData.Instance.GetGlobalShort("big_inv") == 1)
		{
			for (int j = p2_begin_; j < p2_begin_ + n_slots_per_page_; j++)
			{
				list.Add(j);
			}
		}
		foreach (int item in contents.FilledSlots())
		{
			list.Remove(item);
		}
		return list;
	}

	public int HowManyCanAfford(InventoryItem item)
	{
		int buyPrice = MerchantControl.Instance.GetBuyPrice(item);
		if (buyPrice == 0)
		{
			return 999;
		}
		return (int)((float)GetPlayerItemCount("Coins") / (float)buyPrice);
	}

	public int HowManyCanReceive(string item_name)
	{
		BasketContents basketContents = ClonePlayerInventory();
		int itemMaxStack = GetItemMaxStack(item_name);
		int num = 0;
		foreach (int item in basketContents.FilledSlots())
		{
			if (basketContents[item].item.item_name == item_name)
			{
				num += itemMaxStack - basketContents[item].count;
			}
		}
		return num + GetEmptyNonEquipmentInventorySlots(basketContents).Count * itemMaxStack;
	}

	public bool CanReceiveItem(string item_name, int n_give, bool show_full_notif = false, BasketContents hypothetical_inventory = null, bool add_to_crafting_mixers = false)
	{
		return CanReceiveItem(new InventoryItem(item_name), n_give, show_full_notif, hypothetical_inventory, add_to_crafting_mixers);
	}

	public bool CanReceiveItem(InventoryItem item, int n_give, bool show_full_notif = false, BasketContents hypothetical_inventory = null, bool add_to_crafting_mixers = false)
	{
		if (hypothetical_inventory == null)
		{
			hypothetical_inventory = ClonePlayerInventory();
		}
		int itemMaxStack = GetItemMaxStack(item.item_name);
		foreach (int item2 in hypothetical_inventory.FilledSlots())
		{
			if (hypothetical_inventory[item2].item == item)
			{
				n_give -= itemMaxStack - hypothetical_inventory[item2].count;
				if (n_give <= 0)
				{
					if (add_to_crafting_mixers)
					{
						crafting_mixer_slots.Add(new craft_mixer_slot
						{
							index = item2
						});
					}
					return true;
				}
			}
		}
		foreach (int emptyNonEquipmentInventorySlot in GetEmptyNonEquipmentInventorySlots(hypothetical_inventory))
		{
			n_give -= itemMaxStack;
			if (n_give <= 0)
			{
				if (add_to_crafting_mixers)
				{
					crafting_mixer_slots.Add(new craft_mixer_slot
					{
						index = emptyNonEquipmentInventorySlot
					});
				}
				return true;
			}
		}
		if (show_full_notif)
		{
			GameController.Instance.showOverheadNotif(TranslationControl.Instance.TranslateGeneral("Inventory Full!", "GUI"), GameController.Instance.player.transform.position, false, true);
		}
		return false;
	}

	public void GiveItem(string item_name, int count, string fn_validator, bool visual = true)
	{
		GiveItem(new InventoryItem(item_name), count, fn_validator, visual);
	}

	public void GiveItem(InventoryItem item, int count, string fn_validator, bool visual = true)
	{
		if (ToInventory(item, count, ptype.eitherPage) == 0)
		{
			if (visual)
			{
				GameController.Instance.showOverheadNotif("+" + count + " " + GetFullItemName(item), GameController.Instance.player.transform.position, true, true);
			}
		}
		else if (visual)
		{
			GameController.Instance.showOverheadNotif(TranslationControl.Instance.TranslateGeneral("Inventory Full!", "GUI"), GameController.Instance.player.transform.position, false, true);
		}
		if ((WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_crafting || WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container || WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_merchant) && WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.left)
		{
			RedrawInventorySlots();
		}
	}

	private void create_buttons_and_slots()
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(inv_slot_type);
				gameObject.transform.SetParent(for_slots);
				gameObject.transform.localPosition = new Vector3(INV_start_slots.x + INV_slot_spacing * (float)j, INV_start_slots.y - INV_slot_spacing * (float)i, 0f);
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.localRotation = Quaternion.identity;
				instantiated_inv_slots.Add(gameObject);
				gameObject.GetComponent<InventorySlotObject>().index = i * 5 + j;
			}
		}
		instantiated_inv_slots.Add(equip_hand_slot);
		instantiated_inv_slots.Add(equip_hat_slot);
		instantiated_inv_slots.Add(equip_body_slot);
		for (int k = 0; k < 3; k++)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(craft_slot_type);
			gameObject2.transform.SetParent(craft_slot_parent);
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
			gameObject2.transform.localPosition = CRAFT_start_slots + Vector2.right * CRAFT_spacing * k;
			gameObject2.GetComponent<CraftingSlot>().index = k;
			instantiated_crafting_slots.Add(gameObject2.GetComponent<CraftingSlot>());
		}
		drag_item_sprite.transform.parent.SetAsLastSibling();
		angular.transform.SetAsLastSibling();
	}

	public void OpenInventoryAndCrafting(new_craft_list list, InventoryItem item)
	{
		OpenInventoryAndCrafting(WindowControl.tab.right, list, TranslationControl.Instance.TranslateItemName(item.item_name));
	}

	public void OpenInventoryAndCrafting(WindowControl.tab initial_tab, new_craft_list list, string non_inv_tab_name, string inv_tab_name = "INVENTORY")
	{
		if (!WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.inventory_and_crafting))
		{
			return;
		}
		curr_crafting_list = list;
		WindowControl.Instance.miniwindow_header_L.text = TranslationControl.Instance.TranslateGeneral(inv_tab_name, "GUI").ToUpper();
		WindowControl.Instance.miniwindow_header_R.text = TranslationControl.Instance.TranslateGeneral(non_inv_tab_name, "GUI").ToUpper();
		switch (initial_tab)
		{
		case WindowControl.tab.right:
			WindowControl.Instance.VisuallySelectRightMiniwindowTab();
			OnRightMiniwindowTabClicked();
			break;
		case WindowControl.tab.left:
			WindowControl.Instance.VisuallySelectLeftMiniwindowTab();
			OnLeftMiniwindowTabClicked();
			break;
		}
	}

	public void OpenInventoryAndContainer(string non_inv_tab_name)
	{
		if (WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.inventory_and_container))
		{
			curr_crafting_list = null;
			WindowControl.Instance.miniwindow_header_L.text = TranslationControl.Instance.TranslateGeneral("INVENTORY", "GUI").ToUpper();
			WindowControl.Instance.miniwindow_header_R.text = TranslationControl.Instance.TranslateGeneral(non_inv_tab_name, "GUI").ToUpper();
			WindowControl.Instance.VisuallySelectRightMiniwindowTab();
			OnRightMiniwindowTabClicked();
		}
	}

	public void OpenInventoryAndMerchant(WindowControl.tab initial_tab, new_craft_list list)
	{
		if (!WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.inventory_and_merchant))
		{
			return;
		}
		curr_crafting_list = list;
		WindowControl.Instance.miniwindow_header_L.text = TranslationControl.Instance.TranslateGeneral("Sell Items", "GUI").ToUpper();
		WindowControl.Instance.miniwindow_header_R.text = TranslationControl.Instance.TranslateGeneral("Buy Items", "GUI").ToUpper();
		switch (initial_tab)
		{
		case WindowControl.tab.right:
			WindowControl.Instance.VisuallySelectRightMiniwindowTab();
			OnRightMiniwindowTabClicked();
			break;
		case WindowControl.tab.left:
			WindowControl.Instance.VisuallySelectLeftMiniwindowTab();
			OnLeftMiniwindowTabClicked();
			break;
		}
	}

	public void OnLeftMiniwindowTabClicked()
	{
		if (angular_animation_playing || is_dragging)
		{
			return;
		}
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container && GameController.Instance.interacting_element_item.item_name == "Trading Table")
		{
			WindowPrefabsControl.Instance.GetScreen("TradingTable").SetActive(false);
		}
		LayOutInvSlots(false, true, slots_positionings.show_15_left, true, NumPlayerPages(), false, "", fusion_button.hide);
		RedrawInventorySlots();
		hide_selector();
	}

	public void OnRightMiniwindowTabClicked()
	{
		if (!angular_animation_playing && !is_dragging)
		{
			goto_crafting_tab((WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_crafting || WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_merchant) && curr_crafting_list != null ? curr_crafting_list.saved_page : 0);
		}
	}

	public void EquipmentSlotsSetActive(bool val)
	{
		equip_hand_slot.SetActive(val);
		equip_hat_slot.SetActive(val);
		equip_body_slot.SetActive(val);
	}

	public void open_buy()
	{
		open_merchant_window(WindowControl.tab.right);
	}

	public void open_sell()
	{
		open_merchant_window(WindowControl.tab.left);
	}

	private void open_merchant_window(WindowControl.tab tab)
	{
		new_craft_list sellList = MerchantControl.Instance.GetSellList();
		curr_buyback_list = MerchantControl.Instance.GetAllBuybackItems();
		OpenInventoryAndMerchant(tab, sellList);
	}

	public void press_inv_button()
	{
		OpenInventoryAndCrafting(WindowControl.tab.left, GetCraftList("Crafting - Default"), "Crafting");
	}

	public new_craft_list GetCraftList(string craft_list_file_name)
	{
		if (loaded_craft_lists.ContainsKey(craft_list_file_name))
		{
			return loaded_craft_lists[craft_list_file_name];
		}
		new_craft_list new_craft_list = new new_craft_list();
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("Lists/" + craft_list_file_name, ref file_exists);
		if (file_exists)
		{
			List<string> list = new List<string>();
			foreach (string item in textFileLines)
			{
				if (!Startup.StringNullOrWhitespace(item))
				{
					list.Add(item);
				}
			}
			new_craft_list.items = new ItemCountPair[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				new_craft_list.items[i] = new ItemCountPair(new InventoryItem(list[i]), GetItem_nCraft(list[i]));
			}
		}
		loaded_craft_lists.Add(craft_list_file_name, new_craft_list);
		return new_craft_list;
	}

	public List<int> GetPermittedContainerSlots(container_style_t style, string item_name, ptype page)
	{
		List<int> list = new List<int>();
		if (container_style == container_style_t.trading_table || container_style == container_style_t.companion_pockets)
		{
			list.Add(0);
			list.Add(1);
			list.Add(2);
			list.Add(5);
			list.Add(6);
			list.Add(7);
			list.Add(10);
			list.Add(11);
			list.Add(12);
		}
		else if (container_style == container_style_t.world_container)
		{
			switch (item_name)
			{
			case "Loot Basket":
			case "Titanium Chest":
			case "Cave Chest":
			case "Loot Chest":
			case "Basket":
			case "Sky Chest":
			case "Gold Chest":
			case "Cave Basket":
				list.Add(0);
				list.Add(1);
				list.Add(2);
				list.Add(5);
				list.Add(6);
				list.Add(7);
				list.Add(10);
				list.Add(11);
				list.Add(12);
				break;
			case "Crate":
				if (page == ptype.firstPage || page == ptype.eitherPage)
				{
					for (int k = 0; k < n_slots_per_page_; k++)
					{
						list.Add(k);
					}
				}
				if (page == ptype.secondPage || page == ptype.eitherPage)
				{
					for (int l = p2_begin_; l < p2_begin_ + n_slots_per_page_; l++)
					{
						list.Add(l);
					}
				}
				break;
			case "Double Crate":
				if (page == ptype.firstPage || page == ptype.eitherPage)
				{
					for (int m = 0; m < n_slots_per_page_; m++)
					{
						list.Add(m);
					}
				}
				if (page == ptype.secondPage || page == ptype.eitherPage)
				{
					for (int n = p2_begin_; n < p2_begin_ + n_slots_per_page_; n++)
					{
						list.Add(n);
					}
				}
				if (page == ptype.thirdPage || page == ptype.eitherPage)
				{
					for (int num = p2_begin_ + n_slots_per_page_; num < p2_begin_ + n_slots_per_page_ * 2; num++)
					{
						list.Add(num);
					}
				}
				break;
			case "Egg Fuser":
				list.Add(5);
				list.Add(7);
				list.Add(11);
				break;
			case "Custom Statue":
				list.Add(5);
				list.Add(6);
				list.Add(7);
				break;
			case "Armor Display":
			case "Large Weapon Display":
				list.Add(5);
				list.Add(7);
				break;
			case "Weapon Display":
				list.Add(6);
				break;
			default:
				for (int j = 0; j < n_slots_per_page_; j++)
				{
					list.Add(j);
				}
				break;
			}
		}
		return list;
	}

	private int ToContainer(InventoryItem item, int curr_count, ptype page_type)
	{
		int itemMaxStack = GetItemMaxStack(item.item_name);
		List<int> list;
		if (item.GetString("interaction_type") == "chest")
		{
			list = new List<int>();
			for (int i = 0; i < n_slots_per_page_; i++)
			{
				list.Add(i);
			}
		}
		else
		{
			list = GetPermittedContainerSlots(container_style_t.world_container, GameController.Instance.interacting_element_item.item_name, page_type);
		}
		if (itemMaxStack > 1)
		{
			for (int j = 0; j < list.Count; j++)
			{
				int index = list[j];
				if (curr_container[index].item == item && curr_container[index].count != itemMaxStack)
				{
					int num = itemMaxStack - curr_container[index].count;
					if (curr_count <= num)
					{
						curr_container[index] = new ItemCountPair(curr_container[index].item, curr_container[index].count + curr_count);
						return 0;
					}
					curr_container[index] = new ItemCountPair(curr_container[index].item, itemMaxStack);
					curr_count -= num;
				}
			}
		}
		if (curr_count != 0)
		{
			for (int k = 0; k < list.Count; k++)
			{
				int index2 = list[k];
				if (Startup.StringNullOrWhitespace(curr_container[index2].item.item_name))
				{
					if (curr_count <= itemMaxStack)
					{
						curr_container[index2] = new ItemCountPair(item, curr_count);
						return 0;
					}
					curr_container[index2] = new ItemCountPair(item, itemMaxStack);
					curr_count -= itemMaxStack;
				}
			}
		}
		return curr_count;
	}

	private int ToInventory(InventoryItem item, int curr_count, ptype page_type)
	{
		if (page_type == ptype.eitherPage || page_type == ptype.firstPage)
		{
			int num = AddToPreExistingInventoryStacks(item, curr_count, 0, n_slots_per_page_);
			if (num == 0)
			{
				return 0;
			}
			curr_count = AddToEmptyInventoryStacks(item, num, 0, n_slots_per_page_);
		}
		if ((page_type == ptype.secondPage || page_type == ptype.eitherPage) && curr_count != 0 && PlayerData.Instance.GetGlobalShort("big_inv") == 1)
		{
			curr_count = AddToPreExistingInventoryStacks(item, curr_count, p2_begin_, n_slots_per_page_ + p2_begin_);
			if (curr_count != 0)
			{
				return AddToEmptyInventoryStacks(item, curr_count, p2_begin_, n_slots_per_page_ + p2_begin_);
			}
		}
		return curr_count;
	}

	private int AddToPreExistingInventoryStacks(InventoryItem item, int curr_count, int startIndex, int endIndex)
	{
		int itemMaxStack = GetItemMaxStack(item.item_name);
		if (itemMaxStack > 1)
		{
			for (int i = startIndex; i < endIndex; i++)
			{
				if (player_inventory[i].item == item && player_inventory[i].count != itemMaxStack)
				{
					int num = itemMaxStack - player_inventory[i].count;
					if (curr_count <= num)
					{
						player_inventory[i] = new ItemCountPair(player_inventory[i].item, player_inventory[i].count + curr_count);
						return 0;
					}
					player_inventory[i] = new ItemCountPair(player_inventory[i].item, itemMaxStack);
					curr_count -= num;
				}
			}
		}
		return curr_count;
	}

	private int AddToEmptyInventoryStacks(InventoryItem item, int curr_count, int startIndex, int endIndex)
	{
		int itemMaxStack = GetItemMaxStack(item.item_name);
		for (int i = startIndex; i < endIndex; i++)
		{
			if (player_inventory[i].item.item_name == "")
			{
				if (curr_count <= itemMaxStack)
				{
					player_inventory[i] = new ItemCountPair(item, curr_count);
					return 0;
				}
				player_inventory[i] = new ItemCountPair(item, itemMaxStack);
				curr_count -= itemMaxStack;
			}
		}
		return curr_count;
	}

	public void inv_page_switch(int new_page_id)
	{
		AudioControl.Instance.PlayGenericClick();
		hide_selector();
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container && WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.right)
		{
			page_container = new_page_id;
			RedrawContainerSlots();
		}
		else
		{
			page_inventory = new_page_id;
			RedrawInventorySlots();
		}
		RedrawPageSwitchers(new_page_id);
	}

	private void RedrawPageSwitchers(int new_page_id)
	{
		switch (new_page_id)
		{
		case 1:
			invp1.color = WindowControl.Instance.GetMiniwindowLayout("inventory").header_sel_col;
			invp2.color = WindowControl.Instance.GetMiniwindowLayout("inventory").header_desel_col;
			invp3.color = WindowControl.Instance.GetMiniwindowLayout("inventory").header_desel_col;
			break;
		case 2:
			invp1.color = WindowControl.Instance.GetMiniwindowLayout("inventory").header_desel_col;
			invp2.color = WindowControl.Instance.GetMiniwindowLayout("inventory").header_sel_col;
			invp3.color = WindowControl.Instance.GetMiniwindowLayout("inventory").header_desel_col;
			break;
		case 3:
			invp1.color = WindowControl.Instance.GetMiniwindowLayout("inventory").header_desel_col;
			invp2.color = WindowControl.Instance.GetMiniwindowLayout("inventory").header_desel_col;
			invp3.color = WindowControl.Instance.GetMiniwindowLayout("inventory").header_sel_col;
			break;
		}
	}

	public void RedrawInventorySlots()
	{
		string text = "";
		if (MusicBoxControl.Instance.save_screen_open)
		{
			text = "Blank Record";
		}
		else if (MusicBoxControl.Instance.load_screen_open)
		{
			text = "Saved Record";
		}
		else if (PaintingControl.Instance != null)
		{
			if (PaintingControl.Instance.save_screen_open)
			{
				text = "Blank Canvas";
			}
			else if (PaintingControl.Instance.load_screen_open)
			{
				text = "Painting";
			}
		}
		if (text != "")
		{
			for (int i = 0; i < n_slots_per_page_; i++)
			{
				int trueIndex = GetTrueIndex(page_inventory, i);
				instantiated_inv_slots[i].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryAndHighlightItem(player_inventory[trueIndex].item, player_inventory[trueIndex].count, i, text);
			}
			instantiated_inv_slots[hand_index].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryAndHighlightItem(player_inventory[hand_index].item, player_inventory[hand_index].count, hand_index, text);
			instantiated_inv_slots[hat_index].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryAndHighlightItem(player_inventory[hat_index].item, player_inventory[hat_index].count, hat_index, text);
			instantiated_inv_slots[body_index].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryAndHighlightItem(player_inventory[body_index].item, player_inventory[body_index].count, body_index, text);
		}
		else if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_merchant)
		{
			for (int j = 0; j < n_slots_per_page_; j++)
			{
				int trueIndex2 = GetTrueIndex(page_inventory, j);
				instantiated_inv_slots[j].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventorySellItems(player_inventory[trueIndex2].item, player_inventory[trueIndex2].count, j);
			}
			instantiated_inv_slots[hand_index].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventorySellItems(player_inventory[hand_index].item, player_inventory[hand_index].count, hand_index);
			instantiated_inv_slots[hat_index].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventorySellItems(player_inventory[hat_index].item, player_inventory[hat_index].count, hat_index);
			instantiated_inv_slots[body_index].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventorySellItems(player_inventory[body_index].item, player_inventory[body_index].count, body_index);
		}
		else
		{
			for (int k = 0; k < n_slots_per_page_; k++)
			{
				int trueIndex3 = GetTrueIndex(page_inventory, k);
				instantiated_inv_slots[k].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryNormal(player_inventory[trueIndex3].item, player_inventory[trueIndex3].count, k);
			}
			instantiated_inv_slots[hand_index].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryNormal(player_inventory[hand_index].item, player_inventory[hand_index].count, hand_index);
			instantiated_inv_slots[hat_index].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryNormal(player_inventory[hat_index].item, player_inventory[hat_index].count, hat_index);
			instantiated_inv_slots[body_index].GetComponent<InventorySlotObject>().item_sprite.RedrawAsInventoryNormal(player_inventory[body_index].item, player_inventory[body_index].count, body_index);
		}
	}

	public void RedrawContainerSlots()
	{
		bool flag = false;
		if (GameController.Instance.interacting_element_item.item_name == "Egg Fuser" && GameController.Instance.interacting_element_item.GetShort("started_fusion") == 1)
		{
			flag = GameController.Instance.interacting_element_item.HasActiveRespawn("fuser_spawn");
		}
		for (int i = 0; i < n_slots_per_page_; i++)
		{
			int trueIndex = GetTrueIndex(page_container, i);
			if (flag && (i == 5 || i == 7 || i == 11))
			{
				instantiated_inv_slots[i].GetComponent<InventorySlotObject>().item_sprite.RedrawAsContainerGreen(curr_container[i].item, curr_container[i].count, i);
			}
			else
			{
				instantiated_inv_slots[i].GetComponent<InventorySlotObject>().item_sprite.RedrawAsContainer(curr_container[trueIndex].item, curr_container[trueIndex].count, i);
			}
		}
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
		if (PlayerData.Instance.GetGlobalShort("big_inv") == 1)
		{
			return 2;
		}
		return 1;
	}

	public void goto_findBlankRecord()
	{
		ShowInventoryTab(false);
	}

	public void PressStartEggFusion()
	{
		if (GameController.Instance.interacting_element_item.GetShort("started_fusion") != 1)
		{
			ExtraInventoryData extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
			extraDataCopy.SetShort("started_fusion", 1);
			InventoryItem old_item = new InventoryItem("Egg Fuser", extraDataCopy);
			DateTime uTC_when = DateTime.UtcNow.AddSeconds(0.0).AddMinutes(15.0);
			ConstructionControl.Instance.PlayerReplaceInteracting(ChunkControl.Instance.EncodeRespawnIntoItem("fuser_spawn", old_item, uTC_when), true);
			LayOutEggFuser();
			RedrawContainerSlots();
			return;
		}
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
		if (CompanionController.Instance.active_companions.Count < 2)
		{
			CompanionController.Instance.CreateAnimatedEgg(3, false, curr_container[5].item.GetString("egg_monster"), curr_container[7].item.GetString("egg_monster"));
			ExtraInventoryData extraDataCopy2 = GameController.Instance.interacting_element_item.GetExtraDataCopy();
			extraDataCopy2.SetString("has_respawn_fuser_spawn", "");
			extraDataCopy2.SetShort("started_fusion", 0);
			ConstructionControl.Instance.PlayerReplaceInteracting(new InventoryItem("Egg Fuser", extraDataCopy2), true);
			curr_container = new BasketContents();
			WindowControl.Instance.CloseMiniwindow(false);
		}
		else
		{
			PopupControl.Instance.ShowMessage("You can only have 2 followers at a time!", PopupControl.context.message);
		}
	}

	public void jump_to_page(int page)
	{
		craft_PAGE = page;
		bool active = page_exists(1);
		if (page == 0)
		{
			crafting_page_left.SetActive(false);
			crafting_page_right.SetActive(true);
		}
		else
		{
			crafting_page_left.SetActive(true);
			crafting_page_right.SetActive(active);
		}
		RedrawCraftingSlotButtons();
	}

	public void ShowRespawnTimer(string prefix)
	{
		gui_loot_respawn_time.SetActive(true);
		gui_loot_respawn_time.transform.Find("Prefix").GetComponent<Text>().text = prefix + ":";
	}

	public void HideRespawnTimer()
	{
		gui_loot_respawn_time.SetActive(false);
	}

	public void goto_crafting_tab(int start_page)
	{
		pageSwitchers.SetActive(false);
		hide_selector();
		WindowControl.miniwindow_type_t curr_miniwindow = WindowControl.Instance.curr_miniwindow;
		if (curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container)
		{
			if (container_style == container_style_t.trading_table)
			{
				LayOutInvSlots(false, false, slots_positionings.show_9_tradingTable, false, 1, false, "", fusion_button.hide);
				GameObject screen = WindowPrefabsControl.Instance.GetScreen("TradingTable");
				if (screen != null)
				{
					screen.SetActive(true);
				}
				else
				{
					TradingTableControl.Instance = WindowPrefabsControl.Instance.CreateScreen("TradingTable", WindowPrefabsControl.build_into_t.mini_window).GetComponent<TradingTableControl>();
				}
			}
			else if (container_style == container_style_t.companion_pockets)
			{
				LayOutInvSlots(false, false, slots_positionings.show_12, false, 1, false, "", fusion_button.hide);
			}
			else if (container_style == container_style_t.world_container)
			{
				string item_name = GameController.Instance.interacting_element_item.item_name;
				if (item_name == "Basket")
				{
					LayOutInvSlots(false, false, slots_positionings.show_9, false, 1, false, "", fusion_button.hide);
				}
				else if (item_name == "Gold Chest" || item_name == "Titanium Chest" || item_name == "Cave Basket" || item_name == "Cave Chest" || item_name == "Loot Basket" || item_name == "Loot Chest")
				{
					LayOutInvSlots(true, false, slots_positionings.show_9, false, 1, false, "", fusion_button.hide);
				}
				else if (item_name == "Boss Chest")
				{
					LayOutInvSlots(true, false, slots_positionings.show_15_centered, false, 1, false, "", fusion_button.hide);
				}
				else if (item_name == "Egg Fuser")
				{
					LayOutEggFuser();
				}
				else if (item_name == "Weapon Display")
				{
					LayOutInvSlots(false, false, slots_positionings.show_1_wepDisplay, false, 1, false, "Insert a weapon to display it", fusion_button.hide);
				}
				else if (item_name == "Large Weapon Display")
				{
					LayOutInvSlots(false, false, slots_positionings.show_2, false, 1, false, "Insert up to 2 weapons\nto display", fusion_button.hide);
				}
				else if (item_name == "Armor Display")
				{
					LayOutInvSlots(false, false, slots_positionings.show_2, false, 1, true, "Insert hats or clothes\nfor the display", fusion_button.hide);
				}
				else if (item_name == "Custom Statue")
				{
					LayOutInvSlots(false, false, slots_positionings.show_3_statue, false, 1, true, "Insert hats, clothes, or weapons\nfor the statue", fusion_button.hide);
				}
				else
				{
					int n_pages = 1;
					if (item_name == "Crate")
					{
						n_pages = 2;
					}
					else if (item_name == "Double Crate")
					{
						n_pages = 3;
					}
					LayOutInvSlots(false, false, slots_positionings.show_15_centered, false, n_pages, false, "", fusion_button.hide);
				}
			}
			RedrawContainerSlots();
		}
		else if (curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_crafting || curr_miniwindow == WindowControl.miniwindow_type_t.quests_and_achieves || curr_miniwindow == WindowControl.miniwindow_type_t.teleport || curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_merchant)
		{
			craft_PAGE = start_page;
			crafting_page_left.SetActive(page_exists(-1));
			crafting_page_right.SetActive(page_exists(1));
			background_strip_layout background_strip_layout_;
			if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_merchant)
			{
				ShowRespawnTimer(TranslationControl.Instance.TranslateGeneral("Different items in", "GUI"));
				background_strip_layout_ = background_strip_layout.higher;
			}
			else
			{
				HideRespawnTimer();
				background_strip_layout_ = background_strip_layout.normal;
			}
			HideInventoryTab(false);
			crafting_Tab.SetActive(true);
			LayOutCraftingTab(background_strip_layout_);
			RedrawCraftingSlotButtons();
		}
	}

	public void LayOutInvSlots(bool show_respawn_timer, bool show_trash_bin, slots_positionings slot_positioning, bool show_equipment_slots, int n_pages, bool show_advanced_button, string fuser_text, fusion_button fusion_button_layout, string page1_text = "PAGE 1", string page2_text = "PAGE 2")
	{
		if (show_respawn_timer)
		{
			ShowRespawnTimer(TranslationControl.Instance.TranslateGeneral("Loot respawns in", "GUI"));
		}
		else
		{
			HideRespawnTimer();
		}
		trash_bin.SetActive(show_trash_bin);
		switch (slot_positioning)
		{
		case slots_positionings.show_15_centered:
			slot_parent.transform.localPosition = new Vector3(-5f, -16f, 0f);
			for_slots.transform.localPosition = Vector2.zero;
			((RectTransform)slot_parent.transform).sizeDelta = new Vector2(981f, 604f);
			slot_parent.GetComponent<Image>().color = col_background_normal;
			slot_parent.transform.localScale = Vector3.one;
			break;
		case slots_positionings.show_15_left:
			slot_parent.transform.localPosition = standard_slots_position;
			for_slots.transform.localPosition = Vector2.zero;
			((RectTransform)slot_parent.transform).sizeDelta = new Vector2(981f, 604f);
			slot_parent.GetComponent<Image>().color = col_background_normal;
			slot_parent.transform.localScale = Vector3.one;
			break;
		case slots_positionings.show_12:
		case slots_positionings.show_9:
		case slots_positionings.show_3_statue:
		case slots_positionings.show_2:
		case slots_positionings.show_1_wepDisplay:
			slot_parent.transform.localPosition = new Vector3(0f, -16f, 0f);
			for_slots.transform.localPosition = new Vector3(187f, 0f, 0f);
			((RectTransform)slot_parent.transform).sizeDelta = new Vector2(614f, 604f);
			slot_parent.GetComponent<Image>().color = col_background_normal;
			slot_parent.transform.localScale = Vector3.one;
			break;
		case slots_positionings.show_9_tradingTable:
			slot_parent.transform.localPosition = new Vector3(-316f, -33.4f, 0f);
			for_slots.transform.localPosition = new Vector3(187f, 0f, 0f);
			((RectTransform)slot_parent.transform).sizeDelta = new Vector2(614f, 604f);
			slot_parent.GetComponent<Image>().color = col_background_normal;
			slot_parent.transform.localScale = Vector3.one * 0.875f;
			break;
		case slots_positionings.show_3_fuser:
		case slots_positionings.show_1_fuser:
			slot_parent.transform.localPosition = new Vector3(0f, -16f, 0f);
			for_slots.transform.localPosition = new Vector3(187f, 0f, 0f);
			((RectTransform)slot_parent.transform).sizeDelta = new Vector2(614f, 604f);
			slot_parent.GetComponent<Image>().color = col_background_fuser;
			slot_parent.transform.localScale = Vector3.one;
			break;
		}
		for (int i = 0; i < n_slots_per_page_; i++)
		{
			instantiated_inv_slots[i].SetActive(true);
		}
		switch (slot_positioning)
		{
		case slots_positionings.show_12:
			instantiated_inv_slots[4].SetActive(false);
			instantiated_inv_slots[9].SetActive(false);
			instantiated_inv_slots[14].SetActive(false);
			break;
		case slots_positionings.show_9:
		case slots_positionings.show_9_tradingTable:
			instantiated_inv_slots[4].SetActive(false);
			instantiated_inv_slots[9].SetActive(false);
			instantiated_inv_slots[14].SetActive(false);
			instantiated_inv_slots[3].SetActive(false);
			instantiated_inv_slots[8].SetActive(false);
			instantiated_inv_slots[13].SetActive(false);
			break;
		case slots_positionings.show_3_fuser:
			instantiated_inv_slots[4].SetActive(false);
			instantiated_inv_slots[9].SetActive(false);
			instantiated_inv_slots[14].SetActive(false);
			instantiated_inv_slots[3].SetActive(false);
			instantiated_inv_slots[8].SetActive(false);
			instantiated_inv_slots[13].SetActive(false);
			instantiated_inv_slots[0].SetActive(false);
			instantiated_inv_slots[1].SetActive(false);
			instantiated_inv_slots[2].SetActive(false);
			instantiated_inv_slots[10].SetActive(false);
			instantiated_inv_slots[12].SetActive(false);
			instantiated_inv_slots[6].SetActive(false);
			break;
		case slots_positionings.show_3_statue:
			instantiated_inv_slots[4].SetActive(false);
			instantiated_inv_slots[9].SetActive(false);
			instantiated_inv_slots[14].SetActive(false);
			instantiated_inv_slots[3].SetActive(false);
			instantiated_inv_slots[8].SetActive(false);
			instantiated_inv_slots[13].SetActive(false);
			instantiated_inv_slots[0].SetActive(false);
			instantiated_inv_slots[1].SetActive(false);
			instantiated_inv_slots[2].SetActive(false);
			instantiated_inv_slots[10].SetActive(false);
			instantiated_inv_slots[12].SetActive(false);
			instantiated_inv_slots[11].SetActive(false);
			break;
		case slots_positionings.show_2:
			instantiated_inv_slots[4].SetActive(false);
			instantiated_inv_slots[9].SetActive(false);
			instantiated_inv_slots[14].SetActive(false);
			instantiated_inv_slots[3].SetActive(false);
			instantiated_inv_slots[8].SetActive(false);
			instantiated_inv_slots[13].SetActive(false);
			instantiated_inv_slots[0].SetActive(false);
			instantiated_inv_slots[1].SetActive(false);
			instantiated_inv_slots[2].SetActive(false);
			instantiated_inv_slots[10].SetActive(false);
			instantiated_inv_slots[12].SetActive(false);
			instantiated_inv_slots[6].SetActive(false);
			instantiated_inv_slots[11].SetActive(false);
			break;
		case slots_positionings.show_1_fuser:
			instantiated_inv_slots[4].SetActive(false);
			instantiated_inv_slots[9].SetActive(false);
			instantiated_inv_slots[14].SetActive(false);
			instantiated_inv_slots[3].SetActive(false);
			instantiated_inv_slots[8].SetActive(false);
			instantiated_inv_slots[13].SetActive(false);
			instantiated_inv_slots[0].SetActive(false);
			instantiated_inv_slots[1].SetActive(false);
			instantiated_inv_slots[2].SetActive(false);
			instantiated_inv_slots[10].SetActive(false);
			instantiated_inv_slots[12].SetActive(false);
			instantiated_inv_slots[5].SetActive(false);
			instantiated_inv_slots[7].SetActive(false);
			instantiated_inv_slots[6].SetActive(false);
			break;
		case slots_positionings.show_1_wepDisplay:
			instantiated_inv_slots[4].SetActive(false);
			instantiated_inv_slots[9].SetActive(false);
			instantiated_inv_slots[14].SetActive(false);
			instantiated_inv_slots[3].SetActive(false);
			instantiated_inv_slots[8].SetActive(false);
			instantiated_inv_slots[13].SetActive(false);
			instantiated_inv_slots[0].SetActive(false);
			instantiated_inv_slots[1].SetActive(false);
			instantiated_inv_slots[2].SetActive(false);
			instantiated_inv_slots[10].SetActive(false);
			instantiated_inv_slots[12].SetActive(false);
			instantiated_inv_slots[5].SetActive(false);
			instantiated_inv_slots[7].SetActive(false);
			instantiated_inv_slots[11].SetActive(false);
			break;
		}
		EquipmentSlotsSetActive(show_equipment_slots);
		ShowInventoryTab(false);
		HideCraftingTab();
		if (n_pages < 2)
		{
			pageSwitchers.SetActive(false);
		}
		else if (n_pages == 3)
		{
			pageSwitchers.SetActive(true);
			invPAGE3.SetActive(true);
			pageSwitchers.transform.localPosition = Vector2.left * 152f;
		}
		else if (n_pages == 2)
		{
			pageSwitchers.SetActive(true);
			invPAGE3.SetActive(false);
			pageSwitchers.transform.localPosition = Vector2.zero;
			text_page1.text = page1_text;
			text_page2.text = page2_text;
		}
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container && WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.right)
		{
			RedrawPageSwitchers(page_container);
		}
		else
		{
			RedrawPageSwitchers(page_inventory);
		}
		advanced_button.SetActive(show_advanced_button);
		if (fuser_text != "")
		{
			egg_fuser_text.SetActive(true);
			egg_fuser_text.GetComponent<Text>().text = fuser_text;
		}
		else
		{
			egg_fuser_text.SetActive(false);
		}
		switch (fusion_button_layout)
		{
		case fusion_button.hatch:
			start_fuser_button.SetActive(true);
			txt_fuser_startbutton.text = "HATCH EGG";
			txt_fuser_startbutton.color = col_fuser_hatchbutton;
			start_fuser_button.GetComponent<Image>().sprite = spr_fuser_hatchbutton;
			break;
		case fusion_button.start:
			start_fuser_button.SetActive(true);
			txt_fuser_startbutton.text = "START FUSION";
			txt_fuser_startbutton.color = col_fuser_startbutton;
			start_fuser_button.GetComponent<Image>().sprite = spr_fuser_startbutton;
			break;
		case fusion_button.hide:
			start_fuser_button.SetActive(false);
			break;
		}
	}

	public void RedrawCraftingSlotButtons()
	{
		for (int i = 0; i < 3; i++)
		{
			int num = i + craft_PAGE * 3;
			switch (WindowControl.Instance.curr_miniwindow)
			{
			case WindowControl.miniwindow_type_t.inventory_and_crafting:
				if (num < curr_crafting_list.items.Length)
				{
					instantiated_crafting_slots[i].gameObject.SetActive(true);
					DrawCraftingSlotPart1(i, num);
				}
				else
				{
					instantiated_crafting_slots[i].gameObject.SetActive(false);
				}
				break;
			case WindowControl.miniwindow_type_t.quests_and_achieves:
				if (num < AchievesControl.Instance.all_achivements.Length)
				{
					instantiated_crafting_slots[i].gameObject.SetActive(true);
					AchievesControl.Instance.DrawAchievementSlot(i, num);
				}
				else
				{
					instantiated_crafting_slots[i].gameObject.SetActive(false);
				}
				break;
			case WindowControl.miniwindow_type_t.teleport:
				if (WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.left)
				{
					if (num < CustomTeleporterControl.Instance.hardcoded_teleports.Length)
					{
						instantiated_crafting_slots[i].gameObject.SetActive(true);
						CustomTeleporterControl.Instance.DrawHardcodedTeleporterSlot(i, num);
					}
					else
					{
						instantiated_crafting_slots[i].gameObject.SetActive(false);
					}
				}
				else if (num < CustomTeleporterControl.Instance.GetNumberOfActiveTeleporters())
				{
					instantiated_crafting_slots[i].gameObject.SetActive(true);
					CustomTeleporterControl.Instance.DrawCustomTeleporterSlot(i, num);
				}
				else
				{
					instantiated_crafting_slots[i].gameObject.SetActive(false);
				}
				break;
			case WindowControl.miniwindow_type_t.inventory_and_merchant:
				if (num < curr_crafting_list.items.Length)
				{
					instantiated_crafting_slots[i].gameObject.SetActive(true);
					MerchantControl.Instance.DrawMerchantSlot(i, num);
				}
				else
				{
					instantiated_crafting_slots[i].gameObject.SetActive(false);
				}
				break;
			}
		}
		for (int j = 0; j < 3; j++)
		{
			if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_crafting && j + craft_PAGE * 3 < curr_crafting_list.items.Length)
			{
				DrawCraftingSlotPart2(j, j + craft_PAGE * 3);
			}
		}
	}

	private void DrawCraftingSlotPart1(int slot_id, int index)
	{
		InventoryItem item = curr_crafting_list.items[index].item;
		int count = curr_crafting_list.items[index].count;
		string fullItemName = GetFullItemName(item);
		string itemDescription = GetItemDescription(item.item_name);
		GetItemCraftingIngredientA(item.item_name);
		string itemCraftingIngredientB = GetItemCraftingIngredientB(item.item_name);
		int itemCraftingIngredientA_count = GetItemCraftingIngredientA_count(item.item_name);
		int itemCraftingIngredientB_count = GetItemCraftingIngredientB_count(item.item_name);
		string craftingAltIngredientA = GetCraftingAltIngredientA(item.item_name);
		int itemCraftingAltIngredientA_count = GetItemCraftingAltIngredientA_count(item.item_name);
		bool flag = itemCraftingIngredientB == "" && craftingAltIngredientA != "";
		CraftingSlot.req_placement req_place = (flag ? CraftingSlot.req_placement.enable_OR : CraftingSlot.req_placement.enable_normal);
		instantiated_crafting_slots[slot_id].LayOutCraftingSlot(fullItemName, itemDescription, 1f, false, false, false, true, flag, req_place, CraftingSlot.text_area_layout.condensed_crafting, CraftingSlot.slots_positioning.full_size, false);
		CraftingSlot craftingSlot = instantiated_crafting_slots[slot_id];
		craftingSlot.result_sprite.RedrawAsCrafting(item, count);
		craftingSlot.costA_txt.text = "x" + itemCraftingIngredientA_count;
		if (itemCraftingIngredientB != "")
		{
			craftingSlot.costB_txt.text = "x" + itemCraftingIngredientB_count;
		}
		else if (craftingAltIngredientA != "")
		{
			craftingSlot.costB_txt.text = "x" + itemCraftingAltIngredientA_count;
		}
		else
		{
			craftingSlot.reqB_sprite.gameObject.SetActive(false);
			craftingSlot.costB_txt.text = "";
		}
	}

	private void DrawCraftingSlotPart2(int slot_id, int index)
	{
		InventoryItem item = curr_crafting_list.items[index].item;
		string itemCraftingIngredientA = GetItemCraftingIngredientA(item.item_name);
		string itemCraftingIngredientB = GetItemCraftingIngredientB(item.item_name);
		int itemCraftingIngredientA_count = GetItemCraftingIngredientA_count(item.item_name);
		int itemCraftingIngredientB_count = GetItemCraftingIngredientB_count(item.item_name);
		string craftingAltIngredientA = GetCraftingAltIngredientA(item.item_name);
		int itemCraftingAltIngredientA_count = GetItemCraftingAltIngredientA_count(item.item_name);
		CraftingSlot craftingSlot = instantiated_crafting_slots[slot_id];
		craftingSlot.reqA_sprite.RedrawBasicHideCount(itemCraftingIngredientA, itemCraftingIngredientA_count);
		if (itemCraftingIngredientB != "")
		{
			craftingSlot.reqB_sprite.RedrawBasicHideCount(itemCraftingIngredientB, itemCraftingIngredientB_count);
		}
		else if (craftingAltIngredientA != "")
		{
			craftingSlot.reqB_sprite.RedrawBasicHideCount(craftingAltIngredientA, itemCraftingAltIngredientA_count);
		}
	}

	public void hide_selector()
	{
		if (prev_clicked_inv != null)
		{
			prev_clicked_inv.GetComponent<InventorySlotObject>().animated_segment.Stop();
			prev_clicked_inv.GetComponent<InventorySlotObject>().animated_segment.transform.localScale = Vector3.one;
		}
		selector.SetActive(false);
		hover_click.gameObject.SetActive(false);
	}

	public void click_hover_option_A()
	{
		hover_clicked = true;
		do_option(hover_options[0]);
	}

	public void click_hover_option_B()
	{
		hover_clicked = true;
		do_option(hover_options[1]);
	}

	public void click_hover_nullspace()
	{
		hover_clicked = true;
	}

	private void do_option(string action)
	{
		hide_selector();
		if (action == "eat" || action == "tool" || action == "sell" || action == "load song" || action == "load painting" || action == "save song" || action == "save painting")
		{
			StartCoroutine(ON_DOUBLE_CLICK(prev_clicked_inv_i, action, prev_clicked_inv));
			return;
		}
		if (action == "place")
		{
			int trueIndex = GetTrueIndex(page_inventory, prev_clicked_inv_i);
			InventoryItem item = player_inventory[trueIndex].item;
			if (!iap_allowed(item))
			{
				iap_deny_popup(item, "build");
				return;
			}
			if (DevBuildControl.Instance.IsDevPlacedShackZone(ChunkControl.Instance.player_zone) && (!InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) || !(ZoneDataControl.Instance.curr_zonedata.house_item.GetString("quest_miniworld") != "true")))
			{
				PopupControl.Instance.ShowMessage(TranslationControl.Instance.TranslateGeneral("You can't build here. This area is owned by an NPC!", "GUI"), PopupControl.context.message);
				return;
			}
			if (ZoneDataControl.Instance.curr_zonedata.house_item.GetString("bandit_camp_instance") != "" && !BanditCampsControl.Instance.GetBanditCampInstanceByName(ZoneDataControl.Instance.curr_zonedata.house_item.GetString("bandit_camp_instance")).flag_destroyed)
			{
				PopupControl.Instance.ShowMessage(TranslationControl.Instance.TranslateGeneral("You can't build here. First you must find and destroy their flag!", "GUI"), PopupControl.context.message, new InventoryItem("Flagpole"));
				return;
			}
			if (InventoryUtils.IsHouseObject(item.item_name))
			{
				short @short = ZoneDataControl.Instance.curr_zonedata.house_item.GetShort("depth");
				if (item.item_name == "Underground Room")
				{
					if (ZoneDataControl.Instance.curr_zonedata.house_item.item_name == "Upstairs Room" || ZoneDataControl.Instance.curr_zonedata.house_item.item_name == "Magic Bean")
					{
						PopupControl.Instance.ShowMessage("You can't put Underground Rooms here.", PopupControl.context.message);
						return;
					}
					if (@short == 5)
					{
						PopupControl.Instance.ShowMessage("You can't build deeper!", PopupControl.context.message);
						return;
					}
				}
				else if (item.item_name == "Upstairs Room")
				{
					if (ChunkControl.Instance.player_zone == "overworld")
					{
						PopupControl.Instance.ShowMessage("You can only put Upstairs Rooms indoors.", PopupControl.context.message);
						return;
					}
					if (ZoneDataControl.Instance.curr_zonedata.house_item.item_name == "Underground Room" || ZoneDataControl.Instance.curr_zonedata.house_item.item_name == "Magic Bean" || ZoneDataControl.Instance.curr_zonedata.house_item.item_name == "Igloo")
					{
						PopupControl.Instance.ShowMessage("You can't put Upstairs Rooms here.", PopupControl.context.message);
						return;
					}
					if (InventoryUtils.GetBuildingType(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) == InventoryUtils.building_type.tent)
					{
						PopupControl.Instance.ShowMessage("You can't put Upstairs Rooms in Tents.", PopupControl.context.message);
						return;
					}
					if (@short == -2)
					{
						PopupControl.Instance.ShowMessage("You can't build higher!", PopupControl.context.message);
						return;
					}
				}
				else if (ChunkControl.Instance.player_zone != "overworld")
				{
					if (!InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) && !InventoryUtils.IsHeavenDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) && !InventoryUtils.IsHellDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) && InventoryUtils.GetBuildingType(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) != InventoryUtils.building_type.warehouse)
					{
						PopupControl.Instance.ShowMessage("You can't put houses indoors.", PopupControl.context.message);
						return;
					}
					if (item.item_name == "Upstairs Room")
					{
						PopupControl.Instance.ShowMessage("You can't put Upstairs Rooms here.", PopupControl.context.message);
						return;
					}
				}
			}
			if (InventoryUtils.IsCaveObject(item.item_name) && ChunkControl.Instance.player_zone != "overworld")
			{
				PopupControl.Instance.ShowMessage("You can only build Personal Mines outdoors.", PopupControl.context.message);
				return;
			}
			if (InventoryUtils.IsHeavenDimension(item.item_name) && ChunkControl.Instance.player_zone != "overworld")
			{
				PopupControl.Instance.ShowMessage("You can only build Bean Stocks outdoors.", PopupControl.context.message);
				return;
			}
			string item_name = item.item_name;
			if (!(item_name == "3-day Land Claim") && !(item_name == "8-day Land Claim") && item_name == "Admin Land Claim" && ChunkControl.Instance.player_zone != "overworld")
			{
				PopupControl.Instance.ShowMessage("You can only place this outside", PopupControl.context.message);
				return;
			}
			StartCoroutine(ON_DOUBLE_CLICK(prev_clicked_inv_i, action, prev_clicked_inv));
		}
		else if (action == "equip")
		{
			int trueIndex2 = GetTrueIndex(page_inventory, prev_clicked_inv_i);
			InventoryItem item2 = player_inventory[trueIndex2].item;
			if (check_req_skill_to_equip(GetItemEquipReqStat(item2.item_name), GetItemEquipReqLvl(item2.item_name)))
			{
				int num;
				switch (GetItemType(item2))
				{
				case inv_type_t.holdable:
					num = hand_index;
					break;
				case inv_type_t.helmet:
					num = hat_index;
					break;
				case inv_type_t.armor:
					num = body_index;
					break;
				default:
					num = 0;
					break;
				}
				ItemCountPair value = player_inventory[num];
				player_inventory[num] = player_inventory[trueIndex2];
				player_inventory[trueIndex2] = value;
				RedrawInventorySlots();
				instantiated_inv_slots[num].GetComponent<InventorySlotObject>().animated_segment.Play("inventory_craft_oncomplete");
			}
			else
			{
				PopupControl.Instance.ShowMessage("Your <color=#66d8ff>" + GetItemEquipReqStat(item2.item_name) + "</color> skill must be <color=#f44242>" + GetItemEquipReqLvl(item2.item_name) + "</color> or greater to equip this.", PopupControl.context.message, item2);
			}
		}
		else if (action == "unequip")
		{
			List<int> emptyNonEquipmentInventorySlots = GetEmptyNonEquipmentInventorySlots(player_inventory);
			if (emptyNonEquipmentInventorySlots.Count != 0 && emptyNonEquipmentInventorySlots[0] != -1)
			{
				player_inventory[emptyNonEquipmentInventorySlots[0]] = player_inventory[prev_clicked_inv_i];
				player_inventory[prev_clicked_inv_i] = new ItemCountPair("", 0);
				RedrawInventorySlots();
			}
			else
			{
				PopupControl.Instance.ShowMessage("Can't unequip right now. You need some empty space in your inventory!", PopupControl.context.message);
			}
		}
		else if (action == "take")
		{
			attempt_take_item(GetTrueIndex(page_container, prev_clicked_inv_i), true);
			RedrawContainerSlots();
		}
		else if (action == "store" || action == "give" || action == "insert")
		{
			int trueIndex3 = GetTrueIndex(page_inventory, prev_clicked_inv_i);
			if (VendingMachineControl.Instance != null)
			{
				VendingMachineControl.Instance.SelectedItem(trueIndex3);
				return;
			}
			attempt_store_item(trueIndex3);
			RedrawInventorySlots();
		}
		else if (action == "split")
		{
			if (GetEmptyNonEquipmentInventorySlots(player_inventory).Count == 0)
			{
				PopupControl.Instance.ShowMessage("You must have an empty slot in your inventory", PopupControl.context.message);
				return;
			}
			int trueIndex4 = GetTrueIndex(page_inventory, prev_clicked_inv_i);
			if (player_inventory[trueIndex4].count == 0 || player_inventory[trueIndex4].count == 1)
			{
				PopupControl.Instance.ShowMessage("You cannot split a stack of 1", PopupControl.context.message);
				return;
			}
			split_inv_index = trueIndex4;
			HideInventoryTab(true);
			WindowPrefabsControl.Instance.CreateScreen("INVENTORY-split", WindowPrefabsControl.build_into_t.mini_window);
			og_split = player_inventory[trueIndex4].count;
			WindowPrefabsControl.Instance.GetTextLegacy("INVENTORY-split", "OG-count").text = string.Format("{0:n0}", og_split);
			split_obj = player_inventory[trueIndex4].item;
			split_1 = og_split / 2;
			split_2 = og_split - split_1;
			RedrawSplitGraphic();
		}
		else if (action == "read")
		{
			HideInventoryTab(true);
			BookControl.Instance = WindowPrefabsControl.Instance.CreateScreen("Book", WindowPrefabsControl.build_into_t.mini_window).GetComponent<BookControl>();
			int trueIndex5 = GetTrueIndex(page_inventory, prev_clicked_inv_i);
			BookControl.Instance.book_reading = player_inventory[trueIndex5].item;
			BookControl.Instance.InitialDraw();
			BookControl.Instance.curr_page = 0;
			BookControl.Instance.RedrawPages();
		}
	}

	private void RedrawSplitGraphic()
	{
		WindowPrefabsControl.Instance.GetObject("INVENTORY-split", "split1-input").GetComponent<InputField>().SetTextWithoutNotify(string.Format("{0:n0}", split_1));
		WindowPrefabsControl.Instance.GetObject("INVENTORY-split", "split2-input").GetComponent<InputField>().SetTextWithoutNotify(string.Format("{0:n0}", split_2));
		WindowPrefabsControl.Instance.GetObject("INVENTORY-split", "OG-graphic").GetComponent<ItemSprite>().RedrawBasicHideCount(split_obj, og_split);
		WindowPrefabsControl.Instance.GetObject("INVENTORY-split", "split1-graphic").GetComponent<ItemSprite>().RedrawBasicHideCount(split_obj, split_1);
		WindowPrefabsControl.Instance.GetObject("INVENTORY-split", "split2-graphic").GetComponent<ItemSprite>().RedrawBasicHideCount(split_obj, split_2);
	}

	public void edited_split_1()
	{
		int num = int.Parse(WindowPrefabsControl.Instance.GetObject("INVENTORY-split", "split1-input").transform.Find("Text").GetComponent<Text>().text.Replace(",", ""), Startup.parse_culture);
		if (num != -1 && num > 0 && num <= og_split && og_split - num != 0)
		{
			split_1 = num;
			split_2 = og_split - num;
		}
		RedrawSplitGraphic();
	}

	public void edited_split_2()
	{
		int num = int.Parse(WindowPrefabsControl.Instance.GetObject("INVENTORY-split", "split2-input").transform.Find("Text").GetComponent<Text>().text.Replace(",", ""), Startup.parse_culture);
		if (num != -1 && num > 0 && num <= og_split && og_split - num != 0)
		{
			split_2 = num;
			split_1 = og_split - num;
		}
		RedrawSplitGraphic();
	}

	public void press_split_accept()
	{
		ShowInventoryTab(true);
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-split");
		int num = GetEmptyNonEquipmentInventorySlots(player_inventory)[0];
		if (num != -1)
		{
			player_inventory[split_inv_index] = new ItemCountPair(split_obj, split_1);
			player_inventory[num] = new ItemCountPair(split_obj, split_2);
			RedrawInventorySlots();
		}
	}

	public void press_split_cancel()
	{
		ShowInventoryTab(true);
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-split");
	}

	private void attempt_store_item(int index)
	{
		int count = player_inventory[index].count;
		int num = ToContainer(player_inventory[index].item, player_inventory[index].count, ptype.eitherPage);
		if (num == 0)
		{
			player_inventory[index] = new ItemCountPair("", 0);
			return;
		}
		switch (container_style)
		{
		case container_style_t.world_container:
		case container_style_t.trading_table:
			PopupControl.Instance.ShowMessage("Can't store item. The container is full!", PopupControl.context.message);
			break;
		case container_style_t.companion_pockets:
			PopupControl.Instance.ShowMessage("Can't give item. Companion can't hold any more!", PopupControl.context.message);
			break;
		}
		if (count != num)
		{
			player_inventory[index] = new ItemCountPair(player_inventory[index].item, num);
		}
	}

	private void attempt_take_item(int index, bool do_show_angular)
	{
		int count = curr_container[index].count;
		int num = ToInventory(curr_container[index].item, curr_container[index].count, ptype.eitherPage);
		if (num == 0)
		{
			curr_container[index] = new ItemCountPair("", 0);
			if (do_show_angular)
			{
				switch (page_container)
				{
				case 3:
					show_angular(instantiated_inv_slots[index - n_slots_per_page_ - p2_begin_].transform.localPosition, getitem_angular_col);
					break;
				case 2:
					show_angular(instantiated_inv_slots[index - p2_begin_].transform.localPosition, getitem_angular_col);
					break;
				case 1:
					show_angular(instantiated_inv_slots[index].transform.localPosition, getitem_angular_col);
					break;
				}
			}
		}
		else
		{
			PopupControl.Instance.ShowMessage("Can't take item. Your inventory is full!", PopupControl.context.message);
			if (count != num)
			{
				curr_container[index] = new ItemCountPair(curr_container[index].item, num);
			}
		}
	}

	public void OnClose()
	{
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container)
		{
			switch (container_style)
			{
			case container_style_t.trading_table:
				foreach (int permittedContainerSlot in GetPermittedContainerSlots(container_style, "Trading Table", ptype.firstPage))
				{
					if (curr_container[permittedContainerSlot].item.item_name != "")
					{
						GiveItem(curr_container[permittedContainerSlot].item, curr_container[permittedContainerSlot].count, null, false);
					}
				}
				WindowPrefabsControl.Instance.DestroyScreen("TradingTable");
				break;
			case container_style_t.companion_pockets:
				CompanionController.Instance.CompanionPocketsClosed(curr_container);
				break;
			case container_style_t.world_container:
			{
				string item_name = GameController.Instance.interacting_element_item.item_name;
				int @long = GameController.Instance.interacting_element_item.GetLong("basket_id");
				if (GameServerConnector.Instance.FullyInGame())
				{
					GameServerSender.Instance.SendCloseBasket(@long, curr_container, "");
				}
				else if (!GameServerConnector.Instance.MidConnect())
				{
					curr_container.SaveToAllAsContainer(@long);
				}
				ExtraInventoryData extraDataCopy;
				if (item_name == "Armor Display")
				{
					extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
					extraDataCopy.SaveSubItem(curr_container[5].item, "hat");
					extraDataCopy.SaveSubItem(curr_container[7].item, "body");
				}
				else if (item_name == "Custom Statue")
				{
					extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
					extraDataCopy.SaveSubItem(curr_container[5].item, "hat");
					extraDataCopy.SaveSubItem(curr_container[6].item, "wep");
					extraDataCopy.SaveSubItem(curr_container[7].item, "body");
				}
				else if (item_name == "Weapon Display")
				{
					extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
					extraDataCopy.SaveSubItem(curr_container[6].item, "wep");
				}
				else
				{
					if (!(item_name == "Large Weapon Display"))
					{
						break;
					}
					extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
					extraDataCopy.SaveSubItem(curr_container[5].item, "wep");
					extraDataCopy.SaveSubItem(curr_container[7].item, "wep2");
				}
				ConstructionControl.Instance.PlayerReplaceInteracting(new InventoryItem(item_name, extraDataCopy), true);
				break;
			}
			}
			curr_container = new BasketContents();
		}
		CancelDragging();
		if ((WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container || WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_merchant) && GameServerConnector.Instance.FullyInGame())
		{
			GameServerSender.Instance.SendReleaseInteractingObject();
		}
		hide_selector();
		HideInventoryTab(false);
		HideCraftingTab();
		HideRespawnTimer();
		angular.SetActive(false);
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-split");
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-statue advanced");
		WindowPrefabsControl.Instance.DestroyScreen("Book");
	}

	private void create_hover(int index)
	{
		hover_clicked = true;
		if (prev_clicked_inv != instantiated_inv_slots[index])
		{
			hide_selector();
		}
		hover_options = new List<string>();
		prev_clicked_inv_i = index;
		ItemCountPair itemCountPair = ((WindowControl.Instance.curr_miniwindow != WindowControl.miniwindow_type_t.inventory_and_container || WindowControl.Instance.curr_miniwindow_tab_selected != WindowControl.tab.right) ? player_inventory[GetTrueIndex(page_inventory, index)] : curr_container[GetTrueIndex(page_container, index)]);
		if (itemCountPair.item.item_name == "")
		{
			prev_clicked_inv = null;
			return;
		}
		prev_clicked_inv = instantiated_inv_slots[index];
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container && WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.right)
		{
			hover_options.Add("take");
		}
		else if (MusicBoxControl.Instance.save_screen_open)
		{
			if (itemCountPair.item.item_name == "Blank Record")
			{
				hover_options.Add("save song");
			}
		}
		else if (MusicBoxControl.Instance.load_screen_open)
		{
			if (itemCountPair.item.item_name == "Saved Record")
			{
				hover_options.Add("load song");
			}
		}
		else if (PaintingControl.Instance != null)
		{
			if (PaintingControl.Instance.save_screen_open)
			{
				if (itemCountPair.item.item_name == "Blank Canvas")
				{
					hover_options.Add("save painting");
				}
			}
			else if (PaintingControl.Instance.load_screen_open && itemCountPair.item.item_name == "Painting")
			{
				hover_options.Add("load painting");
			}
		}
		else if (VendingMachineControl.Instance != null)
		{
			if (VendingMachineControl.Instance.pick_item_screen_open)
			{
				hover_options.Add("insert");
			}
		}
		else if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container)
		{
			switch (container_style)
			{
			case container_style_t.companion_pockets:
				hover_options.Add("give");
				break;
			case container_style_t.world_container:
			case container_style_t.trading_table:
				if (GameController.Instance.interacting_element_item.item_name == "Egg Fuser")
				{
					hover_options.Add("insert");
				}
				else
				{
					hover_options.Add("store");
				}
				break;
			}
		}
		else if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_merchant)
		{
			if (itemCountPair.item.item_name != "Coins")
			{
				hover_options.Add("sell");
			}
		}
		else
		{
			if (GetItemMaxStack(itemCountPair.item.item_name) != 1 && itemCountPair.count > 1)
			{
				hover_options.Add("split");
			}
			if (itemCountPair.item.item_name == "Book")
			{
				hover_options.Add("read");
			}
			else
			{
				switch (GetItemType(itemCountPair.item))
				{
				case inv_type_t.place_in_world:
					if ((!(ResourceControl.Instance.GetStringFromItemFile(itemCountPair.item.item_name, "not_obtainable") == "true") || PlayerData.Instance.GetGlobalShort("dev_mode") != 0) && (!(itemCountPair.item.item_name == "Trophy") || !(itemCountPair.item.GetString("trophy_VALIDATOR") != FriendServerInterface.Instance.GetTrophyValidatorString(itemCountPair.item.GetString("trophy_name"), itemCountPair.item.GetString("trophy_reason"), itemCountPair.item.GetString("paint"), itemCountPair.item.GetString("trophy_from"), itemCountPair.item.GetString("trophy_for"), itemCountPair.item.GetString("trophy_date")))))
					{
						hover_options.Add("place");
					}
					break;
				case inv_type_t.armor:
				case inv_type_t.helmet:
				case inv_type_t.holdable:
					if (prev_clicked_inv_i == hat_index || prev_clicked_inv_i == body_index || prev_clicked_inv_i == hand_index)
					{
						hover_options.Add("unequip");
					}
					else
					{
						hover_options.Add("equip");
					}
					break;
				case inv_type_t.tool:
					hover_options.Add("tool");
					break;
				case inv_type_t.consumable:
					hover_options.Add("eat");
					break;
				}
			}
		}
		hover_anm.Stop();
		hover_anm.Play();
		hover_click.gameObject.SetActive(true);
		hover_click.transform.localPosition = instantiated_inv_slots[index].transform.localPosition;
		hover_text_title.text = GetFullItemName(itemCountPair.item);
		hover_text_desc.text = GetItemDescription(itemCountPair.item.item_name);
		selector.SetActive(true);
		selector.GetComponent<Animation>().Stop();
		selector.GetComponent<Animation>().Play();
		selector.transform.localPosition = instantiated_inv_slots[index].transform.localPosition;
		selector.transform.SetAsLastSibling();
		if (index == hand_index || index == hat_index || index == body_index)
		{
			((RectTransform)selector.transform).sizeDelta = new Vector2(89f, 89f);
		}
		else
		{
			((RectTransform)selector.transform).sizeDelta = new Vector2(100f, 100f);
		}
		instantiated_inv_slots[index].transform.SetAsLastSibling();
		instantiated_inv_slots[index].GetComponent<InventorySlotObject>().animated_segment.Play("inventory_clickselect");
		hover_click.transform.SetAsLastSibling();
		hover_sprite.RedrawAsHoverIcon(itemCountPair.item, itemCountPair.count);
		float num = (hover_text_title.preferredWidth - 107f) / 158f;
		if (num < 0f)
		{
			num = 0f;
		}
		num = num * 110f + 230f;
		switch (hover_options.Count)
		{
		case 2:
			if (index < 5)
			{
				hover_panel.transform.localPosition = new Vector3(0f, 153f, 0f);
			}
			else if (index == 16)
			{
				hover_panel.transform.localPosition = new Vector3(0f, 175f, 0f);
			}
			else
			{
				hover_panel.transform.localPosition = new Vector3(0f, 259f, 0f);
			}
			hover_panel.sizeDelta = new Vector2(num, 297f);
			hover_item_parent.anchoredPosition = new Vector2(-3f, -23f);
			hover_item_parent.localScale = Vector3.one * 0.9f;
			hover_button.SetActive(true);
			hover_button_2.SetActive(true);
			for (int i = 0; i < hover_button_cols.Length; i++)
			{
				if (hover_button_cols[i].name == hover_options[0])
				{
					hover_button_inner.color = hover_button_cols[i].button_col;
					hover_button_text.text = TranslationControl.Instance.TranslateGeneral(hover_button_cols[i].text, "GUI").ToUpper();
					break;
				}
			}
			for (int j = 0; j < hover_button_cols.Length; j++)
			{
				if (hover_button_cols[j].name == hover_options[1])
				{
					hover_button_2_inner.color = hover_button_cols[j].button_col;
					hover_button_2_text.text = TranslationControl.Instance.TranslateGeneral(hover_button_cols[j].text, "GUI").ToUpper();
					break;
				}
			}
			break;
		case 1:
			if (index < 5)
			{
				hover_panel.transform.localPosition = new Vector3(0f, 194f, 0f);
			}
			else if (index == 16)
			{
				hover_panel.transform.localPosition = new Vector3(0f, 175f, 0f);
			}
			else
			{
				hover_panel.transform.localPosition = new Vector3(0f, 217f, 0f);
			}
			hover_panel.sizeDelta = new Vector2(num, 188f);
			hover_item_parent.anchoredPosition = new Vector2(-3f, -23f);
			hover_item_parent.localScale = Vector3.one * 0.9f;
			hover_button.SetActive(true);
			hover_button_2.SetActive(false);
			for (int k = 0; k < hover_button_cols.Length; k++)
			{
				if (hover_button_cols[k].name == hover_options[0])
				{
					hover_button_inner.color = hover_button_cols[k].button_col;
					hover_button_text.text = TranslationControl.Instance.TranslateGeneral(hover_button_cols[k].text, "GUI").ToUpper();
					break;
				}
			}
			break;
		case 0:
			hover_panel.transform.localPosition = new Vector3(0f, 159f, 0f);
			hover_panel.sizeDelta = new Vector2(num, 82f);
			hover_item_parent.anchoredPosition = new Vector2(-16f, -41f);
			hover_item_parent.localScale = Vector3.one * 1.14f;
			hover_button.SetActive(false);
			hover_button_2.SetActive(false);
			break;
		}
	}

	private int GetTrueInventoryIndex(int inventory_slot_id)
	{
		return GetTrueIndex(page_inventory, inventory_slot_id);
	}

	private int GetTrueContainerIndex(int container_slot_id)
	{
		return GetTrueIndex(page_container, container_slot_id);
	}

	private int GetTrueIndex(int page, int slot_id)
	{
		if (slot_id == hand_index || slot_id == body_index || page == 1 || slot_id == hat_index)
		{
			return slot_id;
		}
		switch (page)
		{
		case 3:
			return p2_begin_ + slot_id + n_slots_per_page_;
		case 2:
			return p2_begin_ + slot_id;
		default:
			return slot_id;
		}
	}

	public void slot_mouse_down(int index, GameObject mouse_down_slot)
	{
		if (angular_animation_playing)
		{
			return;
		}
		if (WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.left)
		{
			if (player_inventory[GetTrueIndex(page_inventory, index)].item.item_name == "")
			{
				return;
			}
		}
		else if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.inventory_and_container)
		{
			if (curr_container[GetTrueIndex(page_container, index)].item.item_name == "")
			{
				return;
			}
			if (GameController.Instance.interacting_element_item.item_name == "Egg Fuser" && GameController.Instance.interacting_element_item.GetShort("started_fusion") == 1)
			{
				return;
			}
		}
		this.mouse_down_slot = mouse_down_slot;
		test_for_dragging = true;
		initial_press = GamepadInput.Instance.GetMousePosition();
	}

	public void CompleteSell()
	{
		int sellPrice = MerchantControl.Instance.GetSellPrice(trying_to_craft.item);
		int num = trying_to_craft.count * sellPrice;
		BasketContents basketContents = ClonePlayerInventory();
		basketContents.RemoveItemExact(trying_to_craft.item, trying_to_craft.count, sell_index);
		if (!CanReceiveItem("Coins", num, false, basketContents))
		{
			PopupControl.Instance.ShowMessage(TranslationControl.Instance.TranslateGeneral("Can't sell item - you don't have enough space in your inventory for all the coins you will recieve!", "GUI"), PopupControl.context.message);
			RedrawInventorySlots();
			return;
		}
		player_inventory.RemoveItemExact(trying_to_craft.item, trying_to_craft.count, sell_index);
		GiveItem("Coins", num, "", false);
		PopupControl.Instance.HideAll();
		mouse_down_slot.GetComponent<InventorySlotObject>().animated_segment.Play("inventory_craft_oncomplete");
		show_angular(mouse_down_slot.transform.localPosition, default_angular_col, angular_sound_t.sell);
		RedrawInventorySlots();
		if (!(GameController.Instance.interacting_element_item.GetString("npc_file") == "Cupcake Crab"))
		{
			return;
		}
		switch (trying_to_craft.item.item_name)
		{
		case "Black Shell":
		case "Blue Shell":
		case "Green Shell":
		case "White Shell":
		case "Gold Shell":
		case "Red Shell":
		case "Purple Shell":
			switch (QuestControl.Instance.TrySellQuestItemToNPC("Colors of the Sea", 1, 2, trying_to_craft.item))
			{
			case QuestControl.sell_quest_item_result.all_collected:
				enter_dialogue_on_close = 800;
				WindowControl.Instance.CloseMiniwindow(false);
				break;
			case QuestControl.sell_quest_item_result.new_collected:
				enter_dialogue_on_close = 800;
				break;
			}
			break;
		}
	}

	private int GetTotalNumberOfItemInInventory(InventoryItem item)
	{
		int num = 0;
		foreach (int item2 in player_inventory.FilledSlots())
		{
			if (player_inventory[item2].item == item)
			{
				num += player_inventory[item2].count;
			}
		}
		return num;
	}

	private IEnumerator ON_DOUBLE_CLICK(int inventory_slot_id, string action, GameObject mouse_down_slot)
	{
		int inv_index = GetTrueInventoryIndex(inventory_slot_id);
		ItemCountPair item_pair = player_inventory[inv_index];
		string fullItemName = GetFullItemName(item_pair.item);
		bool remove_from_inventory_after_use = false;
		if (action == "save song")
		{
			if (item_pair.item.item_name != "Blank Record")
			{
				yield break;
			}
		}
		else if (action == "load song")
		{
			if (item_pair.item.item_name != "Saved Record")
			{
				yield break;
			}
		}
		else if (action == "save painting")
		{
			if (item_pair.item.item_name != "Blank Canvas")
			{
				yield break;
			}
		}
		else if (action == "load painting")
		{
			if (item_pair.item.item_name != "Painting")
			{
				yield break;
			}
		}
		else
		{
			if (action == "sell")
			{
				if (item_pair.item.item_name == "Coins")
				{
					yield break;
				}
				if (MerchantControl.Instance.IsItemSellable(item_pair.item))
				{
					int totalNumberOfItemInInventory = GetTotalNumberOfItemInInventory(item_pair.item);
					if (totalNumberOfItemInInventory == 1)
					{
						trying_to_craft = new ItemCountPair(item_pair.item, 1);
						string tEXT = TranslationControl.Instance.TranslateGeneral("Sell your XYZ?", "GUI").Replace("XYZ", "<color=#eeee55>" + fullItemName + "</color>");
						ShopControl.Instance.ShowShopPopup("YES", ShopControl.button_color_t.yes_green, "CANCEL", ShopControl.button_color_t.no_red, false, tEXT, new Color(1f, 1f, 1f, 1f), item_pair.item, 1, new Color(0.2980392f, 0.50980395f, 0.5294118f, 1f), ShopControl.popup_context.vendor_sell_yes_no);
					}
					else
					{
						trying_to_craft = new ItemCountPair(item_pair.item, item_pair.count);
						string tEXT2 = TranslationControl.Instance.TranslateGeneral("Sell how many?", "GUI");
						ShopControl.Instance.ShowShopPopup("ACCEPT", ShopControl.button_color_t.yes_green, "CANCEL", ShopControl.button_color_t.no_red, false, tEXT2, new Color(1f, 1f, 1f, 1f), item_pair.item, item_pair.count, new Color(0.2980392f, 0.50980395f, 0.5294118f, 1f), ShopControl.popup_context.vendor_sell_yes_no, totalNumberOfItemInInventory);
					}
					sell_index = inv_index;
				}
				else
				{
					PopupControl.Instance.ShowMessage(DialogueControl.Instance.curr_NPC_display_name + " doesn't want that.", PopupControl.context.message);
				}
				yield break;
			}
			if (action == "place")
			{
				remove_from_inventory_after_use = true;
			}
			else if (action == "tool")
			{
				if (InventoryUtils.IsPaintbrush(item_pair.item.item_name))
				{
					remove_from_inventory_after_use = true;
					ConstructionControl.Instance.click_to_place_txt.text = "Click furniture to paint it.";
				}
				else if (InventoryUtils.IsStamp(item_pair.item.item_name))
				{
					remove_from_inventory_after_use = true;
					ConstructionControl.Instance.click_to_place_txt.text = "Click furniture to add symbol";
				}
				else
				{
					switch (item_pair.item.item_name)
					{
					case "Magma Shovel":
						remove_from_inventory_after_use = false;
						ConstructionControl.Instance.click_to_place_txt.text = "Click to PICK UP lava";
						break;
					case "Titanium Shovel":
						remove_from_inventory_after_use = false;
						ConstructionControl.Instance.click_to_place_txt.text = "Click to PICK UP big trees etc.";
						break;
					case "Lock":
						remove_from_inventory_after_use = true;
						ConstructionControl.Instance.click_to_place_txt.text = "Click something to LOCK it";
						break;
					case "Shovel":
						remove_from_inventory_after_use = false;
						ConstructionControl.Instance.click_to_place_txt.text = "Click to PICK UP bushes etc.";
						break;
					case "Builder Tools":
						remove_from_inventory_after_use = false;
						ConstructionControl.Instance.click_to_place_txt.text = "Click to MOVE/PICK-UP furniture";
						break;
					case "Drill":
						remove_from_inventory_after_use = false;
						ConstructionControl.Instance.click_to_place_txt.text = "Click to DESTROY natural objects.";
						break;
					case "Paint Thinner":
						remove_from_inventory_after_use = true;
						ConstructionControl.Instance.click_to_place_txt.text = "Click furniture to REMOVE PAINT";
						break;
					}
				}
			}
			else
			{
				if (!(action == "eat"))
				{
					yield break;
				}
				remove_from_inventory_after_use = true;
				if (item_pair.item.item_name == "Expando Bread" && GameServerConnector.Instance.FullyInGame() && GameServerReceiver.Instance.disabled_perks.Contains("perk_giant"))
				{
					PopupControl.Instance.ShowMessage("You may not eat Expando Bread on this server.", PopupControl.context.message);
					yield break;
				}
			}
		}
		angular_animation_playing = true;
		mouse_down_slot.GetComponent<InventorySlotObject>().animated_segment.Play("inventory_craft_oncomplete");
		show_angular(mouse_down_slot.transform.localPosition, default_angular_col);
		switch (GetItemSoundOnUse(item_pair.item.item_name))
		{
		case sound_on_use.slurp:
			AudioControl.Instance.Play(AudioControl.Instance.sfx_slurp, 0.8f);
			break;
		case sound_on_use.crunch:
			AudioControl.Instance.Play(AudioControl.Instance.sfx_crunch);
			break;
		}
		yield return new WaitForSeconds(0.5f);
		angular_animation_playing = false;
		angular.SetActive(false);
		if (action == "save song")
		{
			MusicBoxControl.Instance.double_click_record_to_select(inv_index);
		}
		else if (action == "load song")
		{
			MusicBoxControl.Instance.double_click_record_to_load(inv_index);
		}
		else if (action == "save painting")
		{
			PaintingControl.Instance.FinalizeSave(inv_index);
		}
		else if (action == "load painting")
		{
			PaintingControl.Instance.FinalizeLoad(inv_index);
		}
		else if (action == "eat")
		{

			int hP_max = GameController.Instance.player.GetComponent<Combatant>().HP_max;
			float num = -10f;
			bool heal = false;
			switch (item_pair.item.item_name)
			{
			case "Bread Loaf":
			case "Coco Loco":
			case "Pumpkin Pie":
			case "Fried Egg":
			case "GoldBerry":
			case "Mushroom Stew":
			case "Toasted Nuts":
			case "Cooked Meat":
				num = 15f;
				heal = true;
				break;
			case "Muffin":
			case "Brown Mushroom":
			case "SalmonBerry":
			case "MoonBerry":
			case "Red Mushroom":
			case "Berry":
				num = 8f;
				heal = true;
				break;
			case "Bug Meat":
			case "Uncooked Meat":
				if (UnityEngine.Random.value >= 0.5f)
				{
					num = 7f;
				}
				heal = true;
				break;
			case "Purple Mushroom":
			case "Venom":
				heal = true;
				break;
			case "Jam":
				num = 33f;
				heal = true;
				break;
			case "Super Jam":
				num = 100f;
				heal = true;
				break;
			case "Super Carrot":
				break;
			case "Rushers":
				WindowControl.Instance.CloseMiniwindow(true);
				PerkData perkData3 = PerkControl.Instance.ClonePerkForCasting("perk_sprint", GameController.Instance.player);
				perkData3.all_effects["EFFECT_SPRINT"].data["Duration"] = "60 seconds";
				PerkControl.Instance.ApplyInitialCastOnto(perkData3, 1, "LOCAL", 1, GameController.Instance.player);
				break;
			case "Hot Pepper":
			{
				WindowControl.Instance.CloseMiniwindow(true);
				PerkData perkData2 = PerkControl.Instance.ClonePerkForCasting("perk_ignite", GameController.Instance.player);
				int num2 = (int)((float)hP_max * 0.1f);
				if (num2 == 0)
				{
					num2 = 1;
				}
				perkData2.all_effects["EFFECT_BURN"].data["Damage"] = num2.ToString() ?? "";
				PerkControl.Instance.ApplyInitialCastOnto(perkData2, 1, "LOCAL", 1, GameController.Instance.player);
				break;
			}
			case "Matcha":
				WindowControl.Instance.CloseMiniwindow(true);
				PerkData perkData4 = PerkControl.Instance.ClonePerkForCasting("perk_cleanse", GameController.Instance.player);
				PerkControl.Instance.ApplyInitialCastOnto(perkData4, 1, "LOCAL", 1, GameController.Instance.player);
				break;
			case "Expando Bread":
				WindowControl.Instance.CloseMiniwindow(true);
				PerkData perkData5 = PerkControl.Instance.ClonePerkForCasting("perk_giant", GameController.Instance.player);
				perkData5.all_effects["EFFECT_CRUSH_ALL"].data["Duration"] = "60 seconds";
				perkData5.all_effects["EFFECT_HUGE"].data["Duration"] = "60 seconds";
				PerkControl.Instance.ApplyInitialCastOnto(perkData5, 1, "LOCAL", 1, GameController.Instance.player);
				break;
			case "Moonberry Juice":
				WindowControl.Instance.CloseMiniwindow(true);
				PerkData perkData6 = PerkControl.Instance.ClonePerkForCasting("perk_eagle_eye", GameController.Instance.player);
				perkData6.all_effects["EFFECT_VISION"].data["Duration"] = "60 seconds";
				PerkControl.Instance.ApplyInitialCastOnto(perkData6, 1, "LOCAL", 1, GameController.Instance.player);
				break;
			case "Salmonberry Scone":
				WindowControl.Instance.CloseMiniwindow(true);
				PerkData perkData7 = PerkControl.Instance.ClonePerkForCasting("perk_defensive_shell", GameController.Instance.player);
				perkData7.all_effects["EFFECT_DEFEND"].data["Duration"] = "60 seconds";
				perkData7.all_effects["EFFECT_DEFEND"].data["Damage Percent Mod On Was-Hit"] = "70%";
				PerkControl.Instance.ApplyInitialCastOnto(perkData7, 1, "LOCAL", 1, GameController.Instance.player);
				break;
			case "Charm Bomb":
				WindowControl.Instance.CloseMiniwindow(true);
				PerkData perkData8 = PerkControl.Instance.ClonePerkForCasting("perk_charm", GameController.Instance.player);
				PerkControl.Instance.ApplyInitialCastOnto(perkData8, 1, "LOCAL", 1, GameController.Instance.player);
				break;
			case "Red Curry":
				WindowControl.Instance.CloseMiniwindow(true);
				PerkData perkData9 = PerkControl.Instance.ClonePerkForCasting("perk_fire_storm", GameController.Instance.player);
				perkData9.all_effects["EFFECT_BURN_ALL"].data["Area Effect Radius"] = "3.5m";
				perkData9.all_effects["EFFECT_BURN_ALL"].data["Particle Radius"] = "3.5m";
				perkData9.all_effects["EFFECT_BURN_ALL"].data["Duration"] = "14 seconds";
				PerkControl.Instance.ApplyInitialCastOnto(perkData9, 1, "LOCAL", 1, GameController.Instance.player);
				break;
			case "Bone Juice":
			{
				WindowControl.Instance.CloseMiniwindow(true);
				GameController.Instance.currentEXP += 30;
				GameController.Instance.SaveCurrentExpToDisk();
				GameController.Instance.animate_exp_bar = true;
				GameController.Instance.showOverheadNotif("+" + 30 + " Exp", GameController.Instance.player.transform.position, true, true);
				break;
			}
			}
			if (heal)
			{
				int num3 = (int)(num * 0.01f * (float)hP_max);
				if (num <= 0f)
				{
					WindowControl.Instance.CloseMiniwindow(true);
					GameController.Instance.player.GetComponent<Combatant>().WasHit(-num3, GameController.Instance.player, false, false, Combatant.hit_col.color_green, true);
				}
				else
				{
					GameController.Instance.player.GetComponent<Combatant>().IncreaseHp(num3);
					GameServerSender.Instance.SendIncreaseHp("LOCAL", num3, "");
				}
			}

		}
		else if (action == "tool")
		{
			ConstructionControl.Instance.EnterToolMode(item_pair.item);
			WindowControl.Instance.CloseMiniwindow(false);
		}
		else if (action == "place")
		{
			ConstructionControl.Instance.EnterBuildMode(item_pair.item, Vector3.zero, 0, false);
			WindowControl.Instance.CloseMiniwindow(false);
		}
		if (remove_from_inventory_after_use)
		{
			player_inventory[inv_index] = new ItemCountPair(player_inventory[inv_index].item, player_inventory[inv_index].count - 1);
			if (player_inventory[inv_index].count == 0)
			{
				player_inventory[inv_index] = new ItemCountPair("", 0);
			}
			RedrawInventorySlots();
		}
	}

	public void LayOutCraftingTab(background_strip_layout background_strip_layout_)
	{
		switch (background_strip_layout_)
		{
		case background_strip_layout.higher:
			crafting_background_strip.sizeDelta = new Vector2(1272f, 589f);
			crafting_background_strip.anchoredPosition = new Vector2(0.8f, -20f);
			break;
		case background_strip_layout.normal:
			crafting_background_strip.sizeDelta = new Vector2(1272f, 640f);
			crafting_background_strip.anchoredPosition = new Vector2(0.8f, -57.8f);
			break;
		}
	}

	public void HideCraftingTab()
	{
		crafting_Tab.SetActive(false);
	}

	public void ShowInventoryTab(bool also_enable_top_buttons)
	{
		inventory_Tab.SetActive(true);
		if (also_enable_top_buttons)
		{
			WindowControl.Instance.ShowMiniwindowHeaders();
		}
	}

	public void HideInventoryTab(bool also_hide_top_buttons)
	{
		inventory_Tab.SetActive(false);
		if (also_hide_top_buttons)
		{
			WindowControl.Instance.HideMiniwindowHeaders();
		}
	}

	public bool page_exists(int mod)
	{
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.teleport)
		{
			if (WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.left)
			{
				if (mod == 1)
				{
					return craft_PAGE * 3 + 3 < CustomTeleporterControl.Instance.hardcoded_teleports.Length;
				}
			}
			else if (mod == 1)
			{
				return craft_PAGE * 3 + 3 < CustomTeleporterControl.Instance.GetNumberOfActiveTeleporters();
			}
			if (mod == -1)
			{
				return craft_PAGE * 3 > 0;
			}
			return false;
		}
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.quests_and_achieves)
		{
			switch (mod)
			{
			case -1:
				return craft_PAGE * 3 > 0;
			case 1:
				return craft_PAGE * 3 + 3 < AchievesControl.Instance.all_achivements.Length;
			default:
				return false;
			}
		}
		switch (mod)
		{
		case -1:
			return craft_PAGE * 3 > 0;
		case 1:
			return craft_PAGE * 3 + 3 < curr_crafting_list.items.Length;
		default:
			return false;
		}
	}

	private void do_cascade_craft_buttons(int dir)
	{
		if (CASCADE_CRAFT_BUTS != null)
		{
			StopCoroutine(CASCADE_CRAFT_BUTS);
		}
		CASCADE_CRAFT_BUTS = cascade_craft_buts(dir);
		StartCoroutine(CASCADE_CRAFT_BUTS);
	}

	private IEnumerator cascade_craft_buts(int dir)
	{
		for (int i = 0; i < 3; i++)
		{
			int index = ((dir == -1) ? i : (2 - i));
			instantiated_crafting_slots[index].GetComponent<Animation>().Stop();
			instantiated_crafting_slots[index].GetComponent<Animation>().Play();
			yield return new WaitForSeconds(0.01f);
		}
		CASCADE_CRAFT_BUTS = null;
	}
}
