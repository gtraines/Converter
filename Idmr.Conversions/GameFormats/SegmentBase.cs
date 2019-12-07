using System.Security.Authentication.ExtendedProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Idmr.Conversions.GameFormats
{
    public abstract class SegmentBase
    {
        protected SegmentBase()
        {
            
        }

        public int BytesCount { get; set; }
        
    }
}