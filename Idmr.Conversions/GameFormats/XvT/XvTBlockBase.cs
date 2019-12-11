using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.GameFormats.XvT
{
    public abstract class XvTBlockBase
    {
        protected static void ConvertDesignations(byte[] xvt, byte[] targetBuffer)
		{
			//xvt  input   8 chars, [0] = team, [1..3] text of role.  EX: "2MIS", repeat for role2
			//xwa  output  4 bytes, [0] = role1 enabled, [1] = role2 enabled, [2] = role1 enum, [3] = role2 enum
			targetBuffer[0] = 0xFF;
			targetBuffer[1] = 0xFF;
			targetBuffer[2] = 0x00;
			targetBuffer[3] = 0x00;


			string t = System.Text.Encoding.ASCII.GetString(xvt).ToUpper();
			for (int i = 0; i < 2; i++)
			{
				string sub = t.Substring(0, 4);
				if (sub[0] == 0) return;

				//Get the role first so that if the team is set to all,
				// both teams can be assigned the same role.
				char team = sub[0];
				sub = sub.Substring(1);
				byte role = 0;
				roleMap.TryGetValue(sub, out role);
				targetBuffer[2 + i] = role;

				switch (team)
				{
					case '1': targetBuffer[i] = 0x0; break;
					case '2': targetBuffer[i] = 0x1; break;
					case '3': targetBuffer[i] = 0x2; break;
					case '4': targetBuffer[i] = 0x3; break;
					case 'A': targetBuffer[0] = 0xA; targetBuffer[1] = 0xB; targetBuffer[2] = role; targetBuffer[3] = role; break;  //~MG: the original single-designation version of this function had 0xB for 'A' and 0xA for 'H'. I have this value as being a bool, need to look into it
					case 'H': targetBuffer[0] = 0xA; targetBuffer[1] = 0xB; targetBuffer[2] = role; targetBuffer[3] = role; break;  //No idea (what 'H' means)
					default: targetBuffer[i] = 0x0; break;
				}

				t = t.Substring(4); //Trim so the next 4 become the current.
			}
		}
	}
}
