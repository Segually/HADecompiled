using System.Collections.Generic;
using UnityEngine;

public class AnimationFunctionsNewPerkScreen : MonoBehaviour
{
	public GameObject gem_prefab;

	private List<GameObject> gems = new List<GameObject>();

	private int n_gems = 6;

	public float gem_dist = 100f;

	public float gem_scale = 1f;

	public float anm_degrees;

	public float white_amount;

	public void SfxResolve()
	{
		PerkControl.Instance.sfx_source.volume = AudioControl.Instance.general_sfx_volume;
		PerkControl.Instance.sfx_source.PlayOneShot(PerkControl.Instance.resolve, AudioControl.Instance.general_sfx_volume);
	}

	public void AnimationComplete()
	{
		GameController.Instance.perk_get_animation_playing = false;
		WindowPrefabsControl.Instance.CreateScreen("NewPerkGet - bottom", WindowPrefabsControl.build_into_t.GAME_CTR);
		WindowPrefabsControl.Instance.GetScreen("NewPerkGet - bottom").GetComponent<CanvasGroup>().alpha = 0f;
		PerkControl.Instance.genomes++;
		PerkControl.Instance.SaveGenomes();
	}

	public void Initialize(Color col)
	{
		gem_prefab.SetActive(false);
		for (int i = 0; i < n_gems; i++)
		{
			GameObject gameObject = Object.Instantiate(gem_prefab);
			gameObject.transform.SetParent(gem_prefab.transform.parent);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = gem_scale * Vector3.one;
			gameObject.SetActive(true);
			gameObject.transform.Find("Image").GetComponent<UnityEngine.UI.Image>().color = col;
			gems.Add(gameObject);
		}
	}

	private void Update()
	{
		for (int i = 0; i < n_gems; i++)
		{
			float f = 360f / (float)n_gems * (float)i * ((float)System.Math.PI / 180f) + anm_degrees * ((float)System.Math.PI / 180f);
			gems[i].transform.localPosition = new Vector3(Mathf.Sin(f), Mathf.Cos(f), 0f) * gem_dist;
			gems[i].transform.localScale = gem_scale * Vector3.one;
			gems[i].transform.Find("overlay").GetComponent<CanvasGroup>().alpha = white_amount;
		}
	}
}
