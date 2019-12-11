using Idmr.Conversions.GameFormats.XvT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Idmr.Conversions.Converters
{
    public class XWingVsTieConverter : ConverterBase
    {
		protected override bool InnerTo(ConversionContext context)
		{
			XvTBriefingBlock xvtBriefingBlock = null;
			XvTFlightGroupsBlock xvtFlightGroupBlock = null;


			var isMultiplayer = false;
			var isBoPMision = true;
			short[] fgIcons;

			context.WriteToTargetBuffer(18, true);
			context.SourceCursor = 2;
			//short i, j;
			short FGs = context.ReadSourceInt16();
			short Messages = context.ReadSourceInt16();
			context.WriteToTarget((short)(FGs + 2)); // [JB] Modified to +2 since generated skirmish files have two backdrops for ambient lighting
			context.WriteToTarget(Messages);
			fgIcons = new short[FGs];

			context.WriteToTargetBuffer(1 , 8);
			context.WriteToTargetBuffer(1, 11); //unknowns
			context.WriteToTargetBuffer(Encoding.ASCII.GetBytes("The Final Frontier"), 100);  //make a nice Region name :P
			context.WriteToTargetBuffer(6, 0x23A);  //starting hangar
			context.TargetCursor++;
			context.SourceCursor = 0x66; 
			context.WriteToTarget(context.ReadByte());       //time limit (minutes)
			
			context.WriteToTargetBuffer(0x62, 0x23B3);                     //unknown

			context.TargetCursor = 0x23F0;

			//[JB] Jumping ahead to get the briefing locations before we load in the FGs.
			xvtBriefingBlock = new XvTBriefingBlock(FGs, Messages, context);
			xvtFlightGroupBlock = new XvTFlightGroupsBlock(FGs, context);

			//[JB] Now that flight groups are done, check for player count and patch in skirmish mode
			if (isMultiplayer && PlayerCraft > 1)
			{
				long backupPos = streams.TargetStream.Position;
				streams.TargetStream.Position = 0x23AC;
				context.WriteToTargetBuffer(4);
				streams.TargetStream.Position = backupPos;
			}
			#region Messages
			for (var i = 0; i < Messages; i++)
			{
				XvTPos = context.SourceCursor;
				XWAPos = streams.TargetStream.Position;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());       //Message# - 1
				switch (streams.SourceStream.ReadByte())                                         //takes care of colors if needed
				{
					case 49:
						streams.TargetWriter.Write(streams.SourceReader.ReadBytes(63)); //green
						streams.TargetStream.Position = XWAPos + 142; context.WriteToTargetBuffer(0);
						break;
					case 50:
						streams.TargetWriter.Write(streams.SourceReader.ReadBytes(63)); //blue
						streams.TargetStream.Position = XWAPos + 142; context.WriteToTargetBuffer(2);
						break;
					case 51:
						streams.TargetWriter.Write(streams.SourceReader.ReadBytes(63)); ;   //yellow
						streams.TargetStream.Position = XWAPos + 142; context.WriteToTargetBuffer(3);
						break;
					default:
						context.SourceCursor--;
						streams.TargetWriter.Write(streams.SourceReader.ReadBytes(64)); //red
						streams.TargetStream.Position = XWAPos + 142; context.WriteToTargetBuffer(1);
						break;
				}
				streams.TargetStream.Position = XWAPos + 82;
				streams.TargetWriter.Write(streams.SourceReader.ReadBytes(14));     //Sent to.. -> T1
				ShipFix(streams.SourceStream, streams.TargetStream);
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());           //T2
				ShipFix(streams.SourceStream, streams.TargetStream);
				context.SourceCursor += 2;
				streams.TargetStream.Position += 2;
				streams.TargetWriter.Write(streams.SourceReader.ReadByte());                                //T1 AND/OR T2
				streams.TargetStream.Position++;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());           //T3
				ShipFix(streams.SourceStream, streams.TargetStream);
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());           //T4
				ShipFix(streams.SourceStream, streams.TargetStream);
				context.SourceCursor += 2;
				streams.TargetStream.Position += 2;
				streams.TargetWriter.Write(streams.SourceReader.ReadByte());                                //T3 AND/OR T4
				streams.TargetStream.Position = XWAPos + 141;
				context.SourceCursor += 17;
				streams.TargetWriter.Write(streams.SourceReader.ReadByte());                                //T (1/2) AND/OR (3/4)
				streams.TargetStream.Position = XWAPos + 132;         //[JB] OriginatingFG
				context.WriteToTargetBuffer(System.Convert.ToByte(FGs + 1));  //[JB] Set to last FG (+2 inserted backdrops so the last new FG index is FG+1). Assigning messages to backdrops ensures the object is always present so messages always fire.
				streams.TargetStream.Position = XWAPos + 140;
				context.SourceCursor = XvTPos + 114;
				int msgDelaySec = streams.SourceStream.ReadByte() * 5;  //[JB] Corrected delay time.
				context.WriteToTargetBuffer(System.Convert.ToByte(msgDelaySec % 60));                        //Delay
				context.WriteToTargetBuffer(System.Convert.ToByte(msgDelaySec / 60));
				streams.TargetStream.Position += 2; context.WriteToTargetBuffer(10);  //[JB] Modified offset for second delay byte
				streams.TargetStream.Position += 5; context.WriteToTargetBuffer(10);                                       //make sure the Cancel triggers are set to FALSE
				streams.TargetStream.Position = XWAPos + 162;
				context.SourceCursor = XvTPos + 116;
			}
			#endregion
			#region Global Goals
			XvTPos = context.SourceCursor;
			XWAPos = streams.TargetStream.Position;
			for (int ti = 0; ti < 10; ti++) //[JB] Converting all 10 teams just in case some triggers depend on them.
			{
				context.SourceCursor = XvTPos + (0x80 * ti);
				streams.TargetStream.Position = XWAPos + (0x170 * ti);
				context.WriteToTargetBuffer(3);
				context.SourceCursor++;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //Prim T1
				ShipFix(streams.SourceStream, streams.TargetStream);
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //PT2
				ShipFix(streams.SourceStream, streams.TargetStream);
				streams.TargetStream.Position += 2;
				context.SourceCursor += 2;
				streams.TargetWriter.Write(streams.SourceReader.ReadByte());                            //PT 1 AND/OR 2
				streams.TargetStream.Position++;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //PT 3
				ShipFix(streams.SourceStream, streams.TargetStream);
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //PT 4
				ShipFix(streams.SourceStream, streams.TargetStream);
				context.SourceCursor += 2;
				streams.TargetStream.Position += 2;
				streams.TargetWriter.Write(streams.SourceReader.ReadByte());                            //PT 3 AND/OR 4
				context.SourceCursor += 17;
				streams.TargetStream.Position += 18;
				streams.TargetWriter.Write(streams.SourceReader.ReadBytes(3));      //PT (1/2) AND/OR (3/4) -> Points
				streams.TargetStream.Position += 70;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //Prev T1
				ShipFix(streams.SourceStream, streams.TargetStream);
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //PT2
				ShipFix(streams.SourceStream, streams.TargetStream);
				streams.TargetStream.Position += 2;
				context.SourceCursor += 2;
				streams.TargetWriter.Write(streams.SourceReader.ReadByte());                            //PT 1 AND/OR 2
				streams.TargetStream.Position++;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //PT 3
				ShipFix(streams.SourceStream, streams.TargetStream);
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //PT 4
				ShipFix(streams.SourceStream, streams.TargetStream);
				context.SourceCursor += 2;
				streams.TargetStream.Position += 2;
				streams.TargetWriter.Write(streams.SourceReader.ReadByte());                            //PT 3 AND/OR 4
				context.SourceCursor += 17;
				streams.TargetStream.Position += 18;
				streams.TargetWriter.Write(streams.SourceReader.ReadBytes(3));      //PT (1/2) AND/OR (3/4) -> Points
				streams.TargetStream.Position += 70;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //Sec T1
				ShipFix(streams.SourceStream, streams.TargetStream);
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //ST2
				ShipFix(streams.SourceStream, streams.TargetStream);
				streams.TargetStream.Position += 2;
				context.SourceCursor += 2;
				streams.TargetWriter.Write(streams.SourceReader.ReadByte());                            //ST 1 AND/OR 2
				streams.TargetStream.Position++;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //ST 3
				ShipFix(streams.SourceStream, streams.TargetStream);
				streams.TargetWriter.Write(streams.SourceReader.ReadInt32());       //ST 4
				ShipFix(streams.SourceStream, streams.TargetStream);
				context.SourceCursor += 2;
				streams.TargetStream.Position += 2;
				streams.TargetWriter.Write(streams.SourceReader.ReadByte());                            //ST 3 AND/OR 4
				context.SourceCursor += 17;
				streams.TargetStream.Position += 18;
				streams.TargetWriter.Write(streams.SourceReader.ReadBytes(3));      //ST (1/2) AND/OR (3/4) -> Points
				streams.TargetStream.Position += 70;
			}
			context.SourceCursor = XvTPos + (0x80 * 10);  //10 teams
			streams.TargetStream.Position = XWAPos + (0x170 * 10);
			#endregion
			#region IFF/Teams
			streams.TargetWriter.Write(streams.SourceReader.ReadBytes(4870));   //well, that was simple..
			#endregion
			#region Briefing
			XWAPos = streams.TargetStream.Position;
			long XWABriefing1 = XWAPos;
			streams.TargetWriter.Write(streams.SourceReader.ReadBytes(6));  //briefing intro
			streams.TargetWriter.Write((short)(streams.SourceReader.ReadInt16() + 10 * Briefs[0])); // adjust length for add/moves
			context.SourceCursor += 2;
			streams.TargetStream.Position += 20 * Briefs[0] + 2;
			streams.TargetWriter.Write(streams.SourceReader.ReadBytes(0x32A));  //briefing content
			streams.TargetStream.Position = XWAPos;
			var briefingLength = (short)(streams.TargetReader.ReadInt16() * 0x19 / 0x14);       // adjust overall briefing length
			streams.TargetStream.Position -= 2;
			streams.TargetWriter.Write(briefingLength);
			streams.TargetStream.Position += 8;
			for (var i = 0; i < 0x320; i += 4)      // work our way through length of briefing. i automatically increases by 4 per event
			{
				briefingLength = streams.TargetReader.ReadInt16();
				if (briefingLength == 0x270F) break;     // stop check at t=9999, end briefing
				briefingLength = (short)(briefingLength * 0x19 / 0x14);
				streams.TargetStream.Position -= 2;
				streams.TargetWriter.Write(briefingLength);
				briefingLength = streams.TargetReader.ReadInt16();      // now get the event type
				if (briefingLength > 8 && briefingLength < 17)        // FG tags 1-8
				{
					briefingLength = streams.TargetReader.ReadInt16();  // FG number
					streams.TargetStream.Position -= 2;
					streams.TargetWriter.Write(fgIcons[briefingLength]);   // Overwrite with the Icon#
					i += 2;
				}
				else if (briefingLength == 7)        // Zoom map command
				{
					briefingLength = (short)(streams.TargetReader.ReadInt16() * 124 / 58);  // X
					streams.TargetStream.Position -= 2;
					streams.TargetWriter.Write(briefingLength);
					briefingLength = (short)(streams.TargetReader.ReadInt16() * 124 / 88);  // Y
					streams.TargetStream.Position -= 2;
					streams.TargetWriter.Write(briefingLength);
					i += 4;
				}
				else
				{
					streams.TargetStream.Position += 2 * BRF[briefingLength];       // skip over vars
					i += (short)(2 * BRF[briefingLength]);   // increase length counter by skipped vars
				}
			}
			streams.TargetStream.Position = 0x8960 + (FGs + 2) * 0xE3E + Messages * 0xA2;  //[JB] FGs+2
			context.WriteToTargetBuffer(1);                   //show the non-existant briefing
			streams.TargetStream.Position += 9;
			#endregion Briefing
			#region Briefing tags & strings
			for (var i = 0; i < 32; i++)    //tags
			{
				briefingLength = streams.SourceReader.ReadInt16();     //check length..
				streams.TargetWriter.Write(briefingLength);                //..write length..
				if (briefingLength != 0)                                     //and copy if not 0
					streams.TargetWriter.Write(streams.SourceReader.ReadBytes(briefingLength));
			}
			streams.TargetStream.Position += 192;
			for (var i = 0; i < 32; i++)    //strings
			{
				briefingLength = streams.SourceReader.ReadInt16();     //check length..
				streams.TargetWriter.Write(briefingLength);                //..write length..
				if (briefingLength != 0)                                     //and copy if not 0
					streams.TargetWriter.Write(streams.SourceReader.ReadBytes(briefingLength));
			}
			streams.TargetStream.Position += 192;
			#endregion Briefing T&S
			#region Briefing2
			//[JB] Begin briefing 2.  Basically just copy/paste the same code.
			if (isMultiplayer)
			{
				long XWABriefing2 = streams.TargetStream.Position;
				XWAPos = streams.TargetStream.Position;
				streams.TargetWriter.Write(streams.SourceReader.ReadBytes(6));  //briefing intro
				streams.TargetWriter.Write((short)(streams.SourceReader.ReadInt16() + 10 * Briefs[1])); // adjust length for add/moves
				context.SourceCursor += 2;
				streams.TargetStream.Position += 20 * Briefs[1] + 2;
				streams.TargetWriter.Write(streams.SourceReader.ReadBytes(0x32A));  //briefing content
				streams.TargetStream.Position = XWAPos;
				briefingLength = (short)(streams.TargetReader.ReadInt16() * 0x19 / 0x14);       // adjust overall briefing length
				streams.TargetStream.Position -= 2;
				streams.TargetWriter.Write(briefingLength);
				streams.TargetStream.Position += 8;
				for (var i = 0; i < 0x320; i += 4)      // work our way through length of briefing. i automatically increases by 4 per event
				{
					briefingLength = streams.TargetReader.ReadInt16();
					if (briefingLength == 0x270F) break;     // stop check at t=9999, end briefing
					briefingLength = (short)(briefingLength * 0x19 / 0x14);
					streams.TargetStream.Position -= 2;
					streams.TargetWriter.Write(briefingLength);
					briefingLength = streams.TargetReader.ReadInt16();      // now get the event type
					if (briefingLength > 8 && briefingLength < 17)        // FG tags 1-8
					{
						briefingLength = streams.TargetReader.ReadInt16();  // FG number
						streams.TargetStream.Position -= 2;
						streams.TargetWriter.Write(fgIcons[briefingLength]);   // Overwrite with the Icon#
						i += 2;
					}
					else if (briefingLength == 7)        // Zoom map command
					{
						briefingLength = (short)(streams.TargetReader.ReadInt16() * 124 / 58);  // X
						streams.TargetStream.Position -= 2;
						streams.TargetWriter.Write(briefingLength);
						briefingLength = (short)(streams.TargetReader.ReadInt16() * 124 / 88);  // Y
						streams.TargetStream.Position -= 2;
						streams.TargetWriter.Write(briefingLength);
						i += 4;
					}
					else
					{
						streams.TargetStream.Position += 2 * BRF[briefingLength];     // skip over vars
						i += (short)(2 * BRF[briefingLength]);   // increase length counter by skipped vars
					}
				}
				streams.TargetStream.Position = 0x8960 + (XWABriefing2 - XWABriefing1) + (FGs + 2) * 0xE3E + Messages * 0xA2;   //[JB] FGs+2
				context.WriteToTargetBuffer(0);                   //show the non-existant briefing
				context.WriteToTargetBuffer(1);                   //show the non-existant briefing
				streams.TargetStream.Position += 8;
				for (var i = 0; i < 32; i++)    //tags
				{
					briefingLength = streams.SourceReader.ReadInt16();     //check length..
					streams.TargetWriter.Write(briefingLength);                //..write length..
					if (briefingLength != 0)                                     //and copy if not 0
						streams.TargetWriter.Write(streams.SourceReader.ReadBytes(briefingLength));
				}
				streams.TargetStream.Position += 192;
				for (var i = 0; i < 32; i++)    //strings
				{
					briefingLength = streams.SourceReader.ReadInt16();     //check length..
					streams.TargetWriter.Write(briefingLength);                //..write length..
					if (briefingLength != 0)                                     //and copy if not 0
						streams.TargetWriter.Write(streams.SourceReader.ReadBytes(briefingLength));
				}
				streams.TargetStream.Position += 192;
			}
			else
			{
				context.SourceCursor += 0x334;    //Jump to tags
				for (briefingLength = 0; briefingLength < 64; briefingLength++)   //32 tags + 32 strings
					context.SourceCursor += streams.SourceReader.ReadInt16();
				streams.TargetStream.Position += 0x4614;  //Empty briefing plus empty tags/strings
			}
			#endregion Briefing2

			streams.TargetStream.Position += 0x187C; //Skip EditorNotes
			streams.TargetStream.Position += 0x3200; //Skip BriefingStringNotes
			streams.TargetStream.Position += 0x1900; //Skip MessageNotes
			streams.TargetStream.Position += 0xBB8;  //Skip EomNotes
			streams.TargetStream.Position += 0xFA0;  //Skip Unknown
			streams.TargetStream.Position += 0x12C;  //Skip DescriptionNotes

			//[JB] Briefings have variable length. Need to step over the remaining 6 XvT briefings by properly calculating how big they are.
			for (var i = 2; i < 8; i++)
			{
				context.SourceCursor += 0x334;    //Jump to tags
				for (briefingLength = 0; briefingLength < 64; briefingLength++)   //32 tags + 32 strings
					context.SourceCursor += streams.SourceReader.ReadInt16();
			}
			#region FG Goal strings
			for (var i = 0; i < FGs; i++)
			{
				for (briefingLength = 0; briefingLength < 24; briefingLength++)  //8 goals * 3 strings
				{
					if (streams.SourceStream.ReadByte() == 0)
					{
						streams.TargetStream.Position++;
						context.SourceCursor += 63;
					}
					else
					{
						context.SourceCursor--;
						streams.TargetWriter.Write(streams.SourceReader.ReadBytes(64));
					}
				}
			}
			streams.TargetStream.Position += 48;                     //compensate for adding the Backdrop  [JB] Was 24 for one backdrop, needs to be 48 since I added an extra one.
			#endregion
			#region Global Goal strings
			for (var i = 0; i < 10; i++)
			{
				for (briefingLength = 0; briefingLength < 36; briefingLength++)
				{
					if (streams.SourceStream.ReadByte() == 0)
					{
						streams.TargetStream.Position++;
						context.SourceCursor += 63;
					}
					else
					{
						context.SourceCursor--;
						streams.TargetWriter.Write(streams.SourceReader.ReadBytes(64));
					}
				}
				context.SourceCursor += 3072;
			}
			#endregion
			streams.TargetStream.Position += 3552;               //skip custom order strings
			#region Debrief and Descrip
			if (!isBoPMision)
			{
				streams.TargetStream.Position += 4096;
				context.WriteToTargetBuffer(35);
				streams.TargetStream.Position += 4095;
				context.WriteToTargetBuffer(35);
				for (var i = 0; i < 1024; i++)
				{
					int d = streams.SourceStream.ReadByte();
					briefingLength = (short)(1024 - i);
					switch (d)
					{
						case -1:
							i = 1024;
							break;
						case 0:
							i = 1024;
							break;
						case 10:
							context.WriteToTargetBuffer(35);
							break;
						default:
							context.SourceCursor--;
							streams.TargetWriter.Write(streams.SourceReader.ReadByte());
							break;
					}
				}
				streams.TargetStream.Position += 3071 + briefingLength;
			}
			else
			{
				streams.TargetWriter.Write(streams.SourceReader.ReadBytes(4096));   // Debrief
				context.WriteToTargetBuffer(35);
				streams.TargetWriter.Write(streams.SourceReader.ReadBytes(4095));   // Hints
				context.SourceCursor++;
				context.WriteToTargetBuffer(35);
				streams.TargetWriter.Write(streams.SourceReader.ReadBytes(4095));   // Brief/Description
			}
			#endregion
			context.WriteToTargetBuffer(0);               //EOF

			return true;
		}


		void ConvertDesignations(byte[] xvt, byte[] targetBuffer)
		{
			//xvt  input   8 chars, [0] = team, [1..3] text of role.  EX: "2MIS", repeat for role2
			//xwa  output  4 bytes, [0] = role1 enabled, [1] = role2 enabled, [2] = role1 enum, [3] = role2 enum
			targetBuffer[0] = 0xFF;
			targetBuffer[1] = 0xFF;
			targetBuffer[2] = 0x00;
			targetBuffer[3] = 0x00;

			Dictionary<string, byte> roleMap = new Dictionary<string, byte> {
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

			string t = System.Text.Encoding.ASCII.GetString(xvt).ToUpper();
			for (int i = 0; i < 2; i++)
			{
				string sub = t.Substring(0, 4);
				if (sub[0] == 0) return;

				//Get the role first so that if the team is set to all, both teams can be assigned the same role.
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

		void ShipFix(FileStream In, FileStream targetStream)     //checks for Ship Type trigger, adjusts value
		{
			In.Position -= 3;
			if (In.ReadByte() == 2)
			{
				targetStream.Position -= 2;
				targetStream.WriteByte(System.Convert.ToByte(In.ReadByte() + 1));
				targetStream.Position++;
				In.Position++;
			}
			else { In.Position += 2; }
			targetStream.Position += 2;
		}
		protected void ShipOrderFix(FileStream In, FileStream targetStream)    //seperate function for Orders
		{
			In.Position -= 12;
			if (In.ReadByte() == 2)             //Target 3
			{
				In.Position++;
				targetStream.Position -= 10;
				targetStream.WriteByte(System.Convert.ToByte(In.ReadByte() + 1));
				In.Position -= 2;
			}
			else { targetStream.Position -= 9; }
			if (In.ReadByte() == 2)             //Target 4
			{
				In.Position++;
				targetStream.WriteByte(System.Convert.ToByte(In.ReadByte() + 1));
				In.Position += 2;
				targetStream.Position += 3;
			}
			else
			{
				targetStream.Position += 4;
				In.Position += 4;
			}
			if (In.ReadByte() == 2)             //Target 1
			{
				targetStream.WriteByte(System.Convert.ToByte(In.ReadByte() + 1));
				targetStream.Position++;
			}
			else
			{
				In.Position++;
				targetStream.Position += 2;
			}
			if (In.ReadByte() == 2)             //Target 2
			{
				targetStream.WriteByte(System.Convert.ToByte(In.ReadByte() + 1));
				In.Position += 2;
				targetStream.Position += 4;
			}
			else
			{
				In.Position += 3;
				targetStream.Position += 5;
			}
		}

		protected byte GetConvertedOrderTimeByte(byte xvtTime)
		{
			//XWA time value, if 20 (decimal) or under is exact seconds.
			//21 = 25 sec, 22 = 30 sec, etc.

			var seconds = xvtTime * 5;
			var scaledSeconds = 0;
			try
			{
				if (seconds <= 20)
				{
					scaledSeconds = seconds;
				}
				else
				{
					scaledSeconds = ((seconds - 20) / 5) + 20;
					if (scaledSeconds < 0)
						scaledSeconds = 0;
				}
				return System.Convert.ToByte(scaledSeconds);
			}
			catch (Exception ex)
			{
				throw new Exception(
					$"Error convering XvT time to XWA time: XvT value:{xvtTime} Stepped time (seconds): {seconds} Scaled time (seconds): {scaledSeconds}",
					ex);
				throw;
			}
		}

		protected void ConvertOrderTime(FileStream fromStream, FileStream toStream)
		{
			long curXvT = fromStream.Position;
			long curXWA = toStream.Position;
			
			byte orderEnum = (byte)fromStream.ReadByte();
			fromStream.Position++;
			
			byte var1 = (byte)fromStream.ReadByte();
			
			toStream.Position += 2;
			switch (orderEnum)
			{
				case 0x0C:   //Board and Give Cargo
				case 0x0D:   //Board and Take Cargo
				case 0x0E:   //Board and Exchange Cargo
				case 0x0F:   //Board and Capture Cargo
				case 0x10:   //Board and Destroy Cargo
				case 0x11:   //Pick up
				case 0x12:   //Drop off   (Deploy time?)
				case 0x13:   //Wait
				case 0x14:   //SS Wait
				case 0x1C:   //SS Hold Steady
				case 0x1E:   //SS Wait
				case 0x1F:   //SS Board
				case 0x20:   //Board to Repair
				case 0x24:   //Self-destruct
					toStream.WriteByte(GetConvertedOrderTimeByte(var1));
					break;
				default:
					break;
			}

			fromStream.Position = curXvT;
			toStream.Position = curXWA;
		}
	}
}
