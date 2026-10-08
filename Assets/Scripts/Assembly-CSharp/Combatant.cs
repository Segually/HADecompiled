using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Combatant : MonoBehaviour
{
	public enum TYPE_T
	{
		unknown = 0,
		stationary = 1,
		mineral = 2,
		creature = 3
	}

	public enum hit_col
	{
		color_red = 0,
		color_blue = 1,
		color_yellow = 2,
		color_green = 3,
		color_purple = 4
	}

	public float start_HP;

	public float start_exp_receie;

	public int exp_recieve;

	public float hp;

	public int HP_max;

	public string combat_name;

	public int respawn_time;

	public TYPE_T mob_type;

	public string item_recieve_on_kill;

	public int count_recieve_on_kill = 1;

	public float scale = 1f;

	public bool is_dead;

	public string origin_zone;

	public int origin_chunkX;

	public int origin_chunkZ;

	public int origin_innerX;

	public int origin_innerZ;

	public InventoryItem original_element_item = new InventoryItem("");

	public int original_element_rot;

	public int spawn_offset_x;

	public int spawn_offset_z;

	public bool destroy_parent;

	public bool deleted;

	private const float wildMobDropWeight = 2f;

	private const float weaponAndArmorWeight = 1f;

	private const float bossKeyWeight = 0.5f;

	public GameObject healthbar;

	private IEnumerator animate_hit;

	public IEnumerator disappearHealthbar;

	private Image healthbarGreen;

	private Image healthbarYellow;

	private Text splatDmg;

	private float yellow_X;

	private float yellow_width;

	private float yellow_goto_X;

	private float yellow_goto_width;

	private Camera mainCamera;

	private Vector2 hit_start_pos;

	public static float min_target_scale = 0.8f;

	public Vector3 startPos => new Vector3((float)origin_innerX + 0.5f + (float)(origin_chunkX * 10), 0f, (float)origin_innerZ + 0.5f + (float)(origin_chunkZ * 10));

	public void Init()
	{
		hp = start_HP;
		HP_max = (int)start_HP;
		float startExp = start_exp_receie;
		start_HP = 0f;
		start_exp_receie = 0f;
		exp_recieve = (int)startExp;
		combat_name = "";
		GetComponent<PerkReceiver>().my_combatant = this;
	}

	public void GainSomeHPOnLevelup()
	{
		hp = Mathf.Min(hp + (float)(int)((float)HP_max * 0.25f), HP_max);
		SetHealthVisual(0);
	}

	public void RefillHP()
	{
		hp = HP_max;
	}

	public void Delete()
	{
		if (!deleted)
		{
			deleted = true;
			if (combat_name != null)
			{
				MobControl.Instance.active_combatants.Remove(combat_name);
			}
			RemoveHealthbar();
		}
	}

	public void OnDestroy()
	{
		Delete();
		if (destroy_parent)
		{
			Object.Destroy(base.transform.parent.gameObject);
		}
	}

	public void IncreaseHp(int amount)
	{
		hp = Mathf.Min(hp + (float)amount, HP_max);
		SetHealthVisual(0);
	}

	public void SetOrigin(string origin_zone, int origin_chunkX, int origin_chunkZ, int origin_innerX, int origin_innerZ)
	{
		this.origin_zone = origin_zone;
		this.origin_chunkX = origin_chunkX;
		this.origin_chunkZ = origin_chunkZ;
		this.origin_innerX = origin_innerX;
		this.origin_innerZ = origin_innerZ;
	}

	public void RemoveHealthbar()
	{
		if (healthbar != null)
		{
			GameController.Instance.possible_destroy.Remove(healthbar);
			Object.Destroy(healthbar);
		}
	}

	private void CloseWindowsOnPlayerHit(int damage)
	{
		if (damage != 0 && !GameController.Instance.casting_projectile_at_enemy && !GameController.Instance.casting_projectile_at_ally && !GameController.Instance.picking_cast_custom_location)
		{
			WindowControl.Instance.CloseAllWindows();
		}
	}

	private byte GetNetMobType(bool this_is_a_net_player)
	{
		if (mob_type == TYPE_T.creature)
		{
			if (base.gameObject == GameController.Instance.player || this_is_a_net_player)
			{
				return 2;
			}
			return 1;
		}
		return 0;
	}

	public void WasHit(int damage, GameObject attacker, bool missed, bool dodged, hit_col col_, bool send_hit, int fake_damage = -1)
	{
		if (is_dead)
		{
			return;
		}
		string text = combat_name;
		if (attacker == GameController.Instance.player)
		{
			if (ConsoleControl.Instance.god_mode_enabled)
			{
				col_ = hit_col.color_red;
				damage = 9999;
				fake_damage = 9999;
			}
			if (original_element_item.item_name == "Flagpole" && !CombatControl.Instance.shown_attack_flagpole_notif)
			{
				GameController.Instance.PAUSE_GAME();
				PopupControl.Instance.ShowMessage(TranslationControl.Instance.TranslateGeneral("Reminder: If you destroy their flag, you'll be allowed to move nearby objects, but Loot and Enemies will not respawn!", "GUI"), PopupControl.context.message_unpause_on_okay, new InventoryItem("Flagpole"));
				CombatControl.Instance.shown_attack_flagpole_notif = true;
			}
		}
		bool flag = GameServerConnector.Instance.FullyInGame() && GameServerInterface.Instance.nearby_players.ContainsKey(text);
		bool flag2 = CombatControl.Instance.HitAllowed(attacker, base.gameObject, flag);
		if (!flag2)
		{
			fake_damage = 0;
			col_ = hit_col.color_blue;
			damage = 0;
		}
		byte netMobType = GetNetMobType(flag);
		if (send_hit && GameServerConnector.Instance.FullyInGame())
		{
			GameServerSender.Instance.SendHitMob(text, damage, fake_damage, col_, flag2 && missed, dodged, attacker, netMobType, "");
		}
		if (base.gameObject == GameController.Instance.player)
		{
			CloseWindowsOnPlayerHit(damage);
		}
		ShowVisualSplat(col_, damage, fake_damage, flag2 && missed, dodged);
		if (mob_type == TYPE_T.creature)
		{
			SharedCreature component = GetComponent<SharedCreature>();
			if (damage > 0 && component.is_local_mob)
			{
				GetComponent<CreatureBrain>().custom_brain.ReactOnHit(attacker);
			}
			if (base.gameObject != GameController.Instance.player && component.is_local_mob)
			{
				CreatureBrain component2 = GetComponent<CreatureBrain>();
				if (!component2.no_more_initial_stun && attacker != null && attacker.GetComponent<SharedCreature>() != null)
				{
					component2.initial_stun = (int)(attacker.GetComponent<SharedCreature>().perk_attack_speed_mod * (float)CreatureBrain.standard_attack_cooldown * 0.5f);
					component2.no_more_initial_stun = true;
				}
			}
		}
		if (mob_type != TYPE_T.stationary && mob_type != TYPE_T.mineral && (mob_type != TYPE_T.creature || !GetComponent<SharedCreature>().is_local_mob))
		{
			return;
		}
		bool flag3 = false;
		bool flag4 = false;
		if (attacker != base.gameObject && attacker == GameController.Instance.player)
		{
			switch (inventory_ctr.Instance.player_inventory[15].item.item_name)
			{
			case "Dark Hasta":
			case "Dark SwirlSword":
			case "Dark Scythe":
			case "Ancient SwirlSword":
			case "Ancient Scythe":
			case "Ancient Lance":
				if (Random.value < 0.01f)
				{
					flag3 = true;
				}
				break;
			case "Spirit Scythe":
			case "Spirit Lance":
				if (Random.value < 0.01f)
				{
					flag4 = true;
				}
				break;
			}
		}
		int respawn_secs = ((original_element_item.GetString("bandit_camp_instance") != "" || InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name)) ? 7200 : respawn_time);
		if (!flag3 && !flag4)
		{
			if (!(hp > 0f))
			{
				if (original_element_item.GetString("is_killGoal_mob") == "true" && QuestControl.Instance.doing_time_trial)
				{
					WindowPrefabsControl.Instance.GetScreen("QUEST COUNTDOWN").GetComponent<QuestCountdown>().IncreaseKills();
				}
				GameServerSender.Instance.SendMobDie(text, 0.5f, 0f, origin_zone, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, respawn_secs, attacker, netMobType, darksword_kill: false, aether_banish: false, original_element_item, "");
				Die(attacker, 0.5f, 0f, respawn_secs);
			}
			return;
		}
		GameServerSender.Instance.SendMobDie(text, 2f, 1f, origin_zone, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, respawn_secs, attacker, netMobType, flag3, flag4, original_element_item, "");
		BeginDarkswordParticles(flag3 ? PerkControl.Instance.prefab_darksword_kill : (flag4 ? PerkControl.Instance.prefab_aether_banish : null));
		Die(attacker, 2f, 1f, respawn_secs);
	}

	public void BeginDarkswordParticles(GameObject particle_prefab)
	{
		healthbar.SetActive(false);
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().isKinematic = true;
		}
		GameObject gameObject = Object.Instantiate(particle_prefab);
		gameObject.transform.position = base.transform.position;
		base.transform.SetParent(gameObject.transform.Find("creature goes here"));
	}

	public bool ShouldGiveExpAndDropsOnDie(GameObject killer)
	{
		if (killer == null)
		{
			return false;
		}
		if (mob_type == TYPE_T.creature)
		{
			if (GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.wolf_pack)
			{
				return false;
			}
			if (GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.ghost)
			{
				return false;
			}
		}
		if (killer.GetComponent<PerkReceiver>().HasPerkRemaining("perk_giant"))
		{
			return false;
		}
		return !killer.GetComponent<PerkReceiver>().HasPerkRemaining("perk_cockroach_squish");
	}

	public void Die(GameObject killa, float delay, float splat_delay, int respawn_secs)
	{
		if (!is_dead)
		{
			hp = 0f;
			is_dead = true;
			if (GameServerInterface.Instance.nearby_players.ContainsKey(combat_name))
			{
				OnlinePlayer onlinePlayer = GameServerInterface.Instance.nearby_players[combat_name];
				onlinePlayer.currently_using = "";
				onlinePlayer.sitting_in_chair = "";
			}
			StartCoroutine(DieCoroutine(killa, delay, splat_delay, respawn_secs));
		}
	}

	private IEnumerator DieCoroutine(GameObject killer, float delay, float splat_delay, int respawn_secs)
	{
		string random_fn_validator = "";
		if (killer != null && killer == GameController.Instance.player)
		{
			GameController.Instance.HideTargetCircle();
			GameController.Instance.resume_input_on_next_click = true;
		}
		if (base.gameObject == GameController.Instance.player)
		{
			GameController.Instance.PlayerDied();
		}
		if (mob_type == TYPE_T.creature)
		{
			SharedCreature component = GetComponent<SharedCreature>();
			component.isDying = true;
			if (component.is_local_mob)
			{
				if (component.brain_type == SharedCreature.brain_type_t.guard)
				{
					if (original_element_item.GetString("tag") != "dev_obj")
					{
						ConstructionControl.Instance.PlayerRemoveAt(new ChunkElement(original_element_item, original_element_rot), origin_zone, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, ConstructionControl.remove_context.self_remove, ConstructionControl.GenerateCacheKey());
						string creature_name = component.creature_name;
						string @string = original_element_item.GetString("companion_owner");
						if (!CompanionController.Instance.WasMyGuard(@string))
						{
							GameServerSender.Instance.SendGuardDieNotif(creature_name, @string);
						}
						CompanionController.Instance.OnGuardDie(creature_name, @string);
					}
				}
				else if (component.brain_type == SharedCreature.brain_type_t.companion)
				{
					CreatureBrainCompanion component2 = GetComponent<CreatureBrainCompanion>();
					if (component2.companion_struct != null)
					{
						CompanionController.Instance.CompanionDeath(component2.companion_struct);
					}
				}
			}
		}
		yield return new WaitForSeconds(delay);
		if (delay != 0.5f && healthbar != null)
		{
			healthbar.SetActive(true);
			ShowVisualSplat(hit_col.color_red, 999, -1, miss: false, dodge: false);
		}
		yield return new WaitForSeconds(splat_delay);
		if (killer != null)
		{
			bool flag = false;
			bool flag2 = false;
			if (GameController.Instance.player != null && killer == GameController.Instance.player)
			{
				flag = true;
			}
			else if (killer.GetComponent<Combatant>().mob_type == TYPE_T.creature && killer.GetComponent<SharedCreature>().is_local_mob && killer.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.companion)
			{
				flag2 = true;
			}
			if ((flag || flag2) && mob_type == TYPE_T.creature)
			{
				int num = GetComponent<SharedCreature>().level - killer.GetComponent<SharedCreature>().level;
				if (num < 1)
				{
					exp_recieve = (int)Mathf.Lerp(10f, 45f, ((float)num + 15f) / 15f);
				}
				else
				{
					exp_recieve = (int)Mathf.Lerp(45f, 90f, (float)num / 15f);
				}
			}
			if (!ShouldGiveExpAndDropsOnDie(killer))
			{
				exp_recieve = 0;
			}
			if (flag)
			{
				if (mob_type == TYPE_T.creature && GameController.Instance.player != null && !GameController.Instance.player.GetComponent<PerkReceiver>().HasPerkRemaining("perk_giant"))
				{
					AchievesControl.Instance.UnlockAchievement("Carnivore");
					int level = GetComponent<SharedCreature>().level;
					if (level > 14)
					{
						AchievesControl.Instance.UnlockAchievement("Apex Predator");
						if (level > 39)
						{
							AchievesControl.Instance.UnlockAchievement("The Mighty Hunt");
						}
					}
				}
				GameController.Instance.currentEXP += exp_recieve;
				GameController.Instance.SaveCurrentExpToDisk();
				GameController.Instance.animate_exp_bar = true;
				string text = "";
				if (item_recieve_on_kill != "")
				{
					text = "+" + count_recieve_on_kill + " " + item_recieve_on_kill;
					inventory_ctr.Instance.GiveItem(item_recieve_on_kill, count_recieve_on_kill, random_fn_validator);
				}
				else if (exp_recieve != 0)
				{
					text = "+" + exp_recieve + " Exp";
					GameController.Instance.showOverheadNotif(text, base.transform.position, sound: true, delete_on_many: true);
				}
				GameServerSender.Instance.SendShowExpReceive(text, base.transform.position);
			}
			else if (flag2 && killer.GetComponent<SharedCreature>().is_local_mob)
			{
				CreatureBrainCompanion component3 = killer.GetComponent<CreatureBrainCompanion>();
				if (exp_recieve != 0)
				{
					string text2 = "+" + exp_recieve + " Exp";
					GameController.Instance.showOverheadNotif(text2, killer.transform.position, sound: false, delete_on_many: false);
					CompanionController.Instance.IncreaseCompanionExp(exp_recieve, component3.companion_struct);
					GameServerSender.Instance.SendShowExpReceive(text2, killer.transform.position);
				}
			}
		}
		if (killer != null && killer == GameController.Instance.player && mob_type == TYPE_T.creature && ShouldGiveExpAndDropsOnDie(killer))
		{
			string mob_file_name = MobControl.Instance.ItemToMobFileName(original_element_item);
			MobFile mobFile = MobControl.Instance.GetMobFile(mob_file_name);
			bool flag3;
			bool flag4;
			bool flag5;
			if (mobFile == null)
			{
				flag3 = true;
				flag4 = false;
				flag5 = GetComponent<SharedCreature>().hat_.item_name != "" || GetComponent<SharedCreature>().body_.item_name != "" || GetComponent<SharedCreature>().hand_.item_name != "";
			}
			else
			{
				flag3 = mobFile.Drops.Contains("WILD_MOB_DROPS");
				bool num2 = mobFile.Drops.Contains("WEAPON_AND_ARMOR");
				flag4 = mobFile.Drops.Contains("BOSS_KEY");
				flag5 = num2 && (GetComponent<SharedCreature>().hat_.item_name != "" || GetComponent<SharedCreature>().body_.item_name != "" || GetComponent<SharedCreature>().hand_.item_name != "");
			}
			int num3;
			if (flag4)
			{
				num3 = 1;
			}
			else
			{
				num3 = 0;
				if (base.transform.localScale.x >= 1.5f)
				{
					float x = base.transform.localScale.x;
					float value = Random.value;
					num3 = ((!(x >= 2.5f)) ? ((value >= 0.75f) ? 1 : 2) : ((value < 0.75f) ? 3 : 2));
				}
				else
				{
					num3 = ((!(Random.value >= 0.75f)) ? 1 : 0);
				}
			}
			Debug.Log("n_drops=" + num3);
			switch (num3)
			{
			case 3:
			{
				ItemCountPair dropFromEnabledCategories = GetDropFromEnabledCategories(flag3, flag5, flag4);
				ItemCountPair dropFromEnabledCategories2 = GetDropFromEnabledCategories(flag3, flag5, flag4);
				ItemCountPair dropFromEnabledCategories3 = GetDropFromEnabledCategories(flag3, flag5, flag4);
				float num4 = Random.Range(0, 360);
				SpawnDrop(num4, 0.4f, dropFromEnabledCategories);
				SpawnDrop(num4 + 120f, 0.4f, dropFromEnabledCategories2);
				SpawnDrop(num4 + 240f, 0.4f, dropFromEnabledCategories3);
				break;
			}
			case 2:
			{
				ItemCountPair dropFromEnabledCategories4 = GetDropFromEnabledCategories(flag3, flag5, flag4);
				ItemCountPair dropFromEnabledCategories5 = GetDropFromEnabledCategories(flag3, flag5, flag4);
				float num5 = Random.Range(0, 360);
				SpawnDrop(Random.Range(num5 + 30f, num5 + 150f), 0.4f, dropFromEnabledCategories4);
				SpawnDrop(Random.Range(num5 + 210f, num5 + 330f), 0.4f, dropFromEnabledCategories5);
				break;
			}
			case 1:
				SpawnDrop(0f, 0f, GetDropFromEnabledCategories(flag3, flag5, flag4));
				break;
			}
		}
		Delete();
		if (origin_zone == "" && origin_chunkX == 0 && origin_chunkZ == 0 && origin_innerX == 0 && origin_innerZ == 0)
		{
			Object.Destroy(base.gameObject);
			yield break;
		}
		if (original_element_item.item_name != "Flagpole")
		{
			QuestControl.Instance.TryNoteQuestMobKilled(origin_zone, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, original_element_item, combat_name);
			InventoryItem new_item = ChunkControl.Instance.EncodeRespawnIntoItem("mob_spawn", original_element_item, System.DateTime.UtcNow.AddSeconds(respawn_secs));
			ConstructionControl.Instance.PlayerReplaceAt(new_item, original_element_item, original_element_rot, origin_zone, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, false, ConstructionControl.GenerateCacheKey());
			QuestControl.Instance.CheckIfAllQuestMobsKilled(origin_zone, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, original_element_item, combat_name);
		}
		else
		{
			string string2 = original_element_item.GetString("bandit_camp_instance");
			BanditCampInstance banditCampInstanceByName = BanditCampsControl.Instance.GetBanditCampInstanceByName(string2);
			banditCampInstanceByName.flag_destroyed = true;
			banditCampInstanceByName.SaveToDisk();
			ConstructionControl.Instance.PlayerRemoveAt(new ChunkElement(original_element_item, original_element_rot), origin_zone, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, ConstructionControl.remove_context.self_remove, ConstructionControl.GenerateCacheKey());
			GameServerSender.Instance.SendBanditFlagDestroyed(string2);
		}
		Object.Destroy(base.gameObject);
	}

	private ItemCountPair GetDropFromEnabledCategories(bool WILD_MOB_DROPS, bool WEAPON_AND_ARMOR, bool BOSS_KEY)
	{
		float num = (WILD_MOB_DROPS ? 2f : 0f);
		float num2 = (WEAPON_AND_ARMOR ? 1f : 0f);
		float num3 = (BOSS_KEY ? 0.5f : 0f);
		float num4 = num + num2 + num3;
		if (num4 > 0f)
		{
			float num5 = num4 * Random.value;
			if (num5 < num && WILD_MOB_DROPS)
			{
				return GetWildMobDrop();
			}
			num5 -= num;
			if (num5 < num2 && WEAPON_AND_ARMOR)
			{
				return GetWeaponAndArmorDrop();
			}
			if (num5 - num2 < num3 && BOSS_KEY)
			{
				return GetBossKeyDrop();
			}
			Debug.Log("got to 'null'");
		}
		return null;
	}

	private ItemCountPair GetWildMobDrop()
	{
		float num = ((!(base.transform.localScale.x >= 1.5f)) ? 0.01f : ((!(base.transform.localScale.x >= 2.5f)) ? 0.05f : 0.2f));
		if (Random.value < num)
		{
			return new ItemCountPair(GetEggDrop(), 1);
		}
		ItemCountPair itemCountPair = new ItemCountPair(GetComponent<SharedCreature>().myCreatureModel.original.drop1, 1);
		ItemCountPair itemCountPair2 = new ItemCountPair(GetComponent<SharedCreature>().myCreatureModel.original.drop2, 1);
		List<ItemCountPair> list = new List<ItemCountPair>();
		if (itemCountPair.item.item_name != "")
		{
			list.Add(itemCountPair);
		}
		if (itemCountPair2.item.item_name != "")
		{
			list.Add(itemCountPair2);
		}
		list.Add(GetCoinsDrop());
		return list[Random.Range(0, list.Count)];
	}

	private ItemCountPair GetWeaponAndArmorDrop()
	{
		List<ItemCountPair> list = new List<ItemCountPair>();
		if (GetComponent<SharedCreature>().hat_.item_name != "")
		{
			list.Add(new ItemCountPair(GetComponent<SharedCreature>().hat_, 1));
		}
		if (GetComponent<SharedCreature>().body_.item_name != "")
		{
			list.Add(new ItemCountPair(GetComponent<SharedCreature>().body_, 1));
		}
		if (GetComponent<SharedCreature>().hand_.item_name != "")
		{
			list.Add(new ItemCountPair(GetComponent<SharedCreature>().hand_, 1));
		}
		if (list.Count != 0)
		{
			return list[Random.Range(0, list.Count)];
		}
		return new ItemCountPair("", 0);
	}

	private ItemCountPair GetBossKeyDrop()
	{
		string @string = original_element_item.GetString("bandit_camp_instance");
		ExtraInventoryData extraInventoryData = new ExtraInventoryData();
		extraInventoryData.SetString("key_name", @string);
		return new ItemCountPair(new InventoryItem("Key", extraInventoryData), 1);
	}

	private InventoryItem GetEggDrop()
	{
		ExtraInventoryData extraInventoryData = new ExtraInventoryData();
		List<string> creatures_that_made_me = GetComponent<SharedCreature>().myCreatureModel.original.creatures_that_made_me;
		int index = Random.Range(0, GetComponent<SharedCreature>().myCreatureModel.original.creatures_that_made_me.Count);
		extraInventoryData.SetString("egg_monster", creatures_that_made_me[index]);
		return new InventoryItem("Egg", extraInventoryData);
	}

	private ItemCountPair GetCoinsDrop()
	{
		float x = base.gameObject.transform.localScale.x;
		int min;
		int max;
		if (x >= 1.5f)
		{
			float value = Random.value;
			if ((x >= 2.5f) ? (value < 0.333f) : (value >= 0.333f))
			{
				min = 15;
				max = 40;
			}
			else
			{
				min = 4;
				max = 15;
			}
		}
		else
		{
			min = 4;
			max = 15;
		}
		return new ItemCountPair("Coins", Random.Range(min, max));
	}

	private void SpawnDrop(float angle, float d, ItemCountPair drop_pair)
	{
		Vector3 position = base.transform.position;
		float f = angle * ((float)System.Math.PI / 180f);
		MobControl.Instance.TrySpawnDrop(drop_pair, new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f)) * d + position);
	}

	private void FixedUpdate()
	{
		if (healthbar != null)
		{
			MobControl.Instance.SnapOverhead((RectTransform)healthbar.transform, base.transform.position);
			yellow_width = Mathf.Lerp(yellow_width, yellow_goto_width, Time.deltaTime * 7f);
			yellow_X = Mathf.Lerp(yellow_X, yellow_goto_X, Time.deltaTime * 7f);
			healthbarYellow.rectTransform.sizeDelta = new Vector2(yellow_width, 9.4f);
			healthbarYellow.transform.localPosition = new Vector3(yellow_X, healthbarYellow.transform.localPosition.y, healthbarYellow.transform.localPosition.z);
		}
	}

	private void ShowVisualSplat(hit_col col_, int damage, int fake_damage, bool miss, bool dodge)
	{
		if (mob_type == TYPE_T.mineral)
		{
			AudioClip[] sfx_ore_impact = GameController.Instance.sfx_ore_impact;
			AudioSource sfx = GameController.Instance.sfx;
			sfx.PlayOneShot(sfx_ore_impact[Random.Range(0, sfx_ore_impact.Length)], AudioControl.Instance.general_sfx_volume * 0.31f);
		}
		else
		{
			AudioControl instance = AudioControl.Instance;
			AudioClip[] sfx_flesh_impact = GameController.Instance.sfx_flesh_impact;
			instance.PlayPitch(sfx_flesh_impact[Random.Range(0, 5)], 1f, 0.6f);
		}
		GameController.Instance.AssignHealthbar(base.gameObject);
		GameObject gameObject = healthbar.transform.Find("splat").gameObject;
		Color color = col_ switch
		{
			hit_col.color_red => MobControl.Instance.color_dmg_hit,
			hit_col.color_blue => MobControl.Instance.color_dmg_miss,
			hit_col.color_yellow => MobControl.Instance.color_dmg_mine,
			hit_col.color_green => MobControl.Instance.color_dmg_poison,
			hit_col.color_purple => MobControl.Instance.color_dmg_vampire,
			_ => Color.black,
		};
		if (damage == 0)
		{
			if (color != MobControl.Instance.color_dmg_mine)
			{
				color = MobControl.Instance.color_dmg_miss;
			}
			if (miss)
			{
				splatDmg.text = "MISS";
				gameObject.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(175f, 140f);
			}
			else if (dodge)
			{
				splatDmg.text = "DODGE";
				gameObject.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(175f, 140f);
			}
			else
			{
				splatDmg.text = "0";
				splatDmg.transform.localPosition = new Vector3(0f, 0f, 0f);
				gameObject.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(140f, 140f);
			}
		}
		else
		{
			splatDmg.text = ((fake_damage == -1) ? damage : fake_damage).ToString();
			gameObject.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(140f, 140f);
		}
		gameObject.GetComponent<Image>().color = color;
		gameObject.GetComponent<Animation>().Stop();
		gameObject.GetComponent<Animation>().PlayQueued("splatAppear");
		if (animate_hit != null)
		{
			StopCoroutine(animate_hit);
		}
		animate_hit = AnimateHitCoroutine();
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(animate_hit);
		}
		SetHealthVisual(damage);
		bool flag = GetComponent<SharedCreature>() != null;
		if (damage == 0 || !flag)
		{
			return;
		}
		bool num = base.gameObject == GameController.Instance.player;
		bool flag2 = GetComponent<SharedCreature>().IsBig();
		if (num)
		{
			if (flag2)
			{
				GameController.Instance.sound_player_hit(1f, 0.57f);
			}
			else
			{
				GameController.Instance.sound_player_hit(0.75f, 1f);
			}
		}
		else if (flag2)
		{
			GameController.Instance.sound_creature_hit(1f, 0.57f);
		}
		else
		{
			GameController.Instance.sound_creature_hit(0.8f, 1f);
		}
	}

	public void ReceiveHealthbar(GameObject healthbar, Camera mainCamera)
	{
		GameController.Instance.possible_destroy.Add(healthbar);
		this.mainCamera = mainCamera;
		this.healthbar = healthbar;
		healthbarGreen = healthbar.transform.Find("GREEN").GetComponent<Image>();
		healthbarYellow = healthbar.transform.Find("yellow").GetComponent<Image>();
		splatDmg = healthbar.transform.Find("splat").Find("dmg").GetComponent<Text>();
		healthbar.transform.SetParent(MobControl.Instance.gameObject.transform);
		healthbar.transform.SetAsFirstSibling();
		healthbar.transform.localRotation = Quaternion.identity;
		healthbar.transform.localPosition = Vector2.zero;
		healthbar.transform.localScale = Vector3.one * 0.85f;
		Transform transform = healthbar.transform;
		Vector2 vector = Vector2.one * 10f;
		((RectTransform)healthbar.transform).anchorMax = vector;
		((RectTransform)transform).anchorMin = vector;
		hit_start_pos = healthbar.transform.Find("hit").transform.localPosition;
		RefreshHealthbarDisappearTimer(5f);
	}

	public void SetHealthVisual(int damage)
	{
		float num = 0f;
		if (HP_max != 0)
		{
			num = hp / (float)HP_max * 100f;
		}
		float num2 = hp - (float)damage;
		yellow_X = (100f - num) * -0.5f;
		yellow_width = num;
		hp = num2;
		float num3 = 0f;
		if (HP_max != 0)
		{
			num3 = num2 / (float)HP_max * 100f;
		}
		yellow_goto_X = (100f - num3) * -0.5f;
		yellow_goto_width = num3;
		if (healthbarGreen != null)
		{
			healthbarGreen.rectTransform.sizeDelta = new Vector2(yellow_goto_width, 9.4f);
			healthbarGreen.transform.localPosition = new Vector3(yellow_goto_X, healthbarGreen.transform.localPosition.y, healthbarGreen.transform.localPosition.z);
		}
		if (mob_type == TYPE_T.creature)
		{
			SharedCreature component = GetComponent<SharedCreature>();
			if (component.brain_type == SharedCreature.brain_type_t.companion && component.is_local_mob)
			{
				ActiveCompanion companion_struct = GetComponent<CreatureBrainCompanion>().companion_struct;
				if (companion_struct != null)
				{
					CompanionController.Instance.SetCompanionGuiHealth(companion_struct.hatch_index, num3);
				}
			}
		}
	}

	private IEnumerator AnimateHitCoroutine()
	{
		healthbar.transform.Find("hit").GetComponent<Image>().enabled = true;
		healthbar.transform.Find("hit").transform.localPosition = hit_start_pos + Random.insideUnitCircle * 23f;
		healthbar.transform.Find("hit").transform.localRotation = Quaternion.Euler(0f, 0f, Random.value * 360f);
		for (int i = 0; i < 5; i++)
		{
			if (healthbar != null)
			{
				healthbar.transform.Find("hit").GetComponent<Image>().sprite = GameController.Instance.hit_sprite_frames[i];
			}
			yield return new WaitForSeconds(0.07f);
		}
		if (healthbar != null)
		{
			healthbar.transform.Find("hit").GetComponent<Image>().enabled = false;
		}
	}

	public void RefreshHealthbarDisappearTimer(float timer)
	{
		if (disappearHealthbar != null)
		{
			StopCoroutine(disappearHealthbar);
		}
		disappearHealthbar = DisappearHealthbar(timer);
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(disappearHealthbar);
		}
	}

	private IEnumerator DisappearHealthbar(float timer)
	{
		yield return new WaitForSeconds(timer);
		if (hp == (float)HP_max)
		{
			RemoveHealthbar();
			SharedCreature component = GetComponent<SharedCreature>();
			if (component != null && component.brain_type != SharedCreature.brain_type_t.local_player && component.brain_type != SharedCreature.brain_type_t.net_player && DialogueControl.Instance.focus_type == DialogueControl.focus_type_t.none)
			{
				component.RedrawLevelDisplay();
			}
		}
		else
		{
			RefreshHealthbarDisappearTimer(timer);
		}
	}

	public bool WithinHitbox(Vector3 position, bool clamp)
	{
		Vector3 position2 = base.transform.position;
		float num = ((!clamp) ? (scale * 0.8f) : Mathf.Max(scale * 0.8f, min_target_scale));
		return Vector3.Distance(position2, position) < num;
	}

	public bool IgnoreFireDamage()
	{
		if (GetComponent<Combatant>().mob_type != TYPE_T.creature)
		{
			return false;
		}
		string item_name = GetComponent<Combatant>().original_element_item.item_name;
		if (!InventoryUtils.IsHellDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			return false;
		}
		if (!(item_name == "Mob - Normal") && !(item_name == "Mob - Tiny") && !(item_name == "Mob - Big") && !(item_name == "Mob - Giant"))
		{
			return item_name == "Mob - Vengeful Spirit";
		}
		return true;
	}

	public void RecursiveApplyMaterial(Transform T, Material mat)
	{
		MeshRenderer component = T.gameObject.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.material = mat;
		}
		int childCount = T.childCount;
		for (int i = 0; i < childCount; i++)
		{
			RecursiveApplyMaterial(T.GetChild(i), mat);
		}
	}
}
