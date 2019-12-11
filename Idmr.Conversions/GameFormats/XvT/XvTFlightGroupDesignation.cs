using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.GameFormats.XvT
{
    public class XvTFlightGroupDesignation
    {
        public XvTFlightGroupDesignation(byte[] sourceBytes)
        {
            if (sourceBytes.Length != 8)
            {
                throw new ArgumentOutOfRangeException("Source bytes must be an array of length 8");
            }
            
            var xvtInputAsString = Encoding.ASCII.GetString(sourceBytes).ToUpper();
            FlightGroupFaction1 = xvtInputAsString[0];
            RoleText1 = xvtInputAsString.Substring(1, 3);

            FlightGroupFaction2 = xvtInputAsString[4];
            RoleText2 = xvtInputAsString.Substring(5, 3);

        }

        public char FlightGroupFaction1 { get; set; }
        public string RoleText1 { get; set; }
        public char FlightGroupFaction2 { get; set; }
        public string RoleText2 { get; set; }

        public byte[] ToXWingAllianceDesignationBytes()
        {
            byte[] results = new byte[4];
            results[0] = 0xFF;
            results[1] = 0xFF;
            results[2] = TryGetXwaRole(RoleText1);
            results[3] = TryGetXwaRole(RoleText2);

        }

        public static byte TryGetXwaRole(string xvtSourceRole)
        {
            var lookupKey = xvtSourceRole.ToUpper();
            return RoleMap[lookupKey];
        }

        public static readonly Dictionary<string, byte> RoleMap = new Dictionary<string, byte> {
                {"NON", 0xFF},
                {"BAS", 1},
                {"COM", 0},
                {"CON", 4},
                {"MAN", 11},
                {"MIS", 3},
                {"PRI", 7},
                {"REL", 6},
                {"RES", 10},
                {"SEC", 8},
                {"STA", 2},
                {"STR", 5},
                {"TER", 9}
            };

    }
}
