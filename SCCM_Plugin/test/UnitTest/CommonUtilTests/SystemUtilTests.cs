using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonUtil.Tests
{
    [TestClass()]
    public class SystemUtilTests
    {
        [TestMethod()]
        public void GetLocalhostIPTest()
        {
            string ip= SystemUtil.GetLocalhostIP();
            Console.WriteLine(ip);
        }
    }
}