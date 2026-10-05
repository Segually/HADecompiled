using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Connection
{
	public enum parent_t
	{
		FriendServerBackend = 0,
		GameServerBackend = 1,
		PingController = 2
	}

	public enum connection_status
	{
		not_connected = 0,
		connecting = 1,
		connected = 2
	}

	public enum flag
	{
		none = 0,
		disconnected = 1,
		connect_failed = 2,
		connect_succeeded = 3,
		please_disconnect = 4
	}

	public enum priority
	{
		SUPER_HIGH = 0,
		HIGH = 1,
		DEFAULT = 2,
		LOW = 3
	}

	public const int buffSize = 8192;

	public Socket socket;

	private string ip;

	private string ip_type;

	private int port;

	public bool already_shut_down_intentionally;

	public List<Packet> received_data;

	public ReaderWriterLockSlim received_data_lock;

	public parent_t parent;

	private connection_status status;

	private IEnumerator connectCoroutine;

	public flag async_flag;

	private int curr_skip;

	private int max_skip;

	private int receive_packet_len;

	private byte[] receive_buffer;

	private int receive_buffer_iterator;

	private ReceiveQueue receive_queue_SUPER_HIGH;

	private ReceiveQueue receive_queue_HIGH;

	private ReceiveQueue receive_queue_DEFAULT;

	private ReceiveQueue receive_queue_LOW;

	private long bytes_sent_this_recursion;

	private bool currently_in_recursive_send;

	private byte[] send_buffer;

	private int send_buffer_iterator;

	private SendQueue send_queue_SUPER_HIGH;

	private SendQueue send_queue_HIGH;

	private SendQueue send_queue_DEFAULT;

	private SendQueue send_queue_LOW;

	private int max_bytes_per_skip;

	public Connection(string ip_type, string ip, int port, parent_t parent, int max_kbps_send)
	{
	}

	public void Disconnect()
	{
	}

	private void CloseSocket()
	{
	}

	public connection_status GetStatus()
	{
		return default(connection_status);
	}

	public void TryConnect(MonoBehaviour caller)
	{
	}

	private IEnumerator ConnectCoroutine()
	{
		return null;
	}

	private void ConnectCallback(IAsyncResult iar)
	{
	}

	public void FixedUpdate()
	{
	}

	private void FixedUpdateSend()
	{
	}

	private void FixedUpdateReceive()
	{
	}

	public void ReadCallback(IAsyncResult ar)
	{
	}

	public ReceiveQueue IdToReceiveQueue(byte stream_id)
	{
		return null;
	}

	private void ReadOne(ref int curr_i)
	{
	}

	public void ProcessReceive(Socket handler)
	{
	}

	private void ProcessSendQueues()
	{
	}

	public void Send(Packet outgoing, priority send_priority = priority.DEFAULT)
	{
	}

	private void OnSendComplete(IAsyncResult iar)
	{
	}

	private void OnSendComplete2(int bytes_sent)
	{
	}
}
