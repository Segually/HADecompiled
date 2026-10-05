using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkScreenDev : MonoBehaviour
{
	public enum dev_window_type_t
	{
		none = 0,
		place_perk = 1,
		add_text = 2,
		add_lines = 3
	}

	private enum bump_direction
	{
		up = 0,
		down = 1,
		left = 2,
		right = 3
	}

	public static PerkScreenDev Instance;

	public GameObject dev_buttons;

	public CanvasGroup dev_button_remove;

	public CanvasGroup dev_button_outline;

	public CanvasGroup dev_button_place_perk;

	public CanvasGroup dev_button_addtext;

	public CanvasGroup dev_button_addlines;

	public CanvasGroup dev_button_bump;

	private GameObject dev_cursor;

	public GameObject add_perk_window;

	public GameObject bump_tab;

	public dev_window_type_t window_input;

	public TextMeshProUGUI text_add_perk_window;

	private string curr_tool_id;

	public bool dev_mode;

	private PerkScreenHex bump_origin;

	private bool bumping;

	private List<PerkScreenHex> move_list;

	private Dictionary<string, string> add_extra_values;

	public Image perk_button_ico;

	public TMP_InputField add_perk_input_key;

	public void PressSave()
	{
	}

	public void PressDevTool(string tool_id)
	{
	}

	private void DeselectAllToolButtons()
	{
	}

	private void SetDevTool(string tool_id)
	{
	}

	public void PressCompleteBump()
	{
	}

	private void GetSurrounding(int hex_x, int hex_y, int hex_offset, ref List<PerkScreenHex> added)
	{
	}

	private void MoveAll(List<PerkScreenHex> to_move, bump_direction direction)
	{
	}

	private void Update()
	{
	}

	public void PressCloseAddPerkWindow()
	{
	}

	public void PressAcceptAddPerk()
	{
	}

	public void EnableDevMode()
	{
	}

	private float Round10(int num)
	{
		return 0f;
	}

	public PerkScreenHex FindNearestHexSnapPoint(Vector3 local_location)
	{
		return null;
	}
}
