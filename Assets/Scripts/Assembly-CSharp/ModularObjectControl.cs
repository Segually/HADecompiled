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

	public Dictionary<string, pathway_bend_type> pathway_bend_types;

	public flooring_item[] flooring_items;

	public Dictionary<string, GameObject> loaded_mesh_objects;

	private List<string> delayed_redraws_keys;

	private Dictionary<string, delayed_redraw> delayed_redraws;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public flooring_item GetPathwayItem(string item_name)
	{
		return default(flooring_item);
	}

	public void ModularChangedAt(int x, int z, type type, string zone, int chunkX, int chunkZ)
	{
	}

	private void SetComplete(string chunkStr, segment set_segment, type set_type)
	{
	}

	private void FixedUpdate()
	{
	}

	public void TryRedrawAtDelayed(type redraw_type, string zone, int chunkX, int chunkZ, segment segment_to_redraw)
	{
	}

	public string GetSuffix(segment seg)
	{
		return null;
	}

	public void AsyncLoadModularModel(string mesh_path, Action<GameObject> on_complete)
	{
	}

	public void AddVertices(GameObject loaded_prefab, int x, int z, int uv_x, int uv_y, int rot, PartiallyGeneratedModularModel new_model)
	{
	}

	public void CreateMesh(GameObject final_object, PartiallyGeneratedModularModel new_model, bool inherit_color_from_house)
	{
	}

	public void FinalizePathway(PartiallyGeneratedModularModel new_model)
	{
	}

	public void FinalizeWalls(PartiallyGeneratedModularModel new_model)
	{
	}
}
