using UnityEngine;

public class ScreenshotWithEffects : MonoBehaviour, OrderedStart
{
	public static ScreenshotWithEffects Instance;

	public float outlineThickness;

	public Color outlineColor;

	public float glowOpacity;

	public float glowSize;

	public float outlineAlphaThreshold;

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
	}

	private void ClearRenderTexture(RenderTexture rt)
	{
	}

	public void CaptureBaseScreenshot()
	{
	}

	public void CaptureParticlesScreenshot()
	{
	}

	public void ApplyGlowOutlineWithoutParticles(Color glowColor)
	{
	}

	public void ApplyGlowOutlineWithParticles(Color glowColor)
	{
	}

	public Texture2D FinalizeTexture()
	{
		return null;
	}

	private void OnDestroy()
	{
	}
}
