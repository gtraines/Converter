using System;
using Idmr.Conversions.Converters;

namespace Idmr.Conversions
{
    public class MissionConverter
    {
        private static XWingVsTieConverter _xWingVsTieConverter;

        public static XWingVsTieConverter XWingVsTieConverter
        {
            get
            {
                if (_xWingVsTieConverter == null)
                {
                    _xWingVsTieConverter = new XWingVsTieConverter();
                }
                return _xWingVsTieConverter; 
            }
            set { _xWingVsTieConverter = value; }
        }

        protected static XWingVsTieConverter XvTConverter { get; set; }

        public static void Validate(
            GameType fromGame, 
            string fromFileName, 
            GameType toGame, 
            string toFileName)
        {
            
        }
        public static bool Convert(
            GameType fromGame, 
            string fromFileName, 
            GameType toGame, 
            string toFileName)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                

                throw;
            }

            return true;
        }
    }
}

