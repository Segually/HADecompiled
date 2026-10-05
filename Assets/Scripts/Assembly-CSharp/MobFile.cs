using System.Collections.Generic;

public class MobFile
{
	public Dictionary<string, string> Entries { get; set; } = new Dictionary<string, string>();

	public List<string> Drops { get; set; } = new List<string>();
}
