using System.Collections.Generic;
using UnityEngine;

public class BanditCampInstance
{
	public string instance_name;

	public int biome_id;

	public string template = "";

	public int instance_rot;

	public float instance_depth;

	public bool flag_destroyed;

	public bool is_debug_data;

	private Dictionary<string, string> bandit_camp_data = new Dictionary<string, string>();

	public void PackForWeb(Packet outgoing)
	{
	}

	public static BanditCampInstance UnpackFromWeb(Packet incoming)
	{
		return null;
	}

	public void ClearBanditCampData()
	{
		bandit_camp_data.Clear();
	}

	public BanditCampInstance(string instance_name, int biome_id, string template, int instance_rot, float instance_depth, bool flag_destroyed, bool is_debug_data)
	{
		this.instance_name = instance_name;
		this.biome_id = biome_id;
		this.template = template;
		this.instance_rot = instance_rot;
		this.instance_depth = instance_depth;
		this.flag_destroyed = flag_destroyed;
		this.is_debug_data = is_debug_data;
	}

	public static BanditCampInstance LoadFromDisk(string instance_name)
	{
		string filename = "bandit-camp-instance(" + instance_name + ")";
		short slotShort = PlayerData.Instance.GetSlotShort("biome_id", filename);
		string slotString = PlayerData.Instance.GetSlotString("template", filename);
		short slotShort2 = PlayerData.Instance.GetSlotShort("instance_rot", filename);
		int slotLong = PlayerData.Instance.GetSlotLong("instance_depth", filename);
		short slotShort3 = PlayerData.Instance.GetSlotShort("flag_destroyed", filename);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		short slotShort4 = PlayerData.Instance.GetSlotShort("n_bandit_camp_data_entries", filename);
		for (int i = 0; i < slotShort4; i++)
		{
			string slotString2 = PlayerData.Instance.GetSlotString("bandit_camp_data_entry_" + i + "_key", filename);
			string slotString3 = PlayerData.Instance.GetSlotString("bandit_camp_data_entry_" + i + "_val", filename);
			dictionary.Add(slotString2, slotString3);
		}
		return new BanditCampInstance(instance_name, slotShort, slotString, slotShort2, (float)slotLong / 10f, slotShort3 == 1, is_debug_data: false)
		{
			bandit_camp_data = dictionary
		};
	}

	public void SaveToDisk()
	{
		if (is_debug_data)
		{
			return;
		}
		string filename = "bandit-camp-instance(" + instance_name + ")";
		PlayerData.Instance.SetSlotShort("biome_id", biome_id, filename);
		PlayerData.Instance.SetSlotString("template", template, filename);
		PlayerData.Instance.SetSlotShort("instance_rot", instance_rot, filename);
		PlayerData.Instance.SetSlotLong("instance_depth", (int)(instance_depth * 10f), filename);
		PlayerData.Instance.SetSlotShort("flag_destroyed", (byte)(flag_destroyed ? 1 : 0), filename);
		PlayerData.Instance.SetSlotShort("n_bandit_camp_data_entries", bandit_camp_data.Count, filename);
		int num = 0;
		foreach (KeyValuePair<string, string> bandit_camp_datum in bandit_camp_data)
		{
			PlayerData.Instance.SetSlotString("bandit_camp_data_entry_" + num + "_key", bandit_camp_datum.Key, filename);
			PlayerData.Instance.SetSlotString("bandit_camp_data_entry_" + num + "_val", bandit_camp_datum.Value, filename);
			num++;
		}
	}

	public string GetRandomizedGem()
	{
		if (!bandit_camp_data.ContainsKey("RANDOM_BIOME_GEM"))
		{
			GenerateRandomizedGems();
		}
		return bandit_camp_data["RANDOM_BIOME_GEM"];
	}

	public string GetRandomizedGemLarge()
	{
		if (!bandit_camp_data.ContainsKey("RANDOM_BIOME_GEM_LARGE"))
		{
			GenerateRandomizedGems();
		}
		return bandit_camp_data["RANDOM_BIOME_GEM_LARGE"];
	}

	public string GetRandomizedBossEntry(string entry)
	{
		if (!bandit_camp_data.ContainsKey(entry))
		{
			GenerateRandomizedBoss();
		}
		return bandit_camp_data[entry];
	}

	private void GenerateRandomizedGems()
	{
		ChunkControl.cave_define correspondingCave = ChunkControl.Instance.GetCorrespondingCave(inventory_ctr.BiomeIdToCaveEntrance(biome_id));
		ChunkControl.cave_mineral_pairs cave_mineral_pairs = correspondingCave.possible_gem_pairs[Random.Range(0, correspondingCave.possible_gem_pairs.Length)];
		string value = cave_mineral_pairs.small_mineral;
		string value2 = cave_mineral_pairs.large_mineral;
		if (Random.value < 0.05f && correspondingCave.possible_SUPER_RARE_minerals.Length != 0)
		{
			ChunkControl.cave_mineral_pairs cave_mineral_pairs2 = correspondingCave.possible_SUPER_RARE_minerals[Random.Range(0, correspondingCave.possible_SUPER_RARE_minerals.Length)];
			value = cave_mineral_pairs2.small_mineral;
			value2 = cave_mineral_pairs2.large_mineral;
		}
		bandit_camp_data.TryAdd("RANDOM_BIOME_GEM", value);
		bandit_camp_data.TryAdd("RANDOM_BIOME_GEM_LARGE", value2);
		SaveToDisk();
	}

	private void GenerateRandomizedBoss()
	{
		string randomBossNameFromFaction = BanditCampsControl.Instance.GetRandomBossNameFromFaction(biome_id);
		string biome_mob_a = "";
		string biome_mob_b = "";
		ChunkControl.Instance.AssignRandomBiomeMobs(biome_id, ref biome_mob_a, ref biome_mob_b);
		string[] array = new string[8] { "Metal DoubleDagger", "Metal Cleaver", "Titanium CurveSword", "Gold DoubleSword", "Uranium Hasta", "Magma SwirlSword", "Dark Scythe", "Ancient Lance" };
		string value = array[Random.Range(0, array.Length)];
		string[] array2 = new string[3] { "Metal Armor", "Uranium Armor", "Dark Armor" };
		string value2 = array2[Random.Range(0, array2.Length)];
		string[] array3 = new string[8] { "Metal Legion Helm", "Titanium Valkyrie Helm", "Gold Shogun Helm", "Uranium Brute Helm", "Magma Menace Helm", "Ice Solaris Helm", "Dark Skull Helm", "Ancient Eldritch Helm" };
		string value3 = array3[Random.Range(0, array3.Length)];
		float value4 = Random.value;
		bandit_camp_data.TryAdd("BOSS_NAME", randomBossNameFromFaction);
		bandit_camp_data.TryAdd("BOSS_ANIMAL_A", biome_mob_a);
		bandit_camp_data.TryAdd("BOSS_ANIMAL_B", biome_mob_b);
		bandit_camp_data.TryAdd("BOSS_WEAPON_MODEL", value);
		bandit_camp_data.TryAdd("BOSS_ARMOR_MODEL", value2);
		bandit_camp_data.TryAdd("BOSS_HELMET_MODEL", value3);
		bandit_camp_data.TryAdd("BOSS_WEAPON_DUAL_WIELD", (value4 < 0.5f) ? "1" : "0");
		SaveToDisk();
	}
}
