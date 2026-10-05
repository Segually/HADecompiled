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
	}

	private void ClearEntryText()
	{
	}

	public void OnIncorrectPassword()
	{
	}

	public void OnCorrectPassword()
	{
	}

	public void OpenLockScreen(string header_text, lock_context curr_lock_context)
	{
	}

	public void OnClose()
	{
	}
}
