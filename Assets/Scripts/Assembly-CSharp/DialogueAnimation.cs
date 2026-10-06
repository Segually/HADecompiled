using System.Diagnostics;

public class DialogueAnimation
{
	private LiteModel npc_model_cached;

	private Stopwatch dialogue_stopwatch = new Stopwatch();

	private Stopwatch mouth_stopwatch = new Stopwatch();

	private float total_milliseconds;

	private bool mouth_open = true;

	private float milliseconds_per_char;

	private float milliseconds_per_sfx;

	private float milliseconds_per_mouthChange;

	private string final_string;

	private string important_col;

	private int important_start;

	private int important_end;

	private bool show_quest_item;

	private LiteModel npc_model
	{
		get
		{
			if (npc_model_cached != null)
			{
				return npc_model_cached;
			}
			if (DialogueControl.Instance.curr_NPC_obj == null)
			{
				return null;
			}
			if (DialogueControl.Instance.curr_NPC_obj.GetComponent<LiteModel>() == null)
			{
				return null;
			}
			npc_model_cached = DialogueControl.Instance.curr_NPC_obj.GetComponent<LiteModel>();
			return npc_model_cached;
		}
	}

	public DialogueAnimation(string final_string, string voice, int important_start, int important_end, string important_col, bool show_quest_item)
	{
		milliseconds_per_char = 19f;
		milliseconds_per_sfx = 130f;
		milliseconds_per_mouthChange = 130f;
		if (PlayerData.Instance.GetGlobalShort("dev_mode") == 1)
		{
			milliseconds_per_char /= ConsoleControl.dialogue_speed_mod;
			milliseconds_per_sfx /= ConsoleControl.dialogue_speed_mod;
			milliseconds_per_mouthChange /= ConsoleControl.dialogue_speed_mod;
		}
		this.final_string = final_string;
		this.important_col = important_col;
		this.important_start = important_start;
		this.important_end = important_end;
		this.show_quest_item = show_quest_item;
		total_milliseconds = milliseconds_per_char * (float)final_string.Length;
		dialogue_stopwatch.Start();
		mouth_stopwatch.Start();
		int num = (int)(total_milliseconds / milliseconds_per_sfx);
		if (num == 0)
		{
			num = 1;
		}
		for (int i = 0; i < num; i++)
		{
			AudioControl.Instance.PlayDialogue(voice + ((UnityEngine.Random.value < 0.5f) ? "1" : "2"), milliseconds_per_sfx * 0.001f * (float)i);
		}
		if (show_quest_item)
		{
			WindowPrefabsControl.Instance.GetObject("Dialogue - other talk", "item").SetActive(true);
		}
	}

	public void Evaluate()
	{
		long elapsedMilliseconds = dialogue_stopwatch.ElapsedMilliseconds;
		float num = ((total_milliseconds != 0f) ? UnityEngine.Mathf.Clamp01((float)elapsedMilliseconds / total_milliseconds) : 0f);
		int num2 = (int)(num * (float)final_string.Length);
		string text = final_string.Substring(0, num2);
		if (important_start != -1 && important_start <= num2)
		{
			if (important_end < num2)
			{
				text = text.Substring(0, important_start) + "<color=" + important_col + ">" + text.Substring(important_start, important_end - important_start) + "</color>" + text.Substring(important_end, text.Length - important_end);
			}
			else
			{
				int length = UnityEngine.Mathf.Min(important_end - important_start, text.Length - important_start);
				text = text.Substring(0, important_start) + "<color=" + important_col + ">" + text.Substring(important_start, length) + "</color>";
			}
		}
		TMPro.TextMeshProUGUI textMeshPro = WindowPrefabsControl.Instance.GetTextMeshPro("Dialogue - other talk", "text");
		textMeshPro.text = text;
		if (show_quest_item)
		{
			float preferredWidth = textMeshPro.preferredWidth;
			UnityEngine.Transform parent = WindowPrefabsControl.Instance.GetObject("Dialogue - other talk", "item").transform.parent;
			parent.localPosition = textMeshPro.transform.localPosition + UnityEngine.Vector3.right * (preferredWidth * 0.5f + 25f);
		}
		if ((float)mouth_stopwatch.ElapsedMilliseconds > milliseconds_per_mouthChange)
		{
			mouth_stopwatch.Restart();
			if (npc_model != null)
			{
				if (!mouth_open)
				{
					mouth_open = true;
					float value = UnityEngine.Random.value;
					npc_model.TryAssignMouthTexture((value < 0.5f) ? "Mouths" : "Mouths2");
				}
				else
				{
					mouth_open = false;
					npc_model.TryAssignMouthTexture("Mouths-closed");
				}
			}
		}
		if (!(num < 1f))
		{
			if (npc_model != null)
			{
				if (UnityEngine.Random.value < 0.333f)
				{
					npc_model.TryAssignMouthTexture("Mouths");
				}
				else
				{
					float value2 = UnityEngine.Random.value;
					npc_model.TryAssignMouthTexture((value2 < 0.333f) ? "Mouths2" : "Mouths-closed");
				}
			}
			WindowPrefabsControl.Instance.CreateScreen("Dialogue - next", WindowPrefabsControl.build_into_t.GAME_CTR);
			DialogueControl.Instance.curr_dialogue_animation = null;
		}
	}
}
