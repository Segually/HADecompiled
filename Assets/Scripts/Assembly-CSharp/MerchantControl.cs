using System.Collections.Generic;
using UnityEngine;

public class MerchantControl : MonoBehaviour, OrderedStart
{
	public static MerchantControl Instance;

	private Dictionary<string, List<LootControl.item_price_pair>> vendor_categories;

	private float odds_painted_sellItem;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	private bool ShouldExcludeFromMerchants(string item_name)
	{
		return false;
	}

	public void DrawMerchantSlot(int slot_id, int index)
	{
	}

	public ItemCountPair[] GenerateRandomSellList(string merchant_type)
	{
		return null;
	}

	private int GetNumSell(string item_name)
	{
		return 0;
	}

	public List<string> GetAllItemsFromCategory(string category_name)
	{
		return null;
	}

	public new_craft_list GetSellList()
	{
		return null;
	}

	public List<string> GetHardCodedItems(string merchant_type)
	{
		return null;
	}

	public List<string> GetAllCategories(string merchant_type)
	{
		return null;
	}

	public List<string> GetAllBuybackItems()
	{
		return null;
	}

	public bool IsItemSellable(InventoryItem item)
	{
		return false;
	}

	public int GetSellPrice(InventoryItem item)
	{
		return 0;
	}

	public float GetSellPercent(string merchant_type)
	{
		return 0f;
	}

	public int GetBuyPrice(InventoryItem item, bool apply_craft_bonus = true)
	{
		return 0;
	}
}
