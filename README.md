# Unity Simple Octree

I started to create it for voxel planet game.
If you interested in voxel technologies join us: https://discord.gg/aegudcbYhr​

![octree](https://raw.githubusercontent.com/patrikholler/Unity-Simple-Octree/master/screenshot/screenshot01.png)

## Usage
<ol>
    <li>Unity Engine version: 2020.3.1f1</li>
    <li>
        Packages:
        <ul>
        <li>using Unity.Collections</li>
        <li>using Unity.Jobs</li>
        <li>using Unity.Burst</li>
        <li>using Unity.Mathematics</li>
        </ul>
    </li>
    <li>Switch gizmo on</li>
</ol>

## Known problems
https://forum.unity.com/threads/voxel-planet-octree-job-system-ecs-dots.1083410/
<ol>
    <li>I dont feel it is the best solution to call the job every seconds and create octree nodes from 0 to max lod level again and again.</li>
    <li>It will be better to use IJobParallelFor</li>
</ol>

## License

MIT