using System.Collections;
using UnityEngine;

public class CreatureBrainFearful : MonoBehaviour, CreatureBrainInterface
{
	private enum state
	{
		roaming = 0,
		fleeing_from_chaser = 1,
		low_HP_retaliate = 2
	}

	private state curr_state;

	private GameObject chasing_me;

	private int roam_to_new_position_timer;

	private int roam_to_new_position_timer_max;

	private int run_away_deke_timer;

	private int run_away_deke_timer_max;

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
