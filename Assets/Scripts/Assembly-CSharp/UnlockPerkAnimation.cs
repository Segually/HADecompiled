using UnityEngine;

public class UnlockPerkAnimation : MonoBehaviour
{
	public void AnimationExpanded()
	{
		PerkScreen.Instance.PerkUnlockAnimationExpanded();
	}

	public void AnimationComplete()
	{
		PerkScreen.Instance.PerkUnlockAnimationComplete();
	}
}
