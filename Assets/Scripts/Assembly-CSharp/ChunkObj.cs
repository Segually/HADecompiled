using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkObj
{
	public GameObject parent_obj;

	public GameObject edge1;

	public GameObject edge2;

	private Dictionary<string, List<BuildableInstance>> buildable_instances = new Dictionary<string, List<BuildableInstance>>();

	public Dictionary<string, GameObject> pathway_segments = new Dictionary<string, GameObject>();

	public Dictionary<string, GameObject> wall_segments = new Dictionary<string, GameObject>();

	public List<GameObject> grasses = new List<GameObject>();

	public List<OccupiedSpace> occupied_spaces_to_deload_later = new List<OccupiedSpace>();

	public string chunkStr;

	public List<RespawnWatcher> respawn_watchers = new List<RespawnWatcher>();

	public ChunkObj(int X, int Z, string zone)
	{
		chunkStr = ChunkControl.Instance.GetChunkString(zone, X, Z);
		if (X == -1 && Z == 0 && zone == "overworld")
		{
			GameController.Instance.EnableElevator(true);
		}
	}

	public GameObject GetBuildableInstanceByItem(int x, int z, InventoryItem item)
	{
		if (!buildable_instances.ContainsKey(x + "," + z))
		{
			return null;
		}
		foreach (BuildableInstance item2 in buildable_instances[x + "," + z])
		{
			if (item2.item == item)
			{
				return item2.obj;
			}
		}
		return null;
	}

	public GameObject GetBuildableInstanceByName(int x, int z, string generic_item_name)
	{
		if (!buildable_instances.ContainsKey(x + "," + z))
		{
			return null;
		}
		foreach (BuildableInstance item in buildable_instances[x + "," + z])
		{
			if (item.item.item_name == generic_item_name)
			{
				return item.obj;
			}
		}
		return null;
	}

	public bool DoesBuildableInstanceAlreadyExist(int x, int z, InventoryItem item)
	{
		if (!buildable_instances.ContainsKey(x + "," + z))
		{
			return false;
		}
		foreach (BuildableInstance item2 in buildable_instances[x + "," + z])
		{
			if (item2.item == item)
			{
				return true;
			}
		}
		return false;
	}

	public void AddBuildableInstance(GameObject instance, InventoryItem item, int chunkX, int chunkZ, int x, int z, byte rot)
	{
		if (!buildable_instances.ContainsKey(x + "," + z))
		{
			buildable_instances.Add(x + "," + z, new List<BuildableInstance>());
		}
		foreach (BuildableInstance item2 in buildable_instances[x + "," + z])
		{
			if (item2.item == item)
			{
				Debug.Log("ERROR: DID NOT ADD BUILDABLE INSTANCE - IT ALREADY EXISTS? (" + item.item_name + ")");
				return;
			}
		}
		buildable_instances[x + "," + z].Add(new BuildableInstance(item, instance));
	}

	public void DestroyBuildableInstance(int chunkX, int chunkZ, int x, int z, InventoryItem old_element_item, int old_element_rot)
	{
		TryEraseBuildableGeometry(old_element_item, new Vector3((float)(x + chunkX * 10) + 0.5f, 0f, (float)(z + chunkZ * 10) + 0.5f), old_element_rot);
		TryEraseRespawnWatcher(x, z, old_element_item);
		bool flag = InventoryUtils.ShouldReplaceOrDeleteExactItem(old_element_item.item_name);
		if (!buildable_instances.ContainsKey(x + "," + z))
		{
			return;
		}
		for (int i = 0; i < buildable_instances[x + "," + z].Count; i++)
		{
			BuildableInstance buildableInstance = buildable_instances[x + "," + z][i];
			if (flag ? (buildableInstance.item == old_element_item) : (buildableInstance.item.item_name == old_element_item.item_name))
			{
				if (buildableInstance.obj != null)
				{
					DeleteOne(buildableInstance.obj.transform);
					UnityEngine.Object.Destroy(buildableInstance.obj);
				}
				buildable_instances[x + "," + z].RemoveAt(i);
				break;
			}
		}
	}

	public void ReplaceElementItemInstance(int chunkX, int chunkZ, int x, int z, InventoryItem new_item, InventoryItem old_element_item, int old_element_rot, ChunkData chunk_data, Action<GameObject> on_complete = null)
	{
		DestroyBuildableInstance(chunkX, chunkZ, x, z, old_element_item, old_element_rot);
		ConstructionControl.Instance.AsyncCreateBuildableInstance(new_item, x, z, old_element_rot, ConstructionControl.build_context_t.on_regular_load, null, chunk_data, this, on_complete);
	}

	public void TemporarilyDisableChunkObjs(Vector3 origin, float range, List<GameObject> new_temporarily_disabled)
	{
	}

	public void TryCreateEdgePieces(ChunkData chunk_data)
	{
		if (chunk_data.biome != 4 && !(ChunkControl.Instance.biomes[chunk_data.biome].edge_color != Color.black))
		{
			return;
		}
		if (edge1 == null)
		{
			string chunkString = ChunkControl.Instance.GetChunkString(chunk_data.zone, chunk_data.X + 1, chunk_data.Z);
			if (ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
			{
				int biome = ChunkControl.Instance.GetChunkData(chunkString).biome;
				if (biome != chunk_data.biome && (biome == 4 || ChunkControl.Instance.biomes[biome].edge_color != Color.black))
				{
					edge1 = UnityEngine.Object.Instantiate(ChunkControl.Instance.biome_edge_piece);
					edge1.name = "edge1";
					edge1.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
					edge1.transform.position = new Vector3((float)chunk_data.X * 10f + 10f, 0.01f, (float)chunk_data.Z * 10f + 10f);
					edge1.transform.SetParent(parent_obj.transform);
					Color value;
					Color value2;
					if (chunk_data.biome == 4)
					{
						if (biome == 7)
						{
							value = ChunkControl.Instance.biomes[7].edge_color_OCEAN;
							value2 = ChunkControl.Instance.biomes[7].edge_color_OCEAN;
						}
						else
						{
							value = ChunkControl.Instance.biomes[biome].edge_color;
							value2 = ChunkControl.Instance.biomes[biome].edge_color_OCEAN;
						}
					}
					else if (biome == 4)
					{
						value = ChunkControl.Instance.biomes[chunk_data.biome].edge_color_OCEAN;
						value2 = ((chunk_data.biome != 7) ? ChunkControl.Instance.biomes[chunk_data.biome].edge_color : ChunkControl.Instance.biomes[chunk_data.biome].edge_color_OCEAN);
					}
					else
					{
						value = ChunkControl.Instance.biomes[biome].edge_color;
						value2 = ChunkControl.Instance.biomes[chunk_data.biome].edge_color;
					}
					ChunkControl.Instance.edge_mat_block.SetColor("_Color", value);
					edge1.transform.Find("model2").GetComponent<MeshRenderer>().SetPropertyBlock(ChunkControl.Instance.edge_mat_block);
					ChunkControl.Instance.edge_mat_block.SetColor("_Color", value2);
					edge1.transform.Find("model1").GetComponent<MeshRenderer>().SetPropertyBlock(ChunkControl.Instance.edge_mat_block);
				}
			}
		}
		if (!(edge2 == null))
		{
			return;
		}
		string chunkString2 = ChunkControl.Instance.GetChunkString(chunk_data.zone, chunk_data.X, chunk_data.Z + 1);
		if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString2))
		{
			return;
		}
		int biome2 = ChunkControl.Instance.GetChunkData(chunkString2).biome;
		if (biome2 != chunk_data.biome && (biome2 == 4 || !(ChunkControl.Instance.biomes[biome2].edge_color == Color.black)))
		{
			edge2 = UnityEngine.Object.Instantiate(ChunkControl.Instance.biome_edge_piece);
			edge2.name = "edge2";
			edge2.transform.rotation = Quaternion.Euler(-90f, 0f, -90f);
			edge2.transform.position = new Vector3((float)chunk_data.X * 10f + 0f, 0.01f, (float)chunk_data.Z * 10f + 10f);
			edge2.transform.SetParent(parent_obj.transform);
			Color value3;
			Color value4;
			if (chunk_data.biome == 4)
			{
				if (biome2 == 7)
				{
					value3 = ChunkControl.Instance.biomes[7].edge_color_OCEAN;
					value4 = ChunkControl.Instance.biomes[7].edge_color_OCEAN;
				}
				else
				{
					value3 = ChunkControl.Instance.biomes[biome2].edge_color;
					value4 = ChunkControl.Instance.biomes[biome2].edge_color_OCEAN;
				}
			}
			else if (biome2 == 4)
			{
				value3 = ChunkControl.Instance.biomes[chunk_data.biome].edge_color_OCEAN;
				value4 = ((chunk_data.biome != 7) ? ChunkControl.Instance.biomes[chunk_data.biome].edge_color : ChunkControl.Instance.biomes[chunk_data.biome].edge_color_OCEAN);
			}
			else
			{
				value3 = ChunkControl.Instance.biomes[biome2].edge_color;
				value4 = ChunkControl.Instance.biomes[chunk_data.biome].edge_color;
			}
			ChunkControl.Instance.edge_mat_block.SetColor("_Color", value3);
			edge2.transform.Find("model2").GetComponent<MeshRenderer>().SetPropertyBlock(ChunkControl.Instance.edge_mat_block);
			ChunkControl.Instance.edge_mat_block.SetColor("_Color", value4);
			edge2.transform.Find("model1").GetComponent<MeshRenderer>().SetPropertyBlock(ChunkControl.Instance.edge_mat_block);
		}
	}

	private void DeleteOne(Transform T)
	{
		Interactable interactable = T.gameObject.GetComponent<Interactable>();
		if (interactable == null && T.Find("Interactable") != null)
		{
			interactable = T.Find("Interactable").GetComponent<Interactable>();
		}
		if (interactable != null)
		{
			interactable.Delete();
		}
		Collectible collectible = T.gameObject.GetComponent<Collectible>();
		if (collectible == null && T.Find("Collectible") != null)
		{
			collectible = T.Find("Collectible").GetComponent<Collectible>();
		}
		if (collectible != null)
		{
			collectible.Delete();
		}
		Combatant combatant = T.gameObject.GetComponent<Combatant>();
		if (combatant == null && T.Find("Combatant") != null)
		{
			combatant = T.Find("Combatant").GetComponent<Combatant>();
		}
		if (combatant != null)
		{
			combatant.Delete();
		}
	}

	public void DeleteObjects(int X, int Z, string zone)
	{
		foreach (List<BuildableInstance> value in buildable_instances.Values)
		{
			foreach (BuildableInstance item in value)
			{
				if (item.obj != null)
				{
					DeleteOne(item.obj.transform);
				}
			}
		}
		UnityEngine.Object.Destroy(parent_obj);
		foreach (OccupiedSpace item2 in occupied_spaces_to_deload_later)
		{
			if (ChunkControl.Instance.global_spaces_occupied_by_buildables.Contains(item2))
			{
				ChunkControl.Instance.global_spaces_occupied_by_buildables.Remove(item2);
			}
		}
		if (X == -1 && Z == 0 && zone == "overworld")
		{
			GameController.Instance.EnableElevator(false);
		}
	}

	public void TryEraseBuildableGeometry(InventoryItem item, Vector3 origin, int rot)
	{
	}

	public void FillBuildableGeometry(InventoryItem item, Vector3 origin, int rot)
	{
		foreach (OccupiedSpace item2 in ConstructionControl.Instance.GetObjectWorldGeometry(item, origin, rot))
		{
			ChunkControl.Instance.global_spaces_occupied_by_buildables.Add(item2);
			occupied_spaces_to_deload_later.Add(item2);
		}
	}

	public void AddRespawnWatcher(int x, int z, InventoryItem element_item, int element_rot, string respawn_check_str)
	{
		respawn_watchers.Add(new RespawnWatcher(x, z, element_item, element_rot, respawn_check_str));
	}

	public void TryEraseRespawnWatcher(int x, int z, InventoryItem item)
	{
	}

	public void TickRespawnWatchers(int X, int Z, string zone)
	{
		List<RespawnWatcher> list = new List<RespawnWatcher>();
		foreach (RespawnWatcher respawn_watcher in respawn_watchers)
		{
			if (!respawn_watcher.element_item.HasActiveRespawn(respawn_watcher.respawn_check_str))
			{
				list.Add(respawn_watcher);
			}
		}
		foreach (RespawnWatcher item in list)
		{
			respawn_watchers.Remove(item);
			bool item_was_changed = false;
			InventoryItem new_item = ChunkControl.Instance.ProcessItemChange(item.element_item, ref item_was_changed);
			if (item_was_changed)
			{
				ConstructionControl.Instance.PlayerReplaceAt(new_item, item.element_item, item.element_rot, zone, X, Z, item.x, item.z, true, ConstructionControl.GenerateCacheKey());
			}
			else
			{
				ChunkControl.Instance.RedrawAtSquare(chunkStr, item.x, item.z);
			}
		}
	}
}
