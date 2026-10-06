public class UniqueIdStatus
{
	public enum status_
	{
		available = 0,
		used_by_shack = 1,
		used_by_basket = 2
	}

	public int id;

	public status_ status;

	public UniqueIdStatus(int id, status_ status)
	{
		this.id = id;
		this.status = status;
	}
}
