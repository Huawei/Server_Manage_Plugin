using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions.Tests
{
  [TestClass()]
  public class ESSessionExpceionTests
  {
    [TestMethod()]
    [ExpectedException(typeof(ESSessionExpceion))]
    public void ESSessionExpceionTest()
    {
      throw new ESSessionExpceion("-99999",null, "test");
    }
  }
}