using UnityEngine;

public class SpriteRandomizer : MonoBehaviour
{
	public Texture2D[] possible_textures;

	private void Start()
	{
		GetComponent<MeshRenderer>().material.mainTexture = possible_textures[UnityEngine.Random.Range(0, possible_textures.Length)];
		UnityEngine.Object.Destroy(this);
	}
}
