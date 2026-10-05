using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompanionController : MonoBehaviour, OrderedStart
{
	public static CompanionController Instance;

	public Image healthbar_0;

	public Image healthbar_1;

	public Image healthbar_bg_0;

	public Image healthbar_bg_1;

	public List<ActiveCompanion> active_companions = new List<ActiveCompanion>();

	public GameObject type_animatedEgg;

	public GameObject EGG;

	public string selected_companion_name;

	public Color col_behaviour_tab_selected;

	public Color col_behaviour_tab_deselected;

	private int curr_wait_icon_selected;

	private int behaviour_tab_selected;

	private bool attack_XP_orbs_selected = true;

	public Color col_happy_icon_bg;

	public Color col_happy_icon_text;

	public Color col_happy_icon_bar;

	public Color col_info_icon_bg;

	public Color col_info_icon_text;

	public Color col_info_icon_bar;

	public int max_personal_companions_right_now = 2;

	public List<GameObject> trail_nodes__ = new List<GameObject>();

	public static int max_trail_nodes;

	public GameObject companion_nib_0;

	public GameObject companion_nib_1;

	private int curr_companion_page;

	public Color col_switcher_YES;

	public Color col_switcher_NO;

	public static string default_wait_message1;

	public static string default_wait_message2;

	public static string default_statue_message1;

	public static string default_statue_message2;

	public static string default_guard_message1;

	public static string default_guard_message2;

	public static string default_merchant_message1;

	public static string default_merchant_message2;

	public Sprite companion_died_ico;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public static int WaitIconIdToStateIconId(int wait_icon_id)
	{
		return 0;
	}

	public Color GetTextColorFromIcon(int icon_id)
	{
		switch (icon_id)
		{
		case 3:
			return col_info_icon_text;
		case 2:
			return col_happy_icon_text;
		default:
			return default(Color);
		}
	}

	public void PressGuard()
	{
	}

	public bool WasMyGuard(string owner_name)
	{
		return false;
	}

	public void OnGuardDie(string mob_name, string owner_name)
	{
	}

	public void AddDeadCompanion(InventoryItem companion_item)
	{
	}

	public void SetCompanionGuiHealth(int index, float percentage)
	{
	}

	public void MoveCompanionsToPlayerPosition()
	{
		ClearTrailNodes();
		Vector3 vector = ((!(GameController.Instance.player != null)) ? GameController.Instance.prev_player_pos : GameController.Instance.player.transform.position);
		int num = ZoneDataControl.Instance.curr_zonedata.outer_item_rot;
		if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			num = ((num + 1 < 4) ? (num + 1) : (num - 3));
		}
		foreach (ActiveCompanion active_companion in active_companions)
		{
			if (!(active_companion.obj != null))
			{
				continue;
			}
			if (active_companion.hatch_index < 2)
			{
				switch (num)
				{
				case 0:
				case 2:
					if (active_companion.hatch_index == 1)
					{
						active_companion.obj.transform.position = vector + new Vector3(0f, 0f, 1.3f);
					}
					else if (active_companion.hatch_index == 0)
					{
						active_companion.obj.transform.position = vector + new Vector3(0f, 0f, -1.3f);
					}
					break;
				case 1:
				case 3:
					if (active_companion.hatch_index == 1)
					{
						active_companion.obj.transform.position = vector + new Vector3(1.3f, 0f, 0f);
					}
					else if (active_companion.hatch_index == 0)
					{
						active_companion.obj.transform.position = vector + new Vector3(-1.3f, 0f, 0f);
					}
					break;
				}
			}
			else
			{
				active_companion.obj.transform.position = vector + Vector3.up;
			}
			active_companion.obj.GetComponent<CreatureBrainCompanion>().ForgetTargetsAndFollowPlayer();
		}
	}

	public void PressCompanionButton(int index)
	{
	}

	public void RedrawCompanionNibs()
	{
		companion_nib_0.SetActive(false);
		companion_nib_1.SetActive(false);
		if (active_companions.Count > 0 && active_companions[0].obj != null)
		{
			companion_nib_0.SetActive(true);
			companion_nib_0.transform.Find("name").GetComponent<Text>().text = active_companions[0].companion_name;
			Image component = companion_nib_0.transform.Find("happy").GetComponent<Image>();
			Sprite[] overhead_logos = DevBuildControl.Instance.overhead_logos;
			int wait_icon = active_companions[0].wait_icon;
			component.sprite = overhead_logos[(wait_icon != 0) ? ((wait_icon == 1) ? 3 : 2) : 2];
			if (active_companions[0].wait_icon == 0)
			{
				companion_nib_0.GetComponent<Image>().color = col_happy_icon_bg;
				companion_nib_0.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_happy_icon_bar;
			}
			else if (active_companions[0].wait_icon == 1)
			{
				companion_nib_0.GetComponent<Image>().color = col_info_icon_bg;
				companion_nib_0.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_info_icon_bar;
			}
			int wait_iconb = active_companions[0].wait_icon;
			companion_nib_0.transform.Find("name").GetComponent<Text>().color = ((wait_iconb == 1) ? col_info_icon_text : col_happy_icon_text);
		}
		if (active_companions.Count > 1 && active_companions[1].obj != null)
		{
			companion_nib_1.SetActive(true);
			companion_nib_1.transform.Find("name").GetComponent<Text>().text = active_companions[1].companion_name;
			Image component2 = companion_nib_1.transform.Find("happy").GetComponent<Image>();
			Sprite[] overhead_logos2 = DevBuildControl.Instance.overhead_logos;
			int wait_icon2 = active_companions[1].wait_icon;
			component2.sprite = overhead_logos2[(wait_icon2 != 0) ? ((wait_icon2 == 1) ? 3 : 2) : 2];
			if (active_companions[1].wait_icon == 0)
			{
				companion_nib_1.GetComponent<Image>().color = col_happy_icon_bg;
				companion_nib_1.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_happy_icon_bar;
			}
			else if (active_companions[1].wait_icon == 1)
			{
				companion_nib_1.GetComponent<Image>().color = col_info_icon_bg;
				companion_nib_1.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_info_icon_bar;
			}
			int wait_icon2b = active_companions[1].wait_icon;
			companion_nib_1.transform.Find("name").GetComponent<Text>().color = ((wait_icon2b == 1) ? col_info_icon_text : col_happy_icon_text);
		}
	}

	public void PrevPage()
	{
	}

	public void NextPage()
	{
	}

	private void RedrawPage(int dir)
	{
	}

	public void PressCommandAttack()
	{
	}

	public void PressCommandWalk()
	{
	}

	public void PressCommandItems()
	{
	}

	public void PressCommandWait()
	{
	}

	public void PressCommandRename()
	{
	}

	public void PressCommandAdvanced()
	{
	}

	public void PressOptionAttackExpOrb()
	{
	}

	private void RedrawAttackExpOrbSwitcher()
	{
	}

	public void PressAdvancedTab(int index)
	{
	}

	public void PressChangeCompanionWaitIcon(int dir)
	{
	}

	private void RedrawWaitLogo()
	{
	}

	public void PressBackOnAdvanced(bool save)
	{
	}

	public void PressCommandComingSoon()
	{
	}

	public void PressCommandMerchant()
	{
	}

	public void PressBackOnRename()
	{
	}

	public void PressAcceptOnRename()
	{
	}

	public void RenameCompanionManually(string rename_to)
	{
	}

	public void CreateSingleCompanion(ActiveCompanion companion)
	{
	}

	public void CreateSingleCompanion(ActiveCompanion companion, Vector3 V)
	{
	}

	public void CompanionPocketsClosed(BasketContents companion_pockets)
	{
	}

	public void DeleteAllActiveCompanions()
	{
	}

	public void DestroyActiveCompanion(ActiveCompanion companion)
	{
	}

	public ActiveCompanion GetCurrSelectedCompanion()
	{
		return null;
	}

	public void RecreateAllCompanions()
	{
	}

	public void AcceptCompanionFollow()
	{
	}

	public void AddTempCompanion(string creatureA, string creatureB, int start_lvl, string companion_name, InventoryItem hat_, InventoryItem body_, InventoryItem hand_)
	{
	}

	public void DestroyTempCompanions()
	{
		int num = 0;
		foreach (ActiveCompanion active_companion in active_companions)
		{
			num += (active_companion.is_temp_companion ? 1 : 0);
		}
		for (int i = 0; i < num; i++)
		{
			int j;
			for (j = 0; !active_companions[j].is_temp_companion; j++)
			{
			}
			ActiveCompanion activeCompanion = active_companions[j];
			RemoveActiveCompanionAt(activeCompanion.hatch_index);
			if (activeCompanion.obj != null)
			{
				Object.Destroy(activeCompanion.obj);
			}
		}
	}

	public void AcceptFreeCompanion()
	{
	}

	private void SaveCompanionList(List<InventoryItem> companion_item_list, PlayerData.filename_t filename_t)
	{
	}

	public List<InventoryItem> LoadCompanionList(PlayerData.filename_t filename_t)
	{
		return null;
	}

	public void RenameCompanionOnHatch(string input)
	{
	}

	public void SaveActiveCompanions()
	{
	}

	private void FixedUpdate()
	{
	}

	public void ClearTrailNodes()
	{
		foreach (GameObject item in trail_nodes__)
		{
			UnityEngine.Object.Destroy(item);
		}
		trail_nodes__.Clear();
	}

	public void CreateAnimatedEgg(int critterLevel, bool paid, string animal1 = "", string animal2 = "")
	{
	}

	private IEnumerator AnimatedEggCoroutine(GameObject EGG, int critterLevel, string animal1 = "", string animal2 = "")
	{
		return null;
	}

	public void IncreaseCompanionExp(int amount, ActiveCompanion the_companion)
	{
	}

	public void CompanionDeath(ActiveCompanion companion)
	{
	}

	private void RemoveActiveCompanionAt(int X)
	{
		for (int i = X + 1; i < active_companions.Count; i++)
		{
			active_companions[i].hatch_index--;
		}
		active_companions.RemoveAt(X);
		SaveActiveCompanions();
	}
}
