using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.GameFormats.MissionAbstractions.Segments
{
    public class ByteSegment : ByteSegmentBase
    {
        public ByteSegment(long byteOffset, long byteCount)
        {
            ByteOffset = byteOffset;
            ByteCount = byteCount;
        }

        public override long ByteOffset { get; protected set; }

        public override long ByteCount { get; protected set; }
    }
}
