public class ItemCountPair
{
	private InventoryItem item_;

	private int count_;

	public InventoryItem item => item_;

	public int count => count_;

	public ItemCountPair(string item_name, int count_)
	{
		item_ = new InventoryItem(item_name);
		this.count_ = count_;
	}

	public ItemCountPair(InventoryItem item_, int count_)
	{
		this.item_ = item_;
		this.count_ = count_;
	}
}
