#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;

    /// <summary>
    /// Fabrik IK Solver
    /// </summary>
    public class CustomIK : MonoBehaviour
    {
        /// <summary>
        /// Chain length of bones
        /// </summary>
        public int ChainLength = 2;

        /// <summary>
        /// Target the chain should bent to
        /// </summary>
        public Transform Target;
        public Transform Pole;

        /// <summary>
        /// Solver iterations per update
        /// </summary>
        [Header("Solver Parameters")]
        public int Iterations = 10;

        /// <summary>
        /// Distance when the solver stops
        /// </summary>
        public float Delta = 0.001f;

        /// <summary>
        /// Strength of going back to the start position.
        /// </summary>
        [Range(0, 1)]
        public float SnapBackStrength = 1f;

        protected float CompleteLength;
        protected Bone[] Bones;
        protected Vector3[] Positions;
   

        protected Vector3 startpos;
        protected Quaternion startrot;
        protected Vector3 goalpos;
        protected Quaternion goalrot;

        private bool initialized = false;


        // Start is called before the first frame update
        void Awake()
        {   
            //OG calls init on awake, since i want to adjust targets before init i need to call init manually
            //Init();
        }

        void Init()
        {
            initialized = true;
            //initial arrays
            Bones = new Bone[ChainLength];
            Positions = new Vector3[ChainLength + 1];
            
            //init target
            if (Target == null)
            {
                Target = new GameObject(gameObject.name + " Target").transform;
                Target.transform.position = transform.position;
                Target.transform.rotation = transform.rotation;
            }

        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (!initialized) Init();//so i dont have to manually initialize
            ResolveIK();
        }
        //For 2 bone chains automatically defer to 2-bone trig solution
        private void Resolve2Bone(){
            //Basically we have bones a to b, b to c and target = c
            //We want ac^2 = ab^2 + bc^2, create a midpoint along ac = d yields:
            //ad^2 + db^2 = ab^2 and dc^2 + db^2 = bc^2
        }

        private void ResolveIK()
        {
            if (Target == null)
                return;
            if (Bones.Length != ChainLength)
                Init();
            if(ChainLength == 2){
                Resolve2Bone();
                return;
            }
            //init data
            //start at the end effector
            var current = GetComponent<Bone>();
            CompleteLength = 0f;
            var index = Bones.Length - 1;
            Positions[index + 1] = current.tip;
            while( index >= 0)
            {
                if (current == null)throw new UnityException("The chain value is longer than the ancestor chain! Excess length:" + (index + 1));
                Bones[index] = current;
                Positions[index] = current.root;
                CompleteLength += current.length;

                current = current.parent;
                index--;
            } 
            startpos = Bones[0].root;
            startrot = Bones[0].transform.rotation;
            goalpos = Target.position;
            goalrot = Target.rotation;            

            //1st is possible to reach?
            if (ChainLength * ChainLength + Delta <= (goalpos - startpos).sqrMagnitude){
                //just strech it
                var direction = (goalpos - startpos).normalized;
                //set everything after root
                for (int i = 1; i < Positions.Length; i++)
                    Positions[i] = Positions[i - 1] + (direction * Bones[i - 1].length);
            } else {
                
                
                for (int iteration = 0; iteration < Iterations; iteration++){
                    //back
                    Positions[Positions.Length - 1] = goalpos; //Set the final position to the goal
                    for (int i = Positions.Length - 2; i > 0; i--) //Go from the second to last position to the second position
                    {
                        //Set current point to length away from previous point towards next point
                        Positions[i] = Positions[i + 1] + (Positions[i] - Positions[i + 1]).normalized * Bones[i].length;
                    }
                    //forward
                    for (int i = 1; i < Positions.Length; i++)
                        Positions[i] = Positions[i - 1] + (Positions[i] - Positions[i - 1]).normalized * Bones[i - 1].length;
                    

                    //close enough?
                    if ((goalpos - Positions[Positions.Length - 1]).sqrMagnitude < Delta * Delta)
                        break;
                }
            }

            //move towards pole
            if (Pole != null)
            {
                var polePosition = Pole.position;
                for (int i = 1; i < Positions.Length - 1; i++)
                {
                    var plane = new Plane(Positions[i + 1] - Positions[i - 1], Positions[i - 1]);
                    var projectedPole = plane.ClosestPointOnPlane(polePosition);
                    var projectedBone = plane.ClosestPointOnPlane(Positions[i]);
                    var angle = Vector3.SignedAngle(projectedBone - Positions[i - 1], projectedPole - Positions[i - 1], plane.normal);
                    Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i - 1]) + Positions[i - 1];
                }
            }
            //set position & rotation
            for (int i = 0; i < Bones.Length; i++)
            {
                /** add a way to blend several ik chains together...**/
                Bones[i].direction = Positions[i + 1] - Positions[i]; 
                Bones[i].root = Positions[i];
            }
        }
        public void ResetTarget(){
            Target.position = transform.position;
            Target.rotation = transform.rotation;
        }

        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Bone current = GetComponent<Bone>();
            for (int i = 0; i < ChainLength && current != null; i++)
            {
                var scale = current.length * 0.1f;
                Handles.matrix = Matrix4x4.TRS(current.root, Quaternion.FromToRotation(Vector3.up, current.direction), new Vector3(scale, current.length, scale));
                Handles.color = Color.green;
                Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
                current = current.parent;
            }
#endif
        }

    }
