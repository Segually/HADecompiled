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

	public string zone_entering = "";

	public string quest_transition_name = "";

	public int quest_transition_step = -1;

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
		if (ZoneDataControl.Instance.curr_zonedata.outer_item_zone != "overworld")
		{
			zone_entering = ZoneDataControl.Instance.curr_zonedata.outer_item_zone;
		}
		GameplayGUIControl.Instance.HideGameplayGui();
		GameController.Instance.PAUSE_GAME();
		transition_type_t = transition_type.on_exit_house;
		ShowSplash(false);
		GetComponent<Animation>().Play("fade_out");
		is_transition_playing = true;
	}

	public void BeginEnterHouseTransition(string zone_entering)
	{
		this.zone_entering = zone_entering;
		AudioControl.Instance.Play(AudioControl.Instance.sfx_opendoor);
		GameplayGUIControl.Instance.HideGameplayGui();
		GameController.Instance.PAUSE_GAME();
		transition_type_t = transition_type.on_enter_house;
		ShowSplash(false);
		GetComponent<Animation>().Play("fade_out");
		is_transition_playing = true;
	}

	public void BeginTeleportTransition()
	{
		transition_type_t = transition_type.on_teleport;
		ShowSplash(false);
		GetComponent<Animation>().Play("fade_out");
		is_transition_playing = true;
	}

	public void BeginQuestProgressionTransition(string quest_transition_name, int quest_transition_step)
	{
		this.quest_transition_name = quest_transition_name;
		this.quest_transition_step = quest_transition_step;
		if (WindowControl.Instance.curr_window == WindowControl.window_type_t.dialogue)
		{
			DialogueControl.Instance.CloseWithIntentionOfMiniwindow(true);
		}
		else
		{
			WindowControl.Instance.CloseMiniwindow(false);
			GameplayGUIControl.Instance.HideGameplayGui();
			GameController.Instance.PAUSE_GAME();
		}
		if (GameController.Instance.player != null)
		{
			GameController.Instance.player.GetComponent<CreatureBrainLocalPlayer>().StopEverything();
		}
		transition_type_t = transition_type.on_quest_progression;
		ShowSplash(false);
		GetComponent<Animation>().Play("fade_out");
		is_transition_playing = true;
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
		switch (transition_type_t)
		{
		case transition_type.on_teleport:
		{
			CompanionController.Instance.RecreateAllCompanions();
			ZoneData new_zone_data3;
			if (CustomTeleporterControl.Instance.click_teleport_zone_to == "overworld")
			{
				new_zone_data3 = ZoneDataControl.Instance.LoadOverworld();
			}
			else
			{
				if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
				{
					GameServerSender.Instance.RequestZoneData(CustomTeleporterControl.Instance.click_teleport_zone_to, ZoneDataControl.change_zone_type.custom_position, CustomTeleporterControl.Instance.ClickedTeleposToVec3());
					break;
				}
				new_zone_data3 = ZoneDataControl.Instance.LoadZoneDataFromDisk(CustomTeleporterControl.Instance.click_teleport_zone_to);
			}
			ZoneDataControl.Instance.ChangeZone(new_zone_data3, ZoneDataControl.change_zone_type.custom_position, CustomTeleporterControl.Instance.ClickedTeleposToVec3(), StartFadeBackInSilent, true, false);
			break;
		}
		case transition_type.on_exit_house:
		{
			ZoneData new_zone_data;
			if (ZoneDataControl.Instance.curr_zonedata.outer_item_zone == "overworld")
			{
				new_zone_data = ZoneDataControl.Instance.LoadOverworld();
			}
			else
			{
				if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
				{
					GameServerSender.Instance.RequestZoneData(ZoneDataControl.Instance.curr_zonedata.outer_item_zone, ZoneDataControl.change_zone_type.place_at_exit);
					break;
				}
				new_zone_data = ZoneDataControl.Instance.LoadZoneDataFromDisk(ZoneDataControl.Instance.curr_zonedata.outer_item_zone);
			}
			ZoneDataControl.Instance.ChangeZone(new_zone_data, ZoneDataControl.change_zone_type.place_at_exit, StartFadeBackInWithDoorSound, true, false);
			break;
		}
		case transition_type.on_enter_house:
			if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
			{
				GameServerSender.Instance.RequestZoneData(zone_entering, ZoneDataControl.change_zone_type.place_at_entrance);
			}
			else
			{
				ZoneDataControl.Instance.ChangeZone(ZoneDataControl.Instance.LoadZoneDataFromDisk(zone_entering), ZoneDataControl.change_zone_type.place_at_entrance, StartFadeBackInSilent, true, false);
			}
			break;
		case transition_type.on_quest_progression:
		{
			QuestControl.Instance.SetQuestProgress(quest_transition_name, quest_transition_step, false, true);
			CompanionController.Instance.RecreateAllCompanions();
			QuestControl.parsed_position parsed_position = QuestControl.Instance.ParseQuestPositions(quest_transition_name, quest_transition_step, "Instantly teleport to")[0];
			Vector3 vector = new Vector3((float)(parsed_position.chunkX * 10) + (float)parsed_position.innerX + 0.5f, 0f, (float)(parsed_position.chunkZ * 10) + (float)parsed_position.innerZ + 0.5f);
			if (parsed_position.zone == "overworld")
			{
				ZoneDataControl.Instance.ChangeZone(ZoneDataControl.Instance.LoadOverworld(), ZoneDataControl.change_zone_type.custom_position, vector, StartFadeBackInSilent, true, false);
			}
			else if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
			{
				GameServerSender.Instance.RequestZoneData(parsed_position.zone, ZoneDataControl.change_zone_type.custom_position, vector);
			}
			else
			{
				ZoneDataControl.Instance.ChangeZone(ZoneDataControl.Instance.LoadZoneDataFromDisk(parsed_position.zone), ZoneDataControl.change_zone_type.custom_position, vector, StartFadeBackInSilent, true, false);
			}
			quest_transition_name = "";
			quest_transition_step = -1;
			break;
		}
		}
		transition_type_t = transition_type.none;
	}

	public void StartFadeBackInWithDoorSound()
	{
		if (fade_back_in_coroutine != null)
		{
			StopCoroutine(fade_back_in_coroutine);
		}
		fade_back_in_coroutine = FadeBackInCoroutine(true);
		StartCoroutine(fade_back_in_coroutine);
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
		GetComponent<Animation>().Stop();
		HideSplash();
		is_transition_playing = false;
	}
}
