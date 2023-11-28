using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.ProBuilder;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMeshGenerator : MonoBehaviour
{
    public Vector3Int size;
    public int subdivisions;

    private Mesh mesh;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Color32[] cubeUV;

    public Transform boneA;
    public Transform boneB;


    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Sphere";
		CreateVertices();
		CreateTriangles();
		CreateColliders();
    }

    // Update is called once per frame
    void Update()
    {
        return;
    }

    public void CreateVertices(){
        vertices = new Vector3[(size.x + 1) * (size.y + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
		Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        for (int i=0, y = 0; y <= size.y ; y++){
            for (int x = 0; x <= size.x ; x++, i++){
                vertices[i] = new Vector3((float) x , (float) y, 0f);
                uv[i] = new Vector2((float) x / size.x, (float) y /size.y);
                tangents[i] = tangent;
            }
        }
		mesh.vertices = vertices;
        //Vertices have to always be added first because unity does this safely and check for verticies size...
        mesh.uv = uv;
        mesh.tangents = tangents;
    }

    public void CreateTriangles(){
        int[] triangles = new int[size.x * size.y * 6];
		for (int ti = 0, vi = 0, y = 0; y < size.y; y++, vi++) {
			for (int x = 0; x < size.x; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + size.x + 1;
				triangles[ti + 5] = vi + size.x + 2;
			}
		}
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
    }

    public void CreateColliders(){
        gameObject.AddComponent<BoxCollider>();
    }

    private void OnDrawGizmos () {
        if (vertices == null) {
			return;
		}
		Gizmos.color = Color.black;
		for (int i = 0; i < vertices.Length; i++) {
			Gizmos.DrawSphere(vertices[i], 0.1f);
		}
	}
}
