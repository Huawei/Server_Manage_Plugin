using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonUtil.Tests
{
  [TestClass()]
  public class CoreUtilTests
  {
    [TestMethod()]
    public void GetObjTranNullTestInt()
    {
      int testNum = 123;
      string testStr = "123";
        Assert.AreEqual(CoreUtil.GetObjTranNull<int>(testStr), testNum);
    }
    [TestMethod()]
    public void GetObjTranNullTestInt1()
    {
      int testNum = 123;
      string testStr = "123aa";
      Assert.AreNotEqual(CoreUtil.GetObjTranNull<int>(testStr), testNum);
    }
  }
}
