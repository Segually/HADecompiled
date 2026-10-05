using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBrain : MonoBehaviour
{
	private CreatureBrainInterface default_brain;

	private CreatureBrainInterface perk_brain;

	public static float think_clock_speed = 0.125f;

	private IEnumerator think_coroutine;

	private float combat_spacing = 0.55f;

	public static int standard_attack_cooldown = 57;

	public int attack_cooldown;

	public int initial_stun;

	public bool no_more_initial_stun;

	private IEnumerator attack_coroutine;

	private GameObject cached_main_target;

	private List<GameObject> list_of_targets = new List<GameObject>();

	public CreatureBrainInterface custom_brain
	{
		get
		{
			if (perk_brain != null)
			{
				return perk_brain;
			}
			return default_brain;
		}
	}

	public GameObject main_target
	{
		get
		{
			if (cached_main_target == null)
			{
				if (list_of_targets.Count == 0)
				{
					return null;
				}
				bool flag = false;
				while (cached_main_target == null && list_of_targets.Count >= 1)
				{
					cached_main_target = list_of_targets[0];
					if (cached_main_target == null)
					{
						list_of_targets.RemoveAt(0);
						flag = true;
					}
				}
				if (flag)
				{
					GameServerSender.Instance.UpdateSyncedTargetIds(base.gameObject);
				}
				if (cached_main_target == null)
				{
					return null;
				}
			}
			return cached_main_target;
		}
	}

	public void SetDefaultBrain(CreatureBrainInterface default_brain)
	{
		this.default_brain = default_brain;
	}

	public void SetPerkBrain(CreatureBrainInterface perk_brain)
	{
	}

	public void RemovePerkBrain()
	{
	}

	public void RestartThinking()
	{
		if (think_coroutine != null)
		{
			StopCoroutine(think_coroutine);
		}
		think_coroutine = custom_brain.Think();
		StartCoroutine(think_coroutine);
	}

	public void ReEnable()
	{
	}

	private void FixedUpdate()
	{
		if (initial_stun > 0)
		{
			initial_stun--;
		}
		if (attack_cooldown > 0)
		{
			attack_cooldown--;
		}
		if (base.transform.position.y < GameController.lowest_player_y)
		{
			custom_brain.OnFallOffWorld();
		}
	}

	public void PursueTarget()
	{
		if (!(main_target != null))
		{
			return;
		}
		SharedCreature component = GetComponent<SharedCreature>();
		component.SpotterLookAt(main_target.transform.position);
		if (!component.isQuickTagging)
		{
			float spacing = combat_spacing + (GetComponent<Combatant>().scale + main_target.GetComponent<Combatant>().scale) * 0.5f;
			component.SetMoveTo(component.DestinationWithSpacing(base.transform.position, main_target.transform.position, spacing));
		}
		else
		{
			component.SetMoveTo(component.DestinationWithSpacing(base.transform.position, main_target.transform.position, 0.1f));
		}
	}

	public void AttackWhenPossible()
	{
		if (!(main_target == null) && CloseEnoughToStrike() && attack_cooldown == 0 && initial_stun == 0)
		{
			if (attack_coroutine != null)
			{
				StopCoroutine(attack_coroutine);
			}
			attack_coroutine = AttackCoroutine();
			StartCoroutine(attack_coroutine);
			attack_cooldown = (int)(GetComponent<SharedCreature>().perk_attack_speed_mod * (float)standard_attack_cooldown);
		}
	}

	private IEnumerator AttackCoroutine()
	{
		GetComponent<SharedCreature>().VisuallyAttack();
		GameServerSender.Instance.SendAttackAnimation(GetComponent<Combatant>().combat_name);
		yield return new WaitForSeconds(0.15f);
		if (CloseEnoughToStrike())
		{
			CombatControl.Instance.CalculateHitLocally(base.gameObject);
		}
	}

	public bool CloseEnoughToStrike()
	{
		if (main_target == null)
		{
			return false;
		}
		return Mathf.Abs(Vector3.Distance(new Vector3(base.transform.position.x, 0f, base.transform.position.z), new Vector3(main_target.transform.position.x, 0f, main_target.transform.position.z)) - (combat_spacing + (GetComponent<Combatant>().scale + main_target.GetComponent<Combatant>().scale) * 0.5f)) < 1f;
	}

	public void ClearAllTargets()
	{
		list_of_targets.Clear();
		GameServerSender.Instance.UpdateSyncedTargetIds(base.gameObject);
		cached_main_target = null;
	}

	public List<GameObject> GetTargetList()
	{
		return list_of_targets;
	}

	public void AddToFrontOfTargetList(GameObject target)
	{
		if (!(target == base.gameObject))
		{
			if (list_of_targets.Contains(target))
			{
				list_of_targets.Remove(target);
			}
			if (list_of_targets.Count == 0)
			{
				list_of_targets.Add(target);
			}
			else
			{
				list_of_targets.Insert(0, target);
			}
			GameServerSender.Instance.UpdateSyncedTargetIds(base.gameObject);
			cached_main_target = target;
			PursueTarget();
		}
	}

	public void RemoveFromTargetList(GameObject target)
	{
		bool flag = cached_main_target == target;
		if (list_of_targets.Contains(target))
		{
			list_of_targets.Remove(target);
		}
		GameServerSender.Instance.UpdateSyncedTargetIds(base.gameObject);
		if (flag)
		{
			cached_main_target = null;
		}
	}
}
