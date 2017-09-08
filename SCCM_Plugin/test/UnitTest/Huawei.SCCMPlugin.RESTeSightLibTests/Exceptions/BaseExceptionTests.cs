using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions.Tests
{
    [TestClass()]
    public class BaseExceptionTests
    {
        [TestMethod()]
        [ExpectedException(typeof(BaseException))]
        public void BaseExceptionTest()
        {
            throw new BaseException("-888", null, "test");
        }
        [TestMethod()]
        public void TestMember() {
            BaseException baseExcepiton= new BaseException("-888", null, "test");
            Console.WriteLine(baseExcepiton.Code);
            Console.WriteLine(baseExcepiton.Message);
        }
    }
}