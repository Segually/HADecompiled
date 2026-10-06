using UnityEngine;

public class ScreenshotWithEffects : MonoBehaviour, OrderedStart
{
	public static ScreenshotWithEffects Instance;

	public float outlineThickness = 1f;

	public Color outlineColor = Color.black;

	public float glowOpacity = 1f;

	public float glowSize = 1f;

	public float outlineAlphaThreshold = 0.9f;

	public Material outlineGlowMaterial;

	public RenderTexture shared_base_rt;

	public RenderTexture shared_particles_rt;

	public RenderTexture shared_final_rt;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		shared_base_rt = new RenderTexture(128, 128, 24, RenderTextureFormat.ARGB32);
		shared_particles_rt = new RenderTexture(128, 128, 24, RenderTextureFormat.ARGB32);
		shared_final_rt = new RenderTexture(128, 128, 24, RenderTextureFormat.ARGB32);
	}

	private void ClearRenderTexture(RenderTexture rt)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = rt;
		GL.Clear(true, true, Color.clear);
		RenderTexture.active = active;
	}

	public void CaptureBaseScreenshot()
	{
		Camera item_screenshots_cam = ItemScreenshotTaker.Instance.item_screenshots_cam;
		item_screenshots_cam.clearFlags = CameraClearFlags.SolidColor;
		item_screenshots_cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
		item_screenshots_cam.targetTexture = shared_base_rt;
		item_screenshots_cam.Render();
		item_screenshots_cam.targetTexture = null;
	}

	public void CaptureParticlesScreenshot()
	{
		Camera item_screenshots_cam = ItemScreenshotTaker.Instance.item_screenshots_cam;
		item_screenshots_cam.clearFlags = CameraClearFlags.SolidColor;
		item_screenshots_cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
		item_screenshots_cam.targetTexture = shared_particles_rt;
		item_screenshots_cam.Render();
		item_screenshots_cam.targetTexture = null;
	}

	public void ApplyGlowOutlineWithoutParticles(Color glowColor)
	{
		outlineGlowMaterial.SetFloat("_OutlineThickness", outlineThickness);
		outlineGlowMaterial.SetColor("_OutlineColor", outlineColor);
		outlineGlowMaterial.SetColor("_GlowColor", glowColor);
		outlineGlowMaterial.SetFloat("_GlowOpacity", glowOpacity);
		outlineGlowMaterial.SetFloat("_GlowSize", glowSize);
		outlineGlowMaterial.SetFloat("_OutlineAlphaThreshold", outlineAlphaThreshold);
		outlineGlowMaterial.DisableKeyword("USE_OVERLAY");
		outlineGlowMaterial.SetTexture("_OverlayTex", null);
		ClearRenderTexture(shared_final_rt);
		Graphics.Blit(shared_base_rt, shared_final_rt, outlineGlowMaterial);
	}

	public void ApplyGlowOutlineWithParticles(Color glowColor)
	{
		outlineGlowMaterial.SetFloat("_OutlineThickness", outlineThickness);
		outlineGlowMaterial.SetColor("_OutlineColor", outlineColor);
		outlineGlowMaterial.SetColor("_GlowColor", glowColor);
		outlineGlowMaterial.SetFloat("_GlowOpacity", glowOpacity);
		outlineGlowMaterial.SetFloat("_GlowSize", glowSize);
		outlineGlowMaterial.SetFloat("_OutlineAlphaThreshold", outlineAlphaThreshold);
		outlineGlowMaterial.EnableKeyword("USE_OVERLAY");
		outlineGlowMaterial.SetTexture("_OverlayTex", shared_particles_rt);
		ClearRenderTexture(shared_final_rt);
		Graphics.Blit(shared_base_rt, shared_final_rt, outlineGlowMaterial);
	}

	public Texture2D FinalizeTexture()
	{
		Texture2D texture2D = new Texture2D(shared_final_rt.width, shared_final_rt.height, TextureFormat.ARGB32, false);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = shared_final_rt;
		texture2D.ReadPixels(new Rect(0f, 0f, shared_final_rt.width, shared_final_rt.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = active;
		return texture2D;
	}

	private void OnDestroy()
	{
		shared_base_rt.Release();
		shared_particles_rt.Release();
		shared_final_rt.Release();
	}
}
