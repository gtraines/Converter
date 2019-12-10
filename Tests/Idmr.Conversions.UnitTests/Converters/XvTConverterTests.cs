using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Idmr.Conversions.Converters;

namespace Idmr.Conversions.UnitTests.Converters
{
    [TestClass]
    public class XvTConverterTests
    {
        
        [TestMethod]
        public void CanConstruct()
        {
            var converter = new XWingVsTieConverter();

            Assert.IsNotNull(converter);
        }
    }
}
