using Huawei.SCCMPlugin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Huawei.SCCMPlugin.DAO
{
    /// <summary>
    /// eSight数据库管理类
    /// </summary>
    public interface IHWESightHostDal:IBaseRepository<HWESightHost>
    {
        /// <summary>
        /// 根据IP查找ESight实体。
        /// </summary>
        /// <param name="eSightIp">IP地址</param>
        /// <returns></returns>
        HWESightHost FindByIP(string eSightIp);
        /// <summary>
        /// 删除eSight
        /// </summary>
        /// <param name="eSightId">eSight Id</param>
        void DeleteESight(int eSightId);
    }
}
