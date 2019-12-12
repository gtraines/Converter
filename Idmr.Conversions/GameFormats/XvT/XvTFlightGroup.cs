using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.GameFormats.XvT
{
    public class XvTFlightGroup : XvTBlockBase
    {
		public readonly long StartXvTPos; //[JB] Sometimes need to bookmark XvT too for better offset handling
		public readonly long StartXWAPos;
		bool Player = false;
		int PlayerCraft = 0; //[JB] for combat engagements
		public List<XvTMessage> Messages { get; set; }
		public byte[] FlightGroupName { get; set; }
		public XvTFlightGroupDesignation FlightGroupDesignation { get; set; }

		public byte[] XwaFlightGroupDesignation;

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
			byte[] flightGroupDesignationBytes = new byte[8];
			
			context.SourceStream.Read(flightGroupDesignationBytes, 0, 8);
			FlightGroupDesignation = new XvTFlightGroupDesignation(flightGroupDesignationBytes);
			XwaFlightGroupDesignation = FlightGroupDesignation.ToXWingAllianceDesignationBytes();

			context.WriteToTargetBuffer(XwaFlightGroupDesignation[0]); 
			context.WriteToTargetBuffer(XwaFlightGroupDesignation[1]);
			context.WriteToTargetBuffer(XwaFlightGroupDesignation[2]); 
			context.WriteToTargetBuffer(XwaFlightGroupDesignation[3]);

			context.SourceCursor -= 8;
			context.TargetStream.Position++;       //Skip unknown

			context.WriteToTargetBuffer(255); //Global cargos set to none
			context.WriteToTargetBuffer(255);

			context.TargetStream.Position = StartXWAPos + 40;
			context.SourceCursor += 20;
			context.TargetWriter.Write(context.ReadBytes(40));   //Cargo & Special
			context.TargetStream.Position = StartXWAPos + 0x69;
			context.TargetWriter.Write(context.ReadBytes(30));           //SC ship# -> Arrival Difficulty
			context.SourceCursor = StartXWAPos + 0x64;
			// [JB] Modified
			if (!context.IsMultiplayerMission)
			{
				if (context.ReadByte() != 0 && Player == false) 
				{ 
					Player = true; 
				}              //check for multiple player craft, take the first one
				else
				{
					context.TargetCursor = StartXvTPos + 0x7D;
					context.WriteToTargetBuffer(0);
				}
			}
			if (context.ReadByte() != 0) { PlayerCraft++; }

			context.TargetStream.Position = StartXWAPos + 0x88;
			context.SourceCursor = StartXvTPos + 0x6E;
			context.TargetWriter.Write(context.ReadSourceInt32());                //Arival trigger 1 (cheating and using Int32 since it's 4 bytes)...
			ShipFix(context.SourceStream, context.TargetStream);
			context.TargetWriter.Write(context.ReadSourceInt32());                //... and 2
			ShipFix(context.SourceStream, context.TargetStream);
			context.TargetStream.Position += 2;
			context.SourceCursor += 2;
			context.TargetWriter.Write(context.ReadByte());                                    //AT 1 AND/OR 2
			context.TargetStream.Position++;
			context.TargetWriter.Write(context.ReadSourceInt32());                //AT 3
			ShipFix(context.SourceStream, context.TargetStream);
			context.TargetWriter.Write(context.ReadSourceInt32());                //AT 4
			ShipFix(context.SourceStream, context.TargetStream);
			context.TargetStream.Position += 2;
			context.SourceCursor += 2;
			context.TargetWriter.Write(context.ReadByte());                                    //AT 3 AND/OR 4
			context.TargetStream.Position++;
			context.TargetWriter.Write(context.ReadSourceInt64());                //AT 1/2 AND/OR 3/4 -> DT (8 bytes)
			ShipFix(context.SourceStream, context.TargetStream);
			context.TargetWriter.Write(context.ReadSourceInt32());                //DT 2
			ShipFix(context.SourceStream, context.TargetStream);
			context.TargetStream.Position += 2;
			context.SourceCursor += 2;
			context.TargetWriter.Write(context.ReadByte());                                    //DT 1 AND/OR 2
			context.TargetStream.Position += 3;
			context.SourceCursor += 2;
			context.TargetWriter.Write(context.ReadByte());                                    //Abort trigger
			context.SourceCursor += 4;
			context.TargetStream.Position += 3;
			context.TargetWriter.Write(context.ReadSourceInt64());                //Arr/Dep methods
			long XvTOrderStart = context.SourceCursor;
			long XWAOrderStart = context.TargetStream.Position;
			long XvTSubOrderStart = context.SourceCursor;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
			long XWASubOrderStart = context.TargetStream.Position;
			context.TargetWriter.Write(context.ReadBytes(18));       //Order 1
			ShipOrderFix(context.SourceStream, context.TargetStream);
			context.SourceCursor = StartXvTPos + 0x46E;
			for (var j = 0; j < 8; j++)
			{
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP X
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP Y
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP Z
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP enabled
				context.SourceCursor -= 0x84;
			}

			//[JB] Patch wait times
			context.SourceCursor = XvTSubOrderStart;
			context.TargetStream.Position = XWASubOrderStart;
			ConvertOrderTime(context.SourceStream, context.TargetStream);
			//Copy order speed
			context.SourceCursor = XvTSubOrderStart + 18;
			context.TargetStream.Position = XWASubOrderStart + 18;
			context.TargetWriter.Write(context.ReadByte());
			//After XvT speed comes flight group player designation, load it to patch the role later. 
			byte[] role = new byte[16];
			context.SourceStream.Read(role, 0, 16);
			context.TargetStream.Position = StartXWAPos + 0x50; //Patch in the display role for the player FG slot screen.
			context.TargetStream.Write(role, 0, 16);
			//[JB] End patch code.

			context.TargetStream.Position = StartXWAPos + 0x15E;
			context.SourceCursor = StartXvTPos + 0xF4;
			XvTSubOrderStart = context.SourceCursor;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
			XWASubOrderStart = context.TargetStream.Position;
			context.TargetWriter.Write(context.ReadBytes(18));       //Order 2
			ShipOrderFix(context.SourceStream, context.TargetStream);
			context.SourceCursor = StartXvTPos + 0x46E;
			for (var j = 0; j < 8; j++)
			{
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP X
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP Y
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP Z
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP enabled
				context.SourceCursor -= 0x84;
			}
			//[JB] Patch wait times
			context.SourceCursor = XvTSubOrderStart;
			context.TargetStream.Position = XWASubOrderStart;
			ConvertOrderTime(context.SourceStream, context.TargetStream);
			//Copy order speed
			context.SourceCursor = XvTSubOrderStart + 18;
			context.TargetStream.Position = XWASubOrderStart + 18;
			context.TargetWriter.Write(context.ReadByte());
			//[JB] End patch code.

			context.TargetStream.Position = StartXWAPos + 0x1F2;
			context.SourceCursor = StartXvTPos + 0x146;
			XvTSubOrderStart = context.SourceCursor;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
			XWASubOrderStart = context.TargetStream.Position;
			context.TargetWriter.Write(context.ReadBytes(18));       //Order 3
			ShipOrderFix(context.SourceStream, context.TargetStream);
			context.SourceCursor = StartXvTPos + 0x46E;
			for (var j = 0; j < 8; j++)
			{
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP X
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP Y
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP Z
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP enabled
				context.SourceCursor -= 0x84;
			}
			//[JB] Patch wait times
			context.SourceCursor = XvTSubOrderStart;
			context.TargetStream.Position = XWASubOrderStart;
			ConvertOrderTime(context.SourceStream, context.TargetStream);
			//Copy order speed
			context.SourceCursor = XvTSubOrderStart + 18;
			context.TargetStream.Position = XWASubOrderStart + 18;
			context.TargetWriter.Write(context.ReadByte());
			//[JB] End patch code.

			context.TargetStream.Position = StartXWAPos + 0x286;
			context.SourceCursor = StartXvTPos + 0x198;
			XvTSubOrderStart = context.SourceCursor;  //[JB] ShipOFix modifies the offsets and I assume it's bad to add anything so I'm going to let the original code run then rewind to these offsets later to apply my patches.
			XWASubOrderStart = context.TargetStream.Position;
			context.TargetWriter.Write(context.ReadBytes(18));       //Order 4
			ShipOrderFix(context.SourceStream, context.TargetStream);
			context.SourceCursor = StartXvTPos + 0x46E;
			for (var j = 0; j < 8; j++)
			{
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP X
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP Y
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP Z
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //WP enabled
				context.SourceCursor -= 0x84;
			}
			//[JB] Patch wait times
			context.SourceCursor = XvTSubOrderStart;
			context.TargetStream.Position = XWASubOrderStart;
			ConvertOrderTime(context.SourceStream, context.TargetStream);
			//Copy order speed
			context.SourceCursor = XvTSubOrderStart + 18;
			context.TargetStream.Position = XWASubOrderStart + 18;
			context.TargetWriter.Write(context.ReadByte());
			//[JB] End patch code.

			context.SourceCursor = StartXvTPos + 0x1EA;
			context.TargetStream.Position = StartXWAPos + 0xA3A;
			context.TargetWriter.Write(context.ReadSourceInt32());        //jump to Order 4 T1
			ShipFix(context.SourceStream, context.TargetStream);
			context.TargetWriter.Write(context.ReadSourceInt32());        //jump to Order 4 T2
			ShipFix(context.SourceStream, context.TargetStream);
			context.TargetStream.Position += 2;
			context.SourceCursor += 2;
			context.TargetWriter.Write(context.ReadByte());                            //jump to Order 4 T 1AND/OR 2
			context.TargetStream.Position = StartXWAPos + 0xB0A;
			context.SourceCursor = StartXvTPos + 0x1F5;
			for (var j = 0; j < 8; j++)
			{
				context.TargetWriter.Write(context.ReadBytes(14));       //Goals
				context.SourceCursor += 0x40;
				context.TargetStream.Position += 0x42;
			}
			context.SourceCursor = StartXvTPos + 0x466;
			context.TargetStream.Position = StartXWAPos + 0xD8A;
			for (var j = 0; j < 3; j++)
			{
				context.TargetWriter.Write(context.ReadSourceInt16());   //SP X
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //SP Y
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //SP Z
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //SP enabled
				context.SourceCursor -= 0x84;
			}
			context.SourceCursor = StartXvTPos + 0x480;
			context.TargetWriter.Write(context.ReadSourceInt16());   //HYP X
			context.SourceCursor += 0x2A;
			context.TargetWriter.Write(context.ReadSourceInt16());   //HYP Y
			context.SourceCursor += 0x2A;
			context.TargetWriter.Write(context.ReadSourceInt16());   //HYP Z
			context.SourceCursor += 0x2A;
			context.TargetWriter.Write(context.ReadSourceInt16());   //HYP enabled
			context.SourceCursor = StartXvTPos + 0x506;
			if (context.SourceStream.ReadByte() == 1)        //okay, briefing time. if BP enabled..
			{
				//[JB] 0x23F0 (fileheader) + 0xE60 (GlobalGoal[10]) + 0x1306 (Team[10]) = 0x4556 (17750 dec)
				context.TargetStream.Position = 17762 
					+ (context.FlightGroupCount + 2) * 3646 
					+ Messages * 162 
					+ Briefs[0] * 20;  //place for next insert command  [JB] Modified to FGs+2
				context.WriteToTargetBuffer(26); context.TargetStream.Position++; context.TargetWriter.Write(Briefs[0]);        //Add command
				
				fgIcons[i] = Briefs[0];     // store the Icon# for the FG
				context.SourceCursor = StartXvTPos + 0x52; context.TargetWriter.Write(context.ReadByte()); context.TargetStream.Position++;        //Ship
				context.SourceCursor = StartXvTPos + 0x57; context.TargetWriter.Write(context.ReadByte()); context.TargetStream.Position += 3;     //IFF
				context.SourceCursor = StartXvTPos + 0x482;
				context.WriteToTargetBuffer(28); context.TargetStream.Position++; context.TargetWriter.Write(Briefs[0]);        //Move command
				context.TargetWriter.Write(context.ReadSourceInt16());   //BP X
				context.SourceCursor += 0x2A;
				context.TargetWriter.Write(context.ReadSourceInt16());   //BP Y
				Briefs[0]++;
			}
			if (context.IsMultiplayerMission)
			{
				context.SourceCursor = StartXvTPos + 0x508;
				if (context.SourceStream.ReadByte() == 1)        //okay, briefing time. if BP enabled..
				{
					//[JB] 0x23F0 (fileheader) + 0xE60 (GlobalGoal[10]) + 0x1306 (Team[10]) = 0x4556 (17750 dec)
					context.TargetStream.Position = 17750 
						+ (context.FlightGroupCount + 2) * 3646 + Messages * 162 + Briefs[1] * 20;  //place for next insert command  [JB] Modified to FGs+2
					context.TargetStream.Position += 0x4414 + brief1StringSize + 384 + 0x000A + 2;  //briefing(minus strings) + XvT string list size + 192 shorts for empty messages in XWA + start of Brief2 event list
					context.WriteToTargetBuffer(26); context.TargetStream.Position++; context.TargetWriter.Write(Briefs[1]);        //Add command
					fgIcons[i] = Briefs[1];     // store the Icon# for the FG
					context.SourceCursor = StartXvTPos + 0x52; context.TargetWriter.Write(context.ReadByte()); context.TargetStream.Position++;        //Ship
					context.SourceCursor = StartXvTPos + 0x57; context.TargetWriter.Write(context.ReadByte()); context.TargetStream.Position += 3;     //IFF
					context.SourceCursor = StartXvTPos + 0x484;  //Offset for BP2
					context.WriteToTargetBuffer(28); context.TargetStream.Position++; context.TargetWriter.Write(Briefs[1]);        //Move command
					context.TargetWriter.Write(context.ReadSourceInt16());   //BP X
					context.SourceCursor += 0x2A;
					context.TargetWriter.Write(context.ReadSourceInt16());   //BP Y
					Briefs[1]++;
				}
			}
			context.TargetStream.Position = StartXWAPos + 0xDC7;
			context.SourceCursor = StartXvTPos + 0x523;
			context.TargetWriter.Write(context.ReadSourceInt32());        //CM -> Global Unit
			context.TargetStream.Position++;
			context.SourceCursor += 9;
			context.TargetWriter.Write(context.ReadBytes(48));   //Optionals
			context.SourceCursor = StartXvTPos + 0x562;
			context.TargetStream.Position = StartXWAPos + 0xE3E;
		}
    }
}
