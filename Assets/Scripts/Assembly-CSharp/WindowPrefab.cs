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

	public Dictionary<string, GameObject> quick_obj_lookup;

	public GameObject GetObj(string obj_name)
	{
		return null;
	}

	public void Function(string fn)
	{
	}
}
