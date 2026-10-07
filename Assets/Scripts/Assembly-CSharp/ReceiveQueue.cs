using System.Collections.Generic;

public class ReceiveQueue
{
	private List<byte> incoming = new List<byte>();

	public void Reset()
	{
		incoming.Clear();
	}

	public void Read(int start_index, int n_read, byte[] read_buffer, byte stream_status, Connection connection)
	{
		if (stream_status == 2 || stream_status == 3)
		{
			incoming = new List<byte>();
		}
		for (int i = 0; i < n_read; i++)
		{
			incoming.Add(read_buffer[start_index + i]);
		}
		if (stream_status == 3 || stream_status == 0)
		{
			Packet packet = new Packet(incoming.ToArray());
			connection.received_data_lock.EnterWriteLock();
			connection.received_data.Add(packet);
			connection.received_data_lock.ExitWriteLock();
		}
	}
}
