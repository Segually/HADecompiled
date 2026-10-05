using System.Collections.Generic;
using UnityEngine;

public class ChunkGeneratorCaves
{
	public enum dir
	{
		left = 0,
		right = 1,
		up = 2,
		down = 3
	}

	public class CaveSprawler
	{
		public int dig_energy;

		public int curr_chunkX;

		public int curr_chunkZ;

		public CaveSprawler(int energy, int curr_chunkX, int curr_chunkZ)
		{
		}
	}

	public class DigSpot
	{
		public int chunkX;

		public int chunkZ;

		public bool left_wall_up;

		public bool right_wall_up;

		public bool bottom_wall_up;

		public bool top_wall_up;

		public DigSpot(int chunkX, int chunkZ, bool left_wall_up, bool right_wall_up, bool bottom_wall_up, bool top_wall_up)
		{
		}
	}

	public static void GenerateUnexplored(ChunkData chunk_data, int chunkX, int chunkZ, string zone, InventoryItem house_item, int override_floormodel, int override_rot, UngeneratedCaveChunk ungenerated_floor, Dictionary<string, ZoneData> auto_built_zones)
	{
	}

	public static CaveData GenerateNewCaveSystem(string zone, int originX, int originZ, InventoryItem zone_house_item)
	{
		return null;
	}

	private static List<CaveDeadEndObject> GetCaveDeadEndVariation(string zone)
	{
		return null;
	}

	private static void PlaceCaveObjects(string obj_name, int n_to_place, ChunkControl.cave_define corresponding_Cave, List<Vector2> allowable_build_locations, bool[,] filled, ChunkData chunk_data, bool log = false)
	{
	}

	private static void FillBottomRightCorner(List<Vector2> allowable_build_locations)
	{
	}

	private static void FillBottomLeftCorner(List<Vector2> allowable_build_locations)
	{
	}

	private static void FillTopRightCorner(List<Vector2> allowable_build_locations)
	{
	}

	private static void FillTopLeftCorner(List<Vector2> allowable_build_locations)
	{
	}

	public static void AsyncAssignTerrainTex(int floor_model_id, ChunkControl.cave_define cave_biome, GameObject floor_model_obj)
	{
	}
}
