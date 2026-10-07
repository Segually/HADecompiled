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

	public List<Packet> received_data = new List<Packet>();

	public ReaderWriterLockSlim received_data_lock = new ReaderWriterLockSlim();

	public parent_t parent;

	private connection_status status;

	private IEnumerator connectCoroutine;

	public flag async_flag;

	private int curr_skip;

	private int max_skip = 5;

	private int receive_packet_len = -1;

	private byte[] receive_buffer = new byte[buffSize];

	private int receive_buffer_iterator;

	private ReceiveQueue receive_queue_SUPER_HIGH = new ReceiveQueue();

	private ReceiveQueue receive_queue_HIGH = new ReceiveQueue();

	private ReceiveQueue receive_queue_DEFAULT = new ReceiveQueue();

	private ReceiveQueue receive_queue_LOW = new ReceiveQueue();

	private long bytes_sent_this_recursion;

	private bool currently_in_recursive_send;

	private byte[] send_buffer = new byte[buffSize];

	private int send_buffer_iterator;

	private SendQueue send_queue_SUPER_HIGH = new SendQueue(3);

	private SendQueue send_queue_HIGH = new SendQueue(2);

	private SendQueue send_queue_DEFAULT = new SendQueue(1);

	private SendQueue send_queue_LOW = new SendQueue(0);

	private int max_bytes_per_skip;

	public Connection(string ip_type, string ip, int port, parent_t parent, int max_kbps_send)
	{
		this.ip_type = ip_type;
		this.ip = ip;
		this.port = port;
		this.parent = parent;
		status = connection_status.not_connected;
		max_bytes_per_skip = (int)(max_kbps_send * 1000f / (60f / max_skip));
	}

	public void Disconnect()
	{
		async_flag = flag.please_disconnect;
	}

	private void CloseSocket()
	{
		socket.Close();
		status = connection_status.not_connected;
		socket = null;
		received_data_lock.EnterWriteLock();
		received_data.Clear();
		received_data_lock.ExitWriteLock();
		receive_packet_len = -1;
		receive_buffer = new byte[buffSize];
		receive_buffer_iterator = 0;
		receive_queue_SUPER_HIGH.Reset();
		receive_queue_HIGH.Reset();
		receive_queue_DEFAULT.Reset();
		receive_queue_LOW.Reset();
		bytes_sent_this_recursion = 0;
		currently_in_recursive_send = false;
		send_buffer = new byte[buffSize];
		send_buffer_iterator = 0;
		send_queue_SUPER_HIGH.Reset();
		send_queue_HIGH.Reset();
		send_queue_DEFAULT.Reset();
		send_queue_LOW.Reset();
	}

	public connection_status GetStatus()
	{
		if (status == connection_status.connected)
		{
			if (socket != null && socket.Connected)
			{
				return connection_status.connected;
			}
			CloseSocket();
			async_flag = flag.disconnected;
		}
		return status;
	}

	public void TryConnect(MonoBehaviour caller)
	{
		if (status == connection_status.connecting)
		{
			return;
		}
		if (status == connection_status.connected && socket != null)
		{
			if (socket.Connected)
			{
				return;
			}
			CloseSocket();
			async_flag = flag.disconnected;
		}
		status = connection_status.connecting;
		already_shut_down_intentionally = false;
		connectCoroutine = ConnectCoroutine();
		caller.StartCoroutine(connectCoroutine);
	}

	private IEnumerator ConnectCoroutine()
	{
		yield return new WaitForSeconds(0.25f);
		socket = new Socket(ip_type == "ipv6" ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		socket.BeginConnect(ip, port, ConnectCallback, null);
		for (int i = 0; i < 80; i++)
		{
			yield return new WaitForSeconds(0.1f);
			if (socket != null && socket.Connected)
			{
				status = connection_status.connected;
				break;
			}
		}
		if (status == connection_status.connected)
		{
			async_flag = flag.connect_succeeded;
		}
		else
		{
			CloseSocket();
			async_flag = flag.connect_failed;
		}
	}

	private void ConnectCallback(IAsyncResult iar)
	{
		socket.EndConnect(iar);
		socket.NoDelay = true;
		socket.BeginReceive(receive_buffer, 0, 2, SocketFlags.None, ReadCallback, null);
	}

	public void FixedUpdate()
	{
		if (curr_skip == 0)
		{
			switch (async_flag)
			{
				case flag.disconnected:
					if (!already_shut_down_intentionally)
					{
						if (parent == parent_t.FriendServerBackend) FriendServerConnector.Instance.OnDisconnected(true);
						else if (parent == parent_t.GameServerBackend) GameServerConnector.Instance.OnDisconnected(true);
						else if (parent == parent_t.PingController) PingController.Instance.OnDisconnected(true, this);
					}
					break;
				case flag.connect_failed:
					if (parent == parent_t.FriendServerBackend) FriendServerConnector.Instance.OnConnectFailed();
					else if (parent == parent_t.GameServerBackend) GameServerConnector.Instance.OnConnectFailed();
					else if (parent == parent_t.PingController) PingController.Instance.OnConnectFailed(this);
					break;
				case flag.connect_succeeded:
					if (parent == parent_t.FriendServerBackend) FriendServerConnector.Instance.OnConnectSucceed();
					else if (parent == parent_t.GameServerBackend) GameServerConnector.Instance.OnConnectSucceed();
					else if (parent == parent_t.PingController) PingController.Instance.OnConnectSucceed(this);
					break;
				case flag.please_disconnect:
					already_shut_down_intentionally = true;
					CloseSocket();
					if (parent == parent_t.FriendServerBackend) FriendServerConnector.Instance.OnDisconnected(false);
					else if (parent == parent_t.GameServerBackend) GameServerConnector.Instance.OnDisconnected(false);
					else if (parent == parent_t.PingController) PingController.Instance.OnDisconnected(false, this);
					break;
			}
			async_flag = flag.none;
			if (status == connection_status.connected)
			{
				FixedUpdateSend();
				FixedUpdateReceive();
			}
			curr_skip = max_skip;
		}
		curr_skip--;
	}

	private void FixedUpdateSend()
	{
		if (!currently_in_recursive_send)
		{
			bytes_sent_this_recursion = 0;
			ProcessSendQueues();
		}
	}

	private void FixedUpdateReceive()
	{
		received_data_lock.EnterReadLock();
		int count = received_data.Count;
		received_data_lock.ExitReadLock();
		if (count == 0)
		{
			return;
		}
		received_data_lock.EnterWriteLock();
		foreach (Packet incoming in received_data)
		{
			if (parent == parent_t.FriendServerBackend) FriendServerReceiver.Instance.OnReceive(incoming);
			else if (parent == parent_t.GameServerBackend) GameServerReceiver.Instance.OnReceive(incoming);
			else if (parent == parent_t.PingController) PingController.Instance.OnReceive(incoming, this);
		}
		received_data.Clear();
		received_data_lock.ExitWriteLock();
	}

	public void ReadCallback(IAsyncResult ar)
	{
		int n_read = socket.EndReceive(ar);
		if (n_read == 0)
		{
			CloseSocket();
			async_flag = flag.disconnected;
			return;
		}
		receive_buffer_iterator += n_read;
		if (receive_packet_len == -1)
		{
			if (receive_buffer_iterator == 2)
			{
				receive_packet_len = BitConverter.ToInt16(receive_buffer, 0) - 2;
				receive_buffer_iterator = 0;
				socket.BeginReceive(receive_buffer, 0, receive_packet_len, SocketFlags.None, ReadCallback, null);
			}
			else
			{
				socket.BeginReceive(receive_buffer, receive_buffer_iterator, 1, SocketFlags.None, ReadCallback, null);
			}
		}
		else
		{
			int remainder = receive_packet_len - receive_buffer_iterator;
			if (remainder == 0)
			{
				ProcessReceive(socket);
				receive_buffer_iterator = 0;
				receive_packet_len = -1;
				socket.BeginReceive(receive_buffer, 0, 2, SocketFlags.None, ReadCallback, null);
			}
			else
			{
				socket.BeginReceive(receive_buffer, receive_buffer_iterator, remainder, SocketFlags.None, ReadCallback, null);
			}
		}
	}

	public ReceiveQueue IdToReceiveQueue(byte stream_id)
	{
		switch (stream_id)
		{
			case 0: return receive_queue_LOW;
			case 1: return receive_queue_DEFAULT;
			case 2: return receive_queue_HIGH;
			case 3: return receive_queue_SUPER_HIGH;
			default: return null;
		}
	}

	private void ReadOne(ref int curr_i)
	{
		byte stream_id = receive_buffer[curr_i++];
		byte stream_status = receive_buffer[curr_i++];
		byte[] bytes = new byte[4];
		for (int i = 0; i < 4; i++)
		{
			bytes[i] = receive_buffer[curr_i++];
		}
		int n_read = BitConverter.ToInt32(bytes, 0);
		IdToReceiveQueue(stream_id).Read(curr_i, n_read, receive_buffer, stream_status, this);
		curr_i += n_read;
	}

	public void ProcessReceive(Socket handler)
	{
		int count = receive_buffer[0];
		int curr_i = 1;
		for (int i = 0; i < count; i++)
		{
			ReadOne(ref curr_i);
		}
	}

	private void ProcessSendQueues()
	{
		List<SendQueue> queues = new List<SendQueue>();
		if (send_queue_SUPER_HIGH.NeedsToSend()) queues.Add(send_queue_SUPER_HIGH);
		if (send_queue_HIGH.NeedsToSend()) queues.Add(send_queue_HIGH);
		if (send_queue_DEFAULT.NeedsToSend()) queues.Add(send_queue_DEFAULT);
		if (send_queue_LOW.NeedsToSend()) queues.Add(send_queue_LOW);
		if (queues.Count == 0)
		{
			currently_in_recursive_send = false;
			return;
		}
		byte[] to_send = new byte[buffSize];
		to_send[2] = (byte)queues.Count;
		int curr_i = 3;
		switch (queues.Count)
		{
			case 2:
				queues[0].Write(ref curr_i, 0.66f, ref to_send);
				break;
			case 3:
				queues[0].Write(ref curr_i, 0.45f, ref to_send);
				queues[1].Write(ref curr_i, 0.66f, ref to_send);
				break;
			case 4:
				queues[0].Write(ref curr_i, 0.4f, ref to_send);
				queues[1].Write(ref curr_i, 0.45f, ref to_send);
				queues[2].Write(ref curr_i, 0.66f, ref to_send);
				break;
		}
		queues[queues.Count - 1].Write(ref curr_i, 1f, ref to_send);
		byte[] length = BitConverter.GetBytes(curr_i);
		to_send[0] = length[0];
		to_send[1] = length[1];
		currently_in_recursive_send = true;
		send_buffer = new byte[curr_i];
		Array.Copy(to_send, send_buffer, curr_i);
		send_buffer_iterator = 0;
		socket.BeginSend(send_buffer, 0, send_buffer.Length, SocketFlags.None, OnSendComplete, null);
	}

	public void Send(Packet outgoing, priority send_priority = priority.DEFAULT)
	{
		switch (send_priority)
		{
			case priority.SUPER_HIGH: send_queue_SUPER_HIGH.Add(outgoing); break;
			case priority.HIGH: send_queue_HIGH.Add(outgoing); break;
			case priority.DEFAULT: send_queue_DEFAULT.Add(outgoing); break;
			case priority.LOW: send_queue_LOW.Add(outgoing); break;
		}
	}

	private void OnSendComplete(IAsyncResult iar)
	{
		OnSendComplete2(socket.EndSend(iar));
	}

	private void OnSendComplete2(int bytes_sent)
	{
		send_buffer_iterator += bytes_sent;
		if (send_buffer_iterator == send_buffer.Length)
		{
			bytes_sent_this_recursion += send_buffer.Length;
			if (bytes_sent_this_recursion < max_bytes_per_skip)
			{
				ProcessSendQueues();
			}
			else
			{
				currently_in_recursive_send = false;
			}
		}
		else
		{
			socket.BeginSend(send_buffer, send_buffer_iterator, send_buffer.Length - send_buffer_iterator, SocketFlags.None, OnSendComplete, null);
		}
	}
}
