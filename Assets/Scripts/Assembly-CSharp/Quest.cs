using System.Collections.Generic;

public class Quest
{
	public string display_name = "???";

	public string complete_text = "???";

	public int repeat_hours = -1;

	public List<ItemCountPair> reward_items = new List<ItemCountPair>();

	public List<int> reward_chests = new List<int>();

	public List<int> reward_superChests = new List<int>();

	public List<Dictionary<string, string>> steps = new List<Dictionary<string, string>>();
}
