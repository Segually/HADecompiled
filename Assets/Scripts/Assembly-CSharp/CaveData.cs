using System.Collections.Generic;

public class CaveData
{
	public List<UngeneratedCaveChunk> ungenerated_floors = new List<UngeneratedCaveChunk>();

	public UngeneratedCaveChunk GetUngeneratedFloorAt(int X, int Z)
	{
		foreach (UngeneratedCaveChunk floor in ungenerated_floors)
		{
			if (floor.chunkX == X && floor.chunkZ == Z) return floor;
		}
		return new UngeneratedCaveChunk { chunkX = X, chunkZ = Z };
	}

	public void SaveToDisk(string zone)
	{
		string file = zone + "-cavedata";
		PlayerData.Instance.SetSlotShort("n_ungenerated_floors", ungenerated_floors.Count, file);
		for (int i = 0; i < ungenerated_floors.Count; i++)
		{
			UngeneratedCaveChunk floor = ungenerated_floors[i];
			string key = "ungenerated_floor_" + i;
			PlayerData.Instance.SetSlotShort(key + "_chunkX", floor.chunkX, file);
			PlayerData.Instance.SetSlotShort(key + "_chunkZ", floor.chunkZ, file);
			PlayerData.Instance.SetSlotShort(key + "_floorModel", floor.floor_model, file);
			PlayerData.Instance.SetSlotShort(key + "_floorRot", floor.floor_rotation, file);
			if (floor.has_fossil) PlayerData.Instance.SetSlotShort(key + "_hasFossil", 1, file);
			if (floor.has_caveart)
			{
				PlayerData.Instance.SetSlotShort(key + "_caveartExists", 1, file);
				PlayerData.Instance.SetSlotShort(key + "_caveartRot", floor.caveart_rot, file);
				PlayerData.Instance.SetSlotShort(key + "_caveartTex", floor.caveart_tex_ind, file);
			}
			if (floor.add_spikes) PlayerData.Instance.SetSlotShort(key + "_spikes", 1, file);
			PlayerData.Instance.SetSlotString(key + "_smallOre", floor.small_ore, file);
			PlayerData.Instance.SetSlotString(key + "_largeOre", floor.large_ore, file);
			PlayerData.Instance.SetSlotShort(key + "_smallBudget", floor.small_budget, file);
			PlayerData.Instance.SetSlotShort(key + "_largeBudget", floor.large_budget, file);
		}
	}

	public static CaveData LoadFromDisk(string zone)
	{
		CaveData data = new CaveData();
		string file = zone + "-cavedata";
		int count = PlayerData.Instance.GetSlotShort("n_ungenerated_floors", file);
		for (int i = 0; i < count; i++)
		{
			string key = "ungenerated_floor_" + i;
			UngeneratedCaveChunk floor = new UngeneratedCaveChunk();
			floor.chunkX = PlayerData.Instance.GetSlotShort(key + "_chunkX", file);
			floor.chunkZ = PlayerData.Instance.GetSlotShort(key + "_chunkZ", file);
			floor.floor_model = (byte)PlayerData.Instance.GetSlotShort(key + "_floorModel", file);
			floor.floor_rotation = (byte)PlayerData.Instance.GetSlotShort(key + "_floorRot", file);
			floor.has_fossil = PlayerData.Instance.GetSlotShort(key + "_hasFossil", file) == 1;
			floor.has_caveart = PlayerData.Instance.GetSlotShort(key + "_caveartExists", file) == 1;
			if (floor.has_caveart)
			{
				floor.caveart_rot = (byte)PlayerData.Instance.GetSlotShort(key + "_caveartRot", file);
				floor.caveart_tex_ind = PlayerData.Instance.GetSlotShort(key + "_caveartTex", file);
			}
			floor.add_spikes = PlayerData.Instance.GetSlotShort(key + "_spikes", file) == 1;
			floor.small_ore = PlayerData.Instance.GetSlotString(key + "_smallOre", file);
			floor.large_ore = PlayerData.Instance.GetSlotString(key + "_largeOre", file);
			floor.small_budget = PlayerData.Instance.GetSlotShort(key + "_smallBudget", file);
			floor.large_budget = PlayerData.Instance.GetSlotShort(key + "_largeBudget", file);
			data.ungenerated_floors.Add(floor);
		}
		return data;
	}
}
