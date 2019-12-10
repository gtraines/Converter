using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Idmr.Conversions.Converters
{
    public class ConversionStreams : IDisposable
    {
        public ConversionStreams(string fromFileName, string toFileName)
        {
            ValidateFileExists(fromFileName);
        }

        protected static bool ValidateFileExists(string fileName)
        {
            
            return true;
        }

        protected void CreateTargetFileWriterAndReader(string toFileName)
        {
            TargetStream = File.Open(toFileName, FileMode.Create, FileAccess.ReadWrite);
            TargetWriter = new BinaryWriter(TargetStream);
            TargetReader = new BinaryReader(TargetStream);
        }

        public void CreateSourceFileReader(string fileName)
        {
            
            SourceStream = File.OpenRead(fileName);
            SourceReader = new BinaryReader(SourceStream);
        }

        public FileStream SourceStream { get; set; }
        public FileStream TargetStream { get; set; }
        public BinaryReader SourceReader { get; set; }
        public BinaryWriter TargetWriter { get; set; }
        public BinaryReader TargetReader { get; set; }
        public void Dispose()
        {
            if (SourceReader != null)
            {
                SourceReader.Close();
                SourceReader.Dispose();
            }
            if (TargetWriter != null)
            {
                TargetWriter.Flush();
                TargetWriter.Close();
                TargetWriter.Dispose();
            }
            
            if (SourceStream != null)
            {
                SourceStream.Close();
                SourceStream.Dispose();
            }
            if (TargetStream != null)
            {
                TargetStream.Close();
                TargetStream.Dispose();
            }
        }
    }
}
