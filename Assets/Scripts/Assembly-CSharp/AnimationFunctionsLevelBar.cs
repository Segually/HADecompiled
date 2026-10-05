using UnityEngine;

public class AnimationFunctionsLevelBar : MonoBehaviour
{
	public void SetLevelupText()
	{
		GameController.Instance.animation_set_levelup_text();
	}

	public void AnimationComplete()
	{
		GameController.Instance.PAUSE_GAME();
		GameController.Instance.animation_sound_levelScreenAppear();
		GameController.Instance.animation_unlock_levelup_text();
		WindowControl.Instance.GetComponent<Animation>().Play("levelup appear");
	}
}
