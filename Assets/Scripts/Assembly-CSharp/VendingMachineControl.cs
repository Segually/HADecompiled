using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class VendingMachineControl : MonoBehaviour
{
	public static VendingMachineControl Instance;

	public AudioClip sfx_open;

	public AudioClip sfx_insert_item;

	public AudioClip sfx_incorrect;

	public TextMeshProUGUI text_items_and_profits;

	public TextMeshProUGUI set_price_screen_header;

	public TextMeshProUGUI set_price_text_above_input;

	public TextMeshProUGUI set_price_accept;

	public GameObject[] nibs;

	public GameObject bottom_buttons;

	public GameObject back_button;

	public GameObject main_window;

	public GameObject additem_window;

	public GameObject additem_visual_slot;

	public TMP_InputField input_additem_price_count;

	public TMP_InputField input_additem_price_item;

	public TextMeshProUGUI header;

	public GameObject angular;

	public bool pick_item_screen_open;

	private int max_price = 300000;

	private int clicked_vending_machine_index;

	private int insert_inventory_index;

	public void ClickAutoFill1()
	{
		string text = GetComponent<AutoComplete>().nib_0.transform.Find("Text").GetComponent<TextMeshProUGUI>().text;
		GetComponent<AutoComplete>().HideNibsAndDisableTypingUpdates();
		input_additem_price_item.SetTextWithoutNotify(text);
		UpdatedCostInput();
	}

	public void ClickAutoFill2()
	{
		string text = GetComponent<AutoComplete>().nib_1.transform.Find("Text").GetComponent<TextMeshProUGUI>().text;
		GetComponent<AutoComplete>().HideNibsAndDisableTypingUpdates();
		input_additem_price_item.SetTextWithoutNotify(text);
		UpdatedCostInput();
	}

	public void OnOpen()
	{
		AudioControl.Instance.PlayPitch(sfx_open, Random.Range(0.95f, 1.03f), 0.2f);
		header.text = TranslationControl.Instance.TranslateGeneral("ITEMS FOR SALE", "VendingMachines");
		text_items_and_profits.text = TranslationControl.Instance.TranslateGeneral("ITEMS", "VendingMachines") + " & " + TranslationControl.Instance.TranslateGeneral("PROFITS", "VendingMachines");
		GetComponent<AutoComplete>().LoadList();
		main_window.SetActive(true);
		additem_window.SetActive(false);
		bottom_buttons.SetActive(true);
		back_button.SetActive(false);
		RedrawAllSlots(false);
	}

	private void RedrawAllSlots(bool edit_mode)
	{
		InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
		ItemCountPair[] itemListFromItem = ChunkControl.Instance.GetItemListFromItem("vending_machine_for_sale", interacting_element_item);
		ItemCountPair[] itemListFromItem2 = ChunkControl.Instance.GetItemListFromItem("vending_machine_costs", interacting_element_item);
		for (int i = 0; i < 10; i++)
		{
			ItemCountPair sell_item;
			ItemCountPair itemCountPair;
			int count;
			if (i < itemListFromItem.Length)
			{
				sell_item = itemListFromItem[i];
				itemCountPair = itemListFromItem2[i];
				count = interacting_element_item.GetLong("item_wanted_count_" + i);
			}
			else
			{
				sell_item = new ItemCountPair("", 0);
				itemCountPair = new ItemCountPair("", 0);
				count = 0;
			}
			bool sold = interacting_element_item.GetShort("sold_" + i) == 1;
			RedrawItemSlot(nibs[i], sell_item, new ItemCountPair(itemCountPair.item, count), sold, edit_mode);
		}
	}

	private void RedrawItemSlot(GameObject nib, ItemCountPair sell_item, ItemCountPair cost, bool sold, bool edit_mode)
	{
		bool flag = sell_item.item.item_name == "";
		GameObject gameObject = nib.transform.Find("None").gameObject;
		if (flag)
		{
			if (!edit_mode)
			{
				gameObject.SetActive(true);
				nib.transform.Find("Add-Item").gameObject.SetActive(false);
				nib.transform.Find("For-Sale").gameObject.SetActive(false);
				nib.transform.Find("Sold-Out").gameObject.SetActive(false);
				nib.transform.Find("Remove-Overlay").gameObject.SetActive(false);
				nib.transform.Find("Collect-Overlay").gameObject.SetActive(false);
			}
			else
			{
				gameObject.SetActive(false);
				nib.transform.Find("Add-Item").gameObject.SetActive(true);
				nib.transform.Find("For-Sale").gameObject.SetActive(false);
				nib.transform.Find("Sold-Out").gameObject.SetActive(false);
				nib.transform.Find("Remove-Overlay").gameObject.SetActive(false);
				nib.transform.Find("Collect-Overlay").gameObject.SetActive(false);
				nib.transform.Find("Add-Item").Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = TranslationControl.Instance.TranslateGeneral("ADD ITEM", "VendingMachines");
			}
			return;
		}
		gameObject.SetActive(false);
		nib.transform.Find("Add-Item").gameObject.SetActive(false);
		nib.transform.Find("For-Sale").gameObject.SetActive(true);
		CanvasGroup component = nib.transform.Find("For-Sale").GetComponent<CanvasGroup>();
		if (!sold)
		{
			component.alpha = 1f;
			nib.transform.Find("Sold-Out").gameObject.SetActive(false);
			if (!edit_mode)
			{
				nib.transform.Find("Remove-Overlay").gameObject.SetActive(false);
			}
			else
			{
				nib.transform.Find("Remove-Overlay").gameObject.SetActive(true);
				nib.transform.Find("Remove-Overlay").Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = TranslationControl.Instance.TranslateGeneral("TAKE ITEM", "VendingMachines");
			}
			nib.transform.Find("Collect-Overlay").gameObject.SetActive(false);
		}
		else if (!edit_mode)
		{
			component.alpha = 0.35f;
			nib.transform.Find("Sold-Out").gameObject.SetActive(true);
			nib.transform.Find("Remove-Overlay").gameObject.SetActive(false);
			nib.transform.Find("Collect-Overlay").gameObject.SetActive(false);
			nib.transform.Find("Sold-Out").GetComponent<TextMeshProUGUI>().text = TranslationControl.Instance.TranslateGeneral("SOLD OUT", "VendingMachines");
		}
		else
		{
			component.alpha = 1f;
			nib.transform.Find("Sold-Out").gameObject.SetActive(false);
			nib.transform.Find("Remove-Overlay").gameObject.SetActive(false);
			nib.transform.Find("Collect-Overlay").gameObject.SetActive(true);
			nib.transform.Find("Collect-Overlay").Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = TranslationControl.Instance.TranslateGeneral("COLLECT PROFIT", "VendingMachines");
		}
		if (!(sell_item.item.item_name != "" && sold && edit_mode))
		{
			nib.transform.Find("For-Sale").Find("item name").GetComponent<TextMeshProUGUI>().text = inventory_ctr.Instance.GetFullItemName(sell_item.item);
			nib.transform.Find("For-Sale").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(sell_item.item, sell_item.count);
		}
		else
		{
			nib.transform.Find("For-Sale").Find("item name").GetComponent<TextMeshProUGUI>().text = inventory_ctr.Instance.GetFullItemName(cost.item);
			nib.transform.Find("For-Sale").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(cost.item, cost.count);
		}
		GameObject gameObject2 = nib.transform.Find("For-Sale").Find("cost").gameObject;
		GameObject gameObject3 = nib.transform.Find("For-Sale").Find("cost sprite").gameObject;
		gameObject2.GetComponent<TextMeshProUGUI>().text = "x" + cost.count;
		gameObject3.GetComponent<ItemSprite>().RedrawAsVendingMachineCost(cost.item, cost.count);
		int count = cost.count;
		if (count < 10)
		{
			gameObject2.transform.localPosition = new Vector3(36.3f, gameObject2.transform.localPosition.y, 0f);
			gameObject3.transform.localPosition = new Vector3(-26.1f, gameObject3.transform.localPosition.y, 0f);
		}
		else if (count < 100)
		{
			gameObject2.transform.localPosition = new Vector3(39.3f, gameObject2.transform.localPosition.y, 0f);
			gameObject3.transform.localPosition = new Vector3(-33.4f, gameObject3.transform.localPosition.y, 0f);
		}
		else if (count < 1000)
		{
			gameObject2.transform.localPosition = new Vector3(38.6f, gameObject2.transform.localPosition.y, 0f);
			gameObject3.transform.localPosition = new Vector3(-42f, gameObject3.transform.localPosition.y, 0f);
		}
		else if (count >= 10000)
		{
			gameObject2.transform.localPosition = new Vector3(38.6f, gameObject2.transform.localPosition.y, 0f);
			gameObject3.transform.localPosition = new Vector3(-56f, gameObject3.transform.localPosition.y, 0f);
			gameObject2.GetComponent<TextMeshProUGUI>().fontSize = 35f;
			return;
		}
		else
		{
			gameObject2.transform.localPosition = new Vector3(38f, gameObject2.transform.localPosition.y, 0f);
			gameObject3.transform.localPosition = new Vector3(-54.8f, gameObject3.transform.localPosition.y, 0f);
		}
		gameObject2.GetComponent<TextMeshProUGUI>().fontSize = 40f;
	}

	private ItemCountPair GetForSaleAt(int index)
	{
		InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
		ItemCountPair[] itemListFromItem = ChunkControl.Instance.GetItemListFromItem("vending_machine_for_sale", interacting_element_item);
		InventoryItem item_;
		int count_;
		if (index < itemListFromItem.Length)
		{
			item_ = itemListFromItem[index].item;
			count_ = itemListFromItem[index].count;
		}
		else
		{
			item_ = new InventoryItem("");
			count_ = 0;
		}
		return new ItemCountPair(item_, count_);
	}

	private ItemCountPair GetPriceAt(int index)
	{
		InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
		ItemCountPair[] itemListFromItem = ChunkControl.Instance.GetItemListFromItem("vending_machine_costs", interacting_element_item);
		InventoryItem item_;
		int count_;
		if (index < itemListFromItem.Length)
		{
			item_ = itemListFromItem[index].item;
			count_ = interacting_element_item.GetLong("item_wanted_count_" + index);
		}
		else
		{
			item_ = new InventoryItem("");
			count_ = 0;
		}
		return new ItemCountPair(item_, count_);
	}

	public void PressBuyItem(int index)
	{
		clicked_vending_machine_index = index;
		ItemCountPair forSaleAt = GetForSaleAt(index);
		ItemCountPair priceAt = GetPriceAt(index);
		string fullItemName = inventory_ctr.Instance.GetFullItemName(forSaleAt.item);
		if (GameController.Instance.interacting_element_item.GetShort("sold_" + clicked_vending_machine_index) == 1)
		{
			string tEXT = ((forSaleAt.count != 1) ? ("<color=#eeee55>x" + forSaleAt.count + " " + fullItemName + "</color>\n" + TranslationControl.Instance.TranslateGeneral("Out of stock, come back another time!", "VendingMachines")) : ("<color=#eeee55>" + fullItemName + "</color>\n" + TranslationControl.Instance.TranslateGeneral("Out of stock, come back another time!", "VendingMachines")));
			ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, tEXT, new Color(0.74509805f, 0.74509805f, 0.74509805f, 1f), forSaleAt.item, forSaleAt.count, new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f), ShopControl.popup_context.vending_machine, -1, null, -1, null, -1, ShopControl.req_context_t.none);
			return;
		}
		int playerItemCount = inventory_ctr.Instance.GetPlayerItemCount(priceAt.item.item_name);
		if (!inventory_ctr.Instance.CanReceiveItem(forSaleAt.item, 1))
		{
			string tEXT2 = ((forSaleAt.count != 1) ? ("<color=#eeee55>x" + forSaleAt.count + " " + fullItemName + "</color>\n" + TranslationControl.Instance.TranslateGeneral("Inventory Full!", "GUI")) : ("<color=#eeee55>" + fullItemName + "</color>\n" + TranslationControl.Instance.TranslateGeneral("Inventory Full!", "GUI")));
			ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, tEXT2, new Color(0.9411765f, 0.3019608f, 0.3019608f, 1f), forSaleAt.item, forSaleAt.count, new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f), ShopControl.popup_context.vending_machine, -1, null, -1, null, -1, ShopControl.req_context_t.none);
		}
		else if (playerItemCount >= priceAt.count)
		{
			string text;
			if (forSaleAt.count == 1)
			{
				text = TranslationControl.Instance.TranslateGeneral("Buy XYZ for", "VendingMachines");
			}
			else
			{
				text = TranslationControl.Instance.TranslateGeneral("Buy ABC XYZ for", "VendingMachines");
				text = text.Replace("ABC", "<color=#eeee55>" + forSaleAt.count + "</color>");
			}
			text = text.Replace("XYZ", "<color=#eeee55>" + fullItemName + "</color>");
			ShopControl.Instance.ShowShopPopup("YES", ShopControl.button_color_t.yes_green, "CANCEL", ShopControl.button_color_t.no_red, false, text, new Color(1f, 1f, 1f, 1f), forSaleAt.item, forSaleAt.count, new Color(0.29803923f, 0.50980395f, 0.5294118f, 1f), ShopControl.popup_context.vending_machine, -1, priceAt.item, priceAt.count, null, -1, ShopControl.req_context_t.inquire);
		}
		else
		{
			string tEXT3 = "<color=#eeee55>" + TranslationControl.Instance.TranslateGeneral("To buy this, you need", "VendingMachines") + ":</color>";
			ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, tEXT3, new Color(0.9411765f, 0.3019608f, 0.3019608f, 1f), forSaleAt.item, forSaleAt.count, new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f), ShopControl.popup_context.vending_machine, -1, priceAt.item, priceAt.count, null, -1, ShopControl.req_context_t.insufficient);
		}
	}

	public void PressAcceptBuy()
	{
		ItemCountPair forSaleAt = GetForSaleAt(clicked_vending_machine_index);
		ItemCountPair priceAt = GetPriceAt(clicked_vending_machine_index);
		inventory_ctr.Instance.GetFullItemName(forSaleAt.item);
		inventory_ctr.Instance.GiveItem(forSaleAt.item, forSaleAt.count, "");
		inventory_ctr.Instance.player_inventory.RemoveItemByName(priceAt.item.item_name, priceAt.count);
		ExtraInventoryData extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
		extraDataCopy.SetShort("sold_" + clicked_vending_machine_index, 1);
		InventoryItem new_item = new InventoryItem("Vending Machine", extraDataCopy);
		ConstructionControl.Instance.PlayerReplaceInteracting(new_item, true);
		RedrawAllSlots(false);
		inventory_ctr.Instance.ShowDelayedBuySuccessNotif(forSaleAt);
	}

	public void PressAddRemoveItems()
	{
		string text = GameController.Instance.interacting_element_item.GetString("vending_machine_owner");
		if (text != PlayerData.Instance.GetGlobalString("username_lower"))
		{
			AudioControl.Instance.PlayPitch(sfx_incorrect, 0.8f, 0.31f);
			PopupControl.Instance.ShowMessage("Only " + text + ", the owner of this Vending Machine, can edit this");
			return;
		}
		AudioControl.Instance.PlayPitch(GameController.Instance.sfx_chestopen, 1f, 0.5f);
		bottom_buttons.SetActive(false);
		back_button.SetActive(true);
		RedrawAllSlots(true);
		header.text = TranslationControl.Instance.TranslateGeneral("ITEMS", "VendingMachines") + " & " + TranslationControl.Instance.TranslateGeneral("PROFITS", "VendingMachines");
	}

	public void PressBackOnAddRemoveItems()
	{
		header.text = TranslationControl.Instance.TranslateGeneral("ITEMS FOR SALE", "VendingMachines");
		bottom_buttons.SetActive(true);
		back_button.SetActive(false);
		RedrawAllSlots(false);
	}

	public void PressAddItem(int index)
	{
		clicked_vending_machine_index = index;
		main_window.SetActive(false);
		WindowPrefabsControl.Instance.CreateScreen("INVENTORY-pickItem", WindowPrefabsControl.build_into_t.mini_window);
		WindowPrefabsControl.Instance.GetTextLegacy("INVENTORY-pickItem", "saveload_text").text = TranslationControl.Instance.TranslateGeneral("Select an item from your inventory to add", "VendingMachines");
		inventory_ctr.Instance.LayOutInvSlots(false, false, inventory_ctr.slots_positionings.show_15_centered, false, inventory_ctr.Instance.NumPlayerPages(), false, "", inventory_ctr.fusion_button.hide);
		inventory_ctr.Instance.RedrawInventorySlots();
		pick_item_screen_open = true;
	}

	public void PressCollectProfit(int index)
	{
		ItemCountPair priceAt = GetPriceAt(index);
		if (!inventory_ctr.Instance.CanReceiveItem(priceAt.item, priceAt.count))
		{
			string fullItemName = inventory_ctr.Instance.GetFullItemName(priceAt.item);
			string tEXT = ((priceAt.count != 1) ? ("<color=#eeee55>x" + priceAt.count + " " + fullItemName + "</color>\n" + TranslationControl.Instance.TranslateGeneral("Inventory Full!", "GUI")) : ("<color=#eeee55>" + fullItemName + "</color>\n" + TranslationControl.Instance.TranslateGeneral("Inventory Full!", "GUI")));
			ShopControl.Instance.ShowShopPopup("OKAY", ShopControl.button_color_t.okay_blue, "", ShopControl.button_color_t.none, false, tEXT, new Color(0.9411765f, 0.3019608f, 0.3019608f, 1f), priceAt.item, priceAt.count, new Color(0.39215687f, 0.39215687f, 0.39215687f, 1f), ShopControl.popup_context.vending_machine, -1, null, -1, null, -1, ShopControl.req_context_t.none);
			return;
		}
		inventory_ctr.Instance.GiveItem(priceAt.item, priceAt.count, "");
		InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
		ItemCountPair[] itemListFromItem = ChunkControl.Instance.GetItemListFromItem("vending_machine_for_sale", interacting_element_item);
		ItemCountPair[] itemListFromItem2 = ChunkControl.Instance.GetItemListFromItem("vending_machine_costs", interacting_element_item);
		itemListFromItem[index] = new ItemCountPair("", 0);
		itemListFromItem2[index] = new ItemCountPair("", 0);
		interacting_element_item = ChunkControl.Instance.EncodeItemListIntoItem("vending_machine_for_sale", itemListFromItem, interacting_element_item);
		ExtraInventoryData extraDataCopy = ChunkControl.Instance.EncodeItemListIntoItem("vending_machine_costs", itemListFromItem2, interacting_element_item).GetExtraDataCopy();
		extraDataCopy.SetShort("sold_" + index, 0);
		extraDataCopy.SetLong("item_wanted_count_" + index, 0);
		InventoryItem new_item = new InventoryItem("Vending Machine", extraDataCopy);
		ConstructionControl.Instance.PlayerReplaceInteracting(new_item, true);
		angular.SetActive(true);
		angular.transform.localPosition = nibs[index].transform.localPosition;
		angular.transform.Find("Image (1)").GetComponent<Image>().color = new Color(1f, 0.97f, 0.6f, 1f);
		angular.GetComponent<Animation>().Play();
		GameController.Instance.sound_ding();
		RedrawAllSlots(true);
	}

	public void PressBackOnSelectItem()
	{
		inventory_ctr.Instance.HideInventoryTab(false);
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-pickItem");
		pick_item_screen_open = false;
		main_window.SetActive(true);
	}

	public void SelectedItem(int inventory_index)
	{
		inventory_ctr.Instance.HideInventoryTab(false);
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-pickItem");
		pick_item_screen_open = false;
		insert_inventory_index = inventory_index;
		InventoryItem item = inventory_ctr.Instance.player_inventory[inventory_index].item;
		int count = inventory_ctr.Instance.player_inventory[inventory_index].count;
		additem_window.SetActive(true);
		int num = MerchantControl.Instance.GetBuyPrice(item) * count;
		if (num < 1)
		{
			num = 1;
		}
		else if (num > max_price)
		{
			num = max_price;
		}
		input_additem_price_count.SetTextWithoutNotify(num.ToString() ?? "");
		input_additem_price_item.SetTextWithoutNotify(inventory_ctr.Instance.GetFullItemName(new InventoryItem("Coins")));
		RedrawItemSlot(additem_visual_slot, new ItemCountPair(item, count), new ItemCountPair("Coins", num), false, false);
		set_price_screen_header.text = TranslationControl.Instance.TranslateGeneral("ADD ITEM", "VendingMachines");
		set_price_accept.text = TranslationControl.Instance.TranslateGeneral("ACCEPT", "VendingMachines");
		set_price_text_above_input.text = TranslationControl.Instance.TranslateGeneral("Set Price", "VendingMachines") + ":";
		GetComponent<AutoComplete>().HideNibsAndDisableTypingUpdates();
	}

	public void TypeCostItemSelected()
	{
		GetComponent<AutoComplete>().EnableTypingUpdates(input_additem_price_item.text);
	}

	public void TypeCostItemDeselected()
	{
	}

	public void UpdatedCostInput()
	{
		int num = int.Parse(input_additem_price_count.text, Startup.parse_culture);
		int count_ = ((num < 1) ? 1 : ((num > max_price) ? max_price : num));
		bool valid_item = false;
		string punctuatedCostItemText = GetPunctuatedCostItemText(ref valid_item);
		InventoryItem item = inventory_ctr.Instance.player_inventory[insert_inventory_index].item;
		int count = inventory_ctr.Instance.player_inventory[insert_inventory_index].count;
		RedrawItemSlot(additem_visual_slot, new ItemCountPair(item, count), new ItemCountPair(valid_item ? punctuatedCostItemText : "unknown", count_), false, false);
		GetComponent<AutoComplete>().TypingDetected(punctuatedCostItemText);
	}

	public void PressRemoveItem(int index)
	{
		InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
		ItemCountPair[] itemListFromItem = ChunkControl.Instance.GetItemListFromItem("vending_machine_for_sale", interacting_element_item);
		ItemCountPair[] itemListFromItem2 = ChunkControl.Instance.GetItemListFromItem("vending_machine_costs", interacting_element_item);
		ItemCountPair itemCountPair = itemListFromItem[index];
		InventoryItem item = itemCountPair.item;
		int count = itemCountPair.count;
		if (!inventory_ctr.Instance.CanReceiveItem(item, count))
		{
			PopupControl.Instance.ShowMessage("Can't take item - Inventory Full!");
			return;
		}
		inventory_ctr.Instance.GiveItem(item, count, "");
		itemListFromItem[index] = new ItemCountPair("", 0);
		itemListFromItem2[index] = new ItemCountPair("", 0);
		interacting_element_item = ChunkControl.Instance.EncodeItemListIntoItem("vending_machine_for_sale", itemListFromItem, interacting_element_item);
		ExtraInventoryData extraDataCopy = ChunkControl.Instance.EncodeItemListIntoItem("vending_machine_costs", itemListFromItem2, interacting_element_item).GetExtraDataCopy();
		extraDataCopy.SetShort("sold_" + index, 0);
		extraDataCopy.SetLong("item_wanted_count_" + index, 0);
		InventoryItem new_item = new InventoryItem("Vending Machine", extraDataCopy);
		ConstructionControl.Instance.PlayerReplaceInteracting(new_item, true);
		RedrawAllSlots(true);
	}

	public string GetPunctuatedCostItemText(ref bool valid_item)
	{
		string key = input_additem_price_item.text.ToLower();
		AutoComplete component = GetComponent<AutoComplete>();
		if (!component.item_list.ContainsKey(key))
		{
			valid_item = false;
			return input_additem_price_item.text;
		}
		valid_item = true;
		return component.item_list[key];
	}

	public void PressAcceptOnAddItem()
	{
		bool valid_item = false;
		string punctuatedCostItemText = GetPunctuatedCostItemText(ref valid_item);
		if (!valid_item)
		{
			PopupControl.Instance.ShowMessage("Invalid item\n'" + input_additem_price_item.text + "' is not an item!");
			return;
		}
		InventoryItem item = inventory_ctr.Instance.player_inventory[insert_inventory_index].item;
		int count = inventory_ctr.Instance.player_inventory[insert_inventory_index].count;
		inventory_ctr.Instance.player_inventory[insert_inventory_index] = new ItemCountPair("", 0);
		int num = int.Parse(input_additem_price_count.text, Startup.parse_culture);
		int value = ((num < 1) ? 1 : ((num > max_price) ? max_price : num));
		ItemCountPair[] array = new ItemCountPair[10];
		ItemCountPair[] array2 = new ItemCountPair[10];
		for (int i = 0; i < 10; i++)
		{
			array[i] = new ItemCountPair("", 0);
			array2[i] = new ItemCountPair("", 0);
		}
		InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
		ItemCountPair[] itemListFromItem = ChunkControl.Instance.GetItemListFromItem("vending_machine_for_sale", interacting_element_item);
		ItemCountPair[] itemListFromItem2 = ChunkControl.Instance.GetItemListFromItem("vending_machine_costs", interacting_element_item);
		for (int j = 0; j < 10 && j < itemListFromItem.Length; j++)
		{
			array[j] = itemListFromItem[j];
			array2[j] = itemListFromItem2[j];
		}
		array[clicked_vending_machine_index] = new ItemCountPair(item, count);
		array2[clicked_vending_machine_index] = new ItemCountPair(punctuatedCostItemText, 1);
		interacting_element_item = ChunkControl.Instance.EncodeItemListIntoItem("vending_machine_for_sale", array, interacting_element_item);
		ExtraInventoryData extraDataCopy = ChunkControl.Instance.EncodeItemListIntoItem("vending_machine_costs", array2, interacting_element_item).GetExtraDataCopy();
		extraDataCopy.SetLong("item_wanted_count_" + clicked_vending_machine_index, value);
		InventoryItem new_item = new InventoryItem("Vending Machine", extraDataCopy);
		ConstructionControl.Instance.PlayerReplaceInteracting(new_item, true);
		angular.SetActive(true);
		angular.transform.localPosition = nibs[clicked_vending_machine_index].transform.localPosition;
		angular.transform.Find("Image (1)").GetComponent<Image>().color = new Color(0.52156866f, 0.9764706f, 1f, 1f);
		angular.GetComponent<Animation>().Play();
		AudioControl.Instance.PlayPitch(sfx_insert_item, Random.Range(0.95f, 1.03f), 0.2f);
		additem_window.SetActive(false);
		main_window.SetActive(true);
		bottom_buttons.SetActive(true);
		back_button.SetActive(false);
		header.text = "ITEMS FOR SALE";
		RedrawAllSlots(false);
	}
}
