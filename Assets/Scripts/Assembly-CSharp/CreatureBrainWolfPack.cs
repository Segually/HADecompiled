using System;
using System.Collections.Generic;
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
		randomized_too_far_checker = UnityEngine.Random.Range(1.5f, 4f);
		SharedCreature component = GetComponent<SharedCreature>();
		component.SetMoveTo(GameController.Instance.player.transform.position + GetFollowOffset());
	}

	public Vector3 GetFollowOffset()
	{
		return GameController.Instance.player.transform.rotation * new Vector3(Mathf.Cos(follow_angle * ((float)Math.PI / 180f)), 0f, Mathf.Sin(follow_angle * ((float)Math.PI / 180f))) * follow_dist;
	}

	public void Swarm(GameObject enemy)
	{
		if (curr_state != state.initial_goto_player)
		{
			GetComponent<CreatureBrain>().AddToFrontOfTargetList(enemy);
			curr_state = state.attacking_target;
		}
	}

	public IEnumerator Think()
	{
		while (true)
		{
			yield return new WaitForSeconds(CreatureBrain.think_clock_speed);
			if (GameController.Instance.is_paused())
			{
				continue;
			}
			switch (curr_state)
			{
			case state.attacking_target:
			{
				CreatureBrain component = GetComponent<CreatureBrain>();
				if (component.main_target == null)
				{
					GetComponent<SharedCreature>().CancelMoveto();
					curr_state = state.follow_player;
				}
				else if (!component.main_target.GetComponent<PerkReceiver>().CheckPerkBoolApplied("Mobs Targetting Me Will Pause") && !GetComponent<PerkReceiver>().CheckPerkBoolApplied("Let My Target Come To Me"))
				{
					component.PursueTarget();
				}
				break;
			}
			case state.follow_player:
			{
				GameObject gameObject = null;
				float num = float.MaxValue;
				foreach (KeyValuePair<string, GameObject> active_combatant in MobControl.Instance.active_combatants)
				{
					GameObject value = active_combatant.Value;
					if (value == base.gameObject)
					{
						continue;
					}
					float ai_lockon_range = GetComponent<SharedCreature>().ai_lockon_range;
					Combatant component2 = value.GetComponent<Combatant>();
					if (!value.GetComponent<PerkReceiver>().CheckPerkBoolApplied("Invisible To Mobs If Not Already Targetted"))
					{
						bool flag = false;
						if (component2.mob_type == Combatant.TYPE_T.creature)
						{
							SharedCreature component3 = value.GetComponent<SharedCreature>();
							flag = component3.brain_type == SharedCreature.brain_type_t.aggressive || (extra_agression && (component3.brain_type == SharedCreature.brain_type_t.neutral || component3.brain_type == SharedCreature.brain_type_t.fearful));
						}
						float num2 = Vector3.Distance(value.transform.position, base.transform.position);
						if (flag && num2 < ai_lockon_range && num2 < num)
						{
							num = num2;
							gameObject = value;
						}
					}
				}
				if (gameObject != null)
				{
					GetComponent<CreatureBrain>().AddToFrontOfTargetList(gameObject);
					curr_state = state.attacking_target;
				}
				else if (GameController.Instance.player != null && Vector3.Distance(base.transform.position, GameController.Instance.player.transform.position + GetFollowOffset()) > randomized_too_far_checker)
				{
					GetComponent<SharedCreature>().SetMoveTo(GameController.Instance.player.transform.position + GetFollowOffset());
				}
				break;
			}
			case state.initial_goto_player:
				GetComponent<SharedCreature>().SetMoveTo(GameController.Instance.player.transform.position + GetFollowOffset());
				break;
			}
		}
	}

	public void TriggerOnReachDesiredMoveAt()
	{
		if (curr_state == state.initial_goto_player)
		{
			curr_state = state.follow_player;
		}
	}

	public void CustomFixedUpdate()
	{
		if (curr_state == state.attacking_target)
		{
			CreatureBrain component = GetComponent<CreatureBrain>();
			if (component.main_target != null && !component.main_target.GetComponent<PerkReceiver>().CheckPerkBoolApplied("Mobs Targetting Me Will Pause"))
			{
				component.AttackWhenPossible();
			}
		}
	}

	public void OnFallOffWorld()
	{
		GetComponent<SharedCreature>().ResetHeight();
	}

	public void ReactOnHit(GameObject hit_by)
	{
		if ((curr_state == state.follow_player || curr_state == state.attacking_target) && !(hit_by == null) && !(hit_by == GameController.Instance.player))
		{
			GetComponent<CreatureBrain>().AddToFrontOfTargetList(hit_by);
			curr_state = state.attacking_target;
		}
	}
}
