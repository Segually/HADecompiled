using UnityEngine;

public class ShopModelGenerator : MonoBehaviour
{
	public string creatureA;

	public string creatureB;

	public float scale0 = 1f;

	public float scale1 = 1f;

	public float h0;

	public float h1;

	private GameObject model_obj;

	private void OnEnable()
	{
		if (!(model_obj == null))
		{
			return;
		}
		if (creatureA != "" && creatureB == "")
		{
			model_obj = CreatureMorpher.Instance.GetHybridLite(creatureA);
			model_obj.transform.SetParent(base.transform);
			model_obj.transform.localPosition = Vector3.zero + Vector3.up * h0 * scale0;
			model_obj.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
			model_obj.transform.localScale = scale0 * Vector3.one;
		}
		else if (creatureA != "" && creatureB != "")
		{
			model_obj = new GameObject("model");
			model_obj.transform.SetParent(base.transform);
			model_obj.transform.localPosition = Vector3.zero;
			model_obj.transform.localScale = Vector3.one;
			model_obj.transform.localRotation = Quaternion.identity;
			GameObject hybridLite = CreatureMorpher.Instance.GetHybridLite(creatureA);
			hybridLite.transform.SetParent(model_obj.transform);
			hybridLite.transform.localPosition = Vector3.zero + Vector3.up * h0 * scale0;
			hybridLite.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
			hybridLite.transform.localPosition = hybridLite.transform.localPosition + hybridLite.transform.rotation * Vector3.forward * 1.3f;
			hybridLite.transform.localScale = scale0 * Vector3.one;
			GameObject hybridLite2 = CreatureMorpher.Instance.GetHybridLite(creatureB);
			hybridLite2.transform.SetParent(model_obj.transform);
			hybridLite2.transform.localPosition = Vector3.zero + Vector3.up * h1 * scale1;
			hybridLite2.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			hybridLite2.transform.localPosition = hybridLite2.transform.localPosition + hybridLite2.transform.rotation * Vector3.left * 1.3f;
			hybridLite2.transform.localScale = scale1 * Vector3.one;
		}
		ItemSprite.RecursiveApplyLayer(model_obj.transform, LayerMask.NameToLayer("GUI-lighting"), false);
	}
}
