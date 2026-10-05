using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GamepadInput : MonoBehaviour, OrderedStart
{
	public static GamepadInput Instance;

	private Gamepad gamepad;

	private bool ignore_gamepad = true;

	private bool ignore_keyboard = true;

	private float x_in;

	private float y_in;

	private bool A_down;

	private bool B_down;

	private Vector3 right_vec;

	private Vector3 up_vec;

	public bool gamepad_in_use;

	private string scene = "Menu";

	private Vector2 prev_mouse_position = Vector2.zero;

	private int interactable_checker_timer;

	private GameObject last_interactable;

	public void Start_0()
	{
		if (!(Instance == null))
		{
			return;
		}
		Instance = this;
		up_vec = new Vector3(0f - (float)Math.Sqrt(0.5), 0f, (float)Math.Sqrt(0.5));
		right_vec = new Vector3((float)Math.Sqrt(0.5), 0f, (float)Math.Sqrt(0.5));
		SceneManager.activeSceneChanged += ChangedActiveScene;
		if (Application.isEditor)
		{
			ignore_gamepad = false;
			ignore_keyboard = false;
		}
		else if (ignore_gamepad)
		{
			return;
		}
		TryGetGamepad();
		InputSystem.onDeviceChange += delegate
		{
			if (Gamepad.all.Count == 0)
			{
				gamepad = null;
			}
			else
			{
				gamepad = Gamepad.all[0];
			}
		};
	}

	public void Start_1()
	{
	}

	private void TryGetGamepad()
	{
		if (Gamepad.all.Count == 0)
		{
			gamepad = null;
		}
		else
		{
			gamepad = Gamepad.all[0];
		}
	}

	private void ChangedActiveScene(Scene current, Scene next)
	{
		scene = next.name;
	}

	public Vector3 GetMousePosition()
	{
		if (Pointer.current == null)
		{
			return Vector3.zero;
		}
		return Pointer.current.position.ReadValue();
	}

	public bool GetMouseButton()
	{
		if (Pointer.current == null)
		{
			return false;
		}
		return Pointer.current.press.isPressed;
	}

	public bool GetMouseButtonDown()
	{
		if (Pointer.current == null)
		{
			return false;
		}
		return Pointer.current.press.wasPressedThisFrame;
	}

	public bool GetMouseButtonUp()
	{
		if (Pointer.current == null)
		{
			return false;
		}
		return Pointer.current.press.wasReleasedThisFrame;
	}

	public float GetMouseScrollWheel()
	{
		if (Mouse.current == null)
		{
			return 0f;
		}
		return Mouse.current.scroll.ReadValue().y;
	}

	public bool GetKeyDown(Key key)
	{
		if (ignore_keyboard)
		{
			return false;
		}
		if (Keyboard.current == null)
		{
			return false;
		}
		return Keyboard.current[key].wasPressedThisFrame;
	}

	public bool GetKeyUp(Key key)
	{
		if (ignore_keyboard)
		{
			return false;
		}
		if (Keyboard.current == null)
		{
			return false;
		}
		return Keyboard.current[key].wasReleasedThisFrame;
	}

	public bool GetKey(Key key)
	{
		if (ignore_keyboard)
		{
			return false;
		}
		if (Keyboard.current == null)
		{
			return false;
		}
		return Keyboard.current[key].isPressed;
	}

	private void Update()
	{
		if (ignore_gamepad)
		{
			return;
		}
		string message;
		if (!gamepad_in_use)
		{
			if (gamepad == null)
			{
				return;
			}
			if (gamepad.leftStick.ReadValue().magnitude != 0f)
			{
				gamepad_in_use = true;
			}
			if (gamepad.buttonSouth.wasPressedThisFrame)
			{
				gamepad_in_use = true;
			}
			if (gamepad.buttonEast.wasPressedThisFrame)
			{
				gamepad_in_use = true;
			}
			else if (!gamepad_in_use)
			{
				return;
			}
			message = "<color=#00ff00>Game Pad Controls Enabled</color>";
			if (scene == "Game")
			{
				GameplayGUIControl.Instance.ShowNotif(message, null, new OnNotifClick(OnNotifClick.type.none));
				if (GameController.Instance.targetShowing)
				{
					GameController.Instance.HideTargetCircle();
				}
				return;
			}
		}
		else
		{
			if (!GetMouseButton())
			{
				if (gamepad_in_use)
				{
					if (!(gamepad == null))
					{
						Vector2 vector = gamepad.leftStick.ReadValue();
						x_in = vector.x;
						y_in = vector.y;
						if (gamepad.buttonSouth.wasPressedThisFrame)
						{
							A_down = true;
						}
						if (gamepad.buttonEast.wasPressedThisFrame)
						{
							B_down = true;
						}
					}
					return;
				}
			}
			else
			{
				gamepad_in_use = false;
			}
			message = "<color=#aaaaaa>Game Pad Controls Disabled</color>";
			if (scene == "Game")
			{
				GameplayGUIControl.Instance.ShowNotif(message, null, new OnNotifClick(OnNotifClick.type.none));
				return;
			}
		}
		PopupControl.Instance.ShowMessage(message);
	}

	private void FixedUpdate()
	{
		if (ignore_gamepad)
		{
			return;
		}
		if (gamepad_in_use)
		{
			if (!PopupControl.Instance.popup_open)
			{
				if (scene == "Game")
				{
					if (WindowControl.Instance.curr_miniwindow != 0)
					{
						if (B_down)
						{
							WindowControl.Instance.CloseMiniwindow(true);
						}
					}
					else if (WindowControl.Instance.curr_window == (WindowControl.window_type_t)0)
					{
						GameplayInput();
					}
					else
					{
						if (WindowControl.Instance.curr_window == (WindowControl.window_type_t)1 && A_down && WindowPrefabsControl.Instance.GetScreen("Dialogue - next") != null)
						{
							DialogueControl.Instance.PressNext();
						}
						if (B_down)
						{
							WindowControl.Instance.PressClose();
						}
					}
				}
			}
			else if (A_down || B_down)
			{
				PopupControl.Instance.PressOkay();
			}
		}
		A_down = false;
		B_down = false;
	}

	private void GameplayInput()
	{
		if (GameController.Instance.player == null)
		{
			return;
		}
		if (x_in == 0f && y_in == 0f)
		{
			GameController.Instance.player.GetComponent<CreatureBrainLocalPlayer>().StopEverything();
		}
		else
		{
			Vector3 normalized = (right_vec * x_in + up_vec * y_in).normalized;
			GameController.Instance.player.GetComponent<CreatureBrainLocalPlayer>().SelectMovePosition(GameController.Instance.player.transform.position + normalized * 5f);
			if (interactable_checker_timer == 0)
			{
				GameObject closestInteractable = GameController.Instance.GetClosestInteractable(GameController.Instance.player.transform.position + normalized, 0.75f);
				if (closestInteractable != last_interactable)
				{
					if (closestInteractable == null)
					{
						GameController.Instance.HideTargetCircle();
					}
					else
					{
						Interactable component = closestInteractable.GetComponent<Interactable>();
						bool flag = component != null;
						GameController.Instance.ShowYellowCircle(closestInteractable.transform.position, flag ? component.circle_size : 1.1f);
						GameController.Instance.BlobbleTargetCircle();
					}
				}
				last_interactable = closestInteractable;
				interactable_checker_timer = 5;
			}
			interactable_checker_timer--;
		}
		if (A_down && last_interactable != null)
		{
			GameController.Instance.player_interact(last_interactable);
			GameController.Instance.HideTargetCircle();
			GameController.Instance.player.GetComponent<CreatureBrainLocalPlayer>().StopEverything();
		}
	}
}
