using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.Converters
{
    public abstract class ConverterBase
    {
        protected static short[] BRF = { 0, 0, 0, 0, 1, 1, 2, 2, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 4, 4, 4, 4, 4, 4, 4, 4, 3, 2, 3, 2, 1, 0, 0, 0, 0 };
        public bool ConvertMission(ConversionContext context)
        {
            try
            {
                using (context.ConversionStreams = new ConversionStreams(
                    context.ConversionOptions.FromFileName, 
                    context.ConversionOptions.ToFileName))
                {
                    return InnerTo(context);
                }       
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected abstract bool InnerTo(ConversionContext context);
    }
}
