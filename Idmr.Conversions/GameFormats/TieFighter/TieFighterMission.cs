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

    }
}
