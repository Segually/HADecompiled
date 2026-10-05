using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdvertUtils : MonoBehaviour, OrderedStart
{
	public static AdvertUtils Instance;

	private int reward_seconds_remaining;

	private IEnumerator reward_ad_timer;

	public Text[] mystery_box_text_to_translate;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
		if (reward_ad_timer == null)
		{
			reward_ad_timer = RewardAdTimer();
			StartCoroutine(reward_ad_timer);
		}
	}

	public void ResetRewardSeconds()
	{
		reward_seconds_remaining = 255;
	}

	private IEnumerator RewardAdTimer()
	{
		int interval = 3;
		reward_seconds_remaining = 255;
		while (true)
		{
			yield return new WaitForSeconds(interval);
			if (!(SceneManager.GetActiveScene().name == "Game"))
			{
				continue;
			}
			if (BreedControl.Instance.state_t == BreedControl.state.none && (!PopupControl.Instance.popup_open || (uint)(PopupControl.Instance.curr_popup - 3) >= 2u))
			{
				reward_seconds_remaining = Mathf.Max(0, reward_seconds_remaining - interval);
			}
			if (DontShowRewardPopupRightNow() || reward_seconds_remaining >= 1)
			{
				continue;
			}
			yield return new WaitForSeconds(1.5f);
			if (!DontShowRewardPopupRightNow())
			{
				if (PlayerPrefs.GetInt("showed_rating_window") != 0 || !(RateAppControl.Instance != null) || !RateAppControl.Instance.TryOpenRatingWindow())
				{
					if (AdvertControl.Instance.AdsActive())
					{
						PopupControl.Instance.ShowRewardAskPopup(AdvertControl.reward_ad_type.on_popup);
					}
				}
				reward_seconds_remaining = 255;
			}
		}
	}

	public bool DontShowRewardPopupRightNow()
	{
		if (SceneManager.GetActiveScene().name == "Menu")
		{
			return true;
		}
		if (WindowControl.Instance.ImportantWindowsOpen())
		{
			return true;
		}
		if (!(GameController.Instance.player != null))
		{
			return true;
		}
		if (GameController.Instance.player.GetComponent<CreatureBrain>().main_target != null && GameController.Instance.player.GetComponent<CreatureBrain>().main_target.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature)
		{
			return true;
		}
		foreach (KeyValuePair<string, GameObject> active_combatant in MobControl.Instance.active_combatants)
		{
			GameObject value = active_combatant.Value;
			if (value != null && value != null && value.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature && value.GetComponent<SharedCreature>().IsTargettingPlayerOrMyCompanions(true, false))
			{
				return true;
			}
		}
		return false;
	}

	public void TranslateMysteryBoxText()
	{
		for (int i = 0; i < mystery_box_text_to_translate.Length; i++)
		{
			Text text = mystery_box_text_to_translate[i];
			text.text = TranslationControl.Instance.TranslateGeneral(text.text, "Market");
		}
	}

	public void InvertedTranslateMysteryBoxText()
	{
		for (int i = 0; i < mystery_box_text_to_translate.Length; i++)
		{
			Text text = mystery_box_text_to_translate[i];
			text.text = TranslationControl.Instance.TranslateGeneralBackwards(text.text, "Market");
		}
	}
}
