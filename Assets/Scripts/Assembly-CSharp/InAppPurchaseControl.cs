using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class InAppPurchaseControl : MonoBehaviour, IStoreListener, OrderedStart
{
	private enum intialize_state_t
	{
		unknown = 0,
		succeed = 1,
		failed = 2
	}

	public static InAppPurchaseControl Instance;

	private static IStoreController m_StoreController;

	private static IExtensionProvider m_StoreExtensionProvider;

	private intialize_state_t initialize_state;

	private IAppleExtensions m_AppleExtensions;

	public Dictionary<string, string> price_infos;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
	}

	private IEnumerator InitializeUnityGamingServicesThenIAP()
	{
		return null;
	}

	public void InitializePurchasing()
	{
	}

	private bool IsInitialized()
	{
		return false;
	}

	public void SignalToGame()
	{
	}

	public void BuyProductID(string productId)
	{
	}

	public void RestoreSubscription()
	{
	}

	private IEnumerator DelayedRestorePurchases()
	{
		return null;
	}

	public void TryRestorePurchases()
	{
	}

	private void OnDeferred(Product item)
	{
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
	}

	public bool IsPurchased(string IAP_key, string alt_IAP_key)
	{
		return false;
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
	{
		return default(PurchaseProcessingResult);
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
	}

	private string GetCurrSceneName()
	{
		return null;
	}

	public void OnInitializeFailed(InitializationFailureReason error, string message)
	{
	}
}
