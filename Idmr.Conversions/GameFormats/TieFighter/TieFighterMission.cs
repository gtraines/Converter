using Idmr.Conversions.GameFormats.MissionAbstractions;
using Idmr.Conversions.GameFormats.MissionAbstractions.Segments;
using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.GameFormats.TieFighter
{
    public class TieFighterMission : MissionBase
    {
        public TieFighterMission()
        {
            PopulateByteSegmentsOrder();
        }

        protected IByteSegment[] PopulateByteSegmentsOrder()
        {
            return new IByteSegment[]
            {
                new ByteSegment(2, 2), // fg count
                new ByteSegment(4, 2), // Message count
                new ByteSegment(0x66, 1) // Time limit
            };
        }

        public override bool IngestSegments(ConversionContext context)
        {
            var fgCountSegs = new ByteSegment(2, 2); // fg count
            var msgCnt = new ByteSegment(4, 2); // Message count

            return true;
        }

        public override IByteSegment[] ByteSegments { get; set; }
        public int FlightGroupCount { get; set; }
        public int MessageCount { get; set; }


    }
}
