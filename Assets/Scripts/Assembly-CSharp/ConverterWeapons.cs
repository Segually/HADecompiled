using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConverterWeapons : MonoBehaviour
{
	public bool complete;

	private Dictionary<string, string> items_to_rename;

	public IEnumerator BeginConverting()
	{
		return null;
	}

	private void ConvertChunks(string full_filename)
	{
	}

	private void TryConvertAllSubItems(string prefix, SingleFile file, string file_segment)
	{
	}

	private void TryConvertOneSubItem(string sub_item_name, string prefix, SingleFile file, string file_segment)
	{
	}

	private void TryConvertEncodedList(string list_name, string prefix, SingleFile file, string file_segment)
	{
	}

	private void ConvertBaskets(string full_filename)
	{
	}

	private void TryConvertItem(string prefix, SingleFile file, string file_segment)
	{
	}
}
