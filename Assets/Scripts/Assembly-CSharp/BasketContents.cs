using System.Collections.Generic;

public class BasketContents
{
	public bool values_edited;

	private Dictionary<int, ItemCountPair> stored_;

	public ItemCountPair this[int slot_id]
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public BasketContents()
	{
	}

	private void ClearContents()
	{
	}

	public void SaveAllAsInventory()
	{
	}

	public void SaveToAllAsContainer(int basket_id)
	{
	}

	private void SaveAll(SingleFile file, string filesegment)
	{
	}

	public void LoadFromDiskAsInventory()
	{
	}

	public void LoadFromDiskAsContainer(int container_id)
	{
	}

	private void LoadFromFile(SingleFile file, string filesegment)
	{
	}

	public BasketContents(Packet incoming)
	{
	}

	public void Pack(Packet outgoing)
	{
	}

	public List<int> FilledSlots()
	{
		return null;
	}

	public List<int> FilledSlotsReversed()
	{
		return null;
	}

	public void RemoveItemByName(string item_name, int count, int start_remove_from_slot = -1)
	{
	}

	public void RemoveItemExact(InventoryItem item, int count, int start_remove_from_slot = -1)
	{
	}
}
