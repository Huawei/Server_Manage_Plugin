namespace Huawei.SCCMPlugin.PluginUI.Interface
{
    interface ICefBrowser
    {
        //bool SetAddress(string url);

        void ExecuteScript(string function, object data);

        void ExecuteScript(string jsCode);

        void RegisterJsObject<T>(string boundName) where T : new();
      
    }
}
