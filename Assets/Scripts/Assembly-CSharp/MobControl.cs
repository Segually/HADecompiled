using System;
using System.Collections.Generic;
using UnityEngine;

public class MobControl : MonoBehaviour, OrderedStart
{
	[Serializable]
	public struct skin_coloring
	{
		public string name;

		public Material mat;
	}

	public enum creature_type_t
	{
		local_mob = 0,
		local_player = 1,
		local_companion = 2,
		network_mob = 3,
		network_player = 4
	}

	public static MobControl Instance;

	public Color color_dmg_hit;

	public Color color_dmg_miss;

	public Color color_dmg_mine;

	public Color color_dmg_poison;

	public Color color_dmg_vampire;

	public Dictionary<string, GameObject> active_combatants = new Dictionary<string, GameObject>();

	public List<string> my_claimed_creatures = new List<string>();

	public skin_coloring[] skin_colors;

	public Dictionary<string, MobFile> loaded_mob_files = new Dictionary<string, MobFile>();

	public GameObject prefab_drop_no_model;

	public List<CreatureStruct> creatures_to_request_next_step = new List<CreatureStruct>();

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	private int ParseLevelString(string level_str, int chunkX, int chunkZ, int innerX, int innerZ, InventoryItem item)
	{
		return 0;
	}

	public string ItemToMobFileName(InventoryItem item)
	{
		return null;
	}

	public MobFile GetMobFile(string mob_file_name)
	{
		return null;
	}

	public CreatureStruct GetCreatureStructFromMobFile(string mob_file_name, ChunkData chunk_data, int innerX, int innerZ, InventoryItem item, int rot)
	{
		return null;
	}

	private static MobFile ReadMobFile(string mob_file_name)
	{
		return null;
	}

	public void TryDeloadDistantMobs()
	{
		foreach (KeyValuePair<string, GameObject> active_combatant in Instance.active_combatants)
		{
			GameObject value = active_combatant.Value;
			SharedCreature component = value.GetComponent<SharedCreature>();
			if (!(component != null) || !component.is_local_mob || value == GameController.Instance.player)
			{
				continue;
			}
			Vector3 chunkCoords = ChunkControl.Instance.GetChunkCoords(value.transform.position);
			if (ChunkControl.Instance.ChunkExists(ChunkControl.Instance.GetChunkString(ChunkControl.Instance.player_zone, (int)chunkCoords.x, (int)chunkCoords.z)))
			{
				continue;
			}
			if (value.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.companion)
			{
				if (GameController.Instance.player != null)
				{
					value.transform.position = new Vector3(GameController.Instance.player.transform.position.x + 1.5f, 1.5f, GameController.Instance.player.transform.position.z - 1.5f);
				}
			}
			else
			{
				component.Deload(true);
				UnityEngine.Object.Destroy(value);
			}
		}
	}

	public Material GetSkinMaterialByName(string mat_name)
	{
		return null;
	}

	public GameObject GetClosestCombatant(Vector3 position, bool target_minerals, bool target_local_companions, bool target_ratroaches, bool target_local_wolf_pack)
	{
		GameObject result = null;
		float num = float.MaxValue;
		foreach (KeyValuePair<string, GameObject> active_combatant in Instance.active_combatants)
		{
			GameObject value = active_combatant.Value;
			if (value == null)
			{
				continue;
			}
			Combatant component = value.GetComponent<Combatant>();
			if (component.is_dead || value == GameController.Instance.player || !value.activeInHierarchy || (!target_minerals && component.mob_type == Combatant.TYPE_T.mineral) || (!target_local_companions && component.mob_type == Combatant.TYPE_T.creature && value.GetComponent<SharedCreature>().is_local_mob && value.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.companion) || (!target_ratroaches && component.original_element_item.item_name == "Mob - Ratroach") || (!target_local_wolf_pack && component.mob_type == Combatant.TYPE_T.creature && value.GetComponent<SharedCreature>().is_local_mob && value.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.wolf_pack))
			{
				continue;
			}
			if (component.WithinHitbox(position, true))
			{
				float num2 = Vector3.Distance(value.transform.position, position);
				if (num2 < num)
				{
					result = value;
					num = num2;
				}
			}
		}
		return result;
	}

	private void FixedUpdate()
	{
	}

	public void TrySpawnDrop(ItemCountPair drop_pair, Vector3 drop_pos)
	{
	}

	public void SnapOverhead(RectTransform R, Vector3 pos)
	{
		Vector3 vector = Camera.main.WorldToViewportPoint(pos);
		R.anchorMin = vector;
		R.anchorMax = vector;
	}

	public void TryInstantiateMobMiniCluster(string minicreature_type, ChunkData chunk_data, int x, int z, InventoryItem item, int rot, ChunkObj chunkObj)
	{
	}

	public CreatureStruct ObjToCreatureStruct(GameObject critter)
	{
		return null;
	}

	public void TryInstantiateMob(CreatureStruct request, InventoryItem origin_item, int origin_rot, ChunkObj chunkObj)
	{
	}

	public GameObject SpawnLocalMob(CreatureStruct creature, Vector3 curr_pos)
	{
		return null;
	}

	public void SpawnNetMob(CreatureStruct creature, Vector3 curr_pos, string combat_id)
	{
	}

	public GameObject SpawnCompanion(Vector3 pos, ActiveCompanion companion)
	{
		return null;
	}

	public void SpawnOtherPlayer(OnlinePlayer player, OnlinePlayerData playerdata)
	{
	}

	public void SpawnMainPlayer(GameObject morph, Vector3 position, string combat_name)
	{
		GameController instance = GameController.Instance;
		instance.player = CreateGenericCreature(creature_type_t.local_player, morph, position + Vector3.up * SharedCreature.H, combat_name, GameController.Instance.playerLevel, 1f, 0.051f, 1, 1, 0, SharedCreature.brain_type_t.local_player, "", 0, 0, 0, 0, 0f, 0f, "", new InventoryItem(""), new InventoryItem(""), new InventoryItem(""), 0, "", new InventoryItem(""), 0, 0, true, 0, 0);
		GameController.Instance.player.GetComponent<SharedCreature>().ReCalcHpMaxAndHpRegen();
		int hP_max = GameController.Instance.player.GetComponent<Combatant>().HP_max;
		GameController.Instance.player.GetComponent<Combatant>().hp = hP_max;
		GameController.Instance.player.name = "PlayerObject";
		if (PlayerData.Instance.GetGlobalShort("dev_mode") == 1)
		{
			float num = GameController.Instance.player.GetComponent<SharedCreature>().walk_speed;
			GameController.Instance.player.GetComponent<SharedCreature>().walk_speed = num * ConsoleControl.auto_sprint_speed;
		}
	}

	private GameObject CreateGenericCreature(creature_type_t creature_type, GameObject morph, Vector3 curr_pos, string combat_name, int critterLevel, float critterSize, float walk_speed, int hp_curr, int hp_max, int hp_regen, SharedCreature.brain_type_t brain_type, string origin_zone, int origin_chunkX, int origin_chunkZ, int origin_innerX, int origin_innerZ, float ai_lockon_range, float wander_dist, string creature_name, InventoryItem hat, InventoryItem body, InventoryItem hand, int respawn_secs, string skin_mat, InventoryItem original_element_item, int original_element_rot, int icon_id, bool is_local_mob, int spawn_offset_x, int spawn_offset_z)
	{
		GameObject original;
		switch (creature_type)
		{
		case creature_type_t.local_mob:
		case creature_type_t.local_player:
		case creature_type_t.local_companion:
			original = GameController.Instance.type_creature;
			break;
		case creature_type_t.network_mob:
		case creature_type_t.network_player:
			original = GameServerInterface.Instance.prefab_online_player;
			break;
		default:
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(original);
		gameObject.GetComponent<SharedCreature>().Init();
		gameObject.GetComponent<Combatant>().Init();
		gameObject.GetComponent<SharedCreature>().myCreatureModel = morph.GetComponent<LiteModel>();
		morph.transform.SetParent(gameObject.transform.Find("model goes here"));
		morph.transform.localPosition = Vector3.zero;
		morph.transform.localRotation = Quaternion.identity;
		gameObject.GetComponent<SharedCreature>().SetMoveTo(curr_pos);
		gameObject.GetComponent<SharedCreature>().myCreatureModel.StartAnimation(0);
		gameObject.GetComponent<SharedCreature>().default_size = critterSize;
		gameObject.GetComponent<SharedCreature>().UpdateSize();
		if (creature_type == creature_type_t.network_mob || creature_type == creature_type_t.local_mob)
		{
			if (critterSize >= 2f)
			{
				gameObject.GetComponent<SharedCreature>().myCreatureModel.animation_choppiness = GraphicsControl.Instance.SpecialAnimationChoppiness();
			}
			else if (critterSize < 0.5f)
			{
				gameObject.GetComponent<SharedCreature>().myCreatureModel.animation_choppiness = 8;
			}
		}
		gameObject.GetComponent<Combatant>().combat_name = combat_name;
		if (!active_combatants.ContainsKey(combat_name))
		{
			active_combatants.Add(combat_name, gameObject);
		}
		gameObject.transform.position = curr_pos;
		gameObject.GetComponent<SharedCreature>().SnapSpotterRotation(gameObject.transform.rotation);
		gameObject.GetComponent<SharedCreature>().walk_speed = walk_speed;
		gameObject.GetComponent<SharedCreature>().level = ((critterLevel < 2) ? 1 : critterLevel);
		gameObject.GetComponent<Combatant>().HP_max = hp_max;
		gameObject.GetComponent<Combatant>().hp = hp_curr;
		gameObject.GetComponent<SharedCreature>().hp_regen = hp_regen;
		gameObject.GetComponent<SharedCreature>().StartRegenHealth();
		gameObject.GetComponent<Combatant>().respawn_time = respawn_secs;
		gameObject.GetComponent<SharedCreature>().brain_type = brain_type;
		SharedCreature component = gameObject.GetComponent<SharedCreature>();
		if (is_local_mob)
		{
			component.is_local_mob = true;
			CreatureBrain creatureBrain = null;
			switch (brain_type)
			{
			case SharedCreature.brain_type_t.local_player:
				creatureBrain = gameObject.AddComponent<CreatureBrain>();
				creatureBrain.SetDefaultBrain(gameObject.AddComponent<CreatureBrainLocalPlayer>());
				break;
			case SharedCreature.brain_type_t.aggressive:
				creatureBrain = gameObject.AddComponent<CreatureBrain>();
				creatureBrain.SetDefaultBrain(gameObject.AddComponent<CreatureBrainAggressive>());
				break;
			case SharedCreature.brain_type_t.neutral:
				creatureBrain = gameObject.AddComponent<CreatureBrain>();
				creatureBrain.SetDefaultBrain(gameObject.AddComponent<CreatureBrainNeutral>());
				break;
			case SharedCreature.brain_type_t.fearful:
				creatureBrain = gameObject.AddComponent<CreatureBrain>();
				creatureBrain.SetDefaultBrain(gameObject.AddComponent<CreatureBrainFearful>());
				break;
			case SharedCreature.brain_type_t.companion:
				creatureBrain = gameObject.AddComponent<CreatureBrain>();
				creatureBrain.SetDefaultBrain(gameObject.AddComponent<CreatureBrainCompanion>());
				break;
			case SharedCreature.brain_type_t.guard:
				creatureBrain = gameObject.AddComponent<CreatureBrain>();
				creatureBrain.SetDefaultBrain(gameObject.AddComponent<CreatureBrainGuard>());
				break;
			case SharedCreature.brain_type_t.wolf_pack:
				creatureBrain = gameObject.AddComponent<CreatureBrain>();
				creatureBrain.SetDefaultBrain(gameObject.AddComponent<CreatureBrainWolfPack>());
				break;
			case SharedCreature.brain_type_t.ghost:
				creatureBrain = gameObject.AddComponent<CreatureBrain>();
				creatureBrain.SetDefaultBrain(gameObject.AddComponent<CreatureBrainGhost>());
				break;
			}
			if (creatureBrain != null)
			{
				creatureBrain.custom_brain.Init();
				creatureBrain.RestartThinking();
			}
		}
		else
		{
			component.is_local_mob = false;
		}
		if (creature_type < creature_type_t.network_player && creature_type != creature_type_t.local_player)
		{
			gameObject.GetComponent<Combatant>().SetOrigin(origin_zone, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ);
			gameObject.GetComponent<Combatant>().original_element_item = original_element_item;
			gameObject.GetComponent<Combatant>().original_element_rot = original_element_rot;
			gameObject.GetComponent<Combatant>().spawn_offset_x = spawn_offset_x;
			gameObject.GetComponent<Combatant>().spawn_offset_z = spawn_offset_z;
			gameObject.GetComponent<SharedCreature>().ai_lockon_range = ai_lockon_range;
			gameObject.GetComponent<SharedCreature>().wander_dist = wander_dist;
			LiteModel myCreatureModel = gameObject.GetComponent<SharedCreature>().myCreatureModel;
			if (myCreatureModel.original.creatures_that_made_me.Count >= 2)
			{
				Color creature_name_col = ((icon_id == 2 || icon_id == 3) ? CompanionController.Instance.GetTextColorFromIcon(icon_id) : Instance.GetOverheadNameColor(myCreatureModel.original.creatures_that_made_me[0] + myCreatureModel.original.creatures_that_made_me[1]));
				if (creature_name == "[TRANSLATE]")
				{
					bool num = myCreatureModel.original.creatures_that_made_me[0] == myCreatureModel.original.creatures_that_made_me[1];
					creature_name = myCreatureModel.original.creatures_that_made_me_TRANSLATED[0];
					if (!num)
					{
						creature_name = creature_name + " " + myCreatureModel.original.creatures_that_made_me_TRANSLATED[1];
					}
					if (critterSize < 0.9f)
					{
						creature_name = "Tiny " + creature_name;
					}
					else if (critterSize >= 2f && critterSize < 3f)
					{
						creature_name = "Big " + creature_name;
					}
					else if (critterSize >= 3f)
					{
						creature_name = "Giant " + creature_name;
					}
				}
				gameObject.GetComponent<SharedCreature>().AssignOverheadName(creature_name, creature_name_col);
				gameObject.GetComponent<SharedCreature>().icon_id = icon_id;
				gameObject.GetComponent<SharedCreature>().RedrawLevelDisplay();
			}
		}
		if (!Startup.StringNullOrWhitespace(skin_mat))
		{
			gameObject.GetComponent<SharedCreature>().base_skin_material = skin_mat;
			gameObject.GetComponent<SharedCreature>().UpdateSkinMaterial();
		}
		if (hat.item_name != "")
		{
			gameObject.GetComponent<SharedCreature>().hat_ = hat;
		}
		if (body.item_name != "")
		{
			gameObject.GetComponent<SharedCreature>().body_ = body;
		}
		if (hand.item_name != "")
		{
			gameObject.GetComponent<SharedCreature>().hand_ = hand;
		}
		if (hat.item_name != "" || body.item_name != "" || hand.item_name != "")
		{
			gameObject.GetComponent<SharedCreature>().OnEquipmentChanged();
		}
		return gameObject;
	}

	public static void PlaceMob(int x, int z, ChunkData chunk_data, float depth, bool on_converter = false)
	{
		float num = 0f;
		float num2 = 0f;
		if (depth >= 20f)
		{
			num = depth * 0.001f + 0.03f;
			num2 = depth * 0.00023f + 0.1f;
			if (num > 1f)
			{
				num = 1f;
			}
			if (num2 > 0.4f)
			{
				num2 = 0.4f;
			}
		}
		string new_item_name;
		if (UnityEngine.Random.value < num2)
		{
			new_item_name = "Mob - Giant";
		}
		else if (UnityEngine.Random.value < num)
		{
			new_item_name = "Mob - Big";
		}
		else
		{
			float num3 = depth * -0.0275f + 1f;
			if (num3 <= 0f)
			{
				num3 = 0f;
			}
			new_item_name = ((!(UnityEngine.Random.value < num3)) ? "Mob - Normal" : "Mob - Tiny");
		}
		ExtraInventoryData extraInventoryData = new ExtraInventoryData();
		if (on_converter)
		{
			extraInventoryData.SetString("tag", "natural");
		}
		chunk_data.AddElement(x, z, new ChunkElement(new InventoryItem(new_item_name, extraInventoryData)));
	}

	private static float OddsTiny(float curr_depth)
	{
		return 0f;
	}

	private static float OddsBig(float curr_depth)
	{
		return 0f;
	}

	private static float OddsGiant(float curr_depth)
	{
		return 0f;
	}

	public int GetWildCreatureLevel(float levelmod, float curr_depth)
	{
		return 0;
	}

	public int GetQuestEnemyLevel(float player_level_percent)
	{
		return 0;
	}

	public void ClearAllCreatures(bool keep_player, bool send_deloads, bool keep_net_players)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, GameObject> active_combatant in active_combatants)
		{
			GameObject value = active_combatant.Value;
			if ((keep_player && GameController.Instance.player != null && value == GameController.Instance.player) || (value.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature && value.GetComponent<SharedCreature>().is_local_mob && value.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.companion) || (keep_net_players && GameServerInterface.Instance.nearby_players.ContainsKey(active_combatant.Key)))
			{
				continue;
			}
			list.Add(active_combatant.Key);
		}
		foreach (string item in list)
		{
			GameObject gameObject = active_combatants[item];
			SharedCreature component = gameObject.GetComponent<SharedCreature>();
			if (component != null && component.is_local_mob)
			{
				gameObject.GetComponent<SharedCreature>().Deload(send_deloads);
			}
			gameObject.GetComponent<Combatant>().Delete();
			UnityEngine.Object.Destroy(gameObject);
		}
		CompanionController.Instance.RedrawCompanionNibs();
	}

	public float GetCritterSize(int critterLevel, float curr_depth = 999f)
	{
		return 0f;
	}

	public Color GetOverheadNameColor(string str)
	{
		return default(Color);
	}

	public SharedCreature.brain_type_t DetermineCreatureBrain(int critterLevel, float depth)
	{
		return default(SharedCreature.brain_type_t);
	}
}
