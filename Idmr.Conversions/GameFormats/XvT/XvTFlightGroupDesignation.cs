using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.GameFormats.XvT
{
    public class XvTFlightGroupDesignation
    {
        //xvt  input   8 chars, [0] = team, [1..3] text of role.  EX: "2MIS", repeat for role2
        //xwa  output  4 bytes, [0] = role1 enabled, [1] = role2 enabled, [2] = role1 enum, [3] = role2 enum
        public XvTFlightGroupDesignation(byte[] sourceBytes)
        {
            if (sourceBytes.Length != 8)
            {
                throw new ArgumentOutOfRangeException("Source bytes must be an array of length 8");
            }

            //Get the role first so that if the team is set to all,
            // both teams can be assigned the same role.
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
            //~MG: 
            // the original single-designation version of this function had 
            // 0xB for 'A' and 0xA for 'H'. 
            // I have this value as being a bool, need to look into it
            switch (FlightGroupFaction1)
            {
                case '1': results[0] = 0x0; break;
                case '2': results[0] = 0x1; break;
                case '3': results[0] = 0x2; break;
                case '4': results[0] = 0x3; break;
                case 'A': 
                    results[0] = 0xA; 
                    results[1] = 0xB; 
                    results[2] = TryGetXwaRole(RoleText1); 
                    results[3] = TryGetXwaRole(RoleText1); 
                    break;  
                case 'H': 
                    results[0] = 0xA; 
                    results[1] = 0xB; 
                    results[2] = TryGetXwaRole(RoleText1); 
                    results[3] = TryGetXwaRole(RoleText1); 
                    break; //No idea (what 'H' means)                    
                default: results[0] = 0x0; break;
            }

            switch (FlightGroupFaction2)
            {
                case '1': results[1] = 0x0; break;
                case '2': results[1] = 0x1; break;
                case '3': results[1] = 0x2; break;
                case '4': results[1] = 0x3; break;
                case 'A':
                    results[0] = 0xA;
                    results[1] = 0xB;
                    results[2] = TryGetXwaRole(RoleText2);
                    results[3] = TryGetXwaRole(RoleText2);
                    break;  //~MG: the original single-designation version of this function had 0xB for 'A' and 0xA for 'H'. I have this value as being a bool, need to look into it
                case 'H':
                    results[0] = 0xA;
                    results[1] = 0xB;
                    results[2] = TryGetXwaRole(RoleText2);
                    results[3] = TryGetXwaRole(RoleText2);
                    break; //No idea (what 'H' means)
                default: results[1] = 0x0; break;
            }

            return results;
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
