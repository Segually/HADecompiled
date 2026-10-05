using System.Collections.Generic;
using UnityEngine;

public class PartiallyGeneratedModularModel
{
	public ModularObjectControl.segment segment;

	public string unique_key;

	public InventoryItem item;

	public PartiallyGeneratedModularModelSubObj[] sub_objects;

	public string zone;

	public int chunkX;

	public int chunkZ;

	public int biome_id;

	public List<Vector2> filled_locations = new List<Vector2>();

	public int n_complete;

	public int n_required;

	public bool n_required_set;

	public PartiallyGeneratedModularModel(InventoryItem item, ModularObjectControl.segment segment, string unique_key, string zone, int chunkX, int chunkZ, int biome_id)
	{
		this.item = item;
		this.segment = segment;
		this.unique_key = unique_key;
		this.zone = zone;
		this.chunkX = chunkX;
		this.chunkZ = chunkZ;
		this.biome_id = biome_id;
		sub_objects = new PartiallyGeneratedModularModelSubObj[0];
	}

	public void OnMeshSegmentAdded()
	{
		n_complete++;
		if (n_required_set && n_complete == n_required)
		{
			if (inventory_ctr.Instance.GetItemBool(item.item_name, "is_flooring_obj"))
			{
				ModularObjectControl.Instance.FinalizePathway(this);
			}
			else
			{
				ModularObjectControl.Instance.FinalizeWalls(this);
			}
		}
	}
}
