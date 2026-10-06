using UnityEngine;

public class PositionRandomizer : MonoBehaviour
{
	public RectTransform[] objects_to_randomize;

	private void Start()
	{
		float x = ((RectTransform)base.transform.parent).sizeDelta.x;
		float y = ((RectTransform)base.transform.parent).sizeDelta.y;
		RectTransform[] array = objects_to_randomize;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].transform.localPosition = new Vector3(UnityEngine.Random.Range(0f - x, x) * 0.5f, UnityEngine.Random.Range(0f - y, y) * 0.5f, 0f);
		}
	}
}
