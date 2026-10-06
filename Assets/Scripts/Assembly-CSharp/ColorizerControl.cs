using UnityEngine;

public class ColorizerControl : MonoBehaviour, OrderedStart
{
	public static ColorizerControl Instance;

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

	public void ColorizeCurrentShackInterior()
	{
		PaintableObject component = ZoneDataControl.Instance.curr_interior.GetComponent<PaintableObject>();
		if (component != null)
		{
			string final_paint = inventory_ctr.Instance.GetPaintFromItemOrUseDefault(ZoneDataControl.Instance.curr_zonedata.house_item);
			BanditCampsControl.Instance.ModifyIfBanditPaint(ref final_paint, ZoneDataControl.Instance.curr_zonedata.house_item.item_name, ZoneDataControl.Instance.curr_zonedata.house_item.GetString("bandit_camp_instance"));
			string stampFromItem = inventory_ctr.Instance.GetStampFromItem(ZoneDataControl.Instance.curr_zonedata.house_item);
			string layoutItemFromItem = inventory_ctr.Instance.GetLayoutItemFromItem(ZoneDataControl.Instance.curr_zonedata.house_item.item_name);
			component.Colorize(final_paint, stampFromItem, layoutItemFromItem, ZoneDataControl.Instance.curr_zonedata.house_item.item_name);
		}
	}
}
