using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
	public InventoryItem corresponding_item;

	public float interaction_distance;

	public float circle_size;

	public int temp_rot;

	public bool is_house_exit;

	public Sprite icon_spr;

	public Sprite icon2_spr;

	public GameObject overhead_icon;

	public GameObject redirect_to;

	public GameObject replace_for;

	public LiteModel overhead_model_snap;

	public string item_name;

	public string active_obj_str;

	public string origin_zone;

	public int origin_chunkX;

	public int origin_chunkZ;

	public int origin_innerX;

	public int origin_innerZ;

	public bool deleted;

	public void InitExit()
	{
	}

	public void InitRedirect()
	{
		active_obj_str = "redirect" + ShopControl.RandomString();
		ChunkControl.Instance.active_interactibles.Add(active_obj_str, base.gameObject);
	}

	private void FixedUpdate()
	{
		if (overhead_icon != null)
		{
			if (overhead_model_snap == null)
			{
				MobControl.Instance.SnapOverhead((RectTransform)overhead_icon.transform, base.transform.position);
			}
			else
			{
				MobControl.Instance.SnapOverhead((RectTransform)overhead_icon.transform, overhead_model_snap.GetLimbWorldPosition(0) + Vector3.up * 0.75f);
			}
		}
	}

	public void AssignOverheadIcon(Sprite set_icon = null, Sprite set_icon_2 = null)
	{
		if (!(overhead_icon != null))
		{
			if (icon_spr == null)
			{
				icon_spr = set_icon;
			}
			if (icon2_spr == null)
			{
				icon2_spr = set_icon_2;
			}
			RedrawOverheadIcon();
		}
	}

	public void RedrawOverheadIcon()
	{
		if (overhead_icon != null || !WindowControl.Instance.ShouldRecreateOverheads())
		{
			return;
		}
		overhead_icon = Object.Instantiate(GameController.Instance.type_NPC_overhead);
		overhead_icon.transform.SetParent(MobControl.Instance.gameObject.transform);
		overhead_icon.transform.SetAsFirstSibling();
		overhead_icon.transform.localPosition = Vector3.zero;
		overhead_icon.transform.localScale = Vector3.one * 0.75f;
		overhead_icon.transform.localRotation = Quaternion.identity;
		Transform transform = overhead_icon.transform;
		Vector2 vector = Vector2.one * 10f;
		((RectTransform)overhead_icon.transform).anchorMax = vector;
		((RectTransform)transform).anchorMin = vector;
		GameController.Instance.possible_destroy.Add(overhead_icon);
		bool flag = icon2_spr == null;
		Transform transform2 = overhead_icon.transform.Find("StateDisplay");
		if (flag)
		{
			transform2.GetComponent<Image>().sprite = icon_spr;
			return;
		}
		transform2.gameObject.SetActive(false);
		overhead_icon.transform.Find("StateDisplay2-1").gameObject.SetActive(true);
		overhead_icon.transform.Find("StateDisplay2-1").GetComponent<Image>().sprite = icon_spr;
		overhead_icon.transform.Find("StateDisplay2-2").gameObject.SetActive(true);
		overhead_icon.transform.Find("StateDisplay2-2").GetComponent<Image>().sprite = icon2_spr;
	}

	public void CustomStart(ChunkData chunk_data, int x, int z, string item_name)
	{
		active_obj_str = chunk_data.zone + "," + chunk_data.X + "," + chunk_data.Z + "," + x + "," + z;
		origin_zone = chunk_data.zone;
		origin_chunkX = chunk_data.X;
		origin_chunkZ = chunk_data.Z;
		origin_innerX = x;
		origin_innerZ = z;
		if (!ChunkControl.Instance.active_interactibles.ContainsKey(active_obj_str))
		{
			ChunkControl.Instance.active_interactibles.Add(active_obj_str, base.gameObject);
		}
		else
		{
			if (item_name == "Companion" || item_name == "DEBUG-npc")
			{
				GameObject gameObject = ChunkControl.Instance.active_interactibles[active_obj_str];
				if (gameObject != null)
				{
					Interactable component = gameObject.GetComponent<Interactable>();
					if (component != null)
					{
						string text = component.item_name;
						if (text == "Chair" || text == "Metal Chair" || text == "Park Bench" || text == "Stump Chair" || text == "Bed" || text == "Sofa Chair" || text == "Throne")
						{
							component.replace_for = base.gameObject;
						}
					}
				}
			}
			if (item_name == "Chair" || item_name == "Metal Chair" || item_name == "Park Bench" || item_name == "Stump Chair" || item_name == "Bed" || item_name == "Sofa Chair" || item_name == "Throne")
			{
				GameObject gameObject2 = ChunkControl.Instance.active_interactibles[active_obj_str];
				if (gameObject2 != null)
				{
					Interactable component2 = gameObject2.GetComponent<Interactable>();
					if (component2 != null)
					{
						string text2 = component2.item_name;
						if (text2 == "Companion" || text2 == "DEBUG-npc")
						{
							replace_for = gameObject2;
							ChunkControl.Instance.active_interactibles[active_obj_str] = base.gameObject;
						}
					}
				}
			}
		}
		this.item_name = item_name;
		if (!(item_name == "Music Box"))
		{
			return;
		}
		if (!MusicBoxControl.Instance.song_instances.ContainsKey(active_obj_str))
		{
			MusicBoxControl.song_struct song_struct;
			if (corresponding_item.GetShort("npc_record_index") == 0)
			{
				song_struct = MusicBoxControl.Instance.LoadCustomSongFromItem(corresponding_item, true);
			}
			else
			{
				song_struct = MusicBoxControl.Instance.LoadNPCSongFromItem(corresponding_item);
				song_struct.is_playing = true;
			}
			song_struct.pos = base.transform.position;
			MusicBoxControl.Instance.song_instances.Add(active_obj_str, song_struct);
		}
		else
		{
			MusicBoxControl.song_struct song_struct2 = MusicBoxControl.Instance.song_instances[active_obj_str];
			if (song_struct2.time_til_deload != -1f)
			{
				if (song_struct2.time_til_deload != 30f)
				{
					song_struct2.slice_selected_ = -1;
				}
				song_struct2.time_til_deload = -1f;
			}
		}
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(DelayedAutoPlayMusic());
		}
	}

	private IEnumerator DelayedAutoPlayMusic()
	{
		yield return new WaitForSeconds(0.02f);
		if (MusicBoxControl.Instance.song_instances.ContainsKey(active_obj_str) && MusicBoxControl.Instance.song_instances[active_obj_str].is_playing)
		{
			MusicBoxControl.Instance.AutoplaySong(active_obj_str, base.transform.position);
		}
	}

	public void Delete()
	{
		if (!deleted)
		{
			if (ChunkControl.Instance.active_interactibles.ContainsKey(active_obj_str))
			{
				ChunkControl.Instance.active_interactibles.Remove(active_obj_str);
			}
			if (overhead_icon != null)
			{
				GameController.Instance.possible_destroy.Remove(overhead_icon);
				Object.Destroy(overhead_icon);
			}
			if (item_name == "Music Box")
			{
				MusicBoxControl.Instance.box_deloaded(active_obj_str);
			}
			deleted = true;
		}
	}

	private void OnDestroy()
	{
		if (!deleted)
		{
			if (ChunkControl.Instance.active_interactibles.ContainsKey(active_obj_str))
			{
				ChunkControl.Instance.active_interactibles.Remove(active_obj_str);
			}
			if (overhead_icon != null)
			{
				GameController.Instance.possible_destroy.Remove(overhead_icon);
				Object.Destroy(overhead_icon);
			}
			if (item_name == "Music Box")
			{
				MusicBoxControl.Instance.box_deloaded(active_obj_str);
			}
			deleted = true;
		}
	}
}
