using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions.Tests
{
    [TestClass()]
    public class DeployExceptionTests
    {
        [TestMethod()]
        [ExpectedException(typeof(DeployException))]
        public void DeployExceptionTest()
        {
            throw new DeployException("-888", null, "test");
        }
    }
}