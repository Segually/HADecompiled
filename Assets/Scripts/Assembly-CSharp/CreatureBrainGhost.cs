using System.Collections;
using UnityEngine;

public class CreatureBrainGhost : MonoBehaviour, CreatureBrainInterface
{
	private enum state
	{
		stand_still = 0
	}

	private state curr_state;

	private float dist_return_to_spawn;

	public static string CorruptString(string input)
	{
		return null;
	}

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
