using UnityEngine;

public class CombatControl : MonoBehaviour, OrderedStart
{
	public enum slider_type
	{
		attack = 0,
		health = 1,
		accuracy = 2,
		dodge = 3,
		mana_recharge = 4,
		health_recharge = 5
	}

	public static CombatControl Instance;

	public bool shown_attack_flagpole_notif;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public bool HitAllowed(GameObject attacker, GameObject defender, bool this_is_a_net_player)
	{
		Combatant combatant = null;
		if (attacker != null)
		{
			combatant = attacker.GetComponent<Combatant>();
		}
		Combatant component = defender.GetComponent<Combatant>();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (GameServerConnector.Instance.FullyInGame() && attacker != null)
		{
			flag = GameServerInterface.Instance.nearby_players.ContainsKey(combatant.combat_name);
			if (attacker.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.companion)
			{
				flag2 = true;
			}
			else
			{
				flag3 = attacker.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.wolf_pack;
			}
		}
		if (component.mob_type != Combatant.TYPE_T.creature)
		{
			return true;
		}
		if (defender == GameController.Instance.player && ConsoleControl.Instance.god_mode_enabled)
		{
			return false;
		}
		if (!GameServerConnector.Instance.FullyInGame() || !GameServerConnector.Instance.pvp_enabled)
		{
			bool flag4;
			if (defender == GameController.Instance.player)
			{
				flag4 = flag;
			}
			else
			{
				flag4 = attacker == GameController.Instance.player;
				if (!this_is_a_net_player)
				{
					if (flag2 || flag3 || flag || flag4)
					{
						if (defender.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.companion)
						{
							return false;
						}
						if (defender.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.guard && component.original_element_item.GetString("tag") != "dev_obj")
						{
							return false;
						}
					}
					goto IL_end;
				}
			}
			if (flag2 || flag3 || flag4)
			{
				return false;
			}
		}
		IL_end:
		if (attacker == GameController.Instance.player && defender.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.companion && defender.GetComponent<SharedCreature>().is_local_mob)
		{
			return false;
		}
		return true;
	}

	public void CalculateHitLocally(GameObject attacker)
	{
		bool attacker_missed = false;
		bool defender_dodged = false;
		Combatant component = attacker.GetComponent<Combatant>();
		GameObject main_target = attacker.GetComponent<CreatureBrain>().main_target;
		if (main_target == null)
		{
			return;
		}
		Combatant component2 = main_target.GetComponent<Combatant>();
		if (main_target == null || GameController.Instance.is_paused() || component2.is_dead || component.is_dead)
		{
			return;
		}
		attacker_missed = false;
		defender_dodged = false;
		string apply_perk = "";
		int actual_damage = 0;
		int fake_damage = -1;
		Combatant.hit_col col_;
		if (component2.mob_type == Combatant.TYPE_T.mineral)
		{
			CalculateMiningHit(attacker, main_target, ref actual_damage, ref fake_damage, ref attacker_missed, ref defender_dodged, ref apply_perk);
			col_ = Combatant.hit_col.color_yellow;
		}
		else
		{
			CalculateCreatureHit(attacker, main_target, ref actual_damage, ref fake_damage, ref attacker_missed, ref defender_dodged, ref apply_perk);
			col_ = Combatant.hit_col.color_red;
		}
		if (!(main_target != null))
		{
			return;
		}
		component2.WasHit(actual_damage, attacker, attacker_missed, defender_dodged, col_, send_hit: true, fake_damage);
		if (apply_perk != "" && main_target != null && !main_target.GetComponent<PerkReceiver>().HasPerkRemaining(apply_perk))
		{
			string combat_name = component.combat_name;
			PerkData perkData = PerkControl.Instance.ClonePerkForCasting(apply_perk, attacker);
			if (apply_perk == "perk_ignite")
			{
				int num = (int)((float)component.HP_max * 0.1f);
				if (num == 0)
				{
					num = 1;
				}
				perkData.all_effects["EFFECT_BURN"].data["Damage"] = num.ToString() ?? "";
			}
			PerkControl.Instance.ApplyInitialCastOnto(perkData, 1, combat_name, 1, main_target);
		}
	}

	private void CalculateCreatureHit(GameObject attacker, GameObject defender, ref int actual_damage, ref int fake_damage, ref bool attacker_missed, ref bool defender_dodged, ref string apply_perk)
	{
		Combatant component = attacker.GetComponent<Combatant>();
		SharedCreature component2 = attacker.GetComponent<SharedCreature>();
		PerkReceiver component3 = attacker.GetComponent<PerkReceiver>();
		Combatant component4 = defender.GetComponent<Combatant>();
		Combatant.TYPE_T mob_type = component4.mob_type;
		PerkReceiver component5 = defender.GetComponent<PerkReceiver>();
		float num = 1f;
		foreach (DurationEffect duration_effect in component3.duration_effects)
		{
			float @float = duration_effect.GetFloat("Accuracy Percent Mod On Attack Other", "%");
			if (@float != -1f)
			{
				num *= @float * 0.01f;
			}
		}
		foreach (DurationEffect duration_effect2 in component5.duration_effects)
		{
			float float2 = duration_effect2.GetFloat("Accuracy Percent Mod On Was-Hit", "%");
			if (float2 != -1f)
			{
				num *= float2 * 0.01f;
			}
		}
		if (attacker == GameController.Instance.player)
		{
			float t = CalcCombatSlider(slider_type.accuracy);
			if (!(Random.value < num * Mathf.Lerp(0.82f, 1f, t)))
			{
				if (component4.mob_type == Combatant.TYPE_T.creature && !(Random.value < 0.75f))
				{
					defender_dodged = true;
				}
				else
				{
					attacker_missed = true;
				}
			}
		}
		else if (defender == GameController.Instance.player)
		{
			float t2 = CalcCombatSlider(slider_type.dodge);
			if (!(Random.value < num * Mathf.Lerp(0.82f, 0.73f, t2)))
			{
				if (Random.value >= 0.75f)
				{
					attacker_missed = true;
				}
				else
				{
					defender_dodged = true;
				}
			}
		}
		else if (!(Random.value < num * 0.8f))
		{
			if (component4.mob_type == Combatant.TYPE_T.creature && Random.value >= 0.5f)
			{
				defender_dodged = true;
			}
			else
			{
				attacker_missed = true;
			}
		}
		if (!attacker_missed && !defender_dodged)
		{
			float num2 = component.HP_max;
			float t3 = 0f;
			int num3;
			if (attacker == GameController.Instance.player)
			{
				t3 = CalcCombatSlider(slider_type.attack);
				num3 = (int)((Random.value < Mathf.Lerp(0f, 0.45f, t3)) ? (num2 * 0.0666f) : (num2 * 0.0333f));
				if (num3 == 0)
				{
					num3 = 1;
				}
			}
			else
			{
				float num4 = Mathf.Clamp01((float)component2.level / 15f);
				num3 = ((Random.value > num4) ? 1 : 0);
			}
			float t4 = CalcCombatSlider(slider_type.accuracy);
			float min;
			float max;
			if (Random.value < Mathf.Lerp(0.23f, 0.28f, t4))
			{
				min = num2 * 0.08f;
				max = Mathf.Lerp(0.1f, 0.13f, t3) * num2;
			}
			else
			{
				min = num2 * 0.018f;
				max = num2 * 0.03f;
			}
			actual_damage = (int)Random.Range(min, max) + num3;
			float num5 = 1f;
			foreach (DurationEffect duration_effect3 in component3.duration_effects)
			{
				float float3 = duration_effect3.GetFloat("Damage Percent Mod On Attack Other", "%");
				if (float3 != -1f)
				{
					num5 *= float3 * 0.01f;
				}
				int @int = duration_effect3.GetInt("Additional Damage Points On Attack Other");
				if (@int != -1)
				{
					actual_damage += @int;
				}
				int int2 = duration_effect3.GetInt("Fewer Damage Points On Attack Other");
				if (int2 != -1)
				{
					actual_damage -= int2;
				}
			}
			foreach (DurationEffect duration_effect4 in component5.duration_effects)
			{
				float float4 = duration_effect4.GetFloat("Damage Percent Mod On Was-Hit", "%");
				if (float4 != -1f)
				{
					num5 *= float4 * 0.01f;
				}
				int int3 = duration_effect4.GetInt("Fewer Damage Points On Was-Hit");
				if (int3 != -1)
				{
					actual_damage -= int3;
				}
			}
			actual_damage = (int)(num5 * (float)actual_damage);
			if (attacker == GameController.Instance.player || component2.brain_type == SharedCreature.brain_type_t.guard || (component2.brain_type == SharedCreature.brain_type_t.companion && attacker.GetComponent<CreatureBrainCompanion>().companion_struct.is_temp_companion))
			{
				ProcessAttackerWeapon(attacker, ref actual_damage, ref apply_perk);
			}
			string combat_name = component4.combat_name;
			if (defender.GetComponent<SharedCreature>() != null)
			{
				bool flag;
				if (defender == GameController.Instance.player)
				{
					flag = true;
				}
				else
				{
					SharedCreature.brain_type_t brain_type = defender.GetComponent<SharedCreature>().brain_type;
					SharedCreature component6 = defender.GetComponent<SharedCreature>();
					flag = ((brain_type != SharedCreature.brain_type_t.companion) ? (component6.brain_type == SharedCreature.brain_type_t.guard || GameServerInterface.Instance.nearby_players.ContainsKey(combat_name)) : (!component6.is_local_mob || !defender.GetComponent<CreatureBrainCompanion>().companion_struct.is_temp_companion));
				}
				if (flag)
				{
					ProcessDefenderArmor(defender, ref actual_damage);
				}
			}
		}
		if (actual_damage < 0)
		{
			actual_damage = 0;
		}
		switch (mob_type)
		{
		case Combatant.TYPE_T.stationary:
			fake_damage = actual_damage;
			if (actual_damage > 1)
			{
				actual_damage = 1;
			}
			break;
		case Combatant.TYPE_T.creature:
			if (component5.HasPerkRemaining("perk_giant"))
			{
				actual_damage = 0;
			}
			break;
		}
	}

	private void ProcessAttackerWeapon(GameObject attacker, ref int actual_damage, ref string apply_perk)
	{
		string item_name = attacker.GetComponent<SharedCreature>().hand_.item_name;
		if (string.IsNullOrWhiteSpace(item_name))
		{
			return;
		}
		int intFromItemFile = ResourceControl.Instance.GetIntFromItemFile(item_name, "Damage Base");
		if (Random.value < 0.666f)
		{
			int num = actual_damage;
			actual_damage = Random.Range(intFromItemFile + 1, (int)((float)intFromItemFile * 1.5f)) + num;
		}
		else
		{
			actual_damage += intFromItemFile;
		}
		string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item_name, "Damage Perk");
		if (!string.IsNullOrWhiteSpace(stringFromItemFile))
		{
			float floatFromItemFile = ResourceControl.Instance.GetFloatFromItemFile(item_name, "Damage Perk Odds");
			if (Random.value < floatFromItemFile)
			{
				apply_perk = stringFromItemFile;
			}
			Debug.Log("Damage Perk [" + stringFromItemFile + "-" + floatFromItemFile + "]");
		}
	}

	private void ProcessDefenderArmor(GameObject defender, ref int actual_damage)
	{
		if (!(Random.value < 0.6f))
		{
			return;
		}
		string text = "";
		string text2 = "";
		if (defender.GetComponent<SharedCreature>() != null)
		{
			text = defender.GetComponent<SharedCreature>().hat_.item_name;
			text2 = defender.GetComponent<SharedCreature>().body_.item_name;
		}
		if (text != "")
		{
			float floatFromItemFile = ResourceControl.Instance.GetFloatFromItemFile(text, "Defend Amount");
			int num = (int)((Random.value < 0.5f) ? Mathf.Floor(floatFromItemFile) : Mathf.Ceil(floatFromItemFile));
			if (Random.value >= 0.66f)
			{
				num = (int)((float)num * 0.666f);
			}
			actual_damage -= num;
		}
		if (text2 != "")
		{
			float floatFromItemFile2 = ResourceControl.Instance.GetFloatFromItemFile(text2, "Defend Amount");
			int num2 = (int)((Random.value < 0.5f) ? Mathf.Floor(floatFromItemFile2) : Mathf.Ceil(floatFromItemFile2));
			if (Random.value >= 0.66f)
			{
				num2 = (int)((float)num2 * 0.666f);
			}
			actual_damage -= num2;
		}
	}

	private void CalculateMiningHit(GameObject attacker, GameObject defender, ref int actual_damage, ref int fake_damage, ref bool missed, ref bool dodged, ref string apply_perk)
	{
		string text = "";
		switch (defender.GetComponent<Combatant>().original_element_item.item_name)
		{
		case "Stone Vein":
		case "Stone Vein (Ocean)":
		case "Large Stone Vein":
		case "Stone Vein (Snowy)":
		case "Stone Vein (White)":
		case "Stone Vein (Desert)":
		case "Stone Vein (Sakura)":
			text = "Wood Pick";
			break;
		case "Metal Vein":
		case "Large Metal Vein":
			text = "Stone Pick";
			break;
		case "Large Sapphire Vein":
		case "Large Gold Vein":
		case "Sapphire Vein":
		case "Large Emerald Vein":
		case "Ruby Vein":
		case "Amber Vein":
		case "Emerald Vein":
		case "Large Amethyst Vein":
		case "Large Titanium Vein":
		case "Silver Vein":
		case "Large Ruby Vein":
		case "Gold Vein":
		case "Titanium Vein (Sakura)":
		case "Titanium Vein":
		case "Large Silver Vein":
		case "Large Amber Vein":
		case "Amethyst Vein":
			text = "Metal Pick";
			break;
		case "Large Uranium Vein":
		case "Large Ice Shard Vein":
		case "Magmite Vein":
		case "Uranium Vein":
		case "Ice Shard Vein":
		case "Large Dark Shard Vein":
		case "Dark Shard Vein":
			text = "Titanium Pick";
			break;
		}
		if (InventoryUtils.GetPickTier(inventory_ctr.Instance.player_inventory[inventory_ctr.hand_index].item.item_name) >= InventoryUtils.GetPickTier(text))
		{
			actual_damage = (int)Random.Range(1f, 2.9f);
			return;
		}
		actual_damage = 0;
		if (GameController.Instance.player != null && attacker == GameController.Instance.player)
		{
			InventoryItem item = new InventoryItem(text);
			PopupControl.Instance.ShowMessage("<color=#787878>You need a </color><color=#ffd38f>" + text + "</color><color=#787878> to get that</color>", PopupControl.context.message, item);
			attacker.GetComponent<CreatureBrain>().ClearAllTargets();
			GameController.Instance.HideTargetCircle();
		}
	}

	public static Combatant.hit_col StringToHitCol(string str)
	{
		if (str == "green")
		{
			return Combatant.hit_col.color_green;
		}
		if (str == "purple")
		{
			return Combatant.hit_col.color_purple;
		}
		return Combatant.hit_col.color_red;
	}

	public static int GetHpMaxPlayer(int player_level, float health_combat_slider)
	{
		float num = player_level;
		return (int)((float)(int)(num + num) + 2f + Mathf.Min(num * 0.125f, 10f) * Mathf.Clamp01(health_combat_slider) + 0f);
	}

	public static int GetHpMaxPlayerInverse(int player_HP)
	{
		return (int)((float)player_HP * 0.5f);
	}

	public static int GetHpMaxMob(int mob_level)
	{
		return (int)((float)(mob_level - 3) * 2f) + 2;
	}

	public static int GetHpRegenPlayer(int player_hp_max, float hp_regen_slider)
	{
		int num = (int)(1f / (Mathf.Lerp(45f, 100f, hp_regen_slider) / 5.5f) * (float)player_hp_max);
		if (num == 0)
		{
			num = 1;
		}
		return num;
	}

	public static int GetHpRegenMob(int mob_max_hp)
	{
		int num = (int)((float)mob_max_hp * 0.09166667f);
		if (num == 0)
		{
			num = 1;
		}
		return num;
	}

	public float CalcCombatSlider(slider_type type)
	{
		int[] player_stats = GameController.Instance.player_stats;
		float num = (float)player_stats[1] + (float)player_stats[2] + (float)player_stats[3] + (float)player_stats[4] + (float)player_stats[6] + (float)player_stats[7];
		float result = 0f;
		if (num != 0f)
		{
			switch (type)
			{
			case slider_type.attack:
				result = (float)player_stats[3] / num;
				break;
			case slider_type.health:
				result = (float)player_stats[2] / num;
				break;
			case slider_type.accuracy:
				result = (float)player_stats[4] / num;
				break;
			case slider_type.dodge:
				result = (float)player_stats[7] / num;
				break;
			case slider_type.mana_recharge:
				result = (float)player_stats[6] / num;
				break;
			case slider_type.health_recharge:
				result = (float)player_stats[1] / num;
				break;
			}
		}
		return result;
	}

	public static void GenerateCombatOutput()
	{
	}
}
