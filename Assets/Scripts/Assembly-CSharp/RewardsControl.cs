using System;
using System.Collections.Generic;
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
		AudioControl.Instance.Play(AudioControl.Instance.sfx_chest_whoosh);
		GetComponent<Animation>().Play("reward-appear");
	}

	public void ShowOkayButton()
	{
		PopupControl.Instance.okay_button.SetActive(true);
	}

	public void PressRewardOkay()
	{
		if (prev_reward != reward_type.items)
		{
			if (prev_reward == reward_type.gems)
			{
				ShopControl.Instance.BlobbleGemText();
			}
			return;
		}
		foreach (int item in reward_items.FilledSlots())
		{
			if (GameController.Instance.player != null && inventory_ctr.Instance.CanReceiveItem(reward_items[item].item, reward_items[item].count, true))
			{
				inventory_ctr.Instance.GiveItem(reward_items[item].item, reward_items[item].count, "", false);
				continue;
			}
			int num = UnityEngine.Random.Range(0, 360);
			MobControl.Instance.TrySpawnDrop(reward_items[item], GameController.Instance.prev_player_pos + new Vector3(Mathf.Sin((float)num * ((float)Math.PI / 180f)), 0f, Mathf.Cos((float)num * ((float)Math.PI / 180f))) * 0.4f);
		}
	}

	public void ClickOpen()
	{
		AudioControl.Instance.Play(AudioControl.Instance.sfx_unlock_chest);
		GetComponent<Animation>().Stop();
		GetComponent<Animation>().Play("reward-open");
		if (curr_reward_model != null)
		{
			UnityEngine.Object.Destroy(curr_reward_model);
		}
		Vector3 localScale = Vector3.one;
		Quaternion localRotation = Quaternion.identity;
		reward_type reward_type = TryRollGems();
		switch (reward_type)
		{
		case reward_type.gems:
			text_reward.text = "<color=#ffffff>You recieved: </color>" + reward_count + " Gems!";
			PlayerData.Instance.SetGlobalShort("GEMS", reward_count + PlayerData.Instance.GetGlobalShort("GEMS"));
			curr_reward_model = UnityEngine.Object.Instantiate(prefab_rewardmodel_gems);
			break;
		case reward_type.creature:
			text_reward.text = "<color=#ffffff>You recieved creature: </color>" + BreedControl.FirstCharUpper(reward_creature_name) + "!";
			curr_reward_model = CreatureMorpher.Instance.GetHybridLite(reward_creature_name);
			curr_reward_model.AddComponent<Spin>();
			curr_reward_model.GetComponent<Spin>().scale = 27f;
			localScale = Vector3.one * 56f;
			localRotation = Quaternion.Euler(0f, 130f, 0f);
			break;
		case reward_type.items:
			text_reward.text = "<color=#ffffff>You recieved: </color>Items!";
			reward_items = LootControl.Instance.GenerateLootChest(new InventoryItem("Titanium Chest"));
			switch (reward_items.FilledSlots().Count)
			{
			case 1:
				curr_reward_model = UnityEngine.Object.Instantiate(prefab_rewardmodel_items_x1);
				curr_reward_model.transform.Find("item 1").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(reward_items[0].item, reward_items[0].count);
				break;
			case 2:
				curr_reward_model = UnityEngine.Object.Instantiate(prefab_rewardmodel_items_x2);
				curr_reward_model.transform.Find("item 1").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(reward_items[0].item, reward_items[0].count);
				curr_reward_model.transform.Find("item 2").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(reward_items[1].item, reward_items[1].count);
				break;
			case 3:
				curr_reward_model = UnityEngine.Object.Instantiate(prefab_rewardmodel_items_x3);
				curr_reward_model.transform.Find("item 1").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(reward_items[0].item, reward_items[0].count);
				curr_reward_model.transform.Find("item 2").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(reward_items[1].item, reward_items[1].count);
				curr_reward_model.transform.Find("item 3").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(reward_items[2].item, reward_items[2].count);
				break;
			case 4:
				curr_reward_model = UnityEngine.Object.Instantiate(prefab_rewardmodel_items_x4);
				curr_reward_model.transform.Find("item 1").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(reward_items[0].item, reward_items[0].count);
				curr_reward_model.transform.Find("item 2").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(reward_items[1].item, reward_items[1].count);
				curr_reward_model.transform.Find("item 3").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(reward_items[2].item, reward_items[2].count);
				curr_reward_model.transform.Find("item 4").Find("item sprite").GetComponent<ItemSprite>().RedrawBasic(reward_items[5].item, reward_items[5].count);
				break;
			default:
				curr_reward_model = new GameObject("");
				break;
			}
			break;
		}
		prev_reward = reward_type;
		curr_reward_model.transform.SetParent(reward_model_parent.transform);
		curr_reward_model.transform.localScale = localScale;
		curr_reward_model.transform.localPosition = Vector3.zero;
		curr_reward_model.transform.localRotation = localRotation;
		ItemSprite.RecursiveApplyLayer(curr_reward_model.transform, LayerMask.NameToLayer("GUI-lighting"), false);
	}

	private reward_type TryRollGems()
	{
		float value = UnityEngine.Random.value;
		if (value >= CalcGemChances() || always_item)
		{
			return TryRollCreature();
		}
		short globalShort = PlayerData.Instance.GetGlobalShort("n_free_gems_given");
		reward_count = ((UnityEngine.Random.value < 0.25f || globalShort == 0) ? 2 : 1);
		PlayerData.Instance.SetGlobalShort("n_free_gems_given", reward_count + PlayerData.Instance.GetGlobalShort("n_free_gems_given"));
		return reward_type.gems;
	}

	private reward_type TryRollCreature()
	{
		int globalShort = PlayerData.Instance.GetGlobalShort("n_free_creatures");
		if (globalShort < 12 && !always_item)
		{
			float num = ((globalShort < 3) ? 0.5f : ((globalShort > 6) ? 0.03f : 0.2f));
			if (UnityEngine.Random.value < num)
			{
				List<string> list = new List<string>();
				for (int i = 0; i < globalShort; i++)
				{
					list.Add(PlayerData.Instance.GetGlobalString("free_animal_" + i));
				}
				List<string> list2 = new List<string>();
				foreach (KeyValuePair<string, List<string>> premium_creature_name in CreatureMorpher.Instance.premium_creature_names)
				{
					foreach (string item in premium_creature_name.Value)
					{
						if (!list.Contains(item))
						{
							list2.Add(item);
						}
					}
				}
				reward_creature_name = list2[UnityEngine.Random.Range(0, list2.Count)];
				PlayerData.Instance.SetGlobalString("free_animal_" + globalShort, reward_creature_name);
				PlayerData.Instance.SetGlobalShort("n_free_creatures", globalShort + 1);
				return reward_type.creature;
			}
		}
		return reward_type.items;
	}

	private float CalcGemChances()
	{
		int num = (int)((float)PlayerData.Instance.GetGlobalShort("n_free_gems_given") / 15f);
		short globalShort = PlayerData.Instance.GetGlobalShort("GEMS");
		float num2;
		switch (num)
		{
		case 0:
			num2 = 0.6f;
			break;
		case 1:
			num2 = (float)globalShort * -0.028f + 0.5f;
			break;
		case 2:
			num2 = (float)globalShort * -0.019f + 0.3f;
			break;
		default:
			num2 = (float)globalShort * -0.0128f + 0.2f;
			break;
		}
		if (num2 <= 0f)
		{
			num2 = 0.03f;
		}
		return num2;
	}
}
