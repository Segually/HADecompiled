using System.Collections;
using UnityEngine;

public class CreatureBrainCompanion : MonoBehaviour, CreatureBrainInterface
{
	private enum state
	{
		follow_player = 0,
		attacking_target = 1,
		overwrite_walkto = 2
	}

	private state curr_state;

	public ActiveCompanion companion_struct;

	private GameObject last_visited_trail_node;

	public void Init()
	{
	}

	public void ManuallySelectedTarget(GameObject closest_object)
	{
	}

	public void Swarm(GameObject enemy)
	{
	}

	public void ManuallySelectOverwriteWalkTo(Vector3 position)
	{
	}

	public void ForgetTargetsAndFollowPlayer()
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
