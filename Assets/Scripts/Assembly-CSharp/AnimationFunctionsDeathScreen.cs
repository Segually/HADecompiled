using UnityEngine;

public class AnimationFunctionsDeathScreen : MonoBehaviour
{
	public GameObject buttons_standard;

	public GameObject buttons_battle_royale;

	public void DeathSoundEffect()
	{
		GameController.Instance.sound_death();
	}

	public void ShowMenuButton()
	{
		WindowPrefabsControl.Instance.CreateScreen("You Died - bottom left", WindowPrefabsControl.build_into_t.GAME_CTR);
	}

	public void ShowOptions()
	{
		buttons_standard.SetActive(true);
		buttons_battle_royale.SetActive(false);
		buttons_standard.GetComponent<Animation>().Play("you died buttons fade in");
	}
}
