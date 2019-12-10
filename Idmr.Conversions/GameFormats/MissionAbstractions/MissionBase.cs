using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.GameFormats.MissionAbstractions
{
    public abstract class MissionBase
    {
        public abstract bool IngestSegments(ConversionContext context);
        public abstract IByteSegment[] ByteSegments { get; set; }
            
    }
}
