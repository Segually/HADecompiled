using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameServerConnector : MonoBehaviour, OrderedStart
{
	public static GameServerConnector Instance;

	public Connection game_server_connection;

	public Connection switch_away_connection;

	public bool dont_goto_menu_on_connect;

	public string server_name = "";

	public bool completely_logged_in;

	public bool pvp_enabled;

	public int n_others_in_game;

	public bool is_moderator;

	private bool is_host_cached;

	public DateTime slow_load_chunks_begin;

	public string disconnected_string;

	public DateTime last_server_ping;

	private IEnumerator ping_server;

	private string random_join_code = "";

	public GameServerSender sender => GameServerSender.Instance;

	public GameServerReceiver receiver => GameServerReceiver.Instance;

	public GameServerInterface interface_ => GameServerInterface.Instance;

	public bool is_host
	{
		get
		{
			return is_host_cached;
		}
		set
		{
			is_host_cached = value;
		}
	}

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

	public bool FullyInGame()
	{
		if (game_server_connection != null && game_server_connection.GetStatus() == Connection.connection_status.connected && completely_logged_in)
		{
			return true;
		}
		return false;
	}

	public bool MidConnect()
	{
		if (game_server_connection != null)
		{
			Connection.connection_status status = game_server_connection.GetStatus();
			if (status == Connection.connection_status.connecting)
			{
				return true;
			}
			if (status == Connection.connection_status.connected && !completely_logged_in)
			{
				return true;
			}
		}
		return false;
	}

	public bool ShouldSaveLocally()
	{
		if (DevBuildControl.Instance.debug_bandit_camp_data != null)
		{
			return false;
		}
		if (game_server_connection != null && game_server_connection.GetStatus() == Connection.connection_status.connected && completely_logged_in)
		{
			return is_host_cached;
		}
		return !MidConnect();
	}

	public void ConnectToGameServer(string ip_address, string ip_address_type, int port, string random_join_code)
	{
		bool save = true;
		if (game_server_connection != null && (game_server_connection.GetStatus() == Connection.connection_status.connecting || game_server_connection.GetStatus() == Connection.connection_status.connected))
		{
			save = is_host_cached;
			dont_goto_menu_on_connect = true;
			game_server_connection.Disconnect();
			switch_away_connection = game_server_connection;
		}
		if (save) ChunkControl.Instance.SaveAllLandClaimChunkTimersWithoutDestroying();
		this.random_join_code = random_join_code;
		completely_logged_in = false;
		server_name = "";
		game_server_connection = new Connection(ip_address_type, ip_address, port, Connection.parent_t.GameServerBackend, 150);
		game_server_connection.TryConnect(this);
	}

	public void OnConnectFailed()
	{
		if (SceneManager.GetActiveScene().name == "Game")
		{
			PopupControl.Instance.ShowMessage("Unable to connect to Game Server\nConnection failed", (PopupControl.context)1);
			if (FriendServerInterface.Instance.curr_screen == FriendServerInterface.friend_window_screen.connecting_screen) WindowControl.Instance.CloseMiniwindow(true);
		}
		if (dont_goto_menu_on_connect)
		{
			GameController.Instance.GoToMenu(false);
			dont_goto_menu_on_connect = false;
		}
		completely_logged_in = false;
		server_name = "";
		StopPinging();
	}

	public void OnDisconnected(bool hard_disconnect)
	{
		if (hard_disconnect) disconnected_string = "Lost connection to Host";
		if (SceneManager.GetActiveScene().name == "Game")
		{
			if (!dont_goto_menu_on_connect) GameController.Instance.GoToMenu(true);
			else dont_goto_menu_on_connect = false;
		}
		completely_logged_in = false;
		server_name = "";
		StopPinging();
	}

	public void StartPinging()
	{
		StopPinging();
		ping_server = PingServer();
		StartCoroutine(ping_server);
	}

	private void StopPinging()
	{
		if (ping_server != null) StopCoroutine(ping_server);
	}

	public void OnApplicationFocus(bool focus)
	{
		if (game_server_connection == null) return;
		if (game_server_connection.GetStatus() == Connection.connection_status.connected && focus && SceneManager.GetActiveScene().name == "Game" && completely_logged_in) StartPinging();
		else StopPinging();
	}

	public void OnApplicationQuit()
	{
		if (game_server_connection != null && game_server_connection.socket != null)
		{
			game_server_connection.socket.Close();
		}
		if (PlayerData.Instance != null)
		{
			PlayerData.Instance.SaveAllOnApplicationQuit();
		}
		if (!(SceneManager.GetActiveScene().name == "Game"))
		{
			return;
		}
		if (FullyInGame())
		{
			if (!is_host_cached)
			{
				return;
			}
		}
		else if (MidConnect())
		{
			return;
		}
		if (ChunkControl.Instance != null)
		{
			ChunkControl.Instance.SaveAllLandClaimChunkTimersWithoutDestroying();
		}
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
				Disconnect();
				yield break;
			}
		}
	}

	public void Disconnect()
	{
		if (game_server_connection != null) game_server_connection.Disconnect();
	}

	public void OnConnectSucceed()
	{
		dont_goto_menu_on_connect = false;
		interface_.game_chat = new ChatCollection();
		PopupControl.Instance.HideAll();
		FriendServerSender.ChangeConnectingText("Logging in");
		sender.SendLoginAttempt(random_join_code);
	}

	public void FixedUpdate()
	{
		if (game_server_connection != null)
		{
			game_server_connection.FixedUpdate();
		}
		if (switch_away_connection != null)
		{
			switch_away_connection.FixedUpdate();
			if (switch_away_connection.GetStatus() == Connection.connection_status.not_connected)
			{
				switch_away_connection = null;
			}
		}
	}
}
