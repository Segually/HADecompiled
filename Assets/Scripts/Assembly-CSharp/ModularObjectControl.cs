using System;
using System.Collections.Generic;
using UnityEngine;

public class ModularObjectControl : MonoBehaviour, OrderedStart
{
	[Serializable]
	public struct flooring_item
	{
		public string item_name;

		public GameObject particle_obj;

		public GameObject particle_obj_2;
	}

	public struct pathway_bend_type
	{
		public int sprite_index;

		public int rot;
	}

	public enum segment
	{
		top_edge = 0,
		bottom_edge = 1,
		left_edge = 2,
		right_edge = 3,
		topLeft_corner = 4,
		topRight_corner = 5,
		bottomLeft_corner = 6,
		bottomRight_corner = 7,
		center_chunk = 8,
		undefined = 9
	}

	public enum type
	{
		PATHWAYS = 0,
		WALLS = 1
	}

	public static ModularObjectControl Instance;

	private int max_modular_process;

	private GameObject pathway_prefabs_folder;

	public Dictionary<string, pathway_bend_type> pathway_bend_types = new Dictionary<string, pathway_bend_type>();

	public flooring_item[] flooring_items;

	public Dictionary<string, GameObject> loaded_mesh_objects = new Dictionary<string, GameObject>();

	private List<string> delayed_redraws_keys = new List<string>();

	private Dictionary<string, delayed_redraw> delayed_redraws = new Dictionary<string, delayed_redraw>();

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("pathway_configurations", ref file_exists);
		if (file_exists)
		{
			pathway_bend_type value = default(pathway_bend_type);
			foreach (string item in textFileLines)
			{
				if (!Startup.StringNullOrWhitespace(item))
				{
					if (item[0] == '[')
					{
						int num = item.IndexOf(',');
						value.sprite_index = int.Parse(item.Substring(1, num - 1), Startup.parse_culture);
						value.rot = int.Parse(item.Substring(num + 1, item.Length - num - 2), Startup.parse_culture);
					}
					else
					{
						pathway_bend_types.Add(item, value);
					}
				}
			}
		}
		pathway_prefabs_folder = new GameObject("Pathway-prefabs");
		max_modular_process = 18;
	}

	public flooring_item GetPathwayItem(string item_name)
	{
		for (int i = 0; i < flooring_items.Length; i++)
		{
			if (flooring_items[i].item_name == item_name)
			{
				return flooring_items[i];
			}
		}
		return flooring_items[0];
	}

	public void ModularChangedAt(int x, int z, type type, string zone, int chunkX, int chunkZ)
	{
		TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.center_chunk);
		if (x == 0 && z == 0)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottomLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottom_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.left_edge);
			TryRedrawAtDelayed(type, zone, chunkX - 1, chunkZ, segment.bottomRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX - 1, chunkZ, segment.right_edge);
			TryRedrawAtDelayed(type, zone, chunkX - 1, chunkZ - 1, segment.topRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ - 1, segment.topLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ - 1, segment.top_edge);
		}
		else if (x == 0 && z == 9)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.topLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.top_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.left_edge);
			TryRedrawAtDelayed(type, zone, chunkX - 1, chunkZ, segment.topRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX - 1, chunkZ, segment.right_edge);
			TryRedrawAtDelayed(type, zone, chunkX - 1, chunkZ + 1, segment.bottomRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ + 1, segment.bottomLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ + 1, segment.bottom_edge);
		}
		else if (x == 9 && z == 0)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottomRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottom_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.right_edge);
			TryRedrawAtDelayed(type, zone, chunkX + 1, chunkZ, segment.bottomLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX + 1, chunkZ, segment.left_edge);
			TryRedrawAtDelayed(type, zone, chunkX + 1, chunkZ - 1, segment.topLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ - 1, segment.topRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ - 1, segment.top_edge);
		}
		else if (x == 9 && z == 9)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.topRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.top_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.right_edge);
			TryRedrawAtDelayed(type, zone, chunkX + 1, chunkZ, segment.topLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX + 1, chunkZ, segment.left_edge);
			TryRedrawAtDelayed(type, zone, chunkX + 1, chunkZ + 1, segment.bottomLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ + 1, segment.bottomRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ + 1, segment.bottom_edge);
		}
		else if (x == 0 && z == 1)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottomLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottom_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.left_edge);
			TryRedrawAtDelayed(type, zone, chunkX - 1, chunkZ, segment.bottomRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX - 1, chunkZ, segment.right_edge);
		}
		else if (x == 0 && z == 8)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.topLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.top_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.left_edge);
			TryRedrawAtDelayed(type, zone, chunkX - 1, chunkZ, segment.topRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX - 1, chunkZ, segment.right_edge);
		}
		else if (x == 9 && z == 1)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottomRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottom_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.right_edge);
			TryRedrawAtDelayed(type, zone, chunkX + 1, chunkZ, segment.bottomLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX + 1, chunkZ, segment.left_edge);
		}
		else if (x == 9 && z == 8)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.topRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.top_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.right_edge);
			TryRedrawAtDelayed(type, zone, chunkX + 1, chunkZ, segment.topLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX + 1, chunkZ, segment.left_edge);
		}
		else if (x == 1 && z == 0)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottomLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottom_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.left_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ - 1, segment.topLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ - 1, segment.top_edge);
		}
		else if (x == 1 && z == 9)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.topLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.top_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.left_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ + 1, segment.bottomLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ + 1, segment.bottom_edge);
		}
		else if (x == 8 && z == 0)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottomRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottom_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.right_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ - 1, segment.topRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ - 1, segment.top_edge);
		}
		else if (x == 8 && z == 9)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.topRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.top_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.right_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ + 1, segment.bottomRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ + 1, segment.bottom_edge);
		}
		else if (x == 1 && z == 1)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottomLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottom_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.left_edge);
		}
		else if (x == 1 && z == 8)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.topLeft_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.top_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.left_edge);
		}
		else if (x == 8 && z == 1)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottomRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottom_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.right_edge);
		}
		else if (x == 8 && z == 8)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.topRight_corner);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.top_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.right_edge);
		}
		else if (x == 0)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.left_edge);
			TryRedrawAtDelayed(type, zone, chunkX - 1, chunkZ, segment.right_edge);
		}
		else if (x == 9)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.right_edge);
			TryRedrawAtDelayed(type, zone, chunkX + 1, chunkZ, segment.left_edge);
		}
		else if (z == 9)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.top_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ + 1, segment.bottom_edge);
		}
		else if (z == 0)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottom_edge);
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ - 1, segment.top_edge);
		}
		else if (x == 1)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.left_edge);
		}
		else if (x == 8)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.right_edge);
		}
		else if (z == 1)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.bottom_edge);
		}
		else if (z == 8)
		{
			TryRedrawAtDelayed(type, zone, chunkX, chunkZ, segment.top_edge);
		}
	}

	private void SetComplete(string chunkStr, segment set_segment, type set_type)
	{
		Chunk chunk = ChunkControl.Instance.GetChunk(chunkStr);
		string item = set_type.ToString() + "," + set_segment;
		chunk.mid_load_modulars.Remove(item);
		if (chunk.all_modulars_accounted_for && chunk.mid_load_modulars.Count == 0)
		{
			ChunkControl.Instance.ChangeChunkStatus(chunkStr, Chunk.status_t.async_building_modularsComplete);
		}
	}

	private void FixedUpdate()
	{
		if (delayed_redraws.Count == 0)
		{
			return;
		}
		int num = max_modular_process;
		while (num > 0 && delayed_redraws.Count > 0)
		{
			string key = delayed_redraws_keys[0];
			delayed_redraw delayed_redraw = delayed_redraws[key];
			num = delayed_redraw.Process(num);
			if (delayed_redraw.status == delayed_redraw.process_status.complete)
			{
				delayed_redraw.FinalizeAll();
				if (delayed_redraw.redraw_type == type.WALLS)
				{
					delayed_redraw.ProcessPaintings();
				}
				delayed_redraws.Remove(key);
				delayed_redraws_keys.RemoveAt(0);
				SetComplete(delayed_redraw.chunkStr, delayed_redraw.segment_to_redraw, delayed_redraw.redraw_type);
			}
			else if (delayed_redraw.status == delayed_redraw.process_status.no_longer_exists)
			{
				delayed_redraws.Remove(key);
				delayed_redraws_keys.RemoveAt(0);
			}
		}
	}

	public void TryRedrawAtDelayed(type redraw_type, string zone, int chunkX, int chunkZ, segment segment_to_redraw)
	{
		string text = redraw_type.ToString() + "," + zone + "," + chunkX + "," + chunkZ + "," + segment_to_redraw;
		if (delayed_redraws_keys.Contains(text))
		{
			return;
		}
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			return;
		}
		ChunkObj chunkObj = ChunkControl.Instance.GetChunkObj(chunkString);
		Dictionary<string, GameObject> dictionary = null;
		switch (redraw_type)
		{
		case type.WALLS:
			dictionary = chunkObj.wall_segments;
			break;
		case type.PATHWAYS:
			dictionary = chunkObj.pathway_segments;
			break;
		}
		string suffix = GetSuffix(segment_to_redraw);
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, GameObject> item in dictionary)
		{
			if (item.Key.Contains(suffix))
			{
				list.Add(item.Key);
			}
		}
		foreach (string item2 in list)
		{
			if (dictionary[item2] != null)
			{
				UnityEngine.Object.Destroy(dictionary[item2]);
			}
			dictionary.Remove(item2);
		}
		delayed_redraw value = new delayed_redraw(redraw_type, zone, chunkX, chunkZ, segment_to_redraw);
		delayed_redraws.Add(text, value);
		delayed_redraws_keys.Add(text);
		ChunkControl.Instance.GetChunk(chunkString).mid_load_modulars.Add(redraw_type.ToString() + "," + segment_to_redraw);
	}

	public string GetSuffix(segment seg)
	{
		return seg switch
		{
			segment.top_edge => "-top_edge",
			segment.bottom_edge => "-bottom_edge",
			segment.left_edge => "-left_edge",
			segment.right_edge => "-right_edge",
			segment.topLeft_corner => "-topLeft_corner",
			segment.topRight_corner => "-topRight_corner",
			segment.bottomLeft_corner => "-bottomLeft_corner",
			segment.bottomRight_corner => "-bottomRight_corner",
			segment.center_chunk => "-center",
			_ => "",
		};
	}

	public void AsyncLoadModularModel(string mesh_path, Action<GameObject> on_complete)
	{
		if (!loaded_mesh_objects.ContainsKey(mesh_path))
		{
			GameObject check_not_null = pathway_prefabs_folder;
			ResourceControl.Instance.AsyncInstantiateModularPrefab(mesh_path, delegate(GameObject pathway_mesh_instance)
			{
				if (check_not_null == null)
				{
					UnityEngine.Object.Destroy(pathway_mesh_instance);
				}
				else
				{
					if (!loaded_mesh_objects.ContainsKey(mesh_path))
					{
						pathway_mesh_instance.transform.SetParent(pathway_prefabs_folder.transform);
						pathway_mesh_instance.name = mesh_path;
						pathway_mesh_instance.SetActive(false);
						loaded_mesh_objects.Add(mesh_path, pathway_mesh_instance);
					}
					else
					{
						UnityEngine.Object.Destroy(pathway_mesh_instance);
					}
					on_complete(pathway_mesh_instance);
				}
			});
		}
		else
		{
			on_complete(loaded_mesh_objects[mesh_path]);
		}
	}

	public void AddVertices(GameObject loaded_prefab, int x, int z, int uv_x, int uv_y, int rot, PartiallyGeneratedModularModel new_model)
	{
		int num = 1;
		if (loaded_prefab.transform.Find("obj2") != null)
		{
			num = ((!(loaded_prefab.transform.Find("obj3") != null)) ? 2 : 3);
		}
		if (new_model.sub_objects.Length != num)
		{
			PartiallyGeneratedModularModelSubObj[] array = new PartiallyGeneratedModularModelSubObj[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = new PartiallyGeneratedModularModelSubObj();
			}
			for (int j = 0; j < new_model.sub_objects.Length; j++)
			{
				array[j] = new_model.sub_objects[j];
			}
			new_model.sub_objects = array;
		}
		for (int k = 0; k < num; k++)
		{
			Mesh mesh = loaded_prefab.transform.Find("obj" + (k + 1)).GetComponent<MeshFilter>().mesh;
			PartiallyGeneratedModularModelSubObj partiallyGeneratedModularModelSubObj = new_model.sub_objects[k];
			int subMeshCount = mesh.subMeshCount;
			List<Vector3> list = new List<Vector3>();
			Vector3[] vertices = mesh.vertices;
			for (int l = 0; l < vertices.Length; l++)
			{
				list.Add(vertices[l]);
			}
			for (int m = 0; m < list.Count; m++)
			{
				list[m] = Quaternion.Euler(-90f, 0f, 0f) * list[m];
				list[m] = Quaternion.Euler(0f, rot * -90, 0f) * list[m];
				list[m] *= 0.5f;
				list[m] += new Vector3((float)x + 0.5f, 0f, (float)z + 0.5f);
			}
			List<Vector3> list2 = new List<Vector3>();
			Vector3[] normals = mesh.normals;
			for (int n = 0; n < normals.Length; n++)
			{
				list2.Add(normals[n]);
			}
			for (int num2 = 0; num2 < list2.Count; num2++)
			{
				list2[num2] = Quaternion.Euler(-90f, 0f, 0f) * list2[num2];
				list2[num2] = Quaternion.Euler(0f, rot * -90, 0f) * list2[num2];
			}
			if (partiallyGeneratedModularModelSubObj.multi_mat_triangles_.Count < subMeshCount)
			{
				partiallyGeneratedModularModelSubObj.multi_mat_triangles_.Clear();
				for (int num3 = 0; num3 < subMeshCount; num3++)
				{
					partiallyGeneratedModularModelSubObj.multi_mat_triangles_.Add(new List<int>());
				}
			}
			if (partiallyGeneratedModularModelSubObj.multi_UVs.Count < 4)
			{
				partiallyGeneratedModularModelSubObj.multi_UVs.Clear();
				for (int num4 = 0; num4 < 4; num4++)
				{
					partiallyGeneratedModularModelSubObj.multi_UVs.Add(new List<Vector2>());
				}
			}
			int count = partiallyGeneratedModularModelSubObj.vertices.Count;
			foreach (Vector3 item in list)
			{
				partiallyGeneratedModularModelSubObj.vertices.Add(item);
			}
			for (int num5 = 0; num5 < subMeshCount; num5++)
			{
				int[] triangles = mesh.GetTriangles(num5);
				for (int num6 = 0; num6 < triangles.Length; num6++)
				{
					partiallyGeneratedModularModelSubObj.multi_mat_triangles_[num5].Add(triangles[num6] + count);
				}
			}
			foreach (Vector3 item2 in list2)
			{
				partiallyGeneratedModularModelSubObj.normals.Add(item2);
			}
			for (int num7 = 0; num7 < 4; num7++)
			{
				List<Vector2> list3 = new List<Vector2>();
				mesh.GetUVs(num7, list3);
				if (num7 == 0)
				{
					foreach (Vector2 item3 in list3)
					{
						partiallyGeneratedModularModelSubObj.multi_UVs[0].Add(new Vector2((float)uv_x * 0.1953125f + item3.x, item3.y - (float)uv_y * 0.1953125f));
					}
					continue;
				}
				foreach (Vector2 item4 in list3)
				{
					partiallyGeneratedModularModelSubObj.multi_UVs[num7].Add(item4);
				}
			}
		}
		new_model.filled_locations.Add(new Vector2(x, z));
	}

	public void CreateMesh(GameObject final_object, PartiallyGeneratedModularModel new_model, bool inherit_color_from_house)
	{
		for (int i = 0; i < new_model.sub_objects.Length; i++)
		{
			PartiallyGeneratedModularModelSubObj partiallyGeneratedModularModelSubObj = new_model.sub_objects[i];
			Transform transform = final_object.transform.Find("obj" + (i + 1));
			int subMeshCount = transform.GetComponent<MeshRenderer>().materials.Length;
			Mesh mesh = new Mesh();
			mesh.subMeshCount = subMeshCount;
			mesh.vertices = partiallyGeneratedModularModelSubObj.vertices.ToArray();
			for (int j = 0; j < partiallyGeneratedModularModelSubObj.multi_mat_triangles_.Count; j++)
			{
				mesh.SetTriangles(partiallyGeneratedModularModelSubObj.multi_mat_triangles_[j].ToArray(), j);
			}
			mesh.normals = partiallyGeneratedModularModelSubObj.normals.ToArray();
			for (int k = 0; k < partiallyGeneratedModularModelSubObj.multi_UVs.Count; k++)
			{
				mesh.SetUVs(k, partiallyGeneratedModularModelSubObj.multi_UVs[k]);
			}
			transform.GetComponent<MeshFilter>().mesh = mesh;
		}
	}

	public void FinalizePathway(PartiallyGeneratedModularModel new_model)
	{
		string chunkString = ChunkControl.Instance.GetChunkString(new_model.zone, new_model.chunkX, new_model.chunkZ);
		ChunkObj chunkObj = ChunkControl.Instance.GetChunkObj(chunkString);
		if (chunkObj == null || chunkObj.pathway_segments.ContainsKey(new_model.unique_key))
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(loaded_mesh_objects["Pathway-Prefabs/" + new_model.item.item_name]);
		gameObject.SetActive(true);
		gameObject.transform.position = new Vector3(new_model.chunkX * 10, 0f, new_model.chunkZ * 10);
		gameObject.transform.SetParent(chunkObj.parent_obj.transform);
		gameObject.name = "(pathways) " + new_model.unique_key;
		chunkObj.pathway_segments.Add(new_model.unique_key, gameObject);
		CreateMesh(gameObject, new_model, false);
		PaintableObject component = gameObject.GetComponent<PaintableObject>();
		string paint = new_model.item.GetString("paint");
		BanditCampsControl.Instance.ModifyIfBanditPaint(ref paint, new_model.item.item_name, new_model.item.GetString("bandit_camp_instance"));
		if (paint == "")
		{
			bool flag = ChunkControl.Instance.player_zone == "overworld";
			string item_name = new_model.item.item_name;
			int num = -1;
			if (item_name == "Dirt Path" || item_name == "Cobblestone Path" || item_name == "Stone Bricks")
			{
				if (flag)
				{
					num = new_model.biome_id;
				}
				else
				{
					switch (ZoneDataControl.Instance.curr_zonedata.house_item.item_name)
					{
					case "Grass Cave Entrance":
						num = 0;
						break;
					case "Snow Cave Entrance":
					case "Magic Bean":
					case "Pocket World Snow":
						num = 1;
						break;
					case "Desert Cave Entrance":
						num = 2;
						break;
					case "Evergreen Cave Entrance":
					case "Pocket World Evergreen":
						num = 3;
						break;
					case "Ocean Cave Entrance":
						num = 4;
						break;
					case "Swamp Cave Entrance":
					case "Spooky Well":
						num = 6;
						break;
					}
				}
			}
			string text = null;
			if (item_name == "Dirt Path")
			{
				switch (num)
				{
				case 0:
					text = "Biome - Dirt on Grass";
					break;
				case 1:
					text = "Biome - Dirt on Snow";
					break;
				case 2:
					text = "Biome - Dirt on Desert";
					break;
				case 3:
					text = "Biome - Dirt on Evergreen";
					break;
				case 4:
				case 5:
					text = "Biome - Dirt on Ocean";
					break;
				case 6:
				case 7:
					text = "Biome - Dirt on Swamp";
					break;
				case 8:
					text = "Biome - Dirt on Woodlands";
					break;
				case 9:
					text = "Biome - Dirt on Sakura";
					break;
				}
			}
			else if (item_name == "Cobblestone Path")
			{
				switch (num)
				{
				case 0:
					text = "Biome - Cobble on Grass";
					break;
				case 1:
					text = "Biome - Cobble on Snow";
					break;
				case 2:
					text = "Biome - Cobble on Desert";
					break;
				case 3:
					text = "Biome - Cobble on Evergreen";
					break;
				case 4:
				case 5:
					text = "Biome - Cobble on Ocean";
					break;
				case 6:
				case 7:
					text = "Biome - Cobble on Swamp";
					break;
				case 8:
					text = "Biome - Cobble on Woodlands";
					break;
				case 9:
					text = "Biome - Cobble on Sakura";
					break;
				}
			}
			else if (item_name == "Stone Bricks")
			{
				switch (num)
				{
				case 0:
					text = "Biome - Slab on Grass";
					break;
				case 1:
					text = "Biome - Slab on Snow";
					break;
				case 2:
					text = "Biome - Slab on Desert";
					break;
				case 3:
					text = "Biome - Slab on Evergreen";
					break;
				case 4:
				case 5:
					text = "Biome - Slab on Ocean";
					break;
				case 6:
				case 7:
					text = "Biome - Slab on Swamp";
					break;
				case 8:
					text = "Biome - Slab on Woodlands";
					break;
				case 9:
					text = "Biome - Slab on Sakura";
					break;
				}
			}
			paint = text ?? inventory_ctr.Instance.GetPaintFromItemOrUseDefault(new_model.item);
		}
		component.Colorize(paint, inventory_ctr.Instance.GetStampFromItem(new_model.item), inventory_ctr.Instance.GetLayoutItemFromItem(new_model.item.item_name), new_model.item.item_name);
		GameObject particle_obj = GetPathwayItem(new_model.item.item_name).particle_obj;
		if (!(particle_obj != null))
		{
			return;
		}
		int num2 = GraphicsControl.Instance.GetNumFloorParticles(new_model.segment);
		if (num2 < 1 || new_model.filled_locations.Count < 1)
		{
			return;
		}
		int chunkX = new_model.chunkX;
		int chunkZ = new_model.chunkZ;
		List<Vector2> list = new List<Vector2>();
		foreach (Vector2 filled_location in new_model.filled_locations)
		{
			list.Add(filled_location);
		}
		while (list.Count != 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			Vector2 vector = list[index];
			list.RemoveAt(index);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(particle_obj);
			gameObject2.transform.position = new Vector3(vector.x + 0.5f + (float)(chunkX * 10), 0f, vector.y + 0.5f + (float)(chunkZ * 10));
			gameObject2.transform.SetParent(gameObject.transform);
			num2--;
			if (num2 == 0)
			{
				break;
			}
		}
	}

	public void FinalizeWalls(PartiallyGeneratedModularModel new_model)
	{
		string chunkString = ChunkControl.Instance.GetChunkString(new_model.zone, new_model.chunkX, new_model.chunkZ);
		ChunkObj chunkObj = ChunkControl.Instance.GetChunkObj(chunkString);
		if (chunkObj == null)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(loaded_mesh_objects["Wall-Prefabs/" + new_model.item.item_name]);
		gameObject.SetActive(true);
		gameObject.transform.position = new Vector3(new_model.chunkX * 10, 0f, new_model.chunkZ * 10);
		gameObject.transform.SetParent(chunkObj.parent_obj.transform);
		if (!chunkObj.wall_segments.ContainsKey(new_model.unique_key))
		{
			gameObject.name = "(walls)" + new_model.unique_key;
			chunkObj.wall_segments.Add(new_model.unique_key, gameObject);
		}
		else
		{
			gameObject.name = "(walls) ERROR";
			chunkObj.wall_segments.Add(ShopControl.RandomString(), gameObject);
		}
		CreateMesh(gameObject, new_model, false);
		PaintableObject component = gameObject.GetComponent<PaintableObject>();
		string paint = new_model.item.GetString("paint");
		BanditCampsControl.Instance.ModifyIfBanditPaint(ref paint, new_model.item.item_name, new_model.item.GetString("bandit_camp_instance"));
		if (paint == "")
		{
			if (ChunkControl.Instance.player_zone != "overworld")
			{
				paint = ZoneDataControl.Instance.curr_zonedata.house_item.GetString("paint");
			}
			if (paint == "")
			{
				paint = inventory_ctr.Instance.GetPaintFromItemOrUseDefault(new_model.item);
			}
		}
		component.Colorize(paint, inventory_ctr.Instance.GetStampFromItem(new_model.item), inventory_ctr.Instance.GetLayoutItemFromItem(new_model.item.item_name), new_model.item.item_name);
		if (new_model.filled_locations.Count == 0)
		{
			return;
		}
		float num = ((new_model.item.item_name == "Palisade Wall" || new_model.item.item_name == "Tall Stone Wall") ? 2f : 1f);
		bool[,] array = new bool[10, 10];
		foreach (Vector2 filled_location in new_model.filled_locations)
		{
			array[(int)filled_location.x, (int)filled_location.y] = true;
		}
		for (int i = 0; i < 200; i++)
		{
			int num2 = 0;
			Vector2 vector = Vector2.zero;
			Vector2 vector2 = Vector2.zero;
			for (int j = 0; j < 10; j++)
			{
				int num3 = 0;
				for (int k = 0; k < 10; k++)
				{
					if (array[k, j])
					{
						if (num3 == 0)
						{
							vector2 = new Vector2(k, j);
							num3 = 1;
						}
						else
						{
							num3++;
						}
						continue;
					}
					if (num3 > num2)
					{
						num2 = num3;
						vector = vector2;
					}
					num3 = 0;
				}
				if (num3 > num2)
				{
					num2 = num3;
					vector = vector2;
				}
			}
			int num4 = 0;
			Vector2 vector3 = Vector2.zero;
			for (int l = 0; l < 10; l++)
			{
				int num5 = 0;
				for (int m = 0; m < 10; m++)
				{
					if (array[l, m])
					{
						if (num5 == 0)
						{
							vector2 = new Vector2(l, m);
							num5 = 1;
						}
						else
						{
							num5++;
						}
						continue;
					}
					if (num5 > num4)
					{
						num4 = num5;
						vector3 = vector2;
					}
					num5 = 0;
				}
				if (num5 > num4)
				{
					num4 = num5;
					vector3 = vector2;
				}
			}
			if (num2 == 0 && num4 == 0)
			{
				break;
			}
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			if (num2 < num4)
			{
				for (int n = 0; n < num4; n++)
				{
					array[(int)vector3.x, (int)vector3.y + n] = false;
				}
				boxCollider.center = new Vector3(vector3.x + 0.5f, num * 0.5f, vector3.y + (float)num4 * 0.5f);
				boxCollider.size = new Vector3(1f, num, num4);
			}
			else
			{
				for (int num6 = 0; num6 < num2; num6++)
				{
					array[(int)vector.x + num6, (int)vector.y] = false;
				}
				boxCollider.center = new Vector3(vector.x + (float)num2 * 0.5f, num * 0.5f, vector.y + 0.5f);
				boxCollider.size = new Vector3(num2, num, 1f);
			}
		}
	}
}
