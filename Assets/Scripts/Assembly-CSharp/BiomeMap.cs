public class BiomeMap
{
	private int[,] biome_ids = new int[36, 36];

	private string[,] mobAs = new string[36, 36];

	private string[,] mobBs = new string[36, 36];

	public void SaveToDisk(string biome_map_str)
	{
		string filename = "biome-map(" + biome_map_str + ")";
		PlayerData.Instance.SetSlotShort("exists", 1, filename);
		PlayerData.Instance.SetSlotShort("version", ChunkControl.newest_biomemap_v, filename);
		for (int i = 0; i < 36; i++)
		{
			for (int j = 0; j < 36; j++)
			{
				PlayerData.Instance.SetSlotShort(i + "," + j + ",id", GetBiomeIdAt(i, j), filename);
				PlayerData.Instance.SetSlotString(i + "," + j + ",mobA", GetFirstMobAt(i, j), filename);
				PlayerData.Instance.SetSlotString(i + "," + j + ",mobB", GetSecondMobAt(i, j), filename);
			}
		}
	}

	public int GetBiomeIdAt(int innerX, int innerZ)
	{
		return biome_ids[innerX, innerZ];
	}

	public void SetBiomeIdAt(int innerX, int innerZ, int val)
	{
		biome_ids[innerX, innerZ] = val;
	}

	public string GetFirstMobAt(int innerX, int innerZ)
	{
		string text = mobAs[innerX, innerZ];
		if (!Startup.StringNullOrEmpty(text))
		{
			return text;
		}
		return "";
	}

	public string GetSecondMobAt(int innerX, int innerZ)
	{
		string text = mobBs[innerX, innerZ];
		if (!Startup.StringNullOrEmpty(text))
		{
			return text;
		}
		return "";
	}

	public void SetFirstMobAt(int innerX, int innerZ, string val)
	{
		mobAs[innerX, innerZ] = val;
	}

	public void SetSecondMobAt(int innerX, int innerZ, string val)
	{
		mobBs[innerX, innerZ] = val;
	}
}
