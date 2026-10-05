using System.Collections.Generic;
using System.Threading;

public class SendQueue
{
	private ReaderWriterLockSlim queue_lock;

	private List<Packet> queue;

	private byte[] curr_sending;

	private int curr_sending_iterator;

	public byte stream_id;

	public void Reset()
	{
	}

	public byte GetStatus(int budget)
	{
		return 0;
	}

	public int GetSendLength(int budget)
	{
		return 0;
	}

	public bool CanFitRemainderIntoBudget(int budget)
	{
		return false;
	}

	public void Write(ref int curr_i, float consume_percent, ref byte[] to_send)
	{
	}

	public void WriteToSendArray(int start_index, int budget, ref byte[] to_send, byte status)
	{
	}

	public SendQueue(byte stream_id)
	{
	}

	public bool NeedsToSend()
	{
		return false;
	}

	public void PopNew()
	{
	}

	public void Add(Packet outgoing)
	{
	}
}
