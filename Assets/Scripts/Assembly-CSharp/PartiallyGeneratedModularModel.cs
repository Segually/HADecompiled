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

	public List<Vector2> filled_locations;

	public int n_complete;

	public int n_required;

	public bool n_required_set;

	public PartiallyGeneratedModularModel(InventoryItem item, ModularObjectControl.segment segment, string unique_key, string zone, int chunkX, int chunkZ, int biome_id)
	{
	}

	public void OnMeshSegmentAdded()
	{
	}
}
