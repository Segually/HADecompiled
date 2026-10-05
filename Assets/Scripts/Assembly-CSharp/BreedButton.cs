using UnityEngine;

public class BreedButton : MonoBehaviour
{
	public string creature;

	public void Click()
	{
		BreedControl.Instance.TryClickBreedButton(base.gameObject);
	}
}
