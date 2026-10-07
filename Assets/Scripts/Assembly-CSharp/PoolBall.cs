using System.Collections;
using UnityEngine;

public class PoolBall : MonoBehaviour
{
	public GameObject model;

	public GameObject eye1;

	public GameObject eye2;

	public string eye_sprite;

	public Vector3 sunk_position;

	public PoolGameControl.team_t team;

	public PoolBallSimulated physics = new PoolBallSimulated();

	private IEnumerator blink_t;

	private void OnEnable()
	{
		if (blink_t != null)
		{
			StopCoroutine(blink_t);
			blink_t = null;
		}
		blink_t = BlinkCoroutine();
		StartCoroutine(blink_t);
	}

	private IEnumerator BlinkCoroutine()
	{
		if (eye1 == null || eye2 == null)
		{
			yield break;
		}
		ResourceControl.Instance.AssignCreatureDecorativeTexture("Eyes/" + eye_sprite, eye1.GetComponent<MeshRenderer>(), true);
		ResourceControl.Instance.AssignCreatureDecorativeTexture("Eyes/" + eye_sprite, eye2.GetComponent<MeshRenderer>(), true);
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(1f, 5f));
			int n_blinks = Random.Range(1, 3);
			for (int i = 0; i < n_blinks; i++)
			{
				eye1.SetActive(false);
				eye2.SetActive(false);
				yield return new WaitForSeconds(0.05f);
				eye1.SetActive(true);
				eye2.SetActive(true);
				yield return new WaitForSeconds(0.05f);
			}
		}
	}

	private void FixedUpdate()
	{
		if (physics.is_sunk)
		{
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, sunk_position, Time.fixedDeltaTime * 15f);
			if (Vector3.Distance(base.transform.localPosition, sunk_position) < 1f)
			{
				base.gameObject.SetActive(false);
			}
		}
	}
}
