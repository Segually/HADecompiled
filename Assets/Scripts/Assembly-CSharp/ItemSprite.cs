using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSprite : MonoBehaviour
{
	public enum draw_param
	{
		COLOR_req_not_met = 0,
		COLOR_unclickable = 1,
		COLOR_fusionGreen = 2,
		BG_inventory = 3,
		BG_container = 4,
		BG_alwaysVisible = 5,
		COUNT_hide = 6,
		COUNT_show0and1 = 7,
		COUNT_sellPrice = 8,
		COUNT_buyPrice = 9,
		COUNT_zeroPrice = 10,
		REQ_showCrafting = 11,
		REQ_showSkill = 12,
		REQ_hide = 13,
		PREM_ignore = 14,
		PREM_hideLockOnly = 15
	}

	private GameObject background_;

	private GameObject item_graphic_;

	private GameObject overlay_;

	private GameObject requirement_;

	private GameObject count_obj_;

	private GameObject locked_obj_;

	public GameObject model3d_generated_graphic_;

	private GameObject loading_circle_;

	private GameObject damage_indicator_obj;

	private GameObject shield_indicator_obj;

	private Texture2D custom_graphic_texture;

	private bool raycast;

	public void RedrawBasic(InventoryItem item, int count)
	{
		List<draw_param> list = new List<draw_param>();
		Redraw(item, count, list, -1);
	}

	public void RedrawBasicIgnorePremium(InventoryItem item, int count)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.PREM_ignore);
		Redraw(item, count, list, -1);
	}

	public void RedrawBasicHideCount(InventoryItem item, int count)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.COUNT_hide);
		Redraw(item, count, list, -1);
	}

	public void RedrawAsHoverIcon(InventoryItem item, int count)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.COUNT_hide);
		list.Add(draw_param.REQ_hide);
		list.Add(draw_param.PREM_hideLockOnly);
		Redraw(item, count, list, -1);
	}

	public void RedrawAsInventoryNormal(InventoryItem item, int count, int slot_id)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.BG_inventory);
		if (inventory_ctr.Instance.GetItemEquipReqStat(item.item_name) != "" && !inventory_ctr.Instance.check_req_skill_to_equip(inventory_ctr.Instance.GetItemEquipReqStat(item.item_name), inventory_ctr.Instance.GetItemEquipReqLvl(item.item_name)))
		{
			list.Add(draw_param.REQ_showSkill);
			list.Add(draw_param.COLOR_req_not_met);
		}
		Redraw(item, count, list, slot_id);
	}

	public void RedrawAsInventorySellItems(InventoryItem item, int count, int slot_id)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.BG_inventory);
		list.Add(draw_param.COUNT_sellPrice);
		if (!MerchantControl.Instance.IsItemSellable(item) && item.item_name != "Coins")
		{
			list.Add(draw_param.COLOR_unclickable);
			list.Add(draw_param.COUNT_zeroPrice);
		}
		Redraw(item, count, list, slot_id);
	}

	public void RedrawAsInventoryAndHighlightItem(InventoryItem item, int count, int slot_id, string item_highlight)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.BG_inventory);
		if (item.item_name != item_highlight)
		{
			list.Add(draw_param.COLOR_unclickable);
		}
		else
		{
			list.Add(draw_param.PREM_ignore);
		}
		Redraw(item, count, list, slot_id);
	}

	public void RedrawAsContainer(InventoryItem item, int count, int slot_id)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.BG_container);
		Redraw(item, count, list, slot_id);
	}

	public void RedrawAsContainerGreen(InventoryItem item, int count, int slot_id)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.BG_container);
		list.Add(draw_param.COLOR_fusionGreen);
		Redraw(item, count, list, slot_id);
	}

	public void RedrawAsMinigameReward(InventoryItem item, int count)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.BG_alwaysVisible);
		Redraw(item, count, list, -1);
	}

	public void RedrawAsBuyPopupHeader(InventoryItem item, int count)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.COUNT_buyPrice);
		Redraw(item, count, list, -1);
	}

	public void RedrawAsSellPopupHeader(InventoryItem item, int count)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.COUNT_sellPrice);
		Redraw(item, count, list, -1);
	}

	public void RedrawAsBuyItem(InventoryItem item, int count)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.COUNT_show0and1);
		if (item.item_name == "Saved Record")
		{
			list.Add(draw_param.REQ_hide);
		}
		Redraw(item, count, list);
	}

	public void RedrawAsCrafting(InventoryItem item, int count)
	{
		List<draw_param> list = new List<draw_param>();
		int num = GameController.Instance.player_stats[5];
		if (num < inventory_ctr.Instance.GetItemCraftingLevelRequired(item.item_name))
		{
			list.Add(draw_param.REQ_showCrafting);
			list.Add(draw_param.COLOR_req_not_met);
			list.Add(draw_param.COUNT_hide);
		}
		Redraw(item, count, list);
	}

	public void RedrawAsVendingMachineCost(InventoryItem item, int count)
	{
		List<draw_param> list = new List<draw_param>();
		list.Add(draw_param.COUNT_hide);
		list.Add(draw_param.BG_alwaysVisible);
		Redraw(item, count, list, -1);
	}

	private void Redraw(InventoryItem item, int count, List<draw_param> parameters, int slot_id = -1)
	{
		ProcessPreDraw();
		ProcessBackground(item, count, parameters, slot_id);
		ProcessItemGraphic(item, count, parameters, slot_id);
		ProcessModel3d(item, count, parameters);
		ProcessCount(item, count, parameters);
		ProcessRequirement(item, count, parameters);
		ProcessOverlay(item, count, parameters);
		ProcessPremiumLock(item, count, parameters);
		ProcessDamageIndicator(item, count, parameters);
		ProcessShieldIndicator(item, count, parameters);
		List<GameObject> list = new List<GameObject>();
		list.Add(background_);
		list.Add(item_graphic_);
		list.Add(overlay_);
		list.Add(model3d_generated_graphic_);
		list.Add(loading_circle_);
		list.Add(requirement_);
		list.Add(locked_obj_);
		list.Add(count_obj_);
		list.Add(damage_indicator_obj);
		list.Add(shield_indicator_obj);
		inventory_ctr.Instance.Reorder(list, base.transform);
		if (background_ != null)
		{
			background_.GetComponent<Image>().raycastTarget = raycast;
		}
	}

	private void ProcessPreDraw()
	{
		if (GetComponent<Image>() != null)
		{
			raycast = GetComponent<Image>().raycastTarget;
			UnityEngine.Object.Destroy(GetComponent<Image>());
		}
		if (custom_graphic_texture != null)
		{
			UnityEngine.Object.Destroy(custom_graphic_texture);
		}
		if (model3d_generated_graphic_ != null)
		{
			ItemScreenshotTaker.Instance.CancelScreenshotByItemSprite(this);
			DestroyLoading();
		}
	}

	private void ProcessBackground(InventoryItem item, int count, List<draw_param> parameters, int slot_id)
	{
		if (parameters.Contains(draw_param.BG_alwaysVisible))
		{
			CreateBackground(inventory_ctr.Instance.sprite_blank_bg);
		}
		else if (parameters.Contains(draw_param.BG_inventory))
		{
			if (item.item_name != "")
			{
				CreateBackground(inventory_ctr.Instance.sprite_blank_bg);
			}
			else if (slot_id == inventory_ctr.hand_index)
			{
				CreateBackground(inventory_ctr.Instance.blank_hand_sprite);
			}
			else if (slot_id == inventory_ctr.hat_index)
			{
				CreateBackground(inventory_ctr.Instance.blank_hat_sprite);
			}
			else if (slot_id == inventory_ctr.body_index)
			{
				CreateBackground(inventory_ctr.Instance.blank_body_sprite);
			}
			else
			{
				DestroyBackground();
			}
		}
		else if (parameters.Contains(draw_param.BG_container))
		{
			if (item.item_name != "")
			{
				CreateBackground(inventory_ctr.Instance.sprite_blank_bg);
			}
			else
			{
				switch (inventory_ctr.Instance.container_style)
				{
				case inventory_ctr.container_style_t.world_container:
				case inventory_ctr.container_style_t.trading_table:
				{
					string item_name = GameController.Instance.interacting_element_item.item_name;
					if (item_name == "Egg Fuser")
					{
						if (slot_id == 11)
						{
							CreateBackground(inventory_ctr.Instance.spr_fuser_fuelSlot);
						}
						else if (slot_id == 7 || slot_id == 5)
						{
							CreateBackground(inventory_ctr.Instance.spr_fuser_eggSlot);
						}
						else
						{
							DestroyBackground();
						}
					}
					else if (item_name == "Weapon Display")
					{
						if (slot_id == 6)
						{
							CreateBackground(inventory_ctr.Instance.spr_weapondisplay_empty);
						}
						else
						{
							DestroyBackground();
						}
					}
					else if (item_name == "Large Weapon Display")
					{
						if (slot_id == 5 || slot_id == 7)
						{
							CreateBackground(inventory_ctr.Instance.spr_weapondisplay_empty);
						}
						else
						{
							DestroyBackground();
						}
					}
					else if (item_name == "Armor Display")
					{
						if (slot_id == 7)
						{
							CreateBackground(inventory_ctr.Instance.blank_body_sprite);
						}
						else if (slot_id == 5)
						{
							CreateBackground(inventory_ctr.Instance.blank_hat_sprite);
						}
						else
						{
							DestroyBackground();
						}
					}
					else if (item_name == "Custom Statue")
					{
						switch (slot_id)
						{
						case 7:
							CreateBackground(inventory_ctr.Instance.spr_weapondisplay_empty);
							break;
						case 6:
							CreateBackground(inventory_ctr.Instance.blank_body_sprite);
							break;
						case 5:
							CreateBackground(inventory_ctr.Instance.blank_hat_sprite);
							break;
						default:
							DestroyBackground();
							break;
						}
					}
					else
					{
						DestroyBackground();
					}
					break;
				}
				case inventory_ctr.container_style_t.companion_pockets:
					switch (slot_id)
					{
					case 13:
						CreateBackground(inventory_ctr.Instance.blank_hand_sprite);
						break;
					case 8:
						CreateBackground(inventory_ctr.Instance.blank_body_sprite);
						break;
					case 3:
						CreateBackground(inventory_ctr.Instance.blank_hat_sprite);
						break;
					default:
						DestroyBackground();
						break;
					}
					break;
				}
			}
		}
		else
		{
			string item_name2 = item.item_name;
			if (item_name2 != "" && item_name2 != "DEBUG-quest-header-equals" && item_name2 != "DEBUG-happy-header-android" && item_name2 != "DEBUG-happy-header-ios")
			{
				CreateBackground(inventory_ctr.Instance.sprite_blank_bg);
			}
			else
			{
				DestroyBackground();
			}
		}
		if (!(background_ == null))
		{
			Image component = background_.GetComponent<Image>();
			if (parameters.Contains(draw_param.COLOR_unclickable))
			{
				component.color = inventory_ctr.Instance.col_inv_unclickable;
			}
			else if (parameters.Contains(draw_param.COLOR_req_not_met))
			{
				component.color = new Color(0.46f, 0.46f, 0.46f, 1f);
			}
			else if (parameters.Contains(draw_param.COLOR_fusionGreen))
			{
				component.color = inventory_ctr.Instance.fusion_inprogress_slot_color;
			}
			else if (inventory_ctr.Instance.iap_allowed(item) || parameters.Contains(draw_param.PREM_ignore))
			{
				component.color = new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				component.color = inventory_ctr.Instance.premium_inv_color;
			}
		}
	}

	private void ProcessItemGraphic(InventoryItem item, int count, List<draw_param> parameters, int slot_id)
	{
		if (item.item_name != "")
		{
			if (ResourceControl.Instance.GetStringFromItemFile(item.item_name, "show_model3d") != "true")
			{
				if (item.GetShort("custom_graphic_version") == 0)
				{
					if (item.item_name == "Coins")
					{
						CreateItemGraphic(null);
						CreateLoading();
						ResourceControl.Instance.AssignItemSprite(InventoryUtils.GetCoinSprite(count), item_graphic_.GetComponent<Image>(), DestroyLoading);
					}
					else if (item.item_name != "")
					{
						InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
						if (slot_id == 11 && interacting_element_item.item_name == "Egg Fuser" && interacting_element_item.GetShort("started_fusion") == 1 && !interacting_element_item.HasActiveRespawn("fuser_spawn"))
						{
							CreateItemGraphic(null);
							item_graphic_.GetComponent<Image>().sprite = inventory_ctr.Instance.spr_mystery_egg;
						}
						else
						{
							CreateItemGraphic(null);
							CreateLoading();
							ResourceControl.Instance.AssignItemSprite(inventory_ctr.Instance.GetItemSpritePath(item.item_name), item_graphic_.GetComponent<Image>(), DestroyLoading);
						}
					}
					else
					{
						DestroyItemGraphic();
					}
				}
				else
				{
					CreateItemGraphic(LoadCustomGraphic(item));
				}
			}
			else
			{
				DestroyItemGraphic();
			}
		}
		else
		{
			DestroyItemGraphic();
		}
		if (item_graphic_ == null)
		{
			return;
		}
		Image component = item_graphic_.GetComponent<Image>();
		if (item.GetShort("custom_graphic_version") != 0)
		{
			component.color = new Color(1f, 1f, 1f, 1f);
		}
		else if (item.item_name == "Egg")
		{
			Color color = (parameters.Contains(draw_param.COLOR_fusionGreen) ? inventory_ctr.Instance.fusion_inprogress_slot_color : ((!(item.GetString("egg_monster") != "")) ? new Color(1f, 1f, 1f, 1f) : MobControl.Instance.GetOverheadNameColor(item.GetString("egg_monster") + item.GetString("egg_monster"))));
			component.color = new Color(color.r, color.g, color.b, parameters.Contains(draw_param.COLOR_unclickable) ? 0.3f : color.a);
		}
		else if (item.item_name == "Saved Record")
		{
			short @short = item.GetShort("npc_record_index");
			Color color2 = ((@short != 0) ? DevBuildControl.Instance.NPC_musicboxes[@short].sale_col : new Color((float)item.GetShort("record_col_R") / 100f, (float)item.GetShort("record_col_G") / 100f, (float)item.GetShort("record_col_B") / 100f, 1f));
			component.color = new Color(color2.r, color2.g, color2.b, parameters.Contains(draw_param.COLOR_unclickable) ? 0.25f : color2.a);
		}
		else if (item.item_name == "Blank Record")
		{
			if (!parameters.Contains(draw_param.COLOR_unclickable))
			{
				component.color = new Color(0.6f, 0.6f, 0.6f, 1f);
			}
			else
			{
				component.color = new Color(0.6f, 0.6f, 0.6f, 0.3f);
			}
		}
		else
		{
			if (parameters.Contains(draw_param.COLOR_unclickable))
			{
				component.color = inventory_ctr.Instance.col_inv_unclickable;
			}
			else if (parameters.Contains(draw_param.COLOR_req_not_met))
			{
				component.color = new Color(0.46f, 0.46f, 0.46f, 1f);
			}
			else if (parameters.Contains(draw_param.COLOR_fusionGreen))
			{
				component.color = inventory_ctr.Instance.fusion_inprogress_slot_color;
			}
			else if (inventory_ctr.Instance.iap_allowed(item) || parameters.Contains(draw_param.PREM_ignore))
			{
				component.color = new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				component.color = inventory_ctr.Instance.premium_inv_color;
			}
		}
	}

	private void ProcessModel3d(InventoryItem item, int count, List<draw_param> parameters)
	{
		if (item.item_name != "" && ResourceControl.Instance.GetStringFromItemFile(item.item_name, "show_model3d") == "true")
		{
			CreateModel3D();
			CreateLoading();
			ItemScreenshotTaker.Instance.QueueForScreenshot(item, model3d_generated_graphic_, this);
		}
		else
		{
			DestroyModel3D();
		}
		if (!(model3d_generated_graphic_ == null))
		{
			RawImage component = model3d_generated_graphic_.GetComponent<RawImage>();
			if (parameters.Contains(draw_param.COLOR_unclickable))
			{
				component.color = inventory_ctr.Instance.col_inv_unclickable;
			}
			else if (parameters.Contains(draw_param.COLOR_req_not_met))
			{
				component.color = new Color(0.46f, 0.46f, 0.46f, 1f);
			}
			else if (parameters.Contains(draw_param.COLOR_fusionGreen))
			{
				component.color = inventory_ctr.Instance.fusion_inprogress_slot_color;
			}
			else if (inventory_ctr.Instance.iap_allowed(item) || parameters.Contains(draw_param.PREM_ignore))
			{
				component.color = new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				component.color = inventory_ctr.Instance.premium_inv_color;
			}
		}
	}

	private void ProcessCount(InventoryItem item, int count, List<draw_param> parameters)
	{
		if (item.item_name != "" && !parameters.Contains(draw_param.COUNT_hide))
		{
			if (item.item_name == "Coins" || parameters.Contains(draw_param.COUNT_show0and1))
			{
				CreateCount(string.Format("{0:n0}", count));
			}
			else if (parameters.Contains(draw_param.COUNT_buyPrice))
			{
				CreateCount("$" + string.Format("{0:n0}", MerchantControl.Instance.GetBuyPrice(item) * count));
			}
			else if (parameters.Contains(draw_param.COUNT_sellPrice))
			{
				if (parameters.Contains(draw_param.COUNT_zeroPrice))
				{
					CreateCount("$0");
				}
				else
				{
					CreateCount("$" + string.Format("{0:n0}", MerchantControl.Instance.GetSellPrice(item) * count));
				}
			}
			else if (item.item_name == "Bonsai Tree")
			{
				int @short = item.GetShort("bonsai_age");
				if (@short == 0)
				{
					CreateCount("? years");
				}
				else
				{
					CreateCount(@short + " years");
				}
			}
			else if (count < 2)
			{
				DestroyCount();
			}
			else
			{
				CreateCount(string.Format("{0:n0}", count));
			}
		}
		else
		{
			DestroyCount();
		}
		if (count_obj_ == null)
		{
			return;
		}
		Text component = count_obj_.transform.Find("Text").GetComponent<Text>();
		bool flag = item.item_name == "Coins";
		if (flag || (!parameters.Contains(draw_param.COUNT_show0and1) && parameters.Contains(draw_param.COUNT_buyPrice)))
		{
			if (!parameters.Contains(draw_param.COLOR_unclickable))
			{
				component.color = new Color(1f, 0.936f, 0.1839f, 1f);
			}
			else
			{
				component.color = new Color(0.2f, 0.19f, 0.09f, 1f);
			}
			return;
		}
		if (!parameters.Contains(draw_param.COUNT_show0and1))
		{
			if (parameters.Contains(draw_param.COUNT_sellPrice))
			{
				if (!parameters.Contains(draw_param.COLOR_unclickable))
				{
					component.color = new Color(0.66f, 1f, 0.38f, 1f);
				}
				else
				{
					component.color = new Color(0.5f, 0.5f, 0.5f, 1f);
				}
				return;
			}
			if (item.item_name == "Bonsai Tree")
			{
				if (!parameters.Contains(draw_param.COLOR_unclickable))
				{
					component.color = new Color(0.81f, 0.81f, 0.81f, 1f);
				}
				else
				{
					component.color = new Color(0.17f, 0.19f, 0.25f, 1f);
				}
				return;
			}
		}
		if (!parameters.Contains(draw_param.COLOR_unclickable))
		{
			component.color = new Color(0.53f, 0.93f, 1f, 1f);
		}
		else
		{
			component.color = new Color(0.14f, 0.22f, 0.23f, 1f);
		}
	}

	private void ProcessDamageIndicator(InventoryItem item, int count, List<draw_param> parameters)
	{
		if (item.item_name != "")
		{
			int intFromItemFile = ResourceControl.Instance.GetIntFromItemFile(item.item_name, "Damage Base");
			if (intFromItemFile != 0)
			{
				CreateDamageIndicator(intFromItemFile);
				return;
			}
		}
		DestroyDamageIndicator();
	}

	private void ProcessShieldIndicator(InventoryItem item, int count, List<draw_param> parameters)
	{
		if (item.item_name != "")
		{
			float floatFromItemFile = ResourceControl.Instance.GetFloatFromItemFile(item.item_name, "Defend Amount");
			if (floatFromItemFile != 0f)
			{
				CreateShieldIndicator(floatFromItemFile);
				return;
			}
		}
		DestroyShieldIndicator();
	}

	private void ProcessRequirement(InventoryItem item, int count, List<draw_param> parameters)
	{
		string text = null;
		if (item.item_name != "" && !parameters.Contains(draw_param.REQ_hide))
		{
			if (parameters.Contains(draw_param.REQ_showSkill))
			{
				text = inventory_ctr.Instance.GetItemEquipReqStat(item.item_name) + " " + inventory_ctr.Instance.GetItemEquipReqLvl(item.item_name) + "+";
			}
			else if (parameters.Contains(draw_param.REQ_showCrafting))
			{
				text = "Crafting " + inventory_ctr.Instance.GetItemCraftingLevelRequired(item.item_name) + "+";
			}
			else if (item.item_name == "Saved Record" || item.item_name == "Blank Record")
			{
				text = "EMPTY";
				if (item.item_name == "Saved Record")
				{
					short @short = item.GetShort("npc_record_index");
					text = ((@short != 0) ? DevBuildControl.Instance.NPC_musicboxes[@short].sale_name : item.GetString("record_name"));
				}
			}
		}
		if (text != null)
		{
			CreateRequirement(text);
		}
		else
		{
			DestroyRequirement();
		}
		if (requirement_ != null)
		{
			Text component = requirement_.transform.Find("Text").GetComponent<Text>();
			if (!parameters.Contains(draw_param.COLOR_unclickable))
			{
				component.color = new Color(0.76f, 0.76f, 0.76f, 1f);
			}
			else
			{
				component.color = new Color(0.23f, 0.24f, 0.28f, 1f);
			}
		}
	}

	private void ProcessOverlay(InventoryItem item, int count, List<draw_param> parameters)
	{
		if (item.item_name != "" && ResourceControl.Instance.GetStringFromItemFile(item.item_name, "show_model3d") != "true")
		{
			if (item.item_name == "Egg")
			{
				string @string = item.GetString("egg_monster");
				if (!Startup.StringNullOrWhitespace(@string))
				{
					CreateOverlay(null, 0.5f);
					ResourceControl.Instance.AssignCreatureSprite(@string, overlay_.GetComponent<Image>());
				}
				else
				{
					DestroyOverlay();
				}
			}
			else if (item.item_name == "Fossil")
			{
				string string2 = item.GetString("fossil_monster");
				if (!Startup.StringNullOrWhitespace(string2))
				{
					CreateOverlay(null, 0.5f);
					ResourceControl.Instance.AssignCreatureSprite(string2, overlay_.GetComponent<Image>());
				}
				else
				{
					DestroyOverlay();
				}
			}
			else if (item.item_name == "Saved Record" || item.item_name == "Blank Record")
			{
				CreateOverlay(inventory_ctr.Instance.record_center_sprite, 1f);
			}
			else
			{
				DestroyOverlay();
			}
		}
		else
		{
			DestroyOverlay();
		}
		if (overlay_ == null)
		{
			return;
		}
		Image component = overlay_.GetComponent<Image>();
		if (item.item_name == "Egg")
		{
			if (parameters.Contains(draw_param.COLOR_unclickable))
			{
				component.color = new Color(0.2314f, 0.3216f, 0.6118f, 0.65f);
			}
			else if (parameters.Contains(draw_param.COLOR_fusionGreen))
			{
				component.color = inventory_ctr.Instance.fusion_inprogress_slot_color;
			}
			else
			{
				component.color = new Color(1f, 1f, 1f, 1f);
			}
		}
		else if (item.item_name == "Fossil")
		{
			component.color = inventory_ctr.Instance.fossil_col;
		}
		else if (item.item_name == "Saved Record" || item.item_name == "Blank Record")
		{
			if (parameters.Contains(draw_param.COLOR_unclickable))
			{
				component.color = new Color(1f, 1f, 1f, 0.15f);
			}
			else
			{
				component.color = new Color(1f, 1f, 1f, 1f);
			}
		}
		else
		{
			DestroyOverlay();
		}
	}

	private void ProcessPremiumLock(InventoryItem item, int count, List<draw_param> parameters)
	{
		if (item.item_name != "" && !parameters.Contains(draw_param.PREM_ignore) && !parameters.Contains(draw_param.REQ_showCrafting) && !inventory_ctr.Instance.iap_allowed(item) && !parameters.Contains(draw_param.PREM_hideLockOnly))
		{
			CreatePremiumLock();
		}
		else
		{
			DestroyPremiumLock();
		}
		if (!(locked_obj_ == null))
		{
			Image component = locked_obj_.GetComponent<Image>();
			if (!parameters.Contains(draw_param.COLOR_unclickable))
			{
				component.color = new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				component.color = new Color(0.22f, 0.27f, 0.33f, 1f);
			}
		}
	}

	private void DestroyBackground()
	{
		if (background_ != null)
		{
			UnityEngine.Object.Destroy(background_.gameObject);
		}
	}

	public void DestroyLoading()
	{
		if (loading_circle_ != null)
		{
			UnityEngine.Object.Destroy(loading_circle_);
			loading_circle_ = null;
		}
	}

	private void DestroyModel3D()
	{
		if (model3d_generated_graphic_ != null)
		{
			UnityEngine.Object.Destroy(model3d_generated_graphic_);
		}
	}

	private void DestroyItemGraphic()
	{
		if (item_graphic_ != null)
		{
			UnityEngine.Object.Destroy(item_graphic_.gameObject);
		}
	}

	private void DestroyOverlay()
	{
		if (overlay_ != null)
		{
			UnityEngine.Object.Destroy(overlay_.gameObject);
		}
	}

	private void DestroyRequirement()
	{
		if (requirement_ != null)
		{
			UnityEngine.Object.Destroy(requirement_.gameObject);
		}
	}

	private void DestroyPremiumLock()
	{
		if (locked_obj_ != null)
		{
			UnityEngine.Object.Destroy(locked_obj_.gameObject);
		}
	}

	private void DestroyCount()
	{
		if (count_obj_ != null)
		{
			UnityEngine.Object.Destroy(count_obj_.gameObject);
		}
	}

	private void DestroyDamageIndicator()
	{
		if (damage_indicator_obj != null)
		{
			UnityEngine.Object.Destroy(damage_indicator_obj.gameObject);
		}
	}

	private void DestroyShieldIndicator()
	{
		if (shield_indicator_obj != null)
		{
			UnityEngine.Object.Destroy(shield_indicator_obj.gameObject);
		}
	}

	private void CreateLoading()
	{
		if (loading_circle_ == null)
		{
			loading_circle_ = UnityEngine.Object.Instantiate(inventory_ctr.Instance.prefab_item_loading);
			loading_circle_.transform.SetParent(base.transform);
			loading_circle_.transform.localPosition = Vector3.zero;
			loading_circle_.transform.localRotation = Quaternion.identity;
			loading_circle_.transform.localScale = Vector3.one;
		}
	}

	private void CreateBackground(Sprite sprite, float scale = 1f)
	{
		if (background_ == null)
		{
			background_ = UnityEngine.Object.Instantiate(inventory_ctr.Instance.prefab_item_bg);
			background_.transform.SetParent(base.transform);
			background_.transform.localPosition = Vector3.zero;
			background_.transform.localRotation = Quaternion.identity;
			background_.transform.localScale = Vector3.one * scale;
		}
		background_.GetComponent<Image>().sprite = sprite;
	}

	private void CreateItemGraphic(Sprite sprite)
	{
		if (item_graphic_ == null)
		{
			item_graphic_ = UnityEngine.Object.Instantiate(inventory_ctr.Instance.prefab_item_graphic);
			item_graphic_.transform.SetParent(base.transform);
			item_graphic_.transform.localPosition = Vector3.zero;
			item_graphic_.transform.localRotation = Quaternion.identity;
			item_graphic_.transform.localScale = Vector3.one;
		}
		item_graphic_.GetComponent<Image>().sprite = sprite;
	}

	private void CreateCount(string count_str)
	{
		if (count_obj_ == null)
		{
			count_obj_ = UnityEngine.Object.Instantiate(inventory_ctr.Instance.prefab_item_count);
			count_obj_.transform.SetParent(base.transform);
			count_obj_.transform.localPosition = new Vector3(41f, -69f, 0f);
			count_obj_.transform.localRotation = Quaternion.identity;
			count_obj_.transform.localScale = Vector3.one;
		}
		Text component = count_obj_.transform.Find("Text").GetComponent<Text>();
		component.text = count_str;
		float num = component.preferredWidth + 38f;
		if (num > 157f)
		{
			num = 157f;
		}
		((RectTransform)count_obj_.transform).sizeDelta = new Vector2(num, 50f);
	}

	private void CreateDamageIndicator(int damage_base)
	{
		if (damage_indicator_obj == null)
		{
			damage_indicator_obj = UnityEngine.Object.Instantiate(inventory_ctr.Instance.prefab_item_dmg_indicator);
			damage_indicator_obj.transform.SetParent(base.transform);
			damage_indicator_obj.transform.localPosition = new Vector3(-42f, 47f, 0f);
			damage_indicator_obj.transform.localRotation = Quaternion.identity;
			damage_indicator_obj.transform.localScale = Vector3.one;
		}
		damage_indicator_obj.transform.Find("Text").GetComponent<Text>().text = "+" + damage_base;
	}

	private void CreateShieldIndicator(float defense_base)
	{
		if (shield_indicator_obj == null)
		{
			shield_indicator_obj = UnityEngine.Object.Instantiate(inventory_ctr.Instance.prefab_item_shield_indicator);
			shield_indicator_obj.transform.SetParent(base.transform);
			shield_indicator_obj.transform.localPosition = new Vector3(-47f, 47f, 0f);
			shield_indicator_obj.transform.localRotation = Quaternion.identity;
			shield_indicator_obj.transform.localScale = Vector3.one;
		}
		shield_indicator_obj.transform.Find("Text").GetComponent<Text>().text = "+" + defense_base;
	}

	private void CreateRequirement(string str)
	{
		if (requirement_ == null)
		{
			requirement_ = UnityEngine.Object.Instantiate(inventory_ctr.Instance.prefab_item_requirement);
			requirement_.transform.SetParent(base.transform);
			requirement_.transform.localPosition = new Vector3(-1f, -37f, 0f);
			requirement_.transform.localRotation = Quaternion.identity;
			requirement_.transform.localScale = Vector3.one * 0.84f;
		}
		requirement_.transform.Find("Text").GetComponent<Text>().text = str;
	}

	private void CreateOverlay(Sprite sprite, float scale)
	{
		if (overlay_ == null)
		{
			overlay_ = UnityEngine.Object.Instantiate(inventory_ctr.Instance.prefab_item_overlay);
			overlay_.transform.SetParent(base.transform);
			overlay_.transform.localPosition = Vector3.zero;
			overlay_.transform.localRotation = Quaternion.identity;
			overlay_.transform.localScale = Vector3.one;
		}
		overlay_.GetComponent<Image>().sprite = sprite;
		overlay_.transform.localScale = Vector3.one * scale;
	}

	private void CreatePremiumLock()
	{
		if (locked_obj_ != null)
		{
			return;
		}
		locked_obj_ = UnityEngine.Object.Instantiate(inventory_ctr.Instance.prefab_item_locked);
		locked_obj_.transform.SetParent(base.transform);
		locked_obj_.transform.localPosition = new Vector3(69f, 58f, 0f);
		locked_obj_.transform.localRotation = Quaternion.identity;
		locked_obj_.transform.localScale = Vector3.one;
	}

	private void CreateModel3D()
	{
		if (model3d_generated_graphic_ == null)
		{
			model3d_generated_graphic_ = UnityEngine.Object.Instantiate(inventory_ctr.Instance.prefab_model3d_graphic);
			model3d_generated_graphic_.transform.SetParent(base.transform);
			model3d_generated_graphic_.transform.localPosition = Vector3.zero;
			model3d_generated_graphic_.transform.localRotation = Quaternion.identity;
			model3d_generated_graphic_.transform.localScale = Vector3.one;
		}
	}

	public void RedrawBasicHideCount(string item_name, int count)
	{
		RedrawBasicHideCount(new InventoryItem(item_name), count);
	}

	public void RedrawAsInventoryNormal(string item_name, int count, int slot_id)
	{
		RedrawAsInventoryNormal(new InventoryItem(item_name), count, slot_id);
	}

	public static void RecursiveApplyLayer(Transform T, int layer, bool ignore_root)
	{
		if (!ignore_root)
		{
			T.gameObject.layer = layer;
		}
		for (int i = 0; i < T.childCount; i++)
		{
			RecursiveApplyLayer(T.GetChild(i), layer, false);
		}
	}

	private Sprite LoadCustomGraphic(InventoryItem item)
	{
		int @long = item.GetLong("n_img_bts");
		byte[] array = new byte[@long];
		int num = 0;
		byte[] array2 = null;
		int num2 = 0;
		for (int i = 0; i < @long; i++)
		{
			if (array2 == null)
			{
				array2 = BitConverter.GetBytes(item.GetLong("ib" + num));
			}
			array[i] = array2[num2];
			num2++;
			if (num2 == 4)
			{
				num2 = 0;
				array2 = null;
				num++;
			}
		}
		custom_graphic_texture = new Texture2D(128, 128);
		custom_graphic_texture.LoadImage(array);
		return Sprite.Create(custom_graphic_texture, new Rect(0f, 0f, custom_graphic_texture.width, custom_graphic_texture.height), Vector2.zero);
	}

	private void OnDestroy()
	{
		if (custom_graphic_texture != null)
		{
			UnityEngine.Object.Destroy(custom_graphic_texture);
		}
	}

	public void Model3DScreenshotComplete(Texture tex)
	{
		if (model3d_generated_graphic_ != null)
		{
			model3d_generated_graphic_.GetComponent<RawImage>().texture = tex;
			model3d_generated_graphic_.GetComponent<RawImage>().enabled = true;
		}
		DestroyLoading();
	}
}
