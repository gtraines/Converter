using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idmr.Conversions.GameFormats.XvT
{
    public class XvTBriefingBlock
    {
        public XvTBriefingBlock(int flightGroupCount, int messageCount, ConversionContext context)
        {
            Brief1BlockStart = GetBlockStartOffset(flightGroupCount, messageCount);
            Brief1StringSize = 0L;
            foreach (var _ in Enumerable.Range(0, 64))
            {
                var len = context.ReadSourceInt16();
                Brief1StringSize += (4 + len);
                context.SourceCursor += len;
            }
            Brief1EndSize = context.SourceCursor;
        }
        public long Brief1BlockStart { get; set; }
        public long Brief1StringSize { get; set; }
        public long Brief1EndSize { get; set; }
        public long Brief1TagsStart => 0x334 + Brief1BlockStart;

        public static long GetBlockStartOffset(int flightGroupCount, 
            int messageCount)
        {
            return 0xA4
                + (0x562 * flightGroupCount)
                + (0x74 * messageCount)
                + 0x500
                + 0x1306;
        }
    }
}
