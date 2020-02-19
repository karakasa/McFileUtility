using System;
using System.Collections.Generic;
using System.Text;

namespace McFileIo.Enum
{
    public enum HeightmapType
    {
        Default = -1,
        LightBlocking = 0,
        MotionBlocking = 1,
        MotionBlockingNoLeaves = 2,
        OceanFloor = 3,
        OceanFloorWg = 4,
        WorldSurface = 5,
        WorldSurfaceWg = 6
    }
}
