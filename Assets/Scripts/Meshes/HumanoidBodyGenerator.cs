using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
/**
The winding order for quads is counter clockwise since i blindly trusted chat gpt
**/
public class HumanoidBodyGenerator : MonoBehaviour
{
    public bool generate = false;

    public GameObject body;
    public Rigidbody rbody;
    public Mesh mesh;
    public Material uvMat;

    public float width = 0f;
    public float depth = 0f;
    public float maxheight = 0f;
    public  float minheight = 0f;

    private List<Vector3> vertices = new();
    private List<Vector3> normals = new();
    private List<Vector2> uvs = new();
    private List<int> triangles = new();
    private List<BoneWeight1> weights = new();
    private List<byte> bones = new();
    private List<Transform> skeleton = new();
    private List<Matrix4x4> bindposes = new();


    public Transform leftfootiktarget {get => currentBody.leftfootiktarget; set => currentBody.leftfootiktarget = value; }
    public Transform rightfootiktarget {get => currentBody.rightfootiktarget; set => currentBody.rightfootiktarget = value; }
    public Transform lefthandiktarget {get => currentBody.lefthandiktarget; set => currentBody.lefthandiktarget = value; }
    public Transform righthandiktarget {get => currentBody.righthandiktarget; set => currentBody.righthandiktarget = value; }
    public Transform headiktarget {get => currentBody.headiktarget; set => currentBody.headiktarget = value; }

    public HumanoidBody currentBody;

    // Update is called once per frame
    void Update()
    {
        if (generate){
            generate = false;
            this.Generate(null);
        }
    }

    void reset(GameObject p){
        width = 0f;
        depth = 0f;
        maxheight = 0f;
        minheight = 0f;
        if (p != null) body = p;
        else {
            body = new GameObject("character body");
            body.transform.parent = null;
        }
        currentBody = body.AddComponent<HumanoidBody>();
        rbody = null;
        mesh = new Mesh();

         List<Vector3> vertices = new();
         List<Vector3> normals = new();
         List<Vector2> uvs = new();
         List<int> triangles = new();
         List<BoneWeight1> weights = new();
         List<byte> bones = new();
         List<Transform> skeleton = new();
         List<Matrix4x4> bindposes = new();
    }

    public HumanoidBody Generate(GameObject p)
    {
        reset(p);
        //What renders our object
        SkinnedMeshRenderer meshRenderer = body.AddComponent<SkinnedMeshRenderer>();
        meshRenderer.updateWhenOffscreen = true;
        meshRenderer.sharedMesh = mesh;
        //What holds the mesh
        MeshFilter meshFilter = body.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        meshFilter.mesh = mesh;

        //Our character (game logic physics body for walking and such)
        BoxCollider boundingBoxCollider = body.AddComponent<BoxCollider>();
        rbody = body.AddComponent<Rigidbody>();
        rbody.isKinematic = false;
        rbody.useGravity = false; //Since our player controller want to handle this differently....
        rbody.angularDrag = 10f;
        rbody.excludeLayers = LayerMask.GetMask("Default");
        rbody.constraints = RigidbodyConstraints.FreezeRotation;
        
        /**The idea of the root joint was when ragdolling to keep the collider near the ragdoll, but that doesnt make sense with animations
        //Also having the joint inhibits the character capsule from being a rigidbody when the bones are kinematic
        //CharacterJoint rootJoint = body.AddComponent<CharacterJoint>();
        //Bone.setJointLimits(rootJoint);
        **/

        //*****The Torso.....
        Bone pelvis = generateTorso();
        currentBody.root = pelvis;
        currentBody.body = body;
        //rootJoint.connectedBody = pelvis.rb;
        boundingBoxCollider.size = new Vector3(width, maxheight - minheight, depth);
        boundingBoxCollider.center = new Vector3(0f, (maxheight + minheight) *0.5f ,0f);
        

        //*****Create the mesh from our lists***/
        mesh.Clear();
        mesh.SetVertices(vertices.ToArray());
        mesh.triangles = triangles.ToArray();
        mesh.SetNormals(normals.ToArray());
        mesh.SetUVs(0, uvs.ToArray());
        NativeArray<byte> numBones = new NativeArray<byte>(bones.ToArray(), Allocator.Temp);
        NativeArray<BoneWeight1> boneWeights = new NativeArray<BoneWeight1>(weights.ToArray(), Allocator.Temp);
        mesh.SetBoneWeights(numBones, boneWeights);
        numBones.Dispose();
        boneWeights.Dispose();
        foreach (Transform obj in skeleton){
            bindposes.Add(obj.worldToLocalMatrix * body.transform.localToWorldMatrix);
        }
        mesh.bindposes = bindposes.ToArray();
        
        //**Setup the meshrender?? half this stuff isnt necessary besides setting the material and the bones (idk why im calling it a skeleton)

        meshRenderer.rootBone = pelvis.transform;
        meshRenderer.material = uvMat;
        meshRenderer.bones = skeleton.ToArray();
        return currentBody;
    }


    public Bone generateTorso(float pw = 0.35f, float pd = 0.1f, float ph = 0.15f,
                    float aw = 0.30f, float ad = 0.08f, float ah = 0.3f, 
                    float cw = 0.45f, float cd = 0.2f, float ch = 0.3f,
                    float nw = 0.1f, float nd = 0.1f, float nh = 0.1f,
                    float jw = 0.12f, float jd = 0.2f, float hh = 0.2f){
        //Updating our body collider's bounding box
        maxheight += ph + ah + ch + nh + hh;
        currentBody.shoulderwidth = cw;
        currentBody.hipwidth = pw;
        currentBody.headheight = maxheight;
        width += pw;
        depth += pd;
        //Generating the rectangle rings that define the body of the character
        Quad pquad = new Quad(pw, pd);
        Quad aquad = new Quad(aw, ad, new Vector3(0f, ph ,0f));
        Quad cquad = new Quad(cw, cd, new Vector3(0f, ph + ah, 0f));
        Quad c2quad = new Quad(cw, cd, new Vector3(0f, ph + ah + 0.6f * ch, 0f));
        Quad nquad = new Quad(nw, nd, new Vector3(0f, ph + ah + ch, 0f));
        Quad hquad = new Quad(nw, nd, new Vector3(0f, ph + ah + ch + nh, 0f));
        Quad jaw = new Quad(jw, jd, new Vector3(0f, ph + ah + ch + nh, 0f));
        Quad crown = new Quad(jw, jd, new Vector3(0f, ph + ah + ch + nh + hh, 0f));
        
        //Pelvis
        Bone pelvis = Bone.NewBone("pelvis", Vector3.up * ph, pw, pd);
        pelvis.transform.parent = body.transform;
        pelvis.transform.localPosition = new Vector3(0f, ph / 2f, 0f);
        updateLists(vertices, uvs, normals, triangles, weights, bones, generateRectPrism(getBoneMap(addBone(pelvis)), pquad,aquad, vertices.Count, 8));
        
        //Legs
        Bone rightthigh = generateLeg(pelvis, new Vector3(pw * 0.4f, 0f, 0f), 1f, pw, ph);
        Bone leftthigh = generateLeg(pelvis, new Vector3(pw * -0.4f, 0f, 0f), -1f, pw, ph);

        //Abdomen
        Bone abdomen = Bone.NewBone("abdomen", Vector3.up * ah, aw, ad);
        pelvis.attachBone(abdomen, -15f, 15f);
        updateLists(vertices, uvs, normals, triangles, weights, bones, generateRectPrism(getBoneMap(addBone(abdomen)), aquad, cquad, vertices.Count, 8));

         //Chest
        Bone chest = Bone.NewBone("chest", Vector3.up * ch, cw, cd);
        abdomen.attachBone(chest, -15f, 15f);
        var bid = addBone(chest);
        updateLists(vertices, uvs, normals, triangles, weights, bones,  generateRectPrism(getBoneMap(bid), cquad,c2quad, vertices.Count, 4));
        updateLists(vertices, uvs, normals, triangles, weights, bones, generateRectPrism(getBoneMap(bid), c2quad,nquad, vertices.Count, 4));
        
        //Arms
        Bone rightarm = generateArm(chest, new Vector3(cw * 0.5f, ph + ah + (ch * 0.5f), 0f), 1f, cw);
        Bone leftarm = generateArm(chest, new Vector3(cw * -0.5f, ph + ah + (ch * 0.5f), 0f), -1f, cw);     
        
        //Neck
        Bone neck = Bone.NewBone("neck", Vector3.up * nh, nw, nd);
        chest.attachBone(neck,-30f, 30f, 30f, 30f);
        updateLists(vertices, uvs, normals, triangles, weights, bones, generateRectPrism(getBoneMap(addBone(neck)), nquad, hquad, vertices.Count, 8));
        
        //Head
        Bone head = Bone.NewBone("head", Vector3.up * hh, jw, jd);
        neck.attachBone(head,-10f, 10f, 30f, 30f);
        updateLists(vertices, uvs, normals, triangles, weights, bones, generateRectPrism(getBoneMap(addBone(head)), jaw, crown, vertices.Count, 8));
        CustomIK ik = head.AddComponent<CustomIK>();
        ik.ChainLength = 2;
        headiktarget = new GameObject("head target").transform;
        headiktarget.parent = body.transform;
        headiktarget.position = head.tip;
        ik.Target = headiktarget;
        


        //******THE ORGANS //TODO: Give the organs separate meshes and stuff
        GameObject heart = new GameObject("Default Organ");
        var heartRB = heart.AddComponent<Rigidbody>();
        chest.attachRB(heartRB, new Vector3(-0.2f  * cw, 0f, 0f));
        //refactor later
        SphereCollider organCollider = heart.AddComponent<SphereCollider>();
        organCollider.radius = 0.05f;
        heartRB.isKinematic = true;
        heartRB.useGravity = false;
        
        
        return pelvis;
    }
    public Bone generateArm(Bone chest, Vector3 origin, float direction = 1f, float cw = 0.45f,
            float ual = 0.3f, float uah = 0.09f, float uaw= 0.07f,
            float fal = 0.24f, float fah = 0.05f, float faw= 0.06f,
            float hl = 0.1f, float hh = 0.03f, float hw= 0.1f){
        
        currentBody.armlength = ual + fal + hl;
        Vector3 center = origin;
        //Upper Arm
        center.x += ual * 0.5f * direction;
        (Quad topupperarm, Quad botupperarm)  = getBox(ual, uaw, uah, center);
        Bone upperarm = Bone.NewBone("upperarm", Vector3.right * direction * ual, uaw, uah);
        chest.attachBone(upperarm, new Vector3(cw * 0.5f * direction, 0f, 0f), -45f, 45f);
        updateLists(vertices, uvs, normals, triangles, weights, bones, generateRectPrism(getBoneMap(addBone(upperarm)), botupperarm,topupperarm, vertices.Count, 4));

        //Forearm
        center.x += (fal + ual) * 0.5f * direction;
        (Quad topforearm, Quad botforearm)  = getBox(fal, faw, fah, center);
        Bone forearm = Bone.NewBone("forearm", Vector3.right * direction * fal, faw, fah);
        upperarm.attachBone(forearm,  0f, 0f, 90f);
        updateLists(vertices, uvs, normals, triangles, weights, bones, generateRectPrism(getBoneMap(addBone(forearm)), botforearm,topforearm, vertices.Count, 4));

        //Hand
        center.x += (fal + hl) * 0.5f * direction;
        (Quad tophand, Quad bothand)  = getBox(hl, hw, hh, center);
        Bone hand = Bone.NewBone("hand", Vector3.right * direction * hl, hw, hh);
        forearm.attachBone(hand, -45f, 45f, 45f, 5f);
        updateLists(vertices, uvs, normals, triangles, weights, bones, generateRectPrism(getBoneMap(addBone(hand)), bothand,tophand, vertices.Count, 4));
        CustomIK ik = hand.AddComponent<CustomIK>();
        //If chain length is 2 we should be using trig instead of Fabrik
        ik.ChainLength = 3;
        if (direction == -1f){
            lefthandiktarget = new GameObject("lhand target").transform;
            lefthandiktarget.parent = body.transform;
            lefthandiktarget.position = hand.tip;
            ik.Target = lefthandiktarget;
        } else {
            righthandiktarget = new GameObject("rhand target").transform;
            righthandiktarget.parent = body.transform;
            righthandiktarget.position = hand.tip;
            ik.Target = righthandiktarget;
        }
        return upperarm;
    }
    
    public Bone generateLeg(Bone pelvis, Vector3 origin, float direction = 1f, float pw = 0.35f, float ph = 0.15f,
            float tw = 0.15f, float td = 0.15f, float th = 0.4f,
            float lw = 0.1f, float ld = 0.1f, float lh = 0.3f,
            float fw = 0.12f, float fd = 0.05f, float fh = 0.25f){
        currentBody.leglength = th + lh + fd;
        if (direction == 1f){
            minheight -=  th + lh + fd;
        }
        

        Vector3 center = origin;
        //Thigh
        center.y -= th * 0.5f;
        (Quad topthigh, Quad botthigh)  = getBox(tw, td, th, center);
        Bone thigh = Bone.NewBone("thigh", Vector3.down * th, tw, td);
        pelvis.attachBone(thigh, new Vector3(pw * direction * 0.4f, 0f, -0.5f * ph), -45f, 45f);
        updateLists(vertices, uvs, normals, triangles, weights, bones, generateRectPrism(getBoneMap(addBone(thigh)), botthigh,topthigh, vertices.Count, 4));

        //Lower Leg
        center.y -= (th + lh) * 0.5f;
        (Quad topleg, Quad botleg)  = getBox(lw, ld, lh, center);
        Bone leg = Bone.NewBone("lowerleg", Vector3.down * lh, lw, ld);
        thigh.attachBone(leg, 0f, 90f);
        updateLists(vertices, uvs, normals, triangles, weights, bones, generateRectPrism(getBoneMap(addBone(leg)), botleg,topleg, vertices.Count, 4));
        
        //Foot
        center.y -= (lh + fd) * 0.5f;
        center.z += (fh - ld) * 0.5f;
        (Quad topfoot, Quad botfoot)  = getBox(fw, fh, fd, center);
        Bone foot = Bone.NewBone("foot", Vector3.forward * fh, fw, fd);
        leg.attachBone(foot, new Vector3(0f, -0.5f * ld, 0.5f * (lh + fd) ),  -15f, 15f, 15f, 15f);
        updateLists(vertices, uvs, normals, triangles, weights, bones, generateRectPrism(getBoneMap(addBone(foot)), botfoot,topfoot, vertices.Count, 4));

        CustomIK ik = leg.AddComponent<CustomIK>();
        ik.ChainLength = 2;
        if (direction == -1f){
            leftfootiktarget = new GameObject("lfoot target").transform;
            leftfootiktarget.parent = body.transform;
            leftfootiktarget.position = leg.tip;
            ik.Target = leftfootiktarget;
        } else {
            rightfootiktarget = new GameObject("rfoot target").transform;
            rightfootiktarget.parent = body.transform;
            rightfootiktarget.position = leg.tip;
            ik.Target = rightfootiktarget;
        }

        return thigh;
    }
    
    /** Whoever said the unity documentation is good is misleading, I've spent a handful of hours trying to figure out boneweights just to realize that despite finding forum posts saying they get normalized, they do not in fact get normalized. This would all be easily avoided if the documentation actually explained the engine implementation, but no we cant have more than a sentence explaining the obvious on the docs because why would we want anyone to do anything with our engine besides mash assets together and ship another variation of the same game? 
    I am not even sure what it means to normalize because for a single bone weight 1 vs weight 10 is the same but that doesnt seem to be the case for several bones
    I think for this implementation it might be easier to just add weights in range 0 to 1, then sum all the weights and divide by the sum**/
    public Func<int, float, float, List<BoneWeight1>> getBoneMap(int b = 0, int bot = -1, int top = -1, int left = -1, int right = -1, int front = -1, int back = -1){
        Func<int, float, float, List<BoneWeight1>> bm = (face, u, v) => 
            {
            List<BoneWeight1> weights = new();
            BoneWeight1 weight = new();
            float sum = 0f;
            Action<int, float> addWeight = (id, w) => {
                weight.boneIndex = id;
                weight.weight = w;
                sum += w;
                weights.Add(weight);
            };
            addWeight(b, 1f);
            switch (face){
                case 0: //front
                    if(front != -1) addWeight(front, 1f);
                    if(left != -1) addWeight(left, 1f - u);
                    if(right != -1) addWeight(right, u);
                    goto case -1;
                case 1: //right
                    if(right != -1) addWeight(right, 1f);
                    if (front != -1)addWeight(front, 1f - u);
                    if (back != -1)addWeight(back, u);
                    goto case -1;
                case 2: //left
                    if(left != -1) addWeight(left, 1f);
                    if (front != -1)addWeight(front, u);
                    if (back != -1)addWeight(back, 1f - u);
                    goto case -1;
                case 3: //back
                    if(back != -1) addWeight(back, 1f);
                    if (left != -1) addWeight(left, u);
                    if (right != -1)addWeight(right, 1f - u);
                    goto case -1;
                case 4: //top
                    if (top != -1)addWeight(top, 1f);
                    if (left != -1)addWeight(left, 1f - u);
                    if (right != -1)addWeight(right, u);
                    goto case -2;
                case 5: //bottom
                    if (bot != -1)addWeight(bot, 1f);
                    if (left != -1)addWeight(left, u);
                    if (right != -1)addWeight(right, 1f - u);
                    goto case -2;
                case -1: //The sides used for universal top bot code
                    if (top != -1)addWeight(top, v);
                    if (bot != -1)addWeight(bot, 1f - v);
                    break;
                case -2://the top/bot sides used for universal front back code since they both have v starting in the front
                    if (front != -1)addWeight(front, 1f - v);
                    if (back != -1)addWeight(back, v);
                    break;
                default:
                    break;  
            }
            //Normalize the weights by dividing by the sum, i think the new list is needed since weights are a struct so are by value so for each editing them doesnt change the list values??
            //But i thought foreach loops were supposed to be by reference instead, if i used a normal array i wouldnt have this problem, but i didnt...
            List<BoneWeight1> finalweights = new();
            foreach (var w in weights){
                weight.boneIndex = w.boneIndex;
                weight.weight = w.weight / sum;
                finalweights.Add(weight);
            }

            return finalweights;
        };
        return bm;
    }
    private int addBone(GameObject obj){
        skeleton.Add(obj.transform);
        return skeleton.Count - 1;
    }
    private int addBone(Component bone){
        return addBone(bone.gameObject);
    }
    
    public (List<Vector3>, List<Vector2>, List<Vector3>, List<int>, List<BoneWeight1>, List<byte>) 
        generateRectPrism(Func<int, float, float, List<BoneWeight1>> bonemap, Quad bot, Quad top, int index = 0, int steps = 8){
        List<Vector3> verts = new();
        List<Vector3> normals = new();
        List<Vector2> UVs = new();
        List<int> tris = new();
        List<byte> bones = new();
        List<BoneWeight1> weights = new();
        //Front Face
        updateLists(verts, UVs, normals, tris, weights, bones,  
            generateQuad(bonemap, 0, 
                new Quad(top.corners[3], top.corners[2], bot.corners[2], bot.corners[3]), 
                index + verts.Count, steps));
        //Right
        updateLists(verts, UVs, normals, tris, weights, bones,  
            generateQuad(bonemap, 1, 
                new Quad(top.corners[0], top.corners[3], bot.corners[3], bot.corners[0]), 
                index + verts.Count, steps));
        //Left
        updateLists(verts, UVs, normals, tris, weights, bones, 
            generateQuad(bonemap, 2, 
                new Quad(top.corners[2], top.corners[1], bot.corners[1], bot.corners[2]), 
                index + verts.Count, steps));
        //back
        updateLists(verts, UVs, normals, tris, weights, bones, 
            generateQuad(bonemap, 3, 
                new Quad(top.corners[1], top.corners[0], bot.corners[0], bot.corners[1]),
                index + verts.Count, steps));
        //top
        updateLists(verts, UVs, normals, tris, weights, bones, 
            generateQuad(bonemap, 4, top, index + verts.Count, steps));
        //bot
        updateLists(verts, UVs, normals, tris, weights, bones,  
            generateQuad(bonemap, 5, 
                new Quad(bot.corners[1], bot.corners[0], bot.corners[3], bot.corners[2]),
                index + verts.Count, steps));    
        return(verts, UVs, normals, tris, weights, bones);
    }

    /**
    makes a plane along the four coordinates with #steps evenly spaced...
    **/
    public void updateLists( List<Vector3> verts,  List<Vector2> UVs,   List<Vector3> normals,  List<int> tris,  List<BoneWeight1> weights,  List<byte> bones,
            (List<Vector3> v, List<Vector2> u, List<Vector3> n, List<int> t, List<BoneWeight1> w, List<byte> b) data){
        verts.AddRange<Vector3>(data.v);
        UVs.AddRange<Vector2>(data.u);
        normals.AddRange<Vector3>(data.n);
        tris.AddRange<int>(data.t);
        weights.AddRange<BoneWeight1>(data.w);
        bones.AddRange<byte>(data.b);
    }

    public (List<Vector3>, List<Vector2>, List<Vector3>, List<int>, List<BoneWeight1>, List<byte>) generateQuad(Func<int, float, float, List<BoneWeight1>> bonemap, int face, Quad quad, int index = 0, int steps = 10){
        
        Vector3 tr = quad.corners[0];
        Vector3 tl = quad.corners[1];
        Vector3 bl = quad.corners[2];
        Vector3 br = quad.corners[3];


        List<Vector3> verts = new();
        List<Vector3> normals = new();
        List<Vector2> UVs = new();
        List<int> tris = new();
        List<BoneWeight1> weights = new();
        List<byte> bones = new();
        Vector3 dvleft = (tl - bl) / steps;
        Vector3 dvright = (tr - br) / steps;
        Vector3 rowmin = bl;
        Vector3 rowmax = br;
        Vector3 position = rowmin;
        Vector3 normal =  Vector3.Cross(tl - bl, br - bl);
        

        for (int v = 0; v <= steps; v++){
            Vector3 du = (rowmax - rowmin) / steps;
            for (int u = 0; u <= steps; u++){
                verts.Add(position);
                normals.Add(normal);
                float ur = (float) u / (steps);
                float vr = (float) v / (steps);
                UVs.Add(new Vector2(ur, vr));
                List<BoneWeight1> w = bonemap(face, ur, vr);
                weights.AddRange(w);
                bones.Add((byte) w.Count);
                position += du;
                if (u != 0 && v != 0){
                    tris.Add(index - 1);
                    tris.Add(index - (steps + 1));
                    tris.Add(index - (steps + 2) );

                    tris.Add(index - 1);
                    tris.Add(index );
                    tris.Add(index - (steps + 1));
                }
                index++;
            }
            
            rowmin += dvleft;
            position = rowmin;
            rowmax += dvright;
        }
        return (verts, UVs, normals, tris, weights, bones);
    }
    
    public (Quad, Quad) getBox(float width, float depth, float height, Vector3 center){
        center.y -= height * 0.5f;
        Quad bot = new(width, depth, center);
        center.y += height;
        Quad top = new(width, depth, center);
        return (top, bot);
    }
    public struct Quad{
        public Vector3[] corners;
        public float width;
        public float depth;

        public Quad(Vector3 tr, Vector3 tl, Vector3 bl, Vector3 br){
            corners = new Vector3[4] {tr, tl, bl, br};
            width = Mathf.Max((tr - tl).magnitude, (br - bl).magnitude);
            depth = Mathf.Max((tr - br).magnitude, (tl - bl).magnitude);
        }
        public Quad(float w, float d){
            width = w;
            depth = d;
            corners = new Vector3[4];
            corners[0] = new Vector3(0.5f * w, 0f, 0.5f * d); //tr
            corners[1] = new Vector3(-0.5f * w, 0f, 0.5f * d); //tl
            corners[2] = new Vector3(-0.5f * w, 0f, -0.5f * d); //bl
            corners[3] = new Vector3(0.5f * w, 0f, -0.5f * d); //br
            
        }
        public Quad(float w, float d, Vector3 center){
            width = w;
            depth = d;
            corners = new Vector3[4];
            corners[0] = center + new Vector3(0.5f * w, 0f, 0.5f * d); //tr
            corners[1] = center + new Vector3(-0.5f * w, 0f, 0.5f * d); //tl
            corners[2] = center + new Vector3(-0.5f * w, 0f, -0.5f * d); //bl
            corners[3] = center + new Vector3(0.5f * w, 0f, -0.5f * d); //br
        }

        public Quad(float w, float d, float h){
            width = w;
            depth = d;
            corners = new Vector3[4];
            corners[0] = new Vector3(0.5f * w, h, 0.5f * d); //tr
            corners[1] = new Vector3(-0.5f * w, h, 0.5f * d); //tl
            corners[2] = new Vector3(-0.5f * w, h, -0.5f * d); //bl
            corners[3] = new Vector3(0.5f * w, h, -0.5f * d); //br
            
        }
    }

}

