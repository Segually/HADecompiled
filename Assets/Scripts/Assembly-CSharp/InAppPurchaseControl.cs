using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using Unity.Services.Core;

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

	public Dictionary<string, string> price_infos = new Dictionary<string, string>();

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
		if (this == Instance)
		{
			DontDestroyOnLoad(gameObject);
			StartCoroutine(InitializeUnityGamingServicesThenIAP());
		}
	}

	private IEnumerator InitializeUnityGamingServicesThenIAP()
	{
		if (UnityServices.State != ServicesInitializationState.Initialized)
		{
			var task = UnityServices.InitializeAsync();
			while (!task.IsCompleted) yield return null;
			if (task.IsFaulted || task.IsCanceled) Debug.LogError("Failed to initialize Unity Gaming Services.\n" + task.Exception?.ToString());
			else Debug.Log("Unity Gaming Services successfully initialized.");
		}
		else Debug.Log("Unity Gaming Services is already initialized.");
		if (m_StoreController == null) InitializePurchasing();
	}

	public void InitializePurchasing()
	{
		if (IsInitialized()) return;
		ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		foreach (string name in ShopControl.Instance.purchase_structs_ordering)
		{
			string key = ShopControl.Instance.GetPurchaseableKey(name);
			string type = ShopControl.Instance.GetPurchaseableType(name);
			if (type == "subscription")
			{
				StoreSpecificIds ids = new StoreSpecificIds();
				ids.Add("no_ads_subscription", AppleAppStore.Name);
				ids.Add("no_ads_subscr", GooglePlay.Name);
				builder.AddProduct("no_ads_subscr", ProductType.Subscription, ids);
			}
			else if (type == "consumable" && key != "doCOMPANION" && key != "doMUTATE") builder.AddProduct(key, ProductType.Consumable);
			else if (type == "permanent") builder.AddProduct(key, ProductType.NonConsumable);
		}
		UnityPurchasing.Initialize(this, builder);
	}

	private bool IsInitialized()
	{
		return m_StoreController != null && m_StoreExtensionProvider != null;
	}

	public void SignalToGame()
	{
		if (initialize_state == intialize_state_t.succeed) ShopControl.Instance.ShopFinishedLoading();
		else if (initialize_state == intialize_state_t.failed) ShopControl.Instance.ShopFailedLoading();
	}

	public void BuyProductID(string productId)
	{
		if (IsInitialized())
		{
			Product product = m_StoreController.products.WithID(productId);
			if (product != null && product.availableToPurchase)
			{
				m_StoreController.InitiatePurchase(product);
				return;
			}
		}
		ShopControl.Instance.OnTransactionFailed();
	}

	public void RestoreSubscription()
	{
		if (!IsInitialized())
		{
			PopupControl.Instance.ShowMessage("ERROR - Extension provider not initialized", PopupControl.context.message);
			return;
		}
		PopupControl.Instance.ShowConnecting("Restoring Purchases", PopupControl.context.loading_NO_TIMEOUT);
		StartCoroutine(DelayedRestorePurchases());
	}

	private IEnumerator DelayedRestorePurchases()
	{
		yield return new WaitForSeconds(0.5f);
		TryRestorePurchases();
	}

	public void TryRestorePurchases()
	{
		m_StoreExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(delegate(bool success, string message)
		{
			if (success)
			{
				PopupControl.Instance.ShowMessage("Restoration complete!\n<color=#aaaaaa>(You might have to reboot the game)</color>", PopupControl.context.message);
				if (ShopControl.Instance != null) ShopControl.Instance.RedrawAll();
			}
			else PopupControl.Instance.ShowMessage("ERROR - Restoration could not complete", PopupControl.context.message);
		});
	}

	private void OnDeferred(Product item)
	{
		Debug.Log("Purchase deferred: " + item.definition.id);
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		initialize_state = intialize_state_t.succeed;
		m_StoreController = controller;
		m_StoreExtensionProvider = extensions;
		m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
		m_AppleExtensions.RegisterPurchaseDeferredListener(OnDeferred);
		Dictionary<string, string> introductory = m_AppleExtensions.GetIntroductoryPriceDictionary();
		bool subscribed = false;
		foreach (Product product in controller.products.all)
		{
			if (product.receipt != null)
			{
				string intro = introductory != null && introductory.ContainsKey(product.definition.storeSpecificId) ? introductory[product.definition.storeSpecificId] : null;
				if (product.definition.type == ProductType.Subscription)
				{
					SubscriptionInfo info = new SubscriptionManager(product, intro).getSubscriptionInfo();
					if (info.isSubscribed() == Result.True && info.isExpired() == Result.False) subscribed = true;
				}
			}
			if (price_infos.ContainsKey(product.definition.id)) price_infos[product.definition.id] = product.metadata.localizedPriceString;
			else price_infos.Add(product.definition.id, product.metadata.localizedPriceString);
		}
		if (!subscribed) AdvertControl.Instance.subscribed_to_remove_ads = false;
		if (GetCurrSceneName() == "Game") SignalToGame();
	}

	public bool IsPurchased(string IAP_key, string alt_IAP_key)
	{
		if (IAP_key == "no_ads_subscr") return !AdvertControl.Instance.AdsActive();
		bool alt_empty = Startup.StringNullOrWhitespace(alt_IAP_key);
		short purchased = PlayerData.Instance.GetGlobalShort(IAP_key);
		if (!alt_empty && purchased != 1) return PlayerData.Instance.GetGlobalShort(alt_IAP_key) == 1;
		return purchased == 1;
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		initialize_state = intialize_state_t.failed;
		if (GetCurrSceneName() == "Game") SignalToGame();
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
	{
		ShopControl.Instance.OnTransactionSucceed(args.purchasedProduct.definition.id);
		return PurchaseProcessingResult.Complete;
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		ShopControl.Instance.OnTransactionFailed();
	}

	private string GetCurrSceneName()
	{
		return SceneManager.GetActiveScene().name;
	}

	public void OnInitializeFailed(InitializationFailureReason error, string message)
	{
		throw new NotImplementedException();
	}
}
