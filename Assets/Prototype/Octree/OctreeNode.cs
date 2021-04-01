using Unity.Mathematics;
using UnityEngine;

public struct OctreeNode
{
    // node value
    public float3 position;
    public float3 voxelPosition;
    public float size;
    public byte lodLevel;

    public OctreeNode
    (
        float3 position,
        float3 voxelPosition,
        float size,
        byte lodLevel
    )
    {
        this.position = position;
        this.voxelPosition = voxelPosition;
        this.size = size;
        this.lodLevel = lodLevel;
    }
}

