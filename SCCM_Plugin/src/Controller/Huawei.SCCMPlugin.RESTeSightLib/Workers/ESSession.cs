using Huawei.SCCMPlugin.RESTeSightLib.IWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Models;
using Huawei.SCCMPlugin.Const;
using Huawei.SCCMPlugin.DAO;
using Huawei.SCCMPlugin.RESTeSightLib.Exceptions;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CommonUtil;
using System.Net;
using Huawei.SCCMPlugin.Models.Devices;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Huawei.SCCMPlugin.RESTeSightLib.Workers
{
    /// <summary>
    /// eSSession 连接对象，一个对象代表一个eSight，该类存储了连接信息。
    /// </summary>
    public class ESSession : IESSession, System.IDisposable
    {
        HWESightHost _hwESightHost = null;
        DateTime _latestConnectedTime = DateTime.MinValue;
        int _timeoutSec = ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC;
        string _openID = "";
        bool _disposed = false;
        bool _isOpen = false;

        string _httpMode = "https";

        HttpClient _httpClient;
        IDeviceWorker _deviceWorker;
        ISoftwareSourceWorker _softSourceWorker;
        IHWDeployWorker _hwDeployWorker;
        IBasePackageWorker _basePackageWorker;
        /// <summary>
        /// 初始化eSight信息
        /// </summary>
        /// <param name="hostIP">主机IP</param>
        /// <param name="port">端口</param>
        /// <param name="userAccount">用户账户</param>
        /// <param name="passowrd">密码</param>
        /// <param name="certpath">证书路径</param>
        /// <param name="timeoutSec">超时时间默认为:ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC</param>
        private void InitESight(string hostIP, int port, string userAccount, string passowrd, string certpath, int timeoutSec)
        {
            if (_hwESightHost == null)
                _hwESightHost = new HWESightHost();
            _hwESightHost.HostIP = hostIP;
            _hwESightHost.LoginAccount = userAccount;
            _hwESightHost.LoginPwd = passowrd;
            _hwESightHost.HostPort = port;
            _hwESightHost.CertPath = certpath;

            InitESight(_hwESightHost, timeoutSec);
        }
        /// <summary>
        /// 初始化eSight信息
        /// </summary>
        /// <param name="hostIP">主机IP</param>
        /// <param name="port">端口</param>
        /// <param name="userAccount">用户账户</param>
        /// <param name="passowrd">密码</param>
        /// <param name="timeoutSec">超时时间默认为:ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC</param>
        public void InitESight(string hostIP, int port, string userAccount, string passowrd, int timeoutSec)
        {
            InitESight(hostIP, port, userAccount, passowrd, "", timeoutSec);
        }
        /// <summary>
        /// 初始化eSight信息
        /// </summary>
        /// <param name="hostIP">主机IP</param>
        /// <param name="port">端口</param>
        /// <param name="userAccount">用户账户</param>
        /// <param name="passowrd">密码</param>
        /// <param name="timeoutSec">超时时间默认为:ConstMgr.HWESightHost.DEFAULT_TIMEOUT_SEC</param>
        public void InitESight(HWESightHost hwESightHost, int timeoutSec)
        {
            _hwESightHost = hwESightHost;
            this._timeoutSec = timeoutSec;
            _hwESightHost.LatestStatus = ConstMgr.HWESightHost.LATEST_STATUS_NONE;
        }
        /// <summary>
        /// 设置https模式，true or false
        /// </summary>
        /// <param name="isHttps">true or false</param>
        public void SetHttpMode(bool isHttps)
        {
            if (isHttps)
                _httpMode = "https";
            //else
            //    _httpMode = "http";
        }
        /// <summary>
        /// 是否已经连接eSight
        /// </summary>
        /// <returns>true or false</returns>
        public bool IsConnected()
        {
            if (IsTimeout()) return false;
            if (String.Equals(_hwESightHost.LatestStatus, ConstMgr.HWESightHost.LATEST_STATUS_ONLINE) && _isOpen) return true;
            return false;
        }
        /// <summary>
        /// 当前eSight连接是否超时
        /// </summary>
        /// <returns>true or false</returns>
        public bool IsTimeout()
        {
            TimeSpan tSpan = System.DateTime.Now - LatestConnectedTime;
            //加上30秒的偏差。
            if (tSpan.TotalSeconds + 30 > _timeoutSec)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获得eSightHOST的实体类。
        /// </summary>
        public HWESightHost HWESightHost
        {
            get
            {
                return _hwESightHost;
            }
        }
        /// <summary>
        /// 上次连接时间。
        /// </summary>
        public DateTime LatestConnectedTime
        {
            get
            {
                return _latestConnectedTime;
            }
        }
        /// <summary>
        /// 当前连接的eSight openid.
        /// </summary>
        public string OpenID
        {
            get
            {
                return _openID;
            }
        }
        /// <summary>
        /// 检测是否初始化了eSight连接信息。没有则抛出错误。
        /// </summary>
        private void CheckInit()
        {
            if (HWESightHost == null) throw new ESSessionExpceion(ConstMgr.ErrorCode.SYS_UNINIT, this, "Please initialize first");
        }
        /// <summary>
        /// 解析复合Exception里的innerException,返回innerException
        /// </summary>
        /// <param name="ae">复合Exception</param>
        /// <returns>inner Exception 列表</returns>
        private List<Exception> GetFlattenAggregateException(AggregateException ae)
        {
            // Initialize a collection to contain the flattened exceptions.
            List<Exception> flattenedExceptions = new List<Exception>();

            // Create a list to remember all aggregates to be flattened, this will be accessed like a FIFO queue
            List<AggregateException> exceptionsToFlatten = new List<AggregateException>();
            exceptionsToFlatten.Add(ae);
            int nDequeueIndex = 0;

            // Continue removing and recursively flattening exceptions, until there are no more.
            while (exceptionsToFlatten.Count > nDequeueIndex)
            {
                // dequeue one from exceptionsToFlatten
                IList<Exception> currentInnerExceptions = exceptionsToFlatten[nDequeueIndex++].InnerExceptions;

                for (int i = 0; i < currentInnerExceptions.Count; i++)
                {
                    Exception currentInnerException = currentInnerExceptions[i];

                    if (currentInnerException == null)
                    {
                        continue;
                    }

                    AggregateException currentInnerAsAggregate = currentInnerException as AggregateException;

                    // If this exception is an aggregate, keep it around for later.  Otherwise,
                    // simply add it to the list of flattened exceptions to be returned.
                    if (currentInnerAsAggregate != null)
                    {
                        exceptionsToFlatten.Add(currentInnerAsAggregate);
                    }
                    else
                    {
                        flattenedExceptions.Add(currentInnerException);
                        flattenedExceptions.AddRange(GetFlattenException(currentInnerException));

                    }
                }
            }
            return flattenedExceptions;
        }
        /// <summary>
        /// 解析Exception里的innerException,返回innerException
        /// </summary>
        /// <param name="se">Exception</param>
        /// <returns>inner Exception 列表</returns>
        private List<Exception> GetFlattenException(Exception se)
        {
            List<Exception> exs = new List<Exception>();

            Exception currentInnerException = se.InnerException;
            if (currentInnerException == null)
                return new List<Exception>();
            else
            {
                exs.Add(currentInnerException);
                exs.AddRange(GetFlattenException(currentInnerException));
            }
            return exs;
        }
        /// <summary>
        /// 解析复合Exception里的innerException,返回解析过的内部Exception，方便前台判断。
        /// </summary>
        /// <param name="ae">复合Exception</param>
        /// <returns> 解析过的内部Exception，方便前台判断。</returns>
        public Exception HandleException(AggregateException ae)
        {
            StringBuilder sb = new StringBuilder();
            LogUtil.HWLogger.API.Error(ae);
            List<Exception> flattenedExceptions = GetFlattenAggregateException(ae);
            foreach (var ex in flattenedExceptions)
            {
                if (ex is WebException)
                {
                    WebException we = (WebException)ex;
                    LogUtil.HWLogger.API.Error(we.Response);
                    if (we.Response != null)
                    {
                        HttpWebResponse response = (HttpWebResponse)we.Response;
                        StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8);
                        string backstr = sr.ReadToEnd();
                        sr.Close();
                        response.Close();
                        LogUtil.HWLogger.API.Error(backstr);
                    }
                }
                sb.AppendLine(ex.Message);
            }
            LogUtil.HWLogger.API.Error(sb.ToString());
            int errCnt = flattenedExceptions.Count;
            for (int i = errCnt - 1; i >= 0; i--)
            {
                var ex = flattenedExceptions[i];
                if (ex is SocketException)//是否socket连接错误
                {
                    var sex = ex as SocketException;
                    if (sex.NativeErrorCode == 10061)//是否socket拒绝连接
                    {
                        throw new ESSessionExpceion(ConstMgr.ErrorCode.NET_SOCKET_REFUSED, this, ex.Message);
                    }
                    else
                    {
                        throw new ESSessionExpceion(ConstMgr.ErrorCode.NET_SOCKET_UNKNOWN, this, ex.Message);
                    }
                }
                else if (ex is WebException)
                {
                    throw new ESSessionExpceion(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, this, ex.Message);
                }
            }
            throw new ESSessionExpceion(ConstMgr.ErrorCode.SYS_UNKNOWN_ERR, this, sb.ToString());
        }
        /// <summary>
        /// 根据Response解析result结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="hrm">eSight返回的http result</param>
        /// <returns>解析过的结果，JObject</returns>
        public JObject HCCheckResult(string url, HttpResponseMessage hrm)
        {
            if (hrm.IsSuccessStatusCode)
            {
                string retVal = hrm.Content.ReadAsStringAsync().Result;
                LogUtil.HWLogger.API.Info("Huawei return:" + retVal);
                JObject data = JsonUtil.DeserializeObject<JObject>(hrm.Content.ReadAsStringAsync().Result);
                return data;
            }
            else
            {
                string webErrorCode = ConstMgr.ErrorCode.SYS_UNKNOWN_ERR;
                if (hrm != null)
                {
                    int statusCode = CoreUtil.GetObjTranNull<int>(hrm.StatusCode);
                    if (statusCode >= 400 && statusCode <= 600)
                    {
                        webErrorCode = "-50" + statusCode;
                    }
                }


                LogUtil.HWLogger.API.ErrorFormat("Accessing[{0}] ,StatusCode:[{1}],ReasonPhrase:[{2}], Error occurred: [{3}]",
                url, hrm.StatusCode, hrm.ReasonPhrase, hrm.Content.ReadAsStringAsync().Result);

                throw new ESSessionExpceion(webErrorCode, this,
                    String.Format("Accessing[{0}] ,StatusCode:[{1}],ReasonPhrase:[{2}], Error occurred: [{3}]",
                    url, hrm.StatusCode, hrm.ReasonPhrase, hrm.Content.ReadAsStringAsync().Result));
            }
        }
        /// <summary>
        /// 过滤提交的正文内的密码信息。打印使用。
        /// </summary>
        /// <param name="context">提交的正文</param>
        /// <returns>打印的正文信息</returns>
        private String GetPrintInfoOfJson(Object context)
        {
            string jsonContext = JsonUtil.SerializeObject(context);
            string replacement = "\"${str}\":\"********\"";
            string pattern1 = "\"(?<str>([A-Za-z0-9_]*)password)\":\"(.*?)\"";
            jsonContext = Regex.Replace(jsonContext, pattern1, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
            string pattern2 = "\"(?<str>([A-Za-z0-9_]*)pwd)\":\"(.*?)\"";
            jsonContext = Regex.Replace(jsonContext, pattern2, replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
            return jsonContext;
        }
        /// <summary>
        /// application/x-www-form-urlencode 提交方式
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="nameValueCollection">key/value值对对象</param>
        /// <param name="isOpenAgain">重新打开连接</param>
        /// <returns>eSight返回结果，JObject</returns>
        private JObject HCPostForm(string url, IEnumerable<KeyValuePair<string, object>> nameValueCollection, bool isOpenAgain)
        {
            if (isOpenAgain) Open();
            string abUrl = GetFullURL(url);
            InitHCHead();
            JObject retObj = null;
            try
            {
                for (int i = 0; i <= 1; i++)
                {
                    var content = new FormUrlEncodedContentEx(nameValueCollection);//, Encoding.UTF8, "application/x-www-form-urlencoded");               
                    LogUtil.HWLogger.API.DebugFormat("Send json by PostForm[{0}]:{1}", abUrl, GetPrintInfoOfJson(nameValueCollection));
                    HttpResponseMessage hrm = HClient.PostAsync(abUrl, content).Result;
                    retObj = HCCheckResult(abUrl, hrm);
                    if (!string.Equals(GetJObjectPropVal<string>(retObj, "code"), Const.ConstMgr.ErrorCode.HW_LOGIN_AUTH))
                    {
                        break;
                    }
                    else
                    {
                        if (isOpenAgain)
                        {
                            LogUtil.HWLogger.API.WarnFormat("Login agin,Retry..");
                            TryOpen();
                        }
                        else
                            break;
                    }
                }
            }
            catch (System.AggregateException ae)
            {
                HandleException(ae);
            }
            catch (Exception se)
            {
                LogUtil.HWLogger.API.Error(se);
                throw;
            }
            return retObj;
        }
        /// <summary>
        /// application/x-www-form-urlencode 提交方式,默认重新打开eSight连接。
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="nameValueCollection">key/value值对对象</param>        
        /// <returns>eSight返回结果，JObject</returns>
        public JObject HCPostForm(string url, IEnumerable<KeyValuePair<string, object>> nameValueCollection)
        {
            return HCPostForm(url, nameValueCollection, true);
        }
        /// <summary>
        /// 提交post请求，并返回json结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="jsonObject">提交的json对象</param>
        /// <param name="isOpenAgain">重新打开连接</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        private JObject HCPost(string url, object jsonObject, bool isOpenAgain)
        {
            if (isOpenAgain) Open();
            string abUrl = GetFullURL(url);
            InitHCHead();
            JObject retObj = null;
            try
            {
                for (int i = 0; i <= 1; i++)
                {
                    var content = new StringContent(JsonUtil.SerializeObject(jsonObject), Encoding.UTF8, "application/json");
                    LogUtil.HWLogger.API.DebugFormat("Send json by post[{0}]:{1}", abUrl, GetPrintInfoOfJson(jsonObject));
                    HttpResponseMessage hrm = HClient.PostAsync(abUrl, content).Result;
                    retObj = HCCheckResult(abUrl, hrm);
                    if (!string.Equals(GetJObjectPropVal<string>(retObj, "code"), Const.ConstMgr.ErrorCode.HW_LOGIN_AUTH))
                    {
                        break;
                    }
                    else
                    {
                        if (isOpenAgain)
                        {
                            LogUtil.HWLogger.API.WarnFormat("Login agin,Retry..");
                            TryOpen();
                        }
                        else
                            break;
                    }
                }

            }
            catch (System.AggregateException ae)
            {
                HandleException(ae);
            }
            catch (Exception se)
            {
                LogUtil.HWLogger.API.Error(se);
                throw;
            }
            return retObj;
        }
        /// <summary>
        /// 提交post请求，并返回json结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="jsonObject">提交的json对象</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        public JObject HCPost(string url, object jsonObject)
        {
            return HCPost(url, jsonObject, true);
        }
        /// <summary>
        /// 提交put请求，并返回json结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="jsonObject">提交的json对象</param>
        /// <param name="isOpenAgain">重新打开连接</param>
        /// <param name="isPrint">是否打印</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        public JObject HCPut(string url, object jsonObject, bool isOpenAgain, bool isPrint)
        {
            if (isOpenAgain) Open();
            string abUrl = GetFullURL(url);//abUrl=eSight_Url+业务url
            InitHCHead();//初始化openId.
            JObject retObj = null;
            try
            {
                for (int i = 0; i <= 1; i++)
                {
                    var content = new StringContent(JsonUtil.SerializeObject(jsonObject), Encoding.UTF8, "application/json");
                    LogUtil.HWLogger.API.DebugFormat("Send json by put[{0}]:{1}", abUrl, isPrint ? GetPrintInfoOfJson(jsonObject) : "******");
                    HttpResponseMessage hrm = HClient.PutAsync(abUrl, content).Result;
                    retObj = HCCheckResult(abUrl, hrm);
                    if (!string.Equals(GetJObjectPropVal<string>(retObj, "code"), Const.ConstMgr.ErrorCode.HW_LOGIN_AUTH))
                    {
                        break;
                    }
                    else
                    {
                        if (isOpenAgain)
                        {
                            LogUtil.HWLogger.API.WarnFormat("Login agin,Retry..");
                            TryOpen();
                        }
                        else
                            break;
                    }
                }
            }
            catch (System.AggregateException ae)
            {
                HandleException(ae);
            }
            return retObj;
        }
        /// <summary>
        /// 提交put请求，并返回json结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="jsonObject">提交的json对象</param>
        /// <param name="isPrint">是否打印,默认为true</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        public JObject HCPut(string url, object jsonObject, bool isPrint = true)
        {
            return HCPut(url, jsonObject, true, isPrint);
        }
        /// <summary>
        /// 提交get请求，并返回json结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="isOpenAgain">重新打开连接，获取eSight openid</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        public JObject HCGet(string url, bool isOpenAgain)
        {
            if (isOpenAgain) Open();
            string abUrl = GetFullURL(url);
            InitHCHead();
            JObject retObj = null;
            try
            {
                for (int i = 0; i <= 1; i++)
                {
                    LogUtil.HWLogger.API.DebugFormat("Send json by get:{0}", url);
                    HttpResponseMessage hrm = HClient.GetAsync(abUrl).Result;
                    retObj = HCCheckResult(abUrl, hrm);
                    if (!string.Equals(GetJObjectPropVal<string>(retObj, "code"), Const.ConstMgr.ErrorCode.HW_LOGIN_AUTH))
                    {
                        break;
                    }
                    else
                    {
                        if (isOpenAgain)
                        {
                            LogUtil.HWLogger.API.WarnFormat("Login agin,Retry..");
                            TryOpen();
                        }
                        else
                            break;
                    }
                }
            }
            catch (System.AggregateException ae)
            {
                HandleException(ae);
            }
            catch (Exception ae)
            {
                LogUtil.HWLogger.API.Error(ae);
                throw;
            }
            return retObj;
        }
        /// <summary>
        /// 提交get请求，并返回json结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        public JObject HCGet(string url)
        {
            return HCGet(url, true);
        }
        /// <summary>
        /// 提交delete请求，并返回json结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <param name="isOpenAgain">重新打开连接，获取eSight openid</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        private JObject HCDelete(string url, bool isOpenAgain)
        {
            if (isOpenAgain) Open();
            string abUrl = GetFullURL(url);
            InitHCHead();
            JObject retObj = null;
            try
            {
                for (int i = 0; i <= 1; i++)
                {
                    LogUtil.HWLogger.API.DebugFormat("Send json by delete:{0}", abUrl);
                    HttpResponseMessage hrm = HClient.DeleteAsync(abUrl).Result;
                    retObj = HCCheckResult(abUrl, hrm);
                    if (!string.Equals(GetJObjectPropVal<string>(retObj, "code"), Const.ConstMgr.ErrorCode.HW_LOGIN_AUTH))
                    {
                        break;
                    }
                    else
                    {
                        if (isOpenAgain)
                        {
                            LogUtil.HWLogger.API.WarnFormat("Login agin,Retry..");
                            TryOpen();
                        }
                        else
                            break;
                    }
                }
            }
            catch (System.AggregateException ae)
            {
                HandleException(ae);
            }
            return retObj;
        }
        /// <summary>
        /// 提交delete请求，并返回json结果
        /// </summary>
        /// <param name="url">提交eSight的url</param>
        /// <returns>返回JObject的eSight返回结果</returns>
        public JObject HCDelete(string url)
        {
            return HCDelete(url, true);
        }
        /// <summary>
        /// 返回完整的连接 eSight url+业务url
        /// </summary>
        /// <param name="url">业务url</param>
        /// <returns>eSight url+业务url</returns>
        private string GetFullURL(string url)
        {
            return GetBaseURL() + url;
        }
        /// <summary>
        /// 获取eSight URL. (https://192.168.1.1:32120)
        /// </summary>
        /// <returns>eg: https://192.168.1.1:32120</returns>
        private string GetBaseURL()
        {
            return String.Format(ConstMgr.HWESightHost.BASE_URI_FROMATE, _httpMode, _hwESightHost.HostIP, _hwESightHost.HostPort);
        }
        /// <summary>
        /// 初始化HttpClient。
        /// </summary>
        private void InitHC()
        {
            HClient = new HttpClient() { BaseAddress = new Uri(GetBaseURL()) };
            TrustCertificate();//忽略证书问题。
        }
        /// <summary>
        /// 定义需要使用的系统枚举。
        /// </summary>
        [Flags]
        private enum MySecurityProtocolType
        {
            //
            // Summary:
            //     Specifies the Secure Socket Layer (SSL) 3.0 security protocol.
            Ssl3 = 48,
            //
            // Summary:
            //     Specifies the Transport Layer Security (TLS) 1.0 security protocol.
            Tls = 192,
            //
            // Summary:
            //     Specifies the Transport Layer Security (TLS) 1.1 security protocol.
            Tls11 = 768,
            //
            // Summary:
            //     Specifies the Transport Layer Security (TLS) 1.2 security protocol.
            Tls12 = 3072
        }
        /// <summary>
        /// 默认信任证书，忽略证书警告。
        /// </summary>
        private void TrustCertificate()
        {
            //默认忽略证书
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            //兼容所有ssl协议
            System.Net.ServicePointManager.SecurityProtocol = (SecurityProtocolType)(MySecurityProtocolType.Tls12 | MySecurityProtocolType.Tls11 | MySecurityProtocolType.Tls | MySecurityProtocolType.Ssl3);
        }
        /// <summary>
        /// 初始化HttpClient 头信息，主要是openid.
        /// </summary>
        private void InitHCHead()
        {
            if (!string.IsNullOrEmpty(_openID))
            {
                HClient.DefaultRequestHeaders.Remove("openid");//防止重复添加
                HClient.DefaultRequestHeaders.Add("openid", _openID);
                LogUtil.HWLogger.API.InfoFormat("openid:{0}", _openID);
            }
            //再次设置信任证书。
            TrustCertificate();
        }
        /// <summary>
        /// 获取需要提交的登陆信息。
        /// </summary>
        /// <returns></returns>
        private JObject GetLoginParam()
        {
            JObject paramJson = new JObject();
            paramJson.Add("userid", _hwESightHost.LoginAccount);//用户名
            paramJson.Add("value", EncryptUtil.DecryptPwd(_hwESightHost.LoginPwd));//密码
            string localIp = SystemUtil.GetLocalhostIP();
            if(!string.IsNullOrEmpty(localIp)) paramJson.Add("ipaddr", localIp);//本机ip。
            return paramJson;
        }
        /// <summary>
        /// 获取需要提交的退出登录地址
        /// </summary>
        /// <returns>退出登录地址</returns>
        private string GetLoginOutURL()
        {
            return ConstMgr.HWESightHost.URL_LOGIN + "?openid=" + _openID;
        }
        /// <summary>
        /// 获得一个JObject的属性值，当该属性为不存在时，返回一个default 值的 期待对象。
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="result">需要解析的Json对象</param>
        /// <param name="name">属性名</param>
        /// <returns></returns>
        private static T GetJObjectPropVal<T>(JObject result, string name)
        {
            return CoreUtil.GetObjTranNull<T>(result.Property(name).Value);
        }
        /// <summary>
        /// 检查当前连接请求是否成功。
        /// </summary>
        /// <param name="result">连接以后返回的json对象</param>
        public void ChkOpenResult(JObject result)
        {
            if (GetJObjectPropVal<string>(result, "code") == "0")//为0代表连接成功
            {
                LogUtil.HWLogger.API.InfoFormat("[{0}]Login successful, code= {1},openID={2} ", GetBaseURL(), GetJObjectPropVal<string>(result, "code"), GetJObjectPropVal<string>(result, "data"));
                //2. 连接成功，更新状态, 更新token.    
                this._openID = GetJObjectPropVal<string>(result, "data");//读取openId.
                this._hwESightHost.LastModifyTime = System.DateTime.Now;
                this._hwESightHost.LatestStatus = ConstMgr.HWESightHost.LATEST_STATUS_ONLINE;
                _isOpen = true;
                SyncToDB();
            }
            else
            {
                //3.连接失败，更新状态，返回错误。
                LogUtil.HWLogger.API.InfoFormat("[{0}]Login failed, code= {1},openID={2},description={3} ", GetBaseURL(), GetJObjectPropVal<string>(result, "code"), GetJObjectPropVal<string>(result, "data"), GetJObjectPropVal<string>(result, "description"));
                this._hwESightHost.LastModifyTime = System.DateTime.Now;
                this._hwESightHost.LatestStatus = ConstMgr.HWESightHost.LATEST_STATUS_FAILED;
                SyncToDB();
                throw new ESSessionExpceion("-33"+GetJObjectPropVal<string>(result, "code"), this, GetJObjectPropVal<string>(result, "description"));
            }
        }
        /// <summary>
        /// 尝试连接eSight
        /// </summary>
        public void TryOpen()
        {
            CheckInit();//初始化检查
            //1. HttpClient连接eSight. 
            InitHC();//初始化头和eSight请求地址。

            LogUtil.HWLogger.API.InfoFormat("[{0}]Logging in...", GetBaseURL());
            JObject result = HCPut(ConstMgr.HWESightHost.URL_LOGIN, GetLoginParam(), false, false);
            _latestConnectedTime = System.DateTime.Now;
            LogUtil.HWLogger.API.InfoFormat("Login successful!!");
            ChkOpenResult(result);
        }
        /// <summary>
        /// 尝试连接eSight，如果已经打开过，就使用cache的openid。
        /// </summary>
        public void Open()
        {
            if (!IsConnected()) TryOpen();
            //因为用户状态会有锁定等问题，所以每次都打开。
            //TryOpen();
        }
        /// <summary>
        /// 关闭连接。
        /// </summary>
        public void Close()
        {

            try
            {
                if (IsConnected())//是否连接状态。
                {
                    string loginOutURL = GetLoginOutURL();
                    LogUtil.HWLogger.API.InfoFormat("[{0}]Logging out...", loginOutURL, false);
                    JObject result = HCDelete(loginOutURL);
                    LogUtil.HWLogger.API.InfoFormat("Logout successful!!!");
                    if (GetJObjectPropVal<string>(result, "code") == "0")
                    {
                        LogUtil.HWLogger.API.InfoFormat("[{0}]Logout successful, code= {1},openID={2} ", GetBaseURL(), GetJObjectPropVal<string>(result, "code"), GetJObjectPropVal<string>(result, "data"));
                        SyncToDB();
                    }
                    else
                    {
                        //3.连接失败，更新状态，返回错误。
                        LogUtil.HWLogger.API.InfoFormat("[{0}]Logging out failed, code= {1},openID={2},description={3} ", GetBaseURL(), GetJObjectPropVal<string>(result, "code"), GetJObjectPropVal<string>(result, "data"), GetJObjectPropVal<string>(result, "description"));
                    }
                }
            }
            catch (Exception se)
            {
                LogUtil.HWLogger.API.Error(se);
                throw;
            }
            finally
            {
                //Clear.
                this._hwESightHost.LastModifyTime = System.DateTime.Now;
                this._hwESightHost.LatestStatus = ConstMgr.HWESightHost.LATEST_STATUS_DISCONNECT;
                _isOpen = false;
                _openID = "";
                SyncToDB();
            }
        }
        /// <summary>
        /// 检测是否 读取 iBMC 服务器错误。
        /// </summary>
        /// <param name="jsonData">返回JObject</param>
        /// <param name="errorTip">错误提示头</param>
        private void CheckAndThrowDviException(JObject jsonData, string errorTip = "")
        {
            int retCode = 0;
            retCode = JsonUtil.GetJObjectPropVal<int>(jsonData, "code");
            if (retCode != 0)
            {
                throw new DeviceExpceion(retCode.ToString(), null, errorTip + JsonUtil.GetJObjectPropVal<string>(jsonData, "description"));
            }
        }
        /// <summary>
        /// 查询 iBMC 服务器分页。
        /// </summary>
        /// <param name="queryDeviceParam">查询参数</param>
        /// <returns>分页对象</returns>
        public QueryPageResult<HWDevice> QueryHWPage(DeviceParam queryDeviceParam)
        {
            StringBuilder sb = new StringBuilder(ConstMgr.HWESightHost.URL_DEVICEPAGE);
            sb.Append("?servertype=").Append(queryDeviceParam.ServerType);
            if (queryDeviceParam.StartPage > 0) sb.Append("&start=").Append(queryDeviceParam.StartPage);
            if (queryDeviceParam.PageSize > 0) sb.Append("&size=").Append(queryDeviceParam.PageSize);
            if (string.IsNullOrEmpty(queryDeviceParam.PageOrder)) sb.Append("&orderby=").Append(queryDeviceParam.PageOrder);
            if (string.IsNullOrEmpty(queryDeviceParam.PageOrder)) sb.Append("&desc=").Append(queryDeviceParam.OrderDesc);
            JObject jResult = HCGet(sb.ToString());
            CheckAndThrowDviException(jResult);
            QueryPageResult<HWDevice> pageResult = jResult.ToObject<QueryPageResult<HWDevice>>();
            if (pageResult.Code != 0)
            {
                LogUtil.HWLogger.API.ErrorFormat("openid:" + this.OpenID);
                throw new ESSessionExpceion(pageResult.Code.ToString(), this, "Query device list error:" + pageResult.Description);
            }
            return pageResult;
        }

        /// <summary>
        /// 返回 iBMC Server业务类, 该eSight的业务类对象。只能操作本eSight.
        /// </summary>
        public IDeviceWorker DviWorker
        {
            get
            {
                if (_deviceWorker == null)
                {
                    _deviceWorker = new DeviceWorker();
                    _deviceWorker.ESSession = this;
                }
                return _deviceWorker;
            }
        }
        /// <summary>
        /// 返回 软件源业务对象, 该eSight的业务类对象。只能操作本eSight.
        /// </summary>
        public ISoftwareSourceWorker SoftSourceWorker
        {
            get
            {
                if (_softSourceWorker == null)
                {
                    _softSourceWorker = new SoftwareSourceWorker();
                    _softSourceWorker.ESSession = this;
                }
                return _softSourceWorker;
            }
        }
        /// <summary>
        /// 返回 iBMC Server业务类, 该eSight的业务类对象。只能操作本eSight.
        /// </summary>
        public IHWDeployWorker DeployWorker
        {
            get
            {
                if (_hwDeployWorker == null)
                {
                    _hwDeployWorker = new HWDeployWorker();
                    _hwDeployWorker.ESSession = this;
                }
                return _hwDeployWorker;
            }
        }
        /// <summary>
        /// 返回 固件升级, 该eSight的业务类对象。只能操作本eSight.
        /// </summary>
        public IBasePackageWorker BasePackageWorker
        {
            get
            {
                if (_basePackageWorker == null)
                {
                    _basePackageWorker = new BasePackageWorker();
                    _basePackageWorker.ESSession = this;
                }
                return _basePackageWorker;
            }
        }
        #region database operation
        /// <summary>
        /// 保存当前eSight连接对象信息或状态到服务器。
        /// </summary>
        private void SyncToDB()
        {
            if (_hwESightHost.ID > 0)//是否新的eSight对象
            {
                SaveToDB();
            }
        }
        /// <summary>
        /// 锁对象，防止多线程操作。
        /// </summary>
        private object _lockInstance = new object();
        /// <summary>
        /// 保存当前eSight连接对象信息或状态到服务器。
        /// </summary
        public bool SaveToDB()
        {
            CheckInit();
            try
            {
                System.Threading.Monitor.Enter(_lockInstance);//开锁
                IHWESightHostDal hwESightHostDal = HWESightHostDal.Instance;
                LogUtil.HWLogger.API.DebugFormat("db id={0}", this.HWESightHost.ID);
                if (this.HWESightHost.ID > 0)//是否新的eSight对象
                {
                    HWESightHost hwESightHost = hwESightHostDal.GetEntityById(this.HWESightHost.ID);
                    if (hwESightHost != null)//是否新的eSight对象,是否找到
                    {
                        LogUtil.HWLogger.API.DebugFormat("update old id={0}", _hwESightHost.ID);
                        hwESightHostDal.UpdateEntity(this._hwESightHost);
                    }
                    else //重新保存一个新的。
                    {
                        LogUtil.HWLogger.API.Debug("add1 old id=" + HWESightHost.ID);
                        int oldid = _hwESightHost.ID;

                        //_hwESightHost.ID = 0;
                        _hwESightHost.CreateTime = System.DateTime.Now;
                        hwESightHostDal.UpdateEntity(_hwESightHost);
                        LogUtil.HWLogger.API.DebugFormat("add old id={0},new id={1}", oldid, _hwESightHost.ID);
                    }
                }
                else
                {
                    //插入新的eSight对象。
                    _hwESightHost.CreateTime = System.DateTime.Now;
                    _hwESightHost.ID = hwESightHostDal.InsertEntity(_hwESightHost);
                    LogUtil.HWLogger.API.DebugFormat("add1 id=" + _hwESightHost.ID);
                }
            }
            catch (Exception se) {
                LogUtil.HWLogger.API.Error(se);
                throw;
            }
            finally
            {
                System.Threading.Monitor.Exit(_lockInstance);//释放锁。
            }
            return true;
        }
        /// <summary>
        /// 删除eSight对象，从数据库中。
        /// </summary>
        /// <returns></returns>
        public bool DeleteESight()
        {
            CheckInit();//是否不存在eSight对象。
            IHWESightHostDal hwESightHostDal = HWESightHostDal.Instance;
            if (this.HWESightHost.ID > 0)
            {
                hwESightHostDal.DeleteESight(this.HWESightHost.ID);
            }
            else
            {
                LogUtil.HWLogger.API.WarnFormat("This eSight host has not add to the database，IP：{0}", HWESightHost.HostIP);
            }
            return true;
        }
        #endregion database operation
        #region dispose
        /// <summary>
        /// 销毁对象时调用
        /// </summary>
        ~ESSession()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        /// <summary>
        /// Implement IDisposable.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources 
        /// can be disposed.
        /// If disposing equals false, the method has been called by the 
        /// runtime from inside the finalizer and you should not reference 
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    try
                    {
                        this.Close();
                    }
                    catch (Exception se)
                    {
                        LogUtil.HWLogger.API.WarnFormat("There was an error clearing the connection", se);
                    }
                }
                // If disposing is false, only the following code is executed.
            }
            this._disposed = true;
        }


        #endregion dispose
        /// <summary>
        /// HttpClient对象。
        /// </summary>
        public HttpClient HClient
        {
            get { return _httpClient; }
            set { _httpClient = value; }
        }
    }
}
