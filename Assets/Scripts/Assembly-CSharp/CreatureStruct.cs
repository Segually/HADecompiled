using System.Collections.Generic;
using UnityEngine;

public class CreatureStruct
{
	public string combat_name = "";

	public List<string> creatures = new List<string>();

	public int critterLevel;

	public float critterSize;

	public SharedCreature.brain_type_t brain_type;

	public float walk_speed;

	public int hp_regen;

	public int hp_max;

	public int hp_curr;

	public float ai_lockon_range;

	public float wander_dist;

	public InventoryItem hat_ = new InventoryItem("");

	public InventoryItem body_ = new InventoryItem("");

	public InventoryItem hand_ = new InventoryItem("");

	public string creature_name = "";

	public int respawn_seconds;

	public string skin_mat = "";

	public InventoryItem original_element_item;

	public int original_element_rot;

	public int icon_id;

	public string original_element_zone = "";

	public int original_element_chunkX;

	public int original_element_chunkZ;

	public int original_element_innerX;

	public int original_element_innerZ;

	public int spawn_offset_x;

	public int spawn_offset_z;

	public static int DEFAULT_CRITTER_SIZE = -1;

	public static SharedCreature.brain_type_t DEFAULT_BRAIN_TYPE = SharedCreature.brain_type_t.auto_set;

	public static float DEFAULT_WALK_SPEED = -1f;

	public static int DEFAULT_HP_REGEN = -1;

	public static int DEFAULT_HP_MAX = -1;

	public static int DEFAULT_HP_CURR = -1;

	public static float DEFAULT_AI_LOCKON = 3.8f;

	public static float DEFAULT_WANDER_DIST = 5f;

	public static int DEFAULT_RESPAWN_SECONDS = 60;

	public static int DEFAULT_ICON_ID = -1;

	public CreatureStruct(string combat_name, List<string> creatures, int critterLevel_, float critterSize_, SharedCreature.brain_type_t brain_type, float walk_speed_, int hp_regen_, int hp_max_, int hp_curr_, float ai_lockon_range_, float wander_dist_, InventoryItem hat_, InventoryItem body_, InventoryItem hand_, string creature_name_, int respawn_seconds_, string skin_mat_, InventoryItem original_element_item_, int original_element_rot_, int icon_id_, string original_element_zone, int original_element_chunkX, int original_element_chunkZ, int original_element_innerX, int original_element_innerZ, int spawn_offset_x, int spawn_offset_z)
	{
		this.combat_name = combat_name;
		this.creatures = creatures;
		critterSize = critterSize_;
		walk_speed = walk_speed_;
		critterLevel = critterLevel_;
		this.brain_type = brain_type;
		hp_regen = hp_regen_;
		hp_max = hp_max_;
		ai_lockon_range = ai_lockon_range_;
		wander_dist = wander_dist_;
		hp_curr = hp_curr_;
		this.hat_ = hat_;
		this.body_ = body_;
		this.hand_ = hand_;
		creature_name = creature_name_;
		respawn_seconds = respawn_seconds_;
		skin_mat = skin_mat_;
		original_element_item = original_element_item_;
		original_element_rot = original_element_rot_;
		icon_id = icon_id_;
		this.original_element_zone = original_element_zone;
		this.original_element_chunkX = original_element_chunkX;
		this.original_element_chunkZ = original_element_chunkZ;
		this.original_element_innerX = original_element_innerX;
		this.original_element_innerZ = original_element_innerZ;
		this.spawn_offset_x = spawn_offset_x;
		this.spawn_offset_z = spawn_offset_z;
		float curr_depth = GameController.Instance.DepthAt(new Vector3((float)(original_element_chunkX * 10) + (float)original_element_innerX + (float)spawn_offset_x + 0.5f, 1.5f, (float)(original_element_chunkZ * 10) + (float)original_element_innerZ + (float)spawn_offset_z + 0.5f));
		ProcessDefaults(curr_depth);
	}

	private void ProcessDefaults(float curr_depth)
	{
		if (critterSize == (float)DEFAULT_CRITTER_SIZE)
		{
			critterSize = MobControl.Instance.GetCritterSize(critterLevel, curr_depth);
		}
		if (brain_type == DEFAULT_BRAIN_TYPE)
		{
			brain_type = MobControl.Instance.DetermineCreatureBrain(critterLevel, curr_depth);
		}
		if (walk_speed == DEFAULT_WALK_SPEED)
		{
			int num = critterLevel - GameController.Instance.playerLevel;
			if (num < 1)
			{
				walk_speed = 0.046f;
			}
			else
			{
				walk_speed = Mathf.Lerp(0.046f, 0.0485f, (float)num / 15f);
			}
		}
		if (hp_max == DEFAULT_HP_MAX)
		{
			hp_max = CombatControl.GetHpMaxMob(critterLevel);
		}
		if (hp_regen == DEFAULT_HP_REGEN)
		{
			hp_regen = CombatControl.GetHpRegenMob(hp_max);
		}
		if (hp_curr == DEFAULT_HP_CURR)
		{
			hp_curr = hp_max;
		}
		if (icon_id == DEFAULT_ICON_ID)
		{
			switch (brain_type)
			{
			case SharedCreature.brain_type_t.aggressive:
				icon_id = 7;
				break;
			case SharedCreature.brain_type_t.neutral:
				icon_id = 5;
				break;
			case SharedCreature.brain_type_t.fearful:
				icon_id = 6;
				break;
			case SharedCreature.brain_type_t.guard:
				icon_id = 8;
				break;
			case SharedCreature.brain_type_t.ghost:
				icon_id = 9;
				break;
			}
		}
	}

	public void Pack(Packet outgoing)
	{
		outgoing.PutShort(creatures.Count);
		foreach (string creature in creatures) outgoing.PutString(creature);
		outgoing.PutString(combat_name);
		outgoing.PutLong(critterLevel);
		outgoing.PutShort(critterSize * 10f);
		outgoing.PutByte((byte)brain_type);
		outgoing.PutShort(walk_speed * 1000f);
		outgoing.PutLong(hp_regen * 10);
		outgoing.PutLong(hp_max);
		outgoing.PutLong(hp_curr);
		outgoing.PutShort(ai_lockon_range * 10f);
		outgoing.PutShort(wander_dist * 10f);
		hat_.PackForWeb(outgoing);
		body_.PackForWeb(outgoing);
		hand_.PackForWeb(outgoing);
		outgoing.PutString(creature_name);
		outgoing.PutLong(respawn_seconds);
		outgoing.PutString(skin_mat);
		original_element_item.PackForWeb(outgoing);
		outgoing.PutShort(original_element_rot);
		outgoing.PutShort(icon_id);
		outgoing.PutString(original_element_zone);
		outgoing.PutShort(original_element_chunkX);
		outgoing.PutShort(original_element_chunkZ);
		outgoing.PutShort(original_element_innerX);
		outgoing.PutShort(original_element_innerZ);
		outgoing.PutShort(spawn_offset_x);
		outgoing.PutShort(spawn_offset_z);
	}

	public static CreatureStruct PacketToCreatureStruct(Packet incoming)
	{
		List<string> creatures = new List<string>();
		int count = incoming.GetShort();
		for (int i = 0; i < count; i++) creatures.Add(incoming.GetString());
		string id = incoming.GetString();
		int level = incoming.GetLong();
		float size = incoming.GetShort() / 10f;
		SharedCreature.brain_type_t brain = (SharedCreature.brain_type_t)incoming.GetByte();
		float speed = incoming.GetShort() / 1000f;
		int regen = (int)(incoming.GetLong() / 10f);
		int max_hp = incoming.GetLong();
		int hp = incoming.GetLong();
		float lockon = incoming.GetShort() / 10f;
		float wander = incoming.GetShort() / 10f;
		InventoryItem hat = InventoryItem.UnpackFromWeb(incoming);
		InventoryItem body = InventoryItem.UnpackFromWeb(incoming);
		InventoryItem hand = InventoryItem.UnpackFromWeb(incoming);
		string name = incoming.GetString();
		int respawn = incoming.GetLong();
		string skin = incoming.GetString();
		InventoryItem element = InventoryItem.UnpackFromWeb(incoming);
		int rot = incoming.GetShort();
		int icon = incoming.GetShort();
		string zone = incoming.GetString();
		int chunkX = incoming.GetShort();
		int chunkZ = incoming.GetShort();
		int innerX = incoming.GetShort();
		int innerZ = incoming.GetShort();
		int offsetX = incoming.GetShort();
		int offsetZ = incoming.GetShort();
		return new CreatureStruct(id, creatures, level, size, brain, speed, regen, max_hp, hp, lockon, wander, hat, body, hand, name, respawn, skin, element, rot, icon, zone, chunkX, chunkZ, innerX, innerZ, offsetX, offsetZ);
	}

	public static CreatureStruct GenerateNewWildMob(ChunkData chunk_data, int innerX, int innerZ, float size, float level_mod, InventoryItem item, int rot)
	{
		List<string> list = new List<string>();
		list.Add(chunk_data.biome_mobA);
		list.Add(chunk_data.biome_mobB);
		string text = chunk_data.zone + "," + chunk_data.X + "," + chunk_data.Z + "," + innerX + "," + innerZ;
		float curr_depth = GameController.Instance.DepthAt(new Vector3((float)(chunk_data.X * 10) + (float)innerX + 0.5f, 1.5f, (float)(chunk_data.Z * 10) + (float)innerZ + 0.5f));
		int wildCreatureLevel = MobControl.Instance.GetWildCreatureLevel(level_mod, curr_depth);
		return new CreatureStruct(text, list, wildCreatureLevel, size, DEFAULT_BRAIN_TYPE, DEFAULT_WALK_SPEED, DEFAULT_HP_REGEN, DEFAULT_HP_MAX, DEFAULT_HP_CURR, DEFAULT_AI_LOCKON, DEFAULT_WANDER_DIST, new InventoryItem(""), new InventoryItem(""), new InventoryItem(""), "[TRANSLATE]", DEFAULT_RESPAWN_SECONDS, "", item, rot, DEFAULT_ICON_ID, chunk_data.zone, chunk_data.X, chunk_data.Z, innerX, innerZ, 0, 0);
	}

	public static CreatureStruct GenerateNestMob(ChunkData chunk_data, int innerX, int innerZ, InventoryItem item, int rot)
	{
		string biome_mobA = chunk_data.biome_mobA;
		List<string> list = new List<string>();
		list.Add(biome_mobA);
		list.Add(biome_mobA);
		string text = chunk_data.zone + "," + chunk_data.X + "," + chunk_data.Z + "," + innerX + "," + innerZ;
		float curr_depth = GameController.Instance.DepthAt(new Vector3((float)(chunk_data.X * 10) + (float)innerX + 0.5f, 1.5f, (float)(chunk_data.Z * 10) + (float)innerZ + 0.5f));
		int wildCreatureLevel = MobControl.Instance.GetWildCreatureLevel(2f, curr_depth);
		return new CreatureStruct(text, list, wildCreatureLevel, 2f, SharedCreature.brain_type_t.neutral, 0.04f, DEFAULT_HP_REGEN, DEFAULT_HP_MAX, DEFAULT_HP_CURR, 0.5f, 0f, new InventoryItem(""), new InventoryItem(""), new InventoryItem(""), "Big-Momma " + biome_mobA, 7200, "", item, rot, DEFAULT_ICON_ID, chunk_data.zone, chunk_data.X, chunk_data.Z, innerX, innerZ, 0, 0);
	}

	public static CreatureStruct GenerateCompanionGhost(InventoryItem companion_item, InventoryItem original_item, ChunkData chunk_data, int rot, int innerX, int innerZ, int spawn_offset_x, int spawn_offset_z)
	{
		List<string> list = new List<string>();
		list.Add(companion_item.GetString("creature_A"));
		list.Add(companion_item.GetString("creature_B"));
		ItemCountPair[] itemListFromItem = ChunkControl.Instance.GetItemListFromItem("pockets", companion_item);
		int num = chunk_data.X;
		int num2 = chunk_data.Z;
		int num3 = spawn_offset_x + innerX;
		int num4 = spawn_offset_z + innerZ;
		if (num3 >= 10)
		{
			num3 -= 10;
			num++;
		}
		else if (num3 < 0)
		{
			num3 += 10;
			num--;
		}
		if (num4 >= 10)
		{
			num4 -= 10;
			num2++;
		}
		else if (num4 < 0)
		{
			num4 += 10;
			num2--;
		}
		string text = chunk_data.zone + "," + num + "," + num2 + "," + num3 + "," + num4;
		return new CreatureStruct(text, list, companion_item.GetLong("level"), 1f, SharedCreature.brain_type_t.ghost, DEFAULT_WALK_SPEED, DEFAULT_HP_REGEN, DEFAULT_HP_MAX, DEFAULT_HP_CURR, DEFAULT_AI_LOCKON, 0f, itemListFromItem[3].item, itemListFromItem[8].item, itemListFromItem[13].item, companion_item.GetString("npc_display_name"), DEFAULT_RESPAWN_SECONDS, "Blue Glow", original_item, rot, DEFAULT_ICON_ID, chunk_data.zone, chunk_data.X, chunk_data.Z, innerX, innerZ, spawn_offset_x, spawn_offset_z);
	}

	public static CreatureStruct GenerateHiveMob(ChunkData chunk_data, int innerX, int innerZ, string minicreature, InventoryItem item, int rot, int spawn_offset_x, int spawn_offset_z)
	{
		List<string> list = new List<string>();
		list.Add(minicreature);
		list.Add(minicreature);
		int num = chunk_data.X;
		int num2 = chunk_data.Z;
		int num3 = spawn_offset_x + innerX;
		int num4 = spawn_offset_z + innerZ;
		if (num3 >= 10)
		{
			num3 -= 10;
			num++;
		}
		else if (num3 < 0)
		{
			num3 += 10;
			num--;
		}
		if (num4 >= 10)
		{
			num4 -= 10;
			num2++;
		}
		else if (num4 < 0)
		{
			num4 += 10;
			num2--;
		}
		string text = chunk_data.zone + "," + num + "," + num2 + "," + num3 + "," + num4;
		float curr_depth = GameController.Instance.DepthAt(new Vector3((float)innerX + (float)(chunk_data.X * 10) + 0.5f, 1.5f, (float)innerZ + (float)(chunk_data.Z * 10) + 0.5f));
		int wildCreatureLevel = MobControl.Instance.GetWildCreatureLevel(0.7f, curr_depth);
		return new CreatureStruct(text, list, wildCreatureLevel, 0.5f, SharedCreature.brain_type_t.aggressive, 0.049f, DEFAULT_HP_REGEN, DEFAULT_HP_MAX, DEFAULT_HP_CURR, 2f, 1f, new InventoryItem(""), new InventoryItem(""), new InventoryItem(""), "[TRANSLATE]", 180, "", item, rot, DEFAULT_ICON_ID, chunk_data.zone, chunk_data.X, chunk_data.Z, innerX, innerZ, spawn_offset_x, spawn_offset_z);
	}

	public static CreatureStruct GenerateGuardMob(ChunkData chunk_data, int innerX, int innerZ, string creatureA, string creatureB, InventoryItem hat, InventoryItem body, InventoryItem hand, string companion_name, int level, InventoryItem item, int rot)
	{
		List<string> list = new List<string>();
		list.Add(creatureA);
		list.Add(creatureB);
		string text = chunk_data.zone + "," + chunk_data.X + "," + chunk_data.Z + "," + innerX + "," + innerZ;
		return new CreatureStruct(text, list, level, 1f, SharedCreature.brain_type_t.guard, DEFAULT_WALK_SPEED, DEFAULT_HP_REGEN, DEFAULT_HP_MAX, DEFAULT_HP_CURR, 5f, 0f, hat, body, hand, companion_name, DEFAULT_RESPAWN_SECONDS, "", item, rot, DEFAULT_ICON_ID, chunk_data.zone, chunk_data.X, chunk_data.Z, innerX, innerZ, 0, 0);
	}
}
