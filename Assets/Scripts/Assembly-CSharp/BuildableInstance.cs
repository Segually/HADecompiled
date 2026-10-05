using UnityEngine;

public class BuildableInstance
{
	public InventoryItem item;

	public GameObject obj;

	public BuildableInstance(InventoryItem item, GameObject obj)
	{
		this.item = item;
		this.obj = obj;
	}
}
