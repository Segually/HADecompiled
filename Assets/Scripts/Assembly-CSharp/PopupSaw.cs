using UnityEngine;

public class PopupSaw : MonoBehaviour
{
	public enum action_type_t
	{
		saw = 0,
		blast = 1
	}

	public GameObject saw;

	private int last_second_processed;

	private bool saw_up;

	public string animation_saw_up;

	public string animation_saw_down;

	public bool reverse_timing;

	public action_type_t action_type;

	public float hit_check_dist;

	private int hit_checker;

	private void Descend()
	{
	}

	private void Ascend()
	{
	}

	private void FixedUpdate()
	{
	}

	private void DamageCombatant(float hurt_percentage, Combatant combatant)
	{
	}
}
