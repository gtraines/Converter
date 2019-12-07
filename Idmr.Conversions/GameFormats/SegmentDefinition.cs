using System.Runtime.ConstrainedExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Idmr.Conversions.GameFormats
{
    public class SegmentDefinition
    {
        public SegmentDefinition(
            GameType game, 
            string friendlyName,
            int segmentOffset,
            int segmentLength)
        {
            if (string.IsNullOrWhiteSpace(friendlyName))
            {
                throw new ArgumentException("message", nameof(friendlyName));
            }

            Game = game;
            FriendlyName = friendlyName;
            SegmentOffset = segmentOffset;
            SegmentLength = segmentLength;
        }

        public GameType Game { get; }
        public string FriendlyName { get; }
        public int SegmentOffset { get; }
        public int SegmentLength { get; }
    }
}