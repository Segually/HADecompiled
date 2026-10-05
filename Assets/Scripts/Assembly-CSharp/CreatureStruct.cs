using System.Collections.Generic;

public class CreatureStruct
{
	public string combat_name;

	public List<string> creatures;

	public int critterLevel;

	public float critterSize;

	public SharedCreature.brain_type_t brain_type;

	public float walk_speed;

	public int hp_regen;

	public int hp_max;

	public int hp_curr;

	public float ai_lockon_range;

	public float wander_dist;

	public InventoryItem hat_;

	public InventoryItem body_;

	public InventoryItem hand_;

	public string creature_name;

	public int respawn_seconds;

	public string skin_mat;

	public InventoryItem original_element_item;

	public int original_element_rot;

	public int icon_id;

	public string original_element_zone;

	public int original_element_chunkX;

	public int original_element_chunkZ;

	public int original_element_innerX;

	public int original_element_innerZ;

	public int spawn_offset_x;

	public int spawn_offset_z;

	public static int DEFAULT_CRITTER_SIZE;

	public static SharedCreature.brain_type_t DEFAULT_BRAIN_TYPE;

	public static float DEFAULT_WALK_SPEED;

	public static int DEFAULT_HP_REGEN;

	public static int DEFAULT_HP_MAX;

	public static int DEFAULT_HP_CURR;

	public static float DEFAULT_AI_LOCKON;

	public static float DEFAULT_WANDER_DIST;

	public static int DEFAULT_RESPAWN_SECONDS;

	public static int DEFAULT_ICON_ID;

	public CreatureStruct(string combat_name, List<string> creatures, int critterLevel_, float critterSize_, SharedCreature.brain_type_t brain_type, float walk_speed_, int hp_regen_, int hp_max_, int hp_curr_, float ai_lockon_range_, float wander_dist_, InventoryItem hat_, InventoryItem body_, InventoryItem hand_, string creature_name_, int respawn_seconds_, string skin_mat_, InventoryItem original_element_item_, int original_element_rot_, int icon_id_, string original_element_zone, int original_element_chunkX, int original_element_chunkZ, int original_element_innerX, int original_element_innerZ, int spawn_offset_x, int spawn_offset_z)
	{
	}

	private void ProcessDefaults(float curr_depth)
	{
	}

	public void Pack(Packet outgoing)
	{
	}

	public static CreatureStruct PacketToCreatureStruct(Packet incoming)
	{
		return null;
	}

	public static CreatureStruct GenerateNewWildMob(ChunkData chunk_data, int innerX, int innerZ, float size, float level_mod, InventoryItem item, int rot)
	{
		return null;
	}

	public static CreatureStruct GenerateNestMob(ChunkData chunk_data, int innerX, int innerZ, InventoryItem item, int rot)
	{
		return null;
	}

	public static CreatureStruct GenerateCompanionGhost(InventoryItem companion_item, InventoryItem original_item, ChunkData chunk_data, int rot, int innerX, int innerZ, int spawn_offset_x, int spawn_offset_z)
	{
		return null;
	}

	public static CreatureStruct GenerateHiveMob(ChunkData chunk_data, int innerX, int innerZ, string minicreature, InventoryItem item, int rot, int spawn_offset_x, int spawn_offset_z)
	{
		return null;
	}

	public static CreatureStruct GenerateGuardMob(ChunkData chunk_data, int innerX, int innerZ, string creatureA, string creatureB, InventoryItem hat, InventoryItem body, InventoryItem hand, string companion_name, int level, InventoryItem item, int rot)
	{
		return null;
	}
}
