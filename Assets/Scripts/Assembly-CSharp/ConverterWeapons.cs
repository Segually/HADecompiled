using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConverterWeapons : MonoBehaviour
{
	public bool complete;

	private Dictionary<string, string> items_to_rename = new Dictionary<string, string>();

	public IEnumerator BeginConverting()
	{
		GrandConverter.Instance.DrawProgressBar(0.01f);
		GrandConverter.Instance.filename_text.text = "";
		yield return new WaitForEndOfFrame();
		items_to_rename.Add("Titanium Sword", "Titanium DoubleSword");
		items_to_rename.Add("Magma Sword", "Magma DoubleSword");
		items_to_rename.Add("Ice Sword", "Ice DoubleSword");
		items_to_rename.Add("Dark Sword", "Dark Hasta");
		List<string> list = new List<string>();
		list.Add("Slot_0");
		list.Add("Slot_1");
		list.Add("Slot_2");
		float base_progress = 0f;
		foreach (string item in list)
		{
			string path = Path.Combine(Startup.persistentDataPath, item);
			string[] fileEntries = new string[0];
			fileEntries = Directory.GetFiles(path);
			for (int i = 0; i < fileEntries.Length; i++)
			{
				string text = fileEntries[i];
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
				if (fileNameWithoutExtension.Contains("ClumpedChunks"))
				{
					ConvertChunks(text);
				}
				else if (fileNameWithoutExtension.Contains("ClumpedBaskets") || fileNameWithoutExtension == "the_inventory")
				{
					ConvertBaskets(text);
				}
				GrandConverter.Instance.DrawProgressBar(base_progress + (float)i / (float)fileEntries.Length * 0.33f);
				yield return new WaitForEndOfFrame();
			}
			base_progress += 0.33f;
		}
		complete = true;
	}

	private void ConvertChunks(string full_filename)
	{
		SingleFile singleFile = new SingleFile();
		singleFile.LoadFromBytes(File.ReadAllBytes(full_filename));
		foreach (string key in singleFile.file_segments.Keys)
		{
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					short shortValue = singleFile.GetShortValue(i + "," + j + "_numObjects", key);
					for (int k = 0; k < shortValue; k++)
					{
						TryConvertAllSubItems(i + "," + j + "," + k, singleFile, key);
					}
				}
			}
		}
		if (singleFile.edited_since_load)
		{
			File.WriteAllBytes(full_filename, singleFile.ToByteArray());
		}
	}

	private void TryConvertAllSubItems(string prefix, SingleFile file, string file_segment)
	{
		short shortValue = file.GetShortValue(prefix + "_n_strings", file_segment);
		for (int i = 0; i < shortValue; i++)
		{
			if (!(file.GetStringValue(prefix + "_string" + i + "_key", file_segment) == "item_id"))
			{
				continue;
			}
			string stringValue = file.GetStringValue(prefix + "_string" + i + "_value", file_segment);
			if (stringValue == "Weapon Display")
			{
				TryConvertOneSubItem("wep", prefix, file, file_segment);
			}
			else if (stringValue == "Large Weapon Display")
			{
				TryConvertOneSubItem("wep", prefix, file, file_segment);
				TryConvertOneSubItem("wep2", prefix, file, file_segment);
			}
			else if (stringValue == "Custom Statue")
			{
				TryConvertOneSubItem("hat", prefix, file, file_segment);
				TryConvertOneSubItem("body", prefix, file, file_segment);
				TryConvertOneSubItem("wep", prefix, file, file_segment);
			}
			else if (stringValue == "Companion")
			{
				TryConvertEncodedList("pockets", prefix, file, file_segment);
			}
			else if (stringValue == "Vending Machine")
			{
				TryConvertEncodedList("vending_machine_for_sale", prefix, file, file_segment);
				TryConvertEncodedList("vending_machine_costs", prefix, file, file_segment);
			}
			break;
		}
	}

	private void TryConvertOneSubItem(string sub_item_name, string prefix, SingleFile file, string file_segment)
	{
		short shortValue = file.GetShortValue(prefix + "_n_strings", file_segment);
		for (int i = 0; i < shortValue; i++)
		{
			if (file.GetStringValue(prefix + "_string" + i + "_key", file_segment) == "item(" + sub_item_name + ")item_id")
			{
				string stringValue = file.GetStringValue(prefix + "_string" + i + "_value", file_segment);
				if (items_to_rename.ContainsKey(stringValue))
				{
					file.SetString(prefix + "_string" + i + "_value", items_to_rename[stringValue], file_segment);
				}
				break;
			}
		}
	}

	private void TryConvertEncodedList(string list_name, string prefix, SingleFile file, string file_segment)
	{
		short shortValue = file.GetShortValue(prefix + "_n_shorts", file_segment);
		int num = 0;
		for (int i = 0; i < shortValue; i++)
		{
			if (file.GetStringValue(prefix + "_short" + i + "_key", file_segment) == list_name + "_n_encoded_items")
			{
				num = file.GetShortValue(prefix + "_short" + i + "_value", file_segment);
			}
		}
		for (int j = 0; j < num; j++)
		{
			string text = "item(" + list_name + "_item_" + j + ")item_id";
			short shortValue2 = file.GetShortValue(prefix + "_n_strings", file_segment);
			for (int k = 0; k < shortValue2; k++)
			{
				if (file.GetStringValue(prefix + "_string" + k + "_key", file_segment) == text)
				{
					string stringValue = file.GetStringValue(prefix + "_string" + k + "_value", file_segment);
					if (items_to_rename.ContainsKey(stringValue))
					{
						file.SetString(prefix + "_string" + k + "_value", items_to_rename[stringValue], file_segment);
					}
					break;
				}
			}
		}
	}

	private void ConvertBaskets(string full_filename)
	{
		SingleFile singleFile = new SingleFile();
		singleFile.LoadFromBytes(File.ReadAllBytes(full_filename));
		foreach (string key in singleFile.file_segments.Keys)
		{
			short shortValue = singleFile.GetShortValue("n_stored_items", key);
			for (int i = 0; i < shortValue; i++)
			{
				TryConvertItem("entry" + i, singleFile, key);
			}
		}
		if (singleFile.edited_since_load)
		{
			File.WriteAllBytes(full_filename, singleFile.ToByteArray());
		}
	}

	private void TryConvertItem(string prefix, SingleFile file, string file_segment)
	{
		short shortValue = file.GetShortValue(prefix + "_n_strings", file_segment);
		for (int i = 0; i < shortValue; i++)
		{
			if (file.GetStringValue(prefix + "_string" + i + "_key", file_segment) == "item_id")
			{
				string stringValue = file.GetStringValue(prefix + "_string" + i + "_value", file_segment);
				if (items_to_rename.ContainsKey(stringValue))
				{
					file.SetString(prefix + "_string" + i + "_value", items_to_rename[stringValue], file_segment);
				}
				break;
			}
		}
	}
}
