using TMPro;
using UnityEngine;

public class TradingTableControl : MonoBehaviour
{
	public static TradingTableControl Instance;

	public CanvasGroup alpha_accept_button;

	public CanvasGroup alpha_other_bg;

	public TextMeshProUGUI header_other;

	public GameObject waiting_overlay;

	public bool other_player_has_joined;

	public void OnOtherPlayerJoinedMe(string other_username)
	{
	}

	public void OnIJoinedOtherPlayer()
	{
	}
}
