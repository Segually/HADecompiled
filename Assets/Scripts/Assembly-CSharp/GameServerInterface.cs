using System.Collections.Generic;
using UnityEngine;

public class GameServerInterface : MonoBehaviour, OrderedStart
{
	public static GameServerInterface Instance;

	public GameObject prefab_online_player;

	public GameObject prefab_MP_display;

	public float overhead_y_offset;

	public ChatCollection game_chat = new ChatCollection();

	public Dictionary<string, OnlinePlayer> nearby_players = new Dictionary<string, OnlinePlayer>();

	public Connection connection => GameServerConnector.Instance.game_server_connection;

	public GameServerConnector connector => GameServerConnector.Instance;

	public GameServerSender sender => GameServerSender.Instance;

	public GameServerReceiver receiver => GameServerReceiver.Instance;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
		Vector3 zero = Camera.main.ScreenToViewportPoint(Vector2.zero);
		Vector3 offset = Camera.main.ScreenToViewportPoint(Vector2.up * 65.8f * GetComponent<Canvas>().scaleFactor);
		overhead_y_offset = offset.y - zero.y;
	}

	public void PressChatButton()
	{
		PopupControl.Instance.SetButtonWasPressed();
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.chat);
		WindowPrefabsControl.Instance.CreateScreen("CHAT", WindowPrefabsControl.build_into_t.mini_window);
		FriendServerInterface.Instance.RedrawChat(game_chat);
	}

	public void PressSubmitReportObject()
	{
		string notes = WindowPrefabsControl.Instance.GetObject("FRIENDS-report-finalize_obj", "additional_input").GetComponent<UnityEngine.UI.InputField>().text;
		if (Startup.StringNullOrWhitespace(notes))
		{
			PopupControl.Instance.ShowMessage("You must enter a description!", PopupControl.context.message);
			return;
		}
		FriendServerInterface.Instance.report_data.Add("additional_notes", notes);
		FriendServerInterface.Instance.report_data.Add("server_name", connector.server_name);
		WindowPrefabsControl.Instance.DestroyScreen("FRIENDS-report-finalize_obj");
		WindowPrefabsControl.Instance.CreateScreen("FRIENDS-report-doubleCheck-obj-self", WindowPrefabsControl.build_into_t.mini_window, WindowPrefabsControl.set_transform_t.over_top);
	}

	public void PressReportConfirmUsername()
	{
		FriendServerInterface.Instance.report_data.Add("doubleCheck_own_username", WindowPrefabsControl.Instance.GetObject("FRIENDS-report-doubleCheck-obj-self", "username_input").GetComponent<UnityEngine.UI.InputField>().text);
		WindowPrefabsControl.Instance.DestroyScreen("FRIENDS-report-doubleCheck-obj-self");
		WindowPrefabsControl.Instance.CreateScreen("FRIENDS-report-doubleCheck-obj", WindowPrefabsControl.build_into_t.mini_window, WindowPrefabsControl.set_transform_t.over_top);
	}

	public void PressReportConfirmType(int type)
	{
		switch (type)
		{
			case 0: FriendServerInterface.Instance.report_data.Add("doubleCheck_obj_type", "Painting"); break;
			case 1: FriendServerInterface.Instance.report_data.Add("doubleCheck_obj_type", "Companion or NPC"); break;
			case 2: FriendServerInterface.Instance.report_data.Add("doubleCheck_obj_type", "Statue"); break;
			case 3: FriendServerInterface.Instance.report_data.Add("doubleCheck_obj_type", "Sign of some kind"); break;
			case 4: FriendServerInterface.Instance.report_data.Add("doubleCheck_obj_type", "Teleporter"); break;
			case 5: FriendServerInterface.Instance.report_data.Add("doubleCheck_obj_type", "Other"); break;
		}
		FriendServerSender.Instance.SendSubmitReport();
		PopupControl.Instance.ShowConnecting("Submitting report");
		WindowControl.Instance.CloseMiniwindow(true);
	}

	public void OtherPlayerChangeCreatures(string username, List<string> parents)
	{
		GameObject obj = GetPlayerByUsername(username);
		if (obj == null) return;
		SharedCreature creature = obj.GetComponent<SharedCreature>();
		Object.Destroy(creature.myCreatureModel.gameObject);
		GameObject model = CreatureMorpher.Instance.GetHybridLite(parents);
		creature.myCreatureModel = model.GetComponent<LiteModel>();
		model.transform.SetParent(obj.transform.Find("model goes here"));
		model.transform.localPosition = Vector3.zero;
		model.transform.localRotation = Quaternion.identity;
		CreatureMorpher.Instance.CreateMutantParticle(model.GetComponent<LiteModel>());
		creature.OnEquipmentChanged();
	}

	public void PlayerChangeEquip(string username, byte type, InventoryItem new_equip)
	{
		GameObject obj = GetPlayerByUsername(username);
		if (obj == null) return;
		SharedCreature creature = obj.GetComponent<SharedCreature>();
		if (type == 0) creature.hat_ = new_equip;
		else if (type == 1) creature.body_ = new_equip;
		else if (type == 2) creature.hand_ = new_equip;
		creature.OnEquipmentChanged();
	}

	public void GameChatReceived(chat_log new_log)
	{
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.chat) FriendServerInterface.Instance.RedrawChat(game_chat);
		GameplayGUIControl.Instance.ShowNotif(new_log.text, new_log.img, new OnNotifClick(OnNotifClick.type.none));
	}

	public void ShowPlayerLogInOrOut(string username_punctuated, byte state)
	{
		string text = state == 0 ? "<color=#ff4f4f>" + username_punctuated + "</color> left the game" : state == 1 ? "<color=#00ff00>" + username_punctuated + "</color> joined the game" : "";
		chat_log log = new chat_log(text, "", null, false, null);
		game_chat.AddLog(log);
		GameChatReceived(log);
	}

	public void RemovePlayer(string username)
	{
		if (!nearby_players.ContainsKey(username)) return;
		if (nearby_players[username].obj != null) Destroy(nearby_players[username].obj);
		nearby_players.Remove(username);
	}

	public bool AnyoneUsing(string obj_str)
	{
		if (GameServerConnector.Instance.game_server_connection != null && GameServerConnector.Instance.game_server_connection.GetStatus() == Connection.connection_status.connected && GameServerConnector.Instance.completely_logged_in)
		{
			foreach (KeyValuePair<string, OnlinePlayer> nearby_player in nearby_players)
			{
				if (nearby_player.Value.currently_using == obj_str)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CreateMovementSmoother(GameObject for_obj, Vector3 start_pos, Vector3 end_pos)
	{
		SharedCreature creature = for_obj.GetComponent<SharedCreature>();
		if (creature.movement_smoother != null) Destroy(creature.movement_smoother);
		if (end_pos == Vector3.zero) return;
		GameObject smoother = new GameObject("Movement Smoother");
		smoother.AddComponent<MovementSmoother>();
		creature.movement_smoother = smoother;
		smoother.GetComponent<MovementSmoother>().obj_to_smooth = for_obj;
		smoother.transform.position = start_pos;
	}

	public void NewPlayerNearby(Packet incoming)
	{
		string lower = incoming.GetString();
		string punctuated = incoming.GetString();
		OnlinePlayerData stats = new OnlinePlayerData();
		stats.Unpack(incoming);
		if (nearby_players.ContainsKey(lower))
		{
			Debug.Log("NEABY_PLAYERS ALREADY CONTAINS " + lower);
			return;
		}
		OnlinePlayer player = new OnlinePlayer(lower, punctuated, stats);
		nearby_players.Add(lower, player);
		if (!stats.is_dead) MobControl.Instance.SpawnOtherPlayer(player, stats);
		sender.SendAllPreAppliedPerks(lower, "");
		FriendServerReceiver.Instance.AddToRecentlySeenPlayers(lower, punctuated, "");
	}

	public void NearbyPlayerWentAway(Packet incoming)
	{
		string username = incoming.GetString();
		int count = incoming.GetByte();
		for (int i = 0; i < count; i++)
		{
			string mob = incoming.GetString();
			if (MobControl.Instance.active_combatants.ContainsKey(mob) && !MobControl.Instance.my_claimed_creatures.Contains(mob)) Destroy(MobControl.Instance.active_combatants[mob]);
		}
		RemovePlayer(username);
	}

	public void UnknownZoneGotoSpawn(bool send, bool on_map_change)
	{
		if ((int)BreedControl.Instance.state_t != 0) return;
		ZoneDataControl.Instance.ChangeZone(ZoneDataControl.Instance.LoadOverworld(), (ZoneDataControl.change_zone_type)3, BreedControl.Instance.campos_result.position, null, send, on_map_change);
		GameController.Instance.SnapCam(0.65f);
		if (TransitionControl.Instance.is_transition_playing) TransitionControl.Instance.StartFadeBackInSilent();
	}

	public void StartTeleportPlayer(string username)
	{
		GameObject obj = GetPlayerByUsername(username);
		if (obj == null) return;
		CustomTeleporterControl.Instance.VisuallyTeleportSomeone(obj);
	}

	public void EndTeleportPlayer(string username, Vector3 new_position)
	{
		GameObject obj = GetPlayerByUsername(username);
		if (obj == null) return;
		CustomTeleporterControl.Instance.EndTeleportAnimation(obj);
		obj.transform.position = new_position;
		obj.GetComponent<SharedCreature>().SetMoveTo(new_position);
		if (obj.GetComponent<SharedCreature>().movement_smoother != null) Object.Destroy(obj.GetComponent<SharedCreature>().movement_smoother);
	}

	public GameObject GetPlayerByUsername(string username)
	{
		return nearby_players.ContainsKey(username) ? nearby_players[username].obj : null;
	}

	public void ProcessIncomingZoneData(Packet incoming, bool on_map_change)
	{
		if ((int)BreedControl.Instance.state_t != 0) return;
		string zone = incoming.GetString();
		ZoneData data = ZoneData.UnpackFromWeb(incoming, zone);
		byte type = incoming.GetByte();
		if (type == 0) ZoneDataControl.Instance.ChangeZone(data, (ZoneDataControl.change_zone_type)0, TransitionControl.Instance.StartFadeBackInSilent, true, on_map_change);
		else if (type == 1) ZoneDataControl.Instance.ChangeZone(data, (ZoneDataControl.change_zone_type)1, TransitionControl.Instance.StartFadeBackInWithDoorSound, true, on_map_change);
		else if (type == 2) ZoneDataControl.Instance.ChangeZone(data, (ZoneDataControl.change_zone_type)2, receiver.UnpackPosition(incoming), TransitionControl.Instance.StartFadeBackInSilent, true, on_map_change);
		else if (type == 3) ZoneDataControl.Instance.ChangeZone(data, (ZoneDataControl.change_zone_type)3, receiver.UnpackPosition(incoming), null, true, on_map_change);
		GameController.Instance.SnapCam(0.65f);
	}
}
