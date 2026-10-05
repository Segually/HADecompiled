using System.Collections.Generic;
using UnityEngine;

public class LootControl : MonoBehaviour, OrderedStart
{
	private enum price_type_t
	{
		UNKNOWN = 0,
		calculated = 1,
		hardcoded = 2,
		no_price_mentioned = 3,
		ERROR_no_ingredients = 4
	}

	public struct item_price_pair
	{
		public string item_name;

		public int base_price;

		public bool paintable;

		public int max_stack;
	}

	private struct generated_loot
	{
		public InventoryItem item;

		public int price;

		public int count;
	}

	private enum gen_result_t
	{
		range2_upperNotFound = 0,
		range2_upperFound = 1,
		exact = 2
	}

	public static LootControl Instance;

	private List<item_price_pair> default_loot_all;

	public List<item_price_pair> paintbrushes_loot;

	private List<item_price_pair> stamps_loot;

	private List<item_price_pair> weapons_and_armor_loot;

	private float odds_painted_loot;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public bool ShouldPaintLoot(string item_name)
	{
		return false;
	}

	public static void GenerateLootData()
	{
	}

	private static void FormatPrices(List<string> output, List<item_price_pair> input)
	{
	}

	private static void InsertPair(string item, int price, bool paintable, int max_stack, ref List<item_price_pair> list)
	{
	}

	private static Dictionary<string, string> DebugParseItemEntries(string[] lines)
	{
		return null;
	}

	private static int DebugGetPrice(string item_name, bool apply_craft_bonus, ref price_type_t price_type, List<string> lists, ref bool paintable, ref bool dont_paint_on_sell, ref int max_stack, Dictionary<string, string> item_entries)
	{
		return 0;
	}

	public void ResolveLootExtraData(string item_name, ExtraInventoryData data, InventoryItem original_chest_item = null)
	{
	}

	public void LoadLootFile(string file_name, List<item_price_pair> load_to)
	{
	}

	public ItemCountPair GetSingleLoot(int target_value, int force_count = -1, bool allow_coins = true, InventoryItem original_chest_item = null)
	{
		return null;
	}

	public void AddManyLoots(int total_low, int total_high, BasketContents contents, int min_slots, int max_slots, string generate_as_if_chest_is, InventoryItem original_chest_item)
	{
	}

	public BasketContents GenerateLootChest(InventoryItem original_chest_item)
	{
		return null;
	}

	public ItemCountPair GetRelevantItemFromLootList(int target_price, List<item_price_pair> original_list, bool paint_all, int force_count = -1, List<InventoryItem> ignore_items = null)
	{
		return null;
	}

	private ItemCountPair PickFromMultiList(List<generated_loot> low, List<generated_loot> high, int target_price)
	{
		return null;
	}

	private ItemCountPair PickFromSingleList(List<generated_loot> list)
	{
		return null;
	}
}
