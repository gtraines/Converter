using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Idmr.Conversions.Converters
{
    public class TieFighterConverter : ConverterBase
    {

        protected override bool InnerTo(ConversionContext context)
        {
			FileStream TIE, XvT;
			TIE = File.OpenRead(txtExist.Text);
			XvT = File.Open(txtSave.Text, FileMode.Create, FileAccess.ReadWrite);
			BinaryReader br = new BinaryReader(TIE);
			BinaryWriter bw = new BinaryWriter(XvT);
			BinaryReader brXvT = new BinaryReader(XvT);
			long XvTPos;
			byte format = 12;  //[JB] Added platform selection for XvT/BoP
			if (chkXvtBop.Checked) format = 14;
			XvT.WriteByte(format);          //XvT format

			XvT.Position = 2;
			TIE.Position = 2;
			short i, j;
			bw.Write(br.ReadInt32());           //# of FGs and messages
			TIE.Position = 2;
			short FGs = br.ReadInt16();         //store them
			short Messages = br.ReadInt16();
			if (chkXvtBop.Checked)  //[JB] patch in
			{
				XvT.Position = 0x64;
				XvT.WriteByte(3);  //MPtraining
			}
			TIE.Position = 0x1CA;
			XvT.Position = 0xA4;
			#region Flight Groups
			for (i = 0; i < FGs; i++)       //Flight Groups
			{
				long TIEPos = TIE.Position;
				XvTPos = XvT.Position;
				bw.Write(br.ReadBytes(12));     //FG Name
				TIE.Position = TIEPos + 24;
				XvT.Position = XvTPos + 40;
				bw.Write(br.ReadBytes(12));     //Cargo
				XvT.Position = XvTPos + 60;
				bw.Write(br.ReadBytes(12));     //Special Cargo
				XvT.Position = XvTPos + 80;
				bw.Write(br.ReadBytes(8));      //SC ship# -> IFF
				TIE.Position -= 1;
				// if player is Imperial, have to switch Team/IFF values, since Imps have to be Team 1 (0) and Rebs are Team 2 (1)
				XvT.Position = XvTPos + 88;
				switch (TIE.ReadByte())
				{
					case 0:
						if (optImp.Checked) XvT.WriteByte(1);
						else XvT.WriteByte(0);
						break;
					case 1:
						if (optImp.Checked) XvT.WriteByte(0);
						else XvT.WriteByte(1);
						break;
					default:
						TIE.Position--;
						XvT.WriteByte(br.ReadByte());
						break;
				}
				bw.Write(br.ReadBytes(9));      //AI -> # of waves
				TIE.Position += 1;
				XvT.Position += 2;
				if (TIE.ReadByte() != 0)
				{
					XvT.WriteByte(1);                                                       //Player 1
					XvT.Position++;
					TIE.Position--;
					XvT.WriteByte(System.Convert.ToByte(TIE.ReadByte() - 1));                      //Player's craft #
				}
				else XvT.Position += 3;
				bw.Write(br.ReadBytes(3));      //Orientation values
				TIE.Position = TIEPos + 73;
				XvT.Position = XvTPos + 109;
				bw.Write(br.ReadBytes(9));      //Arrival trigger 1&2
				XvT.Position += 2;
				XvT.WriteByte(br.ReadByte());                              //trigger 1 AND/OR 2
				TIE.Position += 1;
				XvT.Position = XvTPos + 134;
				bw.Write(br.ReadBytes(6));      //Arrv delay -> Dep trigger
				TIE.Position += 2;
				XvT.Position += 9;
				XvT.WriteByte(br.ReadByte());                              //Abort trigger
				TIE.Position += 3;
				XvT.Position += 4;
				bw.Write(br.ReadBytes(8));      //Arrv/Dep methods
				bw.Write(br.ReadBytes(18));     //Order 1
				XvT.Position += 64;
				bw.Write(br.ReadBytes(18));     //Order 2
				XvT.Position += 64;
				bw.Write(br.ReadBytes(18));     //Order 3
				XvT.Position = XvTPos + 502;
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(1);                                                       // must NOT
					XvT.WriteByte(2);                                                       // be destroyed
				}
				else XvT.WriteByte(System.Convert.ToByte(j));                                      //Primary Goal
				byte b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				XvT.WriteByte(b);                                                           //Amount
				XvT.WriteByte(System.Convert.ToByte((j != 10 && j != 9) ? 1 : 0));                 //250 points  [JB] Changed to 250 points only if a condition exists (not "FALSE") but also not "must have survived"
				if (j != 10) XvT.WriteByte(1);                                              //Enable Primary Goal
				else XvT.WriteByte(0);
				XvT.Position += 73;
				XvT.WriteByte(2);                                                           //Secondary Goal
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(3);                                                       // BONUS must NOT
					XvT.WriteByte(2);                                                       // be destroyed
				}
				else XvT.WriteByte(System.Convert.ToByte(j));
				b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				XvT.WriteByte(b);
				XvT.WriteByte(System.Convert.ToByte((j != 10) ? 1 : 0));                           //250 points  [JB] Changed to 250 points only if a condition exists (not "FALSE")
				if (j != 10) XvT.WriteByte(1);
				else XvT.WriteByte(0);
				XvT.Position += 73;
				XvT.WriteByte(2);                                                           //Secret Goal
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(3);                                                       // BONUS must NOT
					XvT.WriteByte(2);                                                       // be destroyed
				}
				else XvT.WriteByte(System.Convert.ToByte(j));
				b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				XvT.WriteByte(b);
				XvT.Position += 1;
				if (j != 10) XvT.WriteByte(1);
				else XvT.WriteByte(0);
				XvT.Position += 73;
				XvT.WriteByte(2);                                                           //Bonus Goal
				j = br.ReadByte();
				if (j == 9)                                                                 // if EXIST
				{
					XvT.Position--;
					XvT.WriteByte(3);                                                       // BONUS must NOT
					XvT.WriteByte(2);                                                       // be destroyed
				}
				else XvT.WriteByte(System.Convert.ToByte(j));
				b = br.ReadByte();
				if (b == 1) b++;    // 50%
				else if (b >= 2) b += 2;    // >=1 and on
				XvT.WriteByte(b);
				XvT.WriteByte(br.ReadByte());      //Bonus points, will need fiddling
				if (j != 10) XvT.WriteByte(1);
				else XvT.WriteByte(0);
				XvT.Position += 74;
				XvT.WriteByte(10);                                  //10 is the 'null' trigger (goal 5)
				XvT.Position += 77;
				XvT.WriteByte(10);                                  //goal 6
				XvT.Position += 77;
				XvT.WriteByte(10);                                  //goal 7
				XvT.Position += 77;
				XvT.WriteByte(10);                                  //goal 8
				TIE.ReadByte();
				XvT.Position = XvTPos + 1126;
				bw.Write(br.ReadBytes(30));     //X points
				XvT.Position += 14;
				bw.Write(br.ReadBytes(30));     //Y points
				XvT.Position += 14;
				bw.Write(br.ReadBytes(30));     //Z points
				XvT.Position += 14;
				bw.Write(br.ReadBytes(30));     //Enable points
				XvT.Position += 90;                                                         //goto End
				TIE.Position += 4;
			}
			#endregion
			#region Messages
			for (i = 0; i < Messages; i++)
			{
				XvT.WriteByte(System.Convert.ToByte(i));
				XvT.Position++;
				bw.Write(br.ReadBytes(64)); //Color & message
				XvT.WriteByte(1);
				XvT.Position += 9;
				bw.Write(br.ReadBytes(8));  //triggers 1 & 2
				XvT.Position++;
				TIE.Position += 17;
				XvT.WriteByte(br.ReadByte());                          //trigger 1 AND/OR 2
				TIE.Position -= 2;
				XvT.Position += 28;
				XvT.WriteByte(br.ReadByte());                          //Delay
				TIE.Position++;
				XvT.Position++;
			}
			#endregion
			XvT.WriteByte(3); XvT.Position++;   //Unknown
			#region Global Goals
			bw.Write(br.ReadBytes(8));          //Prim Goal trigger 1 & 2
			XvT.Position += 2;
			TIE.Position += 17;
			XvT.WriteByte(br.ReadByte());       //Prim trigger 1 AND/OR 2
			XvT.Position += 73;
			TIE.Position += 2;
			bw.Write(br.ReadBytes(8));          //Sec Goal trigger 1 & 2
			XvT.Position += 2;
			TIE.Position += 17;
			XvT.WriteByte(br.ReadByte());       //Sec Goal trigger A/O

			//[JB] Convert TIE bonus global goals into a second set of XvT secondary goals
			TIE.Position += 2; //Skip 2 bytes after the previous and/or byte to reach the start of the bonus goal triggers
			bw.Write(br.ReadBytes(8));          //Bonus Goal trigger 1 & 2
			XvT.Position += 2;
			TIE.Position += 17;
			XvT.WriteByte(br.ReadByte());       //Bonus Goal trigger A/O
			XvT.Position += 17;
			XvT.WriteByte(0);   //And/Or
			XvT.Position += 2;  //Jump to the end of the Global Goal block, which happens to be the start of the second teams's global goal block
			XvTPos = XvT.Position;

			//[JB] Patch to convert all "must be FALSE" conditions to TRUE so that and/or doesn't cause conflicts
			XvT.Position -= 0x2A; //Rewind back to start of XvT Secondary, Trigger 1
			if (XvT.ReadByte() == 0x0A) { XvT.Position--; XvT.WriteByte(0); }  //Trig1
			XvT.Position += 3;
			if (XvT.ReadByte() == 0x0A) { XvT.Position--; XvT.WriteByte(0); }  //Trig2
			XvT.Position += 3;
			XvT.Position += 3;  //Jump the gap between Trig 1/2 and T3/4
			if (XvT.ReadByte() == 0x0A) { XvT.Position--; XvT.WriteByte(0); }  //Trig3
			XvT.Position += 3;
			if (XvT.ReadByte() == 0x0A) { XvT.Position--; XvT.WriteByte(0); }  //Trig4

			XvT.Position = XvTPos;
			for (j = 0; j < 9; j++)
			{
				XvT.WriteByte(3);
				XvT.Position += 127;
			}
			#endregion
			TIE.Position = 0x19A;
			#region IFF/Teams
			XvTPos = XvT.Position;
			XvT.WriteByte(1);
			XvT.Position++;
			if (optImp.Checked) XvT.Write(System.Text.Encoding.ASCII.GetBytes("Imperial"), 0, 7);
			else XvT.Write(System.Text.Encoding.ASCII.GetBytes("Rebel"), 0, 5);
			XvT.Position = XvTPos + 0x1A;
			XvT.WriteByte(1);                                   //Team 1 Allies
			XvT.Position++;                                     //Team 2 bad guys
			if (TIE.ReadByte() == 49) XvT.WriteByte(1); else XvT.Position++;            //IFF 3 stance (blue)
			TIE.Position = 422;
			if (TIE.ReadByte() == 49) XvT.WriteByte(1); else XvT.Position++;            //IFF 4 stance (purple)
			TIE.Position = 434;
			if (TIE.ReadByte() == 49) XvT.WriteByte(1); else XvT.Position++;            //IFF 5 stance (red)
			TIE.Position = 446;
			if (TIE.ReadByte() == 49) XvT.WriteByte(1); else XvT.Position++;            //IFF 6 stance (purple)
			XvT.Position += 4;
			TIE.Position = 24;
			bw.Write(br.ReadBytes(128)); //Primary mission complete
			TIE.Position += 128;
			bw.Write(br.ReadBytes(128)); //Primary failed
			TIE.Position -= 256;  //[JB] Fixed offset, was off by one.
			bw.Write(br.ReadBytes(128)); //Secondary complete
			TIE.Position = 410;
			XvT.Position += 67;
			XvTPos = XvT.Position;
			XvT.WriteByte(1); XvT.Position++;
			if (optImp.Checked) XvT.Write(System.Text.Encoding.ASCII.GetBytes("Rebel"), 0, 5);
			else XvT.Write(System.Text.Encoding.ASCII.GetBytes("Imperial"), 0, 7);
			XvT.Position = XvTPos + 0x1E7;
			XvT.WriteByte(1); XvT.Position++;
			if (TIE.ReadByte() != 49) TIE.Position--;                                   //check for hostile char
			bw.Write(br.ReadBytes(11));     //IFF 3 name
			XvT.Position += 474; XvT.WriteByte(1); XvT.Position++;
			TIE.Position = 422;
			if (TIE.ReadByte() != 49) TIE.Position--;
			bw.Write(br.ReadBytes(11));     //IFF 4 name
			XvT.Position += 474; XvT.WriteByte(1); XvT.Position++;
			TIE.Position = 434;
			if (TIE.ReadByte() != 49) TIE.Position--;
			bw.Write(br.ReadBytes(11));     //IFF 5 name
			XvT.Position += 474; XvT.WriteByte(1); XvT.Position++;
			TIE.Position = 446;
			if (TIE.ReadByte() != 49) TIE.Position--;
			bw.Write(br.ReadBytes(11));     //IFF 6 name
			XvT.Position += 474; XvT.WriteByte(1);                                      //markers infront of other IFF name spots
			XvT.Position += 486; XvT.WriteByte(1);
			XvT.Position += 486; XvT.WriteByte(1);
			XvT.Position += 486; XvT.WriteByte(1);
			#endregion
			XvT.Position += 0x1E6;
			TIE.Position = 0x21E + 0x124 * FGs + 0x5A * Messages;
			XvTPos = XvT.Position;
			#region Briefing
			bw.Write(br.ReadBytes(0x32A)); //Briefing
			XvT.WriteByte(1); XvT.Position += 9;
			XvT.Position = XvTPos;
			j = (short)(brXvT.ReadInt16() * 0x14 / 0xC);        // adjust overall briefing length
			XvT.Position -= 2;
			bw.Write(j);
			XvT.Position += 8;
			for (i = 0; i < 0x320; i += 4)      // work our way through length of briefing. i automatically increases by 4 per event
			{
				j = brXvT.ReadInt16();
				if (j == 0x270F) break;     // stop check at t=9999, end briefing
				j = (short)(j * 0x14 / 0xC);
				XvT.Position -= 2;
				bw.Write(j);
				j = brXvT.ReadInt16();       // now get the event type
				if (j == 7)     // Zoom map command
				{
					j = (short)(brXvT.ReadInt16() * 58 / 47); // X
					XvT.Position -= 2;
					bw.Write(j);
					j = (short)(brXvT.ReadInt16() * 88 / 47); // Y
					XvT.Position -= 2;
					bw.Write(j);
					i += 4;
				}
				else
				{
					XvT.Position += 2 * BRF[j];     // skip over vars
					i += (short)(2 * BRF[j]);    // increase length counter by skipped vars
				}
			}
			#endregion
			XvT.Position = 0x1BDE + 0x562 * FGs + 0x74 * Messages;
			#region Briefing tags & strings
			int BriefingTagLength = 0;
			for (i = 0; i < 64; i++)
			{
				j = br.ReadInt16();       //check length..  (will always be <256)
				BriefingTagLength += 2 + j;  //[JB] Calculate briefing size so we can factor it into calculations to find the description location.
				bw.Write(j);             //..write length..
				if (j != 0)                                     //and copy if not 0
					for (int k = 0; k < j; k++) XvT.WriteByte(br.ReadByte());
			}
			for (i = 0; i < 7; i++)
			{   // End Briefing event at time=9999
				XvT.WriteByte(0xC8); XvT.Position += 5; XvT.WriteByte(2); XvT.Position += 3; XvT.WriteByte(0xF); XvT.WriteByte(0x27); XvT.WriteByte(0x22);
				XvT.Position += 0x3A7;
			}
			#endregion
			#region Mission Questions

			string preMissionQuestions = "";
			for (i = 0; i < 10; i++)
			{
				int len = br.ReadInt16();
				if (len == 0) continue;
				if (len == 1)
				{
					TIE.Position++;
					continue;
				}
				byte[] buffer = new byte[len];
				TIE.Read(buffer, 0, len);
				string s = System.Text.Encoding.ASCII.GetString(buffer);
				int sep = s.IndexOf((char)0xA);
				if (sep >= 0)
				{
					string q = s.Substring(0, sep);
					string a = s.Substring(sep + 1);
					a = a.Replace((char)0xA, ' ');
					a = a.Replace((char)0x2, '[');
					a = a.Replace((char)0x1, ']');
					if (preMissionQuestions.Length > 0) preMissionQuestions += "$";
					preMissionQuestions += q + "$$" + a + "$";
				}
			}
			string postMissionFail = "";
			string postMissionSuccess = "";
			for (i = 0; i < 10; i++)
			{
				int len = br.ReadInt16();
				if (len == 0) continue;
				if (len == 3)
				{
					TIE.Position += 3;
					continue;
				}
				int qCondition = TIE.ReadByte();
				int qType = TIE.ReadByte();
				byte[] buffer = new byte[len - 2];  //Length includes condition/type bytes.
				TIE.Read(buffer, 0, len - 2);

				if (qCondition == 0 || qType == 0) continue;

				string s = System.Text.Encoding.ASCII.GetString(buffer);
				if (buffer[len - 3] == 0xFF)  //If this is the last byte of the string data, remove it.
					s = s.Remove(s.Length - 1);
				int sep = s.IndexOf((char)0xA);
				if (sep >= 0)
				{
					string q = s.Substring(0, sep);
					string a = s.Substring(sep + 1);
					a = a.Replace((char)0xA, ' ');
					a = a.Replace((char)0x2, '[');
					a = a.Replace((char)0x1, ']');

					if (qCondition == 4)
					{
						if (postMissionSuccess.Length > 0) postMissionSuccess += "$";
						postMissionSuccess += q + "$$" + a + "$";
					}
					else if (qCondition == 5)
					{
						if (postMissionFail.Length > 0) postMissionFail += "$";
						postMissionFail += q + "$$" + a + "$";
					}
				}
			}
			#endregion
			#region Mission Description
			XvT.Position = 0xA4 + (FGs * 0x562) + (Messages * 0x74) + (0x80 * 10) + (0x1E7 * 10); //Header + FGs + Messages + 10 Globals + 10 Teams
			XvT.Position += (0x334 + BriefingTagLength) + (0x3B4 * 7);  //Briefing 1 (plus tag/string lengths) + 7 empty briefings (static size)
			XvT.Position += (0x600 * FGs) + 0x5A00;  //FGGoalStrings(FGs*8*3*64) + GlobalGoalStrings(10*3*4*3*64)
			XvT.Position += (0xC00 * 10);  //Empty GlobalGoalStrings data.

			int maxLength = (format == 12 ? 0x400 : 0x1000);
			if (preMissionQuestions.Length > maxLength) preMissionQuestions = preMissionQuestions.Remove(maxLength);
			if (postMissionSuccess.Length > maxLength) postMissionSuccess = postMissionSuccess.Remove(maxLength);
			if (postMissionFail.Length > maxLength) postMissionFail = postMissionFail.Remove(maxLength);

			byte[] desc;
			if (format == 12) desc = System.Text.Encoding.ASCII.GetBytes(preMissionQuestions);
			else desc = System.Text.Encoding.ASCII.GetBytes(postMissionSuccess);

			XvTPos = XvT.Position;
			XvT.Write(desc, 0, desc.Length);
			XvT.Position = XvTPos + maxLength - 1;
			XvT.WriteByte(0);

			if (format == 14)
			{
				XvTPos += maxLength;
				desc = System.Text.Encoding.ASCII.GetBytes(postMissionFail);
				XvT.Write(desc, 0, desc.Length);
				XvT.Position = XvTPos + maxLength - 1;
				XvT.WriteByte(0);

				XvTPos += maxLength;
				desc = System.Text.Encoding.ASCII.GetBytes(preMissionQuestions);
				XvT.Write(desc, 0, desc.Length);
				XvT.Position = XvTPos + maxLength - 1;
				XvT.WriteByte(0);
			}
			#endregion

			return true;
		}

		//TODO: Some of the delays may still be off due to different conversion factors

		protected bool ToXvT(ConversionStreams streams)
		{
			

			MessageBox.Show("Conversion completed", "Finished");
		}

		void TIE2XWA(string fromFileName, string toFileName)
		{
			//instead of writing it all out again, cheat and use the other two
			string save, exist;
			save = txtSave.Text;
			exist = txtExist.Text;
			txtSave.Text = "temp.tie";
			TIE2XvT();
			txtSave.Text = save;
			txtExist.Text = "temp.tie";
			lblType.Text = "XvT";
			XWingVsTieConverter.ConvertTo(fromFileName, toFileName, GameType.XWA);
			lblType.Text = "TIE";
			txtExist.Text = exist;
			File.Delete("temp.tie");
			MessageBox.Show("Conversion completed", "Finished");
		}
	}
}
