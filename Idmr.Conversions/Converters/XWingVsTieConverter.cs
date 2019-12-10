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
			var isMultiplayer = false;
			var isBoPMision = true;
			short[] fgIcons;

			context.WriteToTargetBuffer(18, true);
			context.SourceCursor = 2;
			//short i, j;
			short FGs = context.ReadSourceInt16();
			short Messages = context.ReadSourceInt16();
			streams.TargetWriter.Write((short)(FGs + 2)); // [JB] Modified to +2 since generated skirmish files have two backdrops for ambient lighting
			streams.TargetWriter.Write(Messages);
			fgIcons = new short[FGs];

			streams.TargetStream.Position = 8;
			context.WriteToTargetBuffer(1); 
			streams.TargetStream.Position = 11; 
			context.WriteToTargetBuffer(1);        //unknowns

			streams.TargetStream.Position = 100;
			streams.TargetWriter.Write(System.Text.Encoding.ASCII.GetBytes("The Final Frontier"));    //make a nice Region name :P
			streams.TargetStream.Position = 0x23AC; context.WriteToTargetBuffer(6);                        //starting hangar
			streams.TargetStream.Position++;
			context.SourceCursor = 0x66; streams.TargetWriter.Write(streams.SourceReader.ReadByte());       //time limit (minutes)
			streams.TargetStream.Position = 0x23B3; context.WriteToTargetBuffer(0x62);                     //unknown
			streams.TargetStream.Position = 0x23F0;

			//[JB] Jumping ahead to get the briefing locations before we load in the FGs.
			long brief1BlockStart = 0xA4 + (0x562 * FGs) + (0x74 * Messages) + 0x500 + 0x1306;  //FGs, messages, teams, global goals
			context.SourceCursor = brief1BlockStart + 0x334;  //Jump to tags
			long brief1StringSize = 0;
			long brief1EndSize = 0;
			for (var idx = 0; idx < 64; idx++)   //32 tags + 32 strings
			{
				int len = streams.SourceReader.ReadInt16();
				brief1StringSize += 2;
				context.SourceCursor += len;
				brief1StringSize += len;
			}
			brief1EndSize = context.SourceCursor;
			context.SourceCursor = 0xA4;  //Rewind back to start of FG data
								  //[JB] End loading briefing offset;

			#region Flight Groups
			long XvTPos; //[JB] Sometimes need to bookmark XvT too for better offset handling
			long XWAPos;
			bool Player = false;
			int PlayerCraft = 0; //[JB] for combat engagements
			short[] Briefs = new short[2];
			Briefs[0] = 0;
			Briefs[1] = 0;
			for (var i = 0; i < FGs; i++)
			{
				XvTPos = context.SourceCursor;
				XWAPos = streams.TargetStream.Position;
				streams.TargetWriter.Write(streams.SourceReader.ReadBytes(20));   //name
				byte[] d1 = new byte[2];
				byte[] d2 = new byte[2];
				byte[] buffer = new byte[4];
				byte[] buffer2 = new byte[8];
				byte[] des2 = new byte[4];
				streams.SourceStream.Read(buffer2, 0, 8);
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
			Random rnd = new Random();
			int craft1 = rnd.Next(1, 59);
			int craft2 = rnd.Next(63, 102);
			short[] coord1 = { 0, 0, 0 };
			short[] coord2 = { 0, 0, 0 };
			coord1[rnd.Next(0, 2)] = 1; //[JB] ensures backdrop isn't on origin
			coord2[rnd.Next(0, 2)] = -1;

			//okay, now write in the default Backdrop
			XWAPos = streams.TargetStream.Position;
			streams.TargetWriter.Write(System.Text.Encoding.ASCII.GetBytes("1.0 1.0 1.0"));   //Name
			streams.TargetStream.Position = XWAPos + 20;
			context.WriteToTargetBuffer(255); context.WriteToTargetBuffer(255); streams.TargetStream.Position += 3; context.WriteToTargetBuffer(255); context.WriteToTargetBuffer(255);  //EnableDesignation1, EnableDesignation2, ... GlobalCargoIndex GlobalSpecialCargoIndex 
			streams.TargetStream.Position = XWAPos + 40;
			streams.TargetWriter.Write(System.Text.Encoding.ASCII.GetBytes("1.0"));   //Brightness
			streams.TargetStream.Position = XWAPos + 60;
			streams.TargetWriter.Write(System.Text.Encoding.ASCII.GetBytes("1.9"));   //Size (match skirmish output)
			streams.TargetStream.Position = XWAPos + 105; context.WriteToTargetBuffer(2);           //SpecialCargoCraft?
			streams.TargetStream.Position++; context.WriteToTargetBuffer(183); context.WriteToTargetBuffer(1);
			streams.TargetStream.Position += 3; context.WriteToTargetBuffer(4); //[JB] Changed to IFF Red since it is used less frequently, and is less likely to interfere with IFF triggers.
			streams.TargetStream.Position = XWAPos + 113; context.WriteToTargetBuffer(9);   //[JB] Team (so it doesn't interfere with triggers)
			streams.TargetStream.Position = XWAPos + 120; context.WriteToTargetBuffer(31);  //[JB] Global group (for same reason)
			streams.TargetStream.Position = XWAPos + 2827; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10);
			streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79;
			streams.TargetStream.Position = XWAPos + 3466;
			streams.TargetWriter.Write(coord1[0]);
			streams.TargetWriter.Write(coord1[1]);
			streams.TargetWriter.Write(coord1[2]);
			streams.TargetStream.Position++; context.WriteToTargetBuffer(1);
			streams.TargetStream.Position = XWAPos + 3602;
			context.WriteToTargetBuffer((byte)craft1); //[JB] Set backdrop value to random(1-59)
			streams.TargetStream.Position = XWAPos + 3646;

			//[JB] Adding a second backdrop, since the game generates two backdrops for skirmish files.  Offers better ambient lighting for the player.
			XWAPos = streams.TargetStream.Position;
			streams.TargetWriter.Write(System.Text.Encoding.ASCII.GetBytes("1.0 1.0 1.0"));   //Name
			streams.TargetStream.Position = XWAPos + 20;
			context.WriteToTargetBuffer(255); context.WriteToTargetBuffer(255); streams.TargetStream.Position += 3; context.WriteToTargetBuffer(255); context.WriteToTargetBuffer(255);  //[JB] Modified to update both global cargo values.
			streams.TargetStream.Position = XWAPos + 40;
			streams.TargetWriter.Write(System.Text.Encoding.ASCII.GetBytes("1.0"));   //Brightness
			streams.TargetStream.Position = XWAPos + 60;
			streams.TargetWriter.Write(System.Text.Encoding.ASCII.GetBytes("1.0"));   //Size
			streams.TargetStream.Position = XWAPos + 105; context.WriteToTargetBuffer(2);
			streams.TargetStream.Position++; context.WriteToTargetBuffer(183); context.WriteToTargetBuffer(1);
			streams.TargetStream.Position += 3; context.WriteToTargetBuffer(4);
			streams.TargetStream.Position = XWAPos + 113; context.WriteToTargetBuffer(9);   //[JB] Team (so it doesn't interfere with triggers)
			streams.TargetStream.Position = XWAPos + 120; context.WriteToTargetBuffer(31);  //[JB] Global group (for same reason)
			streams.TargetStream.Position = XWAPos + 2827; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10);
			streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79; context.WriteToTargetBuffer(10); streams.TargetStream.Position += 79;
			streams.TargetStream.Position = XWAPos + 3466;
			streams.TargetWriter.Write(coord2[0]);
			streams.TargetWriter.Write(coord2[1]);
			streams.TargetWriter.Write(coord2[2]);
			streams.TargetStream.Position++; context.WriteToTargetBuffer(1);
			streams.TargetStream.Position = XWAPos + 3602; context.WriteToTargetBuffer((byte)craft2); //[JB] Set backdrop value to random(63-102)
			streams.TargetStream.Position = XWAPos + 3646;

			#endregion
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
