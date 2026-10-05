using System.Collections.Generic;

public class QuestBuildableEntry
{
	public string main_str;

	public string item_name;

	public string overlap_check_str;

	public List<string> extra_data;

	public QuestBuildableEntry(string main_str)
	{
	}

	public int FindExtraDataIndex(string search_str)
	{
		return 0;
	}
}
