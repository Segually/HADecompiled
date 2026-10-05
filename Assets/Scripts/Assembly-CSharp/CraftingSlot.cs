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
	}

	public void OnClickReport()
	{
	}

	public void LayOutCraftingSlot(string title_str, string desc, float canvas_alpha, bool show_graphic_sprite, bool show_tele_particles, bool show_market_cost, bool show_result_sprite, bool show_or_text, req_placement req_place, text_area_layout text_layout, slots_positioning slot_positioning, bool show_online_report_button)
	{
	}
}
