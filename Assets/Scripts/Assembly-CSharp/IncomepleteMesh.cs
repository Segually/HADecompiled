using System.Collections.Generic;
using UnityEngine;

public class IncomepleteMesh
{
	public List<Vector3> mesh_vertices = new List<Vector3>();

	public List<int> mesh_tris = new List<int>();

	public List<Vector2> mesh_uvs = new List<Vector2>();

	public int curr_animation_id;
}
