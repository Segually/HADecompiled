using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScreenshotTaker : MonoBehaviour, OrderedStart
{
	public static ItemScreenshotTaker Instance;

	public Camera item_screenshots_cam;

	public GameObject item_screenshots_light;

	public Dictionary<InventoryItem, Texture2D> cached_model3d_graphics;

	public List<screenshot_pair> screenshot_queue;

	public Color item_screenshots_ambientCol;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	private IEnumerator ProcessScreenshots()
	{
		return null;
	}

	public void QueueForScreenshot(InventoryItem item_unmodified, GameObject model3d_generated_graphic_, ItemSprite original_sprite)
	{
	}

	private void InstantiateObjectForScreenshot(screenshot_pair pair)
	{
	}

	private void OnObjectInstantiated(screenshot_pair pair)
	{
	}

	private void PositionCamera(screenshot_pair pair)
	{
	}

	private void TakeBaseScreenshot(screenshot_pair pair)
	{
	}

	private void TakeParticlesScreenshot(screenshot_pair pair)
	{
	}

	private Color AssignGlowCol(screenshot_pair pair)
	{
		return default(Color);
	}

	public void AddCachedGraphic(InventoryItem item, Texture2D tex)
	{
	}

	private void DetectParticles(screenshot_pair pair)
	{
	}

	private void ApplyOutlineGlowWithoutParticles(screenshot_pair pair)
	{
	}

	private void ApplyOutlineGlowWithParticles(screenshot_pair pair)
	{
	}

	private void Finalize(screenshot_pair pair)
	{
	}

	private void ShowLights(screenshot_pair pair, ref Color col_before)
	{
	}

	private void HideLights(screenshot_pair pair, ref Color col_before)
	{
	}

	public void CancelScreenshotByItemSprite(ItemSprite itemSprite)
	{
	}

	private bool HasTag(Transform T, string tag)
	{
		return false;
	}

	private void SetVisibility(GameObject G, bool state)
	{
	}

	private void SetVisibleByTag(Transform T, string tag, bool state)
	{
	}

	private void SetOtherVisibleByTag(Transform T, string tag, bool state, bool ignore_root)
	{
	}

	public bool ShouldCancel(screenshot_pair pair)
	{
		return false;
	}

	private Color RGBCol(int r, int g, int b)
	{
		return default(Color);
	}

	private Color ParseRGBString(string col_str)
	{
		return default(Color);
	}
}
