using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimationFunctionsBreeder : MonoBehaviour, OrderedStart
{
	public static AnimationFunctionsBreeder Instance;

	public bool ignore;

	public GameObject obj_crab;

	public GameObject obj_plus;

	public GameObject obj_kang;

	public GameObject obj_equals;

	public AnimationCurve overshoot;

	private Vector2 crab_start;

	private Vector2 plus_start;

	private Vector2 kang_start;

	private Vector2 equals_start;

	private Vector2 crab_end;

	private Vector2 plus_end;

	private Vector2 kang_end;

	private Vector2 equals_end;

	private float spacing = 40f;

	private bool is_animating;

	private Stopwatch animation_stopwatch = new Stopwatch();

	public float white_extra_width;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		obj_crab.gameObject.SetActive(true);
		obj_crab.GetComponent<CanvasGroup>().alpha = 0f;
		obj_plus.gameObject.SetActive(true);
		obj_plus.GetComponent<CanvasGroup>().alpha = 0f;
		obj_kang.gameObject.SetActive(true);
		obj_kang.GetComponent<CanvasGroup>().alpha = 0f;
		obj_equals.gameObject.SetActive(true);
		obj_equals.GetComponent<CanvasGroup>().alpha = 0f;
	}

	private void Update()
	{
		if (is_animating)
		{
			float num = (float)animation_stopwatch.ElapsedMilliseconds / 250f;
			if (num >= 1f)
			{
				is_animating = false;
				return;
			}
			obj_crab.transform.localPosition = Vector3.LerpUnclamped(crab_start, crab_end, overshoot.Evaluate(num));
			obj_plus.transform.localPosition = Vector3.LerpUnclamped(plus_start, plus_end, overshoot.Evaluate(num));
			obj_kang.transform.localPosition = Vector3.LerpUnclamped(kang_start, kang_end, overshoot.Evaluate(num));
			obj_equals.transform.localPosition = Vector3.LerpUnclamped(equals_start, equals_end, overshoot.Evaluate(num));
		}
	}

	public void ShowFirstText()
	{
		crab_end = Vector2.zero;
		animation_stopwatch.Restart();
		is_animating = true;
		obj_crab.GetComponent<Animation>().Play("show_dad_mom_text");
		crab_start = crab_end;
		obj_crab.transform.localPosition = crab_end;
	}

	public void PrepPlus()
	{
		TextMeshProUGUI component = obj_crab.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI component2 = obj_plus.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		float num = (component.bounds.size.y + spacing + component2.bounds.size.y) * 0.5f;
		num -= component.bounds.size.y * 0.5f;
		crab_end = new Vector2(0f, num);
		num = num - component.bounds.size.y * 0.5f - spacing;
		plus_end = new Vector2(0f, num - component2.bounds.size.y * 0.5f);
		crab_start = obj_crab.transform.localPosition;
		animation_stopwatch.Restart();
		is_animating = true;
	}

	public void ShowPlus()
	{
		obj_plus.GetComponent<Animation>().Play("show_dad_mom_text");
		plus_start = plus_end;
		obj_plus.transform.localPosition = plus_end;
	}

	public void PrepSecond()
	{
		TextMeshProUGUI component = obj_crab.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI component2 = obj_plus.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI component3 = obj_kang.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		float num = (component.bounds.size.y + spacing + component2.bounds.size.y + spacing + component3.bounds.size.y) * 0.5f;
		num -= component.bounds.size.y * 0.5f;
		crab_end = new Vector2(0f, num);
		num = num - component.bounds.size.y * 0.5f - spacing;
		num -= component2.bounds.size.y * 0.5f;
		plus_end = new Vector2(0f, num);
		num = num - component2.bounds.size.y * 0.5f - spacing;
		kang_end = new Vector2(0f, num - component3.bounds.size.y * 0.5f);
		crab_start = obj_crab.transform.localPosition;
		plus_start = obj_plus.transform.localPosition;
		animation_stopwatch.Restart();
		is_animating = true;
	}

	public void ShowSecondText()
	{
		obj_kang.GetComponent<Animation>().Play("show_dad_mom_text");
		kang_start = kang_end;
		obj_kang.transform.localPosition = kang_end;
	}

	public void PrepEquals()
	{
		TextMeshProUGUI component = obj_crab.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI component2 = obj_plus.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI component3 = obj_kang.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI component4 = obj_equals.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		float num = (component.bounds.size.y + spacing + component2.bounds.size.y + spacing + component3.bounds.size.y + spacing + component4.bounds.size.y) * 0.5f;
		num -= component.bounds.size.y * 0.5f;
		crab_end = new Vector2(0f, num);
		num = num - component.bounds.size.y * 0.5f - spacing;
		num -= component2.bounds.size.y * 0.5f;
		plus_end = new Vector2(0f, num);
		num = num - component2.bounds.size.y * 0.5f - spacing;
		num -= component3.bounds.size.y * 0.5f;
		kang_end = new Vector2(0f, num);
		num = num - component3.bounds.size.y * 0.5f - spacing;
		equals_end = new Vector2(0f, num - component4.bounds.size.y * 0.5f);
		crab_start = obj_crab.transform.localPosition;
		plus_start = obj_plus.transform.localPosition;
		kang_start = obj_kang.transform.localPosition;
		animation_stopwatch.Restart();
		is_animating = true;
	}

	public void ShowEquals()
	{
		obj_equals.GetComponent<Animation>().Play("show_dad_mom_text");
		equals_start = equals_end;
		obj_equals.transform.localPosition = equals_end;
	}

	public void HideAllTexts()
	{
		obj_crab.GetComponent<Animation>().Play("hide_dad_mom_text");
		obj_plus.GetComponent<Animation>().Play("hide_dad_mom_text");
		obj_kang.GetComponent<Animation>().Play("hide_dad_mom_text");
		obj_equals.GetComponent<Animation>().Play("hide_dad_mom_text");
	}

	public void OnTextWasSet()
	{
		StartCoroutine(DelayedPositionElements());
	}

	private IEnumerator DelayedPositionElements()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		TextMeshProUGUI component = obj_crab.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		Image component2 = obj_crab.transform.Find("white").GetComponent<Image>();
		TextMeshProUGUI component3 = obj_plus.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		Image component4 = obj_plus.transform.Find("white").GetComponent<Image>();
		TextMeshProUGUI component5 = obj_kang.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		Image component6 = obj_kang.transform.Find("white").GetComponent<Image>();
		TextMeshProUGUI component7 = obj_equals.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		Image component8 = obj_equals.transform.Find("white").GetComponent<Image>();
		component.transform.localPosition = -component.bounds.center;
		component2.rectTransform.sizeDelta = new Vector2(component.bounds.size.x + white_extra_width * 2f, component.bounds.size.y + white_extra_width * 2f);
		component3.transform.localPosition = -component3.bounds.center;
		component4.rectTransform.sizeDelta = new Vector2(component3.bounds.size.x + white_extra_width * 2f, component3.bounds.size.y + white_extra_width * 2f);
		component5.transform.localPosition = -component5.bounds.center;
		component6.rectTransform.sizeDelta = new Vector2(component5.bounds.size.x + white_extra_width * 2f, component5.bounds.size.y + white_extra_width * 2f);
		component7.transform.localPosition = -component7.bounds.center;
		component8.rectTransform.sizeDelta = new Vector2(component7.bounds.size.x + white_extra_width * 2f, component7.bounds.size.y + white_extra_width * 2f);
	}

	public void SoundMother()
	{
		AudioControl.Instance.Play(AudioControl.Instance.sfx_mother);
	}

	public void SoundFather()
	{
		AudioControl.Instance.Play(AudioControl.Instance.sfx_father);
	}

	public void MutantsMerge()
	{
		BreedControl.Instance.AnimatedMergeMutants();
	}

	public void MutantsComplete()
	{
		BreedControl.Instance.OnMutateComplete();
	}

	public void ViewFirst()
	{
		BreedControl.Instance.AnimatedViewFirstCreature();
	}

	public void ViewSecond()
	{
		BreedControl.Instance.AnimatedViewSecondCreature();
	}

	public void ViewResult()
	{
		BreedControl.Instance.AnimatedViewResult();
	}

	public void ResultEmerge()
	{
		BreedControl.Instance.AnimatedResultCreatureEmergeFromElevator();
	}
}
