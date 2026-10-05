public class ChunkElement
{
	public InventoryItem item;

	public int rot;

	public ChunkElement(string item_name)
	{
		item = new InventoryItem(item_name);
		rot = 0;
	}

	public ChunkElement(string item_name, int rot)
	{
		item = new InventoryItem(item_name);
		this.rot = rot;
	}

	public ChunkElement(InventoryItem item)
	{
		this.item = item;
		rot = 0;
	}

	public ChunkElement(InventoryItem item, int rot)
	{
		this.item = item;
		this.rot = rot;
	}
}
