using System;

namespace Idmr.Conversions.GameFormats 
{
    public abstract class GameFormatBase
    {
        protected GameFormatBase()
        {
            
        }

        public abstract int InitialByteVal { get; }

        public Dictionary<Game, object> Conversions { get; set; }

        public ToGameFormat(Games game, ByteStream fromStream, ByteStream toStream) 
        {
            
        }





    }
}