internal class ToolUseResult
{
	public enum status
	{
		success = 0,
		error_land_claimed = 1,
		error_someone_using = 2,
		error_dev_obj = 3,
		error_need_tool = 4,
		error_not_editable_at_all = 5,
		error_bandit_camp = 6,
		error_unknown = 7
	}

	public status status_;

	public string extra_data;

	public ToolUseResult(status status_, string extra_data = "")
	{
	}
}
