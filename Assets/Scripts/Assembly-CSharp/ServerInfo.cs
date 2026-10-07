public class ServerInfo
{
	public string server_name;

	public int n_online;

	public string server_description1;

	public string server_description2;

	public string server_description3;

	public string server_description4;

	public int max_players;

	public string server_game_mode;

	public ServerInfo(string server_name, string server_description1, string server_description2, string server_description3, string server_description4, int n_online, int max_players, string server_game_mode)
	{
		this.server_name = server_name;
		this.server_description1 = server_description1;
		this.server_description2 = server_description2;
		this.server_description3 = server_description3;
		this.server_description4 = server_description4;
		this.n_online = n_online;
		this.max_players = max_players;
		this.server_game_mode = server_game_mode;
	}
}
