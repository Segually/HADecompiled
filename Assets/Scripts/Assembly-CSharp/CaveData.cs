using System.Collections.Generic;

public class CaveData
{
	public List<UngeneratedCaveChunk> ungenerated_floors;

	public UngeneratedCaveChunk GetUngeneratedFloorAt(int X, int Z)
	{
		return null;
	}

	public void SaveToDisk(string zone)
	{
	}

	public static CaveData LoadFromDisk(string zone)
	{
		return null;
	}
}
