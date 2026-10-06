using System.Collections.Generic;
using UnityEngine;

public class ChunkGeneratorClouds
{
	public static void GenerateUnexplored(ChunkData chunk_data, int X, int Z, string zone, InventoryItem house_item, int origin_chunkX, int origin_chunkZ, int origin_innerX, int origin_innerZ, Dictionary<string, ZoneData> auto_built_zones)
	{
		chunk_data.biome_mobA = "angel";
		chunk_data.biome_mobB = "angel";
		if (!DevBuildControl.Instance.IsDevPlacedShackZone(zone)) TryAutoBuildStartArea(house_item, chunk_data, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, zone, auto_built_zones);
		int count = Random.Range(2, 5);
		for (int i = 0; i < count; i++)
		{
			int x = Random.Range(0, 10);
			int z = Random.Range(0, 10);
			foreach (Vector2 offset in new Vector2[]
			{
				new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1),
				new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(-1, -1),
				new Vector2(2, 0), new Vector2(0, 2), new Vector2(-2, 0), new Vector2(0, -2),
				new Vector2(2, 2), new Vector2(2, -2), new Vector2(-2, 2), new Vector2(-2, -2),
				new Vector2(1, 2), new Vector2(-1, 2), new Vector2(1, -2), new Vector2(-1, -2),
				new Vector2(2, 1), new Vector2(2, -1), new Vector2(-2, 1), new Vector2(-2, -1)
			}) try_fill_cloud(x + (int)offset.x, z + (int)offset.y, ref chunk_data);
		}
		if ((double)Random.value >= 0.2) return;
		List<Vector2> positions = new List<Vector2>();
		for (int x = 0; x < 10; x++)
		{
			for (int z = 0; z < 10; z++)
			{
				if (!chunk_data.EmptyAt(x, z)) positions.Add(new Vector2(x, z));
			}
		}
		int mobs = Random.Range(1, 2);
		float depth = GameController.Instance.DepthAt(new Vector3(X * 10f + 5f, 0f, Z * 10f + 5f));
		for (int i = 0; i < mobs; i++)
		{
			int index = Random.Range(0, positions.Count);
			Vector2 position = positions[index];
			positions.RemoveAt(index);
			MobControl.PlaceMob((int)position.x, (int)position.y, chunk_data, depth);
		}
	}

	private static void try_fill_cloud(int x, int z, ref ChunkData chunk_data)
	{
		if (x < 0 || z < 0 || x > 9 || z > 9 || !chunk_data.EmptyAt(x, z)) return;
		chunk_data.AddElement(x, z, new ChunkElement("Clouds"));
	}

	private static void TryAutoBuildStartArea(InventoryItem house_item, ChunkData chunk_data, int origin_chunkX, int origin_chunkZ, int origin_innerX, int origin_innerZ, string zone, Dictionary<string, ZoneData> auto_built_zones)
	{
		for (int x = -5; x <= 5; x++)
		{
			for (int z = -5; z <= 5; z++)
			{
				if ((x != 0 || z != 0) && !(Mathf.Abs(x) == 5 && Mathf.Abs(z) == 5)) chunk_data.TryAutoBuildAt("Clouds", x, z, 0, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, zone, auto_built_zones);
			}
		}
		if (house_item.GetShort("dont_build_start_area") != 0) return;
		foreach (Vector2 position in new List<Vector2> { new Vector2(-4, -2), new Vector2(-4, 0), new Vector2(-4, 2) })
		{
			chunk_data.TryAutoBuildAt("Sky Chest", (int)position.x, (int)position.y, 0, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, zone, auto_built_zones);
		}
		List<Vector2> paths = new List<Vector2>
		{
			new Vector2(1, 1), new Vector2(1, 0), new Vector2(1, -1),
			new Vector2(0, 1), new Vector2(0, -1),
			new Vector2(-1, 1), new Vector2(-1, 0), new Vector2(-1, -1), new Vector2(-2, 0)
		};
		for (int z = -2; z <= 2; z++)
		{
			paths.Add(new Vector2(-3, z));
			paths.Add(new Vector2(-4, z));
		}
		foreach (Vector2 position in paths)
		{
			chunk_data.TryAutoBuildAt("Cobblestone Path", (int)position.x, (int)position.y, 0, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, zone, auto_built_zones);
		}
	}
}
