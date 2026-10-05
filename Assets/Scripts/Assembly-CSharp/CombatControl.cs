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
		return false;
	}

	public void CalculateHitLocally(GameObject attacker)
	{
	}

	private void CalculateCreatureHit(GameObject attacker, GameObject defender, ref int actual_damage, ref int fake_damage, ref bool attacker_missed, ref bool defender_dodged, ref string apply_perk)
	{
	}

	private void ProcessAttackerWeapon(GameObject attacker, ref int actual_damage, ref string apply_perk)
	{
	}

	private void ProcessDefenderArmor(GameObject defender, ref int actual_damage)
	{
	}

	private void CalculateMiningHit(GameObject attacker, GameObject defender, ref int actual_damage, ref int fake_damage, ref bool missed, ref bool dodged, ref string apply_perk)
	{
	}

	public static Combatant.hit_col StringToHitCol(string str)
	{
		return default(Combatant.hit_col);
	}

	public static int GetHpMaxPlayer(int player_level, float health_combat_slider)
	{
		float num = player_level;
		return (int)((float)(int)(num + num) + 2f + Mathf.Min(num * 0.125f, 10f) * Mathf.Clamp01(health_combat_slider) + 0f);
	}

	public static int GetHpMaxPlayerInverse(int player_HP)
	{
		return 0;
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
