using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExpGainParticle : MonoBehaviour
{
	private Vector3 position;

	public Text particle;

	private Camera mainCamera;

	public void Init(Vector3 position, Camera mainCamera, string str)
	{
		this.mainCamera = mainCamera;
		this.position = position;
		particle.text = str;
		StartCoroutine(delayed_destry());
	}

	private void FixedUpdate()
	{
		MobControl.Instance.SnapOverhead((RectTransform)base.transform, position);
	}

	private IEnumerator delayed_destry()
	{
		yield return new WaitForSeconds(2f);
		GameController.Instance.possible_destroy.Remove(base.gameObject);
		Object.Destroy(base.gameObject);
	}
}
