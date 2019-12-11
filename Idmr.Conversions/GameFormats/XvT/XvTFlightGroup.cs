using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.GameFormats.XvT
{
    public class XvTFlightGroup
    {
		public readonly long StartXvTPos; //[JB] Sometimes need to bookmark XvT too for better offset handling
		public readonly long StartXWAPos;
		bool Player = false;
		int PlayerCraft = 0; //[JB] for combat engagements
		public List<XvTMessage> Messages { get; set; }
		public byte[] FlightGroupName { get; set; }
		protected static byte[] GetFlightGroupName(ConversionContext context)
		{
			return context.ReadBytes(20);
		}

		public XvTFlightGroup(long startXvTPosition,
			long startXwaPosition, ConversionContext context)
        {
			Messages = new List<XvTMessage>();
			StartXvTPos = startXvTPosition;
			StartXWAPos = startXwaPosition;

			FlightGroupName = GetFlightGroupName(context);
			context.TargetWriter.Write(
				context.ConversionStreams.SourceReader.ReadBytes(20));   //name
			byte[] d1 = new byte[2];
			byte[] d2 = new byte[2];
			byte[] buffer = new byte[4];
			byte[] buffer2 = new byte[8];
			byte[] des2 = new byte[4];

			context.SourceStream.Read(buffer2, 0, 8);
			ConvertDesignations(buffer2, des2);
			context.SourceCursor -= 8;
			context.WriteToTargetBuffer(des2[0]); context.WriteToTargetBuffer(des2[1]);
			context.WriteToTargetBuffer(des2[2]); context.WriteToTargetBuffer(des2[3]);
			streams.TargetStream.Position++;       //Skip unknown
			context.WriteToTargetBuffer(255); //Global cargos set to none
			context.WriteToTargetBuffer(255);
			streams.TargetStream.Position = XWAPos + 40;
			context.SourceCursor += 20;
			streams.TargetWriter.Write(streams.SourceReader.ReadBytes(40));   //Cargo & Special
			streams.TargetStream.Position = XWAPos + 0x69;
			streams.TargetWriter.Write(streams.SourceReader.ReadBytes(30));           //SC ship# -> Arrival Difficulty
			context.SourceCursor = XvTPos + 0x64;
			// [JB] Modified
			if (!isMultiplayer)
			{
				if (streams.SourceStream.ReadByte() != 0 && Player == false) { Player = true; }              //check for multiple player craft, take the first one
				else
				{
					streams.TargetStream.Position = XWAPos + 0x7D;
					context.WriteToTargetBuffer(0);
				}
			}
			if (streams.SourceStream.ReadByte() != 0) { PlayerCraft++; }

			streams.TargetStream.Position = XWAPos + 0x88;
			context.SourceCursor = XvTPos + 0x6E;
			streams.TargetWriter.Write(streams.SourceReader.ReadInt32());                //Arival trigger 1 (cheating and using Int32 since it's 4 bytes)...
			ShipFix(streams.SourceStream, streams.TargetStream);
			streams.TargetWriter.Write(streams.SourceReader.ReadInt32());                //... and 2
			ShipFix(streams.SourceStream, streams.TargetStream);
			streams.TargetStream.Position += 2;
			context.SourceCursor += 2;
			streams.TargetWriter.Write(streams.SourceReader.ReadByte());                                    //AT 1 AND/OR 2
			streams.TargetStream.Position++;
			streams.TargetWriter.Write(streams.SourceReader.ReadInt32());                //AT 3
			ShipFix(streams.SourceStream, streams.TargetStream);
			streams.TargetWriter.Write(streams.SourceReader.ReadInt32());                //AT 4
			ShipFix(streams.SourceStream, streams.TargetStream);
			streams.TargetStream.Position += 2;
			context.SourceCursor += 2;
			streams.TargetWriter.Write(streams.SourceReader.ReadByte());                                    //AT 3 AND/OR 4
			streams.TargetStream.Position++;
			streams.TargetWriter.Write(streams.SourceReader.ReadInt64());                //AT 1/2 AND/OR 3/4 -> DT (8 bytes)
			ShipFix(streams.SourceStream, streams.TargetStream);
			streams.TargetWriter.Write(streams.SourceReader.ReadInt32());                //DT 2
			ShipFix(streams.SourceStream, streams.TargetStream);
			streams.TargetStream.Position += 2;
			context.SourceCursor += 2;
			streams.TargetWriter.Write(streams.SourceReader.ReadByte());                                    //DT 1 AND/OR 2
			streams.TargetStream.Position += 3;
			context.SourceCursor += 2;
			streams.TargetWriter.Write(streams.SourceReader.ReadByte());                                    //Abort trigger
			context.SourceCursor += 4;
			streams.TargetStream.Position += 3;
			streams.TargetWriter.Write(streams.SourceReader.ReadInt64());                //Arr/Dep methods
			long XvTOrderStart = context.SourceCursor;
			long XWAOrderStart = streams.TargetStream.Position;
			long XvTSubOrderStart = context.SourceCursor;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
			long XWASubOrderStart = streams.TargetStream.Position;
			streams.TargetWriter.Write(streams.SourceReader.ReadBytes(18));       //Order 1
			ShipOrderFix(streams.SourceStream, streams.TargetStream);
			context.SourceCursor = XvTPos + 0x46E;
			for (var j = 0; j < 8; j++)
			{
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP X
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP Y
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP Z
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP enabled
				context.SourceCursor -= 0x84;
			}

			//[JB] Patch wait times
			context.SourceCursor = XvTSubOrderStart;
			streams.TargetStream.Position = XWASubOrderStart;
			ConvertOrderTime(streams.SourceStream, streams.TargetStream);
			//Copy order speed
			context.SourceCursor = XvTSubOrderStart + 18;
			streams.TargetStream.Position = XWASubOrderStart + 18;
			streams.TargetWriter.Write(streams.SourceReader.ReadByte());
			//After XvT speed comes flight group player designation, load it to patch the role later. 
			byte[] role = new byte[16];
			streams.SourceStream.Read(role, 0, 16);
			streams.TargetStream.Position = XWAPos + 0x50; //Patch in the display role for the player FG slot screen.
			streams.TargetStream.Write(role, 0, 16);
			//[JB] End patch code.

			streams.TargetStream.Position = XWAPos + 0x15E;
			context.SourceCursor = XvTPos + 0xF4;
			XvTSubOrderStart = context.SourceCursor;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
			XWASubOrderStart = streams.TargetStream.Position;
			streams.TargetWriter.Write(streams.SourceReader.ReadBytes(18));       //Order 2
			ShipOrderFix(streams.SourceStream, streams.TargetStream);
			context.SourceCursor = XvTPos + 0x46E;
			for (var j = 0; j < 8; j++)
			{
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP X
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP Y
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP Z
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP enabled
				context.SourceCursor -= 0x84;
			}
			//[JB] Patch wait times
			context.SourceCursor = XvTSubOrderStart;
			streams.TargetStream.Position = XWASubOrderStart;
			ConvertOrderTime(streams.SourceStream, streams.TargetStream);
			//Copy order speed
			context.SourceCursor = XvTSubOrderStart + 18;
			streams.TargetStream.Position = XWASubOrderStart + 18;
			streams.TargetWriter.Write(streams.SourceReader.ReadByte());
			//[JB] End patch code.

			streams.TargetStream.Position = XWAPos + 0x1F2;
			context.SourceCursor = XvTPos + 0x146;
			XvTSubOrderStart = context.SourceCursor;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
			XWASubOrderStart = streams.TargetStream.Position;
			streams.TargetWriter.Write(streams.SourceReader.ReadBytes(18));       //Order 3
			ShipOrderFix(streams.SourceStream, streams.TargetStream);
			context.SourceCursor = XvTPos + 0x46E;
			for (var j = 0; j < 8; j++)
			{
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP X
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP Y
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP Z
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP enabled
				context.SourceCursor -= 0x84;
			}
			//[JB] Patch wait times
			context.SourceCursor = XvTSubOrderStart;
			streams.TargetStream.Position = XWASubOrderStart;
			ConvertOrderTime(streams.SourceStream, streams.TargetStream);
			//Copy order speed
			context.SourceCursor = XvTSubOrderStart + 18;
			streams.TargetStream.Position = XWASubOrderStart + 18;
			streams.TargetWriter.Write(streams.SourceReader.ReadByte());
			//[JB] End patch code.

			streams.TargetStream.Position = XWAPos + 0x286;
			context.SourceCursor = XvTPos + 0x198;
			XvTSubOrderStart = context.SourceCursor;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
			XWASubOrderStart = streams.TargetStream.Position;
			streams.TargetWriter.Write(streams.SourceReader.ReadBytes(18));       //Order 4
			ShipOrderFix(streams.SourceStream, streams.TargetStream);
			context.SourceCursor = XvTPos + 0x46E;
			for (var j = 0; j < 8; j++)
			{
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP X
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP Y
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP Z
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //WP enabled
				context.SourceCursor -= 0x84;
			}
			//[JB] Patch wait times
			context.SourceCursor = XvTSubOrderStart;
			streams.TargetStream.Position = XWASubOrderStart;
			ConvertOrderTime(streams.SourceStream, streams.TargetStream);
			//Copy order speed
			context.SourceCursor = XvTSubOrderStart + 18;
			streams.TargetStream.Position = XWASubOrderStart + 18;
			streams.TargetWriter.Write(streams.SourceReader.ReadByte());
			//[JB] End patch code.

			context.SourceCursor = XvTPos + 0x1EA;
			streams.TargetStream.Position = XWAPos + 0xA3A;
			streams.TargetWriter.Write(streams.SourceReader.ReadInt32());        //jump to Order 4 T1
			ShipFix(streams.SourceStream, streams.TargetStream);
			streams.TargetWriter.Write(streams.SourceReader.ReadInt32());        //jump to Order 4 T2
			ShipFix(streams.SourceStream, streams.TargetStream);
			streams.TargetStream.Position += 2;
			context.SourceCursor += 2;
			streams.TargetWriter.Write(streams.SourceReader.ReadByte());                            //jump to Order 4 T 1AND/OR 2
			streams.TargetStream.Position = XWAPos + 0xB0A;
			context.SourceCursor = XvTPos + 0x1F5;
			for (var j = 0; j < 8; j++)
			{
				streams.TargetWriter.Write(streams.SourceReader.ReadBytes(14));       //Goals
				context.SourceCursor += 0x40;
				streams.TargetStream.Position += 0x42;
			}
			context.SourceCursor = XvTPos + 0x466;
			streams.TargetStream.Position = XWAPos + 0xD8A;
			for (var j = 0; j < 3; j++)
			{
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //SP X
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //SP Y
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //SP Z
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //SP enabled
				context.SourceCursor -= 0x84;
			}
			context.SourceCursor = XvTPos + 0x480;
			streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //HYP X
			context.SourceCursor += 0x2A;
			streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //HYP Y
			context.SourceCursor += 0x2A;
			streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //HYP Z
			context.SourceCursor += 0x2A;
			streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //HYP enabled
			context.SourceCursor = XvTPos + 0x506;
			if (streams.SourceStream.ReadByte() == 1)        //okay, briefing time. if BP enabled..
			{
				//[JB] 0x23F0 (fileheader) + 0xE60 (GlobalGoal[10]) + 0x1306 (Team[10]) = 0x4556 (17750 dec)
				streams.TargetStream.Position = 17762 + (FGs + 2) * 3646 + Messages * 162 + Briefs[0] * 20;  //place for next insert command  [JB] Modified to FGs+2
				context.WriteToTargetBuffer(26); streams.TargetStream.Position++; streams.TargetWriter.Write(Briefs[0]);        //Add command
				fgIcons[i] = Briefs[0];     // store the Icon# for the FG
				context.SourceCursor = XvTPos + 0x52; streams.TargetWriter.Write(streams.SourceReader.ReadByte()); streams.TargetStream.Position++;        //Ship
				context.SourceCursor = XvTPos + 0x57; streams.TargetWriter.Write(streams.SourceReader.ReadByte()); streams.TargetStream.Position += 3;     //IFF
				context.SourceCursor = XvTPos + 0x482;
				context.WriteToTargetBuffer(28); streams.TargetStream.Position++; streams.TargetWriter.Write(Briefs[0]);        //Move command
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //BP X
				context.SourceCursor += 0x2A;
				streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //BP Y
				Briefs[0]++;
			}
			if (isMultiplayer)
			{
				context.SourceCursor = XvTPos + 0x508;
				if (streams.SourceStream.ReadByte() == 1)        //okay, briefing time. if BP enabled..
				{
					//[JB] 0x23F0 (fileheader) + 0xE60 (GlobalGoal[10]) + 0x1306 (Team[10]) = 0x4556 (17750 dec)
					streams.TargetStream.Position = 17750 + (FGs + 2) * 3646 + Messages * 162 + Briefs[1] * 20;  //place for next insert command  [JB] Modified to FGs+2
					streams.TargetStream.Position += 0x4414 + brief1StringSize + 384 + 0x000A + 2;  //briefing(minus strings) + XvT string list size + 192 shorts for empty messages in XWA + start of Brief2 event list
					context.WriteToTargetBuffer(26); streams.TargetStream.Position++; streams.TargetWriter.Write(Briefs[1]);        //Add command
					fgIcons[i] = Briefs[1];     // store the Icon# for the FG
					context.SourceCursor = XvTPos + 0x52; streams.TargetWriter.Write(streams.SourceReader.ReadByte()); streams.TargetStream.Position++;        //Ship
					context.SourceCursor = XvTPos + 0x57; streams.TargetWriter.Write(streams.SourceReader.ReadByte()); streams.TargetStream.Position += 3;     //IFF
					context.SourceCursor = XvTPos + 0x484;  //Offset for BP2
					context.WriteToTargetBuffer(28); streams.TargetStream.Position++; streams.TargetWriter.Write(Briefs[1]);        //Move command
					streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //BP X
					context.SourceCursor += 0x2A;
					streams.TargetWriter.Write(streams.SourceReader.ReadInt16());   //BP Y
					Briefs[1]++;
				}
			}
			streams.TargetStream.Position = XWAPos + 0xDC7;
			context.SourceCursor = XvTPos + 0x523;
			streams.TargetWriter.Write(streams.SourceReader.ReadInt32());        //CM -> Global Unit
			streams.TargetStream.Position++;
			context.SourceCursor += 9;
			streams.TargetWriter.Write(streams.SourceReader.ReadBytes(48));   //Optionals
			context.SourceCursor = XvTPos + 0x562;
			streams.TargetStream.Position = XWAPos + 0xE3E;
		}
    }
}
