using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ResourceControl : MonoBehaviour, OrderedStart
{
	public static ResourceControl Instance;

	public Material mat_paint_advanced;

	public Material mat_paint_advanced_no_light;

	public Material mat_weapon_particle;

	public GameObject prefab_weapon_particle_system;

	public Texture2D white_tex;

	private Dictionary<string, AsyncOperationHandle<Sprite>> sprites_loaded_or_midLoad = new Dictionary<string, AsyncOperationHandle<Sprite>>();

	private Dictionary<Image, string> images_potentially_using_sprites = new Dictionary<Image, string>();

	private MaterialPropertyBlock biome_floor_mat_block;

	private Dictionary<string, AsyncOperationHandle<Texture2D>> textures_loaded_or_midLoad = new Dictionary<string, AsyncOperationHandle<Texture2D>>();

	private Dictionary<MeshRenderer, string> renderers_potentially_using_textures = new Dictionary<MeshRenderer, string>();

	private Dictionary<string, AsyncOperationHandle<AudioClip>> sfx_loaded_or_midLoad = new Dictionary<string, AsyncOperationHandle<AudioClip>>();

	private Dictionary<AudioSource, string> audiosources_potentially_using_sfx = new Dictionary<AudioSource, string>();

	private Dictionary<string, Texture2D> loaded_full_patterns = new Dictionary<string, Texture2D>();

	private Dictionary<string, Texture2D> loaded_side_patterns = new Dictionary<string, Texture2D>();

	private Dictionary<string, Texture2D> loaded_symbols = new Dictionary<string, Texture2D>();

	private Dictionary<string, Texture2D> loaded_particle_textures = new Dictionary<string, Texture2D>();

	private Dictionary<string, ColorScheme> loaded_color_schemes = new Dictionary<string, ColorScheme>();

	private Dictionary<string, AsyncOperationHandle<GameObject>> prefabs_loaded_or_midLoad = new Dictionary<string, AsyncOperationHandle<GameObject>>();

	private Dictionary<GameObject, string> instances_of_prefabs = new Dictionary<GameObject, string>();

	private Dictionary<string, Dictionary<string, string>> loaded_inventory_item_files = new Dictionary<string, Dictionary<string, string>>();

	private GameObject null_consumer;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
		if (this == Instance)
		{
			null_consumer = new GameObject("null consumer");
			UnityEngine.Object.DontDestroyOnLoad(null_consumer);
			StartCoroutine(DeloadResourcesPeriodically());
			biome_floor_mat_block = new MaterialPropertyBlock();
			white_tex = new Texture2D(4, 4);
			Color[] array = new Color[16];
			for (int i = 0; i < 16; i++)
			{
				array[i] = new Color(1f, 1f, 1f, 1f);
			}
			white_tex.SetPixels(array);
			white_tex.Apply();
		}
	}

	public string GetRandomListString(string list_name)
	{
		string result = "";
		List<string> wholeList = GetWholeList(list_name);
		if (wholeList.Count != 0)
		{
			return wholeList[UnityEngine.Random.Range(0, wholeList.Count)];
		}
		return result;
	}

	public List<string> GetTextFileLines(string file_name, ref bool file_exists)
	{
		if (ValidSynchronousPath(file_name))
		{
			if (!Application.isEditor)
			{
				AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>("Assets/SYNCHRONOUS/TextFiles/" + file_name + ".txt");
				TextAsset textAsset = handle.WaitForCompletion();
				if (textAsset != null)
				{
					List<string> result = new List<string>(Regex.Split(textAsset.text, "\n|\r|\r\n"));
					Addressables.Release(handle);
					file_exists = true;
					return result;
				}
			}
			else
			{
				string path = Path.Combine(Application.dataPath, "SYNCHRONOUS/TextFiles/" + file_name + ".txt");
				if (File.Exists(path))
				{
					List<string> collection = new List<string>(Regex.Split(File.ReadAllText(path), "\n|\r|\r\n"));
					file_exists = true;
					return new List<string>(collection);
				}
			}
			file_exists = false;
		}
		return new List<string>();
	}

	public byte[] GetBytesFileBytes(string file_name, ref bool file_exists)
	{
		if (ValidSynchronousPath(file_name))
		{
			AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>("Assets/SYNCHRONOUS/BytesFiles/" + file_name + ".bytes");
			TextAsset textAsset = handle.WaitForCompletion();
			if (textAsset != null)
			{
				byte[] bytes = textAsset.bytes;
				Addressables.Release(handle);
				file_exists = true;
				return bytes;
			}
			file_exists = false;
		}
		return new byte[0];
	}

	public Texture2D LoadImageSynchronously(string file_name)
	{
		if (!ValidSynchronousPath(file_name))
		{
			return null;
		}
		AsyncOperationHandle<Texture2D> handle = Addressables.LoadAssetAsync<Texture2D>("Assets/SYNCHRONOUS/Images/" + file_name + ".png");
		Texture2D texture2D = handle.WaitForCompletion();
		if (texture2D != null)
		{
			Addressables.Release(handle);
			return texture2D;
		}
		return null;
	}

	public GameObject LoadWindowSynchronously(string file_name)
	{
		if (!ValidSynchronousPath(file_name))
		{
			return null;
		}
		AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("Assets/SYNCHRONOUS/Windows/" + file_name + ".prefab");
		GameObject gameObject = handle.WaitForCompletion();
		if (gameObject != null)
		{
			Addressables.Release(handle);
			return gameObject;
		}
		return null;
	}

	private bool ValidSynchronousPath(string file_name)
	{
		if (Startup.StringNullOrWhitespace(file_name))
		{
			if (Application.isEditor)
			{
				Debug.Log("ERROR: empty file name");
			}
			return false;
		}
		if (file_name.IndexOfAny(Path.GetInvalidPathChars()) < 0)
		{
			return true;
		}
		if (Application.isEditor)
		{
			Debug.Log("ERROR Invalid Path Chars (" + file_name + ")");
		}
		return false;
	}

	public List<string> GetWholeList(string list_name)
	{
		List<string> list = new List<string>();
		bool file_exists = false;
		List<string> textFileLines = Instance.GetTextFileLines("Lists/" + list_name, ref file_exists);
		if (file_exists)
		{
			foreach (string item in textFileLines)
			{
				if (!Startup.StringNullOrWhitespace(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private IEnumerator DeloadResourcesPeriodically()
	{
		while (true)
		{
			yield return new WaitForSeconds(60f);
			yield return new WaitForSeconds(5f);
			GenericDeloadUnused(sprites_loaded_or_midLoad, images_potentially_using_sprites);
			yield return new WaitForSeconds(5f);
			GenericDeloadUnused(sfx_loaded_or_midLoad, audiosources_potentially_using_sfx);
			yield return new WaitForSeconds(5f);
			GenericDeloadUnused(textures_loaded_or_midLoad, renderers_potentially_using_textures);
			yield return new WaitForSeconds(5f);
			GenericDeloadUnused(prefabs_loaded_or_midLoad, instances_of_prefabs);
		}
	}

	public void AssignItemSprite(string file_name, Image img, Action on_load_complete = null)
	{
		img.enabled = false;
		if (file_name != "")
		{
			LoadAndAssignSprite("Assets/Images/InventorySprites/" + file_name + ".png", img, delegate // Keyed by .png to make it work in the editor (exported sprites are .png)
			// LoadAndAssignSprite("Assets/Images/InventorySprites/" + file_name + ".psd", img, delegate
			{
				img.enabled = true;
				if (on_load_complete != null)
				{
					on_load_complete();
				}
			});
		}
	}

	public void AssignPerkSprite(string perk_key, Image img)
	{
		img.sprite = PerkControl.Instance.perk_not_loaded;
		LoadAndAssignSprite("Assets/Images/PerkIcons/" + perk_key + ".png", img);
	}

	public void AssignCreatureSprite(string creature_name, Image img)
	{
		img.enabled = false;
		LoadAndAssignSprite("Assets/Images/CreatureButtonGraphics/" + creature_name + ".png", img, delegate
		{
			img.enabled = true;
		});
	}

	public void AssignGraphicsLevelScreenshot(string screenshot_name, Image img)
	{
		LoadAndAssignSprite("Assets/Images/GraphicsScreenshots/" + screenshot_name + ".png", img);
	}

	public void AssignKaraokeGif(string image_name, Image img)
	{
		LoadAndAssignSprite("Assets/Images/KaraokeGIFS/" + image_name + ".png", img);
	}

	public void AssignPainting(string image_name, Image img, Action on_asset_ready = null)
	{
		LoadAndAssignSprite("Assets/Images/Paintings/" + image_name + ".png", img, on_asset_ready);
	}

	private void LoadAndAssignSprite(string sprite_path, Image img, Action on_asset_ready = null)
	{
		GenericTryLoad(sprites_loaded_or_midLoad, images_potentially_using_sprites, sprite_path, img, delegate(Sprite loaded_sprite)
		{
			img.sprite = loaded_sprite;
			if (on_asset_ready != null)
			{
				on_asset_ready();
			}
		});
	}

	public void AssignCreatureDecorativeTexture(string tex_name, MeshRenderer renderer, bool disable_reenable = false, Action on_decorative_texture_loaded = null)
	{
		renderer.material.color = new Color(1f, 1f, 1f, 1f);
		LoadAndAssignTexture("Assets/Models/CreatureTextures/" + tex_name + ".png", renderer, disable_reenable, on_decorative_texture_loaded);
	}

	public void AssignCreatureLimbTexture(string tex_name, MeshRenderer renderer, Color limb_col, Action on_limb_texture_loaded = null)
	{
		renderer.material.color = limb_col;
		LoadAndAssignTexture("Assets/Models/CreatureTextures/" + tex_name + ".png", renderer, false, on_limb_texture_loaded);
	}

	public void AssignKaraokeGif(string image_name, MeshRenderer renderer)
	{
		LoadAndAssignTexture("Assets/Images/KaraokeGIFS/" + image_name + ".png", renderer);
	}

	public void AssignPainting(string image_name, MeshRenderer renderer, Action on_frame_loaded = null)
	{
		LoadAndAssignTexture("Assets/Images/Paintings/" + image_name + ".png", renderer, false, on_frame_loaded);
	}

	public void AssignCaveWallTexture(string tex_name, MeshRenderer renderer)
	{
		LoadAndAssignTexture("Assets/Models/CaveTextures/floors-and-walls/" + tex_name + ".png", renderer);
	}

	public void AssignCaveArtTexture(string tex_name, MeshRenderer renderer, Action on_complete)
	{
		LoadAndAssignTexture("Assets/Models/CaveTextures/cave-art/" + tex_name + ".png", renderer, false, on_complete);
	}

	public void AssignBiomeFloorTexture(string tex_name, MeshRenderer renderer, Action on_complete)
	{
		LoadAndAssignTextureWithMatBlock("Assets/Models/BiomeFloors/" + tex_name + ".png", renderer, biome_floor_mat_block, new Color(1f, 1f, 1f, 1f), false, on_complete);
	}

	private void LoadAndAssignTexture(string texture_path, MeshRenderer renderer, bool disable_reenable = false, Action on_texture_loaded = null)
	{
		if (disable_reenable)
		{
			renderer.enabled = false;
		}
		GenericTryLoad(textures_loaded_or_midLoad, renderers_potentially_using_textures, texture_path, renderer, delegate(Texture2D loaded_texture)
		{
			renderer.material.mainTexture = loaded_texture;
			if (disable_reenable)
			{
				renderer.enabled = true;
			}
			if (on_texture_loaded != null)
			{
				on_texture_loaded();
			}
		}, delegate
		{
			renderer.material.mainTexture = null;
			if (disable_reenable)
			{
				renderer.enabled = true;
			}
			if (on_texture_loaded != null)
			{
				on_texture_loaded();
			}
		});
	}

	private void LoadAndAssignTextureWithMatBlock(string texture_path, MeshRenderer renderer, MaterialPropertyBlock block, Color col, bool disable_reenable = false, Action on_texture_loaded = null)
	{
		if (disable_reenable)
		{
			renderer.enabled = false;
		}
		GenericTryLoad(textures_loaded_or_midLoad, renderers_potentially_using_textures, texture_path, renderer, delegate(Texture2D loaded_texture)
		{
			block.SetColor("_Color", col);
			block.SetTexture("_MainTex", loaded_texture);
			renderer.SetPropertyBlock(block);
			if (disable_reenable)
			{
				renderer.enabled = true;
			}
			if (on_texture_loaded != null)
			{
				on_texture_loaded();
			}
		}, delegate
		{
			block.SetColor("_Color", col);
			block.SetTexture("_MainTex", null);
			renderer.SetPropertyBlock(block);
			if (disable_reenable)
			{
				renderer.enabled = true;
			}
			if (on_texture_loaded != null)
			{
				on_texture_loaded();
			}
		});
	}

	public void PlayExploreMusic(string song_name, AudioSource source, float volume)
	{
		LoadAndPlaySfx("Assets/Sounds/Music/" + song_name + ".mp3", source, volume);
	}

	public void PlayKaraokeMusic(string song_name, AudioSource source, float volume)
	{
		LoadAndPlaySfx("Assets/Sounds/KaraokeMusic/" + song_name + ".mp3", source, volume);
	}

	public void PlayVoice(string note_path, AudioSource source, float delay)
	{
		LoadAndPlaySfxDelayed("Assets/Sounds/AnimalSounds/" + note_path + ".mp3", source, delay);
	}

	public void PlayMusicBoxNote(string note_path, AudioSource source, float delay)
	{
		LoadAndPlaySfxDelayed("Assets/Sounds/" + note_path + ".mp3", source, delay);
	}

	public void PlayFootstepSound(string footstep_path, AudioSource source, float volume)
	{
		LoadAndPlaySfx("Assets/Sounds/Footstep Sounds/" + footstep_path + ".mp3", source, volume);
	}

	private void LoadAndPlaySfx(string audioclip_path, AudioSource source, float volume)
	{
		GenericTryLoad(sfx_loaded_or_midLoad, audiosources_potentially_using_sfx, audioclip_path, source, delegate(AudioClip loaded_sfx)
		{
			source.clip = loaded_sfx;
			source.volume = volume;
			source.Play();
		});
	}

	private void LoadAndPlaySfxDelayed(string audioclip_path, AudioSource source, float delay)
	{
		DateTime load_start = DateTime.UtcNow;
		GenericTryLoad(sfx_loaded_or_midLoad, audiosources_potentially_using_sfx, audioclip_path, source, delegate(AudioClip loaded_sfx)
		{
			double totalMilliseconds = (DateTime.UtcNow - load_start).TotalMilliseconds;
			source.clip = loaded_sfx;
			float num = (float)totalMilliseconds / 1000f;
			if (num <= 0f)
			{
				num = 0f;
			}
			source.PlayDelayed(delay - num);
		});
	}

	public void AsyncInstantiateEquipment(string equipment_path, Action<GameObject> on_asset_ready)
	{
		GenericTryLoad(prefabs_loaded_or_midLoad, instances_of_prefabs, "Assets/Prefabs/" + equipment_path + ".prefab", null, on_asset_ready);
	}

	public void AsyncInstantiateShopModel(string shop_model_path, Action<GameObject> on_asset_ready)
	{
		GenericTryLoad(prefabs_loaded_or_midLoad, instances_of_prefabs, "Assets/Prefabs/Shop Models/" + shop_model_path + ".prefab", null, on_asset_ready);
	}

	public void AsyncInstantiateHouseInterior(string interior_path, Action<GameObject> on_asset_ready)
	{
		GenericTryLoad(prefabs_loaded_or_midLoad, instances_of_prefabs, "Assets/Prefabs/Interiors/" + interior_path + ".prefab", null, on_asset_ready);
	}

	public void AsyncInstantiatePerkObj(string projectile_path, Action<GameObject> on_asset_ready)
	{
		GenericTryLoad(prefabs_loaded_or_midLoad, instances_of_prefabs, "Assets/Prefabs/PerkCastPrefabs/" + projectile_path + ".prefab", null, on_asset_ready);
	}

	public void AsyncInstantiateModularPrefab(string mesh_path, Action<GameObject> on_asset_ready)
	{
		GenericTryLoad(prefabs_loaded_or_midLoad, instances_of_prefabs, "Assets/Models/" + mesh_path + ".prefab", null, on_asset_ready);
	}

	public void AsyncInstantiateDropModel(string drop_path, Action<GameObject> on_asset_ready)
	{
		GenericTryLoad(prefabs_loaded_or_midLoad, instances_of_prefabs, "Assets/Prefabs/DropModels/" + drop_path + ".prefab", null, on_asset_ready);
	}

	public static bool ValidWorldModel(InventoryItem item)
	{
		if (item.GetString("custom_type") == "furniture")
		{
			return true;
		}
		return !Startup.StringNullOrWhitespace(inventory_ctr.Instance.GetItemWorldObjPath(item.item_name));
	}

	public void AsyncInstantiateWorldObjectPrefab(InventoryItem item, Chunk chunk, Action<GameObject> on_asset_ready)
	{
		if (item.GetString("custom_type") == "furniture")
		{
			GameObject gameObject = inventory_ctr.Instance.GenerateCustomModel(item);
			if (item.GetString("interaction_type") != "")
			{
				Interactable interactable = gameObject.AddComponent<Interactable>();
				short num = item.GetShort("extra_interact_dist");
				interactable.interaction_distance = (float)num / 10f + 1f;
			}
			on_asset_ready(gameObject);
			return;
		}
		Action<GameObject> on_asset_ready2 = delegate(GameObject new_instance)
		{
			on_asset_ready(new_instance);
		};
		Action on_load_failed = delegate
		{
			on_asset_ready(null);
		};
		string itemWorldObjPath = inventory_ctr.Instance.GetItemWorldObjPath(item.item_name);
		GenericTryLoad(prefabs_loaded_or_midLoad, instances_of_prefabs, "Assets/Prefabs/" + itemWorldObjPath + ".prefab", null, on_asset_ready2, on_load_failed);
	}

	public Texture2D GetFullPattern(string pattern_name)
	{
		if (loaded_full_patterns.ContainsKey(pattern_name))
		{
			return loaded_full_patterns[pattern_name];
		}
		Texture2D texture2D = Instance.LoadImageSynchronously("PaintBrushFullPatterns/" + pattern_name);
		if (texture2D == null)
		{
			texture2D = white_tex;
		}
		loaded_full_patterns.Add(pattern_name, texture2D);
		return texture2D;
	}

	public Texture2D GetSidePattern(string pattern_name)
	{
		if (loaded_side_patterns.ContainsKey(pattern_name))
		{
			return loaded_side_patterns[pattern_name];
		}
		Texture2D texture2D = Instance.LoadImageSynchronously("PaintBrushSidePatterns/" + pattern_name);
		if (texture2D == null)
		{
			texture2D = white_tex;
		}
		loaded_side_patterns.Add(pattern_name, texture2D);
		return texture2D;
	}

	public Texture2D GetSymbol(string symbol_name)
	{
		if (loaded_symbols.ContainsKey(symbol_name))
		{
			return loaded_symbols[symbol_name];
		}
		Texture2D texture2D = Instance.LoadImageSynchronously("PaintBrushSymbols/" + symbol_name);
		if (texture2D == null)
		{
			texture2D = white_tex;
		}
		loaded_symbols.Add(symbol_name, texture2D);
		return texture2D;
	}

	public Texture2D GetParticleTexture(string tex_name)
	{
		if (loaded_particle_textures.ContainsKey(tex_name))
		{
			return loaded_particle_textures[tex_name];
		}
		Texture2D texture2D = Instance.LoadImageSynchronously("ParticleTextures/" + tex_name);
		if (texture2D == null)
		{
			texture2D = white_tex;
		}
		loaded_particle_textures.Add(tex_name, texture2D);
		return texture2D;
	}

	public ColorScheme GetColorScheme(string paint_str)
	{
		if (loaded_color_schemes.ContainsKey(paint_str))
		{
			return loaded_color_schemes[paint_str];
		}
		ColorScheme colorScheme = new ColorScheme();
		string text = "";
		string text2 = paint_str;
		if (paint_str == "Debug Paint")
		{
			text2 = PaintableObject.DebugPaint;
		}
		bool file_exists = false;
		List<string> textFileLines = Instance.GetTextFileLines("PaintBrushData/" + text2, ref file_exists);
		if (file_exists)
		{
			Dictionary<string, object> dictionary = null;
			foreach (string item in textFileLines)
			{
				if (Startup.StringNullOrWhitespace(item))
				{
					continue;
				}
				if (item[0] == '[')
				{
					if (text != "")
					{
						colorScheme.SetSwatchData(text, dictionary);
					}
					text = item.Substring(1, item.Length - 2);
					dictionary = new Dictionary<string, object>();
					continue;
				}
				int num = item.IndexOf('=');
				string text3 = item.Substring(0, num);
				string text4 = item.Substring(num + 1, item.Length - (num + 1));
				if (text3 == "color" || text3 == "colorB" || text3 == "col_side" || text3 == "col_symbol" || text3 == "start_col")
				{
					dictionary.Add(text3, ParseColor(text4));
				}
				else if (text3 == "size_over_lifetime_curve")
				{
					dictionary.Add(text3, DeserializeCurve(text4));
				}
				else if (text3 == "color_over_lifetime_gradient")
				{
					dictionary.Add(text3, DeserializeGradient(text4));
				}
				else
				{
					dictionary.Add(text3, text4);
				}
			}
			if (text != "")
			{
				colorScheme.SetSwatchData(text, dictionary);
			}
		}
		if (!loaded_color_schemes.ContainsKey(paint_str))
		{
			loaded_color_schemes.Add(paint_str, colorScheme);
		}
		return colorScheme;
	}

	private Color ParseColor(string suffix)
	{
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < suffix.Length; i++)
		{
			if (suffix[i] == ',')
			{
				if (num != -1)
				{
					num2 = i;
					break;
				}
				num = i;
			}
		}
		string s = suffix.Substring(0, num);
		string s2 = suffix.Substring(num + 1, num2 - (num + 1));
		string s3 = suffix.Substring(num2 + 1, suffix.Length - (num2 + 1));
		return new Color(float.Parse(s, Startup.parse_culture), float.Parse(s2, Startup.parse_culture), float.Parse(s3, Startup.parse_culture));
	}

	public void AsyncInstantiateWorldObjectPrefab(string obj_path, Chunk chunk, Action<GameObject> on_asset_ready)
	{
		Action<GameObject> on_asset_ready2 = delegate(GameObject new_instance)
		{
			on_asset_ready(new_instance);
		};
		Action on_load_failed = delegate
		{
			on_asset_ready(null);
		};
		GenericTryLoad(prefabs_loaded_or_midLoad, instances_of_prefabs, "Assets/Prefabs/" + obj_path + ".prefab", null, on_asset_ready2, on_load_failed);
	}

	public GameObject GetWindowPrefab(string prefab_path, string log_parent = "")
	{
		GameObject gameObject = Instance.LoadWindowSynchronously(prefab_path);
		if (gameObject == null)
		{
			Debug.Log("ERROR - CANNOT LOCATE WINDOW PREFAB (" + prefab_path + ")[" + log_parent + "]");
			return new GameObject();
		}
		return gameObject;
	}

	public string GetStringFromItemFile(string item_name, string key)
	{
		if (!loaded_inventory_item_files.ContainsKey(item_name))
		{
			TryLoadInventoryItem(item_name);
		}
		if (loaded_inventory_item_files.ContainsKey(item_name))
		{
			Dictionary<string, string> dictionary = loaded_inventory_item_files[item_name];
			if (dictionary.ContainsKey(key))
			{
				return dictionary[key];
			}
		}
		return "";
	}

	public int GetIntFromItemFile(string item_name, string key)
	{
		if (!loaded_inventory_item_files.ContainsKey(item_name))
		{
			TryLoadInventoryItem(item_name);
		}
		if (loaded_inventory_item_files.ContainsKey(item_name))
		{
			Dictionary<string, string> dictionary = loaded_inventory_item_files[item_name];
			if (dictionary.ContainsKey(key))
			{
				return int.Parse(dictionary[key], Startup.parse_culture);
			}
		}
		return 0;
	}

	public Vector3 GetVector3FromItemFile(string item_name, string key)
	{
		string[] array = GetStringFromItemFile(item_name, key).Split(',');
		return new Vector3(float.Parse(array[0], Startup.parse_culture), float.Parse(array[1], Startup.parse_culture), float.Parse(array[2], Startup.parse_culture));
	}

	public void ReleaseItemFile(string item_name)
	{
		if (loaded_inventory_item_files.ContainsKey(item_name))
		{
			loaded_inventory_item_files.Remove(item_name);
		}
	}

	public float GetFloatFromItemFile(string item_name, string key)
	{
		if (!loaded_inventory_item_files.ContainsKey(item_name))
		{
			TryLoadInventoryItem(item_name);
		}
		if (loaded_inventory_item_files.ContainsKey(item_name))
		{
			Dictionary<string, string> dictionary = loaded_inventory_item_files[item_name];
			if (dictionary.ContainsKey(key))
			{
				return float.Parse(dictionary[key], Startup.parse_culture);
			}
		}
		return 0f;
	}

	private bool TryLoadInventoryItem(string item_name)
	{
		bool file_exists = false;
		List<string> textFileLines = GetTextFileLines("InventoryItems/" + item_name, ref file_exists);
		if (!file_exists)
		{
			return false;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string item in textFileLines)
		{
			int num = item.IndexOf(" = ");
			if (num != -1)
			{
				dictionary.Add(item.Substring(0, num), item.Substring(num + 3, item.Length - num - 3));
			}
		}
		if (!loaded_inventory_item_files.ContainsKey(item_name))
		{
			loaded_inventory_item_files.Add(item_name, dictionary);
		}
		return true;
	}

	public bool ItemExists(string item_name)
	{
		if (loaded_inventory_item_files.ContainsKey(item_name))
		{
			return true;
		}
		return TryLoadInventoryItem(item_name);
	}

	public Quest LoadQuest(string quest_key)
	{
		Quest quest = new Quest();
		quest.display_name = quest_key;
		bool file_exists = false;
		List<string> textFileLines = Instance.GetTextFileLines("Quests/" + quest_key, ref file_exists);
		if (!file_exists)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("Hint", "ERROR: QUEST NOT FOUND");
			quest.steps.Add(dictionary);
			return quest;
		}
		Dictionary<string, string> dictionary2 = null;
		foreach (string item in textFileLines)
		{
			if (Startup.StringNullOrWhitespace(item))
			{
				continue;
			}
			if (item.Contains("[step "))
			{
				dictionary2 = new Dictionary<string, string>();
				quest.steps.Add(dictionary2);
				continue;
			}
			if (dictionary2 != null)
			{
				int num = item.IndexOf(" = ");
				if (num == -1)
				{
					continue;
				}
				string text = item.Substring(0, num);
				string value = item.Substring(num + 3, item.Length - num - 3);
				if (text == "Hint" || text == "Hint_POR" || text == "Hint_RUS" || text == "Hint_IND" || text == "Hint_SPN" || text == "Hint_TAI")
				{
					switch (TranslationControl.Instance.use_language)
					{
					case TranslationControl.languages.English:
						if (text == "Hint")
						{
							dictionary2.Add("Hint", value);
						}
						break;
					case TranslationControl.languages.Russian:
						if (text == "Hint_RUS")
						{
							dictionary2.Add("Hint", value);
						}
						break;
					case TranslationControl.languages.Portuguese:
						if (text == "Hint_POR")
						{
							dictionary2.Add("Hint", value);
						}
						break;
					case TranslationControl.languages.Indonesian:
						if (text == "Hint_IND")
						{
							dictionary2.Add("Hint", value);
						}
						break;
					case TranslationControl.languages.Spanish:
						if (text == "Hint_SPN")
						{
							dictionary2.Add("Hint", value);
						}
						break;
					case TranslationControl.languages.Thai:
						if (text == "Hint_TAI")
						{
							dictionary2.Add("Hint", value);
						}
						break;
					}
				}
				else
				{
					dictionary2.Add(text, value);
				}
				continue;
			}
			int num2 = item.IndexOf(" = ");
			if (num2 == -1)
			{
				continue;
			}
			item.Substring(0, num2);
			string text2 = item.Substring(num2 + 3, item.Length - num2 - 3);
			if (item.Contains("Repeat_hours"))
			{
				int num3 = int.Parse(text2, Startup.parse_culture);
				if (num3 != -1)
				{
					quest.repeat_hours = num3;
				}
			}
			else if (item.Contains("Reward0_item"))
			{
				if (quest.reward_items.Count == 0)
				{
					quest.reward_items.Add(new ItemCountPair("", 0));
				}
				ExtraInventoryData extraDataCopy = quest.reward_items[0].item.GetExtraDataCopy();
				quest.reward_items[0] = new ItemCountPair(new InventoryItem(text2, extraDataCopy), quest.reward_items[0].count);
			}
			else if (item.Contains("Reward0_count"))
			{
				if (quest.reward_items.Count == 0)
				{
					quest.reward_items.Add(new ItemCountPair("", 0));
				}
				quest.reward_items[0] = new ItemCountPair(quest.reward_items[0].item, int.Parse(text2, Startup.parse_culture));
			}
			else if (item.Contains("Reward0_key_name"))
			{
				if (quest.reward_items.Count == 0)
				{
					quest.reward_items.Add(new ItemCountPair("", 0));
				}
				ExtraInventoryData extraDataCopy2 = quest.reward_items[0].item.GetExtraDataCopy();
				extraDataCopy2.SetString("key_name", text2);
				quest.reward_items[0] = new ItemCountPair(new InventoryItem(quest.reward_items[0].item.item_name, extraDataCopy2), quest.reward_items[0].count);
			}
			else if (item.Contains("RewardChest"))
			{
				quest.reward_chests.Add(int.Parse(text2, Startup.parse_culture));
			}
			else if (item.Contains("RewardSuperChest"))
			{
				quest.reward_superChests.Add(int.Parse(text2, Startup.parse_culture));
			}
			else if (item.Contains("Complete_text"))
			{
				if (item.Contains("Complete_text_POR"))
				{
					if (TranslationControl.Instance.use_language == TranslationControl.languages.Portuguese)
					{
						quest.complete_text = text2;
					}
				}
				else if (item.Contains("Complete_text_RUS"))
				{
					if (TranslationControl.Instance.use_language == TranslationControl.languages.Russian)
					{
						quest.complete_text = text2;
					}
				}
				else if (item.Contains("Complete_text_IND"))
				{
					if (TranslationControl.Instance.use_language == TranslationControl.languages.Indonesian)
					{
						quest.complete_text = text2;
					}
				}
				else if (item.Contains("Complete_text_SPN"))
				{
					if (TranslationControl.Instance.use_language == TranslationControl.languages.Spanish)
					{
						quest.complete_text = text2;
					}
				}
				else if (item.Contains("Complete_text_TAI"))
				{
					if (TranslationControl.Instance.use_language == TranslationControl.languages.Thai)
					{
						quest.complete_text = text2;
					}
				}
				else
				{
					quest.complete_text = text2;
				}
			}
			else if (item.Contains("questname"))
			{
				switch (TranslationControl.Instance.use_language)
				{
				case TranslationControl.languages.Russian:
					if (item.Contains("questname_RUS"))
					{
						quest.display_name = text2;
					}
					break;
				case TranslationControl.languages.Portuguese:
					if (item.Contains("questname_POR"))
					{
						quest.display_name = text2;
					}
					break;
				case TranslationControl.languages.Indonesian:
					if (item.Contains("questname_IND"))
					{
						quest.display_name = text2;
					}
					break;
				case TranslationControl.languages.Spanish:
					if (item.Contains("questname_SPN"))
					{
						quest.display_name = text2;
					}
					break;
				case TranslationControl.languages.Thai:
					if (item.Contains("questname_TAI"))
					{
						quest.display_name = text2;
					}
					break;
				}
			}
		}
		return quest;
	}

	private void GenericTryLoad<AssetType, ConsumerType>(Dictionary<string, AsyncOperationHandle<AssetType>> loaded_or_midLoad_dict, Dictionary<ConsumerType, string> consumer_list, string addressable_path, ConsumerType consumer, Action<AssetType> on_asset_ready, Action on_load_failed = null)
	{
		if (!loaded_or_midLoad_dict.ContainsKey(addressable_path))
		{
			AsyncOperationHandle<AssetType> value = Addressables.LoadAssetAsync<AssetType>(addressable_path);
			loaded_or_midLoad_dict.Add(addressable_path, value);
		}
		if (typeof(ConsumerType) == typeof(GameObject) && (consumer == null || consumer.ToString() == "null"))
		{
			consumer = (ConsumerType)Convert.ChangeType(null_consumer, typeof(ConsumerType));
		}
		AsyncOperationHandle<AssetType> load_op = loaded_or_midLoad_dict[addressable_path];
		if (!load_op.IsDone)
		{
			load_op.Completed += delegate
			{
				GenericAssetIsReady(load_op, consumer_list, loaded_or_midLoad_dict, addressable_path, consumer, on_asset_ready, on_load_failed);
			};
		}
		else
		{
			GenericAssetIsReady(load_op, consumer_list, loaded_or_midLoad_dict, addressable_path, consumer, on_asset_ready, on_load_failed);
		}
	}

	private void GenericAssetIsReady<AssetType, ConsumerType>(AsyncOperationHandle<AssetType> load_op, Dictionary<ConsumerType, string> consumer_list, Dictionary<string, AsyncOperationHandle<AssetType>> loaded_or_midLoad_dict, string addressable_path, ConsumerType consumer, Action<AssetType> on_asset_ready, Action on_load_failed)
	{
		if (load_op.Status == AsyncOperationStatus.Failed)
		{
			Debug.Log(Time.time.ToString() + " ... could not load (" + addressable_path + ")");
			if (loaded_or_midLoad_dict.ContainsKey(addressable_path))
			{
				loaded_or_midLoad_dict.Remove(addressable_path);
				Addressables.Release(load_op);
			}
			if (on_load_failed != null)
			{
				on_load_failed();
			}
		}
		else
		{
			if (consumer == null || consumer.ToString() == "null")
			{
				return;
			}
			AssetType val = load_op.Result;
			if (typeof(ConsumerType) == typeof(GameObject))
			{
				GameObject value = UnityEngine.Object.Instantiate((GameObject)Convert.ChangeType(val, typeof(GameObject)));
				val = (AssetType)Convert.ChangeType(value, typeof(AssetType));
				consumer = (ConsumerType)Convert.ChangeType(value, typeof(ConsumerType));
			}
			on_asset_ready(val);
			if (consumer_list.ContainsKey(consumer))
			{
				consumer_list[consumer] = addressable_path;
			}
			else
			{
				consumer_list.Add(consumer, addressable_path);
			}
		}
	}

	private void GenericDeloadUnused<AssetType, ConsumerType>(Dictionary<string, AsyncOperationHandle<AssetType>> loaded_or_midLoad_list, Dictionary<ConsumerType, string> consumer_list)
	{
		List<ConsumerType> list = new List<ConsumerType>();
		foreach (KeyValuePair<ConsumerType, string> item in consumer_list)
		{
			if (item.Key.ToString() == "null")
			{
				list.Add(item.Key);
			}
		}
		foreach (ConsumerType item2 in list)
		{
			consumer_list.Remove(item2);
		}
		List<string> list2 = new List<string>();
		foreach (KeyValuePair<string, AsyncOperationHandle<AssetType>> item3 in loaded_or_midLoad_list)
		{
			AsyncOperationHandle asyncOperationHandle = item3.Value;
			if (!asyncOperationHandle.IsDone)
			{
				continue;
			}
			bool flag = false;
			foreach (KeyValuePair<ConsumerType, string> item4 in consumer_list)
			{
				if (item4.Value == item3.Key)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list2.Add(item3.Key);
			}
		}
		foreach (string item5 in list2)
		{
			Addressables.Release(loaded_or_midLoad_list[item5]);
			loaded_or_midLoad_list.Remove(item5);
		}
	}

	public static void DeserializeParticleTransform(string input_str, ref Vector3 local_position, ref Quaternion local_rotation, ref Vector3 local_scale, ref string shape_type, ref Dictionary<string, string> shape_data)
	{
		string[] array = input_str.Split(':');
		if (array.Length == 6)
		{
			local_position = new Vector3(float.Parse(array[0].Split(',')[0], Startup.parse_culture), float.Parse(array[0].Split(',')[1], Startup.parse_culture), float.Parse(array[0].Split(',')[2], Startup.parse_culture));
			local_rotation = new Quaternion(float.Parse(array[1].Split(',')[0], Startup.parse_culture), float.Parse(array[1].Split(',')[1], Startup.parse_culture), float.Parse(array[1].Split(',')[2], Startup.parse_culture), float.Parse(array[1].Split(',')[3], Startup.parse_culture));
			local_scale = new Vector3(float.Parse(array[2].Split(',')[0], Startup.parse_culture), float.Parse(array[2].Split(',')[1], Startup.parse_culture), float.Parse(array[2].Split(',')[2], Startup.parse_culture));
			shape_type = array[3];
			if (shape_type == "CONE")
			{
				shape_data.Add("shape_CONE_length", array[4]);
				shape_data.Add("shape_CONE_radius", array[5]);
			}
		}
	}

	public static AnimationCurve DeserializeCurve(string curveString)
	{
		return new AnimationCurve(curveString.Split('|').Select(delegate(string kfs)
		{
			float[] array = kfs.Split(':').Select((string p) => float.Parse(p, Startup.parse_culture)).ToArray();
			return new Keyframe(array[0], array[1], array[2], array[3]);
		}).ToArray());
	}

	public static Gradient DeserializeGradient(string serializedGradient)
	{
		string[] array = serializedGradient.Split('`');
		string[] source = array[0].Split('|');
		string[] source2 = array[1].Split('|');
		GradientColorKey[] colorKeys = source.Select(delegate(string part)
		{
			string[] array2 = part.Split(',');
			return new GradientColorKey(new Color(float.Parse(array2[0], Startup.parse_culture), float.Parse(array2[1], Startup.parse_culture), float.Parse(array2[2], Startup.parse_culture), float.Parse(array2[3], Startup.parse_culture)), float.Parse(array2[4], Startup.parse_culture));
		}).ToArray();
		GradientAlphaKey[] alphaKeys = source2.Select(delegate(string part)
		{
			string[] array3 = part.Split(',');
			return new GradientAlphaKey(float.Parse(array3[0], Startup.parse_culture), float.Parse(array3[1], Startup.parse_culture));
		}).ToArray();
		Gradient gradient = new Gradient();
		gradient.SetKeys(colorKeys, alphaKeys);
		return gradient;
	}
}
