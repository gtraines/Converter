using Idmr.Conversions.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions
{
    public class ConversionContext
    {
        public ConversionContext(ConversionOptions conversionOptions)
        {
            ConversionOptions = conversionOptions;
        }

        public ConversionOptions ConversionOptions { get; set; }
        public ConversionStreams ConversionStreams { get; set; }
        
        public long SourceCursor
        {
            get => ConversionStreams.SourceStream.Position;
            set => ConversionStreams.SourceStream.Position = value;
        }

        public long TargetCursor
        {
            get => ConversionStreams.TargetStream.Position;
            set => ConversionStreams.TargetStream.Position = value;
        }

        public short ReadSourceInt16()
        {
            return ConversionStreams.SourceReader.ReadInt16();
        }
        public byte ReadByte()
        {
            return ConversionStreams.SourceReader.ReadByte();
        }

        public void WriteToTargetBuffer(byte byteToWrite)
        {
            ConversionStreams.TargetStream.WriteByte(byteToWrite);
        }

        public void WriteToTargetBuffer(byte byteToWrite, bool advanceCursor)
        {
            WriteToTargetBuffer(byteToWrite);
            if (advanceCursor)
            {
                ConversionStreams.TargetStream.Position++;
            }
        }
    }
}
