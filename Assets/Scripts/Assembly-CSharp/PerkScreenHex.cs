using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkScreenHex
{
	public enum hex_type
	{
		none = 0,
		outline = 1,
		perk = 2,
		text = 3
	}

	public int hex_dev_x;

	public int hex_dev_y;

	public int hex_dev_offset;

	public hex_type type;

	public Dictionary<string, string> extra_values = new Dictionary<string, string>();

	public GameObject obj;

	public List<GameObject> corresponding_line_objects = new List<GameObject>();

	public Vector3 local_position
	{
		get
		{
			if (hex_dev_offset != 0)
			{
				return new Vector3(PerkScreen.hex_w * (float)hex_dev_x + PerkScreen.hex_w * 0.5f, PerkScreen.hex_h * (float)hex_dev_y * 2f + PerkScreen.hex_h, 0f);
			}
			return new Vector3(PerkScreen.hex_w * (float)hex_dev_x, PerkScreen.hex_h * (float)hex_dev_y * 2f, 0f);
		}
	}

	public PerkScreenHex(hex_type type, Dictionary<string, string> extra_values, int hex_dev_x, int hex_dev_y, int hex_dev_offset)
	{
		this.type = type;
		this.extra_values = extra_values;
		this.hex_dev_x = hex_dev_x;
		this.hex_dev_y = hex_dev_y;
		this.hex_dev_offset = hex_dev_offset;
	}

	public void Redraw(bool hide_rings = false, bool show_unlock_animation_A = false, bool show_unlock_animation_B = false, bool post_unlock = false)
	{
		if (obj != null)
		{
			Object.Destroy(obj);
		}
		foreach (GameObject corresponding_line_object in corresponding_line_objects)
		{
			Object.Destroy(corresponding_line_object);
		}
		corresponding_line_objects.Clear();
		RedrawMainObj(hide_rings, show_unlock_animation_A, show_unlock_animation_B, post_unlock);
		RedrawLineSegments();
	}

	private void RedrawMainObj(bool hide_rings, bool show_unlock_animation_A, bool show_unlock_animation_B, bool post_unlock)
	{
		switch (type)
		{
		case hex_type.text:
			obj = Object.Instantiate(PerkScreen.Instance.prefab_text);
			obj.transform.SetParent(PerkScreen.Instance.scrollwheel_parent.transform.Find("folder_outlines"));
			if (extra_values.ContainsKey("hex_text_str"))
			{
				obj.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = extra_values["hex_text_str"];
			}
			break;
		case hex_type.perk:
		{
			obj = Object.Instantiate(PerkScreen.Instance.prefab_perk_hex);
			obj.transform.SetParent(PerkScreen.Instance.scrollwheel_parent.transform.Find("folder_perks"));
			if (!extra_values.ContainsKey("hex_perk_key"))
			{
				break;
			}
			string text = extra_values["hex_perk_key"];
			if (!PerkScreen.Instance.spend_points_mode)
			{
				Object.Destroy(obj.transform.Find("Upgradable").gameObject);
				Object.Destroy(obj.transform.Find("Upgrade").gameObject);
			}
			else if (!PerkControl.Instance.CanUnlockNextLevel(PerkControl.Instance.GetPerkDataForInfoDisplay(text)) && !show_unlock_animation_B && !post_unlock)
			{
				Object.Destroy(obj.transform.Find("Upgradable").gameObject);
				Object.Destroy(obj.transform.Find("Upgrade").gameObject);
			}
			else if (show_unlock_animation_A)
			{
				obj.transform.Find("Upgrade").gameObject.SetActive(true);
				obj.GetComponent<Animation>().Play("perk-upgrade");
				Object.Destroy(obj.transform.Find("Upgradable").gameObject);
			}
			else if (show_unlock_animation_B)
			{
				obj.transform.Find("Upgrade").gameObject.SetActive(true);
				obj.GetComponent<Animation>().Play("perk-upgrade-2");
				Object.Destroy(obj.transform.Find("Upgradable").gameObject);
			}
			else
			{
				GameObject gameObject = obj.transform.Find("Upgradable").gameObject;
				if (hide_rings)
				{
					Object.Destroy(gameObject);
				}
				else
				{
					gameObject.SetActive(true);
				}
				Object.Destroy(obj.transform.Find("Upgrade").gameObject);
			}
			int perkLevel = PerkControl.Instance.GetPerkLevel(text);
			GameObject gameObject2 = obj.transform.Find("Unlocked").gameObject;
			if (perkLevel < 1)
			{
				Object.Destroy(gameObject2);
				obj.transform.Find("Locked").gameObject.SetActive(true);
				if (!PerkScreen.Instance.spend_points_mode)
				{
					obj.transform.Find("Locked").Find("Image").GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
					break;
				}
				bool flag = PerkControl.Instance.CanUnlockNextLevel(PerkControl.Instance.GetPerkDataForInfoDisplay(text));
				Image component = obj.transform.Find("Locked").Find("Image").GetComponent<Image>();
				if (flag)
				{
					component.color = new Color(1f, 1f, 1f, 1f);
				}
				else
				{
					component.color = new Color(0.49f, 0.52f, 0.57f, 1f);
				}
				break;
			}
			gameObject2.SetActive(true);
			Object.Destroy(obj.transform.Find("Locked").gameObject);
			ResourceControl.Instance.AssignPerkSprite(text, obj.transform.Find("Unlocked").Find("Image").GetComponent<Image>());
			GameObject gameObject3 = obj.transform.Find("Unlocked").Find("lvl-circle").gameObject;
			if (perkLevel < 2)
			{
				Object.Destroy(gameObject3);
			}
			else
			{
				gameObject3.SetActive(true);
				obj.transform.Find("Unlocked").Find("lvl-circle").Find("Text").GetComponent<TextMeshProUGUI>().text = perkLevel.ToString() ?? "";
			}
			Image component2;
			if (!PerkScreen.Instance.spend_points_mode)
			{
				component2 = obj.transform.Find("Unlocked").Find("Image").GetComponent<Image>();
			}
			else
			{
				bool flag2 = PerkControl.Instance.CanUnlockNextLevel(PerkControl.Instance.GetPerkDataForInfoDisplay(text));
				component2 = obj.transform.Find("Unlocked").Find("Image").GetComponent<Image>();
				if (!flag2 && !show_unlock_animation_B && !post_unlock)
				{
					component2.color = new Color(0.34f, 0.4f, 0.61f, 1f);
					Object.Destroy(obj.transform.Find("Unlocked").Find("glow").gameObject);
					break;
				}
			}
			component2.color = new Color(1f, 1f, 1f, 1f);
			obj.transform.Find("Unlocked").Find("glow").gameObject.SetActive(true);
			break;
		}
		case hex_type.outline:
			obj = Object.Instantiate(PerkScreen.Instance.prefab_outline);
			obj.transform.SetParent(PerkScreen.Instance.scrollwheel_parent.transform.Find("folder_outlines"));
			break;
		default:
			return;
		}
		obj.transform.localPosition = local_position;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
		obj.SetActive(true);
	}

	private void RedrawLineSegments()
	{
		if (type == hex_type.outline || type == hex_type.perk || type == hex_type.text)
		{
			RedrawLineSegment(0, "clockwise", -120f);
			RedrawLineSegment(1, "clockwise", -180f);
			RedrawLineSegment(2, "clockwise", -240f);
			RedrawLineSegment(3, "clockwise", -300f);
			RedrawLineSegment(4, "clockwise", 0f);
			RedrawLineSegment(5, "clockwise", -60f);
		}
	}

	private void RedrawLineSegment(int point_id, string dir_str, float corresponding_rotation)
	{
		if (!extra_values.ContainsKey("line_pos" + point_id + "_" + dir_str + "_perk_req"))
		{
			return;
		}
		string perk_key = extra_values["line_pos" + point_id + "_" + dir_str + "_perk_req"];
		GameObject gameObject = Object.Instantiate(PerkScreen.Instance.prefab_line);
		gameObject.transform.SetParent(obj.transform);
		gameObject.transform.localScale = Vector3.one;
		switch (point_id)
		{
		case 0:
			gameObject.transform.localPosition = new Vector3(0f, 39f, 0f);
			break;
		case 1:
			gameObject.transform.localPosition = new Vector3(34.5f, 19.2f, 0f);
			break;
		case 2:
			gameObject.transform.localPosition = new Vector3(34.5f, -19.2f, 0f);
			break;
		case 3:
			gameObject.transform.localPosition = new Vector3(0f, -39f, 0f);
			break;
		case 4:
			gameObject.transform.localPosition = new Vector3(-34.5f, -19.2f, 0f);
			break;
		case 5:
			gameObject.transform.localPosition = new Vector3(-34.5f, 19.2f, 0f);
			break;
		}
		gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, corresponding_rotation);
		int perkLevel = PerkControl.Instance.GetPerkLevel(perk_key);
		if (perkLevel < 1)
		{
			gameObject.transform.Find("Image").GetComponent<Image>().sprite = PerkScreen.Instance.dna_strand_locked;
		}
		else
		{
			gameObject.transform.Find("Image").GetComponent<Image>().sprite = PerkScreen.Instance.dna_strand_unlocked;
		}
		gameObject.SetActive(true);
		corresponding_line_objects.Add(gameObject);
		gameObject.transform.SetParent(PerkScreen.Instance.scrollwheel_parent.transform.Find("folder_lines"));
	}
}
