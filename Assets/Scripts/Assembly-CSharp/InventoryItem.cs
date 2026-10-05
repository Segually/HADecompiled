using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class InventoryItem
{
	private Dictionary<string, object> data = new Dictionary<string, object>();

	public string item_name => GetString("item_id");

	public InventoryItem(string item_name)
	{
		if (!Startup.StringNullOrWhitespace(item_name))
		{
			if (data.ContainsKey("item_id"))
			{
				data["item_id"] = item_name;
			}
			else
			{
				data.Add("item_id", item_name);
			}
		}
	}

	public InventoryItem(string new_item_name, ExtraInventoryData extra_data)
	{
		if (extra_data == null)
		{
			return;
		}
		foreach (KeyValuePair<string, object> item in extra_data.GetDataForCopying())
		{
			if (!data.ContainsKey(item.Key))
			{
				data.Add(item.Key, item.Value);
			}
			else
			{
				data[item.Key] = item.Value;
			}
		}
		if (!Startup.StringNullOrWhitespace(new_item_name))
		{
			if (data.ContainsKey("item_id"))
			{
				data["item_id"] = new_item_name;
			}
			else
			{
				data.Add("item_id", new_item_name);
			}
		}
	}

	public bool HasActiveRespawn(string respawn_prefix)
	{
		if (!HasActiveOrExpiredRespawn(respawn_prefix))
		{
			return false;
		}
		return !IsRespawnExpired(respawn_prefix);
	}

	public bool HasActiveOrExpiredRespawn(string respawn_prefix)
	{
		return GetString("has_respawn_" + respawn_prefix) == "true";
	}

	public bool IsRespawnExpired(string respawn_prefix)
	{
		if (HasActiveOrExpiredRespawn(respawn_prefix))
		{
			return GetRespawnDateTime(respawn_prefix) < DateTime.UtcNow;
		}
		return true;
	}

	public DateTime GetRespawnDateTime(string respawn_prefix)
	{
		if (HasActiveOrExpiredRespawn(respawn_prefix) && DateTime.TryParseExact(GetString("UTC_dateTime_" + respawn_prefix), "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
		{
			return result;
		}
		return DateTime.UtcNow;
	}

	public short GetShort(string key)
	{
		if (data.ContainsKey(key))
		{
			return (short)data[key];
		}
		return 0;
	}

	public string GetString(string key)
	{
		if (data.ContainsKey(key))
		{
			return (string)data[key];
		}
		return "";
	}

	public int GetLong(string key)
	{
		if (data.ContainsKey(key))
		{
			return (int)data[key];
		}
		return 0;
	}

	public int DataSize()
	{
		return data.Count;
	}

	public Dictionary<string, object> GetRawData()
	{
		return data;
	}

	public ExtraInventoryData GetExtraDataCopy()
	{
		ExtraInventoryData extraInventoryData = new ExtraInventoryData();
		foreach (KeyValuePair<string, object> datum in data)
		{
			if (datum.Value.GetType() == typeof(short))
			{
				extraInventoryData.SetShort(datum.Key, (short)datum.Value);
			}
			else if (datum.Value.GetType() == typeof(string))
			{
				extraInventoryData.SetString(datum.Key, (string)datum.Value);
			}
			else if (datum.Value.GetType() == typeof(int))
			{
				extraInventoryData.SetLong(datum.Key, (int)datum.Value);
			}
		}
		return extraInventoryData;
	}

	public void SaveToDisk(string filename, string prefix, string filesegment)
	{
		SaveToFile(PlayerData.Instance.GetSlotFilesGroup(-1).GetFile(filename), prefix, filesegment);
	}

	public void SaveToFile(SingleFile file, string prefix, string filesegment)
	{
		Dictionary<string, short> dictionary = new Dictionary<string, short>();
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
		foreach (KeyValuePair<string, object> datum in data)
		{
			if (datum.Value == null)
			{
				Debug.Log("ERROR: trying to save InventoryItem with value=null [key=" + datum.Key + "]");
			}
			else if (datum.Value.GetType() == typeof(short))
			{
				short num = (short)datum.Value;
				if (num != 0)
				{
					if (!dictionary.ContainsKey(datum.Key))
					{
						dictionary.Add(datum.Key, num);
					}
					else
					{
						dictionary[datum.Key] = num;
					}
				}
			}
			else if (datum.Value.GetType() == typeof(string))
			{
				string text = (string)datum.Value;
				if (!Startup.StringNullOrEmpty(text))
				{
					if (!dictionary2.ContainsKey(datum.Key))
					{
						dictionary2.Add(datum.Key, text);
					}
					else
					{
						dictionary2[datum.Key] = text;
					}
				}
			}
			else if (datum.Value.GetType() == typeof(int))
			{
				int num2 = (int)datum.Value;
				if (num2 != 0)
				{
					if (!dictionary3.ContainsKey(datum.Key))
					{
						dictionary3.Add(datum.Key, num2);
					}
					else
					{
						dictionary3[datum.Key] = num2;
					}
				}
			}
		}
		file.SetShort(prefix + "_n_shorts", dictionary.Count, filesegment);
		int num3 = 0;
		foreach (KeyValuePair<string, short> item in dictionary)
		{
			file.SetString(prefix + "_short" + num3 + "_key", item.Key, filesegment);
			file.SetShort(prefix + "_short" + num3 + "_value", item.Value, filesegment);
			num3++;
		}
		file.SetShort(prefix + "_n_strings", dictionary2.Count, filesegment);
		num3 = 0;
		foreach (KeyValuePair<string, string> item2 in dictionary2)
		{
			file.SetString(prefix + "_string" + num3 + "_key", item2.Key, filesegment);
			file.SetString(prefix + "_string" + num3 + "_value", item2.Value, filesegment);
			num3++;
		}
		file.SetShort(prefix + "_n_longs", dictionary3.Count, filesegment);
		num3 = 0;
		foreach (KeyValuePair<string, int> item3 in dictionary3)
		{
			file.SetString(prefix + "_long" + num3 + "_key", item3.Key, filesegment);
			file.SetLong(prefix + "_long" + num3 + "_value", item3.Value, filesegment);
			num3++;
		}
	}

	public static InventoryItem LoadFromDisk(string prefix, string filename, string filesegment)
	{
		return LoadFromFile(prefix, PlayerData.Instance.GetSlotFilesGroup(-1).GetFile(filename), filesegment);
	}

	public static InventoryItem LoadFromFile(string prefix, SingleFile file, string filesegment)
	{
		InventoryItem inventoryItem = new InventoryItem("");
		short shortValue = file.GetShortValue(prefix + "_n_shorts", filesegment);
		for (int i = 0; i < shortValue; i++)
		{
			string stringValue = file.GetStringValue(prefix + "_short" + i + "_key", filesegment);
			short shortValue2 = file.GetShortValue(prefix + "_short" + i + "_value", filesegment);
			if (!inventoryItem.data.ContainsKey(stringValue))
			{
				inventoryItem.data.Add(stringValue, shortValue2);
			}
			else
			{
				inventoryItem.data[stringValue] = shortValue2;
			}
		}
		short shortValue3 = file.GetShortValue(prefix + "_n_strings", filesegment);
		for (int j = 0; j < shortValue3; j++)
		{
			string stringValue2 = file.GetStringValue(prefix + "_string" + j + "_key", filesegment);
			string stringValue3 = file.GetStringValue(prefix + "_string" + j + "_value", filesegment);
			if (!inventoryItem.data.ContainsKey(stringValue2))
			{
				inventoryItem.data.Add(stringValue2, stringValue3);
			}
			else
			{
				inventoryItem.data[stringValue2] = stringValue3;
			}
		}
		short shortValue4 = file.GetShortValue(prefix + "_n_longs", filesegment);
		for (int k = 0; k < shortValue4; k++)
		{
			string stringValue4 = file.GetStringValue(prefix + "_long" + k + "_key", filesegment);
			int longValue = file.GetLongValue(prefix + "_long" + k + "_value", filesegment);
			if (!inventoryItem.data.ContainsKey(stringValue4))
			{
				inventoryItem.data.Add(stringValue4, longValue);
			}
			else
			{
				inventoryItem.data[stringValue4] = longValue;
			}
		}
		return inventoryItem;
	}

	public InventoryItem LoadSubItem(string sub_item_name)
	{
		InventoryItem inventoryItem = new InventoryItem("");
		string text = "item(" + sub_item_name + ")";
		foreach (KeyValuePair<string, object> datum in data)
		{
			if (datum.Key.Contains(text))
			{
				string key = datum.Key.Replace(text, "");
				if (!inventoryItem.data.ContainsKey(key))
				{
					inventoryItem.data.Add(key, datum.Value);
				}
				else
				{
					inventoryItem.data[key] = datum.Value;
				}
			}
		}
		return inventoryItem;
	}

	public void PackForWeb(Packet outgoing)
	{
	}

	public static InventoryItem UnpackFromWeb(Packet incoming)
	{
		return null;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as InventoryItem);
	}

	public bool Equals(InventoryItem other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (GetType() != other.GetType())
		{
			return false;
		}
		if (data.Count != other.data.Count)
		{
			return false;
		}
		foreach (KeyValuePair<string, object> datum in other.data)
		{
			if (!data.ContainsKey(datum.Key))
			{
				return false;
			}
		}
		foreach (KeyValuePair<string, object> datum2 in data)
		{
			if (!other.data.ContainsKey(datum2.Key))
			{
				return false;
			}
			if (datum2.Value.GetType() == typeof(short))
			{
				if ((short)data[datum2.Key] != (short)other.data[datum2.Key])
				{
					return false;
				}
			}
			else if (datum2.Value.GetType() == typeof(string))
			{
				if ((string)data[datum2.Key] != (string)other.data[datum2.Key])
				{
					return false;
				}
			}
			else if (datum2.Value.GetType() == typeof(int) && (int)data[datum2.Key] != (int)other.data[datum2.Key])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = 0;
		foreach (KeyValuePair<string, object> datum in data)
		{
			num += datum.Key.GetHashCode();
			if (datum.Value.GetType() == typeof(short))
			{
				num += ((short)datum.Value).GetHashCode();
			}
			else if (datum.Value.GetType() == typeof(string))
			{
				num += ((string)datum.Value).GetHashCode();
			}
			else if (datum.Value.GetType() == typeof(int))
			{
				num += ((int)datum.Value).GetHashCode();
			}
		}
		return num;
	}

	public static bool operator ==(InventoryItem lhs, InventoryItem rhs)
	{
		if ((object)lhs == null)
		{
			return (object)rhs == null;
		}
		return lhs.Equals(rhs);
	}

	public static bool operator !=(InventoryItem lhs, InventoryItem rhs)
	{
		return !(lhs == rhs);
	}

	public static InventoryItem SetDisplayDefaults(InventoryItem item)
	{
		return null;
	}
}
