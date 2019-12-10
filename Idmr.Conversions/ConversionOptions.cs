using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions
{
    public class ConversionOptions
    {
        public ConversionOptions(string fromFileName, 
            GameType fromGame, 
            string toFileName, 
            GameType toGame, 
            bool isMultiplayer = false,
            bool isBalanceOfPower = false)
        {
            FromFileName = fromFileName;
            FromGame = fromGame;
            ToFileName = toFileName;
            ToGame = toGame;
            IsMultiplayer = isMultiplayer;
        }

        public string FromFileName { get; set; }
        public GameType FromGame { get; set; }
        public string ToFileName { get; set; }
        public GameType ToGame { get; set; }
        public bool IsMultiplayer { get; set; }
        public bool IsBalanceOfPower { get; set; }

    }
}
