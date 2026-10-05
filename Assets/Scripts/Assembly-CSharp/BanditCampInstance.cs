using System.Collections.Generic;

public class BanditCampInstance
{
	public string instance_name;

	public int biome_id;

	public string template;

	public int instance_rot;

	public float instance_depth;

	public bool flag_destroyed;

	public bool is_debug_data;

	private Dictionary<string, string> bandit_camp_data;

	public void PackForWeb(Packet outgoing)
	{
	}

	public static BanditCampInstance UnpackFromWeb(Packet incoming)
	{
		return null;
	}

	public void ClearBanditCampData()
	{
	}

	public BanditCampInstance(string instance_name, int biome_id, string template, int instance_rot, float instance_depth, bool flag_destroyed, bool is_debug_data)
	{
	}

	public static BanditCampInstance LoadFromDisk(string instance_name)
	{
		return null;
	}

	public void SaveToDisk()
	{
	}

	public string GetRandomizedGem()
	{
		return null;
	}

	public string GetRandomizedGemLarge()
	{
		return null;
	}

	public string GetRandomizedBossEntry(string entry)
	{
		return null;
	}

	private void GenerateRandomizedGems()
	{
	}

	private void GenerateRandomizedBoss()
	{
	}
}
