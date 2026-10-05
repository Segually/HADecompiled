using System;
using System.Collections.Generic;
using UnityEngine;

public class PingController : MonoBehaviour, OrderedStart
{
	public static PingController Instance;

	public Dictionary<string, PingInProgress> pings_in_progress = new Dictionary<string, PingInProgress>();

	public Dictionary<string, short> previous_results = new Dictionary<string, short>();

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

	public void PingMany(List<ToPing> to_ping, Action<Dictionary<string, short>> on_complete, MonoBehaviour caller)
	{
		ManyPingInProgress manyPingInProgress = new ManyPingInProgress(on_complete);
		foreach (ToPing item in to_ping)
		{
			if (previous_results.ContainsKey(item.dispatcher_name))
			{
				manyPingInProgress.results.Add(item.dispatcher_name, previous_results[item.dispatcher_name]);
				continue;
			}
			if (pings_in_progress.ContainsKey(item.dispatcher_name))
			{
				manyPingInProgress.waiting_on.Add(item.dispatcher_name);
				pings_in_progress[item.dispatcher_name].on_complete.Add(manyPingInProgress.OnSingleComplete);
				continue;
			}
			manyPingInProgress.waiting_on.Add(item.dispatcher_name);
			PingInProgress pingInProgress = new PingInProgress(item.dispatcher_name, item.dispatcher_ip, item.dispatcher_port);
			pings_in_progress.Add(item.dispatcher_name, pingInProgress);
			pings_in_progress[item.dispatcher_name].on_complete.Add(manyPingInProgress.OnSingleComplete);
			pingInProgress.BeginConnect(caller);
		}
		manyPingInProgress.TestIsComplete();
	}

	public void OnConnectSucceed(Connection connection)
	{
		GetPingByConnection(connection)?.PingNow();
	}

	public void OnReceive(Packet incoming, Connection connection)
	{
		PingInProgress pingByConnection = GetPingByConnection(connection);
		if (pingByConnection != null && !pingByConnection.remove_self)
		{
			double totalMilliseconds = (DateTime.UtcNow - pingByConnection.ping_sent).TotalMilliseconds;
			pingByConnection.Finish((short)((totalMilliseconds >= 30000.0) ? 30000 : ((int)totalMilliseconds)));
		}
	}

	public void OnConnectFailed(Connection connection)
	{
		PingInProgress pingByConnection = GetPingByConnection(connection);
		if (pingByConnection != null && !pingByConnection.remove_self)
		{
			pingByConnection.Finish(-1);
		}
	}

	public void OnDisconnected(bool hard_disconnect, Connection connection)
	{
		GenericFail(connection);
	}

	private void GenericFail(Connection connection)
	{
		PingInProgress pingByConnection = GetPingByConnection(connection);
		if (pingByConnection != null && !pingByConnection.remove_self)
		{
			pingByConnection.Finish(-1);
		}
	}

	private PingInProgress GetPingByConnection(Connection connection)
	{
		foreach (KeyValuePair<string, PingInProgress> item in pings_in_progress)
		{
			if (item.Value.connection == connection)
			{
				return item.Value;
			}
		}
		return null;
	}

	public void FixedUpdate()
	{
		if (pings_in_progress.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, PingInProgress> item in pings_in_progress)
		{
			PingInProgress value = item.Value;
			value.connection.FixedUpdate();
			if (!value.remove_self)
			{
				if ((DateTime.UtcNow - value.intially_sent).TotalSeconds > 7.0)
				{
					value.Finish(-1);
					Debug.Log("TIMEOUT (" + value.dispatcher_name + ")");
				}
			}
			else if (value.connection.GetStatus() == Connection.connection_status.not_connected)
			{
				list.Add(item.Key);
			}
		}
		foreach (string item2 in list)
		{
			pings_in_progress.Remove(item2);
		}
	}
}
