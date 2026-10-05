using System.Collections.Generic;
using UnityEngine;

public class TranslationControl : MonoBehaviour, OrderedStart
{
	public enum languages
	{
		English = 0,
		Russian = 1,
		Portuguese = 2,
		Indonesian = 3,
		Spanish = 4,
		Thai = 5
	}

	public static TranslationControl Instance;

	public languages use_language;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
		if (PlayerData.Instance.GetGlobalString("LANG") == "")
		{
			switch (Application.systemLanguage)
			{
			case SystemLanguage.Indonesian:
				PlayerData.Instance.SetGlobalString("LANG", "INDONESIAN");
				break;
			case SystemLanguage.Portuguese:
				PlayerData.Instance.SetGlobalString("LANG", "PORTUGUESE");
				break;
			case SystemLanguage.Thai:
				if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.OSXPlayer)
				{
					PlayerData.Instance.SetGlobalString("LANG", "THAI");
				}
				break;
			case SystemLanguage.Spanish:
				PlayerData.Instance.SetGlobalString("LANG", "SPANISH");
				break;
			case SystemLanguage.Russian:
				PlayerData.Instance.SetGlobalString("LANG", "RUSSIAN");
				break;
			default:
				PlayerData.Instance.SetGlobalString("LANG", "ENGLISH");
				break;
			}
		}
		string globalString = PlayerData.Instance.GetGlobalString("LANG");
		if (globalString == "RUSSIAN")
		{
			use_language = languages.Russian;
		}
		else if (globalString == "PORTUGUESE")
		{
			use_language = languages.Portuguese;
		}
		else if (globalString == "INDONESIAN")
		{
			use_language = languages.Indonesian;
		}
		else if (globalString == "SPANISH")
		{
			use_language = languages.Spanish;
		}
		else if (globalString == "THAI")
		{
			use_language = languages.Thai;
		}
		else
		{
			use_language = languages.English;
		}
		MenuController.Instance.TranslateMenuElements();
		AdvertUtils.Instance.TranslateMysteryBoxText();
	}

	public string TranslateItemName(string item_name)
	{
		string text = item_name;
		switch (use_language)
		{
		case languages.Russian:
			text = ResourceControl.Instance.GetStringFromItemFile(item_name, "Name_RUS");
			break;
		case languages.Portuguese:
			text = ResourceControl.Instance.GetStringFromItemFile(item_name, "Name_POR");
			break;
		case languages.Indonesian:
			text = ResourceControl.Instance.GetStringFromItemFile(item_name, "Name_IND");
			break;
		case languages.Spanish:
			text = ResourceControl.Instance.GetStringFromItemFile(item_name, "Name_SPN");
			break;
		case languages.Thai:
			text = ResourceControl.Instance.GetStringFromItemFile(item_name, "Name_TAI");
			break;
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return item_name;
	}

	public string TranslateGeneral(string text, string file)
	{
		string text2;
		switch (use_language)
		{
		case languages.English:
			return text;
		case languages.Russian:
			text2 = "Russian";
			break;
		case languages.Portuguese:
			text2 = "Portuguese";
			break;
		case languages.Indonesian:
			text2 = "Indonesian";
			break;
		case languages.Spanish:
			text2 = "Spanish";
			break;
		case languages.Thai:
			text2 = "Thai";
			break;
		default:
			text2 = "";
			break;
		}
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("lang-" + text2 + "/" + text2 + "-" + file, ref file_exists);
		if (file_exists)
		{
			bool flag = false;
			for (int i = 0; i < textFileLines.Count; i++)
			{
				if (!Startup.StringNullOrWhitespace(textFileLines[i]) && !(textFileLines[i] == "-"))
				{
					if (flag)
					{
						return textFileLines[i];
					}
					flag = textFileLines[i] == text;
				}
			}
		}
		return text;
	}

	public string TranslateGeneralBackwards(string text, string file)
	{
		string result = "???";
		string text2;
		switch (use_language)
		{
		case languages.English:
			return text;
		case languages.Russian:
			text2 = "Russian";
			break;
		case languages.Portuguese:
			text2 = "Portuguese";
			break;
		case languages.Indonesian:
			text2 = "Indonesian";
			break;
		case languages.Spanish:
			text2 = "Spanish";
			break;
		case languages.Thai:
			text2 = "Thai";
			break;
		default:
			text2 = "";
			break;
		}
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("lang-" + text2 + "/" + text2 + "-" + file, ref file_exists);
		if (file_exists)
		{
			for (int i = 0; i < textFileLines.Count; i++)
			{
				if (!Startup.StringNullOrWhitespace(textFileLines[i]) && !(textFileLines[i] == "-"))
				{
					if (textFileLines[i] == text)
					{
						return result;
					}
					result = textFileLines[i];
				}
			}
		}
		return text;
	}
}
