using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MerchantControl : MonoBehaviour, OrderedStart
{
	public static MerchantControl Instance;

	private Dictionary<string, List<LootControl.item_price_pair>> vendor_categories = new Dictionary<string, List<LootControl.item_price_pair>>();

	private float odds_painted_sellItem = 0.66f;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	private bool ShouldExcludeFromMerchants(string item_name)
	{
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "exclude_from_vendors") == "true";
	}

	public void DrawMerchantSlot(int slot_id, int index)
	{
		InventoryItem item = inventory_ctr.Instance.curr_crafting_list.items[index].item;
		int count = inventory_ctr.Instance.curr_crafting_list.items[index].count;
		string title_str;
		string desc;
		if (item.item_name == "Saved Record")
		{
			short @short = item.GetShort("npc_record_index");
			title_str = DevBuildControl.Instance.NPC_musicboxes[@short].sale_name;
			desc = inventory_ctr.Instance.GetItemDescription(item.item_name);
		}
		else if (item.item_name == "Painting")
		{
			short short2 = item.GetShort("dev_painting_id");
			title_str = DevBuildControl.Instance.NPC_paintings[short2].name;
			string creator = DevBuildControl.Instance.NPC_paintings[short2].creator;
			desc = (Startup.StringNullOrWhitespace(creator) ? "???" : ("Created by '" + creator + "'"));
		}
		else
		{
			title_str = inventory_ctr.Instance.GetFullItemName(item);
			desc = inventory_ctr.Instance.GetItemDescription(item.item_name);
		}
		float canvas_alpha = ((count < 1) ? 0.5f : 1f);
		inventory_ctr.Instance.instantiated_crafting_slots[slot_id].LayOutCraftingSlot(title_str, desc, canvas_alpha, false, false, true, true, false, CraftingSlot.req_placement.disable, CraftingSlot.text_area_layout.condensed_merchant, CraftingSlot.slots_positioning.up_and_squashed, false);
		CraftingSlot craftingSlot = inventory_ctr.Instance.instantiated_crafting_slots[slot_id];
		ItemSprite buy_ico = craftingSlot.buy_ico;
		Text buy_cost = craftingSlot.buy_cost;
		int buyPrice = GetBuyPrice(item);
		buy_ico.RedrawBasicHideCount(new InventoryItem("Coins"), buyPrice);
		buy_cost.text = string.Format("{0:n0}", buyPrice);
		if (buyPrice < 10)
		{
			buy_cost.fontSize = 50;
			buy_cost.transform.localPosition = new Vector3(42.4f, buy_cost.transform.localPosition.y, buy_cost.transform.localPosition.z);
			buy_ico.transform.localPosition = new Vector3(-30f, buy_ico.transform.localPosition.y, buy_ico.transform.localPosition.z);
		}
		else if (buyPrice < 100)
		{
			buy_cost.fontSize = 48;
			buy_cost.transform.localPosition = new Vector3(35.1f, buy_cost.transform.localPosition.y, buy_cost.transform.localPosition.z);
			buy_ico.transform.localPosition = new Vector3(-33.8f, buy_ico.transform.localPosition.y, buy_ico.transform.localPosition.z);
		}
		else if (buyPrice < 1000)
		{
			buy_cost.fontSize = 46;
			buy_cost.transform.localPosition = new Vector3(24.1f, buy_cost.transform.localPosition.y, buy_cost.transform.localPosition.z);
			buy_ico.transform.localPosition = new Vector3(-45.8f, buy_ico.transform.localPosition.y, buy_ico.transform.localPosition.z);
		}
		else if (buyPrice < 10000)
		{
			buy_cost.fontSize = 45;
			buy_cost.transform.localPosition = new Vector3(15.3f, buy_cost.transform.localPosition.y, buy_cost.transform.localPosition.z);
			buy_ico.transform.localPosition = new Vector3(-54f, buy_ico.transform.localPosition.y, buy_ico.transform.localPosition.z);
		}
		else
		{
			buy_cost.fontSize = 43;
			buy_cost.transform.localPosition = new Vector3(5.9f, buy_cost.transform.localPosition.y, buy_cost.transform.localPosition.z);
			buy_ico.transform.localPosition = new Vector3(-64.2f, buy_ico.transform.localPosition.y, buy_ico.transform.localPosition.z);
		}
		craftingSlot.result_sprite.RedrawAsBuyItem(item, count);
	}

	public ItemCountPair[] GenerateRandomSellList(string merchant_type)
	{
		List<InventoryItem> list = new List<InventoryItem>();
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("VendorDefines/" + merchant_type, ref file_exists);
		if (file_exists)
		{
			for (int i = 0; i < textFileLines.Count; i++)
			{
				string text = textFileLines[i];
				if (Startup.StringNullOrWhitespace(text) || text.Contains("Sell Percent"))
				{
					continue;
				}
				InventoryItem inventoryItem;
				if (!text.Contains("\""))
				{
					int num = text.IndexOf("(");
					string text2 = text.Substring(0, num);
					string text3 = text.Substring(num + 1, text.Length - num - 2);
					int num2 = text3.IndexOf("-");
					int minInclusive = int.Parse(text3.Substring(0, num2), Startup.parse_culture);
					int maxExclusive = int.Parse(text3.Substring(num2 + 1, text3.Length - (num2 + 1)), Startup.parse_culture);
					if (!vendor_categories.ContainsKey(text2) && text2 != "Random")
					{
						List<LootControl.item_price_pair> list2 = new List<LootControl.item_price_pair>();
						LootControl.Instance.LoadLootFile("(Auto Gen) VENDOR_" + text2, list2);
						vendor_categories.Add(text2, list2);
					}
					int num3 = UnityEngine.Random.Range(minInclusive, maxExclusive);
					inventoryItem = null;
					for (int j = 0; j < 6; j++)
					{
						bool paint_all = UnityEngine.Random.value < odds_painted_sellItem;
						ItemCountPair itemCountPair = ((!(text2 != "Random")) ? LootControl.Instance.GetSingleLoot(num3, 1, false) : LootControl.Instance.GetRelevantItemFromLootList(num3, vendor_categories[text2], paint_all, -1, list));
						if (itemCountPair == null)
						{
							if (i != 5 || !(text2 != "Random"))
							{
								continue;
							}
							itemCountPair = LootControl.Instance.GetRelevantItemFromLootList(num3, vendor_categories[text2], paint_all);
							if (itemCountPair == null)
							{
								continue;
							}
						}
						else if (ShouldExcludeFromMerchants(itemCountPair.item.item_name))
						{
							continue;
						}
						ExtraInventoryData extraDataCopy = itemCountPair.item.GetExtraDataCopy();
						LootControl.Instance.ResolveLootExtraData(itemCountPair.item.item_name, extraDataCopy);
						inventoryItem = new InventoryItem(itemCountPair.item.item_name, extraDataCopy);
						break;
					}
					if (inventoryItem == null)
					{
						inventoryItem = new InventoryItem("Snowball");
					}
				}
				else
				{
					string text4 = text.Replace("\"", "").Replace("\"", "");
					inventoryItem = null;
					for (int k = 0; k < 6; k++)
					{
						ExtraInventoryData extraInventoryData = new ExtraInventoryData();
						LootControl.Instance.ResolveLootExtraData(text4, extraInventoryData, new InventoryItem(""));
						if (inventory_ctr.Instance.IsItemPaintable(text4) && LootControl.Instance.ShouldPaintLoot(text4) && UnityEngine.Random.value < odds_painted_sellItem)
						{
							extraInventoryData.SetString("paint", LootControl.Instance.paintbrushes_loot[UnityEngine.Random.Range(0, LootControl.Instance.paintbrushes_loot.Count)].item_name);
						}
						InventoryItem inventoryItem2 = new InventoryItem(text4, extraInventoryData);
						if (i == 5 || !list.Contains(inventoryItem2))
						{
							inventoryItem = inventoryItem2;
							break;
						}
					}
				}
				list.Add(inventoryItem);
			}
		}
		ItemCountPair[] array = new ItemCountPair[list.Count];
		for (int l = 0; l < list.Count; l++)
		{
			array[l] = new ItemCountPair(list[l], GetNumSell(list[l].item_name));
		}
		return array;
	}

	private int GetNumSell(string item_name)
	{
		int num;
		int num2;
		if (InventoryUtils.IsPaintbrush(item_name))
		{
			num = 3;
			num2 = 5;
		}
		else if (InventoryUtils.IsStamp(item_name))
		{
			num = 1;
			num2 = 2;
		}
		else
		{
			string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item_name, "NumSell");
			if (Startup.StringNullOrWhitespace(stringFromItemFile))
			{
				num2 = 1;
				num = 1;
			}
			else
			{
				int num3 = stringFromItemFile.IndexOf(" to ");
				if (num3 == -1)
				{
					num = int.Parse(stringFromItemFile);
					num2 = int.Parse(stringFromItemFile);
				}
				else
				{
					num = int.Parse(stringFromItemFile.Substring(0, num3), Startup.parse_culture);
					num2 = int.Parse(stringFromItemFile.Substring(num3 + 4, stringFromItemFile.Length - (num3 + 4)), Startup.parse_culture);
				}
			}
		}
		int num4 = UnityEngine.Random.Range(num, num2 + 1);
		if (num4 > num2)
		{
			num4 = num2;
		}
		return num4;
	}

	public List<string> GetAllItemsFromCategory(string category_name)
	{
		if (!vendor_categories.ContainsKey(category_name))
		{
			List<LootControl.item_price_pair> list = new List<LootControl.item_price_pair>();
			LootControl.Instance.LoadLootFile("(Auto Gen) VENDOR_" + category_name, list);
			vendor_categories.Add(category_name, list);
		}
		List<string> list2 = new List<string>();
		foreach (LootControl.item_price_pair item in vendor_categories[category_name])
		{
			list2.Add(item.item_name);
		}
		return list2;
	}

	public new_craft_list GetSellList()
	{
		InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
		string @string = interacting_element_item.GetString("merchant_type");
		new_craft_list new_craft_list = new new_craft_list();
		if (!interacting_element_item.IsRespawnExpired(@string))
		{
			new_craft_list.items = ChunkControl.Instance.GetItemListFromItem("sell_list_" + @string, interacting_element_item);
		}
		else
		{
			new_craft_list.items = GenerateRandomSellList(@string);
			DateTime uTC_when = DateTime.UtcNow.AddSeconds(0.0).AddHours(2.0);
			InventoryItem old_item = ChunkControl.Instance.EncodeItemListIntoItem("sell_list_" + @string, new_craft_list.items, interacting_element_item);
			ConstructionControl.Instance.PlayerReplaceInteracting(ChunkControl.Instance.EncodeRespawnIntoItem(@string, old_item, uTC_when), true);
		}
		return new_craft_list;
	}

	public List<string> GetHardCodedItems(string merchant_type)
	{
		List<string> list = new List<string>();
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("VendorDefines/" + merchant_type, ref file_exists);
		if (file_exists)
		{
			for (int i = 0; i < textFileLines.Count; i++)
			{
				string text = textFileLines[i];
				if (!Startup.StringNullOrWhitespace(text) && !text.Contains("Sell Percent") && text.Contains("\""))
				{
					string item = text.Replace("\"", "").Replace("\"", "");
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
		}
		return list;
	}

	public List<string> GetAllCategories(string merchant_type)
	{
		List<string> list = new List<string>();
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("VendorDefines/" + merchant_type, ref file_exists);
		if (file_exists)
		{
			for (int i = 0; i < textFileLines.Count; i++)
			{
				string text = textFileLines[i];
				if (!Startup.StringNullOrWhitespace(text) && !text.Contains("Sell Percent") && !text.Contains("\""))
				{
					string item = text.Substring(0, text.IndexOf("("));
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
		}
		return list;
	}

	public List<string> GetAllBuybackItems()
	{
		List<string> list = new List<string>();
		string @string = GameController.Instance.interacting_element_item.GetString("merchant_type");
		foreach (string hardCodedItem in GetHardCodedItems(@string))
		{
			list.Add(hardCodedItem);
		}
		foreach (string allCategory in GetAllCategories(@string))
		{
			foreach (string item in GetAllItemsFromCategory(allCategory))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public bool IsItemSellable(InventoryItem item)
	{
		_ = item.item_name;
		string @string = GameController.Instance.interacting_element_item.GetString("merchant_type");
		if (@string == "BuyerOnly" || @string == "BuyerOnly 2" || @string == "BuyerOnly 3" || @string == "Random")
		{
			return GetBuyPrice(item) != 0;
		}
		return inventory_ctr.Instance.curr_buyback_list.Contains(item.item_name);
	}

	public int GetSellPrice(InventoryItem item)
	{
		string @string = GameController.Instance.interacting_element_item.GetString("merchant_type");
		InventoryItem item2 = item;
		if (item.item_name == "Painting")
		{
			item2 = new InventoryItem((item.GetShort("dev_painting_id") != 0) ? "Painting" : "Blank Canvas");
		}
		int num = (int)(GetSellPercent(@string) * (float)GetBuyPrice(item2));
		if (num == 0)
		{
			return 1;
		}
		if (GetBuyPrice(item) < num)
		{
			return GetBuyPrice(item);
		}
		return num;
	}

	public float GetSellPercent(string merchant_type)
	{
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("VendorDefines/" + merchant_type, ref file_exists);
		if (!file_exists)
		{
			return 0f;
		}
		foreach (string item in textFileLines)
		{
			if (!Startup.StringNullOrWhitespace(item) && item.Contains("Sell Percent"))
			{
				int num = item.IndexOf('=');
				return float.Parse(item.Substring(num + 2, item.Length - num - 3), Startup.parse_culture);
			}
		}
		return 0f;
	}

	public int GetBuyPrice(InventoryItem item, bool apply_craft_bonus = true)
	{
		string item_name = item.item_name;
		string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item_name, "Market Cost");
		int num;
		if (stringFromItemFile == "CALCULATE")
		{
			string itemCraftingIngredientA = inventory_ctr.Instance.GetItemCraftingIngredientA(item_name);
			int itemCraftingIngredientA_count = inventory_ctr.Instance.GetItemCraftingIngredientA_count(item_name);
			string itemCraftingIngredientB = inventory_ctr.Instance.GetItemCraftingIngredientB(item_name);
			int itemCraftingIngredientB_count = inventory_ctr.Instance.GetItemCraftingIngredientB_count(item_name);
			bool flag = itemCraftingIngredientB != "";
			int buyPrice = GetBuyPrice(new InventoryItem(itemCraftingIngredientA), false);
			float num2 = ((itemCraftingIngredientB_count != 0 && flag) ? ((float)(buyPrice * itemCraftingIngredientA_count + GetBuyPrice(new InventoryItem(itemCraftingIngredientB), false) * itemCraftingIngredientB_count)) : ((float)(buyPrice * itemCraftingIngredientA_count)));
			int item_nCraft = inventory_ctr.Instance.GetItem_nCraft(item_name);
			num = (apply_craft_bonus ? ((int)((num2 * (float)inventory_ctr.Instance.GetItemCraftingLevelRequired(item_name) * inventory_ctr.crafting_bonus_multiplier + num2) / (float)item_nCraft)) : ((int)(num2 / (float)item_nCraft)));
		}
		else
		{
			num = ((!Startup.StringNullOrWhitespace(stringFromItemFile)) ? int.Parse(stringFromItemFile, Startup.parse_culture) : 1);
		}
		if (item.item_name != "Book")
		{
			string @string = item.GetString("paint");
			if (@string != "")
			{
				num += GetBuyPrice(new InventoryItem(@string));
			}
			string string2 = item.GetString("stamp");
			if (string2 != "")
			{
				num += GetBuyPrice(new InventoryItem(string2));
			}
		}
		if (item.item_name == "Bonsai")
		{
			num += inventory_ctr.Instance.BonsaiBonus(item.GetShort("bonsai_age"));
		}
		if (num == 0)
		{
			num = 1;
		}
		return num;
	}
}
