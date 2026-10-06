using System.Collections;
using System.Collections.Generic;
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
		GetComponent<CreatureBrain>().AddToFrontOfTargetList(closest_object);
		curr_state = state.attacking_target;
	}

	public void Swarm(GameObject enemy)
	{
		GetComponent<CreatureBrain>().AddToFrontOfTargetList(enemy);
		curr_state = state.attacking_target;
	}

	public void ManuallySelectOverwriteWalkTo(Vector3 position)
	{
		GetComponent<CreatureBrain>().ClearAllTargets();
		curr_state = state.overwrite_walkto;
		GetComponent<SharedCreature>().SetMoveTo(position);
	}

	public void ForgetTargetsAndFollowPlayer()
	{
		GetComponent<CreatureBrain>().ClearAllTargets();
		GetComponent<SharedCreature>().SetMoveTo(transform.position);
		curr_state = state.follow_player;
	}

	public IEnumerator Think()
	{
		while (true)
		{
			yield return new WaitForSeconds(CreatureBrain.think_clock_speed);
			if (GameController.Instance.is_paused()) continue;
			if (curr_state == state.attacking_target)
			{
				CreatureBrain brain = GetComponent<CreatureBrain>();
				if (brain.main_target == null)
				{
					GetComponent<SharedCreature>().CancelMoveto();
					curr_state = state.follow_player;
				}
				else if (!brain.main_target.GetComponent<PerkReceiver>().CheckPerkBoolApplied("Mobs Targetting Me Will Pause") && !GetComponent<PerkReceiver>().CheckPerkBoolApplied("Let My Target Come To Me"))
					brain.PursueTarget();
			}
			else if (curr_state == state.follow_player)
			{
				GameObject closest = null;
				float best = float.MaxValue;
				foreach (KeyValuePair<string, GameObject> combatant in MobControl.Instance.active_combatants)
				{
					GameObject obj = combatant.Value;
					if (obj == gameObject) continue;
					float range = GetComponent<SharedCreature>().ai_lockon_range;
					Combatant target = obj.GetComponent<Combatant>();
					if (obj.GetComponent<PerkReceiver>().CheckPerkBoolApplied("Invisible To Mobs If Not Already Targetted")) continue;
					bool valid = false;
					if (target.mob_type == Combatant.TYPE_T.stationary)
					{
						valid = companion_struct != null && companion_struct.attack_xp_orbs;
						if (valid) range = 2f;
					}
					else if (target.mob_type == Combatant.TYPE_T.creature)
						valid = obj.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.aggressive;
					float distance = Vector3.Distance(obj.transform.position, transform.position);
					if (distance < best & valid & distance < range)
					{
						best = distance;
						closest = obj;
					}
				}
				if (closest != null)
				{
					GetComponent<CreatureBrain>().AddToFrontOfTargetList(closest);
					curr_state = state.attacking_target;
				}
				else if (GameController.Instance.player != null && companion_struct != null)
				{
					int index = companion_struct.hatch_index * 2 + 1;
					if (index + 1 < CompanionController.Instance.trail_nodes__.Count)
					{
						GameObject node = CompanionController.Instance.trail_nodes__[index];
						if (node != last_visited_trail_node)
						{
							last_visited_trail_node = node;
							GetComponent<SharedCreature>().SetMoveTo(last_visited_trail_node.transform.position);
						}
					}
				}
			}
		}
	}

	public void TriggerOnReachDesiredMoveAt()
	{
		if (curr_state == state.overwrite_walkto) curr_state = state.attacking_target;
	}

	public void CustomFixedUpdate()
	{
		if (curr_state == state.attacking_target)
		{
			CreatureBrain brain = GetComponent<CreatureBrain>();
			if (brain.main_target != null && !brain.main_target.GetComponent<PerkReceiver>().CheckPerkBoolApplied("Mobs Targetting Me Will Pause")) brain.AttackWhenPossible();
		}
	}

	public void OnFallOffWorld()
	{
		GetComponent<SharedCreature>().ResetHeight();
	}

	public void ReactOnHit(GameObject hit_by)
	{
		if ((uint)curr_state < 2 && hit_by != null && hit_by != GameController.Instance.player)
		{
			GetComponent<CreatureBrain>().AddToFrontOfTargetList(hit_by);
			curr_state = state.attacking_target;
		}
	}
}
