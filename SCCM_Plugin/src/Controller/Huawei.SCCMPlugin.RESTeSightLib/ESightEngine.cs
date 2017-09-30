using CommonUtil;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.DAO;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using Huawei.SCCMPlugin.RESTeSightLib.Workers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib
{
    /// <summary>
    /// eSight管理类。（eSight引擎)
    /// </summary>
    public class ESightEngine
    {
        private static ESightEngine _instance = null;
        private static object _lockInstance = new object();
        /// <summary>
        /// 单例模式，保证在内存中仅有一个管理类的实例。
        /// </summary>
        public static ESightEngine Instance
        {
            get
            {
                if (_instance == null)//是否没有初始化
                {
                    try
                    {
                        System.Threading.Monitor.Enter(_lockInstance);
                        //打开线程锁，再判断一次
                        //两次判断是为了提高单例获取时的效率.
                        //开锁后，相对判断会比较满。
                        if (_instance == null)
                        {
                            _instance = new ESightEngine();
                            _instance.InitESSessions();
                            _instance.RefreshPwdsOneDay();
                        }
                    }
                    finally
                    {
                        //释放
                        System.Threading.Monitor.Exit(_lockInstance);
                    }
                }
                else
                {
                    _instance.RefreshPwdsOneDay();
                }
                return _instance;
            }
        }

        /// <summary>
        /// eSight Session的Dictionary存储向量。
        /// </summary>
        Dictionary<string, IESSession> eSightSessions = new Dictionary<string, IESSession>();
        /// <summary>
        /// 初始化所有的eSight连接的配置信息。
        /// 注意，并没有open。
        /// </summary>
        /// <param name="timeoutSec">连接超时时间，默认为ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC</param>
        public void InitESSessions(int timeoutSec = ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC)
        {
            IHWESightHostDal hwESightHostDal = HWESightHostDal.Instance;
            IList<HWESightHost> hostList = hwESightHostDal.GetList("1=1");//获取eSight
            foreach (HWESightHost hwESightHost in hostList)
            {
                lock (eSightSessions)//开锁
                {
                    if (!eSightSessions.ContainsKey(hwESightHost.HostIP.ToUpper()))//判断是否已经在内存中存在，防止反复初始化。
                    {
                        IESSession iESSession = new ESSession();
                        iESSession.SetHttpMode(_isHttps);
                        iESSession.InitESight(hwESightHost, timeoutSec);
                        eSightSessions[hwESightHost.HostIP.ToUpper()] = iESSession;
                    }
                }
            }
        }
        /// <summary>
        /// 是否使用https协议,请求eSight.
        /// </summary>
        bool _isHttps = true;
                


        /// <summary>
        /// 查询已有的连接列表
        /// </summary>
        /// <returns>返回已有的连接列表</returns>
        public IList<IESSession> ListESSessions()
        {
            return new List<IESSession>(eSightSessions.Values);
        }
        /// <summary>
        /// 查询eSight列表（数据库中的数据，而不是ESSession）
        /// </summary>
        /// <returns>eSight列表</returns>
        public IList<HWESightHost> ListESHost()
        {
            IHWESightHostDal hwESightHostDal = HWESightHostDal.Instance;
            IList<HWESightHost> hostList = hwESightHostDal.GetList("1=1");
            return hostList;
        }
        /// <summary>
        /// 添加一个新的eSight到数据库和Engine中。
        /// </summary>
        /// <param name="hostIP">eSight IP</param>
        /// <param name="port">eSight 端口</param>
        /// <param name="userAccount">对应的账号</param>
        /// <param name="passowrd">对应的密码</param>
        /// <param name="timeoutSec">连接超时时间，默认：ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC</param>
        /// <returns></returns>
        public IESSession SaveNewESSession(string hostIP, int port, string userAccount, string passowrd, int timeoutSec = ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC)
        {
            return SaveNewESSession(hostIP, "", port, userAccount, passowrd, timeoutSec);
        }
        /// <summary>
        /// 修改已有的eSight信息，如果不存在则抛出错误。
        /// </summary>
        /// <param name="hostIP">eSight IP</param>
        /// <param name="aliasName">别名</param>
        /// <param name="port">eSight 端口</param>
        /// <returns></returns>
        public IESSession SaveESSession(string hostIP, string aliasName, int port)
        {
            IESSession iESSession = null;
            iESSession = FindESSession(hostIP);
            if (iESSession != null)
            {
                SaveNewESSession(hostIP, aliasName, port, iESSession.HWESightHost.LoginAccount, iESSession.HWESightHost.LoginPwd);
            }
            else
            {
                throw new BaseException(ConstMgr.ErrorCode.NET_ESIGHT_NOFOUND, this, "Sytem can not find this eSight");
            }
            return iESSession;
        }

        /// <summary>
        /// 返回IESSession
        /// </summary>
        /// <param name="hostIP">eSight IP</param>
        /// <param name="aliasName">别名</param>
        /// <param name="port">eSight 端口</param>
        /// <param name="userAccount">对应的账号</param>
        /// <param name="passowrd">对应的密码</param>
        /// <param name="timeoutSec">连接超时时间，默认：ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC</param>
        /// <returns></returns>
        public IESSession SaveNewESSession(string hostIP, string aliasName, int port, string userAccount, string passowrd, int timeoutSec = ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC)
        {
            //测试连接...
            using (ESSession eSSession = new ESSession())
            {
                eSSession.SetHttpMode(_isHttps);
                eSSession.InitESight(hostIP, port, userAccount, passowrd, timeoutSec);
                eSSession.Open();
            }

            IESSession iESSession = null;

            iESSession = FindESSession(hostIP);//查找已有的eSesssion,防止重复
            if (iESSession == null)//没有找到已有的eSight.
            {
                iESSession = new ESSession();
                iESSession.SetHttpMode(_isHttps);//设置默认协议为全局。
            }
            iESSession.InitESight(hostIP, port, userAccount, passowrd, timeoutSec);//初始化eSight.
            iESSession.HWESightHost.AliasName = aliasName;//初始化eSight别名.
            iESSession.Open();//尝试打开连接，可能会抛出异常。

            iESSession.SaveToDB();//保存到数据库
            lock (eSightSessions)//锁定向量，防止并发
            {
                eSightSessions[iESSession.HWESightHost.HostIP.ToUpper()] = iESSession;//存储到缓存。
            }
            return iESSession;
        }
        /// <summary>
        /// 删除一个已有的eSight.
        /// </summary>
        /// <param name="hostIP">eSight IP</param>
        /// <returns>默认返回成功</returns>
        public bool RemoveESSession(string hostIP)
        {
            IESSession iESSession = FindESSession(hostIP);
            if (iESSession != null)//没有找到已有的eSight.
            {
                eSightSessions.Remove(hostIP.ToUpper());
                iESSession.DeleteESight();
            }
            return true;
        }
        /// <summary>
        /// 删除多个已有的eSight.
        /// </summary>
        /// <param name="hostIP">eSight IP</param>
        /// <returns>默认返回成功</returns>
        public bool RemoveESSession(string[] hostIPs)
        {
            foreach (string hostIP in hostIPs)
            {
                RemoveESSession(hostIP);
            }
            return true;
        }
        /// <summary>
        /// 同步eSight目前正在运行的固件上传任务，并且查询目前所有的固件上传任务。
        /// </summary>
        /// <returns>任务列表</returns>
        public IList<HWESightTask> FindAllBasePackageTaskWithSync()
        {
            List<HWESightTask> taskList = new List<HWESightTask>();
            foreach (IESSession iESSession in eSightSessions.Values)
            {
                //遍历每一个eSight，获取所有的未完成任务。(并且同步任务状态)
                taskList.AddRange(iESSession.BasePackageWorker.FindUnFinishedUploadPackageTask());
            }
            return taskList;
        }
        /// <summary>
        /// 清除所有失败的升级包任务。
        /// </summary>
        /// <returns></returns>
        public int ClearAllFailedPackageTask()
        {
            return HWESightTaskDal.Instance.ClearFailedPackageTask();
        }
        /// <summary>
        /// 同步eSight目前正在运行的软件源上传任务，并且查询目前所有的软件源上传任务。
        /// </summary>
        /// <returns>任务列表</returns>
        public IList<HWESightTask> FindAllSoftwareSourceTaskWithSync()
        {
            List<HWESightTask> taskList = new List<HWESightTask>();
            foreach (IESSession iESSession in eSightSessions.Values)
            {
                taskList.AddRange(iESSession.SoftSourceWorker.FindUnFinishedTask());
            }
            return taskList;
        }
        /// <summary>
        /// 清除所有失败的软件源任务。
        /// </summary>
        /// <returns></returns>
        public int ClearAllFailedSoftwareSourceTask()
        {
            return HWESightTaskDal.Instance.ClearFailedSoftwareSourceTask();
        }
        /// <summary>
        /// 根据ID删除一个eSight，删除内存和数据库。
        /// </summary>
        /// <param name="id">eSight id</param>
        /// <returns>成功失败。</returns>
        public bool RemoveESSession(int id)
        {
            bool isNotFind = true;
            IList<HWESightHost> hostList = ListESHost();
            foreach (HWESightHost host in hostList)
            {
                if (host.ID == id)
                {
                    LogUtil.HWLogger.API.InfoFormat("Begining to delete eSight: [{0}, {1}]", host.HostIP, host.ID);
                    RemoveESSession(host.HostIP);
                    isNotFind = false;
                }
            }
            if (isNotFind)
            {//多删除一次，在数据库中。 没找到时。
                IHWESightHostDal hwESightHostDal = HWESightHostDal.Instance;
                hwESightHostDal.DeleteESight(id);
                LogUtil.HWLogger.API.InfoFormat("deleted eSight: [{0}]", id);
            }
            return true;
        }
        /// <summary>
        /// 根据ID删除多个eSight，删除内存和数据库。
        /// </summary>
        /// <param name="id">eSight id</param>
        /// <returns>成功失败。</returns>
        public bool RemoveESSession(int[] ids)
        {
            foreach (int id in ids)
            {
                RemoveESSession(id);
            }
            return true;
        }
        /// <summary>
        /// 测试eSight是否能够连通。
        /// </summary>
        /// <param name="hostIP">主机IP</param>
        /// <param name="port">端口</param>
        /// <param name="userAccount">用户账户</param>
        /// <param name="passowrd">密码</param>
        /// <param name="timeoutSec">超时时间默认为:ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC</param>
        /// <returns>失败返回错误码，成功返回为空字符</returns>
        public string TestESSession(string hostIP, int port, string userAccount, string passowrd, int timeoutSec = ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC)
        {
            try
            {
                using (ESSession eSSession = new ESSession())
                {
                    eSSession.SetHttpMode(_isHttps);
                    eSSession.InitESight(hostIP, port, userAccount, passowrd, timeoutSec);
                    eSSession.Open();
                }
            }
            catch (ESSessionExpceion ess)
            {
                LogUtil.HWLogger.API.Error(ess);
                if (ess.Code == "1")
                    return ConstMgr.ErrorCode.SYS_USER_LOGING;
                else
                    return ess.Code;
            }
            catch (Exception se)
            {
                LogUtil.HWLogger.API.Error(se);
                return ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
            }
            return "";
        }

        /// <summary>
        /// 查找eSight主机信息
        /// </summary>
        /// <param name="hostIP">主机IP</param>
        /// <returns>返回查找到的eSight，没有找到返回为null</returns>
        public IESSession FindESSession(string hostIP)
        {
            if (eSightSessions.ContainsKey(hostIP.ToUpper()))
            {
                return eSightSessions[hostIP.ToUpper()];
            }
            return null;
        }
        /// <summary>
        /// 查找eSight主机信息，并且打开.
        /// </summary>
        /// <param name="hostIP">主机IP</param>
        /// <returns>返回查找到的eSight，没有找到返回为null</returns>
        public IESSession FindAndOpenESSession(string hostIP)
        {
            try
            {
                IESSession iESSession = FindESSession(hostIP);
                if (iESSession != null)
                {
                    iESSession.Open();
                    //iESSession.SaveToDB();//保存状态。
                }
                return iESSession;
            }
            catch (Exception se)
            {
                LogUtil.HWLogger.API.Error(se);
                throw;
            }
        }
        /// <summary>
        /// 查找eSight主机信息，并且打开.
        /// </summary>
        /// <param name="hostIP">主机IP</param>
        /// <returns>返回查找到的eSight，没有找到返回为null</returns>
        public IESSession this[string hostIP]
        {
            get
            {
                return FindAndOpenESSession(hostIP);
            }
        }

        private static object _lockRefreshPwds = new object();

        private DateTime _lastRefreshPwdTime = DateTime.MinValue;
        /// <summary>
        /// 隔天更新密钥。
        /// 1. 当启动时更新。
        /// 2. 启动后，距离上次更新超过一天时更新。
        /// </summary>
        public void RefreshPwdsOneDay()
        {
            DateTime now = DateTime.Now;
            TimeSpan d = now.Subtract(_lastRefreshPwdTime);
            if (d.Days > 1)
            {
                InitESSessions();
                _lastRefreshPwdTime = now;
                RefreshPwds();
            }
        }
        /// <summary>
        /// 刷新密码。时机是每次启动时，这里会加在这个单例的初始化。
        /// 重置密钥，并且更新密码。
        /// 规则1：密钥须支持可更新，并明确更新周期，在一次性可编程的芯片中保存的密钥除外
        /// 说明：工作密钥及密钥加密密钥在使用过程中，都应保证其可以更新。对于根密钥暂不要求必须支持可更新。
        /// </summary>
        public void RefreshPwds()
        {           
            LogUtil.HWLogger.API.InfoFormat("Refresh password with encryption...");
            lock (_lockRefreshPwds)
            {
                //旧的key
                string rootKey = EncryptUtil.GetRootKey();
                string oldMainKey = EncryptUtil.GetMainKey();
                //重新初始化主密钥。
                EncryptUtil.InitMainKey();
                string newMainKey = EncryptUtil.GetMainKey();

                //遍历所有session.
                IList<HWESightHost> hostlist = ESightEngine.Instance.ListESHost();
                foreach (HWESightHost eSightHost in hostlist)
                {
                    string pwd = EncryptUtil.DecryptWithKey(oldMainKey, eSightHost.LoginPwd);
                    string enPwd = EncryptUtil.EncryptWithKey(newMainKey, pwd);

                    IESSession iESSession = FindESSession(eSightHost.HostIP);
                    iESSession.HWESightHost.LoginPwd = enPwd;

                    eSightSessions[eSightHost.HostIP.ToUpper()] = iESSession;
                    iESSession.SaveToDB();
                }
            }
            LogUtil.HWLogger.API.InfoFormat("Refresh password with encryption successful!");
        }
    }
}
