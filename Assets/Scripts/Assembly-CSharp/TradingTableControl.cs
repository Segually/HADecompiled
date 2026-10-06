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
		alpha_accept_button.alpha = 1f;
		alpha_other_bg.alpha = 1f;
		header_other.GetComponent<CanvasGroup>().alpha = 1f;
		waiting_overlay.SetActive(false);
		header_other.text = other_username + "'s <color=#ffffff>offer</color>";
	}

	public void OnIJoinedOtherPlayer()
	{
	}
}
