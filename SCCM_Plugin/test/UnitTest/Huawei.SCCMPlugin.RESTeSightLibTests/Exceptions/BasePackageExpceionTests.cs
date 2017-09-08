using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions.Tests
{
    [TestClass()]
    public class BasePackageExpceionTests
    {
        [TestMethod()]
        [ExpectedException(typeof(BasePackageExpceion))]
        public void BasePackageExpceionTest()
        {
            throw new BasePackageExpceion("-888", null, "test");
        }
    }
}