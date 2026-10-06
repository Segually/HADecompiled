using System.Collections.Generic;

public class QuestBuildableFile
{
	public string auto_version_entry = "";

	public List<string> header_entries = new List<string>();

	public List<QuestBuildableEntry> buildable_entries = new List<QuestBuildableEntry>();

	public QuestBuildableEntry FindBuildableEntry(string search_str)
	{
		foreach (QuestBuildableEntry entry in buildable_entries)
			if (entry.main_str.Contains(search_str)) return entry;
		return null;
	}
}
