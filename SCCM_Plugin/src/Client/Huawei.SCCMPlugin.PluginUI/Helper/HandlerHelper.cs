using Huawei.SCCMPlugin.PluginUI.Attributes;
using Huawei.SCCMPlugin.PluginUI.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.PluginUI.Helper
{
    /// <summary>
    /// Handler对象帮助器
    /// </summary>
    public sealed class HandlerHelper
    {
        private static Dictionary<string, IWebHandler> _handlerDict = null;

        public static Dictionary<string, IWebHandler> HandlerDict
        {
            get
            {
                if (_handlerDict == null || _handlerDict.Count == 0)
                {
                    LogUtil.HWLogger.UI.InfoFormat("HandlerDict does not found data, need collect the data of handler.");
                    try
                    {
                        if (_handlerDict == null)
                        {
                            _handlerDict = new Dictionary<string, IWebHandler>();
                        }

                        IList<Type> types = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("Huawei.SCCMPlugin.PluginUI"))
                        .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IWebHandler))))
                        .ToArray();

                        foreach (Type handlerType in types)
                        {
                            var cefUrlAttr = handlerType.GetCustomAttributes(typeof(CefURLAttribute), false).FirstOrDefault() as CefURLAttribute;
                            if (cefUrlAttr == null) throw new ArgumentNullException(nameof(cefUrlAttr));
                            _handlerDict.Add(cefUrlAttr.URL.ToUpper(), (IWebHandler)Activator.CreateInstance(handlerType));
                        }
                    }
                    catch
                    {
                        throw;
                    }
                    LogUtil.HWLogger.UI.InfoFormat("HandlerDict collect finish, total is [{0}]", _handlerDict.Count);
                }
                return _handlerDict;
            }
        }
    }
}
