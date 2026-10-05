using System.Collections.Generic;
using UnityEngine;

public class ItemSprite : MonoBehaviour
{
	public enum draw_param
	{
		COLOR_req_not_met = 0,
		COLOR_unclickable = 1,
		COLOR_fusionGreen = 2,
		BG_inventory = 3,
		BG_container = 4,
		BG_alwaysVisible = 5,
		COUNT_hide = 6,
		COUNT_show0and1 = 7,
		COUNT_sellPrice = 8,
		COUNT_buyPrice = 9,
		COUNT_zeroPrice = 10,
		REQ_showCrafting = 11,
		REQ_showSkill = 12,
		REQ_hide = 13,
		PREM_ignore = 14,
		PREM_hideLockOnly = 15
	}

	private GameObject background_;

	private GameObject item_graphic_;

	private GameObject overlay_;

	private GameObject requirement_;

	private GameObject count_obj_;

	private GameObject locked_obj_;

	public GameObject model3d_generated_graphic_;

	private GameObject loading_circle_;

	private GameObject damage_indicator_obj;

	private GameObject shield_indicator_obj;

	private Texture2D custom_graphic_texture;

	private bool raycast;

	public void RedrawBasic(InventoryItem item, int count)
	{
	}

	public void RedrawBasicIgnorePremium(InventoryItem item, int count)
	{
	}

	public void RedrawBasicHideCount(InventoryItem item, int count)
	{
	}

	public void RedrawAsHoverIcon(InventoryItem item, int count)
	{
	}

	public void RedrawAsInventoryNormal(InventoryItem item, int count, int slot_id)
	{
	}

	public void RedrawAsInventorySellItems(InventoryItem item, int count, int slot_id)
	{
	}

	public void RedrawAsInventoryAndHighlightItem(InventoryItem item, int count, int slot_id, string item_highlight)
	{
	}

	public void RedrawAsContainer(InventoryItem item, int count, int slot_id)
	{
	}

	public void RedrawAsContainerGreen(InventoryItem item, int count, int slot_id)
	{
	}

	public void RedrawAsMinigameReward(InventoryItem item, int count)
	{
	}

	public void RedrawAsBuyPopupHeader(InventoryItem item, int count)
	{
	}

	public void RedrawAsSellPopupHeader(InventoryItem item, int count)
	{
	}

	public void RedrawAsBuyItem(InventoryItem item, int count)
	{
	}

	public void RedrawAsCrafting(InventoryItem item, int count)
	{
	}

	public void RedrawAsVendingMachineCost(InventoryItem item, int count)
	{
	}

	private void Redraw(InventoryItem item, int count, List<draw_param> parameters, int slot_id = -1)
	{
	}

	private void ProcessPreDraw()
	{
	}

	private void ProcessBackground(InventoryItem item, int count, List<draw_param> parameters, int slot_id)
	{
	}

	private void ProcessItemGraphic(InventoryItem item, int count, List<draw_param> parameters, int slot_id)
	{
	}

	private void ProcessModel3d(InventoryItem item, int count, List<draw_param> parameters)
	{
	}

	private void ProcessCount(InventoryItem item, int count, List<draw_param> parameters)
	{
	}

	private void ProcessDamageIndicator(InventoryItem item, int count, List<draw_param> parameters)
	{
	}

	private void ProcessShieldIndicator(InventoryItem item, int count, List<draw_param> parameters)
	{
	}

	private void ProcessRequirement(InventoryItem item, int count, List<draw_param> parameters)
	{
	}

	private void ProcessOverlay(InventoryItem item, int count, List<draw_param> parameters)
	{
	}

	private void ProcessPremiumLock(InventoryItem item, int count, List<draw_param> parameters)
	{
	}

	private void DestroyBackground()
	{
	}

	public void DestroyLoading()
	{
	}

	private void DestroyModel3D()
	{
	}

	private void DestroyItemGraphic()
	{
	}

	private void DestroyOverlay()
	{
	}

	private void DestroyRequirement()
	{
	}

	private void DestroyPremiumLock()
	{
	}

	private void DestroyCount()
	{
	}

	private void DestroyDamageIndicator()
	{
	}

	private void DestroyShieldIndicator()
	{
	}

	private void CreateLoading()
	{
	}

	private void CreateBackground(Sprite sprite, float scale = 1f)
	{
	}

	private void CreateItemGraphic(Sprite sprite)
	{
	}

	private void CreateCount(string count_str)
	{
	}

	private void CreateDamageIndicator(int damage_base)
	{
	}

	private void CreateShieldIndicator(float defense_base)
	{
	}

	private void CreateRequirement(string str)
	{
	}

	private void CreateOverlay(Sprite sprite, float scale)
	{
	}

	private void CreatePremiumLock()
	{
	}

	private void CreateModel3D()
	{
	}

	public void RedrawBasicHideCount(string item_name, int count)
	{
	}

	public void RedrawAsInventoryNormal(string item_name, int count, int slot_id)
	{
	}

	public static void RecursiveApplyLayer(Transform T, int layer, bool ignore_root)
	{
	}

	private Sprite LoadCustomGraphic(InventoryItem item)
	{
		return null;
	}

	private void OnDestroy()
	{
	}

	public void Model3DScreenshotComplete(Texture tex)
	{
	}
}
