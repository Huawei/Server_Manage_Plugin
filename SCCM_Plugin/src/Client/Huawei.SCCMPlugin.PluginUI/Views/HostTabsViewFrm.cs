using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using CefSharp;
using Huawei.SCCMPlugin.PluginUI.Interface;
using Huawei.SCCMPlugin.PluginUI.Attributes;
using Huawei.SCCMPlugin.PluginUI.Helper;
using CefSharpSchemeHandlerFactory = Huawei.SCCMPlugin.PluginUI.ESightScheme.CefSharpSchemeHandlerFactory;
using ESightBrowser = Huawei.SCCMPlugin.PluginUI.ESightScheme.ESightBrowser;
using Huawei.SCCMPlugin.PluginUI.ESightScheme;

namespace Huawei.SCCMPlugin.PluginUI.Views
{
    public partial class HostTabsViewFrm : Form
    {
        ESightBrowser eSightBrowser;
        public string Url { get; set; }
        public HostTabsViewFrm(string url)
        {
            try
            {
                InitializeComponent();

                //处理未捕捉异常
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.ThreadException += Application_ThreadException;

                //初始化
                this.Url = url;
                CefInstanse.Init(this);
                Application.ApplicationExit += Application_ApplicationExit;
            }
            catch (NullReferenceException se)
            {
                LogUtil.HWLogger.UI.Error(se);
            }
            catch (Exception se)
            {
                LogUtil.HWLogger.UI.Error("Error occurred when init HostTabsViewFrm: ", se);
            }
        }

        private void HostTabsViewFrm_Load(object sender, EventArgs e)
        {
            try
            {
                JumpToPage(Url);
            }
            catch (Exception se)
            {
                LogUtil.HWLogger.UI.Error("Error occurred when load HostTabsViewFrm: ", se);
            }
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            try
            {
                LogUtil.HWLogger.UI.Info("Exit application...");
                CefInstanse.Exit();
            }
            catch (Exception se)
            {
                LogUtil.HWLogger.UI.Error(se);
            }
        }

        /// <summary>
        /// Jumps to page.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void JumpToPage(string url)
        {
            try
            {
                if (!Cef.IsInitialized) return;

                LogUtil.HWLogger.UI.InfoFormat("beginning jump to [{0}]...", url);
                var handlerDict = HandlerHelper.HandlerDict;
                if (handlerDict!=null && handlerDict.ContainsKey(url.ToUpper()))
                {
                    var v = handlerDict[url.ToUpper()];
                    eSightBrowser = new ESightBrowser(url)
                    {
                        MenuHandler = new CefSharpContextMenuHandler(),
                        OnConsoleMessage = OnConsoleMessage,
                        OnLoadError = OnLoadError,
                        OnLoadFinish = OnLoadFinish
                    };

                    //var bound = v.GetType().GetCustomAttributes(typeof(BoundAttribute), false).FirstOrDefault() as BoundAttribute;
                    //eSightBrowser.RegisterJsObject(bound.BoundName, Activator.CreateInstance(v.GetType()));
                    //eSightBrowser.RegisterJsObject("NetBound", v);
                    if (v != null)
                    {
                        eSightBrowser.RegisterAsyncJsObject("NetBound", v);
                        this.pnlContent.Controls.Clear();
                        this.pnlContent.Controls.Add(eSightBrowser);
                    }
                }
                else
                {
                    MessageBox.Show(@"can not find handler:" + url);
                }
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Error occurred when load url: ", ex);
            }
        }

        /// <summary>
        /// Called when [load finish].
        /// </summary>
        public void OnLoadFinish()
        {
            LogUtil.HWLogger.UI.InfoFormat("url load finish!");
        }

        /// <summary>
        /// Called when [load error].
        /// </summary>
        /// <param name="failedUrl">The failed URL.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorText">The error text.</param>
        public void OnLoadError(string failedUrl, string errorCode, string errorText)
        {
            LogUtil.HWLogger.UI.Error($"LoadError：failedUrl:{failedUrl} errorCode:{errorCode} {errorText}");
        }

        /// <summary>
        /// Called when [console message].
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="line">The line.</param>
        /// <param name="message">The message.</param>
        public void OnConsoleMessage(string source, int line, string message)
        {
            //Debug.WriteLine($"'Source:{source} line:{line} {message}'");
        }

        #region 未捕捉异常处理
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogUtil.HWLogger.UI.Error("CurrentDomain_UnhandledException", (Exception)e.ExceptionObject);
        }

        private void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogUtil.HWLogger.UI.Error($"Application.ThreadException. Source:{e.Exception.Source}", e.Exception);
        }
        #endregion  
    }
}
