using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkReceiver : MonoBehaviour
{
	public GameObject curr_perk_animation;

	public List<DurationEffect> duration_effects = new List<DurationEffect>();

	public Combatant my_combatant;

	public bool delete_once_all_effects_gone;

	public void InitializeDurationTimer(int duration, string effect_name, PerkData perk_Data, int perk_level, string caster_id, int caster_level, bool pre_applied)
	{
	}

	private void RecalcAdditiveEffects(string effect_name, PerkData perk_data, int perk_level, int caster_level)
	{
	}

	public void ApplyPerkEffect(bool on_duration_reapply, string effect_name, PerkData perk_data, int perk_level, string caster_id, int caster_level, bool send)
	{
	}

	public void OnDurationEffectRemoved(DurationEffect duration_effect)
	{
	}

	private void OnEnable()
	{
	}

	private IEnumerator ProcessDurationEffects()
	{
		return null;
	}

	public bool HasPerkRemaining(string perk_key)
	{
		return false;
	}

	private bool AlreadyHasMinions(DurationEffect new_timer)
	{
		return false;
	}

	public void RemovePerk(string perk_key)
	{
	}

	public void ClearAllDurationEffects()
	{
	}

	public bool CheckPerkBoolApplied(string key, DurationEffect skip = null)
	{
		foreach (DurationEffect duration_effect in duration_effects)
		{
			if ((skip == null || duration_effect != skip) && duration_effect.GetBool(key))
			{
				return true;
			}
		}
		return false;
	}

	private void ApplyInstantParticle(string particle_instant, PerkData perk_data, string effect_name, int perk_level, int caster_level)
	{
	}

	private void ApplyPerkAnimation(string animation_prefab, PerkData perk_data, int perk_level)
	{
	}

	private void ApplyDurationParticle(string particle_duration, DurationEffect new_timer, PerkData perk_Data, string effect_name, bool pre_applied)
	{
	}

	private void ApplyScreenOverlay(string particle_screen_overlay, DurationEffect new_timer)
	{
	}

	public void PerkAnimationComplete()
	{
	}

	private void SummonMinions(DurationEffect new_timer, int n_summon, PerkData perk_Data, string effect_name, int perk_level, string caster_id, int caster_level)
	{
	}

	private void ProcessStealHPAndSend(int steal_hp, PerkData perk_data, string effect_name, int perk_level, string caster_id)
	{
	}

	private void ProcessPerkDamageAndSend(int damage, PerkData perk_data, string effect_name, int perk_level, string caster_id)
	{
	}

	private void ProcessHealAndSend(int heal)
	{
	}

	private void ProcessBlastBack(float blastback, string caster_id, int caster_level)
	{
	}

	private void ApplySkinMaterial(string skin_mat)
	{
	}

	private void ApplyCustomBrain(DurationEffect new_timer, string brain_str)
	{
	}

	public bool CheckPerkStringApplied(string key, DurationEffect skip = null)
	{
		return false;
	}
}
