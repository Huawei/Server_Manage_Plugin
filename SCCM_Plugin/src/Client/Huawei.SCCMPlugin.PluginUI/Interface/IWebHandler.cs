namespace Huawei.SCCMPlugin.PluginUI.Interface
{
    public interface IWebHandler
    {
        /// <summary>
        /// 前端呼叫后端。
        /// 可以呼叫多次。
        /// 初始化，
        /// 提交数据。
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="eventData">The post data.</param>
        /// <returns>System.String.</returns>
        string Execute(string eventName, object eventData);

    }
}
