using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueControl : MonoBehaviour, OrderedStart
{
	private enum npc_load_state
	{
		translated_names = 0,
		dialogue = 1
	}

	public enum focus_type_t
	{
		none = 0,
		stationary_npc = 1,
		moving_npc = 2,
		painting = 3,
		trophy = 4
	}

	public static DialogueControl Instance;

	private AudioSource voice_holder_1;

	private AudioSource voice_holder_2;

	public Dictionary<int, Dictionary<string, object>> curr_dialogue_data_;

	private string curr_voice = "";

	private Dictionary<string, FullNPC> loaded_FullNPCs = new Dictionary<string, FullNPC>();

	public string curr_NPC_display_name;

	public string curr_NPC_combo_text;

	public GameObject curr_NPC_obj;

	public focus_type_t focus_type;

	public Font dialogue_font;

	public DialogueAnimation curr_dialogue_animation;

	private Dictionary<string, object> curr_other_dialogue;

	private Dictionary<string, object> curr_self_options;

	public Sprite buy_icon_blue;

	public Sprite buy_icon_green;

	public Sprite sell_icon_blue;

	public Sprite sell_icon_green;

	private IEnumerator delayed_initial_blobble_t;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		voice_holder_1 = base.gameObject.AddComponent<AudioSource>();
		voice_holder_1.volume = 0f;
		voice_holder_2 = base.gameObject.AddComponent<AudioSource>();
		voice_holder_2.volume = 0f;
	}

	public FullNPC GetFullCurrNPC()
	{
		return GetFullNPC(GameController.Instance.interacting_element_item);
	}

	public FullNPC GetFullNPC(InventoryItem npc_item)
	{
		string @string = npc_item.GetString("npc_file");
		if (!loaded_FullNPCs.ContainsKey(@string))
		{
			FullNPC fullNPC = LoadFullNpcFile(npc_item);
			if (fullNPC != null)
			{
				loaded_FullNPCs.Add(@string, fullNPC);
			}
			return fullNPC;
		}
		return loaded_FullNPCs[@string];
	}

	public void AddImportant(string entire_string, string highlight, Dictionary<string, object> curr_dialogue_collection)
	{
		int num = entire_string.IndexOf(highlight);
		if (num != -1)
		{
		if (!curr_dialogue_collection.ContainsKey("text"))
		{
			curr_dialogue_collection.Add("text", entire_string);
		}
		else
		{
			curr_dialogue_collection["text"] = entire_string;
		}
		if (!curr_dialogue_collection.ContainsKey("type"))
		{
			curr_dialogue_collection.Add("type", "important_speak");
		}
		else
		{
			curr_dialogue_collection["type"] = "important_speak";
		}
			if (!curr_dialogue_collection.ContainsKey("important_start"))
			{
				curr_dialogue_collection.Add("important_start", num);
			}
			if (!curr_dialogue_collection.ContainsKey("important_end"))
			{
				curr_dialogue_collection.Add("important_end", highlight.Length + num);
			}
		if (!curr_dialogue_collection.ContainsKey("important_color"))
		{
			curr_dialogue_collection.Add("important_color", "#33d3ff");
		}
		else
		{
			curr_dialogue_collection["important_color"] = "#33d3ff";
		}
		}
	}

	public FullNPC LoadFullNpcFile(InventoryItem npc_item)
	{
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("NPCs/" + npc_item.GetString("npc_file"), ref file_exists);
		if (!file_exists)
		{
			return null;
		}
		FullNPC fullNPC = new FullNPC();
		Dictionary<string, object> dictionary = null;
		int key = -1;
		bool flag = false;
		foreach (string item in textFileLines)
		{
			if (Startup.StringNullOrWhitespace(item))
			{
				continue;
			}
			if (item.Contains("----- DIALOGUE -----"))
			{
				flag = true;
			}
			else if (!flag)
			{
				if (!item.Contains("Overwrite_name") && !item.Contains("Name_POR") && !item.Contains("Name_RUS") && !item.Contains("Name_IND") && !item.Contains("Name_SPN") && !item.Contains("Name_TAI"))
				{
					continue;
				}
				int num = item.IndexOf('=');
				string text2 = item.Substring(0, num);
				string translated_display_name = item.Substring(num + 1, item.Length - (num + 1));
				switch (text2)
				{
				case "Overwrite_name":
					if (TranslationControl.Instance.use_language == TranslationControl.languages.English)
					{
						fullNPC.translated_display_name = translated_display_name;
					}
					break;
				case "Name_POR":
					if (TranslationControl.Instance.use_language == TranslationControl.languages.Portuguese)
					{
						fullNPC.translated_display_name = translated_display_name;
					}
					break;
				case "Name_RUS":
					if (TranslationControl.Instance.use_language == TranslationControl.languages.Russian)
					{
						fullNPC.translated_display_name = translated_display_name;
					}
					break;
				case "Name_IND":
					if (TranslationControl.Instance.use_language == TranslationControl.languages.Indonesian)
					{
						fullNPC.translated_display_name = translated_display_name;
					}
					break;
				case "Name_SPN":
					if (TranslationControl.Instance.use_language == TranslationControl.languages.Spanish)
					{
						fullNPC.translated_display_name = translated_display_name;
					}
					break;
				case "Name_TAI":
					if (TranslationControl.Instance.use_language == TranslationControl.languages.Thai)
					{
						fullNPC.translated_display_name = translated_display_name;
					}
					break;
				}
			}
			else if (item[0] == '[')
			{
				if (dictionary != null)
				{
					fullNPC.dialogue_data.Add(key, dictionary);
				}
				key = ParseNumber(item);
				dictionary = new Dictionary<string, object>();
			}
			else if (item[0] == '*')
			{
				string text = item;
				if (text == "*EXIT_WITH_POPUP*")
				{
					dictionary.Add("type", "exit_popup_message");
				}
				else if (text == "*test IS_MY_WORLD*")
				{
					dictionary.Add("type", "test_is_my_world");
				}
				else if (text.Contains("*test QUEST_PROGRESS*"))
				{
					dictionary.Add("type", "test_quest_progress");
					int num2 = text.IndexOf('(');
					int num3 = text.IndexOf(" == ");
					int num4 = text.IndexOf(')');
					string value3 = text.Substring(num2 + 1, num3 - (num2 + 1));
					int num5 = int.Parse(text.Substring(num3 + 4, num4 - (num3 + 4)), Startup.parse_culture);
					dictionary.Add("quest_name", value3);
					dictionary.Add("test_progress", num5);
				}
				else if (text.Contains("*test QUEST_READY_TO_RESTART*"))
				{
					dictionary.Add("type", "test_quest_ready_to_restart");
					int num6 = text.IndexOf('(');
					int num7 = text.IndexOf(')');
					dictionary.Add("quest_name", text.Substring(num6 + 1, num7 - (num6 + 1)));
				}
				else if (text.Contains("*test HAS_ANY_QUEST_ITEMS*"))
				{
					dictionary.Add("type", "test_has_any_quest_items");
					int num8 = text.IndexOf('(');
					int num9 = text.IndexOf(", step ");
					int num10 = text.IndexOf(')');
					string value4 = text.Substring(num8 + 1, num9 - (num8 + 1));
					int num11 = int.Parse(text.Substring(num9 + 7, num10 - (num9 + 7)), Startup.parse_culture);
					dictionary.Add("quest_name", value4);
					dictionary.Add("step", num11);
				}
				else if (text.Contains("*test HAS_ALL_QUEST_ITEMS*"))
				{
					dictionary.Add("type", "test_has_all_quest_items");
					int num12 = text.IndexOf('(');
					int num13 = text.IndexOf(", step ");
					int num14 = text.IndexOf(')');
					string value5 = text.Substring(num12 + 1, num13 - (num12 + 1));
					int num15 = int.Parse(text.Substring(num13 + 7, num14 - (num13 + 7)), Startup.parse_culture);
					dictionary.Add("quest_name", value5);
					dictionary.Add("step", num15);
				}
				else if (text.Contains("*TAKE_QUEST_ITEMS*"))
				{
					dictionary.Add("type", "take_quest_items");
					int num16 = text.IndexOf('(');
					int num17 = text.IndexOf(", step ");
					int num18 = text.IndexOf(") (if all collected, advance quest to ");
					int num19 = text.LastIndexOf(')');
					string value6 = text.Substring(num16 + 1, num17 - (num16 + 1));
					int num20 = int.Parse(text.Substring(num17 + 7, num18 - (num17 + 7)), Startup.parse_culture);
					int num21 = int.Parse(text.Substring(num18 + 38, num19 - (num18 + 38)), Startup.parse_culture);
					dictionary.Add("quest_name", value6);
					dictionary.Add("curr_progress", num20);
					dictionary.Add("set_progress_if_all_collected", num21);
				}
				else if (text.Contains("*test FINISHED_COLLECTING_QUEST_ITEMS*"))
				{
					dictionary.Add("type", "test_collected_all_quest_items");
					int num22 = text.IndexOf('(');
					int num23 = text.IndexOf(", step ");
					int num24 = text.IndexOf(')');
					string value7 = text.Substring(num22 + 1, num23 - (num22 + 1));
					int num25 = int.Parse(text.Substring(num23 + 7, num24 - (num23 + 7)), Startup.parse_culture);
					dictionary.Add("quest_name", value7);
					dictionary.Add("step", num25);
				}
				else if (text.Contains("*TRY_GIVE_QUEST_REWARD*"))
				{
					dictionary.Add("type", "quest_rewards");
					int num26 = text.IndexOf('(');
					int num27 = text.IndexOf(") (if succeed, advance quest to ");
					int num28 = text.LastIndexOf(')');
					string value8 = text.Substring(num26 + 1, num27 - (num26 + 1));
					int num29 = int.Parse(text.Substring(num27 + 32, num28 - (num27 + 32)), Startup.parse_culture);
					dictionary.Add("quest_name", value8);
					dictionary.Add("set_progress_on_success", num29);
				}
				else if (text.Contains("*ADVANCE_QUEST*"))
				{
					dictionary.Add("type", "advance_quest");
					int num30 = text.IndexOf('(');
					int num31 = text.IndexOf(" => ");
					int num32 = text.IndexOf(')');
					string value9 = text.Substring(num30 + 1, num31 - (num30 + 1));
					int num33 = int.Parse(text.Substring(num31 + 4, num32 - (num31 + 4)), Startup.parse_culture);
					dictionary.Add("quest_name", value9);
					dictionary.Add("set_progress_to", num33);
				}
				else if (text == "*test LESS_THAN_2_COMPANIONS*")
				{
					dictionary.Add("type", "test_less_than_2_companions");
				}
				else if (text.Contains("*TELEPORT*"))
				{
					dictionary.Add("type", "teleport");
					int num34 = text.IndexOf('(');
					int num35 = text.LastIndexOf(')');
					dictionary.Add("tele_str", text.Substring(num34 + 1, num35 - (num34 + 1)));
				}
				else if (text.Contains("*SHOW_REWARD_ITEM*"))
				{
				if (!dictionary.ContainsKey("type"))
				{
					dictionary.Add("type", "show_quest_reward");
				}
				else
				{
					dictionary["type"] = "show_quest_reward";
				}
					string value10 = text.Substring(20, text.Length - 21);
				if (!dictionary.ContainsKey("quest_name"))
				{
					dictionary.Add("quest_name", value10);
				}
				else
				{
					dictionary["quest_name"] = value10;
				}
				}

			}
			else
			{
				int num36 = item.IndexOf('=');
				string text3 = item.Substring(0, num36);
				string text4 = item.Substring(num36 + 1, item.Length - (num36 + 1));
				string text5 = "X";
				string text6 = "X";
				string text7 = "X";
				string text8 = "X";
				string text9 = "X";
				switch (TranslationControl.Instance.use_language)
				{
				case TranslationControl.languages.Russian:
					text9 = "highlight_RUS";
					text5 = "otherspeak_RUS";
					text6 = "popuptext_RUS";
					text7 = "optionB_RUS";
					text8 = "optionA_RUS";
					break;
				case TranslationControl.languages.Portuguese:
					text9 = "highlight_POR";
					text5 = "otherspeak_POR";
					text6 = "popuptext_POR";
					text7 = "optionB_POR";
					text8 = "optionA_POR";
					break;
				case TranslationControl.languages.Indonesian:
					text9 = "highlight_IND";
					text5 = "otherspeak_IND";
					text6 = "popuptext_IND";
					text7 = "optionB_IND";
					text8 = "optionA_IND";
					break;
				case TranslationControl.languages.Spanish:
					text9 = "highlight_SPN";
					text5 = "otherspeak_SPN";
					text6 = "popuptext_SPN";
					text7 = "optionB_SPN";
					text8 = "optionA_SPN";
					break;
				case TranslationControl.languages.Thai:
					text9 = "highlight_TAI";
					text5 = "otherspeak_TAI";
					text6 = "popuptext_TAI";
					text7 = "optionB_TAI";
					text8 = "optionA_TAI";
					break;
				}
				if (text3 == "otherspeak" || text3 == text5)
				{
					if (!text4.Contains("SHOW_QUEST_ITEM"))
					{
						if (!dictionary.ContainsKey("type"))
						{
							dictionary.Add("type", "NPC_speak");
						}
						if (!dictionary.ContainsKey("text"))
						{
							dictionary.Add("text", text4);
						}
						else
						{
							dictionary["text"] = text4;
						}

						continue;
					}
					int num37 = text4.IndexOf("SHOW_QUEST_ITEM") + 16;
					int num38 = -1;
					for (int i = num37; i < text4.Length; i++)
					{
						if (text4[i] == ',')
						{
							num38 = i;
							break;
						}
					}
					int num39 = num38 + 7;
					int num40 = -1;
					for (int j = num39; j < text4.Length; j++)
					{
						if (text4[j] == ',')
						{
							num40 = j;
							break;
						}
					}
					int num41 = num40 + 7;
					int num42 = -1;
					for (int k = num41; k < text4.Length; k++)
					{
						if (text4[k] == ')')
						{
							num42 = k;
							break;
						}
					}
					string value11 = text4.Substring(0, text4.IndexOf("SHOW_QUEST_ITEM"));
					string value12 = text4.Substring(num37, num38 - num37);
					int num43 = int.Parse(text4.Substring(num39, num40 - num39), Startup.parse_culture);
					int num44 = int.Parse(text4.Substring(num41, num42 - num41), Startup.parse_culture);
					if (!dictionary.ContainsKey("type"))
					{
						dictionary.Add("type", "show_quest_item");
					}
					if (!dictionary.ContainsKey("text"))
					{
						dictionary.Add("text", value11);
					}
					else
					{
						dictionary["text"] = value11;
					}
					if (!dictionary.ContainsKey("quest_name"))
					{
						dictionary.Add("quest_name", value12);
					}
					else
					{
						dictionary["quest_name"] = value12;
					}
					if (!dictionary.ContainsKey("quest_progress"))
					{
						dictionary.Add("quest_progress", num43);
					}
					else
					{
						dictionary["quest_progress"] = num43;
					}
					if (!dictionary.ContainsKey("item_i"))
					{
						dictionary.Add("item_i", num44);
					}
					else
					{
						dictionary["item_i"] = num44;
					}

				}
				else if (text3 == "highlight" || text3 == text9)
				{
					if (dictionary.ContainsKey("text"))
					{
						AddImportant((string)dictionary["text"], text4, dictionary);
					}
				}
				else if (text3 == "highlight_col")
				{
					if (!dictionary.ContainsKey("important_color"))
					{
						dictionary.Add("important_color", text4);
					}
					else
					{
						dictionary["important_color"] = text4;
					}

				}
				else if (text3 == "optionA" || text3 == text8)
				{
					if (!dictionary.ContainsKey("type"))
					{
						dictionary.Add("type", "MY_options");
					}
					if (!dictionary.ContainsKey("optionA"))
					{
						dictionary.Add("optionA", text4);
					}
					else
					{
						dictionary["optionA"] = text4;
					}

				}
				else if (text3 == "optionB" || text3 == text7)
				{
					if (!dictionary.ContainsKey("type"))
					{
						dictionary.Add("type", "MY_options");
					}
					if (!dictionary.ContainsKey("optionB"))
					{
						dictionary.Add("optionB", text4);
					}
					else
					{
						dictionary["optionB"] = text4;
					}

				}
				else if (text3 == "popuptext" || text3 == text6)
				{
					if (!dictionary.ContainsKey("message"))
					{
						dictionary.Add("message", text4);
					}
					else
					{
						dictionary["message"] = text4;
					}

				}
				else if (text3 == "goto")
				{
					dictionary.Add("go_to", int.Parse(text4, Startup.parse_culture));
				}
				else if (text3 == "gotoA")
				{
					dictionary.Add("optionA_goto", int.Parse(text4, Startup.parse_culture));
				}
				else if (text3 == "gotoB")
				{
					dictionary.Add("optionB_goto", int.Parse(text4, Startup.parse_culture));
				}
				else if (text3 == "goto_true")
				{
					dictionary.Add("goto_success", int.Parse(text4, Startup.parse_culture));
				}
				else if (text3 == "goto_false")
				{
					dictionary.Add("goto_failure", int.Parse(text4, Startup.parse_culture));
				}
			}
		}
		if (dictionary != null)
		{
			fullNPC.dialogue_data.Add(key, dictionary);
		}
		return fullNPC;
	}

	public static int ParseNumber(string input)
	{
		return int.Parse(input.Replace('[', ' ').Replace(']', ' '), Startup.parse_culture);
	}

	public void SetFocusNpc(GameObject npc_obj, string curr_NPC_display_name, string curr_NPC_combo_text, focus_type_t focus_type)
	{
		this.curr_NPC_display_name = curr_NPC_display_name;
		this.curr_NPC_combo_text = curr_NPC_combo_text;
		this.focus_type = focus_type;
		switch (focus_type)
		{
		case focus_type_t.stationary_npc:
			if (npc_obj.transform.Find("creature-go-here").childCount > 0)
			{
				curr_NPC_obj = npc_obj.transform.Find("creature-go-here").GetChild(0).gameObject;
				curr_NPC_obj.GetComponent<LiteModel>().animation_choppiness = GraphicsControl.Instance.SpecialAnimationChoppiness();
			}
			else
			{
				curr_NPC_obj = npc_obj;
			}
			break;
		case focus_type_t.moving_npc:
		{
			LiteModel myCreatureModel = npc_obj.GetComponent<SharedCreature>().myCreatureModel;
			curr_NPC_obj = myCreatureModel.gameObject;
			myCreatureModel.animation_choppiness = GraphicsControl.Instance.SpecialAnimationChoppiness();
			break;
		}
		case focus_type_t.painting:
			curr_NPC_obj = npc_obj.transform.parent.Find("models").gameObject;
			break;
		case focus_type_t.trophy:
			curr_NPC_obj = npc_obj.transform.gameObject;
			break;
		}
	}

	public void ReEnterDialogue(int entry_point)
	{
		GameObject objectAt = ChunkControl.Instance.GetObjectAt("DEBUG-npc", ChunkControl.Instance.player_zone, GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ, GameController.Instance.interacting_element_innerX, GameController.Instance.interacting_element_innerZ);
		SetFocusNpc(objectAt, curr_NPC_display_name, curr_NPC_combo_text, focus_type_t.stationary_npc);
		EnterDialogue(curr_dialogue_data_, entry_point, curr_voice);
	}

	public void EnterDialogue(Dictionary<int, Dictionary<string, object>> new_dialogue, int enter_dialogue_at, string voice)
	{
		curr_dialogue_data_ = new_dialogue;
		string text = (Startup.StringNullOrWhitespace(voice) ? "default" : voice);
		curr_voice = text;
		GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel.gameObject.SetActive(false);
		WindowPrefabsControl.Instance.CreateScreen("Dialogue - header", WindowPrefabsControl.build_into_t.GAME_CTR);
		WindowPrefabsControl.Instance.GetScreen("Dialogue - header").GetComponent<Animation>().Play("dialoguebobble");
		Text textLegacy = WindowPrefabsControl.Instance.GetTextLegacy("Dialogue - header", "npc name");
		Text textLegacy2 = WindowPrefabsControl.Instance.GetTextLegacy("Dialogue - header", "npc combo");
		Image image = WindowPrefabsControl.Instance.GetImage("Dialogue - header", "backdrop");
		textLegacy.text = curr_NPC_display_name;
		textLegacy2.text = curr_NPC_combo_text;
		if (curr_NPC_combo_text != "")
		{
			image.rectTransform.localPosition = new Vector3(0f, 0f, 0f);
			image.rectTransform.sizeDelta = new Vector2(159f, 54f);
		}
		else
		{
			image.rectTransform.localPosition = new Vector3(0f, 8f, 0f);
			image.rectTransform.sizeDelta = new Vector2(159f, 37f);
		}
		delayed_initial_blobble_t = DelayedInitialBlobble(enter_dialogue_at);
		StartCoroutine(delayed_initial_blobble_t);
		GameController.Instance.SetBackground_NPC();
		ResourceControl.Instance.PlayVoice(text + "1", voice_holder_1, 0f);
		ResourceControl.Instance.PlayVoice(text + "2", voice_holder_2, 0f);
		WindowControl.Instance.OpenWindow(WindowControl.window_type_t.dialogue);
	}

	private void FixedUpdate()
	{
		if (curr_dialogue_animation != null)
		{
			curr_dialogue_animation.Evaluate();
		}
	}

	private void NewShowText(string voice)
	{
		WindowPrefabsControl.Instance.GetTextMeshPro("Dialogue - other talk", "text").text = "";
		string text = (string)curr_other_dialogue["type"];
		string text2 = (string)curr_other_dialogue["text"];
		switch (text)
		{
		case "NPC_speak":
			curr_dialogue_animation = new DialogueAnimation(text2, voice, -1, -1, "", false);
			break;
		case "important_speak":
		{
			int important_start = (int)curr_other_dialogue["important_start"];
			int important_end = (int)curr_other_dialogue["important_end"];
			string important_col = (string)curr_other_dialogue["important_color"];
			curr_dialogue_animation = new DialogueAnimation(text2, voice, important_start, important_end, important_col, false);
			break;
		}
		case "show_quest_item":
		{
			string quest_name = (string)curr_other_dialogue["quest_name"];
			int progress = (int)curr_other_dialogue["quest_progress"];
			int index = (int)curr_other_dialogue["item_i"];
			InventoryItem inventoryItem = QuestControl.Instance.GetCollectablesRemaining(quest_name, progress)[index];
			text2 += inventoryItem.item_name;
			WindowPrefabsControl.Instance.GetObject("Dialogue - other talk", "item").GetComponent<ItemSprite>().RedrawBasic(inventoryItem, 1);
			curr_dialogue_animation = new DialogueAnimation(text2, voice, -1, -1, "", true);
			break;
		}
		case "show_quest_reward":
		{
			string quest_name2 = (string)curr_other_dialogue["quest_name"];
			List<ItemCountPair> reward_items = QuestControl.Instance.GetQuest(quest_name2).reward_items;
			WindowPrefabsControl.Instance.GetObject("Dialogue - other talk", "item").GetComponent<ItemSprite>().RedrawBasic(reward_items[0].item, 1);
			curr_dialogue_animation = new DialogueAnimation(text2, voice, -1, -1, "", true);
			break;
		}
		}
	}

	public void CloseWithIntentionOfMiniwindow(bool send_release_object)
	{
		ExitDialogue(send_release_object);
		WindowControl.Instance.close_button.SetActive(false);
		WindowControl.Instance.curr_window = WindowControl.window_type_t.none;
	}

	public void ClickOptionA()
	{
		GenericClickOption((int)curr_self_options["optionA_goto"]);
	}

	public void ClickOptionB()
	{
		GenericClickOption((int)curr_self_options["optionB_goto"]);
	}

	private void GenericClickOption(int go_to)
	{
		WindowPrefabsControl.Instance.DestroyScreen("Dialogue - 1 option");
		WindowPrefabsControl.Instance.DestroyScreen("Dialogue - 2 options");
		ProceedTo(go_to);
	}

	public void PressNext()
	{
		WindowPrefabsControl.Instance.DestroyScreen("Dialogue - other talk");
		WindowPrefabsControl.Instance.DestroyScreen("Dialogue - next");
		ProceedTo((int)curr_other_dialogue["go_to"]);
	}

	private void ProceedTo(int key)
	{
		if (curr_dialogue_data_ == null)
		{
			WindowControl.Instance.PressClose();
			return;
		}
		if (!curr_dialogue_data_.ContainsKey(key))
		{
			switch (key)
			{
			case -99:
				inventory_ctr.Instance.open_sell();
				CloseWithIntentionOfMiniwindow(false);
				return;
			case -88:
				inventory_ctr.Instance.open_buy();
				CloseWithIntentionOfMiniwindow(false);
				return;
			case -77:
			{
				bool flag = DevBuildControl.Instance.IsLockedByDev(GameController.Instance.interacting_element_item);
				bool flag2 = inventory_ctr.Instance.is_locked_by_player(GameController.Instance.interacting_element_item);
				if (!flag)
				{
					if (!flag2)
					{
						inventory_ctr.Instance.TryOpenWorldContainer(GameController.Instance.interacting_element_item, GameController.Instance.interacting_element_rot, GameController.Instance.interacting_element_innerX, GameController.Instance.interacting_element_innerZ, GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ);
					}
					else
					{
						LockControl.Instance.OpenLockScreen("Enter password to unlock", LockControl.lock_context.unlock_container);
					}
					CloseWithIntentionOfMiniwindow(false);
					return;
				}
				GameplayGUIControl.Instance.ShowNotif("Custom Statue locked.", DevBuildControl.Instance.lockedhome_notif, new OnNotifClick(OnNotifClick.type.none));
				break;
			}
			case -66:
			{
				GameController.Instance.interacting_element_item.GetString("npc_display_name");
				string @string = GameController.Instance.interacting_element_item.GetString("companion_owner");
				int num = 0;
				if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host && !LandClaimControl.Instance.LandOwnedByMe(ChunkControl.Instance.player_zone, GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ))
				{
					num = ((@string != PlayerData.Instance.GetGlobalString("username_lower")) ? 4 : 0);
				}
				if (!GameServerConnector.Instance.FullyInGame() || CompanionController.Instance.active_companions.Count + 1 <= GameServerReceiver.Instance.max_companions)
				{
					if (num == 0)
					{
						if (CompanionController.Instance.active_companions.Count < 2)
						{
							CompanionController.Instance.AcceptCompanionFollow();
							num = 0;
						}
						else
						{
							num = 1;
						}
					}
				}
				else
				{
					num = 2;
				}
				WindowControl.Instance.PressClose();
				switch (num)
				{
				case 4:
					PopupControl.Instance.ShowMessage("Cannot Follow!\n<color=#aaaaaa>Companion belongs to " + @string + "</color>", PopupControl.context.message);
					break;
				case 2:
					PopupControl.Instance.ShowMessage("You can't have more than " + GameServerReceiver.Instance.max_companions + " followers on this server!", PopupControl.context.message);
					break;
				case 1:
					PopupControl.Instance.ShowMessage("You can only have 2 followers at a time!", PopupControl.context.message);
					break;
				}
				return;
			}
			case -55:
				CompanionController.Instance.AcceptFreeCompanion();
				break;
			}
			WindowControl.Instance.PressClose();
			return;
		}
		switch ((string)curr_dialogue_data_[key]["type"])
		{
		case "test_quest_progress":
		{
			string quest_name4 = (string)curr_dialogue_data_[key]["quest_name"];
			int num5 = (int)curr_dialogue_data_[key]["test_progress"];
			int key2 = (int)curr_dialogue_data_[key]["goto_success"];
			int key3 = (int)curr_dialogue_data_[key]["goto_failure"];
			ProceedTo((QuestControl.Instance.GetQuestProgress(quest_name4) == num5) ? key2 : key3);
			return;
		}
		case "test_is_my_world":
		{
			int key2 = (int)curr_dialogue_data_[key]["goto_success"];
			int key3 = (int)curr_dialogue_data_[key]["goto_failure"];
			ProceedTo((!GameServerConnector.Instance.FullyInGame() || GameServerConnector.Instance.is_host) ? key2 : key3);
			return;
		}
		case "test_has_all_quest_items":
		{
			string quest_name5 = (string)curr_dialogue_data_[key]["quest_name"];
			int progress2 = (int)curr_dialogue_data_[key]["step"];
			int key2 = (int)curr_dialogue_data_[key]["goto_success"];
			int key3 = (int)curr_dialogue_data_[key]["goto_failure"];
			ProceedTo((QuestControl.Instance.HasAllQuestItemsInInventory(quest_name5, progress2)) ? key2 : key3);
			return;
		}
		case "test_quest_ready_to_restart":
		{
			string quest_name6 = (string)curr_dialogue_data_[key]["quest_name"];
			int key2 = (int)curr_dialogue_data_[key]["goto_success"];
			int key3 = (int)curr_dialogue_data_[key]["goto_failure"];
			ProceedTo((QuestControl.Instance.IsQuestReadyToRepeat(quest_name6)) ? key2 : key3);
			return;
		}
		case "test_collected_all_quest_items":
		{
			string quest_name7 = (string)curr_dialogue_data_[key]["quest_name"];
			int progress3 = (int)curr_dialogue_data_[key]["step"];
			int key2 = (int)curr_dialogue_data_[key]["goto_success"];
			int key3 = (int)curr_dialogue_data_[key]["goto_failure"];
			ProceedTo((QuestControl.Instance.GetCollectablesRemaining(quest_name7, progress3).Count == 0) ? key2 : key3);
			return;
		}
		case "test_less_than_2_companions":
		{
			int key2 = (int)curr_dialogue_data_[key]["goto_success"];
			int key3 = (int)curr_dialogue_data_[key]["goto_failure"];
			ProceedTo((CompanionController.Instance.active_companions.Count < 2) ? key2 : key3);
			return;
		}
		case "test_has_any_quest_items":
		{
			string quest_name8 = (string)curr_dialogue_data_[key]["quest_name"];
			int progress4 = (int)curr_dialogue_data_[key]["step"];
			int key2 = (int)curr_dialogue_data_[key]["goto_success"];
			int key3 = (int)curr_dialogue_data_[key]["goto_failure"];
			ProceedTo((QuestControl.Instance.HasAnyQuestItemsInInventory(quest_name8, progress4)) ? key2 : key3);
			return;
		}
		case "quest_rewards":
		{
			string quest_name = (string)curr_dialogue_data_[key]["quest_name"];
			int set_progress_to = (int)curr_dialogue_data_[key]["set_progress_on_success"];
			int num2 = (int)curr_dialogue_data_[key]["go_to"];
			List<ItemCountPair> reward_items = QuestControl.Instance.GetQuest(quest_name).reward_items;
			foreach (ItemCountPair item in reward_items)
			{
				if (!inventory_ctr.Instance.CanReceiveItem(item.item, item.count))
				{
					WindowControl.Instance.PressClose();
					string message = "<color=#ff0000>Inventory Full!</color>\nYou need an empty slot to accept the reward!";
					if (reward_items.Count != 1)
					{
						message = "<color=#ff0000>Inventory Full!</color>\nYou need " + reward_items.Count + " empty slots to accept the reward!";
					}
					PopupControl.Instance.ShowMessage(message, PopupControl.context.message, reward_items);
					return;
				}
			}
			if (num2 == -1)
			{
				WindowControl.Instance.PressClose();
			}
			else
			{
				ProceedTo(num2);
			}
			foreach (ItemCountPair item2 in reward_items)
			{
				inventory_ctr.Instance.GiveItem(item2.item, item2.count, "", false);
			}
			if (reward_items.Count == 1)
			{
				PopupControl.Instance.ShowMessage("<color=#2ebdff>Received Item!</color>\n" + inventory_ctr.Instance.GetFullItemName(reward_items[0].item), PopupControl.context.quest_reward, reward_items);
			}
			else
			{
				string text = "";
				for (int i = 0; i < reward_items.Count; i++)
				{
					string fullItemName = inventory_ctr.Instance.GetFullItemName(reward_items[i].item);
					text = ((i != 0) ? (text + ", " + fullItemName) : (text + fullItemName));
				}
				PopupControl.Instance.ShowMessage("<color=#2ebdff>Received Items!</color>\n" + text, PopupControl.context.quest_reward, reward_items);
			}
			QuestControl.Instance.SetQuestProgress(quest_name, set_progress_to);
			foreach (int reward_chest in QuestControl.Instance.GetQuest(quest_name).reward_chests)
			{
				LootControl.Instance.GenerateLootChest(new InventoryItem("Sky Chest")).SaveToAllAsContainer(reward_chest);
				PlayerData.Instance.SetSlotShort("NPCchest_" + reward_chest, 1, PlayerData.filename_t.generated_NPC_chests);
			}
			foreach (int reward_superChest in QuestControl.Instance.GetQuest(quest_name).reward_superChests)
			{
				BasketContents basketContents = new BasketContents();
				LootControl.Instance.AddManyLoots(2000, 4000, basketContents, 6, 10, "Chest", new InventoryItem("Chest"));
				basketContents.SaveToAllAsContainer(reward_superChest);
				PlayerData.Instance.SetSlotShort("NPCchest_" + reward_superChest, 1, PlayerData.filename_t.generated_NPC_chests);
			}
			return;
		}
		case "advance_quest":
		{
			string quest_name2 = (string)curr_dialogue_data_[key]["quest_name"];
			int set_progress_to2 = (int)curr_dialogue_data_[key]["set_progress_to"];
			int key4 = (int)curr_dialogue_data_[key]["go_to"];
			QuestControl.Instance.SetQuestProgress(quest_name2, set_progress_to2);
			ProceedTo(key4);
			return;
		}
		case "take_quest_items":
		{
			string quest_name3 = (string)curr_dialogue_data_[key]["quest_name"];
			int progress = (int)curr_dialogue_data_[key]["curr_progress"];
			int set_progress_if_all_collected = (int)curr_dialogue_data_[key]["set_progress_if_all_collected"];
			int key5 = (int)curr_dialogue_data_[key]["go_to"];
			QuestControl.Instance.TakeQuestItems(quest_name3, progress, set_progress_if_all_collected);
			ProceedTo(key5);
			return;
		}
		case "exit_popup_message":
		{
			string message2 = (string)curr_dialogue_data_[key]["message"];
			WindowControl.Instance.PressClose();
			PopupControl.Instance.ShowMessage(message2, PopupControl.context.message);
			return;
		}
		case "teleport":
		{
			CloseWithIntentionOfMiniwindow(true);
			string position_str = (string)curr_dialogue_data_[key]["tele_str"];
			QuestControl.parsed_position parsed_position = QuestControl.Instance.ParsePosition(position_str);
			CustomTeleporterControl.Instance.click_teleport_zone_to = parsed_position.zone;
			CustomTeleporterControl.Instance.click_teleport_to_chunkX = parsed_position.chunkX;
			CustomTeleporterControl.Instance.click_teleport_to_chunkZ = parsed_position.chunkZ;
			CustomTeleporterControl.Instance.click_teleport_to_innerX = parsed_position.innerX;
			CustomTeleporterControl.Instance.click_teleport_to_innerZ = parsed_position.innerZ;
			CustomTeleporterControl.Instance.DelayedTeleportTransition(-1, CustomTeleporterControl.tele_type.custom_network, 0.8f);
			return;
		}
		case "MY_options":
		{
			curr_self_options = curr_dialogue_data_[key];
			string text2 = (string)curr_self_options["optionA"];
			string text3 = "";
			if (curr_self_options.ContainsKey("optionB"))
			{
				text3 = (string)curr_self_options["optionB"];
			}
			int num3 = (int)curr_self_options["optionA_goto"];
			int num4 = ((!curr_self_options.ContainsKey("optionB_goto")) ? (-1) : ((int)curr_self_options["optionB_goto"]));
			if (text3 != "")
			{
				WindowPrefabsControl.Instance.CreateScreen("Dialogue - 2 options", WindowPrefabsControl.build_into_t.GAME_CTR);
				Text textLegacy = WindowPrefabsControl.Instance.GetTextLegacy("Dialogue - 2 options", "option text L");
				Text textLegacy2 = WindowPrefabsControl.Instance.GetTextLegacy("Dialogue - 2 options", "option text R");
				textLegacy.text = text2;
				textLegacy2.text = text3;
				if (num3 == -99)
				{
					textLegacy.transform.parent.Find("icon").gameObject.SetActive(true);
					textLegacy.transform.parent.Find("icon").GetComponent<Image>().sprite = sell_icon_blue;
					textLegacy.rectTransform.localPosition = new Vector3(20.4f, 2f, 0f);
					textLegacy.rectTransform.sizeDelta = new Vector2(129f, 49f);
				}
				else if (num3 == -88)
				{
					textLegacy.transform.parent.Find("icon").gameObject.SetActive(true);
					textLegacy.transform.parent.Find("icon").GetComponent<Image>().sprite = buy_icon_blue;
					textLegacy.rectTransform.localPosition = new Vector3(20.4f, 2f, 0f);
					textLegacy.rectTransform.sizeDelta = new Vector2(129f, 49f);
				}
				else
				{
					textLegacy.transform.parent.Find("icon").gameObject.SetActive(false);
					textLegacy.rectTransform.localPosition = new Vector3(0.8f, 2f, 0f);
					textLegacy.rectTransform.sizeDelta = new Vector2(169f, 49f);
				}
				if (num4 == -99)
				{
					textLegacy2.transform.parent.Find("icon").gameObject.SetActive(true);
					textLegacy2.transform.parent.Find("icon").GetComponent<Image>().sprite = sell_icon_green;
					textLegacy2.rectTransform.localPosition = new Vector3(20.4f, 2f, 0f);
					textLegacy2.rectTransform.sizeDelta = new Vector2(129f, 49f);
				}
				else if (num4 == -88)
				{
					textLegacy2.transform.parent.Find("icon").gameObject.SetActive(true);
					textLegacy2.transform.parent.Find("icon").GetComponent<Image>().sprite = buy_icon_green;
					textLegacy2.rectTransform.localPosition = new Vector3(20.4f, 2f, 0f);
					textLegacy2.rectTransform.sizeDelta = new Vector2(129f, 49f);
				}
				else
				{
					textLegacy2.transform.parent.Find("icon").gameObject.SetActive(false);
					textLegacy2.rectTransform.localPosition = new Vector3(0.8f, 2f, 0f);
					textLegacy2.rectTransform.sizeDelta = new Vector2(169f, 49f);
				}
			}
			else
			{
				WindowPrefabsControl.Instance.CreateScreen("Dialogue - 1 option", WindowPrefabsControl.build_into_t.GAME_CTR);
				Text textLegacy3 = WindowPrefabsControl.Instance.GetTextLegacy("Dialogue - 1 option", "option text");
				textLegacy3.text = text2;
				if (num3 == -99)
				{
					textLegacy3.transform.parent.Find("icon").gameObject.SetActive(true);
					textLegacy3.transform.parent.Find("icon").GetComponent<Image>().sprite = sell_icon_green;
					textLegacy3.rectTransform.localPosition = new Vector3(20.4f, 2f, 0f);
					textLegacy3.rectTransform.sizeDelta = new Vector2(129f, 49f);
				}
				else if (num3 == -88)
				{
					textLegacy3.transform.parent.Find("icon").gameObject.SetActive(true);
					textLegacy3.transform.parent.Find("icon").GetComponent<Image>().sprite = buy_icon_green;
					textLegacy3.rectTransform.localPosition = new Vector3(20.4f, 2f, 0f);
					textLegacy3.rectTransform.sizeDelta = new Vector2(129f, 49f);
				}
				else
				{
					textLegacy3.transform.parent.Find("icon").gameObject.SetActive(false);
					textLegacy3.rectTransform.localPosition = new Vector3(0.8f, 2f, 0f);
					textLegacy3.rectTransform.sizeDelta = new Vector2(169f, 49f);
				}
			}
			return;
		}
		case "NPC_speak":
		case "important_speak":
		case "show_quest_item":
		case "show_quest_reward":
			curr_other_dialogue = curr_dialogue_data_[key];
			BlobbleCurrText();
			return;
		}
		WindowControl.Instance.PressClose();
	}

	public void ExitDialogue(bool send_release_object)
	{
		if ((focus_type == focus_type_t.stationary_npc || focus_type == focus_type_t.moving_npc) && curr_NPC_obj != null && curr_NPC_obj.GetComponent<LiteModel>() != null)
		{
			curr_NPC_obj.GetComponent<LiteModel>().animation_choppiness = GraphicsControl.Instance.DefaultAnimationChoppiness();
		}
		curr_NPC_obj = null;
		focus_type = focus_type_t.none;
		GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel.gameObject.SetActive(true);
		if (delayed_initial_blobble_t != null)
		{
			StopCoroutine(delayed_initial_blobble_t);
			delayed_initial_blobble_t = null;
		}
		curr_dialogue_animation = null;
		WindowPrefabsControl.Instance.DestroyScreen("Dialogue - header");
		WindowPrefabsControl.Instance.DestroyScreen("Dialogue - other talk");
		WindowPrefabsControl.Instance.DestroyScreen("Dialogue - next");
		WindowPrefabsControl.Instance.DestroyScreen("Dialogue - 1 option");
		WindowPrefabsControl.Instance.DestroyScreen("Dialogue - 2 options");
		GameController.Instance.SetBackground_Explore();
		if (QuestControl.Instance.quest_notif_on_exit_dialogue != "")
		{
			GameplayGUIControl.Instance.ShowNotif(QuestControl.Instance.quest_notif_on_exit_dialogue, DevBuildControl.Instance.quest_updated_ico, new OnNotifClick(OnNotifClick.type.quests));
			QuestControl.Instance.quest_notif_on_exit_dialogue = "";
		}
		if (send_release_object)
		{
			GameServerSender.Instance.SendReleaseInteractingObject();
		}
		GameServerReceiver.Instance.HideReportObjectButton();
	}

	public void BlobbleCurrText()
	{
		WindowPrefabsControl.Instance.CreateScreen("Dialogue - other talk", WindowPrefabsControl.build_into_t.GAME_CTR);
		NewShowText(curr_voice);
	}

	private IEnumerator DelayedInitialBlobble(int enter_at)
	{
		yield return new WaitForSeconds(1f);
		ProceedTo(enter_at);
	}
}
