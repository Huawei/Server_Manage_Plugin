using Microsoft.VisualStudio.TestTools.UnitTesting;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Exceptions.Tests
{
  [TestClass()]
  public class SoftwareSourceExpceionTests
  {
    [TestMethod()]
    [ExpectedException(typeof(SoftwareSourceExpceion))]
    public void SoftwareSourceExpceionTest()
    {
      throw new SoftwareSourceExpceion("-333",null, "test");
    }
  }
}