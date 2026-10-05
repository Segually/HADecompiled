using System;
using System.Collections.Generic;

public class ManyPingInProgress
{
	public List<string> waiting_on = new List<string>();

	public Dictionary<string, short> results = new Dictionary<string, short>();

	private Action<Dictionary<string, short>> on_complete;

	public ManyPingInProgress(Action<Dictionary<string, short>> on_complete)
	{
		this.on_complete = on_complete;
	}

	public void OnSingleComplete(string dispatcher_name, short dispatcher_RTT)
	{
		results.Add(dispatcher_name, dispatcher_RTT);
		waiting_on.Remove(dispatcher_name);
		TestIsComplete();
	}

	public void TestIsComplete()
	{
		if (waiting_on.Count == 0)
		{
			on_complete(results);
		}
	}
}
