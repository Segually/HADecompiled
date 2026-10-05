using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowPrefabsControl : MonoBehaviour, OrderedStart
{
	public enum function_reciever_type
	{
		by_reference = 0,
		hard_coded = 1
	}

	[Serializable]
	public struct function_reciever
	{
		public string name;

		public function_reciever_type type;

		public Component trigger_scr;
	}

	public enum build_into_t
	{
		mini_window = 0,
		GAME_CTR = 1,
		gameplay_top_right = 2,
		persistent_global_screen = 3
	}

	public enum set_transform_t
	{
		over_top = 0,
		beneath = 1
	}

	public static WindowPrefabsControl Instance;

	public Dictionary<string, GameObject> prefab_screens_instantiated = new Dictionary<string, GameObject>();

	private Dictionary<string, Component> quick_fn_lookup = new Dictionary<string, Component>();

	public function_reciever[] function_recievers;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public void TriggerFunction(string component_name, string function_name)
	{
	}

	public void TriggerFunction(string component_name, string function_name, object param)
	{
	}

	private Component GetComponentByName(string component_name)
	{
		return null;
	}

	public GameObject CreateScreen(string parent_name, build_into_t build_into, set_transform_t set_transform = set_transform_t.over_top)
	{
		return null;
	}

	public void DestroyScreen(string parent_name)
	{
		if (prefab_screens_instantiated.ContainsKey(parent_name))
		{
			UnityEngine.Object.Destroy(prefab_screens_instantiated[parent_name].gameObject);
			prefab_screens_instantiated.Remove(parent_name);
		}
	}

	public GameObject GetScreen(string parent_name)
	{
		if (prefab_screens_instantiated.ContainsKey(parent_name))
		{
			return prefab_screens_instantiated[parent_name];
		}
		return null;
	}

	public GameObject GetObject(string parent_name, string obj_name)
	{
		return null;
	}

	public Image GetImage(string parent_name, string obj_name)
	{
		return null;
	}

	public Text GetTextLegacy(string parent_name, string obj_name)
	{
		return null;
	}

	public TextMeshProUGUI GetTextMeshPro(string parent_name, string obj_name)
	{
		return null;
	}

	private GameObject InstantiateScreenIfNecessary(string parent_name, build_into_t build_into, set_transform_t set_transform)
	{
		return null;
	}
}
