using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

	private string curr_tool_id = "";

	public bool dev_mode;

	private PerkScreenHex bump_origin;

	private bool bumping;

	private List<PerkScreenHex> move_list = new List<PerkScreenHex>();

	private Dictionary<string, string> add_extra_values = new Dictionary<string, string>();

	public Image perk_button_ico;

	public TMP_InputField add_perk_input_key;

	public void PressSave()
	{
		PopupControl.Instance.SetButtonWasPressed();
		Debug.Log("<color=#00ff00>PERKS DATA SAVED</color>");
		foreach (KeyValuePair<string, PerkScreenHex> hex_point in PerkScreen.Instance.hex_points)
		{
			_ = hex_point.Value.type;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, PerkScreenHex> hex_point2 in PerkScreen.Instance.hex_points)
		{
			PerkScreenHex value = hex_point2.Value;
			if (value.type == PerkScreenHex.hex_type.none)
			{
				continue;
			}
			list.Add("[hex]");
			switch (value.type)
			{
			case PerkScreenHex.hex_type.outline:
				list.Add("hex_type=outline");
				break;
			case PerkScreenHex.hex_type.perk:
				list.Add("hex_type=perk");
				break;
			case PerkScreenHex.hex_type.text:
				list.Add("hex_type=text");
				break;
			}
			list.Add("hex_dev_x=" + value.hex_dev_x);
			list.Add("hex_dev_y=" + value.hex_dev_y);
			list.Add("hex_dev_offset=" + value.hex_dev_offset);
			foreach (KeyValuePair<string, string> extra_value in value.extra_values)
			{
				list.Add(extra_value.Key + "=" + extra_value.Value);
			}
			list.Add("");
		}
		System.IO.File.WriteAllLines(Application.dataPath + "/SYNCHRONOUS/TextFiles/PerkScreenHexLayout.txt", list.ToArray());
	}

	public void PressDevTool(string tool_id)
	{
		PopupControl.Instance.SetButtonWasPressed();
		DeselectAllToolButtons();
		if (tool_id == "place_perk")
		{
			curr_tool_id = "";
			window_input = dev_window_type_t.place_perk;
			add_perk_window.SetActive(true);
			text_add_perk_window.text = "What perk would you like to place? (perk_key)";
		}
		else if (tool_id == "add_text")
		{
			curr_tool_id = "";
			window_input = dev_window_type_t.add_text;
			add_perk_window.SetActive(true);
			text_add_perk_window.text = "What text would you like to add?";
		}
		else if (tool_id == "add_lines")
		{
			curr_tool_id = "";
			window_input = dev_window_type_t.add_lines;
			add_perk_window.SetActive(true);
			text_add_perk_window.text = "What perk_key should be required to light up this line?";
		}
		else
		{
			SetDevTool(tool_id);
		}
	}

	private void DeselectAllToolButtons()
	{
		dev_button_remove.alpha = 0.3f;
		dev_button_outline.alpha = 0.3f;
		dev_button_place_perk.alpha = 0.3f;
		dev_button_addtext.alpha = 0.3f;
		dev_button_addlines.alpha = 0.3f;
		dev_button_bump.alpha = 0.3f;
	}

	private void SetDevTool(string tool_id)
	{
		curr_tool_id = tool_id;
		switch (curr_tool_id)
		{
		case "remove":
			dev_button_remove.alpha = 1f;
			break;
		case "outline":
			dev_button_outline.alpha = 1f;
			break;
		case "place_perk":
			dev_button_place_perk.alpha = 1f;
			break;
		case "add_text":
			dev_button_addtext.alpha = 1f;
			break;
		case "add_lines":
			dev_button_addlines.alpha = 1f;
			break;
		case "bump":
			dev_button_bump.alpha = 1f;
			break;
		}
	}

	public void PressCompleteBump()
	{
		bump_tab.SetActive(false);
		dev_buttons.SetActive(true);
		bump_origin = null;
		bumping = false;
		move_list.Clear();
	}

	private void GetSurrounding(int hex_x, int hex_y, int hex_offset, ref List<PerkScreenHex> added)
	{
		PerkScreenHex perkScreenHex = PerkScreen.Instance.FindHexByDevLookup(hex_x, hex_y, hex_offset);
		if (perkScreenHex == null || perkScreenHex.type == PerkScreenHex.hex_type.none || added.Contains(perkScreenHex))
		{
			return;
		}
		added.Add(perkScreenHex);
		GetSurrounding(hex_x - 1, hex_y, hex_offset, ref added);
		GetSurrounding(hex_x + 1, hex_y, hex_offset, ref added);
		if (hex_offset == 1)
		{
			GetSurrounding(hex_x, hex_y + 1, 0, ref added);
			GetSurrounding(hex_x + 1, hex_y + 1, 0, ref added);
			GetSurrounding(hex_x, hex_y, 0, ref added);
			GetSurrounding(hex_x + 1, hex_y, 0, ref added);
		}
		else if (hex_offset == 0)
		{
			GetSurrounding(hex_x, hex_y, 1, ref added);
			GetSurrounding(hex_x - 1, hex_y, 1, ref added);
			GetSurrounding(hex_x, hex_y - 1, 1, ref added);
			GetSurrounding(hex_x - 1, hex_y - 1, 1, ref added);
		}
	}

	private void MoveAll(List<PerkScreenHex> to_move, bump_direction direction)
	{
		foreach (PerkScreenHex item in to_move)
		{
			Object.Destroy(item.obj);
			foreach (GameObject corresponding_line_object in item.corresponding_line_objects)
			{
				Object.Destroy(corresponding_line_object);
			}
			item.corresponding_line_objects.Clear();
			string key = item.hex_dev_x + "," + item.hex_dev_y + "," + item.hex_dev_offset;
			PerkScreenHex perkScreenHex = new PerkScreenHex(PerkScreenHex.hex_type.none, new Dictionary<string, string>(), item.hex_dev_x, item.hex_dev_y, item.hex_dev_offset);
			PerkScreen.Instance.hex_points[key] = perkScreenHex;
			perkScreenHex.Redraw();
		}
		switch (direction)
		{
		case bump_direction.up:
		{
			int hex_dev_offset = bump_origin.hex_dev_offset;
			foreach (PerkScreenHex item2 in to_move)
			{
				if (hex_dev_offset == 0)
				{
					if (item2.hex_dev_offset == 0)
					{
						item2.hex_dev_offset = 1;
					}
					else if (item2.hex_dev_offset == 1)
					{
						item2.hex_dev_offset = 0;
						item2.hex_dev_x++;
						item2.hex_dev_y++;
					}
				}
				else if (item2.hex_dev_offset == 0)
				{
					item2.hex_dev_offset = 1;
					item2.hex_dev_x--;
				}
				else if (item2.hex_dev_offset == 1)
				{
					item2.hex_dev_y++;
					item2.hex_dev_offset = 0;
				}
			}
			break;
		}
		case bump_direction.down:
		{
			int hex_dev_offset2 = bump_origin.hex_dev_offset;
			foreach (PerkScreenHex item3 in to_move)
			{
				if (hex_dev_offset2 == 0)
				{
					if (item3.hex_dev_offset == 0)
					{
						item3.hex_dev_y--;
						item3.hex_dev_offset = 1;
					}
					else if (item3.hex_dev_offset == 1)
					{
						item3.hex_dev_offset = 0;
						item3.hex_dev_x++;
					}
				}
				else if (item3.hex_dev_offset == 0)
				{
					item3.hex_dev_offset = 1;
					item3.hex_dev_x--;
					item3.hex_dev_y--;
				}
				else if (item3.hex_dev_offset == 1)
				{
					item3.hex_dev_offset = 0;
				}
			}
			break;
		}
		case bump_direction.left:
			foreach (PerkScreenHex item4 in to_move)
			{
				item4.hex_dev_x--;
			}
			break;
		case bump_direction.right:
			foreach (PerkScreenHex item5 in to_move)
			{
				item5.hex_dev_x++;
			}
			break;
		}
		foreach (PerkScreenHex item6 in to_move)
		{
			string key2 = item6.hex_dev_x + "," + item6.hex_dev_y + "," + item6.hex_dev_offset;
			if (!PerkScreen.Instance.hex_points.ContainsKey(key2))
			{
				PerkScreen.Instance.hex_points.Add(key2, item6);
			}
			else
			{
				PerkScreenHex perkScreenHex2 = PerkScreen.Instance.hex_points[key2];
				Object.Destroy(perkScreenHex2.obj);
				foreach (GameObject corresponding_line_object2 in perkScreenHex2.corresponding_line_objects)
				{
					Object.Destroy(corresponding_line_object2);
				}
				perkScreenHex2.corresponding_line_objects.Clear();
				PerkScreen.Instance.hex_points[key2] = item6;
			}
			item6.Redraw();
		}
	}

	private void Update()
	{
		if (!dev_mode)
		{
			return;
		}
		if (bumping)
		{
			if (GamepadInput.Instance.GetKeyDown(Key.LeftArrow) || GamepadInput.Instance.GetKeyDown(Key.A))
			{
				if (move_list.Count == 0)
				{
					GetSurrounding(bump_origin.hex_dev_x, bump_origin.hex_dev_y, bump_origin.hex_dev_offset, ref move_list);
				}
				MoveAll(move_list, bump_direction.left);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.RightArrow) || GamepadInput.Instance.GetKeyDown(Key.D))
			{
				if (move_list.Count == 0)
				{
					GetSurrounding(bump_origin.hex_dev_x, bump_origin.hex_dev_y, bump_origin.hex_dev_offset, ref move_list);
				}
				MoveAll(move_list, bump_direction.right);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.UpArrow) || GamepadInput.Instance.GetKeyDown(Key.W))
			{
				if (move_list.Count == 0)
				{
					GetSurrounding(bump_origin.hex_dev_x, bump_origin.hex_dev_y, bump_origin.hex_dev_offset, ref move_list);
				}
				MoveAll(move_list, bump_direction.up);
			}
			if (GamepadInput.Instance.GetKeyDown(Key.DownArrow) || GamepadInput.Instance.GetKeyDown(Key.S))
			{
				if (move_list.Count == 0)
				{
					GetSurrounding(bump_origin.hex_dev_x, bump_origin.hex_dev_y, bump_origin.hex_dev_offset, ref move_list);
				}
				MoveAll(move_list, bump_direction.down);
			}
		}
		if (PopupControl.Instance.GetButtonWasPressed())
		{
			return;
		}
		if (GamepadInput.Instance.GetMouseButtonDown())
		{
			PerkScreen.Instance.scrollwheel_releaseButtonTimeRemain = 13f;
		}
		if (PerkScreen.Instance.scrollwheel_releaseButtonTimeRemain <= 0f || !GamepadInput.Instance.GetMouseButtonUp())
		{
			return;
		}
		dev_cursor.transform.SetParent(base.transform);
		Vector3 mousePosition = GamepadInput.Instance.GetMousePosition();
		dev_cursor.transform.localPosition = new Vector3((mousePosition.x - (float)Screen.width * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor, (mousePosition.y - (float)Screen.height * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor, 0f);
		dev_cursor.transform.SetParent(PerkScreen.Instance.scrollwheel_parent.transform);
		PerkScreenHex perkScreenHex = FindNearestHexSnapPoint(dev_cursor.transform.localPosition);
		switch (curr_tool_id)
		{
		case "remove":
			perkScreenHex.type = PerkScreenHex.hex_type.none;
			perkScreenHex.extra_values = new Dictionary<string, string>();
			break;
		case "outline":
			perkScreenHex.type = PerkScreenHex.hex_type.outline;
			perkScreenHex.extra_values = new Dictionary<string, string>();
			break;
		case "place_perk":
			perkScreenHex.type = PerkScreenHex.hex_type.perk;
			perkScreenHex.extra_values = add_extra_values;
			break;
		case "add_text":
			perkScreenHex.type = PerkScreenHex.hex_type.text;
			perkScreenHex.extra_values = add_extra_values;
			break;
		case "add_lines":
			if (perkScreenHex.type != PerkScreenHex.hex_type.none)
			{
				Vector2 vector = ((Vector2)dev_cursor.transform.localPosition - (Vector2)perkScreenHex.local_position).normalized;
				float num = Vector2.Angle(Vector2.up, vector);
				if (Vector2.up.x * vector.y - Vector2.up.y * vector.x > 0f)
				{
					num = 180f - num + 180f;
				}
				string value = add_extra_values["line_perk_req"];
				int num2 = ((num <= 30f) ? 0 : ((num <= 90f) ? 1 : ((num <= 150f) ? 2 : ((num <= 210f) ? 3 : ((num <= 270f) ? 4 : ((!(num <= 330f)) ? 0 : 5))))));
				if (!perkScreenHex.extra_values.ContainsKey("line_pos" + num2 + "_clockwise_perk_req"))
				{
					perkScreenHex.extra_values.Add("line_pos" + num2 + "_clockwise_perk_req", value);
				}
				else
				{
					perkScreenHex.extra_values["line_pos" + num2 + "_clockwise_perk_req"] = value;
				}
			}
			break;
		case "bump":
			if (perkScreenHex.type != PerkScreenHex.hex_type.none)
			{
				bump_tab.SetActive(true);
				dev_buttons.SetActive(false);
				bump_origin = perkScreenHex;
				bumping = true;
			}
			break;
		}
		perkScreenHex.Redraw();
		PerkScreen.Instance.scrollwheel_releaseButtonTimeRemain = 0f;
		if (curr_tool_id == "place_perk" || curr_tool_id == "add_text" || curr_tool_id == "add_lines" || curr_tool_id == "bump")
		{
			DeselectAllToolButtons();
			curr_tool_id = "";
		}
	}

	public void PressCloseAddPerkWindow()
	{
		PopupControl.Instance.SetButtonWasPressed();
		add_perk_window.SetActive(false);
		window_input = dev_window_type_t.none;
	}

	public void PressAcceptAddPerk()
	{
		PopupControl.Instance.SetButtonWasPressed();
		string text = add_perk_input_key.text;
		switch (window_input)
		{
		case dev_window_type_t.add_lines:
			add_perk_window.SetActive(false);
			window_input = dev_window_type_t.none;
			if (PerkControl.Instance.PerkExists(text))
			{
				add_extra_values = new Dictionary<string, string>();
				add_extra_values.Add("line_perk_req", text);
				SetDevTool("add_lines");
				return;
			}
			break;
		case dev_window_type_t.add_text:
			add_perk_window.SetActive(false);
			window_input = dev_window_type_t.none;
			add_extra_values = new Dictionary<string, string>();
			add_extra_values.Add("hex_text_str", text);
			SetDevTool("add_text");
			return;
		case dev_window_type_t.place_perk:
			add_perk_window.SetActive(false);
			window_input = dev_window_type_t.none;
			if (PerkControl.Instance.PerkExists(text))
			{
				ResourceControl.Instance.AssignPerkSprite(text, perk_button_ico);
				add_extra_values = new Dictionary<string, string>();
				add_extra_values.Add("hex_perk_key", text);
				SetDevTool("place_perk");
				return;
			}
			break;
		default:
			return;
		}
		PopupControl.Instance.ShowMessage("Perk with key '" + text + "' does not exist.", PopupControl.context.message);
	}

	public void EnableDevMode()
	{
		dev_mode = true;
		dev_cursor = new GameObject("dev_cursor");
		dev_cursor.transform.SetParent(PerkScreen.Instance.scrollwheel_parent.transform);
		dev_cursor.transform.localPosition = Vector3.zero;
		dev_cursor.transform.localScale = Vector3.one;
		dev_cursor.transform.localRotation = Quaternion.identity;
		dev_buttons.SetActive(true);
		if (PerkScreen.Instance.prev_clicked_perk_nib != null)
		{
			PerkScreen.Instance.prev_clicked_perk_nib.GetComponent<Animation>().Stop();
			PerkScreen.Instance.prev_clicked_perk_nib.transform.localScale = Vector3.one;
			PerkScreen.Instance.prev_clicked_perk_nib = null;
		}
		for (int i = -50; i < 50; i++)
		{
			for (int j = -100; j < 100; j++)
			{
				string key = j + "," + i + ",0";
				if (!PerkScreen.Instance.hex_points.ContainsKey(key))
				{
					PerkScreen.Instance.hex_points.Add(key, new PerkScreenHex(PerkScreenHex.hex_type.none, new Dictionary<string, string>(), j, i, 0));
				}
				string key2 = j + "," + i + ",1";
				if (!PerkScreen.Instance.hex_points.ContainsKey(key2))
				{
					PerkScreen.Instance.hex_points.Add(key2, new PerkScreenHex(PerkScreenHex.hex_type.none, new Dictionary<string, string>(), j, i, 1));
				}
			}
		}
	}

	private float Round10(int num)
	{
		return (int)Mathf.Round((float)num / 10f) * 10;
	}

	public PerkScreenHex FindNearestHexSnapPoint(Vector3 local_location)
	{
		PerkScreenHex result = null;
		float num = float.MaxValue;
		foreach (KeyValuePair<string, PerkScreenHex> hex_point in PerkScreen.Instance.hex_points)
		{
			float num2 = Vector3.Distance(local_location, hex_point.Value.local_position);
			if (num2 < num)
			{
				num = num2;
				result = hex_point.Value;
			}
		}
		return result;
	}
}
