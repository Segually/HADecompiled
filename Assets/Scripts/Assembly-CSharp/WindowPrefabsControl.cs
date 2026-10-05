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
		GetComponentByName(component_name).SendMessage(function_name);
	}

	public void TriggerFunction(string component_name, string function_name, object param)
	{
		GetComponentByName(component_name).SendMessage(function_name, param);
	}

	private Component GetComponentByName(string component_name)
	{
		if (quick_fn_lookup.ContainsKey(component_name))
		{
			if (!(quick_fn_lookup[component_name] == null))
			{
				return quick_fn_lookup[component_name];
			}
			quick_fn_lookup.Remove(component_name);
		}
		for (int i = 0; i < function_recievers.Length; i++)
		{
			if (!(function_recievers[i].name == component_name))
			{
				continue;
			}
			if (function_recievers[i].type == function_reciever_type.hard_coded)
			{
				if (function_recievers[i].name == "PoolGameControl")
				{
					quick_fn_lookup.Add(component_name, PoolGameControl.Instance);
				}
				else if (function_recievers[i].name == "KaraokeControl")
				{
					quick_fn_lookup.Add(component_name, KaraokeControl.Instance);
				}
			}
			else if (function_recievers[i].type == function_reciever_type.by_reference)
			{
				quick_fn_lookup.Add(component_name, function_recievers[i].trigger_scr);
			}
			break;
		}
		return quick_fn_lookup[component_name];
	}

	public GameObject CreateScreen(string parent_name, build_into_t build_into, set_transform_t set_transform = set_transform_t.over_top)
	{
		if (prefab_screens_instantiated.ContainsKey(parent_name))
		{
			return prefab_screens_instantiated[parent_name];
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(ResourceControl.Instance.GetWindowPrefab(parent_name));
		switch (build_into)
		{
		case build_into_t.mini_window:
			gameObject.transform.SetParent(WindowControl.Instance.miniwindow.transform);
			break;
		case build_into_t.GAME_CTR:
			gameObject.transform.SetParent(GameplayGUIControl.Instance.transform);
			break;
		case build_into_t.gameplay_top_right:
			gameObject.transform.SetParent(GameplayGUIControl.Instance.top_right_buttons.transform);
			break;
		case build_into_t.persistent_global_screen:
			gameObject.transform.SetParent(PopupControl.Instance.transform);
			break;
		}
		if (set_transform == set_transform_t.beneath)
		{
			gameObject.transform.SetAsFirstSibling();
		}
		gameObject.transform.localPosition = Vector3.zero;
		((RectTransform)gameObject.transform).anchoredPosition = Vector2.zero;
		((RectTransform)gameObject.transform).offsetMin = Vector2.zero;
		((RectTransform)gameObject.transform).offsetMax = Vector2.zero;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localRotation = Quaternion.identity;
		prefab_screens_instantiated.Add(parent_name, gameObject);
		return gameObject;
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
		return prefab_screens_instantiated[parent_name].GetComponent<WindowPrefab>().GetObj(obj_name);
	}

	public Image GetImage(string parent_name, string obj_name)
	{
		return null;
	}

	public Text GetTextLegacy(string parent_name, string obj_name)
	{
		return GetObject(parent_name, obj_name).GetComponent<Text>();
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
