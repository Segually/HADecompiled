using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkScreen : MonoBehaviour
{
	private enum header_type_t
	{
		GENETIC_POWERS = 0,
		SPEND_GENOMES = 1
	}

	private enum cancel_context_t
	{
		none = 0,
		cancel_pick_perk = 1,
		okay_on_unlock = 2
	}

	public static PerkScreen Instance;

	public GameObject scrollwheel_parent;

	public float scroll_nib_max_height;

	private float scroll_area_max_y;

	private Vector2 scrollwheel_parentStartPos;

	public float scrollwheel_releaseButtonTimeRemain;

	private float scrollwheel_velocity_x;

	private float scrollwheel_velocity_y;

	private Vector3 scrollwheel_prevMouse;

	private int attempt_click_index;

	private Action<int> on_succeed_click;

	public GameObject prev_clicked_perk_nib;

	private Vector3 prev_clicked_perk_nib_deselect_dist;

	public GameObject perk_select_bounds_right;

	public GameObject perk_select_bounds_left;

	public GameObject perk_select_bounds_top;

	public GameObject perk_select_bounds_bottom;

	private GameObject perk_select_central_point;

	public GameObject screen_central_point;

	private Stopwatch animate_timer;

	private bool animate_focus;

	private Vector3 animate_start;

	private Vector3 animate_end;

	private bool info_bar_showing;

	public GameObject info_bar;

	public GameObject new_data_bar;

	public Image img_equipped_perk_A;

	public Image img_equipped_perk_B;

	public Image img_none_perk_A;

	public Image img_none_perk_B;

	public Image infobox_glow;

	public bool unlocked_at_least_one_perk;

	public bool came_from_levelup_screen;

	public bool spend_points_mode;

	public bool stop_spend_mode_after_one_unlock;

	public GameObject level_up_display;

	private PerkScreenHex unlock_animation_hex;

	private int anm_length_ms;

	public GameObject cancel_button;

	private cancel_context_t cancel_context;

	public TextMeshProUGUI header_text;

	public GameObject spend_now_tab;

	public GameObject no_points_tab_;

	public GameObject points_to_spend_tab;

	public GameObject equipped_perks_tab;

	public TextMeshProUGUI text_num_points_to_spend;

	public TextMeshProUGUI text_num_points_to_spend_2;

	public AudioClip sfx_equip_perk;

	public GameObject more_into_button;

	public Animation equip_button_A;

	public Animation equip_button_B;

	public Text unlock_button_text;

	private List<PerkScreenHex> perk_type_hexes;

	public GameObject infobox_equip_buttons;

	public GameObject infobox_unlock_button;

	public Sprite spr_infobox_unknown_perk;

	public Text infobox_title;

	public Image infobox_img;

	public Text infobox_desc;

	public Text infobox_level;

	public GameObject infobox_three_question_marks;

	public static float hex_w;

	public static float hex_h;

	public Dictionary<string, PerkScreenHex> hex_points;

	public GameObject prefab_outline;

	public GameObject prefab_perk_hex;

	public GameObject prefab_text;

	public GameObject prefab_line;

	private Vector3 drag_start;

	public Sprite dna_strand_unlocked;

	public Sprite dna_strand_locked;

	public AudioClip sfx_unlock;

	public AudioClip sfx_charge;

	public void Init()
	{
	}

	private void RedrawPointsText()
	{
	}

	public void PressSpendPoints()
	{
	}

	public void PerkUnlockAnimationExpanded()
	{
	}

	public void PerkUnlockAnimationComplete()
	{
	}

	public void PressedUnlock()
	{
	}

	private void StartFocusAnimation(Transform centering_obj, int anm_time)
	{
	}

	private void SetHeader(header_type_t header_type)
	{
	}

	public void PressCancelSpendPerks()
	{
	}

	private void ShowCancel(cancel_context_t new_context, bool animated)
	{
	}

	private void HideCancel()
	{
	}

	public void RedrawEquipSlots()
	{
	}

	public PerkScreenHex FindHexByObj(GameObject obj)
	{
		return null;
	}

	public PerkScreenHex FindHexByDevLookup(int search_x, int search_y, int search_offset)
	{
		return null;
	}

	public void PressEquipSlotA()
	{
	}

	public void PressEquipSlotB()
	{
	}

	public void PressMoreInfo()
	{
	}

	public void ClickPerkNib(GameObject caller)
	{
	}

	public void TryClickNib(int index, Action<int> on_succeed_click)
	{
	}

	private void FixedUpdate()
	{
	}

	private void Update()
	{
	}

	private void HideInfoBar(bool shrink_selected = true)
	{
	}

	private void ClampScreen()
	{
	}
}
