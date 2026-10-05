using UnityEngine;

public class Collectible : MonoBehaviour
{
	public enum pickup_type_t
	{
		destroy = 0,
		hide_object = 1
	}

	private enum collectibe_type_t
	{
		harvestable = 0,
		drop_item = 1
	}

	public string corresponding_inventory_object;

	public int n_give;

	public InventoryItem full_corresponding_inventory_object;

	public int respawn_time;

	public pickup_type_t on_pickup;

	public GameObject hide_obj;

	public float interaction_distance;

	public float circle_size;

	private collectibe_type_t collectible_type;

	public string active_obj_str;

	public string origin_zone;

	public int origin_chunkX;

	public int origin_chunkZ;

	public int origin_innerX;

	public int origin_innerZ;

	public InventoryItem origin_item;

	public int origin_rot;

	public bool deleted;

	public void InitAsHarvestable(string zone, int chunkX, int chunkZ, int x, int z, InventoryItem item, int rot, ChunkObj chunkObj)
	{
		collectible_type = collectibe_type_t.harvestable;
		active_obj_str = zone + "," + chunkX + "," + chunkZ + "," + x + "," + z;
		origin_zone = zone;
		origin_chunkX = chunkX;
		origin_chunkZ = chunkZ;
		origin_innerX = x;
		origin_item = item;
		origin_innerZ = z;
		origin_rot = rot;
		if (item.HasActiveRespawn("collect_spawn"))
		{
			if (on_pickup == pickup_type_t.hide_object)
			{
				hide_obj.SetActive(false);
				chunkObj.AddRespawnWatcher(x, z, item, rot, "collect_spawn");
			}
		}
		else if (!ChunkControl.Instance.active_interactibles.ContainsKey(active_obj_str))
		{
			ChunkControl.Instance.active_interactibles.Add(active_obj_str, base.gameObject);
		}
	}

	public void InitAsDrop(string active_obj_str)
	{
	}

	public void OnCollectLocal()
	{
	}

	public void Delete()
	{
		if (!deleted)
		{
			if (ChunkControl.Instance.active_interactibles.ContainsKey(active_obj_str))
			{
				ChunkControl.Instance.active_interactibles.Remove(active_obj_str);
			}
			deleted = true;
		}
	}

	public void OnDestroy()
	{
	}
}
