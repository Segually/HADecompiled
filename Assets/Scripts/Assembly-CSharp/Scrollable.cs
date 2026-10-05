using System;
using UnityEngine;

public class Scrollable : MonoBehaviour
{
	public GameObject scrollwheel_nib;

	public GameObject scrollwheel_parent;

	private float scroll_area_max_y;

	public float scroll_nib_max_height;

	private Vector2 scrollwheel_parentStartPos;

	private float scrollwheel_releaseButtonTimeRemain;

	private float scrollwheel_velocity;

	private Vector3 scrollwheel_prevMouse;

	private int attempt_click_index;

	private Action<int> on_succeed_click;

	private void Start()
	{
	}

	public void SetScrollAreaMaxY(float scroll_area_max_y)
	{
	}

	public void TryClickNib(int index, Action<int> on_succeed_click)
	{
	}

	private void Update()
	{
	}

	private void FixedUpdate()
	{
	}

	private void ClampButtonParent(GameObject to_clamp)
	{
	}
}
