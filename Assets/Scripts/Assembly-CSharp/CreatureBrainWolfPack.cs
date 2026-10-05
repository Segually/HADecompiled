using System.Collections;
using UnityEngine;

public class CreatureBrainWolfPack : MonoBehaviour, CreatureBrainInterface
{
	private enum state
	{
		initial_goto_player = 0,
		follow_player = 1,
		attacking_target = 2
	}

	private state curr_state;

	public bool extra_agression;

	public float follow_angle;

	public float follow_dist;

	private float randomized_too_far_checker;

	public void Init()
	{
	}

	public Vector3 GetFollowOffset()
	{
		return default(Vector3);
	}

	public void Swarm(GameObject enemy)
	{
	}

	public IEnumerator Think()
	{
		return null;
	}

	public void TriggerOnReachDesiredMoveAt()
	{
	}

	public void CustomFixedUpdate()
	{
	}

	public void OnFallOffWorld()
	{
	}

	public void ReactOnHit(GameObject hit_by)
	{
	}
}
