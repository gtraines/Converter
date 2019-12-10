using Idmr.Conversions.GameFormats.MissionAbstractions;
using Idmr.Conversions.GameFormats.MissionAbstractions.Segments;
using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.GameFormats.XvTMission
{
    public class XvTMission : MissionBase
    {

        public override bool IngestSegments(ConversionContext context)
        {
            FlightGroupCountSegment.Ingest(context);
            MessageCountSegment.Ingest(context);
            TimeLimitSegment.Ingest(context);

            FlightGroupCount = FlightGroupCountSegment.ToInt16();
            MessageCount = MessageCountSegment.ToInt16();
            TimeLimit = TimeLimitSegment.ByteContent[0];


            return true;
        }

        public override IByteSegment[] ByteSegments { get; set; }
        public short FlightGroupCount { get; set; }
        public short MessageCount { get; set; }
        public byte TimeLimit { get; set; }

        protected ByteSegment FlightGroupCountSegment => new ByteSegment(2, 2); // fg count
        protected ByteSegment MessageCountSegment => new ByteSegment(4, 2); // Message count
        protected ByteSegment TimeLimitSegment => new ByteSegment(0x66, 1); // Time limit
    }
}
