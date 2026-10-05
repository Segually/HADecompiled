using System.Collections;
using UnityEngine;

public class CreatureBrainGhost : MonoBehaviour, CreatureBrainInterface
{
	private enum state
	{
		stand_still = 0
	}

	private state curr_state;

	private float dist_return_to_spawn = 23f;

	public static string CorruptString(string input)
	{
		return null;
	}

	public void Init()
	{
	}

	public IEnumerator Think()
	{
		while (true)
		{
			if (!GameController.Instance.is_paused())
			{
				_ = curr_state;
			}
			yield return new WaitForSeconds(CreatureBrain.think_clock_speed);
		}
	}

	public void TriggerOnReachDesiredMoveAt()
	{
	}

	public void CustomFixedUpdate()
	{
	}

	public void OnFallOffWorld()
	{
		bool flag = InventoryUtils.IsHeavenDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name);
		SharedCreature component = GetComponent<SharedCreature>();
		if (flag)
		{
			component.Deload(true);
			Object.Destroy(base.gameObject);
		}
		else
		{
			component.ResetHeight();
		}
	}

	public void ReactOnHit(GameObject hit_by)
	{
	}
}
