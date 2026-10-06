using System.IO;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class AutoComplete : MonoBehaviour
{
	public GameObject nib_0;

	public GameObject nib_1;

	public bool typing_updates_enabled;

	public Dictionary<string, string> item_list = new Dictionary<string, string>();

	public static void GenerateAutoCompleteLookupLists()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		List<string> list4 = new List<string>();
		List<string> list5 = new List<string>();
		List<string> list6 = new List<string>();
		string[] files = Directory.GetFiles(Application.dataPath + "/SYNCHRONOUS/TextFiles/InventoryItems");
		foreach (string path in files)
		{
			if (Path.GetExtension(path) == ".meta")
			{
				continue;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			string text = fileNameWithoutExtension.ToLower();
			string text2 = fileNameWithoutExtension.ToLower();
			string text3 = fileNameWithoutExtension.ToLower();
			string text4 = fileNameWithoutExtension.ToLower();
			string text5 = fileNameWithoutExtension.ToLower();
			string text6 = fileNameWithoutExtension.ToLower();
			string[] array = File.ReadAllLines(path);
			bool flag = false;
			foreach (string text7 in array)
			{
				int num = text7.IndexOf('=');
				if (num == -1)
				{
					continue;
				}
				string text8 = text7.Substring(0, num - 1);
				string text9 = text7.Substring(num + 2, text7.Length - (num + 2));
				switch (text8)
				{
				case "Overwrite_name":
					text = text9.ToLower();
					break;
				case "Name_TAI":
					text5 = text9.ToLower();
					break;
				case "Name_SPN":
					text4 = text9.ToLower();
					break;
				case "Name_IND":
					text2 = text9.ToLower();
					break;
				case "Name_RUS":
					text3 = text9.ToLower();
					break;
				case "Name_POR":
					text6 = text9.ToLower();
					break;
				case "not_obtainable":
					flag |= text9 == "true";
					break;
				}
			}
			if (!flag)
			{
				list.Add(text + "=" + fileNameWithoutExtension);
				list2.Add(text2 + "=" + fileNameWithoutExtension);
				list2.Add(text + "=" + fileNameWithoutExtension);
				list3.Add(text3 + "=" + fileNameWithoutExtension);
				list3.Add(text + "=" + fileNameWithoutExtension);
				list4.Add(text4 + "=" + fileNameWithoutExtension);
				list4.Add(text + "=" + fileNameWithoutExtension);
				list5.Add(text5 + "=" + fileNameWithoutExtension);
				list5.Add(text + "=" + fileNameWithoutExtension);
				list6.Add(text6 + "=" + fileNameWithoutExtension);
				list6.Add(text + "=" + fileNameWithoutExtension);
			}
		}
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) auto_fill_ENG.txt", list.ToArray());
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) auto_fill_IND.txt", list2.ToArray());
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) auto_fill_RUS.txt", list3.ToArray());
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) auto_fill_SPN.txt", list4.ToArray());
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) auto_fill_TAI.txt", list5.ToArray());
		Startup.WriteOnlyIfChanged(Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/(Auto Gen) auto_fill_POR.txt", list6.ToArray());
	}

	public void EnableTypingUpdates(string init_string)
	{
		typing_updates_enabled = true;
		RedrawNibs(init_string.ToLower());
	}

	public void HideNibsAndDisableTypingUpdates()
	{
		typing_updates_enabled = false;
		nib_0.SetActive(false);
		nib_1.SetActive(false);
	}

	public void TypingDetected(string typed_string)
	{
		if (typing_updates_enabled)
		{
			RedrawNibs(typed_string);
		}
	}

	public void RedrawNibs(string typed_string)
	{
		string value = typed_string.ToLower();
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> item in item_list)
		{
			if (item.Key.Contains(value))
			{
				list.Add(item.Value);
				if (list.Count == 2)
				{
					break;
				}
			}
		}
		switch (list.Count)
		{
		case 0:
			nib_0.SetActive(false);
			nib_1.SetActive(false);
			break;
		case 1:
			nib_0.SetActive(true);
			nib_0.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = inventory_ctr.Instance.GetFullItemName(new InventoryItem(list[0]));
			nib_1.SetActive(false);
			break;
		case 2:
			nib_0.SetActive(true);
			nib_0.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = inventory_ctr.Instance.GetFullItemName(new InventoryItem(list[0]));
			nib_1.SetActive(true);
			nib_1.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = inventory_ctr.Instance.GetFullItemName(new InventoryItem(list[1]));
			break;
		}
	}

	public void LoadList()
	{
		string text;
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.Russian:
			text = "auto_fill_RUS";
			break;
		case TranslationControl.languages.Portuguese:
			text = "auto_fill_POR";
			break;
		case TranslationControl.languages.Indonesian:
			text = "auto_fill_IND";
			break;
		case TranslationControl.languages.Spanish:
			text = "auto_fill_SPN";
			break;
		case TranslationControl.languages.Thai:
			text = "auto_fill_TAI";
			break;
		default:
			text = "auto_fill_ENG";
			break;
		}
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("AutoGen/(Auto Gen) " + text, ref file_exists);
		if (!file_exists)
		{
			return;
		}
		foreach (string item in textFileLines)
		{
			if (!Startup.StringNullOrWhitespace(item))
			{
				int num = item.IndexOf('=');
				if (num != -1)
				{
					string key = item.Substring(0, num);
					string value = item.Substring(num + 1, item.Length - (num + 1));
					if (!item_list.ContainsKey(key))
					{
						item_list.Add(key, value);
					}
				}
			}
		}
	}
}
