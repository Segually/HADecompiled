using UnityEngine;

public class PerkAnimation : MonoBehaviour
{
	public GameObject currently_animating;

	public PerkData perk_data;

	public int perk_level;

	public Vector3 original_localPosition;

	public Quaternion original_localRotation;

	public Vector3 original_localScale;

	public string EffectOnTarget;

	public bool EffectOnTargetRangeCheck;

	public string EffectOnTarget2;

	public bool EffectOnTarget2RangeCheck;

	public string EffectOnSelf;

	public string EffectOnSelf2;

	public void TriggerEffectOnTarget()
	{
		TriggerEffectOnTargetGeneric(EffectOnTarget, EffectOnTargetRangeCheck);
	}

	public void TriggerEffectOnTarget2()
	{
		TriggerEffectOnTargetGeneric(EffectOnTarget2, EffectOnTarget2RangeCheck);
	}

	private void TriggerEffectOnTargetGeneric(string str, bool range_check)
	{
		if (str == "" || currently_animating == null)
		{
			return;
		}
		string combat_name = currently_animating.GetComponent<Combatant>().combat_name;
		int caster_level = ((currently_animating.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature) ? currently_animating.GetComponent<SharedCreature>().level : 1);
		if (currently_animating.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature && currently_animating.GetComponent<SharedCreature>().is_local_mob && currently_animating.GetComponent<CreatureBrain>().main_target != null)
		{
			string effect_name = str.Replace("[", "").Replace("]", "");
			if (!range_check || currently_animating.GetComponent<CreatureBrain>().CloseEnoughToStrike())
			{
				currently_animating.GetComponent<CreatureBrain>().main_target.GetComponent<PerkReceiver>().ApplyPerkEffect(false, effect_name, perk_data, perk_level, combat_name, caster_level, true);
			}
		}
	}

	public void TriggerEffectOnSelf()
	{
		TriggerEffectOnSelfGeneric(EffectOnSelf);
	}

	public void TriggerEffectOnSelf2()
	{
		TriggerEffectOnSelfGeneric(EffectOnSelf2);
	}

	private void TriggerEffectOnSelfGeneric(string str)
	{
		if (str == "" || currently_animating == null)
		{
			return;
		}
		string combat_name = currently_animating.GetComponent<Combatant>().combat_name;
		int caster_level = ((currently_animating.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature) ? currently_animating.GetComponent<SharedCreature>().level : 1);
		if (currently_animating.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature && currently_animating.GetComponent<SharedCreature>().is_local_mob)
		{
			string effect_name = str.Replace("[", "").Replace("]", "");
			currently_animating.GetComponent<PerkReceiver>().ApplyPerkEffect(false, effect_name, perk_data, perk_level, combat_name, caster_level, true);
		}
	}

	public void AnimationComplete()
	{
		if (currently_animating == null)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			currently_animating.GetComponent<PerkReceiver>().PerkAnimationComplete();
		}
	}
}
