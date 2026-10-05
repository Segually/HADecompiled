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
	}

	private void CreateItem(string item_name, float scale, Vector3 local_position, Vector3 rot)
	{
	}
}
