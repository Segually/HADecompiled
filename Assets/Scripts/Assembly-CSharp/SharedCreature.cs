using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedCreature : MonoBehaviour
{
	public enum brain_type_t
	{
		none = 0,
		local_player = 1,
		net_player = 2,
		aggressive = 3,
		neutral = 4,
		fearful = 5,
		companion = 6,
		guard = 7,
		auto_set = 8,
		wolf_pack = 9,
		ghost = 10
	}

	public static float H = 0.3f;

	private FakeTransform spotter = new FakeTransform();

	public LiteModel myCreatureModel;

	public bool attack_anm_playing;

	public GameObject MP_display;

	public GameObject levelDisplay;

	public GameObject teleport_particle;

	public GameObject movement_smoother;

	public bool isDying;

	public bool isMoving;

	public bool isQuickTagging;

	public List<string> quickTagEffects = new List<string>();

	public PerkData quickTagPerkData;

	public int quickTagPerkLevel;

	public bool snapped_to_chair_obj;

	public float walk_speed;

	public InventoryItem hat_ = new InventoryItem("");

	public InventoryItem body_ = new InventoryItem("");

	public InventoryItem hand_ = new InventoryItem("");

	public string base_skin_material = "";

	public int level;

	public float default_size = 1f;

	public int hp_regen;

	public float ai_lockon_range;

	public float wander_dist;

	public bool is_local_mob;

	public List<string> synced_target_ids;

	private brain_type_t cached_brain_type;

	public bool deleted;

	private bool reached_desired_location;

	private int last_footstep_sound;

	private Vector3 prev_step_sound_pos;

	private bool model_down_in_water;

	private string prev_applied_skin = "";

	public float perk_attack_speed_mod = 1f;

	public float perk_speed_mod = -1f;

	private Vector3 cached_move_to;

	public string creature_name;

	public Color creature_type_col;

	public int icon_id;

	private IEnumerator regen_health_t;

	public brain_type_t brain_type
	{
		get
		{
			return cached_brain_type;
		}
		set
		{
			cached_brain_type = value;
		}
	}

	public void Init()
	{
		walk_speed = 0f;
		level = 0;
		hp_regen = 0;
	}

	public void ReplaceCreatureModel(List<string> new_parents)
	{
		LiteModel liteModel = myCreatureModel;
		GameObject hybridLite = CreatureMorpher.Instance.GetHybridLite(new_parents);
		hybridLite.GetComponent<LiteModel>().animation_choppiness = GraphicsControl.Instance.SpecialAnimationChoppiness();
		hybridLite.transform.SetParent(liteModel.transform.parent);
		hybridLite.transform.localPosition = Vector3.zero;
		hybridLite.transform.localRotation = liteModel.transform.localRotation;
		hybridLite.transform.localScale = liteModel.transform.localScale;
		myCreatureModel = hybridLite.GetComponent<LiteModel>();
		Object.Destroy(liteModel.gameObject);
	}

	public void ReCalcHpMaxAndHpRegen()
	{
		int num = level;
		int hp_regen;
		if (base.gameObject == GameController.Instance.player)
		{
			int hpMaxPlayer = CombatControl.GetHpMaxPlayer(num, CombatControl.Instance.CalcCombatSlider(CombatControl.slider_type.health));
			GetComponent<Combatant>().HP_max = hpMaxPlayer;
			hp_regen = CombatControl.GetHpRegenPlayer(hpMaxPlayer, CombatControl.Instance.CalcCombatSlider(CombatControl.slider_type.health_recharge));
		}
		else
		{
			int hpMaxMob = CombatControl.GetHpMaxMob(num);
			GetComponent<Combatant>().HP_max = hpMaxMob;
			hp_regen = CombatControl.GetHpRegenMob(hpMaxMob);
		}
		GetComponent<SharedCreature>().hp_regen = hp_regen;
	}

	public void Delete()
	{
	}

	public void EndSittingInChair()
	{
	}

	public Vector3 DestinationWithSpacing(Vector3 curr_pos, Vector3 destination, float spacing)
	{
		return destination + (curr_pos - destination).normalized * spacing;
	}

	private void OnDestroy()
	{
		if (deleted)
		{
			return;
		}
		deleted = true;
		if (MP_display != null)
		{
			Object.Destroy(MP_display);
		}
		if (levelDisplay != null)
		{
			GameController.Instance.possible_destroy.Remove(levelDisplay);
			Object.Destroy(levelDisplay);
		}
		if (teleport_particle != null)
		{
			Object.Destroy(teleport_particle.gameObject);
		}
		if (is_local_mob)
		{
			string combat_name = GetComponent<Combatant>().combat_name;
			if (MobControl.Instance.my_claimed_creatures.Contains(combat_name))
			{
				MobControl.Instance.my_claimed_creatures.Remove(combat_name);
			}
		}
	}

	public Quaternion GetSpotterRotation()
	{
		return spotter.rotation;
	}

	public void Deload(bool send_to_server)
	{
		string combat_name = GetComponent<Combatant>().combat_name;
		if (MobControl.Instance.my_claimed_creatures.Contains(combat_name))
		{
			MobControl.Instance.my_claimed_creatures.Remove(combat_name);
			if (send_to_server)
			{
				GameServerSender.Instance.SendDeloadMob(combat_name);
			}
		}
	}

	public void ResetHeight()
	{
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		base.transform.position = new Vector3(base.transform.position.x, 1.5f, base.transform.position.z);
		base.gameObject.layer = 0;
	}

	public void ShowLevelupParticles(bool show_text_too, float delay)
	{
		StartCoroutine(ShowLevelupParticlesCoroutine(show_text_too, delay));
	}

	private IEnumerator ShowLevelupParticlesCoroutine(bool show_overhead, float delay)
	{
		yield return new WaitForSeconds(delay);
		Object.Instantiate(GameController.Instance.type_friendly_levelup_particles).transform.position = base.transform.position;
		if (show_overhead)
		{
			GameController.Instance.showOverheadNotif("LEVEL UP", base.transform.position, true, false);
		}
	}

	public void RedrawEquipment()
	{
		string currSkin = GetCurrSkin();
		Material mat = null;
		if (!Startup.StringNullOrEmpty(currSkin))
		{
			mat = MobControl.Instance.GetSkinMaterialByName(currSkin);
		}
		myCreatureModel.ApplyHat((hat_.item_name != "" && inventory_ctr.Instance.GetItemType(hat_) == inventory_ctr.inv_type_t.helmet) ? hat_ : new InventoryItem(""), mat);
		myCreatureModel.ApplyArmor((body_.item_name != "" && inventory_ctr.Instance.GetItemType(body_) == inventory_ctr.inv_type_t.armor) ? body_ : new InventoryItem(""), mat);
		myCreatureModel.ApplyWeapon((hand_.item_name != "" && inventory_ctr.Instance.GetItemType(hand_) == inventory_ctr.inv_type_t.holdable) ? hand_ : new InventoryItem(""), mat);
	}

	public void OnEquipmentChanged()
	{
		if (base.gameObject == GameController.Instance.player)
		{
			inventory_ctr.Instance.UpdateManaMods(hat_.item_name);
		}
		UpdateSize();
		UpdateSkinMaterial();
		RedrawEquipment();
	}

	public void VisuallyAttack()
	{
		if (!attack_anm_playing)
		{
			attack_anm_playing = true;
			if (myCreatureModel != null)
			{
				myCreatureModel.StartAnimation(3);
			}
		}
	}

	public void CreateMultiplayerDisplay(string username_punctuated, int level)
	{
	}

	public void RedrawMultiplayerOverhead(string username_punctuated, int level)
	{
	}

	private string GetLevelColor(int my_level, int their_level)
	{
		return null;
	}

	public void TrySitInChairObj(string chair_interactable_id)
	{
	}

	private void FixedUpdate()
	{
		if (MP_display != null)
		{
			Vector3 vector = ((!(ChunkControl.Instance.follow_obj != null)) ? GameController.Instance.prev_player_pos : ChunkControl.Instance.follow_obj.transform.position);
			Vector3 vector2 = base.transform.position;
			if (Vector3.Distance(base.transform.position, vector) >= 10f)
			{
				vector2 = vector + (base.transform.position - vector).normalized * 10f;
			}
			Vector3 vector3 = Camera.main.WorldToViewportPoint(vector2 + Vector3.up * myCreatureModel.original.height);
			float x = Mathf.Clamp(vector3.x, 0.05f, 0.95f);
			float y = Mathf.Clamp(vector3.y, 0.05f - GameServerInterface.Instance.overhead_y_offset, 0.95f - GameServerInterface.Instance.overhead_y_offset);
			((RectTransform)MP_display.transform).anchorMin = new Vector2(x, y);
			((RectTransform)MP_display.transform).anchorMax = new Vector2(x, y);
		}
		if (levelDisplay != null)
		{
			MobControl.Instance.SnapOverhead((RectTransform)levelDisplay.transform, base.transform.position);
		}
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, spotter.rotation, Time.deltaTime * 8f);
		if (!isDying)
		{
			if (is_local_mob && !GameController.Instance.is_paused())
			{
				if (AtDestination())
				{
					if (!reached_desired_location)
					{
						reached_desired_location = true;
						GetComponent<CreatureBrain>().custom_brain.TriggerOnReachDesiredMoveAt();
					}
				}
				else if (reached_desired_location)
				{
					reached_desired_location = false;
				}
				GetComponent<CreatureBrain>().custom_brain.CustomFixedUpdate();
			}
			if (!snapped_to_chair_obj && (!GameController.Instance.is_paused() || AtDestination()))
			{
				ShiftCreaturePosition();
			}
		}
		if (snapped_to_chair_obj)
		{
			return;
		}
		if (attack_anm_playing)
		{
			if (!myCreatureModel.animation_complete)
			{
				return;
			}
			attack_anm_playing = false;
		}
		else if (!AtDestination())
		{
			if (!isMoving)
			{
				if (myCreatureModel != null)
				{
					myCreatureModel.StartAnimation(1);
				}
				isMoving = true;
			}
			return;
		}
		else if (!isMoving)
		{
			return;
		}
		if (myCreatureModel != null)
		{
			myCreatureModel.StartAnimation(0);
		}
		isMoving = false;
	}

	public bool AtDestination()
	{
		return Vector3.Distance(new Vector3(base.transform.position.x, 0f, base.transform.position.z), new Vector3(cached_move_to.x, 0f, cached_move_to.z)) < 0.75f;
	}

	private void TryStepSound()
	{
	}

	public bool IsTargettingPlayerOrMyCompanions(bool check_player, bool check_my_companions)
	{
		if (is_local_mob)
		{
			foreach (GameObject target in GetComponent<CreatureBrain>().GetTargetList())
			{
				if (check_player && GameController.Instance.player != null && target == GameController.Instance.player)
				{
					return true;
				}
				if (check_my_companions && target.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature)
				{
					SharedCreature component = target.GetComponent<SharedCreature>();
					if (component.is_local_mob && component.brain_type == brain_type_t.companion)
					{
						return true;
					}
				}
			}
			return false;
		}
		string globalString = PlayerData.Instance.GetGlobalString("username_lower");
		foreach (string synced_target_id in synced_target_ids)
		{
			if (check_player && GameController.Instance.player != null && synced_target_id == globalString)
			{
				return true;
			}
			if (check_my_companions && MobControl.Instance.active_combatants.ContainsKey(synced_target_id))
			{
				GameObject gameObject = MobControl.Instance.active_combatants[synced_target_id];
				if (gameObject.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature)
				{
					SharedCreature component2 = gameObject.GetComponent<SharedCreature>();
					if (component2.is_local_mob && component2.brain_type == brain_type_t.companion)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsBig()
	{
		return base.transform.localScale.x > 1.05f;
	}

	public void UpdateSize()
	{
		float num = 0f;
		foreach (DurationEffect duration_effect in GetComponent<PerkReceiver>().duration_effects)
		{
			if (duration_effect.time_remaining > 0f)
			{
				float num2 = duration_effect.GetFloat("Player Size", "%");
				if (num2 > num && num2 != -1f)
				{
					num = num2;
				}
			}
		}
		if (num == 0f)
		{
			float equipmentScale = InventoryUtils.GetEquipmentScale(hat_.item_name, body_.item_name);
			if (equipmentScale == 1f)
			{
				base.transform.localScale = Vector3.one * default_size;
				GetComponent<Combatant>().scale = default_size;
			}
			else
			{
				base.transform.localScale = equipmentScale * Vector3.one;
				GetComponent<Combatant>().scale = equipmentScale;
			}
		}
		else
		{
			base.gameObject.transform.localScale = Vector3.one * num * 0.01f;
			base.gameObject.GetComponent<Combatant>().scale = num * 0.01f;
		}
		if (base.gameObject == GameController.Instance.player)
		{
			float creature_scale_view_mod = 1f;
			if (num != 0f)
			{
				float x = base.transform.localScale.x;
				float num3 = ((x < 1f) ? 0.8f : 0.16f);
				creature_scale_view_mod = num3 * (x - 1f + 1f / num3);
			}
			GameController.Instance.creature_scale_view_mod = creature_scale_view_mod;
			if (GraphicsControl.Instance.GraphicsLevel() == 4)
			{
				ChunkControl.Instance.UpdateTerrain(false);
			}
		}
	}

	public string GetCurrSkin()
	{
		string text = "";
		foreach (DurationEffect duration_effect in GetComponent<PerkReceiver>().duration_effects)
		{
			if (duration_effect.time_remaining > 0f)
			{
				string @string = duration_effect.GetString("Change skin mat");
				if (@string != "")
				{
					text = @string;
				}
			}
		}
		if (text != "")
		{
			return text;
		}
		string equipmentSkinMat = InventoryUtils.GetEquipmentSkinMat(hat_.item_name, body_.item_name);
		if (equipmentSkinMat != "")
		{
			return equipmentSkinMat;
		}
		return base_skin_material;
	}

	public void UpdateSkinMaterial()
	{
		string currSkin = GetCurrSkin();
		if (currSkin != prev_applied_skin)
		{
			if (currSkin != "")
			{
				myCreatureModel.ApplySpecialMaterial(MobControl.Instance.GetSkinMaterialByName(currSkin));
			}
			else
			{
				ReplaceCreatureModel(GetComponent<SharedCreature>().myCreatureModel.GetComponent<LiteModel>().original.creatures_that_made_me);
			}
			prev_applied_skin = currSkin;
		}
	}

	public void ReCalcAttackSpeed()
	{
		float num = -1f;
		foreach (DurationEffect duration_effect in GetComponent<PerkReceiver>().duration_effects)
		{
			if (duration_effect.time_remaining > 0f)
			{
				float @float = duration_effect.GetFloat("Attack Speed", "%");
				if (@float != -1f)
				{
					if (@float == 0f)
					{
						num = 0f;
					}
					else if (num < @float)
					{
						num = @float;
					}
				}
			}
		}
		perk_attack_speed_mod = ((num == -1f) ? 1f : ((num != 0f) ? (1f / num * 100f) : 100f));
		if (is_local_mob)
		{
			GetComponent<CreatureBrain>().attack_cooldown = 0;
		}
	}

	public void ReCalcWalkSpeedMod()
	{
		float num = -1f;
		foreach (DurationEffect duration_effect in GetComponent<PerkReceiver>().duration_effects)
		{
			if (duration_effect.time_remaining > 0f)
			{
				float @float = duration_effect.GetFloat("Walk Speed", "%");
				if (@float != -1f)
				{
					if (@float == 0f)
					{
						num = 0f;
					}
					else if (num < @float)
					{
						num = @float;
					}
				}
			}
		}
		perk_speed_mod = ((num != -1f) ? (num * 0.01f) : (-1f));
	}

	public void SetMoveTo(Vector3 position)
	{
		reached_desired_location = false;
		cached_move_to = position;
		SpotterLookAt(position);
	}

	public void CancelMoveto()
	{
		if (!AtDestination())
		{
			Vector3 normalized = (cached_move_to - base.transform.position).normalized;
			SetMoveTo(base.transform.position + normalized * 0.75f);
		}
	}

	public Vector3 GetMoveTo()
	{
		return cached_move_to;
	}

	public void ShiftCreaturePosition()
	{
		float num = walk_speed;
		if (hat_.item_name == "Speed Helm")
		{
			num = 0.1f;
		}
		else if (isQuickTagging)
		{
			num = 0.15f;
		}
		else if (perk_speed_mod != -1f)
		{
			num *= perk_speed_mod;
		}
		if (Vector3.Distance(prev_step_sound_pos, base.transform.position) > 0.9f)
		{
			TryStepSound();
			prev_step_sound_pos = base.transform.position;
		}
		SmoothShiftPosition(base.gameObject, cached_move_to, num);
		if (movement_smoother != null)
		{
			SmoothShiftPosition(movement_smoother, cached_move_to, num);
		}
	}

	public static void SmoothShiftPosition(GameObject to_shift, Vector3 move_to, float final_speed)
	{
		float num = Vector3.Distance(new Vector3(move_to.x, 0f, move_to.z), new Vector3(to_shift.transform.position.x, 0f, to_shift.transform.position.z));
		Vector3 normalized = (move_to - to_shift.transform.position).normalized;
		float num2 = 1f / (1f + Mathf.Pow(2.7182817f, 0f - (num * 8f - 4f)));
		if (num > 0.05f)
		{
			to_shift.transform.position += new Vector3(normalized.x, 0f, normalized.z) * (num2 * final_speed);
		}
	}

	private bool ShouldPlayStepSfx()
	{
		return false;
	}

	public void SpotterLookAt(Vector3 look_at)
	{
		spotter.position = base.transform.position;
		Vector3 vector = new Vector3(look_at.x, base.transform.position.y, look_at.z);
		if (!(Vector3.Distance(vector, spotter.position) < 0.2f))
		{
			spotter.rotation = Quaternion.LookRotation((vector - spotter.position).normalized);
		}
	}

	public void SnapSpotterRotation(Quaternion Q)
	{
		spotter.rotation = Q;
	}

	public void RedrawLevelDisplay()
	{
		if (WindowControl.Instance.ShouldRecreateOverheads())
		{
			levelDisplay = Object.Instantiate(GameController.Instance.type_creatureLevelDisplay);
			levelDisplay.transform.SetParent(MobControl.Instance.gameObject.transform);
			levelDisplay.transform.SetAsFirstSibling();
			levelDisplay.transform.localPosition = Vector3.zero;
			levelDisplay.transform.localScale = Vector3.one * 0.75f;
			levelDisplay.transform.localRotation = Quaternion.identity;
			Transform transform = levelDisplay.transform;
			Vector2 vector = Vector2.one * 10f;
			((RectTransform)levelDisplay.transform).anchorMax = vector;
			((RectTransform)transform).anchorMin = vector;
			GameController.Instance.possible_destroy.Add(levelDisplay);
			RedrawLevelText();
			RedrawCreatureText();
			RedrawIcon(icon_id);
		}
	}

	public void AssignOverheadName(string creature_name, Color creature_name_col)
	{
		this.creature_name = creature_name;
		creature_type_col = creature_name_col;
	}

	public void RedrawIcon(int draw_icon)
	{
		if (!(levelDisplay != null))
		{
			return;
		}
		Sprite sprite = null;
		if (draw_icon >= 0 && draw_icon < DevBuildControl.Instance.overhead_logos.Length)
		{
			sprite = DevBuildControl.Instance.overhead_logos[draw_icon];
		}
		levelDisplay.transform.Find("StateDisplay").GetComponent<UnityEngine.UI.Image>().sprite = sprite;
	}

	public void RedrawCreatureText()
	{
		if (levelDisplay != null)
		{
			UnityEngine.UI.Text component = levelDisplay.transform.Find("creature-type").GetComponent<UnityEngine.UI.Text>();
			component.text = creature_name;
			component.color = creature_type_col;
		}
	}

	public void RedrawLevelText()
	{
		if (!(levelDisplay == null))
		{
			levelDisplay.transform.Find("level").GetComponent<UnityEngine.UI.Text>().text = "LVL " + level;
		}
	}

	public void ReEnable()
	{
	}

	public void StartRegenHealth()
	{
		if (regen_health_t != null)
		{
			StopCoroutine(regen_health_t);
			regen_health_t = null;
		}
		regen_health_t = RegenHealth();
		StartCoroutine(regen_health_t);
	}

	private IEnumerator RegenHealth()
	{
		while (true)
		{
			float health_before = GetComponent<Combatant>().hp;
			yield return new WaitForSeconds(5.5f);
			float hp = GetComponent<Combatant>().hp;
			if (hp == health_before && hp < (float)GetComponent<Combatant>().HP_max && !GetComponent<Combatant>().is_dead)
			{
				GetComponent<Combatant>().IncreaseHp(GetComponent<SharedCreature>().hp_regen);
			}
		}
	}
}
