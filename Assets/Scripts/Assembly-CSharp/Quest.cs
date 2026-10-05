using System.Collections.Generic;

public class Quest
{
	public string display_name;

	public string complete_text;

	public int repeat_hours;

	public List<ItemCountPair> reward_items;

	public List<int> reward_chests;

	public List<int> reward_superChests;

	public List<Dictionary<string, string>> steps;
}
