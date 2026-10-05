using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompanionMerchantWindow : MonoBehaviour
{
	private struct vendor_desc
	{
		public string visual_name;

		public string file_name;

		public int n_Gems;

		public ItemCountPair item1;

		public ItemCountPair item2;

		public string overwrite_description;

		public bool add_more_expensive_str;
	}

	public GameObject prefab_merchant_nib;

	private List<vendor_desc> vendor_types;

	public Image page_L;

	public Image page_R;

	public GameObject page_pick;

	public GameObject page_accept;

	public Text text_vendor_description;

	public Text text_example_items;

	public Text text_description;

	public Text text_dialogue;

	private List<GameObject> instantiated_merchant_nibs;

	public InputField dialogue1;

	public InputField dialogue2;

	public Text header_text;

	public Image header_merchant_icon;

	public GameObject header_item_sprites;

	public ItemSprite header_item1;

	public ItemSprite header_item2;

	public ItemSprite example_item1;

	public ItemSprite example_item2;

	public ItemSprite example_item3;

	public ItemSprite example_item4;

	public ItemSprite example_item5;

	public Image gem_icon_on_accept;

	public Text gem_text_on_accept;

	public Text accept_button_text;

	private int clicked_nib_id;

	public Color col_nib_unlocked;

	private Color col_nib_locked;

	private int final_page;

	private int page;

	private void Start()
	{
	}

	public void PressFinalAccept()
	{
	}

	private void PlaceMerchantObject()
	{
	}

	private void AddVendor(string file_name, string visual_name, int n_gems, ItemCountPair item1, ItemCountPair item2, string overwrite_description, bool add_more_expensive_str)
	{
	}

	public void PressBackOnPickType()
	{
	}

	public void PressBackOnAccept()
	{
	}

	private void RedrawHeader()
	{
	}

	public void ClickMerchantType(GameObject button)
	{
	}

	private string GetGenericDescription(string merchant_type, bool add_more_expensive_str)
	{
		return null;
	}

	public void PressNextPage()
	{
	}

	public void PressPrevPage()
	{
	}

	private void RedrawPage()
	{
	}

	private void RepositionItemPair(InventoryItem item1, Transform sprite2)
	{
	}
}
