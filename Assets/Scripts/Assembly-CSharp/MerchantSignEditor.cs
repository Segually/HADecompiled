using UnityEngine;
using UnityEngine.UI;

public class MerchantSignEditor : MonoBehaviour
{
	public static MerchantSignEditor Instance;

	public InputField input_name;

	public InputField input_motto;

	public ItemSprite item_spr;

	public void AcceptMerchantSign()
	{
		string text = input_name.text;
		string text2 = input_motto.text;
		if (Startup.StringNullOrEmpty(text) && Startup.StringNullOrEmpty(text2))
		{
			PopupControl.Instance.ShowMessage("Enter a shop name or motto!");
			return;
		}
		ExtraInventoryData extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
		extraDataCopy.SetString("sign_header", text);
		extraDataCopy.SetString("sign_text", text2);
		ConstructionControl.Instance.PlayerReplaceInteracting(new InventoryItem(GameController.Instance.interacting_element_item.item_name, extraDataCopy), true);
		PopupControl.Instance.SetButtonWasPressed();
		WindowControl.Instance.CloseMiniwindow(true);
	}
}
