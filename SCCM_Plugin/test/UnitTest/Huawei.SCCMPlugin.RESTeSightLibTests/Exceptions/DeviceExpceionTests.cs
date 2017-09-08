using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions.Tests
{
  [TestClass()]
  public class DeviceExpceionTests
  {
    [TestMethod()]
    [ExpectedException(typeof(DeviceExpceion))]
    public void DeviceExpceionTest()
    {
      throw new DeviceExpceion("-888",null, "test");

    }    
  }
}