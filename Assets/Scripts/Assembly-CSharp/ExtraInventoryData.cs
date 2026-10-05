using System.Collections.Generic;

public class ExtraInventoryData
{
	private Dictionary<string, object> data = new Dictionary<string, object>();

	public void SaveSubItem(InventoryItem to_store, string sub_item_name)
	{
		ClearSubItem(sub_item_name);
		foreach (KeyValuePair<string, object> rawDatum in to_store.GetRawData())
		{
			string key = "item(" + sub_item_name + ")" + rawDatum.Key;
			if (!data.ContainsKey(key))
			{
				data.Add(key, rawDatum.Value);
			}
			else
			{
				data[key] = rawDatum.Value;
			}
		}
	}

	public void ClearSubItem(string sub_item_name)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, object> datum in data)
		{
			if (datum.Key.Contains("item(" + sub_item_name + ")"))
			{
				list.Add(datum.Key);
			}
		}
		foreach (string item in list)
		{
			data.Remove(item);
		}
	}

	public void SetShort(string key, short value)
	{
		if (value == 0)
		{
			data.Remove(key);
		}
		else if (!data.ContainsKey(key))
		{
			data.Add(key, value);
		}
		else
		{
			data[key] = value;
		}
	}

	public void SetShort(string key, int value)
	{
		SetShort(key, (short)value);
	}

	public void SetString(string key, string value)
	{
		if (value == "")
		{
			data.Remove(key);
		}
		else if (data.ContainsKey(key))
		{
			data[key] = value;
		}
		else
		{
			data.Add(key, value);
		}
	}

	public void SetLong(string key, int value)
	{
		if (value == 0)
		{
			data.Remove(key);
		}
		else if (!data.ContainsKey(key))
		{
			data.Add(key, value);
		}
		else
		{
			data[key] = value;
		}
	}

	public Dictionary<string, object> GetDataForCopying()
	{
		return data;
	}
}
