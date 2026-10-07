using System;
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
		queue_lock.EnterWriteLock();
		queue.Clear();
		queue_lock.ExitWriteLock();
		curr_sending = null;
		curr_sending_iterator = 0;
	}

	public byte GetStatus(int budget)
	{
		if (curr_sending == null)
		{
			PopNew();
			return (byte)(CanFitRemainderIntoBudget(budget) ? 3 : 2);
		}
		return (byte)(CanFitRemainderIntoBudget(budget) ? 0 : 1);
	}

	public int GetSendLength(int budget)
	{
		return Math.Min(budget, curr_sending.Length - curr_sending_iterator);
	}

	public bool CanFitRemainderIntoBudget(int budget)
	{
		return curr_sending.Length - curr_sending_iterator <= budget;
	}

	public void Write(ref int curr_i, float consume_percent, ref byte[] to_send)
	{
		to_send[curr_i++] = stream_id;
		int budget = (int)((8192 - curr_i) * consume_percent) - 5;
		byte status = GetStatus(budget);
		to_send[curr_i++] = status;
		int length = GetSendLength(budget);
		byte[] bytes = BitConverter.GetBytes(length);
		for (int i = 0; i < 4; i++)
		{
			to_send[curr_i++] = bytes[i];
		}
		WriteToSendArray(curr_i, length, ref to_send, status);
		curr_i += length;
	}

	public void WriteToSendArray(int start_index, int budget, ref byte[] to_send, byte status)
	{
		Array.Copy(curr_sending, curr_sending_iterator, to_send, start_index, budget);
		if (status != 3 && status != 0)
		{
			curr_sending_iterator += budget;
		}
		else
		{
			curr_sending_iterator = 0;
			curr_sending = null;
		}
	}

	public SendQueue(byte stream_id)
	{
		queue_lock = new ReaderWriterLockSlim();
		queue = new List<Packet>();
		this.stream_id = stream_id;
	}

	public bool NeedsToSend()
	{
		if (curr_sending != null)
		{
			return true;
		}
		queue_lock.EnterReadLock();
		int count = queue.Count;
		queue_lock.ExitReadLock();
		return count > 0;
	}

	public void PopNew()
	{
		queue_lock.EnterWriteLock();
		curr_sending = queue[0].ToByteArray();
		queue.RemoveAt(0);
		queue_lock.ExitWriteLock();
	}

	public void Add(Packet outgoing)
	{
		queue_lock.EnterWriteLock();
		queue.Add(outgoing);
		queue_lock.ExitWriteLock();
	}
}
