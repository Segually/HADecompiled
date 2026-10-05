using UnityEngine;
using UnityEngine.UI;

public class RewardsControl : MonoBehaviour, OrderedStart
{
	private enum reward_type
	{
		gems = 0,
		creature = 1,
		items = 2
	}

	public static RewardsControl Instance;

	public Text text_reward;

	private GameObject curr_reward_model;

	private BasketContents reward_items;

	private bool always_item;

	private reward_type prev_reward;

	public int reward_count;

	public string reward_creature_name;

	public string reward_item_name;

	public GameObject prefab_rewardmodel_items_x1;

	public GameObject prefab_rewardmodel_items_x2;

	public GameObject prefab_rewardmodel_items_x3;

	public GameObject prefab_rewardmodel_items_x4;

	public GameObject prefab_rewardmodel_gems;

	public GameObject reward_model_parent;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
			reward_count = 0;
		}
	}

	public void Start_1()
	{
	}

	public void PlayMysteryBoxAppear()
	{
	}

	public void ShowOkayButton()
	{
	}

	public void PressRewardOkay()
	{
	}

	public void ClickOpen()
	{
	}

	private reward_type TryRollGems()
	{
		return default(reward_type);
	}

	private reward_type TryRollCreature()
	{
		return default(reward_type);
	}

	private float CalcGemChances()
	{
		return 0f;
	}
}
