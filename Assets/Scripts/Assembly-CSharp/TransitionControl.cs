using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class TransitionControl : MonoBehaviour, OrderedStart
{
	public enum transition_type
	{
		none = 0,
		on_teleport = 1,
		on_exit_house = 2,
		on_enter_house = 3,
		on_quest_progression = 4
	}

	public static TransitionControl Instance;

	public static transition_type transition_type_t;

	private bool initial_loading_screen;

	public bool is_transition_playing;

	public string zone_entering;

	public string quest_transition_name;

	public int quest_transition_step;

	private IEnumerator fade_back_in_coroutine;

	public void Start_0()
	{
		Instance = this;
		HideSplash();
	}

	public void Start_1()
	{
	}

	public void BeginExitHouseTransition()
	{
	}

	public void BeginEnterHouseTransition(string zone_entering)
	{
	}

	public void BeginTeleportTransition()
	{
	}

	public void BeginQuestProgressionTransition(string quest_transition_name, int quest_transition_step)
	{
	}

	public void ShowSplash(bool whoosh_on_complete)
	{
		GetComponent<Image>().enabled = true;
		GetComponent<CanvasGroup>().alpha = 1f;
		initial_loading_screen = whoosh_on_complete;
	}

	public void HideSplash()
	{
		GetComponent<Image>().enabled = false;
		GetComponent<CanvasGroup>().alpha = 0f;
	}

	public void FadeToBlackComplete()
	{
	}

	public void StartFadeBackInWithDoorSound()
	{
	}

	public void StartFadeBackInSilent()
	{
		if (fade_back_in_coroutine != null)
		{
			StopCoroutine(fade_back_in_coroutine);
		}
		fade_back_in_coroutine = FadeBackInCoroutine(false);
		StartCoroutine(fade_back_in_coroutine);
	}

	private IEnumerator FadeBackInCoroutine(bool door_close_sound)
	{
		float timer = 0f;
		while (true)
		{
			if (MapEditorControl.Instance.editing_map)
			{
				yield return new WaitForSeconds(0.1f);
				break;
			}
			if (ChunkControl.Instance.ChunksUntilEndTransition() == 0)
			{
				break;
			}
			float step = 0.1f;
			yield return new WaitForSeconds(0.1f);
			timer += step;
			if (timer > 5f)
			{
				Debug.Log("LOADING SLOW : max transition time reached");
				break;
			}
		}
		EndTransitionNow(door_close_sound);
	}

	public void EndTransitionNow(bool door_close_sound)
	{
		GetComponent<Animation>().Stop();
		GetComponent<Animation>().Play("fade_backin");
		if (door_close_sound)
		{
			AudioControl.Instance.Play(AudioControl.Instance.sfx_closedoor);
		}
		if (!QuestControl.Instance.doing_time_trial)
		{
			GameplayGUIControl.Instance.ShowGameplayGui();
			GameController.Instance.UNPAUSE_GAME();
		}
		else
		{
			GameplayGUIControl.Instance.HideAllNotifs();
		}
		if (!initial_loading_screen)
		{
			GameController.Instance.SnapCam(0.65f);
			return;
		}
		initial_loading_screen = false;
		BreedControl.Instance.PlayBreedWhoosh();
		GameController.Instance.SnapCam(0f);
		PopupControl.Instance.HideAll();
		FriendServerInterface.Instance.ShowNumFriendsOnline();
		FriendServerInterface.Instance.ShowNewGiftsNotif();
	}

	public void TransitionComplete()
	{
	}
}
