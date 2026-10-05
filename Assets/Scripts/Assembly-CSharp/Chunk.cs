using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
	public enum status_t
	{
		unknown = 0,
		pls_load_and_build = 1,
		MP_requested = 2,
		MP_got_data = 3,
		async_building_ = 4,
		async_building_buildablesComplete = 5,
		async_building_modularsComplete = 6,
		complete = 7
	}

	public bool is_deleted;

	public status_t status;

	public string zone;

	public int X;

	public int Z;

	public ChunkData chunk_data;

	public ChunkObj chunk_obj;

	public IEnumerator async_build;

	public List<string> mid_load_modulars = new List<string>();

	public bool all_modulars_accounted_for;

	public Chunk(status_t start_status, string zone, int X, int Z, bool quick_load)
	{
		this.zone = zone;
		status = start_status;
		this.X = X;
		this.Z = Z;
	}

	public void AsyncBuildAndAdd(MonoBehaviour caller)
	{
		ChunkControl.Instance.ChangeChunkStatus(ChunkControl.Instance.GetChunkString(zone, X, Z), status_t.async_building_);
		async_build = ChunkControl.Instance.AsyncBuildAndAddChunkCoroutine(this);
		if (caller == null)
		{
			Debug.Log("ERROR: AsyncBuildAndAdd() - 'caller' is null");
		}
		else if (caller.gameObject.activeInHierarchy)
		{
			caller.StartCoroutine(async_build);
		}
		else
		{
			Debug.Log("ERROR: AsyncBuildAndAdd() - 'caller' not active in scene");
		}
	}
}
