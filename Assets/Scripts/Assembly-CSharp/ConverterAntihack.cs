using System.Collections;
using UnityEngine;

public class ConverterAntihack : MonoBehaviour
{
	public bool complete;

	public IEnumerator BeginConverting()
	{
		GrandConverter.Instance.DrawProgressBar(0.01f);
		GrandConverter.Instance.filename_text.text = "";
		yield return new WaitForEndOfFrame();
		complete = true;
	}
}
