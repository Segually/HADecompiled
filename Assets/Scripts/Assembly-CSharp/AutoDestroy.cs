using System.Collections;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
	public float delay;

	private void Start()
	{
		StartCoroutine(DelayedDestroy());
	}

	private IEnumerator DelayedDestroy()
	{
		yield return new WaitForSeconds(delay);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
