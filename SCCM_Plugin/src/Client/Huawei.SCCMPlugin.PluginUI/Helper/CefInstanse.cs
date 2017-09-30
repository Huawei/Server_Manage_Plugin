using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CefSharp;
using Huawei.SCCMPlugin.PluginUI.ESightScheme;
using Huawei.SCCMPlugin.PluginUI.Views;
using System.Windows;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Huawei.SCCMPlugin.PluginUI.Helper
{
    /// <summary>
    /// 多线程初始化CefSharp
    /// </summary>
    public class CefInstanse
    {
        static ParamThread method;
        static Thread cefInitThread;

        /// <summary>
        /// Initializes CefSharp .
        /// Need to Initialize and Shutdown Cef on the same thread. 
        /// 在线程中初始化cef，并在应用程序退出时，在该线程shutdown Cef
        /// </summary>
        /// <param name="form">The form.</param>
        public static void Init(HostTabsViewFrm form)
        {
            if (cefInitThread == null)
            {
                method = new ParamThread(form);
                cefInitThread = new Thread(new ThreadStart(method.DoInit));
                cefInitThread.Start();                
            }
        }

        public static void Exit()
        {
            method.mEvent.Set();
        }

    }
    public class ParamThread
    {
        public AutoResetEvent mEvent = new AutoResetEvent(false);
        private HostTabsViewFrm form;

        //包含参数的构造函数
        public ParamThread(HostTabsViewFrm _form)
        {
            this.form = _form;
        }

        public void DoInit()
        {
            #region Cef.IsInitialized
            try
            {
                if (!Cef.IsInitialized)
                {
                    LogUtil.HWLogger.UI.InfoFormat("init cefsharp...");
                    //Cef.EnableHighDPISupport();

                    CefSettings settings = new CefSettings()
                    {
                        CachePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "CefSharp\\Cache")
                        //,                        PackLoadingDisabled = true
                    };
                    settings.RegisterScheme(new CefCustomScheme()
                    {
                        SchemeName = CefSharpSchemeHandlerFactory.SchemeName,
                        SchemeHandlerFactory = new CefSharpSchemeHandlerFactory()
                    });
                    settings.UserAgent = "ESight Browser" + Cef.CefSharpVersion;

                    //settings.SetOffScreenRenderingBestPerformanceArgs();
                    settings.CefCommandLineArgs.Add("disable-gpu", "1");
                    settings.CefCommandLineArgs.Add("disable-gpu-compositing", "1");
                    settings.CefCommandLineArgs.Add("enable-begin-frame-scheduling", "1");
                    settings.CefCommandLineArgs.Add("no-proxy-server", "1");
                    settings.CefCommandLineArgs.Add("disable-plugins-discovery", "1");
                    settings.CefCommandLineArgs.Add("disable-gpu-vsync", "1"); //Disable Vsync

                    //Disables the DirectWrite font rendering system on windows.
                    //Possibly useful when experiencing blury fonts.
                    //settings.CefCommandLineArgs.Add("disable-direct-write", "1");

                    settings.MultiThreadedMessageLoop = true;

                    //Chromium Command Line args
                    //http://peter.sh/experiments/chromium-command-line-switches/
                    //settings.CefCommandLineArgs.Add("renderer-process-limit", "1");
                    settings.WindowlessRenderingEnabled = true;//Set to true (1) to enable windowless (off-screen) rendering support. Do not enable this value if the application does not use windowless rendering as it may reduce rendering performance on some systems.
                    Cef.Initialize(settings, true, false);
                    //首次加载页面需要等待cef初始化完成
                    SafeInvoke(form, () => { form.JumpToPage(form.Url); });

                    mEvent.WaitOne();
                    Cef.Shutdown();

                }
            }
            catch(TaskCanceledException te)
            {
                LogUtil.HWLogger.UI.Error("Error occurred when init cefsharp(TaskCanceledException): ", te);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Error occurred when init cefsharp: ", ex);
            }
            #endregion
        }

        private static void SafeInvoke(Form uiElement, Action action)
        {
            try
            {
                if (uiElement.InvokeRequired)
                {
                    LogUtil.HWLogger.UI.Info("call action using BeginInvoke mode.");
                    uiElement.BeginInvoke(action);
                }
                else
                {
                    LogUtil.HWLogger.UI.Info("call action using Invoke mode.");
                    action.Invoke();
                }
            }
            catch (TaskCanceledException te)
            {
                LogUtil.HWLogger.UI.Error("Error occurred when init cefsharp(TaskCanceledException): ", te);
            }
            catch (Exception ex)
            {
                LogUtil.HWLogger.UI.Error("Error occurred when init cefsharp: ", ex);
            }
        }
    }
}
