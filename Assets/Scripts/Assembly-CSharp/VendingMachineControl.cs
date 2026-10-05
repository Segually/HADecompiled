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

	private int max_price;

	private int clicked_vending_machine_index;

	private int insert_inventory_index;

	public void ClickAutoFill1()
	{
	}

	public void ClickAutoFill2()
	{
	}

	public void OnOpen()
	{
	}

	private void RedrawAllSlots(bool edit_mode)
	{
	}

	private void RedrawItemSlot(GameObject nib, ItemCountPair sell_item, ItemCountPair cost, bool sold, bool edit_mode)
	{
	}

	private ItemCountPair GetForSaleAt(int index)
	{
		return null;
	}

	private ItemCountPair GetPriceAt(int index)
	{
		return null;
	}

	public void PressBuyItem(int index)
	{
	}

	public void PressAcceptBuy()
	{
	}

	public void PressAddRemoveItems()
	{
	}

	public void PressBackOnAddRemoveItems()
	{
	}

	public void PressAddItem(int index)
	{
	}

	public void PressCollectProfit(int index)
	{
	}

	public void PressBackOnSelectItem()
	{
	}

	public void SelectedItem(int inventory_index)
	{
	}

	public void TypeCostItemSelected()
	{
	}

	public void TypeCostItemDeselected()
	{
	}

	public void UpdatedCostInput()
	{
	}

	public void PressRemoveItem(int index)
	{
	}

	public string GetPunctuatedCostItemText(ref bool valid_item)
	{
		return null;
	}

	public void PressAcceptOnAddItem()
	{
	}
}
