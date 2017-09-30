using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Huawei.SCCMPlugin.Models;
using LogUtil;

namespace Huawei.SCCMPlugin.DAO
{
    /// <summary>
    /// eSight数据库管理类
    /// </summary>
    public class HWESightHostDal :BaseRepository<HWESightHost>, IHWESightHostDal
    {
        /// <summary>
        /// 单例
        /// </summary>
        public static HWESightHostDal Instance
        {
            get { return SingletonProvider<HWESightHostDal>.Instance; }
        }
        /// <summary>
        /// 删除eSight
        /// </summary>
        /// <param name="eSightId">eSight Id</param>
        public void DeleteESight(int eSightId) {
            ExecuteSql("delete from HW_TASK_RESOURCE where HW_ESIGHT_TASK_ID in (select ID from HW_ESIGHT_TASK where HW_ESIGHT_HOST_ID=" + eSightId+")");
            ExecuteSql("delete from HW_ESIGHT_TASK where HW_ESIGHT_HOST_ID="+eSightId);
            ExecuteSql("delete from HWESightHosts where ID=" + eSightId);
        }
    }
}
