using System;
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
		System.Random random = new System.Random();
		System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
		int[] array = new int[18]
		{
			768, 769, 770, 771, 772, 774, 775, 776, 778, 779,
			780, 807, 808, 817, 818, 822, 834, 837
		};
		for (int i = 0; i < input.Length; i++)
		{
			stringBuilder.Append(input[i]);
			stringBuilder.Append(Convert.ToChar(array[random.Next(array.Length)]));
		}
		return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
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
}
