using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using TreeEditor;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter))]
public class DeformableMesh : MonoBehaviour
{

    public int dentEdges = 3;
    public float dentWidthFactor = 0.5f;

    public float dentAngle = 60f;
    public float compressiveStrength = 1f;
    public float springForce = 20f;
    public float damping = 5f;
    float uniformScale = 1f;
    Mesh deformingMesh;

    MeshCollider meshCollider;
	Vector3[] originalVertices, displacedVertices, vertexVelocities;

    // Start is called before the first frame update
    void Start () {
		deformingMesh = GetComponent<MeshFilter>().mesh;
        if (!deformingMesh){
            return;
        }
		originalVertices = deformingMesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
        //incase there are other colliders...
        meshCollider = GetComponent<MeshCollider>();
        if (!meshCollider){
            //Adds a concave MeshCollider so we can use it only for triangle faces...
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.convex = false;
            meshCollider.sharedMesh = deformingMesh;
            meshCollider.cookingOptions = MeshColliderCookingOptions.None;
            //If it doesn't already exist, we want it to be asleep until we use it
            meshCollider.enabled = true;
        } 
		for (int i = 0; i < originalVertices.Length; i++) {
			displacedVertices[i] = originalVertices[i];
		}
        vertexVelocities = new Vector3[originalVertices.Length];
	} 

    // Update is called once per frame
    void Update()
    {
        //uniformScale = transform.localScale.x;
		//for (int i = 0; i < displacedVertices.Length; i++) {
		//	UpdateVertex(i);
		//}
		//deformingMesh.vertices = displacedVertices;
		//deformingMesh.RecalculateNormals();
	
    }

   void UpdateVertex (int i) {
		Vector3 velocity = vertexVelocities[i];
		Vector3 displacement = displacedVertices[i] - originalVertices[i];
        displacement *= uniformScale;
		velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime;
		vertexVelocities[i] = velocity;
		displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
	}

    public void AddDeformingForce (Vector3 point, float force) {
		for (int i = 0; i < displacedVertices.Length; i++) {
			AddForceToVertex(i, point, force);
		}
	}

	void AddForceToVertex (int i, Vector3 point, float force) {
        point = transform.InverseTransformPoint(point);
        Vector3 pointToVertex = displacedVertices[i] - point;
        pointToVertex *= uniformScale;
		float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
		float velocity = attenuatedForce * Time.deltaTime;
		vertexVelocities[i] += pointToVertex.normalized * velocity;
	}

    //Adds a vertex at the point 
    //To improve can push back the vertices of the sourrounding triangles so that we dont get sharp edges
    //This could be something that falls off with distance perhaps, but that runs the risk that deep dents will shove the center of mass back
    //We can fix this by counting the number of vertices adjusted and ensuring that theyre less than 50%,
    //A better approach would be to recalculate the center of mass after sufficient operations.
    //Sufficient operations are when we count the number of vertices being adjusted reaches 50%
    //Additionally, I could work on adding more vertices to the dent, this would make it smoother and look more natural...
    //Next is working on a Hole, mesh-wise that should detecting if it goes through both faces and then cutting both faces and filling the cylindar

    //FIXED THIS BUG FIRST TRY!!! Also theres a bug where moving the object from the origin breaks the dent location...
    public void AddDentingForce(Vector3 point, Vector3 force){
        force = transform.InverseTransformDirection(force);
        force = force * (1f / compressiveStrength);
        //take the dent from global space to our game objects local space (should also be the mesh render's)
        //The rigid bodies and the game objects transform should be the same i think??
        //Since these transforms are linear we should be able to sum then transform for peak efficiency
        //point = transform.InverseTransformPoint(point);
        //force = transform.InverseTransformDirection(force);
        Vector3 dent = point + force;
        dent = transform.InverseTransformPoint(dent);
        point -= (force * 0.01f);
        Ray ray = new();
        ray.origin = point;
        //recall that the dent is at the point + force, so we want to backtrack from the dent....
        ray.direction = force;
        Debug.DrawRay(point, force , UnityEngine.Color.blue, 5f);
        RaycastHit hit;
        RaycastHit second;
        //If we hit the mesh 
        if (meshCollider.Raycast(ray, out hit, force.magnitude * 1.2f)){
            //Check to see if we already passed through a triangle by performing a second raycast
            ray.origin = hit.point + (ray.direction * 0.01f);
            bool pierced = false;
            if (meshCollider.Raycast(ray, out second, force.magnitude * 1.2f)){
                Debug.Log("Pierced through two triangles!");
                //hit = second;
                pierced = true;
            }

            Debug.Log("tirangle index: " + hit.triangleIndex);
            int triangleIndex = hit.triangleIndex * 3;

            Dent d = new();
            
            d.position = dent;
            d.force = force;
            d.deformable = this;
            d.mesh = meshCollider.sharedMesh;
            d.collider = meshCollider;

            d.createDent();
            Vector3[] vertices = d.vertices.ToArray();
            int[] triangles = d.triangles.ToArray();
            
            //replace the mesh's data
            deformingMesh.Clear();
            deformingMesh.vertices = vertices;

            //Update these for our elastic collision deformations to still work
            originalVertices = new Vector3[vertices.Length];
            displacedVertices = new Vector3[vertices.Length];
            vertexVelocities = new Vector3[vertices.Length];
            vertices.CopyTo(displacedVertices, 0);
            vertices.CopyTo(originalVertices, 0);
            Array.Fill<Vector3>(vertexVelocities, new Vector3(0,0,0));

            deformingMesh.uv = d.uvs.ToArray();
            deformingMesh.normals = d.normals.ToArray();
            deformingMesh.triangles = triangles;

            //Have to reset the collider to use the dented limb...
            //For some reason have to delete the old one too... note this is performance intensive
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = deformingMesh;
        
            
        } else {
            Debug.Log("Tried denting but couldn't find the triangle");
            //We cant find the face to deform so do nothing??
        }
    }


    public class Dent {
        //Parameters to help with adjusting and detecting existing meshes.. For example with healing we might want to remove specific dents by reverting everything...
        public Vector3 position; //center of the dent
        public Vector3[] innerEdgeVerts; //external rim of the dent
        public List<int> triangles;//new triangle list
        public List<Vector3> vertices; //new vertices list

        public List<Vector2> uvs;
        public List<Vector3> normals;

        //Parameters needed for generting the dent
        public List<int> intersectedTri; //original triangles that get replaced
        public List<int> newTri; //new triangles to stitch up the whole

        public Vector3 force; //position - force = surface point clicjed on
        public DeformableMesh deformable;
        public Mesh mesh;
        public MeshCollider collider;
        public void createDent(){
            
            triangles = mesh.triangles.ToList(); // The new triangle list
            vertices = mesh.vertices.ToList(); // The new VertexList
            normals = mesh.normals.ToList();
            uvs = mesh.uv.ToList();

            Vector3[] edges = GetEdges(); //calculates all innerEdgeVerts
            innerEdgeVerts = edges;
            ShrinkTriangleSet(edges, triangles, vertices); //creates a list of updated triangles
            vertices.Add(position);
            normals.Add(force * -1);
            uvs.Add(new Vector2(0,0));
            for (int e1, e2, next, e = 0; e < edges.Length; e++){
                next = (e + 1  < edges.Length) ? e + 1 : 0;
                e1 = mesh.vertices.Length + e;
                e2 = mesh.vertices.Length + next;
                AddTriangle(triangles, e1, e2, vertices.Count - 1);
            }

        }
        /// <summary>
        /// shrink the working set of triangles and verts to just those that are on the positive side of our dent position
        /// get outer edges by forming planes with the edge verts, dent, edge vert[i+ 1]
        /// next find all the triangles that intersecting our planes inbetween our edges and change them into 1 or 2 quads depending on if the intersection is collinear
        /// Now all that should be left are the triangles that don't intersect which are either in the interior or encompass the entirety....
        /// We find those by finding those in the set not in intersected: if the raycasts for finidng the dents or edges hit them, then they are 
        /// </summary>
        /// <param name="edges"></param>
        /// <returns></returns>
        void ShrinkTriangleSet(Vector3[] edges, List<int> tris, List<Vector3> vertices){
            List<int> set = new(); //The set of indices to keep checking, 0 -> 0, 1 -> 3, etc...
            for (int i = 0; i < tris.Count / 3; i++){
                set.Add(i * 3);
            }
            Vector3[] verts = mesh.vertices;
            int[] triangles = mesh.triangles;

            intersectedTri = new();
            newTri = new();

            Plane plane = new();
            Vector3 normal = Vector3.Cross(force, new Vector3(force.y, force.z, force.x));
            Vector3 normalToo = Vector3.Cross(force, normal);
            //plane.Set3Points(position, position + normal, position + normalToo);
            plane.SetNormalAndPosition(force * -1, position);
        
            Vector3 p1, p2, p3;
            //Ignore all the triangles that are below our dent position
            for ( int i = 0; i< set.Count; i++){
                int triangle = set[i];
                p1 = verts[triangles[triangle + 0]];
                p2 = verts[triangles[triangle + 1]];
                p3 = verts[triangles[triangle + 2]];
                if (!plane.GetSide(p1) && !plane.GetSide(p2) && !plane.GetSide(p3)){
                    set.RemoveAt(i);
                }
            }
            //Check all the edges
            for (int e1, e2, next, e = 0; e < edges.Length; e++){
                next = (e + 1  < edges.Length) ? e + 1 : 0;
                e1 = verts.Length + e; //index in future vert array of e1
                e2 = verts.Length + next; // to help it wrap around
                Vector3 lineSegment = (edges[next] - edges[e]);
                float length = lineSegment.magnitude;
                //Create the plane from two adjavent edge vertices
                plane.Set3Points(position, edges[e], edges[next]);
                for (int i = 0; i < set.Count; i++){
                    int triangle = set[i];
                    //Vector3 = verts(original triangles [triangle_index for all in set])
                    p1 = verts[triangles[triangle + 0]];
                    p2 = verts[triangles[triangle + 1]];
                    p3 = verts[triangles[triangle + 2]];
                    //If a triangle is on the outside of the plane we can safely ignore it :)
                    if (!plane.GetSide(p1) && !plane.GetSide(p2) && !plane.GetSide(p3)){
                        set.Remove(triangle);
                        //makes it more efficient for the future edges to remove these triangles that are 'cleared'
                    //If a triangle is fully on the inside then we do nothing
                    }else if(plane.GetSide(p1) && plane.GetSide(p2) && plane.GetSide(p3)){
                        if (!intersectedTri.Contains(triangle)){
                            intersectedTri.Add(triangle);
                            //Although this triangle isn't intersected, we still want to mark it for deletion
                        } 
                    //If a triangle is neither, it must intersect at least once so we need to add the intersection vertex and connect it to the edges
                    } else {
                        //No longer a canidate for being an interior trianlge
                        if (!intersectedTri.Contains(triangle)){
                            intersectedTri.Add(triangle);
                        }
                        //We are going to alter this triangle, either intersection or inscription, so mark for deletion
                        Vector3 point;
                        bool side;
                        bool height;
                        bool first = true;
                        if (CheckLine(plane, p1, p2, edges[e], edges[next], out point, out side, out height)){
                            vertices.Add( point);
                            normals.Add(Vector3.Cross(p2 - p1, edges[e] - edges[next]));
                            uvs.Add(new Vector2(0,0));
                            if(side){//P1 is on the inside
                                AddTriangle(tris, vertices.Count-1, triangles[triangle + 1], triangles[triangle + 2]); 
                            } else{ //p2 is on the inside
                                AddTriangle(tris, triangles[triangle + 0], vertices.Count - 1, triangles[triangle + 2]); 
                            }
                            if(height){
                                AddTriangle(tris, vertices.Count - 1, e2, e1); //Creates a new triangle connecting the new vertex to ou
                            } else {
                                //We dont need to do anything since this is beyond the original triangle
                            }
                            first = false;
                        } if (CheckLine(plane, p1, p3, edges[e], edges[next], out point, out side, out height)){
                            vertices.Add( point);
                            normals.Add(Vector3.Cross(p3 - p1, edges[e] - edges[next]));
                            uvs.Add(new Vector2(0,0));
                            if (first){
                                if(side){//P1 is on the inside
                                    AddTriangle(tris, vertices.Count-1, triangles[triangle + 1], triangles[triangle + 2]);
                                } else{ //p3 is on the inside
                                    AddTriangle(tris, triangles[triangle + 0], triangles[triangle + 1], vertices.Count-1);
                                }
                                if(height){
                                    AddTriangle(tris, vertices.Count - 1, e2, e1);
                                }
                            } else { //Create the second part of the quad instead of swapping triangles
                                if(side){//P1 is on the inside
                                    //Note that this logic relies on the fact that in euclidean space triangles cant have all three sides intersected by a single line
                                    AddTriangle(tris, vertices.Count - 2, vertices.Count - 1 , triangles[triangle + 2]);
                                } else{ //p3 inside p1 outside, second therefore p2 inside too
                                    AddTriangle(tris, triangles[triangle + 0], vertices.Count - 2, vertices.Count - 1);
                                }
                                if(height){
                                    //On the second intersection we have to add the base triangle
                                    AddTriangle(tris, vertices.Count - 2, e1, vertices.Count - 1); 
                                }
                            }
                            first = false;
                        } if (CheckLine(plane, p2, p3, edges[e], edges[next], out point, out side, out height)){
                            vertices.Add(point);
                            normals.Add(Vector3.Cross(p3 - p2, edges[e] - edges[next]));
                            uvs.Add(new Vector2(0,0));
                            if (first){
                                if(side){//P2 is on the inside
                                    AddTriangle(tris, triangles[triangle + 0],  vertices.Count-1, triangles[triangle + 2]);
                                } else{ //p3 is on the inside
                                    AddTriangle(tris, triangles[triangle + 0],  triangles[triangle + 1], vertices.Count - 1 );
                                }
                                if(height){
                                    AddTriangle(tris, vertices.Count - 1, e2, e1);
                                }
                            } else { //Create the second part of the quad instead of swapping triangles
                                if(side){//P2 inside p3 outside
                                    AddTriangle(tris, vertices.Count - 2, vertices.Count - 1 , triangles[triangle + 2]);
                                } else{ //p3 inside p2 outside, second therefore p2 inside too
                                    AddTriangle(tris, vertices.Count - 1, triangles[triangle + 1], vertices.Count - 2);
                                }
                                if(height){
                                    //On the second intersection we have to add the base triangle
                                    AddTriangle(tris, vertices.Count - 2, e1, vertices.Count - 1); 
                                }
                            }
                            first = false;
                        }
                        if (first){ //checks if there have been 0 intersections so far
                        //This means our dent is inscribed within the triangle, or at least this face of the dent....
                            //We also need to add triangles between the inside points, our outside points and the verts...
                            //Also if two triangles are on the same base then we need a quad instead
                            if (plane.GetSide(p1) && plane.GetSide(p2)){ //p3 is only one outside
                                //AddTriangle(tris, triangles[triangle + 2], triangles[triangle + 0], e2);
                                //AddTriangle(tris, triangles[triangle + 2], e1, triangles[triangle + 1]);
                                AddTriangle(tris, e2, e1, triangles[triangle + 2]); 
                            } else if (plane.GetSide(p1) && plane.GetSide(p3)){
                                //AddTriangle(tris, triangles[triangle + 1], triangles[triangle + 2], e2);
                                //AddTriangle(tris, triangles[triangle + 1], e1, triangles[triangle + 0]);
                                AddTriangle(tris, e2, e1, triangles[triangle + 1]); 
                            } else if (plane.GetSide(p2) && plane.GetSide(p3)){//p1 is only one outside
                                //AddTriangle(tris, triangles[triangle + 0], triangles[triangle + 1], e2);
                                //AddTriangle(tris, triangles[triangle + 0], e1, triangles[triangle + 2]);
                                AddTriangle(tris, e2, e1, triangles[triangle + 0]); 
                            } else if (plane.GetSide(p1)){
                                AddTriangle(tris, triangles[triangle + 1], triangles[triangle + 2], e1);
                                AddTriangle(tris, triangles[triangle + 2], e2, e1);
                            } else if (plane.GetSide(p2)){
                                AddTriangle(tris, triangles[triangle + 0], e1, triangles[triangle + 2]);
                                AddTriangle(tris, triangles[triangle + 2], e1, e2);
                            } else if (plane.GetSide(p3)){
                                AddTriangle(tris, triangles[triangle + 0], triangles[triangle + 1], e1);
                                AddTriangle(tris, triangles[triangle + 1], e2, e1);
                            }
                        }
                    }   
                }
            }
            //Its crucial that the intersected tris are sorted in ascending order else removing from tris wont be stable
            //they should be in the proper order because on the first edge all triangles that aren't outside get added to the list
            for (int i  = intersectedTri.Count - 1; i >= 0; i--){
                int tri = intersectedTri[i];
                tris.RemoveRange(tri, 3);
            }
            //Everything still in the working set is now an interior triangle!
            //Everything in the intersected set needs to be deleted
            //tris.RemoveRange(triangle, 3);
        }

        //Checks if the given points form a line that crosses the plane between the two points e1 and e2, if so gives the point and the side
        //Also checks if the point is collinear with e1 and e2, if so height is false if not height is true
        bool CheckLine(Plane plane, Vector3 p1, Vector3 p2, Vector3 e1, Vector3 e2, out Vector3 point, out bool side, out bool height){
            Ray ray = new();
            ray.origin = p1;
            ray.direction = p2 - p1;
            float distance;
            if (plane.Raycast(ray, out distance)){
                point = (ray.origin + distance * ray.direction);
                if (PointBetweenLine(point, e1, e2)){
                    Debug.DrawRay(e1 + ((e2-e1) / 2f), plane.normal, Color.white, 99f);
                    Debug.DrawLine(p1, point, Color.yellow, 99f);
                    side = plane.GetSide(p1) ? true : false;
                    height = (Mathf.Abs(Vector3.Dot((point - e1).normalized, (e2 - e1).normalized) - 1) < Mathf.Epsilon) ? false : true;
                    return true;
                } 
            }
            point = Vector3.zero;
            side = false;
            height = false;
            return false;
        }


        void AddTriangle(List<int> triangles, int a, int b, int c, List<int> indexTracker = null){
            if (indexTracker is not null){
                indexTracker.Add(triangles.Count / 3);
            } else {
                this.newTri.Add(triangles.Count / 3);
            }
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        //This is used to determing if a point is between two other points...
        static bool PointBetweenLine(Vector3 point, Vector3 lineA, Vector3 lineB){
            Vector3 line = lineB - lineA;
            float length = line.magnitude;
            Vector3 pointLine = point - lineA;
            float dot = Vector3.Dot(pointLine, line);
            return (dot >= 0 && dot <= length);
        }
        
        /// <summary>
        /// calculate the edge locations, raycast from position to surface
        /// </summary>
        /// <returns>returns a vector of local points on our mesh's surface that correspond to the edges of our dent</returns>
        Vector3[] GetEdges(){
            
            int num = deformable.dentEdges;
            Vector3[] edges = new Vector3[num + 1];
            float angle = deformable.dentAngle;
            float width = deformable.dentWidthFactor;

            Vector3 direction = (force * -1).normalized;
            //Create a random vector perpendivular to our force
            Vector3 normal = Vector3.Cross(direction, new Vector3(direction.y, direction.x, direction.z));
            Vector3 edge;
            //The quaternion that represents rotating our edge direction around our force vector
            

            Ray ray = new();
            ray.origin = deformable.transform.TransformPoint(position);
            RaycastHit hit;
            
            edge = Vector3.Lerp(direction, normal, angle / 90f);

            for (int i = 0; i < num; i++){
                //creates the edge direction vector by rotating from straight out to perpendicular by 90-110% of the angle value
                //edge = Vector3.Lerp(direction, normal, (angle * UnityEngine.Random.Range(0.9f, 1.1f)) / 90f);
                
                //The quaternion that represents rotating our edge direction around our force vector
                edge = Quaternion.AngleAxis(  360f / (float) num, direction) * edge;
                ray.direction = deformable.transform.TransformDirection(edge);
                if (!collider.Raycast(ray, out hit, 100f)){
                    Debug.Log("Couldn't ray cast to our edge, position: " + ray.origin + "\ndirection: " + ray.direction);
                    //If we cant find the surface have it "splash" with half the strength...
                    var point = ray.origin + (ray.direction * (force.magnitude * 1.5f));
                    Debug.DrawLine(ray.origin, point, UnityEngine.Color.red, 99f);
                    edges[i] = point; //deformable.transform.InverseTransformPoint(point);
                } else{
                    edges[i] = deformable.transform.InverseTransformPoint(hit.point);
                    Debug.Log("Found edge (" + i + ") at local position: " + edges[i]);
                    Debug.DrawLine(deformable.transform.TransformPoint(position), hit.point, UnityEngine.Color.red, 99f);
                }
                vertices.Add(edges[i]);
                normals.Add(force * -1);
                uvs.Add(new Vector2(1,1));

            }
            return edges;
        }
        
    }

}
