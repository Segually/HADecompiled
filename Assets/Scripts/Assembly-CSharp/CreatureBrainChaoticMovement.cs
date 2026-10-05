using System.Collections;
using UnityEngine;

public class CreatureBrainChaoticMovement : MonoBehaviour, CreatureBrainInterface
{
	private int roam_to_new_position_timer;

	private int roam_to_new_position_timer_max;

	private float roam_radius;

	private Vector3 start_position;

	public void CustomFixedUpdate()
	{
	}

	public void Init()
	{
	}

	public void OnFallOffWorld()
	{
	}

	public void ReactOnHit(GameObject hit_by)
	{
	}

	public IEnumerator Think()
	{
		return null;
	}

	public void TriggerOnReachDesiredMoveAt()
	{
	}
}
