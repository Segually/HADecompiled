using System.Collections;
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

	public Vector3 startPos => default(Vector3);

	public void Init()
	{
		hp = start_HP;
		HP_max = (int)start_HP;
		start_HP = 0f;
		start_exp_receie = 0f;
		exp_recieve = (int)start_exp_receie;
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
	}

	private byte GetNetMobType(bool this_is_a_net_player)
	{
		return 0;
	}

	public void WasHit(int damage, GameObject attacker, bool missed, bool dodged, hit_col col_, bool send_hit, int fake_damage = -1)
	{
	}

	public void BeginDarkswordParticles(GameObject particle_prefab)
	{
	}

	public bool ShouldGiveExpAndDropsOnDie(GameObject killer)
	{
		return false;
	}

	public void Die(GameObject killa, float delay, float splat_delay, int respawn_secs)
	{
	}

	private IEnumerator DieCoroutine(GameObject killer, float delay, float splat_delay, int respawn_secs)
	{
		return null;
	}

	private ItemCountPair GetDropFromEnabledCategories(bool WILD_MOB_DROPS, bool WEAPON_AND_ARMOR, bool BOSS_KEY)
	{
		return null;
	}

	private ItemCountPair GetWildMobDrop()
	{
		return null;
	}

	private ItemCountPair GetWeaponAndArmorDrop()
	{
		return null;
	}

	private ItemCountPair GetBossKeyDrop()
	{
		return null;
	}

	private InventoryItem GetEggDrop()
	{
		return null;
	}

	private ItemCountPair GetCoinsDrop()
	{
		return null;
	}

	private void SpawnDrop(float angle, float d, ItemCountPair drop_pair)
	{
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
	}

	public void ReceiveHealthbar(GameObject healthbar, Camera mainCamera)
	{
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
		return null;
	}

	public void RefreshHealthbarDisappearTimer(float timer)
	{
	}

	private IEnumerator DisappearHealthbar(float timer)
	{
		return null;
	}

	public bool WithinHitbox(Vector3 position, bool clamp)
	{
		return false;
	}

	public bool IgnoreFireDamage()
	{
		return false;
	}

	public void RecursiveApplyMaterial(Transform T, Material mat)
	{
	}
}
