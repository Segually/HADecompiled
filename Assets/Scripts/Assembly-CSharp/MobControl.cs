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
		if (level_str.Contains("PLAYER_LEVEL*"))
		{
			return GetQuestEnemyLevel(float.Parse(level_str.Replace("PLAYER_LEVEL*", ""), Startup.parse_culture));
		}
		if (level_str.Contains("WILD_DEPTH*"))
		{
			float f = GameController.Instance.DepthAt(new Vector3((float)(chunkX * 10) + (float)innerX + 0.5f, 1.5f, (float)(chunkZ * 10) + (float)innerZ + 0.5f));
			float num = float.Parse(level_str.Replace("WILD_DEPTH*", ""), Startup.parse_culture);
			return (int)(Mathf.Pow(f, 1.65f) * 0.017f * num);
		}
		if (level_str.Contains("CAMP_DEPTH*"))
		{
			string @string = item.GetString("bandit_camp_instance");
			float num2 = float.Parse(level_str.Replace("CAMP_DEPTH*", ""), Startup.parse_culture);
			float f2 = ((!(@string != "")) ? GameController.Instance.DepthAt(new Vector3((float)(chunkX * 10) + (float)innerX + 0.5f, 1.5f, (float)(chunkZ * 10) + (float)innerZ + 0.5f)) : BanditCampsControl.Instance.GetBanditCampInstanceByName(@string).instance_depth);
			return (int)(num2 * Mathf.Pow(f2, 1.65f) * 0.017f);
		}
		return int.Parse(level_str);
	}

	public string ItemToMobFileName(InventoryItem item)
	{
		if (item.item_name == "Boss Spawner - Shindeon")
		{
			return "Shindeon";
		}
		if (item.item_name == "Boss Spawner - Yandeon")
		{
			return "Yandeon";
		}
		string text = item.item_name.Replace("Mob - ", "");
		if (text == "Bandit Elite")
		{
			return "Elite Bandit";
		}
		if (text.Contains("BANDIT_FACTION"))
		{
			BanditCampInstance banditCampInstanceByName = BanditCampsControl.Instance.GetBanditCampInstanceByName(item.GetString("bandit_camp_instance"));
			if (banditCampInstanceByName == null)
			{
				return "Ratroach";
			}
			return BanditCampsControl.Instance.GetMobFromFaction(text, banditCampInstanceByName.biome_id);
		}
		return text;
	}

	public MobFile GetMobFile(string mob_file_name)
	{
		if (!loaded_mob_files.ContainsKey(mob_file_name))
		{
			MobFile mobFile = ReadMobFile(mob_file_name);
			if (mobFile != null)
			{
				loaded_mob_files.Add(mob_file_name, mobFile);
			}
			return mobFile;
		}
		return loaded_mob_files[mob_file_name];
	}

	public CreatureStruct GetCreatureStructFromMobFile(string mob_file_name, ChunkData chunk_data, int innerX, int innerZ, InventoryItem item, int rot)
	{
		MobFile mobFile = GetMobFile(mob_file_name);
		List<string> list = new List<string>();
		if (mobFile.Entries.ContainsKey("creature_A"))
		{
			string text = mobFile.Entries["creature_A"];
			if (text == "BOSS_ANIMAL_A")
			{
				list.Add(BanditCampsControl.Instance.GetBanditCampInstanceByName(item.GetString("bandit_camp_instance")).GetRandomizedBossEntry("BOSS_ANIMAL_A"));
			}
			else if (text == "BIOME_MOB_A")
			{
				list.Add(chunk_data.biome_mobA);
			}
			else
			{
				list.Add(text);
			}
		}
		if (mobFile.Entries.ContainsKey("creature_B"))
		{
			string text2 = mobFile.Entries["creature_B"];
			if (text2 == "BOSS_ANIMAL_B")
			{
				list.Add(BanditCampsControl.Instance.GetBanditCampInstanceByName(item.GetString("bandit_camp_instance")).GetRandomizedBossEntry("BOSS_ANIMAL_B"));
			}
			else if (text2 == "BIOME_MOB_B")
			{
				list.Add(chunk_data.biome_mobB);
			}
			else
			{
				list.Add(text2);
			}
		}
		string combat_name = chunk_data.zone + "," + chunk_data.X + "," + chunk_data.Z + "," + innerX + "," + innerZ;
		string text3 = item.GetString("overwrite_mob_level");
		int critterLevel_;
		if (text3 != "")
		{
			critterLevel_ = ParseLevelString(text3, chunk_data.X, chunk_data.Z, innerX, innerZ, item);
		}
		else if (mobFile.Entries.ContainsKey("level") && !string.IsNullOrWhiteSpace(text3 = mobFile.Entries["level"].Trim()))
		{
			critterLevel_ = ParseLevelString(text3, chunk_data.X, chunk_data.Z, innerX, innerZ, item);
		}
		else
		{
			critterLevel_ = 1;
		}
		float critterSize_ = CreatureStruct.DEFAULT_CRITTER_SIZE;
		if (mobFile.Entries.ContainsKey("size"))
		{
			string text4 = mobFile.Entries["size"].Trim();
			if (!string.IsNullOrWhiteSpace(text4))
			{
				critterSize_ = ((!(text4 == "DEFAULT_CRITTER_SIZE")) ? float.Parse(text4, Startup.parse_culture) : ((float)CreatureStruct.DEFAULT_CRITTER_SIZE));
			}
		}
		SharedCreature.brain_type_t brain_type = CreatureStruct.DEFAULT_BRAIN_TYPE;
		if (mobFile.Entries.ContainsKey("state"))
		{
			string text5 = mobFile.Entries["state"].Trim();
			if (text5 == "DEFAULT_BRAIN_TYPE")
			{
				brain_type = SharedCreature.brain_type_t.auto_set;
			}
			else if (text5 == "aggressive")
			{
				brain_type = SharedCreature.brain_type_t.aggressive;
			}
			else if (text5 == "neutral")
			{
				brain_type = SharedCreature.brain_type_t.neutral;
			}
		}
		float walk_speed_ = CreatureStruct.DEFAULT_WALK_SPEED;
		if (mobFile.Entries.ContainsKey("walk_speed"))
		{
			string text6 = mobFile.Entries["walk_speed"].Trim();
			if (!string.IsNullOrWhiteSpace(text6))
			{
				walk_speed_ = ((!(text6 == "DEFAULT_WALK_SPEED")) ? float.Parse(mobFile.Entries["walk_speed"], Startup.parse_culture) : CreatureStruct.DEFAULT_WALK_SPEED);
			}
		}
		int hp_regen_ = CreatureStruct.DEFAULT_HP_REGEN;
		if (mobFile.Entries.ContainsKey("hp_regen"))
		{
			string text7 = mobFile.Entries["hp_regen"].Trim();
			if (!string.IsNullOrWhiteSpace(text7))
			{
				hp_regen_ = ((!(text7 == "DEFAULT_HP_REGEN")) ? int.Parse(mobFile.Entries["hp_regen"]) : CreatureStruct.DEFAULT_HP_REGEN);
			}
		}
		int hp_max_ = CreatureStruct.DEFAULT_HP_MAX;
		if (mobFile.Entries.ContainsKey("hp_max"))
		{
			string text8 = mobFile.Entries["hp_max"].Trim();
			if (!string.IsNullOrWhiteSpace(text8))
			{
				hp_max_ = ((!(text8 == "DEFAULT_HP_MAX")) ? int.Parse(mobFile.Entries["hp_max"]) : CreatureStruct.DEFAULT_HP_MAX);
			}
		}
		int hp_curr_ = CreatureStruct.DEFAULT_HP_CURR;
		if (mobFile.Entries.ContainsKey("hp_curr"))
		{
			string text9 = mobFile.Entries["hp_curr"].Trim();
			if (!string.IsNullOrWhiteSpace(text9))
			{
				hp_curr_ = ((!(text9 == "DEFAULT_HP_CURR")) ? int.Parse(mobFile.Entries["hp_curr"]) : CreatureStruct.DEFAULT_HP_CURR);
			}
		}
		float ai_lockon_range_ = CreatureStruct.DEFAULT_AI_LOCKON;
		if (mobFile.Entries.ContainsKey("ai_lockon"))
		{
			string text10 = mobFile.Entries["ai_lockon"].Trim();
			if (!string.IsNullOrWhiteSpace(text10))
			{
				ai_lockon_range_ = ((!(text10 == "DEFAULT_AI_LOCKON")) ? float.Parse(mobFile.Entries["ai_lockon"], Startup.parse_culture) : CreatureStruct.DEFAULT_AI_LOCKON);
			}
		}
		float wander_dist_ = CreatureStruct.DEFAULT_WANDER_DIST;
		if (mobFile.Entries.ContainsKey("wander_dist"))
		{
			string text11 = mobFile.Entries["wander_dist"].Trim();
			if (!string.IsNullOrWhiteSpace(text11))
			{
				wander_dist_ = ((!(text11 == "DEFAULT_WANDER_DIST")) ? float.Parse(mobFile.Entries["wander_dist"], Startup.parse_culture) : CreatureStruct.DEFAULT_WANDER_DIST);
			}
		}
		InventoryItem inventoryItem = null;
		if (mobFile.Entries.ContainsKey("hat"))
		{
			string text12 = mobFile.Entries["hat"].Trim();
			ExtraInventoryData extraInventoryData = new ExtraInventoryData();
			if (text12 == "BOSS_HELMET_MODEL")
			{
				text12 = BanditCampsControl.Instance.GetBanditCampInstanceByName(item.GetString("bandit_camp_instance")).GetRandomizedBossEntry("BOSS_HELMET_MODEL");
			}
			if (mobFile.Entries.ContainsKey("hat_paint"))
			{
				extraInventoryData.SetString("paint", mobFile.Entries["hat_paint"]);
			}
			inventoryItem = new InventoryItem(text12, extraInventoryData);
		}
		InventoryItem inventoryItem2 = null;
		if (mobFile.Entries.ContainsKey("armor"))
		{
			string text13 = mobFile.Entries["armor"].Trim();
			ExtraInventoryData extraInventoryData2 = new ExtraInventoryData();
			if (text13 == "BOSS_ARMOR_MODEL")
			{
				text13 = BanditCampsControl.Instance.GetBanditCampInstanceByName(item.GetString("bandit_camp_instance")).GetRandomizedBossEntry("BOSS_ARMOR_MODEL");
			}
			if (mobFile.Entries.ContainsKey("armor_paint"))
			{
				extraInventoryData2.SetString("paint", mobFile.Entries["armor_paint"]);
			}
			inventoryItem2 = new InventoryItem(text13, extraInventoryData2);
		}
		InventoryItem inventoryItem3 = null;
		if (mobFile.Entries.ContainsKey("hand"))
		{
			string text14 = mobFile.Entries["hand"].Trim();
			ExtraInventoryData extraInventoryData3 = new ExtraInventoryData();
			if (text14 == "BOSS_WEAPON_MODEL")
			{
				BanditCampInstance banditCampInstanceByName = BanditCampsControl.Instance.GetBanditCampInstanceByName(item.GetString("bandit_camp_instance"));
				text14 = banditCampInstanceByName.GetRandomizedBossEntry("BOSS_WEAPON_MODEL");
				extraInventoryData3.SetShort("dual_wield", (banditCampInstanceByName.GetRandomizedBossEntry("BOSS_WEAPON_DUAL_WIELD") == "1") ? 1 : 0);
			}
			if (mobFile.Entries.ContainsKey("hand_paint"))
			{
				extraInventoryData3.SetString("paint", mobFile.Entries["hand_paint"]);
			}
			inventoryItem3 = new InventoryItem(text14, extraInventoryData3);
		}
		int respawn_seconds_ = CreatureStruct.DEFAULT_RESPAWN_SECONDS;
		if (mobFile.Entries.ContainsKey("respawn_seconds"))
		{
			string text15 = mobFile.Entries["respawn_seconds"].Trim();
			if (!string.IsNullOrWhiteSpace(text15))
			{
				respawn_seconds_ = ((!(text15 == "DEFAULT_RESPAWN_SECONDS")) ? int.Parse(text15) : CreatureStruct.DEFAULT_RESPAWN_SECONDS);
			}
		}
		string skin_mat_ = "";
		if (mobFile.Entries.ContainsKey("skin_mat"))
		{
			skin_mat_ = mobFile.Entries["skin_mat"].Trim();
		}
		int icon_id_ = CreatureStruct.DEFAULT_ICON_ID;
		if (mobFile.Entries.ContainsKey("icon_id"))
		{
			string text16 = mobFile.Entries["icon_id"].Trim();
			if (!string.IsNullOrWhiteSpace(text16))
			{
				icon_id_ = ((!(text16 == "DEFAULT_ICON_ID")) ? int.Parse(mobFile.Entries["icon_id"]) : CreatureStruct.DEFAULT_ICON_ID);
			}
		}
		int spawn_offset_x = 0;
		if (mobFile.Entries.ContainsKey("spawn_offset_x"))
		{
			string text17 = mobFile.Entries["spawn_offset_x"].Trim();
			if (!string.IsNullOrWhiteSpace(text17))
			{
				spawn_offset_x = int.Parse(text17);
			}
		}
		int spawn_offset_z = 0;
		if (mobFile.Entries.ContainsKey("spawn_offset_z"))
		{
			string text18 = mobFile.Entries["spawn_offset_z"].Trim();
			if (!string.IsNullOrWhiteSpace(text18))
			{
				spawn_offset_z = int.Parse(text18);
			}
		}
		string creature_name_;
		if (item.GetString("overwrite_mob_name") != "")
		{
			creature_name_ = item.GetString("overwrite_mob_name");
		}
		else
		{
			creature_name_ = mob_file_name;
			if (mobFile.Entries.ContainsKey("overwrite_mob_name"))
			{
				string text19 = mobFile.Entries["overwrite_mob_name"].Trim();
				if (!string.IsNullOrWhiteSpace(text19))
				{
					creature_name_ = text19;
					if (text19 == "BOSS_NAME")
					{
						creature_name_ = BanditCampsControl.Instance.GetBanditCampInstanceByName(item.GetString("bandit_camp_instance")).GetRandomizedBossEntry("BOSS_NAME");
					}
				}
			}
		}
		if (inventoryItem == null)
		{
			inventoryItem = new InventoryItem("");
		}
		if (inventoryItem2 == null)
		{
			inventoryItem2 = new InventoryItem("");
		}
		if (inventoryItem3 == null)
		{
			inventoryItem3 = new InventoryItem("");
		}
		return new CreatureStruct(combat_name, list, critterLevel_, critterSize_, brain_type, walk_speed_, hp_regen_, hp_max_, hp_curr_, ai_lockon_range_, wander_dist_, inventoryItem, inventoryItem2, inventoryItem3, creature_name_, respawn_seconds_, skin_mat_, item, rot, icon_id_, chunk_data.zone, chunk_data.X, chunk_data.Z, innerX, innerZ, spawn_offset_x, spawn_offset_z);
	}

	private static MobFile ReadMobFile(string mob_file_name)
	{
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("Mobs/" + mob_file_name, ref file_exists);
		if (!file_exists)
		{
			return null;
		}
		MobFile mobFile = new MobFile();
		bool flag = false;
		foreach (string item in textFileLines)
		{
			if (string.IsNullOrWhiteSpace(item))
			{
				continue;
			}
			if (flag)
			{
				mobFile.Drops.Add(item.Trim());
				continue;
			}
			if (item.Trim().Equals("[drops]", StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
				continue;
			}
			string[] array = item.Split(new char[1] { '=' }, 2);
			if (array.Length == 2)
			{
				mobFile.Entries[array[0].Trim()] = array[1].Trim();
			}
		}
		return mobFile;
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
		for (int i = 0; i < skin_colors.Length; i++)
		{
			if (skin_colors[i].name == mat_name)
			{
				return skin_colors[i].mat;
			}
		}
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
		string chunkString = ChunkControl.Instance.GetChunkString(drop_pos);
		if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			return;
		}
		string text = ShopControl.RandomString();
		if (ChunkControl.Instance.active_interactibles.ContainsKey(text))
		{
			return;
		}
		string drop_path = drop_pair.item.item_name;
		if (drop_pair.item.item_name == "Coins")
		{
			drop_path = ((drop_pair.count < 16) ? "Coins 1" : "Coins 2");
		}
		ChunkObj chunkObj = ChunkControl.Instance.GetChunkObj(chunkString);
		GameObject temp_drop = UnityEngine.Object.Instantiate(prefab_drop_no_model);
		temp_drop.transform.position = new Vector3(drop_pos.x, 0f, drop_pos.z);
		temp_drop.transform.SetParent(chunkObj.parent_obj.transform);
		temp_drop.transform.localScale = Vector3.one;
		temp_drop.GetComponent<Collectible>().InitAsDrop(text);
		GameObject check_not_null = temp_drop;
		ResourceControl.Instance.AsyncInstantiateDropModel(drop_path, delegate(GameObject drop_model_instance)
		{
			if (check_not_null == null)
			{
				UnityEngine.Object.Destroy(drop_model_instance);
			}
			else
			{
				UnityEngine.Object.Destroy(temp_drop.transform.Find("Bag Model").gameObject);
				drop_model_instance.transform.SetParent(temp_drop.transform);
				drop_model_instance.transform.localPosition = Vector3.zero;
				drop_model_instance.transform.localRotation = Quaternion.identity;
				drop_model_instance.transform.localScale = Vector3.one;
				if (drop_pair.item.item_name == "Egg")
				{
					string @string = drop_pair.item.GetString("egg_monster");
					Color overheadNameColor = Instance.GetOverheadNameColor(@string + @string);
					drop_model_instance.transform.Find("spin").Find("Cube").GetComponent<MeshRenderer>().material.color = overheadNameColor;
				}
			}
		});
		temp_drop.GetComponent<Collectible>().full_corresponding_inventory_object = drop_pair.item;
		temp_drop.GetComponent<Collectible>().n_give = drop_pair.count;
	}

	public void SnapOverhead(RectTransform R, Vector3 pos)
	{
		Vector3 vector = Camera.main.WorldToViewportPoint(pos);
		R.anchorMin = vector;
		R.anchorMax = vector;
	}

	public void TryInstantiateMobMiniCluster(string minicreature_type, ChunkData chunk_data, int x, int z, InventoryItem item, int rot, ChunkObj chunkObj)
	{
		for (int i = 0; i < 3; i++)
		{
			int spawn_offset_x;
			int spawn_offset_z;
			switch (i)
			{
			case 0:
				spawn_offset_x = 0;
				spawn_offset_z = 1;
				break;
			case 1:
				spawn_offset_x = 0;
				spawn_offset_z = -1;
				break;
			case 2:
				spawn_offset_x = 1;
				spawn_offset_z = 0;
				break;
			default:
				spawn_offset_x = 0;
				spawn_offset_z = 0;
				break;
			}
			TryInstantiateMob(CreatureStruct.GenerateHiveMob(chunk_data, x, z, minicreature_type, item, rot, spawn_offset_x, spawn_offset_z), item, rot, chunkObj);
		}
	}

	public CreatureStruct ObjToCreatureStruct(GameObject critter)
	{
		return null;
	}

	public void TryInstantiateMob(CreatureStruct request, InventoryItem origin_item, int origin_rot, ChunkObj chunkObj)
	{
		float x = (float)(request.original_element_chunkX * 10) + (float)request.original_element_innerX + (float)request.spawn_offset_x + 0.5f;
		float z = (float)(request.original_element_chunkZ * 10) + (float)request.original_element_innerZ + (float)request.spawn_offset_z + 0.5f;
		foreach (OccupiedSpace item in ChunkControl.Instance.GetBuildablesThatOverlapThisSpace(new Vector3(x, 0f, z)))
		{
			if (item.layer != "flooring" && item.layer != "sub_flooring" && item.element.item != origin_item)
			{
				return;
			}
		}
		if (origin_item.GetString("is_killGoal_mob") == "true")
		{
			string @string = origin_item.GetString("associated_quest_name");
			short @short = origin_item.GetShort("associated_quest_step");
			if (QuestControl.Instance.GetQuestProgress(@string) < @short || QuestControl.Instance.killGoal_mobs_killed.Contains(request.combat_name))
			{
				return;
			}
		}
		else if (origin_item.HasActiveRespawn("mob_spawn"))
		{
			chunkObj.AddRespawnWatcher(request.original_element_innerX, request.original_element_innerZ, origin_item, origin_rot, "mob_spawn");
			return;
		}
		if ((!(origin_item.GetString("bandit_camp_instance") != "") || !BanditCampsControl.Instance.GetBanditCampInstanceByName(origin_item.GetString("bandit_camp_instance")).flag_destroyed) && !active_combatants.ContainsKey(request.combat_name))
		{
			if (!GameServerConnector.Instance.FullyInGame())
			{
				SpawnLocalMob(request, new Vector3(x, 0f, z) + Vector3.up * 1.5f);
			}
			else
			{
				creatures_to_request_next_step.Add(request);
			}
		}
	}

	public GameObject SpawnLocalMob(CreatureStruct creature, Vector3 curr_pos)
	{
		GameObject hybridLite = CreatureMorpher.Instance.GetHybridLite(creature.creatures);
		GameObject gameObject = CreateGenericCreature(creature_type_t.local_mob, hybridLite, curr_pos, creature.combat_name, creature.critterLevel, creature.critterSize, creature.walk_speed, creature.hp_curr, creature.hp_max, creature.hp_regen, creature.brain_type, creature.original_element_zone, creature.original_element_chunkX, creature.original_element_chunkZ, creature.original_element_innerX, creature.original_element_innerZ, creature.ai_lockon_range, creature.wander_dist, creature.creature_name, creature.hat_, creature.body_, creature.hand_, creature.respawn_seconds, creature.skin_mat, creature.original_element_item, creature.original_element_rot, creature.icon_id, true, creature.spawn_offset_x, creature.spawn_offset_z);
		my_claimed_creatures.Add(creature.combat_name);
		gameObject.GetComponent<SharedCreature>().ai_lockon_range = creature.ai_lockon_range;
		gameObject.GetComponent<SharedCreature>().wander_dist = creature.wander_dist;
		if (BreedControl.Instance.state_t != BreedControl.state.none)
		{
			ChunkControl.Instance.enable_on_accept.Add(gameObject);
			gameObject.SetActive(false);
		}
		return gameObject;
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
		return Mathf.Max(curr_depth * -0.0275f + 1f, 0f);
	}

	private static float OddsBig(float curr_depth)
	{
		if (curr_depth < 20f)
		{
			return 0f;
		}
		return Mathf.Min(curr_depth * 0.001f + 0.03f, 1f);
	}

	private static float OddsGiant(float curr_depth)
	{
		if (curr_depth < 20f)
		{
			return 0f;
		}
		return Mathf.Min(curr_depth * 0.00023f + 0.1f, 0.4f);
	}

	public int GetWildCreatureLevel(float levelmod, float curr_depth)
	{
		return (int)(Mathf.Pow(curr_depth, 1.65f) * 0.017f * levelmod);
	}

	public int GetQuestEnemyLevel(float player_level_percent)
	{
		return (int)((float)Mathf.Max(GameController.Instance.playerLevel, 10) * player_level_percent);
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
		if (critterLevel > 3 && curr_depth >= 15f)
		{
			int num = critterLevel - GameController.Instance.playerLevel;
			if (num > 20)
			{
				return 3f;
			}
			if (num > 10)
			{
				return 2f;
			}
			if (num < -10)
			{
				return 0.7f;
			}
			return 1f;
		}
		return 0.6f;
	}

	public Color GetOverheadNameColor(string str)
	{
		int num = 0;
		for (int i = 0; i < str.Length; i++)
		{
			num += str[i] - 97;
		}
		return Color.HSVToRGB((float)(num % 122) / 122f, 0.59f, 1f);
	}

	public SharedCreature.brain_type_t DetermineCreatureBrain(int critterLevel, float depth)
	{
		if (critterLevel > 3 && depth >= 15f)
		{
			int num = critterLevel - GameController.Instance.playerLevel;
			if (num < -15)
			{
				if (UnityEngine.Random.value < 0.25f)
				{
					return SharedCreature.brain_type_t.fearful;
				}
				return SharedCreature.brain_type_t.neutral;
			}
			if (num > 10)
			{
				return SharedCreature.brain_type_t.aggressive;
			}
			if (UnityEngine.Random.value < 0.15f && critterLevel >= 6)
			{
				return SharedCreature.brain_type_t.aggressive;
			}
			return SharedCreature.brain_type_t.neutral;
		}
		return SharedCreature.brain_type_t.fearful;
	}
}
