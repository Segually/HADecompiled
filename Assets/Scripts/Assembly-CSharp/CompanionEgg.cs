using UnityEngine;

public class CompanionEgg : MonoBehaviour
{
	public GameObject shell;

	public ParticleSystem shell_shatter_A;

	public ParticleSystem shell_shatter_B;

	public void VisuallyCrackEgg()
	{
		shell.SetActive(false);
		shell_shatter_A.Emit(12);
		shell_shatter_B.Emit(5);
	}
}
