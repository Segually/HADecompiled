using System;
using System.Collections.Generic;
using UnityEngine;

public class PingInProgress
{
	public string dispatcher_name;

	public Connection connection;

	public List<Action<string, short>> on_complete = new List<Action<string, short>>();

	public bool remove_self;

	public DateTime intially_sent;

	public DateTime ping_sent;

	public PingInProgress(string dispatcher_name, string ip, int port)
	{
		this.dispatcher_name = dispatcher_name;
		connection = new Connection("ipv4", ip, port, Connection.parent_t.PingController, 25);
	}

	public void BeginConnect(MonoBehaviour caller)
	{
		intially_sent = DateTime.UtcNow;
		connection.TryConnect(caller);
	}

	public void PingNow()
	{
		ping_sent = DateTime.UtcNow;
		Packet packet = new Packet();
		packet.PutByte(33);
		connection.Send(packet);
	}

	public void Finish(short rtt)
	{
		foreach (Action<string, short> item in on_complete)
		{
			item(dispatcher_name, rtt);
		}
		PingController.Instance.previous_results.Add(dispatcher_name, rtt);
		remove_self = true;
		connection.Disconnect();
	}
}
