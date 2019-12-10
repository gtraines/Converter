using System;
using System.IO;
using Idmr.Conversions;
using Idmr.Conversions.Converters;

namespace ConverterCli
{
    class Program
    {
		protected static XWingVsTieConverter XvTConverter { get; set; }
		protected static TieFighterConverter TieFighterConverter { get; set; }
		static int Main(string[] args)
        {
			XvTConverter = new XWingVsTieConverter();
			TieFighterConverter = new TieFighterConverter();

			string fromFileName = args[1];
			string toFileName = args[2];
			string toGameType = args[3];
			var success = false;
			try
			{
				if (!File.Exists(fromFileName)) throw new Exception("Cannot locate original file.");
				var fromGameType = FileOps.GetGameTypeFromFile(fromFileName);

				if (fromGameType == GameType.TIE)
				{
					switch (toGameType)
					{
						case "1":
							success = TieFighterConverter.Convert(fromFileName, toFileName, GameType.XvT);
							break;
						case "2":
							success = TieFighterConverter.Convert(fromFileName, toFileName, GameType.XWA);
							break;
					}
				}
				else if (fromGameType == GameType.XvT || fromGameType == GameType.XvTBoP)
				{
					success = XvTConverter.Convert(fromFileName, toFileName, GameType.XWA);
				}
				else
				{
					throw new Exception(
						"Incorrect parameter usage. Correct usage is as follows:\nOriginal path, new path, mode\nModes: 1 - TIE to XvT, 2 - TIE to XWA, 3 - XvT to XWA");
				}
			}
			catch (Exception x)
			{
				Console.WriteLine($"Error: {x.Message}");
				success = false;
			}

			return success ? 0 : 1;
		}
    }
}
