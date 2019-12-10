using System;
using System.Linq;

namespace Idmr.Conversions.GameFormats.MissionAbstractions.Segments
{
    public abstract class ByteSegmentBase: IByteSegment
    {
        public virtual string SegmentName { get; }
        public abstract long ByteOffset { get; protected set; }
        public abstract long ByteCount { get; protected set; }
        public byte[] ByteContent { get; protected set; }

        public short ToInt16(int startIdx = 0)
        {
            return BitConverter.ToInt16(ByteContent, startIdx);
        }
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
