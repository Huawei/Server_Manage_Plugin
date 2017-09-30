using CommonUtil;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.PluginUI.Entitys;
using Huawei.SCCMPlugin.RESTeSightLib;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.PluginUI.Helper
{
    /// <summary>
    /// ESSession帮助器
    /// </summary>
    public sealed class ESSessionHelper
    {
        private static void ValidateESSession(IESSession esSession)
        {
            if (esSession == null)
            {
                throw new ESSessionExpceion(ConstMgr.ErrorCode.NET_ESIGHT_NOFOUND, esSession, "没有发现ESSession!");
            }
        }

        private static void ConnectESSession(IESSession esSession)
        {
            if (string.IsNullOrEmpty(esSession.OpenID))
            {
                esSession.Open();
            }
        }

        /// <summary>
        /// 获取eSight列表数据
        /// </summary>
        /// <returns></returns>
        public static List<HWESightHost> GethwESightHostList()
        {
            IList<IESSession> esSessionList = ESightEngine.Instance.ListESSessions();
            List<HWESightHost> hwESightHostList = esSessionList.Select(x => x.HWESightHost).ToList();
            //深拷贝，否则esSession的密码会一起被清掉
            var hwESightHostListString = JsonUtil.SerializeObject(hwESightHostList);
            var nopwdHwESightHostList = JsonUtil.DeserializeObject<List<HWESightHost>>(hwESightHostListString);
            nopwdHwESightHostList.ForEach(x => x.LoginPwd = "");
            nopwdHwESightHostList = nopwdHwESightHostList.OrderBy(x => x.ID).ToList();
            return nopwdHwESightHostList;
        }

        /// <summary>
        /// 根据eSight IP获取ESSession对象
        /// </summary>
        /// <param name="hostIP">eSight IP</param>
        /// <returns></returns>
        public static IESSession GetESSession(string hostIP)
        {
            IESSession esSession = ESightEngine.Instance.FindESSession(hostIP);
            ValidateESSession(esSession);
            ConnectESSession(esSession);
            return esSession;
        }
    }
}
