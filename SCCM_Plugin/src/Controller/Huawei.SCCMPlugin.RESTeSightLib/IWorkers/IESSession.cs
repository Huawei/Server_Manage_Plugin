using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Models.Devices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.IWorkers
{
    /// <summary>
    /// eSSession 连接对象，一个对象代表一个eSight，该类存储了连接信息。
    /// </summary>
    public interface IESSession
    {
        /// <summary>
        /// 初始化eSight信息
        /// </summary>
        /// <param name="hostIP">主机IP</param>
        /// <param name="port">端口</param>
        /// <param name="userAccount">用户账户</param>
        /// <param name="passowrd">密码</param>
        /// <param name="timeoutSec">超时时间默认为:ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC</param>
        void InitESight(string hostIP, int port, string userAccount, string passowrd, int timeoutSec);

        /// 初始化eSight信息
        /// </summary>
        /// <param name="hwESightHost">eSight信息</param>
        /// <param name="timeoutSec">超时时间默认为:ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC</param>
        void InitESight(HWESightHost hwESightHost, int timeoutSec);
        /// <summary>
        /// 设置https模式，true or false
        /// </summary>
        /// <param name="isHttps">true or false</param>
        void SetHttpMode(bool isHttps);
        /// <summary>
        /// 尝试连接eSight
        /// </summary>
        void TryOpen();
        /// <summary>
        /// 尝试连接eSight，如果已经打开过，就使用cache的openid。
        /// </summary>
        void Open();
        /// <summary>
        /// 是否已经连接eSight
        /// </summary>
        /// <returns></returns>
        bool IsConnected();
        /// <summary>
        /// 上次连接时间。
        /// </summary>
        DateTime LatestConnectedTime { get; }
        /// <summary>
        /// 当前eSight连接是否超时
        /// </summary>
        /// <returns>true or false</returns>
        bool IsTimeout();
        /// <summary>
        /// 关闭连接。
        /// </summary>
        void Close();
        /// <summary>
        /// 当前连接的eSight openid.
        /// </summary>
        String OpenID { get; }
        /// <summary>
        /// 根据Response解析result结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="hrm">eSight返回的http result</param>
        /// <returns></returns>
        JObject HCCheckResult(string url, HttpResponseMessage hrm);
        /// <summary>
        /// application/x-www-form-urlencode 提交方式
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="nameValueCollection">key/value值对对象</param>
        /// <returns></returns>
        JObject HCPostForm(string url, IEnumerable<KeyValuePair<string, object>> nameValueCollection);
        /// <summary>
        /// 提交post请求到eSight，并返回json结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="jsonObject">提交的json对象</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        JObject HCPost(string url, object jsonObject);
        /// <summary>
        /// 提交put请求到eSight，并返回json结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="jsonObject">提交的json对象</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        JObject HCPut(string url, object jsonObject, bool isPrint = true);
        /// <summary>
        /// 提交get请求到eSight，并返回json结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isOpenAgain">是否再次打开eSight连接</param>
        /// <returns></returns>
        JObject HCGet(string url, bool isOpenAgain);
        /// <summary>
        /// 提交Get请求到eSight，并返回json结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        JObject HCGet(string url);
        /// <summary>
        /// 提交Delete请求到eSight，并返回json结果。
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        JObject HCDelete(string url);
        /// <summary>
        /// 获得eSightHOST的实体类。
        /// </summary>
        HWESightHost HWESightHost { get; }
        /// <summary>
        /// 保存到数据库。
        /// </summary>
        /// <returns>true or false</returns>
        bool SaveToDB();
        /// <summary>
        /// 查询 iBMC 服务器分页
        /// </summary>
        /// <param name="queryDeviceParam">查询对象</param>
        /// <returns>分页对象</returns>
        QueryPageResult<HWDevice> QueryHWPage(DeviceParam queryDeviceParam);

        /// <summary>
        /// 从数据库，删除当前eSight。
        /// </summary>
        /// <returns>true or false</returns>
        bool DeleteESight();
        /// <summary>
        /// 返回 iBMC Server业务类, 该eSight的业务类对象。只能操作本eSight.
        /// </summary>
        IDeviceWorker DviWorker { get; }
        /// <summary>
        /// 返回 软件源业务对象, 该eSight的业务类对象。只能操作本eSight.
        /// </summary>
        ISoftwareSourceWorker SoftSourceWorker { get; }
        /// <summary>
        /// 返回 模板业务对象, 该eSight的业务类对象。只能操作本eSight.
        /// </summary>
        IHWDeployWorker DeployWorker { get; }
        /// <summary>
        /// 返回 固件升级, 该eSight的业务类对象。只能操作本eSight.
        /// </summary>
        IBasePackageWorker BasePackageWorker { get; }
    }
}
