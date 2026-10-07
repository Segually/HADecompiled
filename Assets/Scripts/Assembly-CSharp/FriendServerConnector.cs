using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine;

public class FriendServerConnector : MonoBehaviour, OrderedStart
{
	public static FriendServerConnector Instance;

	public static int MP_VERSION = 52;

	public static string MP_VERSION_2 = "hackers_plz_chill_and_play_the_game_normally";

	public bool fully_logged_in;

	public Connection friend_server_connection;

	public DateTime last_server_ping;

	private IEnumerator ping_server;

	public FriendServerSender sender => FriendServerSender.Instance;

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
		if (friend_server_connection == null)
		{
			string globalString = PlayerData.Instance.GetGlobalString("connect_to");
			if (!Startup.StringNullOrEmpty(globalString) && globalString == "custom")
			{
				string globalString2 = PlayerData.Instance.GetGlobalString("custom_ip");
				string globalString3 = PlayerData.Instance.GetGlobalString("custom_port");
				if (Startup.StringNullOrEmpty(globalString2))
				{
					Debug.Log("<color=#ff0000>NO CUSTOM_IP SET</color>");
					friend_server_connection = new Connection("ipv4", "104.45.198.157", 7002, Connection.parent_t.FriendServerBackend, 25);
				}
				else if (Startup.StringNullOrEmpty(globalString3))
				{
					Debug.Log("<color=#ff0000>NO CUSTOM_PORT SET</color>");
					friend_server_connection = new Connection("ipv4", "104.45.198.157", 7002, Connection.parent_t.FriendServerBackend, 25);
				}
				else
				{
					int port = int.Parse(globalString3);
					friend_server_connection = new Connection("ipv4", globalString2, port, Connection.parent_t.FriendServerBackend, 25);
				}
			}
			else
			{
				friend_server_connection = new Connection("ipv4", "104.45.198.157", 7002, Connection.parent_t.FriendServerBackend, 25);
			}
		}
		if (!Startup.StringNullOrEmpty(PlayerData.Instance.GetGlobalString("username_lower")) && PlayerData.Instance.GetGlobalShort("has_friends") != 0 && PlayerData.Instance.GetGlobalShort("n_wasted_autologins") < 10)
		{
			ConnectToFriendServer();
		}
	}

	public void ConnectToFriendServer()
	{
		FriendServerSender.ChangeConnectingText("Connecting to Friend Server");
		fully_logged_in = false;
		sender.EndTimeout();
		friend_server_connection.TryConnect(this);
	}

	public void OnConnectSucceed()
	{
		FriendServerSender.ChangeConnectingText("Checking Version");
		sender.SendWantToLogInAsPlayer();
	}

	public void OnConnectFailed()
	{
		Debug.Log("Connect failed");
		if (SceneManager.GetActiveScene().name == "Game" && WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.new_friends_list && interface_.curr_screen == FriendServerInterface.friend_window_screen.connecting_screen)
		{
			interface_.ShowFailedToConnect("Unable to connect to Friend Server\nPlease try again later.", false);
		}
		fully_logged_in = false;
		sender.EndTimeout();
		StopPinging();
	}

	public void OnDisconnected(bool hard_disconnect)
	{
		if (hard_disconnect)
		{
			Debug.Log("Disconnected by Host (friend server)");
			if (fully_logged_in && SceneManager.GetActiveScene().name == "Game")
			{
				GameplayGUIControl.Instance.ShowNotif("<color=#ff0000>Lost connection to Friend Server</color>", new OnNotifClick(OnNotifClick.type.none));
			}
			else
			{
				PopupControl.Instance.ShowMessage("Lost connection to Friend Server", PopupControl.context.message);
			}
			if (SceneManager.GetActiveScene().name == "Game" && WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.new_friends_list)
			{
				WindowControl.Instance.CloseMiniwindow(true);
			}
		}
		else Debug.Log("Self Disconnected (friend server)");
		fully_logged_in = false;
		sender.EndTimeout();
		StopPinging();
	}

	public void Disconnect()
	{
		friend_server_connection.Disconnect();
	}

	public void StartPinging()
	{
		StopPinging();
		ping_server = PingServer();
		StartCoroutine(ping_server);
	}

	public void StopPinging()
	{
		if (ping_server != null) StopCoroutine(ping_server);
	}

	private IEnumerator PingServer()
	{
		last_server_ping = DateTime.UtcNow;
		yield return new WaitForSeconds(2.5f);
		while (true)
		{
			sender.SendPing();
			yield return new WaitForSeconds(5f);
			if ((DateTime.UtcNow - last_server_ping).TotalSeconds > 60.0)
			{
				friend_server_connection.Disconnect();
				yield break;
			}
		}
	}

	public void OnApplicationFocus(bool focus)
	{
		if (friend_server_connection != null)
		{
			if (friend_server_connection.GetStatus() == Connection.connection_status.connected && focus)
			{
				if (!fully_logged_in)
				{
					if (SceneManager.GetActiveScene().name != "Game" || FriendServerInterface.Instance == null) return;
					if (interface_.curr_screen != FriendServerInterface.friend_window_screen.register_screen && interface_.curr_screen != FriendServerInterface.friend_window_screen.register_attempt) return;
				}
				StartPinging();
			}
			else StopPinging();
		}
	}

	public void OnApplicationQuit()
	{
		if (friend_server_connection != null && friend_server_connection.socket != null) friend_server_connection.socket.Close();
	}

	private void FixedUpdate()
	{
		if (friend_server_connection != null)
		{
			friend_server_connection.FixedUpdate();
		}
	}

	public void TryGotoRegisterScreen()
	{
		if (SceneManager.GetActiveScene().name == "Game" && WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.new_friends_list && interface_.curr_screen == FriendServerInterface.friend_window_screen.connecting_screen)
		{
			interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.register_screen);
			StartPinging();
			return;
		}
		friend_server_connection.Disconnect();
	}

	public void CheckIfAutoLoginShouldBeDisabled()
	{
		bool has_friends = false;
		foreach (Friend friend in receiver.friends)
		{
			if (friend.status_t == Friend.status.offline || friend.status_t == Friend.status.online || friend.status_t == Friend.status.req_sent)
			{
				has_friends = true;
				break;
			}
		}
		if (PlayerData.Instance.GetGlobalShort("has_friends") == 0)
		{
			if (has_friends) PlayerData.Instance.SetGlobalShort("has_friends", 1);
		}
		else if (!has_friends) PlayerData.Instance.SetGlobalShort("has_friends", 0);
	}
}
