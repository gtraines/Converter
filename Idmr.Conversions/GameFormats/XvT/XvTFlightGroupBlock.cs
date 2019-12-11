using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idmr.Conversions.GameFormats.XvT
{
    public class XvTFlightGroupsBlock
    {
		public List<XvTFlightGroup> FlightGroups { get; set; }

		public XvTFlightGroupsBlock(int flightGroupCount, ConversionContext context)
        {
			FlightGroups = new List<XvTFlightGroup>();

            context.SourceCursor = 0xA4;  //Rewind back to start of FG data
										  //[JB] End loading briefing offset;



			#region Flight Groups
			
			int PlayerCraft = 0; //[JB] for combat engagements
			short[] Briefs = new short[2];
			Briefs[0] = 0;
			Briefs[1] = 0;

			foreach (var fgIdx in Enumerable.Range(0, flightGroupCount))
			{
				FlightGroups.Add(new XvTFlightGroup(context));
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
		}

		public long BlockStart => 0xA4;


	}
}
