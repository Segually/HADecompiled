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
	}

	public void TriggerEffectOnTarget2()
	{
	}

	private void TriggerEffectOnTargetGeneric(string str, bool range_check)
	{
	}

	public void TriggerEffectOnSelf()
	{
	}

	public void TriggerEffectOnSelf2()
	{
	}

	private void TriggerEffectOnSelfGeneric(string str)
	{
	}

	public void AnimationComplete()
	{
	}
}
