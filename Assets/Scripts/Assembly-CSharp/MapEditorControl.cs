using UnityEngine;
using UnityEngine.InputSystem;

public class MapEditorControl : MonoBehaviour, OrderedStart
{
	public static MapEditorControl Instance;

	public bool editing_map;

	public GameObject temp_player;

	public Vector3 editor_start_position = Vector3.zero;

	private Vector3 move_direction;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
	}

	public void OnEnterGame()
	{
		temp_player = new GameObject("TempPlayer");
		temp_player.transform.position = editor_start_position;
		ChunkControl.Instance.OverrideFollowObject(temp_player);
	}

	public void Update()
	{
		if (editing_map)
		{
			move_direction = Vector3.zero;
			if (GamepadInput.Instance.GetKey(Key.W))
			{
				move_direction += new Vector3(-1f, 0f, 1f);
			}
			if (GamepadInput.Instance.GetKey(Key.A))
			{
				move_direction += new Vector3(-1f, 0f, -1f);
			}
			if (GamepadInput.Instance.GetKey(Key.S))
			{
				move_direction += new Vector3(1f, 0f, -1f);
			}
			if (GamepadInput.Instance.GetKey(Key.D))
			{
				move_direction += new Vector3(1f, 0f, 1f);
			}
		}
	}

	private void FixedUpdate()
	{
		if (move_direction != Vector3.zero)
		{
			Transform transform = temp_player.transform;
			transform.position = transform.position + move_direction.normalized * 0.15f;
		}
	}
}
