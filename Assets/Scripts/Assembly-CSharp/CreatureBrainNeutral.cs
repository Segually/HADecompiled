using System.Collections;
using UnityEngine;

public class CreatureBrainNeutral : MonoBehaviour, CreatureBrainInterface
{
	private enum state
	{
		roaming = 0,
		attacking_target = 1,
		override_return_to_spawn = 2
	}

	private state curr_state;

	private int roam_to_new_position_timer;

	private int roam_to_new_position_timer_max;

	private float dist_return_to_spawn;

	public void Init()
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
