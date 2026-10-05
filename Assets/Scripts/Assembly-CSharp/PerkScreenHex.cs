using System.Collections.Generic;
using UnityEngine;

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

	public Dictionary<string, string> extra_values;

	public GameObject obj;

	public List<GameObject> corresponding_line_objects;

	public Vector3 local_position => default(Vector3);

	public PerkScreenHex(hex_type type, Dictionary<string, string> extra_values, int hex_dev_x, int hex_dev_y, int hex_dev_offset)
	{
	}

	public void Redraw(bool hide_rings = false, bool show_unlock_animation_A = false, bool show_unlock_animation_B = false, bool post_unlock = false)
	{
	}

	private void RedrawMainObj(bool hide_rings, bool show_unlock_animation_A, bool show_unlock_animation_B, bool post_unlock)
	{
	}

	private void RedrawLineSegments()
	{
	}

	private void RedrawLineSegment(int point_id, string dir_str, float corresponding_rotation)
	{
	}
}
