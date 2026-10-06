using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSlot : MonoBehaviour
{
	public enum req_placement
	{
		disable = 0,
		enable_normal = 1,
		enable_OR = 2
	}

	public enum text_area_layout
	{
		condensed_crafting = 0,
		condensed_merchant = 1,
		full = 2
	}

	public enum slots_positioning
	{
		full_size = 0,
		up_and_squashed = 1
	}

	public int index;

	public Image graphic;

	public Text TITLE;

	public Text description;

	public Text costA_txt;

	public Text costB_txt;

	public Text or_text;

	public ItemSprite result_sprite;

	public ItemSprite reqA_sprite;

	public ItemSprite reqB_sprite;

	public ItemSprite buy_ico;

	public Color locked_col;

	public Text buy_cost;

	public GameObject teleporter_particles;

	public CanvasGroup canvasGroup;

	public GameObject online_report_button;

	public static int n_debug_clicks;

	public void OnClick()
	{
		switch (WindowControl.Instance.curr_miniwindow)
		{
		case WindowControl.miniwindow_type_t.inventory_and_crafting:
			inventory_ctr.Instance.PressCraftItem(index);
			return;
		case WindowControl.miniwindow_type_t.inventory_and_merchant:
			inventory_ctr.Instance.PressBuyItem(index);
			return;
		case WindowControl.miniwindow_type_t.quests_and_achieves:
			if (inventory_ctr.Instance.craft_PAGE != 2)
			{
				break;
			}
			switch (n_debug_clicks)
			{
			default:
				goto IL_anim;
			case 0:
				if (index == 0)
				{
					n_debug_clicks = 1;
				}
				goto IL_anim;
			case 1:
				if (index == 1)
				{
					n_debug_clicks = 2;
					goto IL_anim;
				}
				break;
			case 2:
				if (index == 2)
				{
					n_debug_clicks = 3;
					goto IL_anim;
				}
				break;
			case 3:
				if (index == 0)
				{
					n_debug_clicks = 4;
					goto IL_anim;
				}
				break;
			case 4:
				if (index == 2)
				{
					n_debug_clicks = 5;
					goto IL_anim;
				}
				break;
			case 5:
				if (index == 0)
				{
					n_debug_clicks = 6;
					goto IL_anim;
				}
				break;
			case 6:
				if (index == 2)
				{
					ConsoleControl.Instance.PressConsoleButton();
				}
				break;
			}
			n_debug_clicks = 0;
			break;
		case WindowControl.miniwindow_type_t.teleport:
		{
			int num = index + inventory_ctr.Instance.craft_PAGE * 3;
			bool flag;
			if (WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.left)
			{
				GameServerConnector.Instance.FullyInGame();
				flag = CustomTeleporterControl.Instance.IsHardcodedTeleporterDiscovered(num);
			}
			else
			{
				flag = true;
			}
			if (!GameServerConnector.Instance.FullyInGame() || WindowControl.Instance.curr_miniwindow_tab_selected != WindowControl.tab.right || GameServerConnector.Instance.is_host)
			{
				if (WindowControl.Instance.curr_miniwindow_tab_selected == WindowControl.tab.left)
				{
					if (!CustomTeleporterControl.Instance.IsHardcodedTeleporterDiscovered(num))
					{
						PopupControl.Instance.ShowMessage(TranslationControl.Instance.TranslateGeneral("You have not discovered this teleporter yet.", "GUI"), PopupControl.context.message);
					}
					else
					{
						CustomTeleporterControl.Instance.DelayedTeleportTransition(num, CustomTeleporterControl.tele_type.hardcoded);
					}
				}
				else
				{
					CustomTeleporterControl.Instance.DelayedTeleportTransition(num, CustomTeleporterControl.tele_type.custom_local);
				}
			}
			else
			{
				OnlineTeleporter onlineTeleporter = null;
				switch (index)
				{
				case 2:
					onlineTeleporter = CustomTeleporterControl.Instance.teleporter_R;
					break;
				case 1:
					onlineTeleporter = CustomTeleporterControl.Instance.teleporter_mid;
					break;
				case 0:
					onlineTeleporter = CustomTeleporterControl.Instance.teleporter_L;
					break;
				}
				if (onlineTeleporter != null)
				{
					CustomTeleporterControl.Instance.click_teleport_zone_to = onlineTeleporter.to_zone;
					CustomTeleporterControl.Instance.click_teleport_to_chunkX = onlineTeleporter.to_chunkX;
					CustomTeleporterControl.Instance.click_teleport_to_chunkZ = onlineTeleporter.to_chunkZ;
					CustomTeleporterControl.Instance.click_teleport_to_innerX = onlineTeleporter.to_innerX;
					CustomTeleporterControl.Instance.click_teleport_to_innerZ = onlineTeleporter.to_innerZ;
					CustomTeleporterControl.Instance.DelayedTeleportTransition(-1, CustomTeleporterControl.tele_type.custom_network);
				}
				else
				{
					GetComponent<Animation>().Stop();
					GetComponent<Animation>().Play();
				}
			}
			if (flag)
			{
				CustomTeleporterControl.Instance.VisuallyTeleportMainPlayer(CustomTeleporterControl.Instance.click_teleport_zone_to + "," + CustomTeleporterControl.Instance.click_teleport_to_chunkX + "," + CustomTeleporterControl.Instance.click_teleport_to_chunkZ + "," + CustomTeleporterControl.Instance.click_teleport_to_innerX + "," + CustomTeleporterControl.Instance.click_teleport_to_innerZ);
				return;
			}
			break;
		}
		default:
			return;
		}
		IL_anim:
		GetComponent<Animation>().Stop();
		GetComponent<Animation>().Play();
	}

	public void OnClickReport()
	{
		OnlineTeleporter onlineTeleporter;
		switch (index)
		{
		case 2:
			onlineTeleporter = CustomTeleporterControl.Instance.teleporter_R;
			break;
		case 1:
			onlineTeleporter = CustomTeleporterControl.Instance.teleporter_mid;
			break;
		case 0:
			onlineTeleporter = CustomTeleporterControl.Instance.teleporter_L;
			break;
		default:
			return;
		}
		if (onlineTeleporter != null)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("category", "report_object_teleporter");
			dictionary.Add("zone", onlineTeleporter.to_zone);
			dictionary.Add("chunkX", onlineTeleporter.to_chunkX.ToString() ?? "");
			dictionary.Add("chunkZ", onlineTeleporter.to_chunkZ.ToString() ?? "");
			dictionary.Add("innerX", onlineTeleporter.to_innerX.ToString() ?? "");
			dictionary.Add("innerZ", onlineTeleporter.to_innerZ.ToString() ?? "");
			dictionary.Add("title", onlineTeleporter.title);
			dictionary.Add("desc", onlineTeleporter.description);
			dictionary.Add("tele_str", onlineTeleporter.tele_str);
			dictionary.Add("n_strings", "2");
			dictionary.Add("string0_key", "item_id");
			dictionary.Add("string0_val", "Teleporter");
			dictionary.Add("string1_key", "built_by");
			dictionary.Add("string1_val", onlineTeleporter.built_by);
			FriendServerInterface.Instance.report_data.Clear();
			FriendServerInterface.Instance.report_data = dictionary;
			WindowControl.Instance.SwitchMiniwindow(WindowControl.miniwindow_type_t.report_object);
			GameServerReceiver.Instance.DrawReportObjectScreen();
		}
	}

	public void LayOutCraftingSlot(string title_str, string desc, float canvas_alpha, bool show_graphic_sprite, bool show_tele_particles, bool show_market_cost, bool show_result_sprite, bool show_or_text, req_placement req_place, text_area_layout text_layout, slots_positioning slot_positioning, bool show_online_report_button)
	{
		switch (slot_positioning)
		{
		case slots_positioning.up_and_squashed:
			((RectTransform)base.transform).sizeDelta = new Vector2(280f, 602f);
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, -20f, 0f);
			break;
		case slots_positioning.full_size:
			((RectTransform)base.transform).sizeDelta = new Vector2(280f, 660f);
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, -55f, 0f);
			break;
		}
		switch (text_layout)
		{
		case text_area_layout.condensed_crafting:
			description.rectTransform.localPosition = new Vector3(-1f, -42f, 0f);
			description.rectTransform.sizeDelta = new Vector2(240f, 140f);
			break;
		case text_area_layout.condensed_merchant:
			description.rectTransform.localPosition = new Vector3(-1f, -101f, 0f);
			description.rectTransform.sizeDelta = new Vector2(240f, 196f);
			break;
		case text_area_layout.full:
			description.rectTransform.localPosition = new Vector3(-1f, -100f, 0f);
			description.rectTransform.sizeDelta = new Vector2(240f, 236f);
			break;
		}
		switch (req_place)
		{
		case req_placement.enable_OR:
			costA_txt.rectTransform.localPosition = new Vector3(75f, -153f, 0f);
			((RectTransform)reqA_sprite.transform).localPosition = new Vector3(-30f, -157f, 0f);
			costB_txt.rectTransform.localPosition = new Vector3(75f, -265f, 0f);
			((RectTransform)reqB_sprite.transform).localPosition = new Vector3(-30f, -269f, 0f);
			break;
		case req_placement.enable_normal:
			costA_txt.rectTransform.localPosition = new Vector3(75f, -172f, 0f);
			((RectTransform)reqA_sprite.transform).localPosition = new Vector3(-30f, -176f, 0f);
			costB_txt.rectTransform.localPosition = new Vector3(75f, -258f, 0f);
			((RectTransform)reqB_sprite.transform).localPosition = new Vector3(-30f, -262f, 0f);
			break;
		}
		TITLE.text = title_str;
		description.text = desc;
		canvasGroup.alpha = canvas_alpha;
		graphic.gameObject.SetActive(show_graphic_sprite);
		teleporter_particles.SetActive(show_tele_particles);
		buy_ico.gameObject.SetActive(show_market_cost);
		buy_cost.gameObject.SetActive(show_market_cost);
		result_sprite.gameObject.SetActive(show_result_sprite);
		or_text.gameObject.SetActive(show_or_text);
		if (req_place == req_placement.disable)
		{
			reqA_sprite.gameObject.SetActive(false);
			reqB_sprite.gameObject.SetActive(false);
			costA_txt.gameObject.SetActive(false);
			costB_txt.gameObject.SetActive(false);
		}
		else
		{
			reqA_sprite.gameObject.SetActive(true);
			reqB_sprite.gameObject.SetActive(true);
			costA_txt.gameObject.SetActive(true);
			costB_txt.gameObject.SetActive(true);
		}
		online_report_button.SetActive(show_online_report_button);
	}
}
