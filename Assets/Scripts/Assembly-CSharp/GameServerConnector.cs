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

	public GameServerSender sender => null;

	public GameServerReceiver receiver => null;

	public GameServerInterface interface_ => null;

	public bool is_host
	{
		get
		{
			return is_host_cached;
		}
		set
		{
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
	}

	public void OnConnectFailed()
	{
	}

	public void OnDisconnected(bool hard_disconnect)
	{
	}

	public void StartPinging()
	{
	}

	private void StopPinging()
	{
	}

	public void OnApplicationFocus(bool focus)
	{
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
		return null;
	}

	public void Disconnect()
	{
	}

	public void OnConnectSucceed()
	{
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
