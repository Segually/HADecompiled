using System.Collections;
using UnityEngine;

public class CreatureBrainGuard : MonoBehaviour, CreatureBrainInterface
{
	private enum state
	{
		return_to_post = 0,
		attacking_target = 1
	}

	private state curr_state;

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
