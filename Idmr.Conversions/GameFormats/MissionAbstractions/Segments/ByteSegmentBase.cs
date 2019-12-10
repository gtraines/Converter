using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Idmr.Conversions.GameFormats.MissionAbstractions.Segments
{
    public abstract class ByteSegmentBase: IByteSegment
    {
        public virtual string SegmentName { get; }
        public abstract long ByteOffset { get; protected set; }
        public abstract long ByteCount { get; protected set; }
        public byte[] ByteContent { get; protected set; }
        public virtual void Ingest(ConversionContext context)
        {
            context.SourceCursor = ByteOffset;
            ByteContent = new byte[ByteCount];

            foreach (var idx in Enumerable.Range(0, (int)ByteCount))
            {
                ByteContent[idx] = 0;
            }
        }
    }
}
