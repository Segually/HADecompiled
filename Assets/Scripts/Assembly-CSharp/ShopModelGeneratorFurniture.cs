using UnityEngine;

public class ShopModelGeneratorFurniture : MonoBehaviour
{
	public string item_A;

	public string item_B;

	public string item_C;

	public float item_A_scale;

	public Vector3 item_A_pos;

	public Vector3 item_A_rot;

	public float item_B_scale;

	public Vector3 item_B_pos;

	public Vector3 item_B_rot;

	public float item_C_scale;

	public Vector3 item_C_pos;

	public Vector3 item_C_rot;

	private GameObject model_obj;

	public void OnEnable()
	{
		if (!(model_obj == null))
		{
			return;
		}
		model_obj = new GameObject("model");
		model_obj.transform.SetParent(base.transform);
		model_obj.transform.localPosition = Vector3.zero;
		model_obj.transform.localScale = Vector3.one;
		model_obj.transform.localRotation = Quaternion.identity;
		if (!Startup.StringNullOrWhitespace(item_A))
		{
			CreateItem(item_A, item_A_scale, item_A_pos, item_A_rot);
		}
		if (!Startup.StringNullOrWhitespace(item_B))
		{
			CreateItem(item_B, item_B_scale, item_B_pos, item_B_rot);
		}
		if (!Startup.StringNullOrWhitespace(item_C))
		{
			CreateItem(item_C, item_C_scale, item_C_pos, item_C_rot);
		}
	}

	private void CreateItem(string item_name, float scale, Vector3 local_position, Vector3 rot)
	{
		InventoryItem item = InventoryItem.SetDisplayDefaults(new InventoryItem(item_name));
		GameObject check_not_null = model_obj;
		ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab(item, null, delegate(GameObject instance)
		{
			if (check_not_null == null)
			{
				UnityEngine.Object.Destroy(instance);
				return;
			}
			instance.transform.SetParent(model_obj.transform);
			instance.transform.localPosition = local_position;
			instance.transform.localRotation = Quaternion.Euler(rot);
			instance.transform.localScale = scale * Vector3.one;
			if (instance.GetComponent<PaintableObject>() != null)
			{
				instance.GetComponent<PaintableObject>().Colorize(inventory_ctr.Instance.GetPaintFromItemOrUseDefault(item), inventory_ctr.Instance.GetStampFromItem(item), inventory_ctr.Instance.GetLayoutItemFromItem(item_name), item_name);
			}
			ConstructionControl.Instance.DeleteUnnecessaryComponents(item, instance);
			ConstructionControl.Instance.AdjustBuildableInstance(instance, item, ConstructionControl.usage_context_t.on_mouseObj_or_storeModel, delegate
			{
				if (this != null)
				{
					ItemSprite.RecursiveApplyLayer(base.transform, base.gameObject.layer, true);
				}
			});
		});
	}
}
