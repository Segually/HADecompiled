using System.Collections.Generic;
using UnityEngine;

public class PoolGameRecording
{
	public bool is_playing;

	public bool[] stop_recording_ball;

	public Vector3[] ball_positions;

	public List<List<object>> frames;

	public void PopFrame()
	{
	}

	public byte[] pack_for_web()
	{
		return null;
	}

	public void unpack_from_web(byte[] bytes)
	{
	}
}
