using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Idmr.Conversions.GameFormats.XvT
{
    public abstract class XvTBlockBase
    {
		protected void ShipFix(FileStream In, FileStream XWA)     //checks for Ship Type trigger, adjusts value
		{
			In.Position -= 3;
			if (In.ReadByte() == 2)
			{
				XWA.Position -= 2;
				XWA.WriteByte(Convert.ToByte(In.ReadByte() + 1));
				XWA.Position++;
				In.Position++;
			}
			else { In.Position += 2; }
			XWA.Position += 2;
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

	}
}
