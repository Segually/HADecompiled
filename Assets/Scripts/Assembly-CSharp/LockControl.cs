using UnityEngine;

public class LockControl : MonoBehaviour, OrderedStart
{
	public enum lock_context
	{
		unknown = 0,
		create_password = 1,
		unlock_container = 2,
		unlock_house = 3,
		unlock_musicbox = 4
	}

	public static LockControl Instance;

	public int lock_iterator;

	public lock_context curr_lockscreen_context;

	private int incorrect_unlocks_this_session;

	public bool lock_screen_open;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public void PressKeypadButton(int input)
	{
		AudioControl.Instance.PlayPitch(GameController.Instance.sfx_typewriter, UnityEngine.Random.Range(0.6f, 0.9f), 0.5f);
		if (input == -1)
		{
			ClearEntryText();
		}
		else
		{
			if (lock_iterator >= 5)
			{
				return;
			}
			WindowPrefabsControl.Instance.GetTextLegacy("Locked_screen", "entry" + lock_iterator).text = input.ToString() ?? "";
			lock_iterator++;
			if (lock_iterator != 5)
			{
				return;
			}
			string text = "";
			for (int i = 0; i < 5; i++)
			{
				text += WindowPrefabsControl.Instance.GetTextLegacy("Locked_screen", "entry" + i).text;
			}
			if (curr_lockscreen_context == lock_context.create_password)
			{
				lock_screen_open = false;
				WindowControl.Instance.CloseMiniwindow(true);
				PopupControl.Instance.ShowMessage(GameController.Instance.interacting_element_item.item_name + " locked with password : <color=#00ff00>" + text + "</color>");
				ExtraInventoryData extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
				extraDataCopy.SetString("password", text);
				ConstructionControl.Instance.PlayerReplaceInteracting(new InventoryItem(GameController.Instance.interacting_element_item.item_name, extraDataCopy), true);
			}
			else if ((!GameServerConnector.Instance.FullyInGame() || incorrect_unlocks_this_session < 20) && GameController.Instance.interacting_element_item.GetString("password") == text)
			{
				OnCorrectPassword();
			}
			else
			{
				OnIncorrectPassword();
			}
		}
	}

	private void ClearEntryText()
	{
		lock_iterator = 0;
		for (int i = 0; i < 5; i++)
		{
			WindowPrefabsControl.Instance.GetTextLegacy("Locked_screen", "entry" + i).text = "";
		}
	}

	public void OnIncorrectPassword()
	{
		incorrect_unlocks_this_session++;
		PopupControl.Instance.ShowMessage("<color=#ff0000>Password incorrect</color>");
		AudioControl.Instance.PlayPitch(GameController.Instance.sfx_wrongpass, 0.8f, 0.31f);
		ClearEntryText();
	}

	public void OnCorrectPassword()
	{
		WindowPrefabsControl.Instance.DestroyScreen("Locked_screen");
		AudioControl.Instance.PlayPitch(GameController.Instance.sfx_chestopen, 1f, 0.5f);
		switch (curr_lockscreen_context)
		{
		case lock_context.unlock_musicbox:
			MusicBoxControl.Instance.OpenMusicBox(GameController.Instance.interacting_element_item, new Vector3((float)(GameController.Instance.interacting_element_innerX + GameController.Instance.interacting_element_chunkX * 10) + 0.5f, 0f, (float)(GameController.Instance.interacting_element_innerZ + GameController.Instance.interacting_element_chunkZ * 10) + 0.5f));
			break;
		case lock_context.unlock_house:
			WindowControl.Instance.CloseMiniwindow(true);
			TransitionControl.Instance.BeginEnterHouseTransition("shack" + GameController.Instance.interacting_element_item.GetLong("shack_id"));
			break;
		case lock_context.unlock_container:
			inventory_ctr.Instance.TryOpenWorldContainer(GameController.Instance.interacting_element_item, GameController.Instance.interacting_element_rot, GameController.Instance.interacting_element_innerX, GameController.Instance.interacting_element_innerZ, GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ);
			break;
		}
		lock_screen_open = false;
	}

	public void OpenLockScreen(string header_text, lock_context curr_lock_context)
	{
		curr_lockscreen_context = curr_lock_context;
		incorrect_unlocks_this_session = 0;
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.lock_screen);
		WindowControl.Instance.HideMiniwindowHeaders();
		WindowPrefabsControl.Instance.CreateScreen("Locked_screen", WindowPrefabsControl.build_into_t.mini_window);
		lock_iterator = 0;
		WindowPrefabsControl.Instance.GetTextLegacy("Locked_screen", "header").text = header_text;
		lock_screen_open = true;
	}

	public void OnClose()
	{
		if (lock_screen_open && curr_lockscreen_context == lock_context.create_password)
		{
			inventory_ctr.Instance.GiveItem("Lock", 1, "", false);
		}
		WindowPrefabsControl.Instance.DestroyScreen("Locked_screen");
		lock_screen_open = false;
		if (GameServerConnector.Instance.FullyInGame())
		{
			GameServerSender.Instance.SendReleaseInteractingObject();
		}
	}
}
