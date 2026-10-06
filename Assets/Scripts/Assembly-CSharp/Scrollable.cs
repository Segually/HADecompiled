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

	private int attempt_click_index = -1;

	private Action<int> on_succeed_click;

	private void Start()
	{
		scrollwheel_prevMouse = GamepadInput.Instance.GetMousePosition();
	}

	public void SetScrollAreaMaxY(float scroll_area_max_y)
	{
		this.scroll_area_max_y = scroll_area_max_y;
	}

	public void TryClickNib(int index, Action<int> on_succeed_click)
	{
		this.on_succeed_click = on_succeed_click;
		attempt_click_index = index;
		scrollwheel_releaseButtonTimeRemain = 13f;
	}

	private void Update()
	{
		if (scrollwheel_releaseButtonTimeRemain > 0f && GamepadInput.Instance.GetMouseButtonUp())
		{
			on_succeed_click(attempt_click_index);
			attempt_click_index = -1;
			scrollwheel_releaseButtonTimeRemain = 0f;
		}
		Vector3 mousePosition = GamepadInput.Instance.GetMousePosition();
		if (GamepadInput.Instance.GetMouseButtonDown())
		{
			scrollwheel_prevMouse = mousePosition;
		}
		if (GamepadInput.Instance.GetMouseButton())
		{
			float num = (mousePosition.y - scrollwheel_prevMouse.y) / FriendServerInterface.Instance.canvas.scaleFactor / FriendServerInterface.Instance.scaled_parent.transform.localScale.x;
			scrollwheel_parent.transform.localPosition += new Vector3(0f, num, 0f);
			ClampButtonParent(scrollwheel_parent);
			scrollwheel_velocity = num;
		}
		scrollwheel_prevMouse = mousePosition;
	}

	private void FixedUpdate()
	{
		float t = Mathf.InverseLerp(scrollwheel_parentStartPos.y, scroll_area_max_y, scrollwheel_parent.transform.localPosition.y);
		scrollwheel_nib.transform.localPosition = new Vector3(0f, Mathf.Lerp(scroll_nib_max_height, 0f - scroll_nib_max_height, t), 0f);
		if (scrollwheel_releaseButtonTimeRemain > 0f)
		{
			scrollwheel_releaseButtonTimeRemain -= 1f;
		}
		if (!GamepadInput.Instance.GetMouseButton())
		{
			scrollwheel_parent.transform.localPosition += new Vector3(0f, scrollwheel_velocity, 0f);
			scrollwheel_velocity *= 0.94f;
			ClampButtonParent(scrollwheel_parent);
		}
	}

	private void ClampButtonParent(GameObject to_clamp)
	{
		if (to_clamp.transform.localPosition.y < scrollwheel_parentStartPos.y)
		{
			to_clamp.transform.localPosition = new Vector3(scrollwheel_parentStartPos.x, scrollwheel_parentStartPos.y, 0f);
		}
		else if (to_clamp.transform.localPosition.y > scroll_area_max_y)
		{
			to_clamp.transform.localPosition = new Vector3(scrollwheel_parentStartPos.x, scroll_area_max_y, 0f);
		}
	}
}
