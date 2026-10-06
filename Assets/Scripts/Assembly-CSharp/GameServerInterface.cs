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

	public Connection connection => null;

	public GameServerConnector connector => null;

	public GameServerSender sender => null;

	public GameServerReceiver receiver => null;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
	}

	public void PressChatButton()
	{
	}

	public void PressSubmitReportObject()
	{
	}

	public void PressReportConfirmUsername()
	{
	}

	public void PressReportConfirmType(int type)
	{
	}

	public void OtherPlayerChangeCreatures(string username, List<string> parents)
	{
	}

	public void PlayerChangeEquip(string username, byte type, InventoryItem new_equip)
	{
	}

	public void GameChatReceived(chat_log new_log)
	{
	}

	public void ShowPlayerLogInOrOut(string username_punctuated, byte state)
	{
	}

	public void RemovePlayer(string username)
	{
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
	}

	public void NewPlayerNearby(Packet incoming)
	{
	}

	public void NearbyPlayerWentAway(Packet incoming)
	{
	}

	public void UnknownZoneGotoSpawn(bool send, bool on_map_change)
	{
	}

	public void StartTeleportPlayer(string username)
	{
	}

	public void EndTeleportPlayer(string username, Vector3 new_position)
	{
	}

	public GameObject GetPlayerByUsername(string username)
	{
		return null;
	}

	public void ProcessIncomingZoneData(Packet incoming, bool on_map_change)
	{
	}
}
