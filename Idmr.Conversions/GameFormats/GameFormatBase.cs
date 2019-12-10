using System;
using System.Collections.Generic;
using System.IO;

namespace Idmr.Conversions.GameFormats 
{
    public abstract class GameFormatBase
    {
        protected GameFormatBase()
        {
            
        }

        public abstract int InitialByteVal { get; }

        public Dictionary<GameType, object> Conversions { get; set; }

        public void ToGameFormat(GameType game, Stream fromStream, Stream toStream) 
        {
            
        }
    }
}