using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Packet
{
	private byte[] incoming_data;

	private List<byte> outgoing_data;

	private int read_iterator;

	private bool unreadable;

	public Packet(byte[] incoming_data)
	{
		this.incoming_data = incoming_data;
	}

	public Packet()
	{
		outgoing_data = new List<byte>();
	}

	public void PutByte(byte B)
	{
		outgoing_data.Add(B);
	}

	public void PutShort(short S)
	{
		byte[] bytes = BitConverter.GetBytes(S);
		outgoing_data.Add(bytes[0]);
		outgoing_data.Add(bytes[1]);
	}

	public void PutShort(float f)
	{
		PutShort((short)f);
	}

	public void PutString(string str)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(Startup.StringNullOrEmpty(str) ? "" : str);
		byte[] bytes2 = BitConverter.GetBytes(bytes.Length);
		outgoing_data.Add(bytes2[0]);
		outgoing_data.Add(bytes2[1]);
		for (int i = 0; i < bytes.Length; i++)
		{
			outgoing_data.Add(bytes[i]);
		}
	}

	public void PutLong(int i)
	{
		byte[] bytes = BitConverter.GetBytes(i);
		outgoing_data.Add(bytes[0]);
		outgoing_data.Add(bytes[1]);
		outgoing_data.Add(bytes[2]);
		outgoing_data.Add(bytes[3]);
	}

	public byte GetByte()
	{
		if (unreadable)
		{
			return 0;
		}
		try
		{
			byte result = incoming_data[read_iterator];
			read_iterator++;
			return result;
		}
		catch (Exception ex)
		{
			unreadable = true;
			Debug.Log("<color=#ff0000>PACKET ERROR </color> ... " + ex);
			return 0;
		}
	}

	public short GetShort()
	{
		if (unreadable)
		{
			return 0;
		}
		try
		{
			short result = BitConverter.ToInt16(incoming_data, read_iterator);
			read_iterator += 2;
			return result;
		}
		catch (Exception ex)
		{
			unreadable = true;
			Debug.Log("<color=#ff0000>PACKET ERROR </color> ... " + ex);
			return 0;
		}
	}

	public string GetString()
	{
		string result = "";
		if (unreadable)
		{
			return result;
		}
		try
		{
			short num = BitConverter.ToInt16(incoming_data, read_iterator);
			read_iterator += 2;
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = incoming_data[read_iterator];
				read_iterator++;
			}
			result = Encoding.Unicode.GetString(array);
			return result;
		}
		catch (Exception ex)
		{
			unreadable = true;
			Debug.Log("<color=#ff0000>PACKET ERROR </color> ... " + ex);
			return result;
		}
	}

	public int GetLong()
	{
		if (unreadable)
		{
			return 0;
		}
		try
		{
			int result = BitConverter.ToInt32(incoming_data, read_iterator);
			read_iterator += 4;
			return result;
		}
		catch (Exception ex)
		{
			unreadable = true;
			Debug.Log("<color=#ff0000>PACKET ERROR </color> ... " + ex);
			return 0;
		}
	}

	public byte[] ToByteArray()
	{
		return outgoing_data.ToArray();
	}
}
