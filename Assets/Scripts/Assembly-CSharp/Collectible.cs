using System;
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

	public int n_give = 1;

	public InventoryItem full_corresponding_inventory_object = new InventoryItem("");

	public int respawn_time;

	public pickup_type_t on_pickup;

	public GameObject hide_obj;

	public float interaction_distance = 1f;

	public float circle_size = 1f;

	private collectibe_type_t collectible_type;

	public string active_obj_str;

	public string origin_zone = "";

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
		this.active_obj_str = active_obj_str;
		collectible_type = collectibe_type_t.drop_item;
		if (!ChunkControl.Instance.active_interactibles.ContainsKey(active_obj_str))
		{
			ChunkControl.Instance.active_interactibles.Add(active_obj_str, base.gameObject);
		}
	}

	public void OnCollectLocal()
	{
		InventoryItem inventoryItem;
		if (full_corresponding_inventory_object.item_name != "")
		{
			inventoryItem = full_corresponding_inventory_object;
		}
		else if (corresponding_inventory_object == "Fossil")
		{
			ExtraInventoryData extraInventoryData = new ExtraInventoryData();
			extraInventoryData.SetString("fossil_monster", CreatureMorpher.Instance.GetRandomCreature());
			inventoryItem = new InventoryItem("Fossil", extraInventoryData);
		}
		else if (corresponding_inventory_object == "Egg")
		{
			string biome_mobA = ChunkControl.Instance.GetChunkData(ChunkControl.Instance.GetChunkString(origin_zone, origin_chunkX, origin_chunkZ)).biome_mobA;
			ExtraInventoryData extraInventoryData2 = new ExtraInventoryData();
			extraInventoryData2.SetString("egg_monster", biome_mobA);
			inventoryItem = new InventoryItem("Egg", extraInventoryData2);
		}
		else
		{
			inventoryItem = new InventoryItem(corresponding_inventory_object);
		}
		if (!inventory_ctr.Instance.CanReceiveItem(inventoryItem, n_give, true))
		{
			return;
		}
		inventory_ctr.Instance.GiveItem(inventoryItem, n_give, "");
		if (ChunkControl.Instance.active_interactibles.ContainsKey(active_obj_str))
		{
			ChunkControl.Instance.active_interactibles.Remove(active_obj_str);
		}
		if (collectible_type == collectibe_type_t.drop_item)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (collectible_type == collectibe_type_t.harvestable)
		{
			double value = 7200.0;
			if (origin_item.GetString("bandit_camp_instance") == "" && !InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
			{
				value = respawn_time;
			}
			InventoryItem new_item = ChunkControl.Instance.EncodeRespawnIntoItem("collect_spawn", origin_item, DateTime.UtcNow.AddSeconds(value));
			ConstructionControl.Instance.PlayerReplaceAt(new_item, origin_item, origin_rot, origin_zone, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, true, ConstructionControl.GenerateCacheKey());
		}
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
		if (!deleted)
		{
			if (ChunkControl.Instance.active_interactibles.ContainsKey(active_obj_str))
			{
				ChunkControl.Instance.active_interactibles.Remove(active_obj_str);
			}
			deleted = true;
		}
	}
}
