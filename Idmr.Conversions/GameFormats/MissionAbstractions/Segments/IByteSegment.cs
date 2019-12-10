using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.GameFormats.MissionAbstractions
{
    public interface IByteSegment
    {
        string SegmentName { get; }
        long ByteOffset { get; }
        long ByteCount { get; }
        byte[] ByteContent { get; }
        void Ingest(ConversionContext conversionContext);
    }
    public interface IByteSegment<TElement>
    {
        TElement PopulatedElement { get; set; }   
    }
}
