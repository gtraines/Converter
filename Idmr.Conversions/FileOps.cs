using System.ComponentModel.DataAnnotations;
using System.IO;
using System;

namespace Idmr.Conversions
{
    public class FileOps
    {
        public static bool ValidateInputFileHead(string inputFileName, GameType toGameType) 
        {
            var inputGameType = GetGameTypeFromFile(inputFileName);

            if (toGameType == inputGameType) 
            {
                throw new ValidationException(
                    $"{inputGameType.ToString()} is already a mission for {toGameType.ToString()}"
                    );
            }

            return true;
        }

        public static GameType GetGameTypeFromFile(string file) 
        {
            if (!File.Exists(file)) 
            {
                throw new ValidationException($"Specified file doesn't exist: {file}");
            }

            using (var inReader = new BinaryReader(File.OpenRead(file)))
            {
                int headByte = inReader.ReadByte();

                switch (headByte)
                {
                    case (int)GameType.TIE: break;
                    case (int)GameType.XvT: break;
                    case (int)GameType.XvTBoP: break;
                    case (int)GameType.XWA: 
                        throw new ValidationException("Cannot create a new XWA mission from an XWA file (it's already done!)");
                        break;
                    default:
                        throw new Exception("Invalid file");
                }

                return (GameType)headByte;
            }
        }
    }
}
