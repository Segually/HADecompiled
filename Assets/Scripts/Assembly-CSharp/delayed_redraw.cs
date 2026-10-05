using System.Collections.Generic;
using UnityEngine;

internal class delayed_redraw
{
	public enum process_status
	{
		more_to_process = 0,
		complete = 1,
		no_longer_exists = 2
	}

	public ModularObjectControl.type redraw_type;

	public string zone;

	public int chunkX;

	public int chunkZ;

	public ModularObjectControl.segment segment_to_redraw;

	public string chunkStr;

	private string suffix;

	private List<Vector2> squares_to_process;

	private Dictionary<string, PartiallyGeneratedModularModel> partially_generated_models;

	public process_status status;

	public delayed_redraw(ModularObjectControl.type redraw_type, string zone, int chunkX, int chunkZ, ModularObjectControl.segment segment_to_redraw)
	{
	}

	public int Process(int remaining)
	{
		return 0;
	}

	private void ProcessOne(Vector3 V)
	{
	}

	public void FinalizeAll()
	{
	}

	public void ProcessPaintings()
	{
	}

	private List<Vector2> GetVecList(ModularObjectControl.segment seg)
	{
		return null;
	}

	private char IsFilledWithSimilarObject(string zone, int diffZ, int diffX, int chunkX, int chunkZ, int innerX, int innerZ, string curr_item)
	{
		return '\0';
	}
}
