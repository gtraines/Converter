using Idmr.Conversions.GameFormats.MissionAbstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.GameFormats.XWingAlliance
{
    public class XWingAllianceMission : MissionBase
    {
        public override IByteSegment[] ByteSegments { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool IngestSegments(ConversionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
