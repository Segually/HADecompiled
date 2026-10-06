using System;
using System.Collections;
using UnityEngine;

public class CreatureBrainChaoticMovement : MonoBehaviour, CreatureBrainInterface
{
	private int roam_to_new_position_timer;

	private int roam_to_new_position_timer_max = 5;

	private float roam_radius = 3f;

	private Vector3 start_position;

	public void CustomFixedUpdate()
	{
	}

	public void Init()
	{
		start_position = base.transform.position;
	}

	public void OnFallOffWorld()
	{
		bool flag = InventoryUtils.IsHeavenDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name);
		SharedCreature component = GetComponent<SharedCreature>();
		if (flag)
		{
			component.Deload(true);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			component.ResetHeight();
		}
	}

	public void ReactOnHit(GameObject hit_by)
	{
	}

	public IEnumerator Think()
	{
		while (true)
		{
			yield return new WaitForSeconds(CreatureBrain.think_clock_speed);
			if (!GameController.Instance.is_paused())
			{
				if (roam_to_new_position_timer == 0)
				{
					int num = UnityEngine.Random.Range(0, 360);
					GetComponent<SharedCreature>().SetMoveTo(start_position + new Vector3(Mathf.Sin((float)num * ((float)Math.PI / 180f)), 0f, Mathf.Cos((float)num * ((float)Math.PI / 180f))) * roam_radius);
					roam_to_new_position_timer = roam_to_new_position_timer_max;
				}
				roam_to_new_position_timer--;
			}
		}
	}

	public void TriggerOnReachDesiredMoveAt()
	{
	}
}
