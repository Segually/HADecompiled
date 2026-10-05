using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowPrefab : MonoBehaviour
{
	[Serializable]
	public struct child_object
	{
		public string name;

		public GameObject obj;
	}

	public child_object[] child_objects;

	public Dictionary<string, GameObject> quick_obj_lookup = new Dictionary<string, GameObject>();

	public GameObject GetObj(string obj_name)
	{
		if (!quick_obj_lookup.ContainsKey(obj_name))
		{
			for (int i = 0; i < child_objects.Length; i++)
			{
				if (child_objects[i].name == obj_name)
				{
					quick_obj_lookup.Add(obj_name, child_objects[i].obj);
					return child_objects[i].obj;
				}
			}
		}
		if (!quick_obj_lookup.ContainsKey(obj_name))
		{
			Debug.Log("ERROR: obj not found (name=" + obj_name + ")");
		}
		return quick_obj_lookup[obj_name];
	}

	public void Function(string fn)
	{
		int num = fn.IndexOf('.');
		int num2 = fn.IndexOf('(');
		int num3 = fn.IndexOf(')');
		if (num == -1 || num2 == -1 || num3 == -1)
		{
			Debug.Log("ERROR: BAD FUNCTION FORM");
			return;
		}
		string component_name = fn.Substring(0, num);
		string function_name = fn.Substring(num + 1, num2 - (num + 1));
		string text = fn.Substring(num2 + 1, num3 - (num2 + 1));
		if (text.Length == 0)
		{
			WindowPrefabsControl.Instance.TriggerFunction(component_name, function_name);
		}
		else if (text == "true" || text == "false")
		{
			WindowPrefabsControl.Instance.TriggerFunction(component_name, function_name, bool.Parse(text));
		}
		else
		{
			WindowPrefabsControl.Instance.TriggerFunction(component_name, function_name, int.Parse(text, Startup.parse_culture));
		}
	}
}
