using UnityEngine;
using UnityEngine.Advertisements;

public class AdvertControl : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener, OrderedStart
{
	private enum launch_progress_t
	{
		launching = 0,
		failed = 1,
		launch_succeed = 2
	}

	private enum interstitial_state_t
	{
		none = 0,
		loading = 1,
		ready_to_show = 2,
		failed_try_again = 3
	}

	public enum ad_context
	{
		after_few_levelups = 0,
		on_breeder = 1,
		FORCED = 2,
		reward = 3
	}

	public enum reward_ad_type
	{
		none = 0,
		on_levelup = 1,
		on_popup = 2,
		on_free_gems_button = 3
	}

	public static AdvertControl Instance;

	private string session_interstitial;

	private bool showingRewardAdUsingInterstitial;

	private launch_progress_t launch_progress;

	private interstitial_state_t interstitial_state;

	private bool view_interstital_immediately_when_loaded;

	public bool subscribed_to_remove_ads;

	public int ADS_num_rebreeds;

	public reward_ad_type reward_ad_type_t;

	private bool on_initial_startup;

	public bool DONT_DISCONNECT;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
			subscribed_to_remove_ads = true;
		}
	}

	public void Start_1()
	{
		if (this == Instance)
		{
			_ = Application.platform;
			session_interstitial = "video";
			LaunchAds();
		}
	}

	private void AuthorizationTrackingReceived(int status)
	{
	}

	private void LaunchAds()
	{
		Advertisement.Initialize((Application.platform == RuntimePlatform.IPhonePlayer) ? "1085703" : "1085704", false, this);
	}

	public void OnInitializationComplete()
	{
	}

	public void OnInitializationFailed(UnityAdsInitializationError error, string message)
	{
	}

	public void OnUnityAdsAdLoaded(string placementId)
	{
	}

	public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
	{
	}

	public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
	{
	}

	public void OnUnityAdsShowStart(string placementId)
	{
	}

	public void OnUnityAdsShowClick(string placementId)
	{
	}

	public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
	{
	}

	private void LoadInterstitialAd()
	{
	}

	public void TryShowInterstitialAd(ad_context context)
	{
		if (context == ad_context.reward)
		{
			showingRewardAdUsingInterstitial = true;
			if (!AdsActive() && reward_ad_type_t != reward_ad_type.on_free_gems_button)
			{
				return;
			}
		}
		else
		{
			showingRewardAdUsingInterstitial = false;
			if (!AdsActive())
			{
				OnInterstitialComplete(UnityAdsShowCompletionState.COMPLETED);
				return;
			}
			switch (context)
			{
			case ad_context.after_few_levelups:
				PopupControl.Instance.ShowRewardAskPopup(reward_ad_type.on_levelup);
				AdvertUtils.Instance.ResetRewardSeconds();
				return;
			case ad_context.on_breeder:
				ADS_num_rebreeds++;
				if (ADS_num_rebreeds >= 2)
				{
					ShowInterstitial();
					ADS_num_rebreeds = 0;
				}
				return;
			default:
				return;
			case ad_context.FORCED:
				break;
			}
		}
		ShowInterstitial();
	}

	private void ShowInterstitial()
	{
	}

	private void OnInterstitialFailedToLoad()
	{
	}

	private void OnInterstitialComplete(UnityAdsShowCompletionState showCompletionState)
	{
		if (!showingRewardAdUsingInterstitial)
		{
			if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Game")
			{
				if (PoolGameControl.Instance != null)
				{
					PoolGameControl.Instance.AdFinished();
				}
				else if (KaraokeControl.Instance != null)
				{
					KaraokeControl.Instance.AdFinished();
				}
			}
			PopupControl.Instance.HideAll();
		}
		else
		{
			switch (showCompletionState)
			{
			case UnityAdsShowCompletionState.SKIPPED:
				PopupControl.Instance.HideAll();
				PopupControl.Instance.ShowMessage(TranslationControl.Instance.TranslateGeneral("You must finish the whole video to receive the reward!", "Market"), PopupControl.context.reward_ad_skipped);
				break;
			case UnityAdsShowCompletionState.COMPLETED:
				PopupControl.Instance.HideAll();
				PopupControl.Instance.ShowRewardCompletePopup();
				break;
			default:
				PopupControl.Instance.HideAll();
				PopupControl.Instance.ShowMessage("Oops! The ad could not be loaded.", PopupControl.context.message);
				break;
			}
			showingRewardAdUsingInterstitial = false;
		}
		LoadInterstitialAd();
	}

	private void OnInterstitialReady()
	{
	}

	public void TryShowRewardAd()
	{
	}

	public bool AdsActive()
	{
		return false;
	}

	private void OnApplicationPause(bool pause)
	{
	}
}
