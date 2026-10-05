using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ConsoleControl : MonoBehaviour, OrderedStart
{
	private struct nuke_element
	{
		public int innerX;

		public int innerZ;

		public ChunkElement element;
	}

	public static ConsoleControl Instance;

	public GameObject console_parent_obj;

	public GameObject main_input_obj;

	public GameObject error_notif_obj;

	public InputField input;

	public bool console_open;

	public bool show_errors;

	public Text debug_log_text;

	public GameObject debug_log_obj;

	private List<string> new_logs = new List<string>();

	private List<string> past_logs = new List<string>();

	public static float auto_sprint_speed;

	public static float dialogue_speed_mod;

	private string modify_gfx_obj = "";

	public bool god_mode_enabled;

	private int prev_command_index = -1;

	private List<string> previous_commands = new List<string>();

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
			god_mode_enabled = false;
		}
	}

	public void Start_1()
	{
		Application.logMessageReceived += HandleLog;
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		if (!show_errors)
		{
			return;
		}
		if (logString == null)
		{
			if (stackTrace != null)
			{
				LogError("<color=#ff0000>" + stackTrace + "</color>");
			}
		}
		else if (stackTrace != null)
		{
			LogError("<color=#ff0000>" + logString + " ... " + stackTrace + "</color>");
		}
		else
		{
			LogError("<color=#ff0000>" + logString + "</color>");
		}
	}

	private void LogError(string str)
	{
		if (!str.Contains("_MainTex"))
		{
			new_logs.Add(str);
		}
	}

	private void PrintLog(string str)
	{
		past_logs.Insert(0, str);
		if (past_logs.Count == 17)
		{
			past_logs.RemoveAt(16);
		}
		string text = "";
		foreach (string past_log in past_logs)
		{
			text = text + past_log + "\n";
		}
		debug_log_text.text = text;
	}

	private void FixedUpdate()
	{
		if (!show_errors || new_logs.Count == 0)
		{
			return;
		}
		foreach (string new_log in new_logs)
		{
			PrintLog(new_log);
		}
		new_logs.Clear();
	}

	private IEnumerator TrackAverageFps()
	{
		return null;
	}

	private void ModifyItemValue(string key, float mod, float default_val)
	{
	}

	public void OnPressSubmit()
	{
	}

	private void Update()
	{
		if (modify_gfx_obj != "")
		{
			if (GamepadInput.Instance.GetKeyDown(Key.Home))
			{
				ModifyItemValue("model3d_camDist", 0.1f, 2.5f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.End))
			{
				ModifyItemValue("model3d_camDist", -0.1f, 2.5f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.PageDown))
			{
				ModifyItemValue("model3d_fov", -2.5f, 60f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.PageUp))
			{
				ModifyItemValue("model3d_fov", 2.5f, 60f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.NumpadMinus))
			{
				ModifyItemValue("model3d_camHeight", 0.1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.NumpadPlus))
			{
				ModifyItemValue("model3d_camHeight", -0.1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.LeftArrow))
			{
				ModifyItemValue("model3d_xRot", -5f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.RightArrow))
			{
				ModifyItemValue("model3d_xRot", 5f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.UpArrow))
			{
				ModifyItemValue("model3d_yRot", -5f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.DownArrow))
			{
				ModifyItemValue("model3d_yRot", 5f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.Numpad6))
			{
				ModifyItemValue("model3d_recenterX", -1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.Numpad4))
			{
				ModifyItemValue("model3d_recenterX", 1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.Numpad2))
			{
				ModifyItemValue("model3d_recenterY", 1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.Numpad8))
			{
				ModifyItemValue("model3d_recenterY", -1f, 0f);
			}
			else if (GamepadInput.Instance.GetKeyDown(Key.NumpadMultiply))
			{
				ModifyItemValue("model3d_objRot", 1f, 0f);
			}
		}
		if (GamepadInput.Instance.GetKeyDown(Key.Backquote) && Application.isEditor)
		{
			PressConsoleButton();
		}
		if (GamepadInput.Instance.GetKeyDown(Key.UpArrow) && modify_gfx_obj == "" && console_open && prev_command_index + 1 < previous_commands.Count)
		{
			prev_command_index++;
			input.gameObject.SetActive(false);
			input.SetTextWithoutNotify(previous_commands[prev_command_index]);
			input.gameObject.SetActive(true);
		}
		if (GamepadInput.Instance.GetKeyDown(Key.DownArrow) && modify_gfx_obj == "" && console_open && prev_command_index - 1 >= -1)
		{
			prev_command_index--;
			if (prev_command_index == -1)
			{
				input.SetTextWithoutNotify("");
				input.ActivateInputField();
			}
			else
			{
				input.gameObject.SetActive(false);
				input.SetTextWithoutNotify(previous_commands[prev_command_index]);
				input.gameObject.SetActive(true);
			}
		}
		if (GamepadInput.Instance.GetKeyDown(Key.Enter) && console_open)
		{
			OnPressSubmit();
		}
	}

	public void GiveDevBook(string book_name)
	{
	}

	private void Save(string path, ForcedChunkData forced_chunk_data)
	{
	}

	private void CloseConsole()
	{
		console_open = false;
		console_parent_obj.SetActive(false);
	}

	public void SetLevel(int lvl)
	{
	}

	public void PressConsoleButton()
	{
		if (console_open)
		{
			CloseConsole();
			return;
		}
		console_open = true;
		console_parent_obj.SetActive(true);
		main_input_obj.SetActive(true);
		error_notif_obj.SetActive(false);
		prev_command_index = -1;
		input.SetTextWithoutNotify("");
		input.ActivateInputField();
	}

	private void SaveCustomItemMesh(Mesh mesh, ExtraInventoryData extra_data)
	{
	}

	private void SaveCustomGraphic(Texture2D canvas, ExtraInventoryData extra_data, string counter, string prefix)
	{
	}

	private void ShowNotif(string str)
	{
	}
}
