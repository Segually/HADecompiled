using System.Collections;
using UnityEngine;

public class FastStart : MonoBehaviour
{
	private void Awake()
	{
		Application.targetFrameRate = 60;
	}

	private void Start()
	{
		StartCoroutine(DelayedStart());
	}

	private IEnumerator DelayedStart()
	{
		yield return new WaitForSeconds(0.5f);
		GetComponent<Startup>().Init();
	}
}
