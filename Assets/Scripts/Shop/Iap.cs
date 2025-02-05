using System;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

[Serializable]
public class ProductData
{
	public string id;
	public ProductType type;
}
public class Iap : MonoBehaviour, IDetailedStoreListener
{
	private IStoreController storeController;
	private IExtensionProvider extensionProvider;
	public bool FakeStore = true;
	public List<ProductData> products;
	public UnityEvent<string> OnPurchase;
	public bool init=false;
	public bool hasReceipt(string id)
	{
		return storeController.products.WithID(id).hasReceipt;
	}
	private async void Awake()
	{
		StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStore ? FakeStoreUIMode.Default : FakeStoreUIMode.StandardUser;
		StandardPurchasingModule.Instance().useFakeStoreAlways = FakeStore;
		await UnityServices.InitializeAsync();
		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.GooglePlay));
		foreach (var item in products)
		{
			builder.AddProduct(item.id, item.type);
		}
		UnityPurchasing.Initialize(this, builder);

	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		storeController = controller;
		extensionProvider = extensions;
		FakeRestoreProducts();
		init = true;
		print("IAP init");
	}	

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
	{
		print("purchased " + purchaseEvent.purchasedProduct.definition.id);
		OnPurchase?.Invoke(purchaseEvent.purchasedProduct.definition.id);
		
		FakeSaveProducts();
		return PurchaseProcessingResult.Complete;
	}

	public void Purchase(string id)
	{
		storeController.InitiatePurchase(id);
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		Debug.LogError("IAP init error " + error);
	}

	public void OnInitializeFailed(InitializationFailureReason error, string message)
	{
		Debug.LogError($"IAP init error {error} : {message}");
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
	{
		Debug.LogError($"IAP purchase failed {product} : {failureDescription}");
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		Debug.LogError($"IAP purchase failed {product} : {failureReason}");
	}

	private void FakeRestoreProducts()
	{
		if (!string.IsNullOrEmpty(PlayerPrefs.GetString("fakeStore")))
		{
			var receipts = PlayerPrefs.GetString("fakeStore").Split(",");
			foreach (var item in receipts)
			{
				storeController.InitiatePurchase(item);
			}
			print("load "+receipts);
		}
		
	}

	private void FakeSaveProducts()
	{
		List<string> receipts = new List<string>();
		foreach (var item in storeController.products.all)
		{
			if (item.hasReceipt)
			{
				receipts.Add(item.definition.id);
			}
		}
		PlayerPrefs.SetString("fakeStore", string.Join(",", receipts));
		PlayerPrefs.Save();
		print("save "+ string.Join(",", receipts));
	}
	
}
