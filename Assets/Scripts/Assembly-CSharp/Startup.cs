using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Startup : MonoBehaviour
{
	public static string persistentDataPath;

	public static CultureInfo parse_culture;

	public void Init()
	{
		persistentDataPath = Application.persistentDataPath;
		parse_culture = new CultureInfo("en-US");
		if (!Directory.Exists(persistentDataPath + Path.DirectorySeparatorChar + "General"))
		{
			Directory.CreateDirectory(persistentDataPath + Path.DirectorySeparatorChar + "General");
		}
		if (!Directory.Exists(persistentDataPath + Path.DirectorySeparatorChar + "Slot_0"))
		{
			Directory.CreateDirectory(persistentDataPath + Path.DirectorySeparatorChar + "Slot_0");
		}
		if (!Directory.Exists(persistentDataPath + Path.DirectorySeparatorChar + "Slot_1"))
		{
			Directory.CreateDirectory(persistentDataPath + Path.DirectorySeparatorChar + "Slot_1");
		}
		if (!Directory.Exists(persistentDataPath + Path.DirectorySeparatorChar + "Slot_2"))
		{
			Directory.CreateDirectory(persistentDataPath + Path.DirectorySeparatorChar + "Slot_2");
		}
		string path = Path.Combine(persistentDataPath, "conversion_progress.txt");
		string scene_name;
		if (!File.Exists(path))
		{
			File.WriteAllLines(Path.Combine(persistentDataPath, "conversion_progress.txt"), new string[1] { "complete15" });
			scene_name = "Menu";
		}
		else
		{
			string[] array = File.ReadAllLines(path);
			scene_name = "Upgrade";
			if (array.Length != 0)
			{
				scene_name = ((array[0] == "complete15") ? "Menu" : "Upgrade");
			}
		}
		StartCoroutine(AsyncGoToScene(scene_name));
		if (Application.isEditor && Application.platform == RuntimePlatform.WindowsEditor)
		{
			CreatureMorpher.GenerateCreatureLists();
			ZoneDataControl.GenerateNPCShackData();
			LootControl.GenerateLootData();
			AutoComplete.GenerateAutoCompleteLookupLists();
			PerkControl.GeneratePerkList();
			PerkControl.GeneratePollinatorSummary();
			CombatControl.GenerateCombatOutput();
			QuestControl.ScanQuestDataForChanges(Directory.GetFiles(QuestControl.quest_cache_path), Directory.GetFiles(Application.dataPath + "/SYNCHRONOUS/TextFiles/" + DevBuildControl.quest_scenics_folder_));
		}
	}

	public static bool StringNullOrEmpty(string str)
	{
		if (str != null)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] != ' ' && str[i] != '\n' && str[i] != '\t')
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool StringNullOrWhitespace(string str)
	{
		if (str != null)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] != ' ' && str[i] != '\n' && str[i] != '\t')
				{
					return false;
				}
			}
		}
		return true;
	}

	public static void WriteOnlyIfChanged(string path, string[] array)
	{
		if (File.Exists(path))
		{
			string[] array2 = File.ReadAllLines(path);
			string text = "";
			for (int i = 0; i < array2.Length; i++)
			{
				text += array2[i];
			}
			string text2 = "";
			for (int j = 0; j < array.Length; j++)
			{
				text2 += array[j];
			}
			if (text == text2)
			{
				return;
			}
		}
		File.WriteAllLines(path, array);
	}

	private IEnumerator AsyncGoToScene(string scene_name)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene_name);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}
}
