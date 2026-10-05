using System;
using System.Collections.Generic;
using UnityEngine;

internal class delayed_redraw
{
	public enum process_status
	{
		more_to_process = 0,
		complete = 1,
		no_longer_exists = 2
	}

	public ModularObjectControl.type redraw_type;

	public string zone;

	public int chunkX;

	public int chunkZ;

	public ModularObjectControl.segment segment_to_redraw;

	public string chunkStr;

	private string suffix;

	private List<Vector2> squares_to_process;

	private Dictionary<string, PartiallyGeneratedModularModel> partially_generated_models;

	public process_status status;

	public delayed_redraw(ModularObjectControl.type redraw_type, string zone, int chunkX, int chunkZ, ModularObjectControl.segment segment_to_redraw)
	{
		this.redraw_type = redraw_type;
		this.zone = zone;
		this.chunkX = chunkX;
		this.chunkZ = chunkZ;
		this.segment_to_redraw = segment_to_redraw;
		squares_to_process = GetVecList(segment_to_redraw);
		partially_generated_models = new Dictionary<string, PartiallyGeneratedModularModel>();
		suffix = ModularObjectControl.Instance.GetSuffix(segment_to_redraw);
		chunkStr = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
	}

	public int Process(int remaining)
	{
		if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkStr))
		{
			status = process_status.no_longer_exists;
			return remaining;
		}
		do
		{
			if (squares_to_process.Count > 0)
			{
				ProcessOne(squares_to_process[0]);
				squares_to_process.RemoveAt(0);
				remaining--;
			}
			if (squares_to_process.Count == 0)
			{
				status = process_status.complete;
				break;
			}
		}
		while (remaining != 0);
		return remaining;
	}

	private void ProcessOne(Vector3 V)
	{
		int num = (int)V.x;
		int num2 = (int)V.y;
		ChunkData chunkData = ChunkControl.Instance.GetChunkData(chunkStr);
		foreach (ChunkElement item in chunkData.GetElementsAt(num, num2))
		{
			if (redraw_type == ModularObjectControl.type.WALLS)
			{
				if (!inventory_ctr.Instance.GetItemBool(item.item.item_name, "is_wall_obj"))
				{
					continue;
				}
			}
			else if (redraw_type != ModularObjectControl.type.PATHWAYS || !inventory_ctr.Instance.GetItemBool(item.item.item_name, "is_flooring_obj"))
			{
				continue;
			}
			PartiallyGeneratedModularModel new_model;
			string text = item.item.item_name + suffix + "(" + item.item.GetString("paint") + "/" + item.item.GetString("stamp") + ")";
			if (!partially_generated_models.ContainsKey(text))
			{
				new_model = new PartiallyGeneratedModularModel(item.item, segment_to_redraw, text, zone, chunkX, chunkZ, chunkData.biome);
				partially_generated_models.Add(text, new_model);
				new_model.n_required++;
				Action<GameObject> on_complete = delegate
				{
					new_model.OnMeshSegmentAdded();
				};
				if (redraw_type == ModularObjectControl.type.PATHWAYS)
				{
					ModularObjectControl.Instance.AsyncLoadModularModel("Pathway-Prefabs/" + item.item.item_name, on_complete);
				}
				else if (redraw_type == ModularObjectControl.type.WALLS)
				{
					ModularObjectControl.Instance.AsyncLoadModularModel("Wall-Prefabs/" + item.item.item_name, on_complete);
				}
			}
			else
			{
				new_model = partially_generated_models[text];
			}
			new_model.n_required_set = false;
			string item_name = item.item.item_name;
			if (redraw_type == ModularObjectControl.type.PATHWAYS)
			{
				string key = new string(new char[8]
				{
					IsFilledWithSimilarObject(zone, 1, -1, chunkX, chunkZ, num, num2, item_name),
					IsFilledWithSimilarObject(zone, 1, 0, chunkX, chunkZ, num, num2, item_name),
					IsFilledWithSimilarObject(zone, 1, 1, chunkX, chunkZ, num, num2, item_name),
					IsFilledWithSimilarObject(zone, 0, -1, chunkX, chunkZ, num, num2, item_name),
					IsFilledWithSimilarObject(zone, 0, 1, chunkX, chunkZ, num, num2, item_name),
					IsFilledWithSimilarObject(zone, -1, -1, chunkX, chunkZ, num, num2, item_name),
					IsFilledWithSimilarObject(zone, -1, 0, chunkX, chunkZ, num, num2, item_name),
					IsFilledWithSimilarObject(zone, -1, 1, chunkX, chunkZ, num, num2, item_name)
				});
				if (!ModularObjectControl.Instance.pathway_bend_types.ContainsKey(key))
				{
					continue;
				}
				ModularObjectControl.pathway_bend_type pathway_bend_type = ModularObjectControl.Instance.pathway_bend_types[key];
				ModularObjectControl.Instance.GetPathwayItem(item_name);
				string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item_name, "flooring_model");
				int rot = pathway_bend_type.rot;
				if (pathway_bend_type.sprite_index == 1 || pathway_bend_type.sprite_index == 13)
				{
					rot = UnityEngine.Random.Range(0, 4);
				}
				else if (pathway_bend_type.sprite_index == 3)
				{
					if (pathway_bend_type.rot == 1)
					{
						rot = ((UnityEngine.Random.value < 0.5f) ? 3 : 1);
					}
					else if (pathway_bend_type.rot == 0)
					{
						rot = ((UnityEngine.Random.value < 0.5f) ? 2 : 0);
					}
				}
				new_model.n_required++;
				int x_cache = num;
				int z_cache = num2;
				int uv_x_cache = 0;
				int uv_y_cache = 0;
				switch (pathway_bend_type.sprite_index)
				{
				case 1:
					uv_x_cache = 0;
					uv_y_cache = 1;
					break;
				case 2:
					uv_x_cache = 4;
					uv_y_cache = 0;
					break;
				case 3:
					uv_x_cache = 0;
					uv_y_cache = 0;
					break;
				case 4:
					uv_x_cache = 1;
					uv_y_cache = 1;
					break;
				case 5:
					uv_x_cache = 3;
					uv_y_cache = 0;
					break;
				case 6:
					uv_x_cache = 4;
					uv_y_cache = 2;
					break;
				case 7:
					uv_x_cache = 2;
					uv_y_cache = 1;
					break;
				case 8:
					uv_x_cache = 3;
					uv_y_cache = 2;
					break;
				case 9:
					uv_x_cache = 1;
					uv_y_cache = 0;
					break;
				case 10:
					uv_x_cache = 2;
					uv_y_cache = 2;
					break;
				case 11:
					uv_x_cache = 3;
					uv_y_cache = 1;
					break;
				case 12:
					uv_x_cache = 4;
					uv_y_cache = 1;
					break;
				case 13:
					uv_x_cache = 2;
					uv_y_cache = 0;
					break;
				case 14:
					uv_x_cache = 1;
					uv_y_cache = 2;
					break;
				case 15:
					uv_x_cache = 0;
					uv_y_cache = 2;
					break;
				}
				int rot_cache = rot;
				ModularObjectControl.Instance.AsyncLoadModularModel(stringFromItemFile + "/" + pathway_bend_type.sprite_index + "_prefab", delegate(GameObject new_obj)
				{
					ModularObjectControl.Instance.AddVertices(new_obj, x_cache, z_cache, uv_x_cache, uv_y_cache, rot_cache, new_model);
					new_model.OnMeshSegmentAdded();
				});
			}
			else
			{
				if (redraw_type != ModularObjectControl.type.WALLS)
				{
					continue;
				}
				bool flag = IsFilledWithSimilarObject(zone, 0, -1, chunkX, chunkZ, num, num2, item_name) == '1';
				bool flag2 = IsFilledWithSimilarObject(zone, 0, 1, chunkX, chunkZ, num, num2, item_name) == '1';
				bool flag3 = IsFilledWithSimilarObject(zone, 1, 0, chunkX, chunkZ, num, num2, item_name) == '1';
				bool flag4 = IsFilledWithSimilarObject(zone, -1, 0, chunkX, chunkZ, num, num2, item_name) == '1';
				int num3 = 0;
				if (flag)
				{
					num3++;
				}
				if (flag2)
				{
					num3++;
				}
				if (flag3)
				{
					num3++;
				}
				if (flag4)
				{
					num3++;
				}
				string text2 = "0";
				int num4 = 0;
				switch (num3)
				{
				case 0:
					text2 = "0";
					break;
				case 1:
					text2 = "1";
					if (flag)
					{
						num4 = 2;
					}
					else if (flag4)
					{
						num4 = 3;
					}
					else if (flag3)
					{
						num4 = 1;
					}
					else
					{
						num4 = 0;
					}
					break;
				case 2:
					if (flag && flag2)
					{
						text2 = "2a";
					}
					else if (flag3 && flag4)
					{
						text2 = "2a";
						num4 = 1;
					}
					else if (flag2 && flag4)
					{
						text2 = "2b";
					}
					else if (flag2 && flag3)
					{
						text2 = "2b";
						num4 = 1;
					}
					else if (flag && flag3)
					{
						text2 = "2b";
						num4 = 2;
					}
					else if (flag && flag4)
					{
						text2 = "2b";
						num4 = 3;
					}
					break;
				case 3:
					text2 = "3";
					if (flag && flag3 && flag4)
					{
						num4 = 1;
					}
					else if (!flag || !flag2 || !flag3)
					{
						if (flag3 && flag2 && flag4)
						{
							num4 = 3;
						}
						else if (flag && flag2 && flag4)
						{
							num4 = 2;
						}
					}
					break;
				case 4:
					text2 = "4b";
					break;
				}
				new_model.n_required++;
				int x_cache2 = num;
				int z_cache2 = num2;
				int rot_cache2 = num4;
				ModularObjectControl.Instance.AsyncLoadModularModel("Wall-Models/" + item_name + "/" + text2 + "_prefab", delegate(GameObject new_obj)
				{
					ModularObjectControl.Instance.AddVertices(new_obj, x_cache2, z_cache2, 0, 0, rot_cache2, new_model);
					new_model.OnMeshSegmentAdded();
				});
			}
		}
	}

	public void FinalizeAll()
	{
		foreach (KeyValuePair<string, PartiallyGeneratedModularModel> partially_generated_model in partially_generated_models)
		{
			PartiallyGeneratedModularModel value = partially_generated_model.Value;
			value.n_required_set = true;
			if (value.n_complete == value.n_required)
			{
				if (inventory_ctr.Instance.GetItemBool(value.item.item_name, "is_flooring_obj"))
				{
					ModularObjectControl.Instance.FinalizePathway(value);
				}
				else
				{
					ModularObjectControl.Instance.FinalizeWalls(value);
				}
			}
		}
	}

	public void ProcessPaintings()
	{
		List<Vector2> list = new List<Vector2>();
		switch (segment_to_redraw)
		{
		case ModularObjectControl.segment.top_edge:
			for (int k = 0; k < 10; k++)
			{
				list.Add(new Vector2(k, 8f));
				list.Add(new Vector2(k, 10f));
			}
			list.Add(new Vector2(0f, 9f));
			list.Add(new Vector2(9f, 9f));
			break;
		case ModularObjectControl.segment.bottom_edge:
			for (int m = 0; m < 10; m++)
			{
				list.Add(new Vector2(m, -1f));
				list.Add(new Vector2(m, 1f));
			}
			list.Add(new Vector2(0f, 0f));
			list.Add(new Vector2(9f, 0f));
			break;
		case ModularObjectControl.segment.left_edge:
			for (int l = 0; l < 10; l++)
			{
				list.Add(new Vector2(-1f, l));
				list.Add(new Vector2(1f, l));
			}
			list.Add(new Vector2(0f, 0f));
			list.Add(new Vector2(0f, 9f));
			break;
		case ModularObjectControl.segment.right_edge:
			for (int n = 0; n < 10; n++)
			{
				list.Add(new Vector2(8f, n));
				list.Add(new Vector2(10f, n));
			}
			list.Add(new Vector2(9f, 0f));
			list.Add(new Vector2(9f, 9f));
			break;
		case ModularObjectControl.segment.topLeft_corner:
			list.Add(new Vector2(-1f, 10f));
			list.Add(new Vector2(0f, 10f));
			list.Add(new Vector2(1f, 10f));
			list.Add(new Vector2(-1f, 9f));
			list.Add(new Vector2(1f, 9f));
			list.Add(new Vector2(-1f, 8f));
			list.Add(new Vector2(0f, 8f));
			list.Add(new Vector2(1f, 8f));
			break;
		case ModularObjectControl.segment.topRight_corner:
			list.Add(new Vector2(8f, 10f));
			list.Add(new Vector2(9f, 10f));
			list.Add(new Vector2(10f, 10f));
			list.Add(new Vector2(8f, 9f));
			list.Add(new Vector2(10f, 9f));
			list.Add(new Vector2(8f, 8f));
			list.Add(new Vector2(9f, 8f));
			list.Add(new Vector2(10f, 8f));
			break;
		case ModularObjectControl.segment.bottomLeft_corner:
			list.Add(new Vector2(-1f, 1f));
			list.Add(new Vector2(0f, 1f));
			list.Add(new Vector2(1f, 1f));
			list.Add(new Vector2(-1f, 0f));
			list.Add(new Vector2(1f, 0f));
			list.Add(new Vector2(-1f, -1f));
			list.Add(new Vector2(0f, -1f));
			list.Add(new Vector2(1f, -1f));
			break;
		case ModularObjectControl.segment.bottomRight_corner:
			list.Add(new Vector2(8f, 1f));
			list.Add(new Vector2(9f, 1f));
			list.Add(new Vector2(10f, 1f));
			list.Add(new Vector2(8f, 0f));
			list.Add(new Vector2(10f, 0f));
			list.Add(new Vector2(8f, -1f));
			list.Add(new Vector2(9f, -1f));
			list.Add(new Vector2(10f, -1f));
			break;
		case ModularObjectControl.segment.center_chunk:
			for (int i = -1; i < 11; i++)
			{
				for (int j = -1; j < 11; j++)
				{
					list.Add(new Vector2(i, j));
				}
			}
			break;
		}
		foreach (Vector2 item in list)
		{
			int num = (int)item.x;
			int num2 = (int)item.y;
			int num3 = chunkX;
			int num4 = chunkZ;
			if (num == -1)
			{
				num3--;
				num = 9;
			}
			if (num2 == -1)
			{
				num4--;
				num2 = 9;
			}
			if (num == 10)
			{
				num3++;
				num = 0;
			}
			if (num2 == 10)
			{
				num4++;
				num2 = 0;
			}
			string chunkString = ChunkControl.Instance.GetChunkString(zone, num3, num4);
			if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
			{
				continue;
			}
			ChunkData chunkData = ChunkControl.Instance.GetChunkData(chunkString);
			ChunkObj chunkObj = ChunkControl.Instance.GetChunkObj(chunkString);
			InventoryItem inventoryItem = null;
			foreach (ChunkElement item2 in chunkData.GetElementsAt(num, num2))
			{
				if (item2.item.item_name == "Painting")
				{
					inventoryItem = item2.item;
					break;
				}
			}
			if (inventoryItem != null)
			{
				GameObject buildableInstanceByItem = chunkObj.GetBuildableInstanceByItem(num, num2, inventoryItem);
				if (buildableInstanceByItem != null)
				{
					ConstructionControl.Instance.SnapPainting(buildableInstanceByItem, num3, num4, num, num2);
				}
			}
		}
	}

	private List<Vector2> GetVecList(ModularObjectControl.segment seg)
	{
		List<Vector2> list = new List<Vector2>();
		switch (seg)
		{
		case ModularObjectControl.segment.top_edge:
			for (int k = 1; k < 9; k++)
			{
				list.Add(new Vector2(k, 9f));
			}
			break;
		case ModularObjectControl.segment.bottom_edge:
			for (int m = 1; m < 9; m++)
			{
				list.Add(new Vector2(m, 0f));
			}
			break;
		case ModularObjectControl.segment.left_edge:
			for (int l = 1; l < 9; l++)
			{
				list.Add(new Vector2(0f, l));
			}
			break;
		case ModularObjectControl.segment.right_edge:
			for (int n = 1; n < 9; n++)
			{
				list.Add(new Vector2(9f, n));
			}
			break;
		case ModularObjectControl.segment.topLeft_corner:
			list.Add(new Vector2(0f, 9f));
			break;
		case ModularObjectControl.segment.topRight_corner:
			list.Add(new Vector2(9f, 9f));
			break;
		case ModularObjectControl.segment.bottomLeft_corner:
			list.Add(new Vector2(0f, 0f));
			break;
		case ModularObjectControl.segment.bottomRight_corner:
			list.Add(new Vector2(9f, 0f));
			break;
		case ModularObjectControl.segment.center_chunk:
			for (int i = 1; i < 9; i++)
			{
				for (int j = 1; j < 9; j++)
				{
					list.Add(new Vector2(i, j));
				}
			}
			break;
		}
		return list;
	}

	private char IsFilledWithSimilarObject(string zone, int diffZ, int diffX, int chunkX, int chunkZ, int innerX, int innerZ, string curr_item)
	{
		innerX += diffX;
		innerZ += diffZ;
		if (innerX < 0)
		{
			chunkX--;
			innerX += 10;
		}
		else if (innerX > 9)
		{
			chunkX++;
			innerX -= 10;
		}
		if (innerZ < 0)
		{
			chunkZ--;
			innerZ += 10;
		}
		else if (innerZ > 9)
		{
			chunkZ++;
			innerZ -= 10;
		}
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			return '0';
		}
		foreach (ChunkElement item in ChunkControl.Instance.GetChunkData(chunkString).GetElementsAt(innerX, innerZ))
		{
			string item_name = item.item.item_name;
			if (curr_item == "Dirt Path" || curr_item == "Cobblestone Path" || curr_item == "Stone Bricks" || curr_item == "Bouncy Floor")
			{
				if (item_name == "Dirt Path" || item_name == "Cobblestone Path" || item_name == "Stone Bricks" || item_name == "Bouncy Floor")
				{
					return '1';
				}
			}
			else if (curr_item == item_name)
			{
				return '1';
			}
		}
		return '0';
	}
}
