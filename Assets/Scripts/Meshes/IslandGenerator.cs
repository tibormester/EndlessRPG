using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEditor.UI;

public class IslandGenerator : MonoBehaviour
{
    public bool refresh = false;
    //X,Y dimensions for our grid
    public Vector2Int gridSize = new(100,100);
    //The Default size of our chunks
    public float chunkSize = 5f;
    // Start is called before the first frame update

    Chunk[] chunks;

    void Start()
    {
        chunks = new Chunk[gridSize.x * gridSize.y]; 
    }

    // Update is called once per frame
    void Update()
    {
        if (refresh){
            refresh = false;
            chunks = new Chunk[gridSize.x * gridSize.y];
            generateIsland();
        }
    }
    public void generateIsland(){
        for (int y = 0; y < gridSize.y; y++){
            for (int x = 0; x < gridSize.x; x++){
                Chunk chunk = new();
                Vertex botL;
                Vertex botR;
                Vertex topL;
                Vertex topR;
                Edge right;
                Edge left;
                Edge top;
                Edge bot;
                Vector2 bottomLeft = new Vector2((float)x * chunkSize, (float)y * chunkSize);
                Vector2 bottomRight = new Vector2((float)(x + 1f) * chunkSize, (float)y * chunkSize);
                Vector2 topLeft = new Vector2((float)x * chunkSize, (float)(y + 1f) * chunkSize);
                Vector2 topRight = new Vector2((float)(x + 1f) * chunkSize, (float)(y + 1f) * chunkSize);
                if (y == 0){
                    botR = new Vertex(bottomRight);
                    if (x == 0)
                        botL = new Vertex(bottomLeft);
                    //else
                        //botL = chunks[gridSize.x * y + x].vertices.Find
                    //bot = new Edge(botR, botL, null, chunk);
                    
                }
                if (x == 0){
                    topL = new Vertex(topLeft);
                    //if(botL == null){
                    //  botL = new Vertex(bottomLeft);}
                    //left = new Edge();
                }
                
                chunk.center = new Vector2((float)x * chunkSize, (float)y * chunkSize);
                chunks[gridSize.x * y + x] = chunk;
            }
        }
    }
    public class Vertex {
        //Corners connected to this corner
        public List<Vertex> vertices;
        //Edges portruding from the corner
        public List<Edge> edges;
        //Chunks touching the corner
        public List<Chunk> chunks;
        public Vector2 position;

        public Vertex(Vector2 pos){
            position = pos;
        }
    }
    //Contects two corners and separates two chunks
    public class Edge {
        public Vertex v0;
        public Vertex v1;
        public Chunk c0;
        public Chunk c1;

        public Edge(Vertex first, Vertex second, Chunk cf, Chunk cs){
            v0 = first;
            v1 = second;
            c0 = cf;
            c1 = cs;
        }
    }
    //Has 4 edges, 4 vertices, and 8 neighbours ideally...
    public class Chunk {
        public List<Edge> edges;
        public List<Vertex> vertices;
        public List<Chunk> neighbours;

        public Vector2 center;
        public Texture2D texture;
        public Sprite sprite;

    }
}
