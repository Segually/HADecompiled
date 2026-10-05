public class RespawnWatcher
{
	public int x;

	public int z;

	public InventoryItem element_item;

	public int element_rot;

	public string respawn_check_str;

	public RespawnWatcher(int x, int z, InventoryItem element_item, int element_rot, string respawn_check_str)
	{
		this.x = x;
		this.z = z;
		this.element_item = element_item;
		this.element_rot = element_rot;
		this.respawn_check_str = respawn_check_str;
	}
}
