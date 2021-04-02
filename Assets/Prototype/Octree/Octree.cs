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

    private static readonly int3[] voxelBasePosition = new int3[8]{
        new int3(0, 0, 0),
        new int3(0, 0, 1),
        new int3(1, 0, 0),
        new int3(1, 0, 1),
        new int3(0, 1, 0),
        new int3(0, 1, 1),                   
        new int3(1, 1, 0),
        new int3(1, 1, 1)
    };

    private static readonly int3[] deltaSigns = new int3[8]{
        new int3(-1, -1, -1),
        new int3(-1, -1, 1),
        new int3(1, -1, -1),
        new int3(1, -1, 1),
        new int3(-1, 1, -1),
        new int3(-1, 1, 1),                   
        new int3(1, 1, -1),
        new int3(1, 1, 1)
    };

    /* Render */
    public Material material;

    private List<OctreeNode> mainActiveNodes;
    private NativeHashMap<float3,OctreeNode> outActiveNodes;

    void Start()
    {
        mainActiveNodes = new List<OctreeNode>();
        CheckDistance();
    }

    void Update()
    {
        // if player moved more than 10 meter
        if (Vector3.Distance(playerCurrentPosition, player.transform.position) > 10f && !isCheckDistance)
        {
            playerCurrentPosition = player.transform.position;
            isCheckDistance = true;

            CheckDistance();
        }
    }

    private void CheckDistance()
    {
        outActiveNodes = new NativeHashMap<float3, OctreeNode>(1, Allocator.Persistent);
        mainActiveNodes.Clear();

        var job = new FirstDistanceCheck()
        {
            maxLodLevel = maxLodLevel,
            radius = radius,
            activeNodesIndex = outActiveNodes,
            playerPosition = new float3(player.transform.position.x, player.transform.position.y, player.transform.position.z),
        };

        JobHandle jobHandle = job.Schedule();

        jobHandle.Complete();

        foreach (var item in outActiveNodes)
        {
            mainActiveNodes.Add(item.Value);
        }

        if(jobHandle.IsCompleted) {
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

        float3 GetPosition(int index, float3 parentPosition, float delta)
        {
            int3 deltaSign = deltaSigns[index];
            return new float3(parentPosition.x + deltaSign.x * delta, parentPosition.y + deltaSign.y * delta, parentPosition.z + deltaSign.z * delta);
        }

        private void CheckClosesNodes(int index, float3 position, float3 voxelPosition, byte lodLevel, float size)
        {
            float diagonal = math.sqrt(2 * (size * size));

            if (lodLevel < maxLodLevel &&  math.length(playerPosition - position) < diagonal)
            {
                float delta = size / 4;

                for (int i = 0; i < 8; i++)
                {
                    float3 voxelPosition2 = voxelPosition + (new float3(voxelBasePosition[i].x, voxelBasePosition[i].y,voxelBasePosition[i].z) * size / 2);
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
            CheckClosesNodes(0, new float3(radius / 2, radius / 2, radius / 2), float3.zero, 0, radius);
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
