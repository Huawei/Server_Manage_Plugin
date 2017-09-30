using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentData;
using CommonUtil.ModelHelper;

namespace Huawei.SCCMPlugin.DAO
{
    public class DBUtility
    {

        private static IDbContext _db = null;

        /// <summary>
        /// DbContext
        /// </summary>
        public static IDbContext Context
        {
            get
            {
                string MyConnectionString = @"data source=|DataDirectory|db.sqlite";
#if !DEBUG
                var localPath= System.Environment.GetEnvironmentVariable("PUBLIC");//C:\Users\Public\Huawei\SCCM Plugin
                MyConnectionString = @"data source=" + localPath + "\\Huawei\\SCCM Plugin\\DB\\db.sqlite";
#endif
                _db = new DbContext().ConnectionString(MyConnectionString, new SqliteProvider());
                return _db;
            }
        }

        public static string GetColumnByPropertyName(Type modelType, string propName)
        {
            Dictionary<string, string> propDict = GetPropertyDictionary(modelType);
            if (propDict.ContainsKey(propName.ToLower()))
            {
                return propDict[propName.ToLower()];
            }
            return propName;
        }
        public static Dictionary<string, string> GetPropertyDictionary(Type type)
        {
            var result = new Dictionary<string, string>();

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                /***********(query)属性Name和数据库表字段Column不一样时创建对应映射关系   by suxiaobo  2017-05-02 19:26***************/
                var column = property.GetCustomAttributes(true).OfType<DbColumnAttribute>().FirstOrDefault();
                var columnName = property.Name;
                if (column != null)
                {
                    columnName = column.ColumnName;
                    result.Add(property.Name.ToLower(), columnName);
                }
            }
            return result;
        }
    }
}
