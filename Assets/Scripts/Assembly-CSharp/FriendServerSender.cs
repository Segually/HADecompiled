using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FriendServerSender : MonoBehaviour, OrderedStart
{
	public static FriendServerSender Instance;

	public static string connecting_string;

	private bool sending_request_of_some_sort;

	private DateTime request_sent_at;

	public Connection connection => FriendServerConnector.Instance.friend_server_connection;

	public FriendServerConnector connector => FriendServerConnector.Instance;

	public FriendServerReceiver receiver => FriendServerReceiver.Instance;

	public FriendServerInterface interface_ => FriendServerInterface.Instance;

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

	private void BeginTimeout()
	{
		sending_request_of_some_sort = true;
		request_sent_at = DateTime.UtcNow;
	}

	public void EndTimeout()
	{
		sending_request_of_some_sort = false;
	}

	public bool WaitingOnRequest()
	{
		return sending_request_of_some_sort && (DateTime.UtcNow - request_sent_at).TotalSeconds < 15.0;
	}

	public static void ChangeConnectingText(string str)
	{
		connecting_string = str;
		if (SceneManager.GetActiveScene().name == "Game" && WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.new_friends_list && WindowPrefabsControl.Instance.GetScreen("FRIENDS-loading") != null)
		{
			WindowPrefabsControl.Instance.GetTextLegacy("FRIENDS-loading", "ConnectText").text = connecting_string;
		}
	}

	public void SendSubmitReport()
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(46);
		outgoing.PutLong(interface_.report_data.Count);
		foreach (KeyValuePair<string, string> entry in interface_.report_data)
		{
			outgoing.PutString(entry.Key);
			outgoing.PutString(entry.Value);
		}
		string username = interface_.report_data.ContainsKey("report_username_lower") ? interface_.report_data["report_username_lower"] : "";
		string custom = interface_.report_data.ContainsKey("report_username_custom") ? interface_.report_data["report_username_custom"] : "";
		RecentlySeenPlayer recent = null;
		foreach (RecentlySeenPlayer player in receiver.recently_seen_players)
		{
			if ((player.username_lower == username && !Startup.StringNullOrWhitespace(username)) || (player.username_lower == custom && !Startup.StringNullOrWhitespace(custom)) || (player.username_punctuated == username && !Startup.StringNullOrWhitespace(username)) || (player.username_punctuated == custom && !Startup.StringNullOrWhitespace(custom)))
			{
				recent = player;
			}
		}
		outgoing.PutByte((byte)(recent == null ? 0 : 1));
		if (recent != null)
		{
			outgoing.PutShort((float)recent.game_chats.Count);
			foreach (string chat in recent.game_chats) outgoing.PutString(chat);
		}
		Friend friend = null;
		foreach (Friend entry in receiver.friends)
		{
			if (entry.username_lower == username || entry.username_lower == custom || entry.username_punctuated == username || entry.username_punctuated == custom) friend = entry;
		}
		outgoing.PutByte((byte)(friend == null ? 0 : 1));
		if (friend != null)
		{
			List<string> chats = new List<string>();
			foreach (chat_log log in friend.chat.entries)
			{
				if (!Startup.StringNullOrWhitespace(log.text_simplified_for_report)) chats.Add(log.text_simplified_for_report);
			}
			outgoing.PutShort((float)chats.Count);
			foreach (string chat in chats) outgoing.PutString(chat);
		}
		connection.Send(outgoing);
	}

	public void SendTakeTrophy(string random_trophy_id)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(56);
		outgoing.PutString(random_trophy_id);
		connection.Send(outgoing);
	}

	public void RequestServerIcon(string server_name)
	{
		receiver.requesting_server_icons.Add(server_name);
		Packet outgoing = new Packet();
		outgoing.PutByte(31);
		outgoing.PutString(server_name);
		connection.Send(outgoing, Connection.priority.LOW);
	}

	public void SendPrivateMessage(string username_lower, string message)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(26);
		outgoing.PutString(username_lower);
		outgoing.PutString(message);
		connection.Send(outgoing);
	}

	public void RequestPublicServerList()
	{
		BeginTimeout();
		Packet outgoing = new Packet();
		outgoing.PutByte(29);
		connection.Send(outgoing);
	}

	public void TryJoinPublicServer(string server_name)
	{
		BeginTimeout();
		Packet outgoing = new Packet();
		outgoing.PutByte(30);
		outgoing.PutString(server_name);
		connection.Send(outgoing);
	}

	public void AskToJoinPlayer(string player_name, byte type, string server_name = "")
	{
		BeginTimeout();
		Packet outgoing = new Packet();
		outgoing.PutByte(45);
		outgoing.PutString(player_name);
		outgoing.PutByte(type);
		if (type == 1 || type == 2) outgoing.PutString(server_name);
		connection.Send(outgoing);
	}

	public void TryAddFriend(string username_lower)
	{
		BeginTimeout();
		Packet outgoing = new Packet();
		outgoing.PutByte(16);
		outgoing.PutString(username_lower);
		connection.Send(outgoing);
	}

	public void AcceptFriendRequest(string username_lower)
	{
		BeginTimeout();
		Packet outgoing = new Packet();
		outgoing.PutByte(18);
		outgoing.PutString(username_lower);
		connection.Send(outgoing);
	}

	public void DeclineFriendRequest(string username_lower)
	{
		BeginTimeout();
		Packet outgoing = new Packet();
		outgoing.PutByte(20);
		outgoing.PutString(username_lower);
		connection.Send(outgoing);
	}

	public void InviteFriend(string username_lower)
	{
		BeginTimeout();
		Packet outgoing = new Packet();
		outgoing.PutByte(39);
		outgoing.PutString(username_lower);
		if (GameServerConnector.Instance.FullyInGame())
		{
			if (!GameServerConnector.Instance.is_host)
			{
				outgoing.PutByte(1);
				outgoing.PutString(GameServerConnector.Instance.server_name.Replace("(private)", ""));
			}
			else outgoing.PutByte(0);
		}
		else
		{
			GameServerConnector.Instance.MidConnect();
			outgoing.PutByte(0);
		}
		connection.Send(outgoing);
	}

	public void SendRemoveFriend(string remove_username)
	{
		BeginTimeout();
		Packet outgoing = new Packet();
		outgoing.PutByte(24);
		outgoing.PutString(remove_username);
		connection.Send(outgoing);
	}

	public void UpdateWorldString()
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(44);
		PackWorldString(outgoing);
		connection.Send(outgoing, Connection.priority.LOW);
	}

	public void PackWorldString(Packet outgoing)
	{
		if (SceneManager.GetActiveScene().name != "Game")
		{
			outgoing.PutByte(0);
			outgoing.PutString("");
			outgoing.PutString("");
			outgoing.PutShort((short)0);
		}
		else if (GameServerConnector.Instance.game_server_connection == null || (GameServerConnector.Instance.game_server_connection.GetStatus() != Connection.connection_status.connecting && GameServerConnector.Instance.game_server_connection.GetStatus() != Connection.connection_status.connected))
		{
			outgoing.PutByte(1);
			outgoing.PutString("");
			outgoing.PutString("");
			outgoing.PutShort((short)0);
		}
		else
		{
			outgoing.PutByte((byte)(GameServerConnector.Instance.is_host ? 2 : 3));
			outgoing.PutString(GameServerConnector.Instance.server_name);
			outgoing.PutShort((float)GameServerConnector.Instance.n_others_in_game);
		}
	}

	public void SendAttemptLogin()
	{
		ChangeConnectingText("Logging in");
		string username = PlayerData.Instance.GetGlobalString("username_lower");
		string code = PlayerData.Instance.GetGlobalString("rand_code");
		Packet outgoing = new Packet();
		outgoing.PutByte(11);
		outgoing.PutString(username);
		outgoing.PutString(code);
		connection.Send(outgoing);
	}

	public void SendMathSolution(int solution)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(7);
		outgoing.PutShort((float)solution);
		connection.Send(outgoing);
	}

	public void SignalIntent(byte intent)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(8);
		outgoing.PutByte(intent);
		connection.Send(outgoing);
	}

	public void SendWantToLogInAsPlayer()
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(1);
		outgoing.PutShort((float)FriendServerConnector.MP_VERSION);
		outgoing.PutString(FriendServerConnector.MP_VERSION_2);
		connection.Send(outgoing);
	}

	public void SendAcceptInviteFailed(string tried_to_join, byte reason)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(42);
		outgoing.PutString(tried_to_join);
		outgoing.PutByte(reason);
		connection.Send(outgoing);
	}

	public void SendYouMayJoinMyWorldNow(string tried_to_join_lower, byte type, string friend_of = "")
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(43);
		outgoing.PutString(tried_to_join_lower);
		outgoing.PutByte(type);
		if (type == 1) outgoing.PutString(friend_of);
		connection.Send(outgoing);
	}

	public void SendDispatcherPingResult(Dictionary<string, short> results)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(32);
		outgoing.PutShort((float)results.Count);
		foreach (KeyValuePair<string, short> entry in results)
		{
			outgoing.PutString(entry.Key);
			outgoing.PutShort(entry.Value);
		}
		connection.Send(outgoing);
	}

	public void SendAcceptInvite(string username_lower)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(41);
		outgoing.PutString(username_lower);
		connection.Send(outgoing);
	}

	public void SendPing()
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(15);
		connection.Send(outgoing, Connection.priority.SUPER_HIGH);
	}
}
