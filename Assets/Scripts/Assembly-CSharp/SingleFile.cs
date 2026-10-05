using System;
using System.Collections.Generic;

public class SingleFile
{
	public DateTime last_time_used;

	public bool edited_since_load;

	public Dictionary<string, Dictionary<string, object>> file_segments = new Dictionary<string, Dictionary<string, object>>();

	public void LoadFromBytes(byte[] bytes, bool skip_validator = false)
	{
		Packet packet = new Packet(bytes);
		packet.GetByte();
		short num = packet.GetShort();
		for (int i = 0; i < num; i++)
		{
			string file_segment = packet.GetString();
			packet.GetByte();
			for (int num2 = packet.GetLong(); num2 > 0; num2--)
			{
				string key = packet.GetString();
				short value = packet.GetShort();
				SetShort(key, value, file_segment, true);
			}
			for (int num3 = packet.GetLong(); num3 > 0; num3--)
			{
				string key2 = packet.GetString();
				string value2 = packet.GetString();
				SetString(key2, value2, file_segment, true);
			}
			for (int num4 = packet.GetLong(); num4 > 0; num4--)
			{
				string key3 = packet.GetString();
				int value3 = packet.GetLong();
				SetLong(key3, value3, file_segment, true);
			}
		}
		packet.GetString();
	}

	public byte[] ToByteArray()
	{
		Packet packet = new Packet();
		packet.PutByte(3);
		packet.PutShort((short)file_segments.Count);
		foreach (KeyValuePair<string, Dictionary<string, object>> file_segment in file_segments)
		{
			packet.PutString(file_segment.Key);
			packet.PutByte(1);
			Dictionary<string, short> dictionary = new Dictionary<string, short>();
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
			foreach (KeyValuePair<string, object> item in file_segment.Value)
			{
				if (item.Value == null)
				{
					continue;
				}
				if (item.Value.GetType() == typeof(short))
				{
					if ((short)item.Value != 0)
					{
						dictionary.Add(item.Key, (short)item.Value);
					}
				}
				else if (item.Value.GetType() == typeof(string))
				{
					if (!Startup.StringNullOrWhitespace((string)item.Value))
					{
						dictionary2.Add(item.Key, (string)item.Value);
					}
				}
				else if (item.Value.GetType() == typeof(int) && (int)item.Value != 0)
				{
					dictionary3.Add(item.Key, (int)item.Value);
				}
			}
			packet.PutLong(dictionary.Count);
			foreach (KeyValuePair<string, short> item2 in dictionary)
			{
				packet.PutString(item2.Key);
				packet.PutShort(item2.Value);
			}
			packet.PutLong(dictionary2.Count);
			foreach (KeyValuePair<string, string> item3 in dictionary2)
			{
				packet.PutString(item3.Key);
				packet.PutString(item3.Value);
			}
			packet.PutLong(dictionary3.Count);
			foreach (KeyValuePair<string, int> item4 in dictionary3)
			{
				packet.PutString(item4.Key);
				packet.PutLong(item4.Value);
			}
		}
		packet.PutString("");
		return packet.ToByteArray();
	}

	public string GetStringValue(string key, string file_segment = "default")
	{
		last_time_used = DateTime.Now;
		if (!file_segments.ContainsKey(file_segment))
		{
			return "";
		}
		Dictionary<string, object> dictionary = file_segments[file_segment];
		if (Startup.StringNullOrWhitespace(key))
		{
			return "";
		}
		if (!dictionary.ContainsKey(key))
		{
			return "";
		}
		return (string)dictionary[key];
	}

	public short GetShortValue(string key, string file_segment = "default")
	{
		last_time_used = DateTime.Now;
		if (!file_segments.ContainsKey(file_segment))
		{
			return 0;
		}
		Dictionary<string, object> dictionary = file_segments[file_segment];
		if (Startup.StringNullOrWhitespace(key))
		{
			return 0;
		}
		if (!dictionary.ContainsKey(key))
		{
			return 0;
		}
		return (short)dictionary[key];
	}

	public int GetLongValue(string key, string file_segment = "default")
	{
		last_time_used = DateTime.Now;
		if (!file_segments.ContainsKey(file_segment))
		{
			return 0;
		}
		Dictionary<string, object> dictionary = file_segments[file_segment];
		if (Startup.StringNullOrWhitespace(key))
		{
			return 0;
		}
		if (!dictionary.ContainsKey(key))
		{
			return 0;
		}
		return (int)dictionary[key];
	}

	public void SetShort(string key, int value, string file_segment = "default", bool on_load = false)
	{
		SetShort(key, (short)value, file_segment, on_load);
	}

	public void SetShort(string key, short value, string file_segment = "default", bool on_load = false)
	{
		if (Startup.StringNullOrWhitespace(key))
		{
			return;
		}
		last_time_used = DateTime.Now;
		if (!file_segments.ContainsKey(file_segment))
		{
			file_segments.Add(file_segment, new Dictionary<string, object>());
		}
		Dictionary<string, object> dictionary = file_segments[file_segment];
		if (!dictionary.ContainsKey(key))
		{
			dictionary.Add(key, value);
			if (!on_load)
			{
				edited_since_load = true;
			}
			return;
		}
		if (!on_load && (short)dictionary[key] != value)
		{
			edited_since_load = true;
		}
		dictionary[key] = value;
	}

	public void SetString(string key, string value, string file_segment = "default", bool on_load = false)
	{
		if (Startup.StringNullOrWhitespace(key))
		{
			return;
		}
		last_time_used = DateTime.Now;
		if (!file_segments.ContainsKey(file_segment))
		{
			file_segments.Add(file_segment, new Dictionary<string, object>());
		}
		Dictionary<string, object> dictionary = file_segments[file_segment];
		if (!dictionary.ContainsKey(key))
		{
			dictionary.Add(key, value);
			if (!on_load)
			{
				edited_since_load = true;
			}
			return;
		}
		if (!on_load && value != (string)dictionary[key])
		{
			edited_since_load = true;
		}
		dictionary[key] = value;
	}

	public void SetLong(string key, int value, string file_segment = "default", bool on_load = false)
	{
		if (Startup.StringNullOrWhitespace(key))
		{
			return;
		}
		last_time_used = DateTime.Now;
		if (!file_segments.ContainsKey(file_segment))
		{
			file_segments.Add(file_segment, new Dictionary<string, object>());
		}
		Dictionary<string, object> dictionary = file_segments[file_segment];
		if (!dictionary.ContainsKey(key))
		{
			dictionary.Add(key, value);
			if (!on_load)
			{
				edited_since_load = true;
			}
			return;
		}
		if (!on_load && (int)dictionary[key] != value)
		{
			edited_since_load = true;
		}
		dictionary[key] = value;
	}

	public void ClearSegment(string segment_name)
	{
		if (file_segments.ContainsKey(segment_name))
		{
			file_segments[segment_name].Clear();
		}
	}
}
