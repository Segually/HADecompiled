using System.Collections;
using Google.Play.Review;
using UnityEngine;

public class RateAppControl : MonoBehaviour, OrderedStart
{
	private enum rate_style_t
	{
		variant_1 = 0,
		variant_2 = 1,
		variant_3 = 2
	}

	public static RateAppControl Instance;

	private ReviewManager _reviewManager;

	private int n_ads_without_rate;

	private bool already_pressed_rating;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	private rate_style_t GetRateStyle()
	{
		return default(rate_style_t);
	}

	private bool CheckDateTime(string buildTimeString)
	{
		return false;
	}

	public bool TryOpenRatingWindow()
	{
		return false;
	}

	public void ShowPopupRateVariant1()
	{
	}

	public void ShowPopupRateVariant2()
	{
	}

	public void PressRatingStar(int id)
	{
	}

	private IEnumerator DelayedCloseVariant2Screen()
	{
		return null;
	}

	public void ShowInAppReview()
	{
	}

	public void LaunchReviewScreenAndroid()
	{
	}

	private IEnumerator LaunchReviewScreenAndroidCoroutine()
	{
		return null;
	}
}
