using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

public class Octree : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    private Vector3 playerCurrentPosition;
    private bool isCheckDistance;
    [SerializeField]
    private bool drawGizno;
    [SerializeField]
    private int maxLodLevel = 3;
    [SerializeField]
    private float radius = 1000;

    /* Render */
    public Material material;

    private List<OctreeNode> mainActiveNodes;
    private NativeHashMap<float3,OctreeNode> outActiveNodes;

    void Start()
    {
        mainActiveNodes = new List<OctreeNode>();
        CheckDistance();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(playerCurrentPosition, player.transform.position) > 10f && !isCheckDistance)
        {
            playerCurrentPosition = player.transform.position;
            isCheckDistance = true;

            CheckDistance();
        }

    }

    void OnDestroy()
    {
        // outActiveNodes.Dispose();
    }

    private void CheckDistance()
    {
        outActiveNodes = new NativeHashMap<float3, OctreeNode>(1, Allocator.Persistent);
        mainActiveNodes.Clear();

        var job4 = new FirstDistanceCheck()
        {
            maxLodLevel = maxLodLevel,
            radius = radius,
            activeNodesIndex = outActiveNodes,
            playerPosition = new float3(player.transform.position.x, player.transform.position.y, player.transform.position.z),
        };

        JobHandle jobHandle4 = job4.Schedule();

        jobHandle4.Complete();

        foreach (var item in outActiveNodes)
        {
            mainActiveNodes.Add(item.Value);
            //CreateIsosurface(item.Value);
        }

        if(jobHandle4.IsCompleted) {
            isCheckDistance = false;
        }

        outActiveNodes.Dispose();
    }

    [BurstCompile]
    struct FirstDistanceCheck : IJob
    {
        public int maxLodLevel;
        public float radius;
        public NativeHashMap<float3, OctreeNode> activeNodesIndex;
        public float3 playerPosition;

        private static float Distance(float3 a, float3 b)
        {
            float3 vector = new float3(a.x - b.x, a.y - b.y, a.z - b.z);
            return math.sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        float3 GetPosition(int index, float3 parentPosition, float delta)
        {
            float3 position = float3.zero;

            switch (index)
            {
                case 0:
                    position = new float3(parentPosition.x - delta, parentPosition.y - delta, parentPosition.z - delta);
                    break;
                case 1:
                    position = new float3(parentPosition.x - delta, parentPosition.y - delta, parentPosition.z + delta);
                    break;
                case 2:
                    position = new float3(parentPosition.x + delta, parentPosition.y - delta, parentPosition.z - delta);
                    break;
                case 3:
                    position = new float3(parentPosition.x + delta, parentPosition.y - delta, parentPosition.z + delta);
                    break;
                case 4:
                    position = new float3(parentPosition.x - delta, parentPosition.y + delta, parentPosition.z - delta);
                    break;
                case 5:
                    position = new float3(parentPosition.x - delta, parentPosition.y + delta, parentPosition.z + delta);
                    break;
                case 6:
                    position = new float3(parentPosition.x + delta, parentPosition.y + delta, parentPosition.z - delta);
                    break;
                case 7:
                    position = new float3(parentPosition.x + delta, parentPosition.y + delta, parentPosition.z + delta);
                    break;
            }

            return position;
        }

        float3 GetVoxelBase(int cIndex)
        {
            float3 position = float3.zero;

            switch (cIndex)
            {
                case 0:
                    position = new float3(0, 0, 0);
                    break;
                case 1:
                    position = new float3(0, 0, 1);
                    break;
                case 2:
                    position = new float3(1, 0, 0);
                    break;
                case 3:
                    position = new float3(1, 0, 1);
                    break;
                case 4:
                    position = new float3(0, 1, 0);
                    break;
                case 5:
                    position = new float3(0, 1, 1);
                    break;
                case 6:
                    position = new float3(1, 1, 0);
                    break;
                case 7:
                    position = new float3(1, 1, 1);
                    break;
            }
            return position;
        }

        private void CheckClosesNodes(int index, float3 position, float3 voxelPosition, byte lodLevel, float size)
        {
            float diagonal = Mathf.Sqrt(2 * (size * size));

            if (lodLevel < maxLodLevel && Distance(playerPosition, position) < diagonal)
            {
                float delta = size / 4;

                for (int i = 0; i < 8; i++)
                {
                    float3 voxelPosition2 = voxelPosition + (GetVoxelBase(i) * size / 2);
                    CheckClosesNodes(i, GetPosition(i, position, delta), voxelPosition2, (byte)(lodLevel + 1), size / 2);
                }
            }
            else
            {
                OctreeNode node = new OctreeNode(position, voxelPosition, size, lodLevel);
                activeNodesIndex.TryAdd(position, node);
            }
        }

        public void Execute()
        {

            CheckClosesNodes(-1, new float3(radius / 2, radius / 2, radius / 2), float3.zero, 0, radius);
        }
    }

    void OnDrawGizmos()
    {
        if (drawGizno && mainActiveNodes != null && mainActiveNodes.Count > 0)
        {
            foreach (var node in mainActiveNodes)
            {
                switch (node.lodLevel)
                {
                    case 0:
                        Gizmos.color = Color.red;
                        break;
                    case 1:
                        Gizmos.color = Color.green;
                        break;
                    case 2:
                        Gizmos.color = Color.blue;
                        break;
                    case 3:
                        Gizmos.color = Color.yellow;
                        break;
                    case 4:
                        Gizmos.color = Color.black;
                        break;
                    case 5:
                        Gizmos.color = Color.white;
                        break;
                    case 6:
                        Gizmos.color = Color.cyan;
                        break;
                    default:
                        Gizmos.color = Color.red;
                        break;
                }

                Vector3 s = new Vector3(node.size, node.size, node.size);
                Vector3 centerP = new Vector3(node.position.x, node.position.y, node.position.z);
                Gizmos.DrawWireCube(centerP, s);
            }
        }
    }

}
