using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class PathToWall : MonoBehaviour
    {
        // -- path --
        [SerializeField] private Vector3[] path = System.Array.Empty<Vector3>();
        public Vector3[] GetPath() => path;
        
        // -- height --
        const float DefaultHeight = 4;
        [SerializeField] private bool useCustomHeight;
        [SerializeField] private float customHeight = 4;
        private float GetHeight() => useCustomHeight ? customHeight : DefaultHeight;
        
        MeshFilter meshFilter;
        
        [SerializeField] private Mesh generatedMesh;

        public MeshFilter MeshFilter
        {
            get
            {
                if (meshFilter) return meshFilter;

                if (TryGetComponent(out meshFilter))
                    return meshFilter;
                
                Debug.LogError($"No Mesh Filter Component in GameObject {name}", gameObject);
                return null;
            }
        }

        private void Awake()
        {
            GenerateNewPath();
            GenerateWallMesh();
        }
        
        [ContextMenu("Generate Path")]
        public void GenerateNewPath()
        {
            path = new Vector3[Random.Range(5, 15)];
            Vector3 startPos = Random.insideUnitSphere * 2;
            startPos.y = 0;
            for (int i = 0; i < path.Length; i++)
            {
                path[i] = startPos;
                startPos += Random.insideUnitSphere * 2;
                startPos.y = 0;
            }
        }

        [ContextMenu("Generate Wall Mesh")]
        public void GenerateWallMesh()
        {
            // ASSIGNMENT HERE

            if (path == null || path.Length < 2)
            {
                throw new Exception("Path must have at least 2 points");
            }
            
            
            //------------------------- Arrays Initialization --------------------------------
            
            //Every point has 2 vertices, one where the point is and one above
            
            Vector3[] vertices = new Vector3[path.Length * 2];
            
            
            int segmentsNumber = path.Length - 1;
            
            
            //Every segment has 2 triangles

            int[] triangles = new int[segmentsNumber * 2 * 3];
            
            //-------------------------------------------------------------------------------
            
            
            
            // ------------------------ Filling Arrays --------------------------------------

            //Every point fills 2 spots in the vertices array - so we follow this int to know where to put the next vertex
            int nextFreeIndexInVertices = 0;
            
            //Every point fills 6 spots in the triangles array - so we follow this int to know where to put the next triangle
            int nextFreeIndexInTriangles = 0;
            
            for (int i = 0; i < path.Length; i++)
            {
                //Vertices - Generate 2 vertices for each point
                
                vertices[nextFreeIndexInVertices] = path[i];
                vertices[nextFreeIndexInVertices + 1] = path[i] + Vector3.up * GetHeight();
                
                
                //Triangles - Generate 2 triangles for each point - skip if it's the last point
                
                if (i == path.Length - 1)
                {
                    break;
                }

                triangles[nextFreeIndexInTriangles] = nextFreeIndexInVertices;
                triangles[nextFreeIndexInTriangles+1] = nextFreeIndexInVertices + 1;
                triangles[nextFreeIndexInTriangles+2] = nextFreeIndexInVertices + 2;
                

                triangles[nextFreeIndexInTriangles+3] = nextFreeIndexInVertices + 1;
                triangles[nextFreeIndexInTriangles+4] = nextFreeIndexInVertices + 3;
                triangles[nextFreeIndexInTriangles+5] = nextFreeIndexInVertices + 2;
                

                nextFreeIndexInVertices += 2;
                nextFreeIndexInTriangles += 6;
            }
            
            //-------------------------------------------------------------------------------
            

            // [uncomment when ready with vertices etc., so that it doesn't throw compilation errors]
             Mesh mesh = new Mesh
             {
                 vertices = vertices,
                 triangles = triangles
             };
             
             mesh.RecalculateNormals();

             MeshFilter.mesh = mesh;

             generatedMesh = mesh;
        }

        [ContextMenu("Destroy Wall Mesh")]
        public void DestroyWallMesh()
        {
            DestroyImmediate(generatedMesh);
            generatedMesh = null;
            MeshFilter.mesh = null;
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Super quick debug to preview how things should be
            
            
            if(path == null || path.Length == 0) return;
            
            for (var i = 0; i < path.Length; i++)
                UnityEditor.Handles.Label(path[i], $"{i}");

            Gizmos.color = Color.red;
            float height = GetHeight();
            for (var i = 0; i < path.Length-1; i++)
            {
                var btmLeft = path[i];
                var btmRight = path[i+1];
                var topLeft = btmLeft + Vector3.up * height;
                var topRight = btmRight + Vector3.up * height;
                Gizmos.DrawLine(btmLeft, btmRight);
                Gizmos.DrawLine(btmLeft, topLeft);
                Gizmos.DrawLine(btmRight, topRight);
                Gizmos.DrawLine(topLeft, topRight);
                
            }
            Gizmos.color = Color.white;
        }
#endif
    }

}