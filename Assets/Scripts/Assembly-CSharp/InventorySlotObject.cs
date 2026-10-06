using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotObject : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	public int index;

	public Animation animated_segment;

	public ItemSprite item_sprite;

	public void OnPointerDown(PointerEventData eventData)
	{
		inventory_ctr.Instance.slot_mouse_down(index, base.gameObject);
	}
}
