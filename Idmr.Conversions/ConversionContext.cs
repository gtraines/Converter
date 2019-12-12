using Idmr.Conversions.Converters;
using Idmr.Conversions.GameFormats.XvT;
using System;
using System.Collections.Generic;
using System.IO;
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
        public FileStream SourceStream => ConversionStreams.SourceStream;
        public FileStream TargetStream => ConversionStreams.TargetStream;
        public BinaryWriter TargetWriter => ConversionStreams.TargetWriter;
        public bool IsMultiplayerMission { get; set; }
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
        public int FlightGroupCount { get; set; }

        public int ReadSourceInt32()
        {
            return ConversionStreams.SourceReader.ReadInt32();
        }
        public short ReadSourceInt16()
        {
            return ConversionStreams.SourceReader.ReadInt16();
        }
        public long ReadSourceInt64()
        {
            return ConversionStreams.SourceReader.ReadInt64();
        }
        public byte ReadByte()
        {
            return ConversionStreams.SourceReader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return ConversionStreams.SourceReader.ReadBytes(count);
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

        public void WriteToTargetBuffer(Int32 valueToWrite, long cursorPosition)
        {
            ConversionStreams.TargetStream.Position = cursorPosition;
            WriteToTargetBuffer(valueToWrite);
        }

        public void WriteToTargetBuffer(Int32 valueToWrite)
        {
            ConversionStreams.TargetWriter.Write(valueToWrite);
        }

        public void WriteToTarget(short shortToWrite)
        {
            ConversionStreams.TargetWriter.Write(shortToWrite);
        }

        public void WriteToTargetBuffer(byte[] valueToWrite, long cursorPosition)
        {
            TargetCursor = cursorPosition;
            ConversionStreams.TargetStream.Write(
                valueToWrite, 
                0, 
                valueToWrite.Length);
        }
    }
}
