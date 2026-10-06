using System.IO;
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

	private List<item_price_pair> default_loot_all = new List<item_price_pair>();

	public List<item_price_pair> paintbrushes_loot = new List<item_price_pair>();

	private List<item_price_pair> stamps_loot = new List<item_price_pair>();

	private List<item_price_pair> weapons_and_armor_loot = new List<item_price_pair>();

	private float odds_painted_loot = 0.33f;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		if (default_loot_all.Count == 0)
		{
			LoadLootFile("(Auto Gen) LOOT_normal_all", default_loot_all);
			LoadLootFile("(Auto Gen) LOOT_paintbrushes", paintbrushes_loot);
			LoadLootFile("(Auto Gen) LOOT_stamps", stamps_loot);
			LoadLootFile("(Auto Gen) LOOT_weapons_and_armor", weapons_and_armor_loot);
		}
	}

	public bool ShouldPaintLoot(string item_name)
	{
		return item_name != "Painting";
	}

	public static void GenerateLootData()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		List<string> list4 = new List<string>();
		List<string> list5 = new List<string>();
		List<string> list6 = new List<string>();
		List<string> list7 = new List<string>();
		List<string> list8 = new List<string>();
		List<string> list9 = new List<string>();
		List<item_price_pair> list10 = new List<item_price_pair>();
		List<item_price_pair> list11 = new List<item_price_pair>();
		List<item_price_pair> list12 = new List<item_price_pair>();
		List<item_price_pair> list13 = new List<item_price_pair>();
		List<item_price_pair> list14 = new List<item_price_pair>();
		List<item_price_pair> list15 = new List<item_price_pair>();
		Dictionary<string, List<item_price_pair>> dictionary = new Dictionary<string, List<item_price_pair>>();
		string[] files = Directory.GetFiles(Application.dataPath + "/SYNCHRONOUS/TextFiles/InventoryItems");
		foreach (string path in files)
		{
			if (Path.GetExtension(path) == ".meta")
			{
				continue;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			Dictionary<string, string> item_entries = DebugParseItemEntries(File.ReadAllLines(path));
			price_type_t price_type = price_type_t.UNKNOWN;
			List<string> list16 = new List<string>();
			bool paintable = false;
			bool dont_paint_on_sell = false;
			int max_stack = 1;
			int price = DebugGetPrice(fileNameWithoutExtension, true, ref price_type, list16, ref paintable, ref dont_paint_on_sell, ref max_stack, item_entries);
			switch (price_type)
			{
			case price_type_t.no_price_mentioned:
				list3.Add(fileNameWithoutExtension);
				continue;
			case price_type_t.ERROR_no_ingredients:
				list4.Add(fileNameWithoutExtension);
				continue;
			case price_type_t.calculated:
				InsertPair(fileNameWithoutExtension, price, false, max_stack, ref list10);
				break;
			case price_type_t.hardcoded:
				InsertPair(fileNameWithoutExtension, price, false, max_stack, ref list11);
				break;
			}
			if (list16.Contains("Paintbrushes"))
			{
				InsertPair(fileNameWithoutExtension, price, false, max_stack, ref list13);
			}
			else if (list16.Contains("Stamps"))
			{
				InsertPair(fileNameWithoutExtension, price, false, max_stack, ref list14);
			}
			else if (list16.Contains("Weapons") || list16.Contains("Armor"))
			{
				InsertPair(fileNameWithoutExtension, price, false, max_stack, ref list15);
			}
			else if (!item_entries.ContainsKey("not_obtainable") && fileNameWithoutExtension != "Coins" && fileNameWithoutExtension != "Painting" && fileNameWithoutExtension != "Saved Record" && fileNameWithoutExtension != "Egg" && fileNameWithoutExtension != "Fossil" && fileNameWithoutExtension != "Bonsai Tree" && fileNameWithoutExtension != "Book")
			{
				InsertPair(fileNameWithoutExtension, price, paintable, max_stack, ref list12);
			}
			if (list16.Count == 0)
			{
				list9.Add(fileNameWithoutExtension);
				continue;
			}
			foreach (string item in list16)
			{
				if (!dictionary.ContainsKey(item))
				{
					dictionary.Add(item, new List<item_price_pair>());
				}
				List<item_price_pair> list17 = dictionary[item];
				InsertPair(fileNameWithoutExtension, price, !dont_paint_on_sell && paintable, max_stack, ref list17);
			}
		}
		FormatPrices(list, list10);
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) _DEBUG_prices_Calculated.txt", list.ToArray());
		FormatPrices(list2, list11);
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) _DEBUG_prices_Hardcoded.txt", list2.ToArray());
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) _DEBUG_prices_NoPriceMentioned.txt", list3.ToArray());
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) _DEBUG_prices_ErrorNoIngredients.txt", list4.ToArray());
		FormatPrices(list5, list12);
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) LOOT_normal_all.txt", list5.ToArray());
		FormatPrices(list6, list13);
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) LOOT_paintbrushes.txt", list6.ToArray());
		FormatPrices(list7, list14);
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) LOOT_stamps.txt", list7.ToArray());
		FormatPrices(list8, list15);
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) LOOT_weapons_and_armor.txt", list8.ToArray());
		foreach (KeyValuePair<string, List<item_price_pair>> item2 in dictionary)
		{
			List<string> list18 = new List<string>();
			FormatPrices(list18, item2.Value);
			Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) VENDOR_" + item2.Key + ".txt", list18.ToArray());
		}
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) _DEBUG_vendor_ErrorNoVendor.txt", list9.ToArray());
	}

	private static void FormatPrices(List<string> output, List<item_price_pair> input)
	{
		for (int i = 0; i < input.Count; i++)
		{
			output.Add("[" + input[i].base_price + "]" + (input[i].paintable ? "[paintable]" : "") + ((input[i].max_stack > 1) ? ("[max_stack=" + input[i].max_stack + "]") : "") + " " + input[i].item_name);
		}
	}

	private static void InsertPair(string item, int price, bool paintable, int max_stack, ref List<item_price_pair> list)
	{
		item_price_pair item2 = new item_price_pair
		{
			item_name = item,
			base_price = price,
			paintable = paintable,
			max_stack = max_stack
		};
		for (int i = 0; i < list.Count; i++)
		{
			if (price < list[i].base_price)
			{
				list.Insert(i, item2);
				return;
			}
		}
		list.Add(item2);
	}

	private static Dictionary<string, string> DebugParseItemEntries(string[] lines)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string text in lines)
		{
			if (!string.IsNullOrWhiteSpace(text))
			{
				string key = text;
				string value = "";
				if (text.Contains("="))
				{
					int num = text.IndexOf("=");
					key = text.Substring(0, num).Trim();
					value = text.Substring(num + 1, text.Length - (num + 1)).Trim();
				}
				dictionary.Add(key, value);
			}
		}
		return dictionary;
	}

	private static int DebugGetPrice(string item_name, bool apply_craft_bonus, ref price_type_t price_type, List<string> lists, ref bool paintable, ref bool dont_paint_on_sell, ref int max_stack, Dictionary<string, string> item_entries)
	{
		int num = -1;
		if (item_entries.ContainsKey("Market Cost"))
		{
			string text = item_entries["Market Cost"];
			if (text == "CALCULATE")
			{
				price_type = price_type_t.calculated;
			}
			else
			{
				price_type = price_type_t.hardcoded;
				num = int.Parse(text);
			}
		}
		int num2 = (item_entries.ContainsKey("Crafting_ingredient_A_count") ? int.Parse(item_entries["Crafting_ingredient_A_count"]) : (-1));
		int num3 = (item_entries.ContainsKey("Crafting_ingredient_B_count") ? int.Parse(item_entries["Crafting_ingredient_B_count"]) : (-1));
		string text2 = "";
		if (item_entries.ContainsKey("Crafting_ingredient_A"))
		{
			text2 = item_entries["Crafting_ingredient_A"];
		}
		string text3 = "";
		if (item_entries.ContainsKey("Crafting_ingredient_B"))
		{
			text3 = item_entries["Crafting_ingredient_B"];
		}
		float num4 = (item_entries.ContainsKey("Craft_required_lvl") ? ((float)int.Parse(item_entries["Craft_required_lvl"])) : 1f);
		if (item_entries.ContainsKey("is_paintbrush") && item_entries["is_paintbrush"] == "true")
		{
			lists.Add("Paintbrushes");
		}
		if (item_entries.ContainsKey("is_stamp") && item_entries["is_stamp"] == "true")
		{
			lists.Add("Stamps");
		}
		if (item_entries.ContainsKey("paintable_by_player") && item_entries["paintable_by_player"] == "true")
		{
			paintable = true;
		}
		if (item_entries.ContainsKey("dont_sell_painted") && item_entries["dont_sell_painted"] == "true")
		{
			dont_paint_on_sell = true;
		}
		if (item_entries.ContainsKey("Max_stack"))
		{
			max_stack = int.Parse(item_entries["Max_stack"]);
		}
		float num5 = (item_entries.ContainsKey("Crafting_makes") ? float.Parse(item_entries["Crafting_makes"]) : 1f);
		if (item_entries.ContainsKey("Vendor0"))
		{
			lists.Add(item_entries["Vendor0"]);
		}
		if (item_entries.ContainsKey("Vendor1"))
		{
			lists.Add(item_entries["Vendor1"]);
		}
		if (item_entries.ContainsKey("Vendor2"))
		{
			lists.Add(item_entries["Vendor2"]);
		}
		if (item_entries.ContainsKey("Vendor3"))
		{
			lists.Add(item_entries["Vendor3"]);
		}
		if (item_entries.ContainsKey("Vendor4"))
		{
			lists.Add(item_entries["Vendor4"]);
		}
		if (price_type != price_type_t.hardcoded)
		{
			if (price_type == price_type_t.calculated)
			{
				if (num2 != -1 && text2 != "")
				{
					price_type_t price_type2 = price_type_t.UNKNOWN;
					List<string> lists2 = new List<string>();
					bool paintable2 = false;
					bool dont_paint_on_sell2 = false;
					int max_stack2 = 0;
					bool flag = text3 != "";
					Dictionary<string, string> item_entries2 = DebugParseItemEntries(File.ReadAllLines(Application.dataPath + "/SYNCHRONOUS/TextFiles/InventoryItems/" + text2 + ".txt"));
					if (num3 == -1 || !flag)
					{
						num = DebugGetPrice(text2, false, ref price_type2, lists2, ref paintable2, ref dont_paint_on_sell2, ref max_stack2, item_entries2) * num2;
					}
					else
					{
						Dictionary<string, string> item_entries3 = DebugParseItemEntries(File.ReadAllLines(Application.dataPath + "/SYNCHRONOUS/TextFiles/InventoryItems/" + text3 + ".txt"));
						num = DebugGetPrice(text2, false, ref price_type2, lists2, ref paintable2, ref dont_paint_on_sell2, ref max_stack2, item_entries2) * num2 + DebugGetPrice(text3, false, ref price_type2, lists2, ref paintable2, ref dont_paint_on_sell2, ref max_stack2, item_entries3) * num3;
					}
					float num6 = num;
					if (apply_craft_bonus)
					{
						return (int)((num4 * num6 * inventory_ctr.crafting_bonus_multiplier + num6) / num5);
					}
					return (int)(num6 / num5);
				}
				price_type = price_type_t.ERROR_no_ingredients;
			}
			else
			{
				price_type = price_type_t.no_price_mentioned;
			}
		}
		return num;
	}

	public void ResolveLootExtraData(string item_name, ExtraInventoryData data, InventoryItem original_chest_item = null)
	{
		switch (item_name)
		{
		case "Saved Record":
		{
			int num = UnityEngine.Random.Range(1, DevBuildControl.Instance.NPC_musicboxes.Length + 1);
			if (num == DevBuildControl.Instance.NPC_musicboxes.Length)
			{
				num = DevBuildControl.Instance.NPC_musicboxes.Length - 1;
			}
			data.SetShort("npc_record_index", num);
			break;
		}
		case "Egg":
			data.SetString("egg_monster", CreatureMorpher.Instance.GetRandomCreature());
			break;
		case "Painting":
		{
			int num2 = UnityEngine.Random.Range(1, DevBuildControl.Instance.NPC_paintings.Length + 1);
			if (num2 == DevBuildControl.Instance.NPC_paintings.Length)
			{
				num2 = DevBuildControl.Instance.NPC_paintings.Length - 1;
			}
			data.SetShort("dev_painting_id", num2);
			break;
		}
		case "Fossil":
			data.SetString("fossil_monster", CreatureMorpher.Instance.GetRandomCreature());
			break;
		case "Bonsai Tree":
			data.SetShort("bonsai_age", InventoryUtils.GenerateBonsaiAge());
			break;
		case "Book":
			if (original_chest_item != null)
			{
				BanditCampInstance banditCampInstanceByName = BanditCampsControl.Instance.GetBanditCampInstanceByName(original_chest_item.GetString("bandit_camp_instance"));
				if (banditCampInstanceByName != null)
				{
					inventory_ctr.Instance.FillOutBookData(BanditCampsControl.Instance.GetRandomLoreFromFaction(banditCampInstanceByName.biome_id), ref data);
				}
			}
			break;
		}
	}

	public void LoadLootFile(string file_name, List<item_price_pair> load_to)
	{
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("AutoGen/" + file_name, ref file_exists);
		if (!file_exists)
		{
			return;
		}
		for (int i = 0; i < textFileLines.Count; i++)
		{
			string text = textFileLines[i];
			if (Startup.StringNullOrWhitespace(text))
			{
				continue;
			}
			bool flag = text.Contains("[paintable]");
			if (flag)
			{
				text = text.Replace("[paintable]", "");
			}
			int num = 1;
			if (text.Contains("[max_stack="))
			{
				int num2 = text.IndexOf("[max_stack=") + "[max_stack=".Length;
				for (int j = 0; num2 + j < text.Length; j++)
				{
					if (text[num2 + j] == ']')
					{
						num = int.Parse(text.Substring(num2, j), Startup.parse_culture);
						text = text.Replace("[max_stack=" + num + "]", "");
						break;
					}
				}
			}
			int num3 = text.IndexOf(']');
			string s = text.Substring(1, num3 - 1);
			string item_name = text.Substring(num3 + 2, text.Length - (num3 + 2));
			item_price_pair item = default(item_price_pair);
			item.item_name = item_name;
			item.base_price = int.Parse(s, Startup.parse_culture);
			item.paintable = flag;
			item.max_stack = num;
			load_to.Add(item);
		}
	}

	public ItemCountPair GetSingleLoot(int target_value, int force_count = -1, bool allow_coins = true, InventoryItem original_chest_item = null)
	{
		if (allow_coins && UnityEngine.Random.value < 0.3f)
		{
			int num = (int)((float)target_value * 0.5f);
			if (num > 9999)
			{
				num = 10000;
			}
			return new ItemCountPair("Coins", num);
		}
		float value = UnityEngine.Random.value;
		List<item_price_pair> original_list;
		bool paint_all;
		if (value < 0.61f)
		{
			original_list = default_loot_all;
			paint_all = UnityEngine.Random.value < odds_painted_loot;
		}
		else
		{
			if (value < 0.68f)
			{
				original_list = paintbrushes_loot;
			}
			else if (value < 0.7f)
			{
				original_list = stamps_loot;
			}
			else
			{
				if (!(value < 1f))
				{
					return null;
				}
				original_list = weapons_and_armor_loot;
			}
			force_count = 1;
			paint_all = false;
		}
		ItemCountPair relevantItemFromLootList = GetRelevantItemFromLootList(target_value, original_list, paint_all, force_count);
		if (relevantItemFromLootList == null)
		{
			return null;
		}
		ExtraInventoryData extraDataCopy = relevantItemFromLootList.item.GetExtraDataCopy();
		ResolveLootExtraData(relevantItemFromLootList.item.item_name, extraDataCopy, original_chest_item);
		return new ItemCountPair(new InventoryItem(relevantItemFromLootList.item.item_name, extraDataCopy), relevantItemFromLootList.count);
	}

	public void AddManyLoots(int total_low, int total_high, BasketContents contents, int min_slots, int max_slots, string generate_as_if_chest_is, InventoryItem original_chest_item)
	{
		int num = UnityEngine.Random.Range(total_low, total_high);
		int num2 = UnityEngine.Random.Range(min_slots, max_slots + 1);
		if (num2 <= max_slots)
		{
			max_slots = num2;
		}
		List<int> permittedContainerSlots = inventory_ctr.Instance.GetPermittedContainerSlots(inventory_ctr.container_style_t.world_container, generate_as_if_chest_is, inventory_ctr.ptype.firstPage);
		if (max_slots <= 0)
		{
			return;
		}
		float num3 = (float)max_slots - 1f;
		float num4 = ((num3 != 0f) ? num3 : 1f);
		float num5 = num;
		int num6 = 0;
		do
		{
			ItemCountPair itemCountPair = null;
			bool flag = false;
			if (original_chest_item.GetString("bandit_camp_instance") != "" && UnityEngine.Random.value < 0.04f)
			{
				BanditCampInstance banditCampInstanceByName = BanditCampsControl.Instance.GetBanditCampInstanceByName(original_chest_item.GetString("bandit_camp_instance"));
				if (banditCampInstanceByName != null && BanditCampsControl.Instance.GetRandomLoreFromFaction(banditCampInstanceByName.biome_id) != "")
				{
					ExtraInventoryData extraInventoryData = new ExtraInventoryData();
					ResolveLootExtraData("Book", extraInventoryData, original_chest_item);
					InventoryItem inventoryItem = new InventoryItem("Book", extraInventoryData);
					int buyPrice = MerchantControl.Instance.GetBuyPrice(inventoryItem);
					if (num > buyPrice)
					{
						itemCountPair = new ItemCountPair(inventoryItem, 1);
						num -= buyPrice;
						flag = true;
					}
				}
			}
			if (!flag && UnityEngine.Random.value < 0.06f)
			{
				List<string> list = new List<string>();
				list.Add("Painting");
				list.Add("Fossil");
				list.Add("Egg");
				list.Add("Saved Record");
				list.Add("Bonsai Tree");
				string text = list[UnityEngine.Random.Range(0, list.Count)];
				ExtraInventoryData extraInventoryData2 = new ExtraInventoryData();
				ResolveLootExtraData(text, extraInventoryData2, original_chest_item);
				if (inventory_ctr.Instance.IsItemPaintable(text) && ShouldPaintLoot(text) && UnityEngine.Random.value < odds_painted_loot)
				{
					extraInventoryData2.SetString("paint", paintbrushes_loot[UnityEngine.Random.Range(0, paintbrushes_loot.Count)].item_name);
				}
				InventoryItem inventoryItem2 = new InventoryItem(text, extraInventoryData2);
				int buyPrice2 = MerchantControl.Instance.GetBuyPrice(inventoryItem2);
				if (num > buyPrice2)
				{
					itemCountPair = new ItemCountPair(inventoryItem2, 1);
					num -= buyPrice2;
					flag = true;
				}
			}
			if (!flag)
			{
				int num7 = (int)UnityEngine.Random.Range(num5 / num4, num5 / (float)max_slots + 1f);
				int num8 = num - num7;
				if (num8 <= 0)
				{
					num8 = 0;
					num7 = num;
				}
				itemCountPair = GetSingleLoot(num7);
				num = num8;
			}
			int slot_id = permittedContainerSlots[num6];
			if (itemCountPair != null)
			{
				contents[slot_id] = itemCountPair;
			}
		}
		while (num > 0 && ++num6 < max_slots);
	}

	public BasketContents GenerateLootChest(InventoryItem original_chest_item)
	{
		BasketContents basketContents = new BasketContents();
		string text;
		if (original_chest_item.item_name == "Sky Chest")
		{
			text = ((!(UnityEngine.Random.value < 0.5f)) ? "Titanium Chest" : "Gold Chest");
		}
		else if (original_chest_item.item_name == "Loot Basket")
		{
			float value = UnityEngine.Random.value;
			text = ((value < 0.333f) ? "Gold Chest" : ((!(value < 0.666f)) ? "Cave Basket" : "Cave Chest"));
		}
		else
		{
			text = ((!(original_chest_item.item_name == "Loot Chest")) ? original_chest_item.item_name : "Titanium Chest");
		}
		switch (text)
		{
		case "Gold Chest":
		{
			float value2 = UnityEngine.Random.value;
			if (value2 < 0.75f)
			{
				AddManyLoots(100, 180, basketContents, 2, 3, text, original_chest_item);
			}
			else if (value2 < 0.97f)
			{
				AddManyLoots(200, 500, basketContents, 2, 4, text, original_chest_item);
			}
			else
			{
				AddManyLoots(500, 1500, basketContents, 2, 3, text, original_chest_item);
			}
			break;
		}
		case "Titanium Chest":
		{
			float value3 = UnityEngine.Random.value;
			if (value3 < 0.9f)
			{
				AddManyLoots(400, 1100, basketContents, 2, 3, text, original_chest_item);
			}
			else if (value3 < 0.985f)
			{
				AddManyLoots(1100, 5000, basketContents, 2, 4, text, original_chest_item);
			}
			else
			{
				AddManyLoots(5000, 500000, basketContents, 1, 3, text, original_chest_item);
			}
			break;
		}
		case "Boss Chest":
		{
			float value4 = UnityEngine.Random.value;
			if (value4 < 0.75f)
			{
				Debug.Log("Boss Chest - Normal");
				AddManyLoots(3000, 4500, basketContents, 8, 8, text, original_chest_item);
			}
			else if (value4 < 0.985f)
			{
				Debug.Log("Boss Chest - Rare");
				AddManyLoots(4500, 6500, basketContents, 8, 8, text, original_chest_item);
			}
			else
			{
				Debug.Log("Boss Chest - Super Rare");
				AddManyLoots(6500, 500000, basketContents, 5, 5, text, original_chest_item);
			}
			break;
		}
		case "Cave Chest":
			if (UnityEngine.Random.value < 0.666f)
			{
				AddManyLoots(50, 150, basketContents, 2, 4, text, original_chest_item);
			}
			else
			{
				AddManyLoots(100, 250, basketContents, 2, 4, text, original_chest_item);
			}
			break;
		case "Cave Basket":
			if (UnityEngine.Random.value < 0.666f)
			{
				AddManyLoots(1, 50, basketContents, 2, 4, text, original_chest_item);
			}
			else
			{
				AddManyLoots(50, 125, basketContents, 2, 4, text, original_chest_item);
			}
			break;
		}
		return basketContents;
	}

	public ItemCountPair GetRelevantItemFromLootList(int target_price, List<item_price_pair> original_list, bool paint_all, int force_count = -1, List<InventoryItem> ignore_items = null)
	{
		List<generated_loot> list = new List<generated_loot>();
		foreach (item_price_pair original in original_list)
		{
			generated_loot item = default(generated_loot);
			item.price = original.base_price;
			ExtraInventoryData extraInventoryData = new ExtraInventoryData();
			if (original.paintable && paint_all)
			{
				item_price_pair item_price_pair = paintbrushes_loot[UnityEngine.Random.Range(0, paintbrushes_loot.Count)];
				extraInventoryData.SetString("paint", item_price_pair.item_name);
				item.price += item_price_pair.base_price;
			}
			item.item = new InventoryItem(original.item_name, extraInventoryData);
			if (force_count == -1)
			{
				if (original.max_stack != 1)
				{
					item.count = UnityEngine.Random.Range(0, original.max_stack);
					item.price *= item.count;
				}
				else
				{
					item.count = 1;
				}
			}
			else
			{
				item.count = force_count;
			}
			if (ignore_items != null && ignore_items.Contains(item.item))
			{
				continue;
			}
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				if (item.price < list[i].price)
				{
					list.Insert(i, item);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		List<generated_loot> list2 = new List<generated_loot>();
		List<generated_loot> list3 = new List<generated_loot>();
		List<generated_loot> list4 = new List<generated_loot>();
		gen_result_t gen_result_t = gen_result_t.range2_upperNotFound;
		for (int j = 0; j < list.Count; j++)
		{
			generated_loot item2 = list[j];
			if (item2.price == -1)
			{
				continue;
			}
			switch (gen_result_t)
			{
			case gen_result_t.exact:
				if (item2.price == target_price)
				{
					list2.Add(item2);
					break;
				}
				if (item2.price > target_price)
				{
					return PickFromSingleList(list2);
				}
				break;
			case gen_result_t.range2_upperFound:
				if (item2.price != list4[0].price)
				{
					if (list3.Count == 0)
					{
						return PickFromSingleList(list4);
					}
					return PickFromMultiList(list3, list4, target_price);
				}
				list4.Add(item2);
				break;
			case gen_result_t.range2_upperNotFound:
				if (item2.price > target_price)
				{
					list4.Add(item2);
					gen_result_t = gen_result_t.range2_upperFound;
				}
				else if (item2.price == target_price)
				{
					list2.Add(item2);
					gen_result_t = gen_result_t.exact;
				}
				else
				{
					if (list3.Count != 0 && list3[0].price < item2.price)
					{
						list3.Clear();
					}
					list3.Add(item2);
				}
				break;
			}
		}
		switch (gen_result_t)
		{
		case gen_result_t.exact:
			return PickFromSingleList(list2);
		case gen_result_t.range2_upperFound:
			if (list3.Count == 0)
			{
				return PickFromSingleList(list4);
			}
			return PickFromMultiList(list3, list4, target_price);
		default:
			if (list4.Count != 0)
			{
				return PickFromMultiList(list3, list4, target_price);
			}
			return PickFromSingleList(list3);
		}
	}

	private ItemCountPair PickFromMultiList(List<generated_loot> low, List<generated_loot> high, int target_price)
	{
		float num = low[0].price;
		float num2 = high[0].price;
		float num3 = 0f;
		if (num != num2)
		{
			float num4 = ((float)target_price - num) / (num2 - num);
			if (num4 >= 0f)
			{
				num3 = ((num4 > 1f) ? 1f : num4);
			}
		}
		return PickFromSingleList((UnityEngine.Random.value >= 1f - num3) ? high : low);
	}

	private ItemCountPair PickFromSingleList(List<generated_loot> list)
	{
		int index = ((list.Count == 1) ? 0 : UnityEngine.Random.Range(0, list.Count));
		return new ItemCountPair(list[index].item, list[index].count);
	}
}
