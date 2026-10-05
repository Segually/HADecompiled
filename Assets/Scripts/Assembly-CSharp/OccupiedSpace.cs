using UnityEngine;

public class OccupiedSpace
{
	public Vector3 pos;

	public Vector3 origin;

	public string layer;

	public ChunkElement element;

	public OccupiedSpace(Vector3 pos, Vector3 origin, string layer, ChunkElement element)
	{
		this.pos = pos;
		this.origin = origin;
		this.layer = layer;
		this.element = element;
	}
}
