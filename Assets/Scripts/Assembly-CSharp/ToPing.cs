public class ToPing
{
	public string dispatcher_name;

	public string dispatcher_ip;

	public int dispatcher_port;

	public ToPing(string dispatcher_name, string dispatcher_ip, int dispatcher_port)
	{
		this.dispatcher_name = dispatcher_name;
		this.dispatcher_ip = dispatcher_ip;
		this.dispatcher_port = dispatcher_port;
	}
}
