using UnityEngine;

public class GiftNib : MonoBehaviour
{
	public int index;

	public void PressTake()
	{
		if (inventory_ctr.Instance.GetEmptyNonEquipmentInventorySlots(inventory_ctr.Instance.player_inventory).Count == 0)
		{
			PopupControl.Instance.ShowMessage("You don't have any space in your inventory!", PopupControl.context.message);
			return;
		}
		Trophy trophy = FriendServerReceiver.Instance.trophies[index];
		InventoryItem item = FriendServerInterface.Instance.GenerateTrophy(trophy);
		inventory_ctr.Instance.GiveItem(item, 1, "", true);
		FriendServerReceiver.Instance.trophies.Remove(trophy);
		FriendServerInterface.Instance.RedrawGiftsScreen();
		FriendServerSender.Instance.SendTakeTrophy(trophy.trophy_rand_id);
	}
}
