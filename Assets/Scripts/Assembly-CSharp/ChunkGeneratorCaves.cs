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
			dig_energy = energy;
			this.curr_chunkX = curr_chunkX;
			this.curr_chunkZ = curr_chunkZ;
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
			this.chunkX = chunkX;
			this.chunkZ = chunkZ;
			this.left_wall_up = left_wall_up;
			this.right_wall_up = right_wall_up;
			this.bottom_wall_up = bottom_wall_up;
			this.top_wall_up = top_wall_up;
		}
	}

	public static void GenerateUnexplored(ChunkData chunk_data, int chunkX, int chunkZ, string zone, InventoryItem house_item, int override_floormodel, int override_rot, UngeneratedCaveChunk ungenerated_floor, Dictionary<string, ZoneData> auto_built_zones)
	{
		ChunkControl.cave_define cave = ChunkControl.Instance.GetCorrespondingCave(house_item.item_name);
		int model = override_floormodel == -1 ? ungenerated_floor.floor_model : override_floormodel;
		int rotation = override_rot == -1 ? ungenerated_floor.floor_rotation : override_rot;
		chunk_data.floor_model_id = model;
		chunk_data.floor_rotation = rotation;
		bool[,] filled = new bool[10, 10];
		List<Vector2> positions = new List<Vector2>();
		System.Action<int, int> wall = (x, z) =>
		{
			chunk_data.AddElement(x, z, new ChunkElement("DEBUG-1x1-fillempty"));
			filled[x, z] = true;
		};
		switch (model)
		{
			case 1:
				for (int i = 0; i < 10; i++)
				{
					switch (rotation)
					{
						case 0: wall(0, i); wall(9, i); wall(i, 0); break;
						case 1: wall(i, 0); wall(i, 9); wall(0, i); break;
						case 2: wall(0, i); wall(9, i); wall(i, 9); break;
						case 3: wall(i, 0); wall(i, 9); wall(9, i); break;
					}
				}
				break;
			case 2:
				if (rotation == 0 || rotation == 2)
				{
					for (int i = 0; i < 10; i++)
					{
						wall(i, 0); wall(i, 9);
						positions.Add(new Vector2(i, 1)); positions.Add(new Vector2(i, 2));
						positions.Add(new Vector2(i, 7)); positions.Add(new Vector2(i, 8));
					}
				}
				else if (rotation == 1 || rotation == 3)
				{
					for (int i = 0; i < 10; i++)
					{
						wall(0, i); wall(9, i);
						positions.Add(new Vector2(1, i)); positions.Add(new Vector2(2, i));
						positions.Add(new Vector2(7, i)); positions.Add(new Vector2(8, i));
					}
				}
				break;
			case 3:
				switch (rotation)
				{
					case 0:
						for (int i = 0; i < 10; i++) { wall(0, i); wall(i, 0); }
						wall(9, 9);
						for (int i = 1; i < 10; i++)
						{
							positions.Add(new Vector2(1, i)); positions.Add(new Vector2(2, i));
							positions.Add(new Vector2(i, 1)); positions.Add(new Vector2(i, 2));
						}
						FillTopRightCorner(positions);
						break;
					case 1:
						for (int i = 0; i < 10; i++) { wall(0, i); wall(i, 9); }
						wall(9, 0);
						for (int i = 0; i < 9; i++)
						{
							positions.Add(new Vector2(1, i)); positions.Add(new Vector2(2, i));
							positions.Add(new Vector2(i + 1, 7)); positions.Add(new Vector2(i + 1, 8));
						}
						FillBottomRightCorner(positions);
						break;
					case 2:
						for (int i = 0; i < 10; i++) { wall(i, 9); wall(9, i); }
						wall(0, 0);
						for (int i = 0; i < 9; i++)
						{
							positions.Add(new Vector2(i, 7)); positions.Add(new Vector2(i, 8));
							positions.Add(new Vector2(7, i)); positions.Add(new Vector2(8, i));
						}
						FillBottomLeftCorner(positions);
						break;
					case 3:
						for (int i = 0; i < 10; i++) { wall(i, 0); wall(9, i); }
						wall(0, 9);
						for (int i = 0; i < 9; i++)
						{
							positions.Add(new Vector2(i, 1)); positions.Add(new Vector2(i, 2));
							positions.Add(new Vector2(7, i + 1)); positions.Add(new Vector2(8, i + 1));
						}
						FillTopLeftCorner(positions);
						break;
				}
				break;
			case 4:
				switch (rotation)
				{
					case 0:
						for (int i = 0; i < 10; i++)
						{
							wall(0, i); positions.Add(new Vector2(1, i)); positions.Add(new Vector2(2, i));
						}
						wall(9, 0); wall(9, 9);
						FillBottomRightCorner(positions); FillTopRightCorner(positions);
						break;
					case 1:
						for (int i = 0; i < 10; i++)
						{
							wall(i, 9); positions.Add(new Vector2(i, 7)); positions.Add(new Vector2(i, 8));
						}
						wall(0, 0); wall(9, 0);
						FillBottomLeftCorner(positions); FillBottomRightCorner(positions);
						break;
					case 2:
						for (int i = 0; i < 10; i++)
						{
							wall(9, i); positions.Add(new Vector2(7, i)); positions.Add(new Vector2(8, i));
						}
						wall(0, 0); wall(0, 9);
						FillTopLeftCorner(positions); FillBottomLeftCorner(positions);
						break;
					case 3:
						for (int i = 0; i < 10; i++)
						{
							wall(i, 0); positions.Add(new Vector2(i, 1)); positions.Add(new Vector2(i, 2));
						}
						wall(0, 9); wall(9, 9);
						FillTopRightCorner(positions); FillTopLeftCorner(positions);
						break;
				}
				break;
			case 5: wall(0, 0); wall(0, 9); wall(9, 0); wall(9, 9); break;
		}
		if (ungenerated_floor.has_fossil)
		{
			int x = Random.Range(1, 8);
			int z = Random.Range(1, 8);
			chunk_data.AddElement(x, z, new ChunkElement("Spawner - Fossils"));
			filled[x, z] = true;
		}
		if (ungenerated_floor.has_caveart) chunk_data.floor_texture_index = ungenerated_floor.caveart_tex_ind + (ungenerated_floor.caveart_rot == 1 ? 101 : 1);
		if (model == 1)
		{
			foreach (CaveDeadEndObject decoration in GetCaveDeadEndVariation(zone))
			{
				int x = 0;
				int z = 0;
				int rot = decoration.rot;
				switch (rotation)
				{
					case 0: x = decoration.x; z = decoration.z; break;
					case 1: x = decoration.z; z = decoration.x; rot++; break;
					case 2: x = decoration.x; z = 9 - decoration.z; rot += 2; break;
					case 3: x = 9 - decoration.z; z = decoration.x; rot += 3; break;
				}
				if (rot > 3) rot -= 4;
				string name = decoration.element.item.item_name;
				InventoryItem item;
				if (name == "Cave Basket" || name == "Cave Chest")
				{
					ExtraInventoryData extra = new ExtraInventoryData();
					extra.SetLong("basket_id", ConstructionControl.Instance.GetNewUniqueId());
					item = new InventoryItem(name, extra);
				}
				else item = new InventoryItem(name);
				chunk_data.AddElement(x, z, new ChunkElement(item, rot));
				filled[x, z] = true;
			}
		}
		if (positions.Count > 0)
		{
			if (!Startup.StringNullOrWhitespace(ungenerated_floor.small_ore)) PlaceCaveObjects(ungenerated_floor.small_ore, ungenerated_floor.small_budget, cave, positions, filled, chunk_data);
			if (!Startup.StringNullOrWhitespace(ungenerated_floor.large_ore)) PlaceCaveObjects(ungenerated_floor.large_ore, ungenerated_floor.large_budget, cave, positions, filled, chunk_data);
			if (!Startup.StringNullOrWhitespace(cave.shroom_obj_name))
			{
				int count = Random.Range(0, cave.shroom_density);
				for (int i = 0; i < count; i++)
				{
					for (int attempt = 0; attempt < 9; attempt++)
					{
						int index = Random.Range(0, positions.Count);
						int x = (int)positions[index].x;
						int z = (int)positions[index].y;
						if (filled[x, z] || !chunk_data.EmptyAt(x, z)) continue;
						if (!Startup.StringNullOrWhitespace(cave.giant_shroom_obj_name) && (double)Random.value < 0.333)
						{
							chunk_data.AddElement(x, z, new ChunkElement(cave.giant_shroom_obj_name));
							int remaining = 2;
							for (int nearbyAttempt = 0; nearbyAttempt < 5; nearbyAttempt++)
							{
								int dx = Random.Range(-1, 1);
								int dz = Random.Range(-1, 1);
								int nextX = x + dx;
								int nextZ = z + dz;
								if (nextX < 0 || nextZ < 0 || nextX >= 10 || nextZ >= 10 || (dx == 0 && dz == 0) || filled[nextX, nextZ] || !chunk_data.EmptyAt(x, z)) continue;
								chunk_data.AddElement(nextX, nextZ, new ChunkElement(cave.shroom_obj_name));
								filled[nextX, nextZ] = true;
								if (--remaining == 0) break;
							}
						}
						else chunk_data.AddElement(x, z, new ChunkElement(cave.shroom_obj_name));
						filled[x, z] = true;
						break;
					}
				}
			}
			if (!Startup.StringNullOrWhitespace(cave.stalagmite_name) && !Startup.StringNullOrWhitespace(cave.small_stalagmite_name) && cave.stalagmite_density != 0)
			{
				int remaining = cave.stalagmite_density;
				for (int attempt = 0; attempt < 9; attempt++)
				{
					int index = Random.Range(0, positions.Count);
					int x = (int)positions[index].x;
					int z = (int)positions[index].y;
					if (filled[x, z] || !chunk_data.EmptyAt(x, z)) continue;
					string name = (double)Random.value < 0.5 ? cave.stalagmite_name : cave.small_stalagmite_name;
					chunk_data.AddElement(x, z, new ChunkElement(name, Random.Range(0, 4)));
					filled[x, z] = true;
					if (--remaining == 0) break;
				}
			}
		}
		if (model != 0 && ungenerated_floor.add_spikes)
		{
			for (int attempt = 0; attempt < 9; attempt++)
			{
				int x = Random.Range(0, 10);
				int z = Random.Range(0, 10);
				if (filled[x, z] || !chunk_data.EmptyAt(x, z)) continue;
				chunk_data.AddElement(x, z, new ChunkElement("Poison Spikes"));
				filled[x, z] = true;
				break;
			}
		}
	}

	public static CaveData GenerateNewCaveSystem(string zone, int originX, int originZ, InventoryItem zone_house_item)
	{
		CaveData data = new CaveData();
		ChunkControl.cave_define cave = ChunkControl.Instance.GetCorrespondingCave(zone_house_item.item_name);
		List<CaveSprawler> sprawlers = new List<CaveSprawler>
		{
			new CaveSprawler(5, originX, originZ), new CaveSprawler(5, originX, originZ), new CaveSprawler(5, originX, originZ)
		};
		Dictionary<string, DigSpot> spots = new Dictionary<string, DigSpot>();
		spots.Add(originX + "," + originZ, new DigSpot(originX, originZ, true, true, true, true));
		for (int iteration = 0; iteration < 100; iteration++)
		{
			bool digging = false;
			foreach (CaveSprawler sprawler in sprawlers)
			{
				if (sprawler.dig_energy == 0) continue;
				digging = true;
				dir direction = (dir)Random.Range(0, 4);
				DigSpot current = spots[sprawler.curr_chunkX + "," + sprawler.curr_chunkZ];
				int x = sprawler.curr_chunkX;
				int z = sprawler.curr_chunkZ;
				switch (direction)
				{
					case dir.left:
						if (current.left_wall_up) sprawler.dig_energy--;
						current.left_wall_up = false;
						x--;
						break;
					case dir.right:
						if (current.right_wall_up) sprawler.dig_energy--;
						current.right_wall_up = false;
						x++;
						break;
					case dir.up:
						if (current.top_wall_up) sprawler.dig_energy--;
						current.top_wall_up = false;
						z++;
						break;
					case dir.down:
						if (current.bottom_wall_up) sprawler.dig_energy--;
						current.bottom_wall_up = false;
						z--;
						break;
				}
				string key = x + "," + z;
				if (!spots.ContainsKey(key)) spots.Add(key, new DigSpot(x, z, true, true, true, true));
				DigSpot next = spots[key];
				switch (direction)
				{
					case dir.left: next.right_wall_up = false; break;
					case dir.right: next.left_wall_up = false; break;
					case dir.up: next.bottom_wall_up = false; break;
					case dir.down: next.top_wall_up = false; break;
				}
				sprawler.curr_chunkX = x;
				sprawler.curr_chunkZ = z;
			}
			if (!digging) break;
		}
		int fossilIndex = Random.Range(0, spots.Count);
		DigSpot fossil = null;
		foreach (DigSpot spot in spots.Values)
		{
			if (fossilIndex-- == 0) { fossil = spot; break; }
		}
		List<Vector2> leftWalls = new List<Vector2>();
		List<Vector2> topWalls = new List<Vector2>();
		foreach (DigSpot spot in spots.Values)
		{
			if (spot.left_wall_up) leftWalls.Add(new Vector2(spot.chunkX, spot.chunkZ));
			if (spot.top_wall_up) topWalls.Add(new Vector2(spot.chunkX, spot.chunkZ));
		}
		float artDirection = Random.value;
		List<Vector2> artWalls = (double)artDirection < 0.5 ? leftWalls : topWalls;
		Vector2 art = artWalls[Random.Range(0, artWalls.Count)];
		int artTexture = Random.Range(0, ChunkControl.Instance.cave_art_textures_path.Length);
		float spikes = Random.value;
		string smallOre = "";
		string largeOre = "";
		if (cave.possible_mineral_pairs.Length != 0)
		{
			ChunkControl.cave_mineral_pairs pair = cave.possible_mineral_pairs[Random.Range(0, cave.possible_mineral_pairs.Length)];
			smallOre = pair.small_mineral;
			largeOre = pair.large_mineral;
		}
		if ((double)Random.value < 0.03 && cave.possible_SUPER_RARE_minerals.Length != 0)
		{
			ChunkControl.cave_mineral_pairs pair = cave.possible_SUPER_RARE_minerals[Random.Range(0, cave.possible_SUPER_RARE_minerals.Length)];
			smallOre = pair.small_mineral;
			largeOre = pair.large_mineral;
		}
		string smallGem = "";
		string largeGem = "";
		if (cave.possible_gem_pairs.Length != 0)
		{
			ChunkControl.cave_mineral_pairs pair = cave.possible_gem_pairs[Random.Range(0, cave.possible_gem_pairs.Length)];
			smallGem = pair.small_mineral;
			largeGem = pair.large_mineral;
		}
		foreach (DigSpot spot in spots.Values)
		{
			UngeneratedCaveChunk floor = new UngeneratedCaveChunk();
			floor.chunkX = spot.chunkX;
			floor.chunkZ = spot.chunkZ;
			floor.has_fossil = spot.chunkX == fossil.chunkX && spot.chunkZ == fossil.chunkZ;
			if (spot.chunkX == (int)art.x && spot.chunkZ == (int)art.y)
			{
				floor.has_caveart = true;
				floor.caveart_tex_ind = artTexture;
				floor.caveart_rot = (byte)((double)artDirection < 0.5 ? 0 : 1);
			}
			floor.add_spikes = (double)spikes < 0.1;
			if ((double)Random.value < 0.444)
			{
				bool gem = (double)Random.value < 0.23;
				floor.small_ore = gem ? smallGem : smallOre;
				floor.large_ore = gem ? largeGem : largeOre;
				if (!gem && cave.sparse_minerals && (double)Random.value >= 0.62)
				{
					if ((double)Random.value < 0.62)
					{
						floor.small_budget = 1;
						floor.large_budget = 1;
					}
					else if ((double)Random.value < 0.62) floor.small_budget = 2;
				}
				else
				{
					floor.small_budget = 2;
					floor.large_budget = 1;
				}
			}
			else if ((double)Random.value < 0.444)
			{
				bool gem = (double)Random.value < 0.23;
				floor.small_ore = gem ? smallGem : smallOre;
				if (!gem && cave.sparse_minerals && (double)Random.value >= 0.62) floor.small_budget = (double)Random.value < 0.62 ? 1 : 0;
				else floor.small_budget = 2;
			}
			else if ((double)Random.value < 0.444)
			{
				bool gem = (double)Random.value < 0.23;
				floor.small_ore = gem ? smallGem : smallOre;
				floor.small_budget = !gem && cave.sparse_minerals ? ((double)Random.value < 0.62 ? 1 : 0) : 1;
			}
			string walls = (spot.left_wall_up ? "1" : "0") + (spot.top_wall_up ? "1" : "0") + (spot.right_wall_up ? "1" : "0") + (spot.bottom_wall_up ? "1" : "0");
			switch (walls)
			{
				case "1011": floor.floor_model = 1; floor.floor_rotation = 0; break;
				case "1101": floor.floor_model = 1; floor.floor_rotation = 1; break;
				case "1110": floor.floor_model = 1; floor.floor_rotation = 2; break;
				case "0111": floor.floor_model = 1; floor.floor_rotation = 3; break;
				case "1010": floor.floor_model = 2; floor.floor_rotation = (byte)((double)Random.value < 0.5 ? 1 : 3); break;
				case "0101": floor.floor_model = 2; floor.floor_rotation = (byte)((double)Random.value < 0.5 ? 0 : 2); break;
				case "1001": floor.floor_model = 3; floor.floor_rotation = 0; break;
				case "1100": floor.floor_model = 3; floor.floor_rotation = 1; break;
				case "0110": floor.floor_model = 3; floor.floor_rotation = 2; break;
				case "0011": floor.floor_model = 3; floor.floor_rotation = 3; break;
				case "1000": floor.floor_model = 4; floor.floor_rotation = 0; break;
				case "0100": floor.floor_model = 4; floor.floor_rotation = 1; break;
				case "0010": floor.floor_model = 4; floor.floor_rotation = 2; break;
				case "0001": floor.floor_model = 4; floor.floor_rotation = 3; break;
				case "0000": floor.floor_model = 5; floor.floor_rotation = (byte)Random.Range(0, 4); break;
			}
			data.ungenerated_floors.Add(floor);
		}
		return data;
	}

	private static List<CaveDeadEndObject> GetCaveDeadEndVariation(string zone)
	{
		List<CaveDeadEndObject> objects = new List<CaveDeadEndObject>();
		switch (Random.Range(0, 14))
		{
			case 2:
			case 3:
				objects.Add(new CaveDeadEndObject("Cave Chest", 5, 2, 3));
				objects.Add(new CaveDeadEndObject("Old Torch", 2, 7, 0));
				objects.Add(new CaveDeadEndObject("Cave Basket", 7, 3, 0));
				break;
			case 4:
			case 5:
				objects.Add(new CaveDeadEndObject("Old Torch", 7, 5, 0));
				objects.Add(new CaveDeadEndObject("Cave Basket", 6, 2, 0));
				objects.Add(new CaveDeadEndObject("Cave Basket", 4, 2, 0));
				objects.Add(new CaveDeadEndObject("Cave Basket", 2, 3, 0));
				break;
			case 6:
			case 7:
				objects.Add(new CaveDeadEndObject("Old Torch", 1, 5, 0));
				objects.Add(new CaveDeadEndObject("Cave Chest", 1, 4, 0));
				objects.Add(new CaveDeadEndObject("Old Torch", 8, 5, 0));
				objects.Add(new CaveDeadEndObject("Cave Chest", 8, 4, 2));
				objects.Add(new CaveDeadEndObject("Spawner - Bones", 3, 2, 0));
				break;
			case 8:
			case 9:
				objects.Add(new CaveDeadEndObject("Cave Chest", 5, 5, 3));
				objects.Add(new CaveDeadEndObject("Old Torch", 2, 8, 0));
				break;
			case 10:
			case 11:
				objects.Add(new CaveDeadEndObject("Cave Basket", 2, 2, 0));
				objects.Add(new CaveDeadEndObject("Cave Basket", 2, 3, 0));
				objects.Add(new CaveDeadEndObject("Cave Basket", 3, 1, 0));
				objects.Add(new CaveDeadEndObject("Cave Basket", 4, 1, 0));
				objects.Add(new CaveDeadEndObject("Old Torch", 7, 2, 0));
				objects.Add(new CaveDeadEndObject("Spawner - Bones", 7, 4, 0));
				break;
			case 12:
			case 13:
				objects.Add(new CaveDeadEndObject("Spawner - Bones", 6, 4, 0));
				objects.Add(new CaveDeadEndObject((double)Random.value < 0.1 ? "Spawner - Ancient Bones" : "Spawner - Bones", 5, 6, 0));
				objects.Add(new CaveDeadEndObject("Spawner - Bones", 4, 3, 0));
				objects.Add(new CaveDeadEndObject("Spawner - Bones", 3, 5, 0));
				break;
		}
		return objects;
	}

	private static void PlaceCaveObjects(string obj_name, int n_to_place, ChunkControl.cave_define corresponding_Cave, List<Vector2> allowable_build_locations, bool[,] filled, ChunkData chunk_data, bool log = false)
	{
		for (int i = 0; i < n_to_place; i++)
		{
			for (int attempt = 0; attempt < 10; attempt++)
			{
				Vector2 location = allowable_build_locations[Random.Range(0, allowable_build_locations.Count)];
				int x = (int)location.x;
				int z = (int)location.y;
				List<Vector3> geometry = ConstructionControl.Instance.GetObjectLocalGeometry(obj_name, 0);
				bool valid = true;
				foreach (Vector3 offset in geometry)
				{
					int tileX = (int)(offset.x + x);
					int tileZ = (int)(offset.z + z);
					if (tileX < 0 || tileZ < 0 || tileX >= 10 || tileZ >= 10 || filled[tileX, tileZ])
					{
						valid = false;
						break;
					}
				}
				if (!valid) continue;
				chunk_data.AddElement(x, z, new ChunkElement(obj_name));
				foreach (Vector3 offset in geometry) filled[(int)(offset.x + x), (int)(offset.z + z)] = true;
				break;
			}
		}
	}

	private static void FillBottomRightCorner(List<Vector2> allowable_build_locations)
	{
		allowable_build_locations.Add(new Vector2(7, 0));
		allowable_build_locations.Add(new Vector2(8, 0));
		allowable_build_locations.Add(new Vector2(7, 1));
		allowable_build_locations.Add(new Vector2(8, 1));
		allowable_build_locations.Add(new Vector2(9, 1));
		allowable_build_locations.Add(new Vector2(7, 2));
		allowable_build_locations.Add(new Vector2(8, 2));
		allowable_build_locations.Add(new Vector2(9, 2));
	}

	private static void FillBottomLeftCorner(List<Vector2> allowable_build_locations)
	{
		allowable_build_locations.Add(new Vector2(1, 0));
		allowable_build_locations.Add(new Vector2(2, 0));
		allowable_build_locations.Add(new Vector2(0, 1));
		allowable_build_locations.Add(new Vector2(1, 1));
		allowable_build_locations.Add(new Vector2(2, 1));
		allowable_build_locations.Add(new Vector2(0, 2));
		allowable_build_locations.Add(new Vector2(1, 2));
		allowable_build_locations.Add(new Vector2(2, 2));
	}

	private static void FillTopRightCorner(List<Vector2> allowable_build_locations)
	{
		allowable_build_locations.Add(new Vector2(7, 9));
		allowable_build_locations.Add(new Vector2(8, 9));
		allowable_build_locations.Add(new Vector2(7, 8));
		allowable_build_locations.Add(new Vector2(8, 8));
		allowable_build_locations.Add(new Vector2(9, 8));
		allowable_build_locations.Add(new Vector2(7, 7));
		allowable_build_locations.Add(new Vector2(8, 7));
		allowable_build_locations.Add(new Vector2(9, 7));
	}

	private static void FillTopLeftCorner(List<Vector2> allowable_build_locations)
	{
		allowable_build_locations.Add(new Vector2(1, 9));
		allowable_build_locations.Add(new Vector2(2, 9));
		allowable_build_locations.Add(new Vector2(0, 8));
		allowable_build_locations.Add(new Vector2(1, 8));
		allowable_build_locations.Add(new Vector2(2, 8));
		allowable_build_locations.Add(new Vector2(0, 7));
		allowable_build_locations.Add(new Vector2(1, 7));
		allowable_build_locations.Add(new Vector2(2, 7));
	}

	public static void AsyncAssignTerrainTex(int floor_model_id, ChunkControl.cave_define cave_biome, GameObject floor_model_obj)
	{
		string wall;
		string floor;
		string fallback;
		switch (floor_model_id)
		{
			case 1: wall = cave_biome.overwrite_walltex_deadend_path; floor = cave_biome.overwrite_floortex_deadend_path; fallback = "generic-cave-floor-deadend"; break;
			case 2: wall = cave_biome.overwrite_walltex_straight_path; floor = cave_biome.overwrite_floortex_straight_path; fallback = "generic-cave-floor-straight"; break;
			case 3: wall = cave_biome.overwrite_walltex_hook_path; floor = cave_biome.overwrite_floortex_hook_path; fallback = "generic-cave-floor-hook"; break;
			case 4: wall = cave_biome.overwrite_walltex_3way_path; floor = cave_biome.overwrite_floortex_3way_path; fallback = "generic-cave-floor-3way"; break;
			case 5: wall = cave_biome.overwrite_walltex_4way_path; floor = cave_biome.overwrite_floortex_4way_path; fallback = "generic-cave-floor-4way"; break;
			default: return;
		}
		ResourceControl.Instance.AssignCaveWallTexture(Startup.StringNullOrWhitespace(wall) ? "generic-cave-wall" : wall, floor_model_obj.transform.Find("walls").GetComponent<MeshRenderer>());
		ResourceControl.Instance.AssignCaveWallTexture(Startup.StringNullOrWhitespace(floor) ? fallback : floor, floor_model_obj.transform.Find("floor").GetComponent<MeshRenderer>());
	}
}
