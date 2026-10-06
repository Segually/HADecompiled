using System.Collections.Generic;

public class QuestBuildableEntry
{
	public string main_str;

	public string item_name;

	public string overlap_check_str;

	public List<string> extra_data = new List<string>();

	public QuestBuildableEntry(string main_str)
	{
		this.main_str = main_str;
		int opening = main_str.IndexOf('(');
		overlap_check_str = main_str.Substring(0, opening - 1);
		int equals = main_str.IndexOf('=');
		item_name = main_str.Substring(equals + 2, opening - 1 - equals - 2);
	}

	public int FindExtraDataIndex(string search_str)
	{
		for (int i = 0; i < extra_data.Count; i++)
			if (extra_data[i].Contains(search_str)) return i;
		return -1;
	}
}
