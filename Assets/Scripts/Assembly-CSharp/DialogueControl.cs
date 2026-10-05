using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	private string curr_voice;

	private Dictionary<string, FullNPC> loaded_FullNPCs;

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
	}

	public FullNPC GetFullCurrNPC()
	{
		return null;
	}

	public FullNPC GetFullNPC(InventoryItem npc_item)
	{
		return null;
	}

	public void AddImportant(string entire_string, string highlight, Dictionary<string, object> curr_dialogue_collection)
	{
	}

	public FullNPC LoadFullNpcFile(InventoryItem npc_item)
	{
		return null;
	}

	public static int ParseNumber(string input)
	{
		return 0;
	}

	public void SetFocusNpc(GameObject npc_obj, string curr_NPC_display_name, string curr_NPC_combo_text, focus_type_t focus_type)
	{
	}

	public void ReEnterDialogue(int entry_point)
	{
	}

	public void EnterDialogue(Dictionary<int, Dictionary<string, object>> new_dialogue, int enter_dialogue_at, string voice)
	{
	}

	private void FixedUpdate()
	{
	}

	private void NewShowText(string voice)
	{
	}

	public void CloseWithIntentionOfMiniwindow(bool send_release_object)
	{
	}

	public void ClickOptionA()
	{
	}

	public void ClickOptionB()
	{
	}

	private void GenericClickOption(int go_to)
	{
	}

	public void PressNext()
	{
	}

	private void ProceedTo(int key)
	{
	}

	public void ExitDialogue(bool send_release_object)
	{
	}

	public void BlobbleCurrText()
	{
	}

	private IEnumerator DelayedInitialBlobble(int enter_at)
	{
		return null;
	}
}
