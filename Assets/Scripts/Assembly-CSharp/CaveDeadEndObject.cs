public class CaveDeadEndObject
{
	public int x;

	public int z;

	public int rot;

	public ChunkElement element;

	public CaveDeadEndObject(string buildable_id, int x, int z, int rot)
	{
		this.x = x;
		this.z = z;
		this.rot = rot;
		element = new ChunkElement(buildable_id, rot);
	}
}
