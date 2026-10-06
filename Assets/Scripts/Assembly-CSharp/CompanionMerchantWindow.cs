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

	private List<vendor_desc> vendor_types = new List<vendor_desc>();

	public Image page_L;

	public Image page_R;

	public GameObject page_pick;

	public GameObject page_accept;

	public Text text_vendor_description;

	public Text text_example_items;

	public Text text_description;

	public Text text_dialogue;

	private List<GameObject> instantiated_merchant_nibs = new List<GameObject>();

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

	private int clicked_nib_id = -1;

	public Color col_nib_unlocked;

	private Color col_nib_locked;

	private int final_page;

	private int page;

	private void Start()
	{
		page_pick.SetActive(true);
		page_accept.SetActive(false);
		text_example_items.text = TranslationControl.Instance.TranslateGeneral("Example Items", "Merchants");
		text_description.text = TranslationControl.Instance.TranslateGeneral("Description", "Merchants");
		text_dialogue.text = TranslationControl.Instance.TranslateGeneral("Dialogue", "Merchants");
		instantiated_merchant_nibs.Add(prefab_merchant_nib);
		col_nib_locked = prefab_merchant_nib.GetComponent<Image>().color;
		for (int i = 1; i < 6; i++)
		{
			GameObject gameObject = Object.Instantiate(prefab_merchant_nib);
			gameObject.transform.SetParent(prefab_merchant_nib.transform.parent);
			gameObject.transform.localScale = prefab_merchant_nib.transform.localScale;
			gameObject.transform.localRotation = prefab_merchant_nib.transform.localRotation;
			gameObject.transform.localPosition = prefab_merchant_nib.transform.localPosition + new Vector3(i % 3 * 302, i / 3 * -220, 0f);
			instantiated_merchant_nibs.Add(gameObject);
		}
		ExtraInventoryData extraInventoryData = new ExtraInventoryData();
		extraInventoryData.SetShort("npc_record_index", 1);
		InventoryItem item_ = new InventoryItem("Saved Record", extraInventoryData);
		ExtraInventoryData extraInventoryData2 = new ExtraInventoryData();
		extraInventoryData2.SetShort("dev_painting_id", 1);
		InventoryItem item_2 = new InventoryItem("Painting", extraInventoryData2);
		ExtraInventoryData extraInventoryData3 = new ExtraInventoryData();
		extraInventoryData3.SetString("egg_monster", "Cow");
		InventoryItem item_3 = new InventoryItem("Egg", extraInventoryData3);
		AddVendor("Potions", TranslationControl.Instance.TranslateGeneral("Potions", "Merchants"), 2, new ItemCountPair("MoonBerry Juice", 1), new ItemCountPair("Cauldron", 1), "", false);
		AddVendor("Food", TranslationControl.Instance.TranslateGeneral("Food", "Merchants"), 2, new ItemCountPair("Muffin", 1), new ItemCountPair("Oven", 1), "", false);
		AddVendor("Organic", TranslationControl.Instance.TranslateGeneral("Farming", "Merchants"), 2, new ItemCountPair("Berry Bush", 1), new ItemCountPair("Shovel", 1), "", false);
		AddVendor("Ranger", TranslationControl.Instance.TranslateGeneral("Ranger", "Merchants"), 2, new ItemCountPair("Fur", 1), new ItemCountPair("Feathers", 1), "", false);
		AddVendor("Clothing", TranslationControl.Instance.TranslateGeneral("Clothing", "Merchants"), 2, new ItemCountPair("Top Hat", 1), new ItemCountPair("Loom", 1), "", false);
		AddVendor("Furniture", TranslationControl.Instance.TranslateGeneral("Furniture", "Merchants"), 2, new ItemCountPair("Chair", 1), new ItemCountPair("Shack", 1), "", false);
		AddVendor("Records", TranslationControl.Instance.TranslateGeneral("Music", "Merchants"), 4, new ItemCountPair(item_, 1), new ItemCountPair("Music Box", 1), "", false);
		AddVendor("Paintings", TranslationControl.Instance.TranslateGeneral("Paintings", "Merchants"), 4, new ItemCountPair(item_2, 1), new ItemCountPair("Painting Easel", 1), "", false);
		AddVendor("Paintbrushes And Stamps", TranslationControl.Instance.TranslateGeneral("Paintbrushes & Stamps", "Merchants"), 4, new ItemCountPair("Basic Red", 1), new ItemCountPair("Dragon Stamp", 1), "", false);
		AddVendor("Random", TranslationControl.Instance.TranslateGeneral("Random Items", "Merchants"), 4, new ItemCountPair("", 0), new ItemCountPair("DEBUG-question-mark", 0), "Sells random items.", false);
		AddVendor("Mining", TranslationControl.Instance.TranslateGeneral("Mining", "Merchants"), 4, new ItemCountPair("Stone Pick", 1), new ItemCountPair("Stone Ore", 1), "", false);
		AddVendor("Clothing 2", TranslationControl.Instance.TranslateGeneral("Clothing", "Merchants") + " II", 4, new ItemCountPair("Sapphire Crown", 1), new ItemCountPair("Storm Wizard Hat", 1), "", true);
		AddVendor("Furniture 2", TranslationControl.Instance.TranslateGeneral("Furniture", "Merchants") + " II", 6, new ItemCountPair("Metal Chair", 1), new ItemCountPair("Windmill", 1), "", true);
		AddVendor("BuyerOnly", TranslationControl.Instance.TranslateGeneral("Buys Anything", "Merchants"), 6, new ItemCountPair("", 0), new ItemCountPair("Coins", 100), "Sells nothing, but buys anything. Pays you very little when you sell.", false);
		AddVendor("Mining 2", TranslationControl.Instance.TranslateGeneral("Mining", "Merchants") + " II", 6, new ItemCountPair("Titanium Pick", 1), new ItemCountPair("Ruby", 1), "", true);
		AddVendor("Random 2", TranslationControl.Instance.TranslateGeneral("Random Items", "Merchants") + " II", 6, new ItemCountPair("", 0), new ItemCountPair("DEBUG-question-mark", 0), "Sells more expensive random items.", false);
		AddVendor("Weapons And Armor", TranslationControl.Instance.TranslateGeneral("Weapons & Armor", "Merchants"), 6, new ItemCountPair("Metal Sword", 1), new ItemCountPair("Anvil", 1), "", false);
		AddVendor("Paintbrushes And Stamps 2", TranslationControl.Instance.TranslateGeneral("Paintbrushes & Stamps", "Merchants") + " II", 6, new ItemCountPair("Mystic Dusk", 1), new ItemCountPair("Paint Shaker", 1), "", true);
		AddVendor("Traps", TranslationControl.Instance.TranslateGeneral("Traps", "Merchants"), 8, new ItemCountPair("Giant Puncher", 1), new ItemCountPair("Pop-up Saw", 1), "", false);
		AddVendor("BuyerOnly 2", TranslationControl.Instance.TranslateGeneral("Buys Anything", "Merchants") + " II", 8, new ItemCountPair("", 0), new ItemCountPair("Coins", 500), "Sells nothing, but buys anything. Does not pay you as much as other merchants when you sell.", false);
		AddVendor("Weapons And Armor 2", TranslationControl.Instance.TranslateGeneral("Weapons & Armor", "Merchants") + " II", 8, new ItemCountPair("Magma DoubleSword", 1), new ItemCountPair("Dark Solaris Helm", 1), "", true);
		AddVendor("Eggs", TranslationControl.Instance.TranslateGeneral("Eggs", "Merchants"), 15, new ItemCountPair(item_3, 1), new ItemCountPair("Egg Fuser", 1), "", false);
		AddVendor("BuyerOnly 3", TranslationControl.Instance.TranslateGeneral("Buys Anything", "Merchants") + " III", 15, new ItemCountPair("", 0), new ItemCountPair("Coins", 1500), "Sells nothing, but buys anything. Best deal of all 'Buys Anything' merchants, but still pays less than other merchant types.", false);
		AddVendor("Bone Juice", TranslationControl.Instance.TranslateGeneral("Bone Juice", "Merchants"), 15, new ItemCountPair("", 0), new ItemCountPair("Bone Juice", 1), "", false);
		final_page = (int)((float)(vendor_types.Count - 1) / 6f);
		RedrawPage();
		RedrawHeader();
	}

	public void PressFinalAccept()
	{
		ActiveCompanion currSelectedCompanion = CompanionController.Instance.GetCurrSelectedCompanion();
		if (currSelectedCompanion.companion_item.GetString("unlocked_merchant_type_" + vendor_types[clicked_nib_id].file_name) == "true")
		{
			PlaceMerchantObject();
			return;
		}
		int req_gems = vendor_types[clicked_nib_id].n_Gems;
		if (PlayerData.Instance.GetGlobalShort("GEMS") < req_gems)
		{
			PopupControl.Instance.ShowMessage("<color=#ff3b29>You don't have enough gems!</color>");
			return;
		}
		PopupControl.Instance.on_yes_pressed = delegate
		{
			PlayerData.Instance.SetGlobalShort("GEMS", PlayerData.Instance.GetGlobalShort("GEMS") - req_gems);
			PlaceMerchantObject();
		};
		PopupControl.Instance.ShowYesNo("Spend <color=#38b9ff>" + req_gems + " gems</color> and convert " + currSelectedCompanion.companion_name + " to a <color=#ffdb38>" + vendor_types[clicked_nib_id].visual_name + "</color> Merchant?", "Yes", "No", PopupControl.context.yesno_ACTION);
	}

	private void PlaceMerchantObject()
	{
		ActiveCompanion currSelectedCompanion = CompanionController.Instance.GetCurrSelectedCompanion();
		ExtraInventoryData extraDataCopy = currSelectedCompanion.companion_item.GetExtraDataCopy();
		extraDataCopy.SetString("merchant_message1", dialogue1.text);
		extraDataCopy.SetString("merchant_message2", dialogue2.text);
		extraDataCopy.SetString("unlocked_merchant_type_" + vendor_types[clicked_nib_id].file_name, "true");
		extraDataCopy.SetString("companion_mode", "merchant");
		extraDataCopy.SetString("merchant_type", vendor_types[clicked_nib_id].file_name);
		currSelectedCompanion.companion_item = new InventoryItem(currSelectedCompanion.companion_item.item_name, extraDataCopy);
		CompanionController.Instance.SaveActiveCompanions();
		WindowControl.Instance.CloseMiniwindow(false);
		PopupControl.Instance.SetButtonWasPressed();
		ConstructionControl.Instance.EnterBuildMode(currSelectedCompanion.companion_item, Vector3.zero, 0, false);
	}

	private void AddVendor(string file_name, string visual_name, int n_gems, ItemCountPair item1, ItemCountPair item2, string overwrite_description, bool add_more_expensive_str)
	{
		vendor_types.Add(new vendor_desc
		{
			visual_name = visual_name,
			file_name = file_name,
			n_Gems = n_gems,
			item1 = item1,
			item2 = item2,
			overwrite_description = overwrite_description,
			add_more_expensive_str = add_more_expensive_str
		});
	}

	public void PressBackOnPickType()
	{
		WindowPrefabsControl.Instance.GetScreen("COMPANION-commands").gameObject.SetActive(true);
		WindowPrefabsControl.Instance.DestroyScreen("COMPANION-merchant");
	}

	public void PressBackOnAccept()
	{
		ActiveCompanion currSelectedCompanion = CompanionController.Instance.GetCurrSelectedCompanion();
		ExtraInventoryData extraDataCopy = currSelectedCompanion.companion_item.GetExtraDataCopy();
		extraDataCopy.SetString("merchant_message1", dialogue1.text);
		extraDataCopy.SetString("merchant_message2", dialogue2.text);
		currSelectedCompanion.companion_item = new InventoryItem(currSelectedCompanion.companion_item.item_name, extraDataCopy);
		CompanionController.Instance.SaveActiveCompanions();
		page_pick.SetActive(true);
		page_accept.SetActive(false);
		header_merchant_icon.gameObject.SetActive(true);
		header_item_sprites.SetActive(false);
		RedrawHeader();
	}

	private void RedrawHeader()
	{
		header_text.text = TranslationControl.Instance.TranslateGeneral("Convert to Merchant", "Merchants");
		header_merchant_icon.transform.localPosition = new Vector3(header_text.transform.localPosition.x - header_text.preferredWidth * 0.5f - 70f, header_item_sprites.transform.localPosition.y, 0f);
	}

	public void ClickMerchantType(GameObject button)
	{
		page_pick.SetActive(false);
		page_accept.SetActive(true);
		header_merchant_icon.gameObject.SetActive(false);
		header_item_sprites.SetActive(true);
		ActiveCompanion currSelectedCompanion = CompanionController.Instance.GetCurrSelectedCompanion();
		string text = currSelectedCompanion.companion_item.GetString("merchant_message1");
		string text2 = currSelectedCompanion.companion_item.GetString("merchant_message2");
		if (Startup.StringNullOrWhitespace(dialogue1.text))
		{
			dialogue1.SetTextWithoutNotify(text);
		}
		if (Startup.StringNullOrWhitespace(dialogue2.text))
		{
			dialogue2.SetTextWithoutNotify(text2);
		}
		for (int i = 0; i < instantiated_merchant_nibs.Count; i++)
		{
			if (!(button == instantiated_merchant_nibs[i]))
			{
				continue;
			}
			clicked_nib_id = i + page * 6;
			vendor_desc vendor_desc = vendor_types[clicked_nib_id];
			bool flag = currSelectedCompanion.companion_item.GetString("unlocked_merchant_type_" + vendor_desc.file_name) == "true";
			header_text.text = vendor_desc.visual_name;
			header_item_sprites.transform.localPosition = new Vector3(header_text.transform.localPosition.x - header_text.preferredWidth * 0.5f - 70f, header_item_sprites.transform.localPosition.y, 0f);
			header_item1.RedrawAsHoverIcon(vendor_desc.item1.item, vendor_desc.item1.count);
			header_item2.RedrawAsHoverIcon(vendor_desc.item2.item, vendor_desc.item2.count);
			if (vendor_desc.overwrite_description != "")
			{
				text_vendor_description.text = vendor_desc.overwrite_description;
			}
			else
			{
				text_vendor_description.text = GetGenericDescription(vendor_desc.file_name, vendor_desc.add_more_expensive_str);
			}
			RepositionItemPair(vendor_desc.item1.item, header_item2.transform);
			List<ItemCountPair> list = new List<ItemCountPair>(MerchantControl.Instance.GenerateRandomSellList(vendor_desc.file_name));
			while (list.Count < 5)
			{
				list.Add(new ItemCountPair("", 0));
			}
			while (list.Count > 5)
			{
				list.RemoveAt(Random.Range(0, list.Count));
			}
			example_item1.RedrawAsHoverIcon(list[0].item, list[0].count);
			example_item2.RedrawAsHoverIcon(list[1].item, list[1].count);
			example_item3.RedrawAsHoverIcon(list[2].item, list[2].count);
			example_item4.RedrawAsHoverIcon(list[3].item, list[3].count);
			example_item5.RedrawAsHoverIcon(list[4].item, list[4].count);
			if (!flag && vendor_desc.n_Gems > 0)
			{
				gem_text_on_accept.gameObject.SetActive(true);
				gem_text_on_accept.text = "(Costs " + vendor_desc.n_Gems + "    )";
				gem_icon_on_accept.gameObject.SetActive(true);
				if (vendor_desc.n_Gems < 9)
				{
					gem_icon_on_accept.rectTransform.localPosition = new Vector3(57.6f, 1.2f, 0f);
				}
				else
				{
					gem_icon_on_accept.rectTransform.localPosition = new Vector3(65.2f, 1.2f, 0f);
				}
				accept_button_text.rectTransform.localPosition = new Vector3(0f, 52.5f, 0f);
			}
			else
			{
				gem_text_on_accept.gameObject.SetActive(false);
				gem_icon_on_accept.gameObject.SetActive(false);
				accept_button_text.rectTransform.localPosition = new Vector3(0f, 28.8f, 0f);
			}
			break;
		}
	}

	private string GetGenericDescription(string merchant_type, bool add_more_expensive_str)
	{
		List<string> list = new List<string>();
		foreach (string allCategory in MerchantControl.Instance.GetAllCategories(merchant_type))
		{
			list.Add(allCategory);
		}
		foreach (string hardCodedItem in MerchantControl.Instance.GetHardCodedItems(merchant_type))
		{
			list.Add(hardCodedItem);
		}
		string text = (add_more_expensive_str ? "more expensive " : "");
		if (list.Count == 1)
		{
			return "Sells " + text + list[0] + ".";
		}
		if (list.Count == 2)
		{
			return "Sells " + text + list[0] + " and " + list[1] + ".";
		}
		string text2 = "Sells " + text;
		for (int i = 0; i < list.Count; i++)
		{
			text2 = ((i == list.Count - 2) ? (text2 + list[i] + ", and ") : ((i != list.Count - 1) ? (text2 + list[i] + ", ") : (text2 + list[i] + ".")));
		}
		return text2;
	}

	public void PressNextPage()
	{
		if (page + 1 <= final_page)
		{
			page++;
			RedrawPage();
		}
	}

	public void PressPrevPage()
	{
		if (page > 0)
		{
			page--;
			RedrawPage();
		}
	}

	private void RedrawPage()
	{
		ActiveCompanion currSelectedCompanion = CompanionController.Instance.GetCurrSelectedCompanion();
		for (int i = 0; i < 6; i++)
		{
			int num = page * 6 + i;
			if (num < vendor_types.Count)
			{
				vendor_desc vendor_desc = vendor_types[num];
				bool flag = currSelectedCompanion.companion_item.GetString("unlocked_merchant_type_" + vendor_desc.file_name) == "true";
				instantiated_merchant_nibs[i].SetActive(true);
				instantiated_merchant_nibs[i].transform.Find("Text and Images").Find("Title").GetComponent<Text>().text = vendor_desc.visual_name;
				instantiated_merchant_nibs[i].GetComponent<Image>().color = (flag ? col_nib_unlocked : col_nib_locked);
				if (!flag && vendor_desc.n_Gems != 0)
				{
					instantiated_merchant_nibs[i].transform.Find("gems_count").gameObject.SetActive(true);
					instantiated_merchant_nibs[i].transform.Find("Gem_img").gameObject.SetActive(true);
					instantiated_merchant_nibs[i].transform.Find("gems_count").GetComponent<Text>().text = "x" + vendor_desc.n_Gems;
					instantiated_merchant_nibs[i].transform.Find("Text and Images").localPosition = new Vector3(0f, 30.9f, 0f);
				}
				else
				{
					instantiated_merchant_nibs[i].transform.Find("gems_count").gameObject.SetActive(false);
					instantiated_merchant_nibs[i].transform.Find("Gem_img").gameObject.SetActive(false);
					instantiated_merchant_nibs[i].transform.Find("Text and Images").localPosition = new Vector3(0f, 0f, 0f);
				}
				instantiated_merchant_nibs[i].transform.Find("Text and Images").Find("Images").Find("item sprite").GetComponent<ItemSprite>().RedrawAsHoverIcon(vendor_desc.item1.item, vendor_desc.item1.count);
				instantiated_merchant_nibs[i].transform.Find("Text and Images").Find("Images").Find("item sprite 2").GetComponent<ItemSprite>().RedrawAsHoverIcon(vendor_desc.item2.item, vendor_desc.item2.count);
				RepositionItemPair(vendor_desc.item1.item, instantiated_merchant_nibs[i].transform.Find("Text and Images").Find("Images").Find("item sprite 2"));
			}
			else
			{
				instantiated_merchant_nibs[i].SetActive(false);
			}
		}
		page_L.color = new Color(1f, 1f, 1f, (page == 0) ? 0.4f : 1f);
		page_R.color = new Color(1f, 1f, 1f, (page == final_page) ? 0.4f : 1f);
	}

	private void RepositionItemPair(InventoryItem item1, Transform sprite2)
	{
		if (item1.item_name != "")
		{
			sprite2.localPosition = new Vector3(-7.9f, 7.5f, 0f);
		}
		else
		{
			sprite2.localPosition = new Vector3(0f, 0f, 0f);
		}
	}
}
