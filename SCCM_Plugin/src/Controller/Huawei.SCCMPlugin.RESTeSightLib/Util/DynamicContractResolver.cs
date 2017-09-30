using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.RESTeSightLib.Util
{
    /// <summary>
    /// Json序列化实现。
    /// </summary>
    public class DynamicContractResolver : DefaultContractResolver
    {
        private IList<string> _propertiesToSerialize = null;
        /// <summary>
        /// 动态JSON解析
        /// </summary>
        /// <param name="propertiesToSerialize">需要解析的JSON列表</param>
        public DynamicContractResolver(IList<string> propertiesToSerialize)
        {
            _propertiesToSerialize = propertiesToSerialize;
        }
        /// <summary>
        /// 创建需要序列化的属性列表，默认实现json.net接口。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            return properties.Where(p => _propertiesToSerialize.Contains(p.PropertyName)).ToList();
        }
    }
}
