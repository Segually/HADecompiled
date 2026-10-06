internal class OldPerk
{
	public int max_level;

	public string prerequisite_perk;

	public int prerequisite_level;

	public int curr_level;

	public OldPerk(int max_level, string prerequisite_perk = "", int prerequisite_level = -1)
	{
		this.prerequisite_perk = prerequisite_perk;
		this.max_level = max_level;
		this.prerequisite_level = prerequisite_level;
	}
}
