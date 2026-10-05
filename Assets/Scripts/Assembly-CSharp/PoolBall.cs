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

	public PoolBallSimulated physics;

	private IEnumerator blink_t;

	private void OnEnable()
	{
	}

	private IEnumerator BlinkCoroutine()
	{
		return null;
	}

	private void FixedUpdate()
	{
	}
}
