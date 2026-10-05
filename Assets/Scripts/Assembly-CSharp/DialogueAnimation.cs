using System.Diagnostics;

public class DialogueAnimation
{
	private LiteModel npc_model_cached;

	private Stopwatch dialogue_stopwatch;

	private Stopwatch mouth_stopwatch;

	private float total_milliseconds;

	private bool mouth_open;

	private float milliseconds_per_char;

	private float milliseconds_per_sfx;

	private float milliseconds_per_mouthChange;

	private string final_string;

	private string important_col;

	private int important_start;

	private int important_end;

	private bool show_quest_item;

	private LiteModel npc_model => null;

	public DialogueAnimation(string final_string, string voice, int important_start, int important_end, string important_col, bool show_quest_item)
	{
	}

	public void Evaluate()
	{
	}
}
