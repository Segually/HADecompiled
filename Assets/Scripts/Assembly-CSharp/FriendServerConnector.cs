using System;
using System.Collections;
using UnityEngine;

public class FriendServerConnector : MonoBehaviour, OrderedStart
{
	public static FriendServerConnector Instance;

	public static int MP_VERSION;

	public static string MP_VERSION_2;

	public bool fully_logged_in;

	public Connection friend_server_connection;

	public DateTime last_server_ping;

	private IEnumerator ping_server;

	public FriendServerSender sender => null;

	public FriendServerReceiver receiver => null;

	public FriendServerInterface interface_ => null;

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
	}

	public void OnConnectSucceed()
	{
	}

	public void OnConnectFailed()
	{
	}

	public void OnDisconnected(bool hard_disconnect)
	{
	}

	public void Disconnect()
	{
	}

	public void StartPinging()
	{
	}

	public void StopPinging()
	{
	}

	private IEnumerator PingServer()
	{
		return null;
	}

	public void OnApplicationFocus(bool focus)
	{
	}

	public void OnApplicationQuit()
	{
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
	}

	public void CheckIfAutoLoginShouldBeDisabled()
	{
	}
}
