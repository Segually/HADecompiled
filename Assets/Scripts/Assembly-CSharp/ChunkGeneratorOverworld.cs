using System.Collections.Generic;
using UnityEngine;

public class ChunkGeneratorOverworld
{
	public static void GenerateUnexplored(ChunkData chunk_data, int X, int Z, string zone, int biome_id, string biome_mob_a, string biome_mob_b, bool is_quest_miniworld, Dictionary<string, ZoneData> auto_built_zones)
	{
		ChunkControl.biome biome = ChunkControl.Instance.biomes[biome_id];
		chunk_data.biome = biome_id;
		chunk_data.biome_mobA = biome_mob_a;
		chunk_data.biome_mobB = biome_mob_b;
		chunk_data.floor_rotation = Random.Range(0, 4);
		chunk_data.floor_texture_index = Random.Range(0, biome.floor_texture_paths.Length);
		bool[,] filled = new bool[10, 10];
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				list.Insert(Random.Range(0, list.Count), new Vector2(i, j));
			}
		}
		float depth = GameController.Instance.DepthAt(new Vector3((float)X * 10f + 5f, 0f, (float)Z * 10f + 5f));
		int num = Random.Range(biome.scenics_budget_min, biome.scenics_budget_max + 1);
		if (num != 0)
		{
			GenerateBiomeObjects(chunk_data, num, true, biome.biome_scenic, list, filled, X, Z, depth, is_quest_miniworld, biome_id, auto_built_zones);
		}
		int num2 = Random.Range(biome.item_budget_min, biome.item_budget_max + 1);
		if (num2 != 0)
		{
			GenerateBiomeObjects(chunk_data, num2, false, biome.biome_scenic, list, filled, X, Z, depth, is_quest_miniworld, biome_id, auto_built_zones);
		}
		if (!is_quest_miniworld && Random.value < 0.251f)
		{
			int num3 = ((Random.value < 0.55f) ? 1 : 2);
			for (int k = 0; k < num3; k++)
			{
				Vector2 vector = list[0];
				list.RemoveAt(0);
				MobControl.PlaceMob((int)vector.x, (int)vector.y, chunk_data, depth);
			}
		}
		if (biome.dont_spawn_greens || !(Random.value < 0.411f))
		{
			return;
		}
		float value = Random.value;
		Vector2 vector2 = list[0];
		int num4 = ((value < 0.6f) ? 2 : 3);
		for (int l = 0; l < list.Count; l++)
		{
			Vector2 vector3 = list[l];
			if (Vector3.Distance(vector3, vector2) < 2.5f)
			{
				list.RemoveAt(l);
				chunk_data.AddElement((int)vector3.x, (int)vector3.y, new ChunkElement("Green Blob"));
				num4--;
				if (num4 == 0)
				{
					break;
				}
			}
		}
	}

	private static void GenerateBiomeObjects(ChunkData output_chunkData, int budget, bool is_scenic_layer, ChunkControl.biome_obj[] object_list, List<Vector2> empty_locations, bool[,] filled, int chunkX, int chunkZ, float depth, bool is_quest_miniworld, int biome_id, Dictionary<string, ZoneData> auto_built_zones)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		List<int> list4 = new List<int>();
		for (int i = 0; i < object_list.Length; i++)
		{
			if (is_scenic_layer ? object_list[i].is_item : (!object_list[i].is_item))
			{
				continue;
			}
			switch (object_list[i].spawn_rate)
			{
			case ChunkControl.spawn_commonness.average:
				list.Add(i);
				break;
			case ChunkControl.spawn_commonness.rare_slight:
				list2.Add(i);
				break;
			case ChunkControl.spawn_commonness.rare_medium:
				list3.Add(i);
				break;
			case ChunkControl.spawn_commonness.rare_very:
				list4.Add(i);
				break;
			case ChunkControl.spawn_commonness.DOUBLE:
				list.Add(i);
				list.Add(i);
				break;
			case ChunkControl.spawn_commonness.TRIPLE:
				list.Add(i);
				list.Add(i);
				list.Add(i);
				break;
			}
		}
		List<int> list5 = ((list4.Count != 0) ? list4 : list);
		List<int> list6 = ((list3.Count != 0) ? list3 : list);
		List<int> list7 = ((list2.Count != 0) ? list2 : list);
		for (int j = 0; j < budget; j++)
		{
			ChunkControl.biome_obj biome_obj = default(ChunkControl.biome_obj);
			bool flag = false;
			if (biome_id != 7 && biome_id != 4 && !is_scenic_layer)
			{
				if (Random.Range(0.001f, 1f) < 0.042f)
				{
					biome_obj = default(ChunkControl.biome_obj);
					biome_obj.item_name = "Titanium Chest";
					biome_obj.is_item = true;
					biome_obj.spawn_rate = ChunkControl.spawn_commonness.rare_very;
					biome_obj.dont_rotate = true;
					flag = true;
				}
				else if (Random.Range(0.001f, 1f) < 0.075f)
				{
					biome_obj = default(ChunkControl.biome_obj);
					biome_obj.item_name = "Gold Chest";
					biome_obj.is_item = true;
					biome_obj.spawn_rate = ChunkControl.spawn_commonness.rare_medium;
					biome_obj.dont_rotate = true;
					flag = true;
				}
				else if (depth > 15f && Random.Range(0.001f, 1f) < 0.096f)
				{
					biome_obj = default(ChunkControl.biome_obj);
					biome_obj.item_name = "Creature Nest";
					biome_obj.is_item = true;
					biome_obj.spawn_rate = ChunkControl.spawn_commonness.rare_slight;
					flag = true;
				}
			}
			if (!flag)
			{
				List<int> list8 = list;
				if (Random.value >= 0.62f)
				{
					list8 = list7;
					if (Random.value >= 0.7f)
					{
						list8 = list5;
						if (Random.value < 0.85f)
						{
							list8 = list6;
						}
					}
				}
				int index = list8[Random.Range(0, list8.Count)];
				biome_obj = object_list[index];
			}
			if (!(biome_obj.min_depth <= depth) || (InventoryUtils.IsCaveObject(biome_obj.item_name) && is_quest_miniworld))
			{
				continue;
			}
			Vector2 vector = Vector2.zero;
			int num = Random.Range(biome_obj.additional_clump_min, biome_obj.additional_clump_max + 1);
			string text = biome_obj.item_name;
			string value = "";
			if (text == "Flowers")
			{
				value = LootControl.Instance.paintbrushes_loot[Random.Range(0, LootControl.Instance.paintbrushes_loot.Count)].item_name;
			}
			bool flag2 = false;
			for (int k = 0; k < empty_locations.Count; k++)
			{
				Vector2 vector2 = empty_locations[k];
				int num2 = (int)vector2.x;
				int num3 = (int)vector2.y;
				int num4 = Random.Range(0, 4);
				if (biome_obj.dont_rotate)
				{
					num4 = 0;
				}
				List<Vector3> objectLocalGeometry = ConstructionControl.Instance.GetObjectLocalGeometry(text, num4);
				if (!AllEmpty(num2, num3, objectLocalGeometry, filled))
				{
					continue;
				}
				if (!flag2 || Vector3.Distance(vector2, vector) <= 3.5f)
				{
					InventoryItem item;
					if (text == "Gold Chest" || text == "Titanium Chest")
					{
						ExtraInventoryData extraInventoryData = new ExtraInventoryData();
						extraInventoryData.SetLong("basket_id", ConstructionControl.Instance.GetNewUniqueId());
						item = new InventoryItem(text, extraInventoryData);
					}
					else if (InventoryUtils.IsCaveObject(text))
					{
						int newUniqueId = ConstructionControl.Instance.GetNewUniqueId();
						ExtraInventoryData extraInventoryData2 = new ExtraInventoryData();
						extraInventoryData2.SetLong("shack_id", newUniqueId);
						extraInventoryData2.SetShort("outer_item_chunkX", chunkX);
						extraInventoryData2.SetShort("outer_item_chunkZ", chunkZ);
						extraInventoryData2.SetShort("outer_item_innerX", num2);
						extraInventoryData2.SetShort("outer_item_innerZ", num3);
						item = new InventoryItem(text, extraInventoryData2);
						ZoneData value2 = new ZoneData("shack" + newUniqueId, item, num4, output_chunkData.zone, chunkX, chunkZ, num2, num3);
						auto_built_zones.Add("shack" + newUniqueId, value2);
					}
					else if (text == "Flowers")
					{
						ExtraInventoryData extraInventoryData3 = new ExtraInventoryData();
						extraInventoryData3.SetString("paint", value);
						item = new InventoryItem(text, extraInventoryData3);
					}
					else
					{
						item = new InventoryItem(text);
					}
					output_chunkData.AddElement(num2, num3, new ChunkElement(item, num4));
					Fill(num2, num3, objectLocalGeometry, filled, empty_locations);
					if (!flag2)
					{
						vector = new Vector2(num2, num3);
						if (!Startup.StringNullOrWhitespace(biome_obj.clump_overwrite_obj))
						{
							text = biome_obj.clump_overwrite_obj;
						}
					}
					if (num == 0)
					{
						break;
					}
					num--;
				}
				flag2 = true;
			}
		}
	}

	private static bool AllEmpty(int origin_x, int origin_z, List<Vector3> geometry, bool[,] filled)
	{
		foreach (Vector3 item in geometry)
		{
			int num = (int)(item.x + (float)origin_x);
			int num2 = (int)(item.z + (float)origin_z);
			if (num2 >= 10 || num >= 10 || num2 < 0 || num < 0)
			{
				return false;
			}
		}
		foreach (Vector3 item2 in geometry)
		{
			int num3 = (int)(item2.x + (float)origin_x);
			int num4 = (int)(item2.z + (float)origin_z);
			if (filled[num3, num4])
			{
				return false;
			}
		}
		return true;
	}

	private static void Fill(int origin_x, int origin_z, List<Vector3> geometry, bool[,] filled, List<Vector2> empties)
	{
		foreach (Vector3 item in geometry)
		{
			int num = (int)(item.x + (float)origin_x);
			int num2 = (int)(item.z + (float)origin_z);
			filled[num, num2] = true;
			if (empties.Contains(new Vector2(num, num2)))
			{
				empties.Remove(new Vector2(num, num2));
			}
		}
	}
}
