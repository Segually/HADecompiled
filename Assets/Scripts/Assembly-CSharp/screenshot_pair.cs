using UnityEngine;

public class screenshot_pair
{
	public InventoryItem item_unmodified_for_caching;

	public InventoryItem item_adjusted_for_rendering;

	public ItemSprite itemSprite;

	public GameObject obj;

	public bool obj_created;

	public string paint_str;

	public string stamp_str;

	public string paint_layout_item;

	public Color glowCol;

	public bool model_has_particles_by_default;

	public bool paint_has_particles_and_model_supports_them;

	public bool cancelled;
}
