using System.Collections.Generic;
using UnityEngine;

public class BasketContents
{
	public bool values_edited;

	private Dictionary<int, ItemCountPair> stored_ = new Dictionary<int, ItemCountPair>();

	public ItemCountPair this[int slot_id]
	{
		get
		{
			if (!stored_.ContainsKey(slot_id))
			{
				return new ItemCountPair("", 0);
			}
			return stored_[slot_id];
		}
		set
		{
			if (value.item.item_name != "" && value.count > 0)
			{
				if (!stored_.ContainsKey(slot_id))
				{
					stored_.Add(slot_id, value);
				}
				else
				{
					stored_[slot_id] = value;
				}
				if (inventory_ctr.Instance != null && this == inventory_ctr.Instance.player_inventory)
				{
					inventory_ctr.Instance.EquipIfPossible(slot_id);
				}
			}
			else
			{
				if (stored_.ContainsKey(slot_id))
				{
					stored_.Remove(slot_id);
				}
				if (inventory_ctr.Instance != null && this == inventory_ctr.Instance.player_inventory)
				{
					inventory_ctr.Instance.UnequipIfPossible(slot_id);
				}
			}
			values_edited = true;
		}
	}

	public BasketContents()
	{
		ClearContents();
	}

	private void ClearContents()
	{
		stored_.Clear();
	}

	public void SaveAllAsInventory()
	{
		string filenameString = PlayerData.Instance.GetFilenameString(PlayerData.filename_t.the_inventory);
		SingleFile file = PlayerData.Instance.GetSlotFilesGroup(-1).GetFile(filenameString);
		file.ClearSegment("default");
		SaveAll(file, "default");
	}

	public void SaveToAllAsContainer(int basket_id)
	{
		string text = "basket" + basket_id;
		string basketFilename = ChunkControl.GetBasketFilename(text);
		SingleFile file = PlayerData.Instance.GetSlotFilesGroup(-1).GetFile(basketFilename);
		file.ClearSegment(text);
		SaveAll(file, text);
	}

	private void SaveAll(SingleFile file, string filesegment)
	{
		file.SetShort("n_stored_items", stored_.Count, filesegment);
		int num = 0;
		foreach (KeyValuePair<int, ItemCountPair> item in stored_)
		{
			string text = "entry" + num;
			file.SetShort(text + "_slot", item.Key, filesegment);
			file.SetShort(text + "_count", item.Value.count, filesegment);
			item.Value.item.SaveToFile(file, text, filesegment);
			num++;
		}
	}

	public void LoadFromDiskAsInventory()
	{
		ClearContents();
		SlotFilesGroup slotFilesGroup = PlayerData.Instance.GetSlotFilesGroup(-1);
		string filenameString = PlayerData.Instance.GetFilenameString(PlayerData.filename_t.the_inventory);
		LoadFromFile(slotFilesGroup.GetFile(filenameString), "default");
	}

	public void LoadFromDiskAsContainer(int container_id)
	{
		ClearContents();
		string text = "basket" + container_id;
		string basketFilename = ChunkControl.GetBasketFilename(text);
		LoadFromFile(PlayerData.Instance.GetSlotFilesGroup(-1).GetFile(basketFilename), text);
	}

	private void LoadFromFile(SingleFile file, string filesegment)
	{
		short shortValue = file.GetShortValue("n_stored_items", filesegment);
		for (int i = 0; i < shortValue; i++)
		{
			string text = "entry" + i;
			short shortValue2 = file.GetShortValue(text + "_slot", filesegment);
			short shortValue3 = file.GetShortValue(text + "_count", filesegment);
			InventoryItem inventoryItem = InventoryItem.LoadFromFile(text, file, filesegment);
			int count_ = Mathf.Clamp(shortValue3, 0, inventory_ctr.Instance.GetItemMaxStack(inventoryItem.item_name));
			if (!stored_.ContainsKey(shortValue2))
			{
				stored_.Add(shortValue2, new ItemCountPair(inventoryItem, count_));
			}
			else
			{
				stored_[shortValue2] = new ItemCountPair(inventoryItem, count_);
			}
		}
	}

	public BasketContents(Packet incoming)
	{
	}

	public void Pack(Packet outgoing)
	{
	}

	public List<int> FilledSlots()
	{
		List<int> list = new List<int>();
		foreach (int key in stored_.Keys)
		{
			list.Add(key);
		}
		return list;
	}

	public List<int> FilledSlotsReversed()
	{
		List<int> list = FilledSlots();
		list.Reverse();
		return list;
	}

	public void RemoveItemByName(string item_name, int count, int start_remove_from_slot = -1)
	{
		if (start_remove_from_slot != -1 && this[start_remove_from_slot].item.item_name == item_name)
		{
			int count2 = this[start_remove_from_slot].count;
			int count3 = this[start_remove_from_slot].count;
			if (count <= count2)
			{
				this[start_remove_from_slot] = new ItemCountPair(this[start_remove_from_slot].item, count3 - count);
				return;
			}
			count -= count3;
			this[start_remove_from_slot] = new ItemCountPair("", 0);
		}
		foreach (int item2 in FilledSlotsReversed())
		{
			if ((item2 != start_remove_from_slot || start_remove_from_slot == -1) && this[item2].item.item_name == item_name)
			{
				if (count <= this[item2].count)
				{
					int count4 = this[item2].count;
					this[item2] = new ItemCountPair(this[item2].item, count4 - count);
					break;
				}
				int count5 = this[item2].count;
				this[item2] = new ItemCountPair("", 0);
				count -= count5;
			}
		}
	}

	public void RemoveItemExact(InventoryItem item, int count, int start_remove_from_slot = -1)
	{
		if (start_remove_from_slot != -1 && this[start_remove_from_slot].item == item)
		{
			int count2 = this[start_remove_from_slot].count;
			int count3 = this[start_remove_from_slot].count;
			if (count <= count2)
			{
				this[start_remove_from_slot] = new ItemCountPair(this[start_remove_from_slot].item, count3 - count);
				return;
			}
			count -= count3;
			this[start_remove_from_slot] = new ItemCountPair("", 0);
		}
		foreach (int item2 in FilledSlotsReversed())
		{
			if ((item2 != start_remove_from_slot || start_remove_from_slot == -1) && this[item2].item == item)
			{
				if (count <= this[item2].count)
				{
					int count4 = this[item2].count;
					this[item2] = new ItemCountPair(this[item2].item, count4 - count);
					break;
				}
				int count5 = this[item2].count;
				this[item2] = new ItemCountPair("", 0);
				count -= count5;
			}
		}
	}
}
