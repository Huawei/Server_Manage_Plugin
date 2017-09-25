using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentData;
using CommonUtil.ModelHelper;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

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
                MyConnectionString = @"data source=" + getAndInitDatabase();
#endif
                _db = new DbContext().ConnectionString(MyConnectionString, new SqliteProvider());
                return _db;
            }
        }
        private static string getAndInitDatabase()
        {
            
            string userDBPath = "";
#if !DEBUG
            try
            {
                using (var mutex = new System.Threading.Mutex(false, "huawei.sccmplugin.db"))
                {
                    if (mutex.WaitOne(TimeSpan.FromSeconds(60), false))
                    {
                        var localPath = System.Environment.GetEnvironmentVariable("userprofile");//C:\Users\Public\Huawei\SCCM Plugin

                        var allUserPath = System.Environment.GetEnvironmentVariable("PUBLIC");

                        userDBPath = Path.Combine(localPath , "Huawei","SCCM Plugin","DB","db.sqlite");
                        string allDBPath = Path.Combine(allUserPath , "Huawei","SCCM Plugin","DB","db.sqlite");
                        if (!File.Exists(userDBPath))
                        {
                            //Init folder.
                            FileInfo file = new FileInfo(userDBPath);
                            if (!file.Directory.Exists) file.Directory.Create();
                            //Copy
                            if(File.Exists(allDBPath)) File.Copy(allDBPath, userDBPath);

                            AuthorizationRuleCollection accessRules = file.GetAccessControl().GetAccessRules(true, true,
                                                    typeof(System.Security.Principal.SecurityIdentifier));

                            System.Security.AccessControl.FileSecurity fileSecurity = file.GetAccessControl();
                            IList<FileSystemAccessRule> existsList = new List<FileSystemAccessRule>();
                            foreach (FileSystemAccessRule rule in accessRules)
                            {
                                //all rule.
                                existsList.Add(rule);
                            }
                            //Add full control to curent user.
                            WindowsIdentity wi = WindowsIdentity.GetCurrent();
                            IdentityReference ir = wi.User.Translate(typeof(NTAccount));
                            fileSecurity.AddAccessRule(new FileSystemAccessRule(ir, FileSystemRights.FullControl, AccessControlType.Allow));
                            //administrators
                            IdentityReference BuiltinAdministrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                            fileSecurity.AddAccessRule(new FileSystemAccessRule(BuiltinAdministrators, FileSystemRights.FullControl, AccessControlType.Allow));

                            //Clear all rules.
                            foreach (FileSystemAccessRule rule in existsList)
                            {
                                if (!rule.IdentityReference.Equals(ir) && !rule.Equals(BuiltinAdministrators))
                                    fileSecurity.RemoveAccessRuleAll(rule);
                            }
                            file.SetAccessControl(fileSecurity);
                        }
                    }
                }
            }
            catch (Exception se)
            {
                LogUtil.HWLogger.API.Error(se);
                throw;
            }
            
#endif
            return userDBPath;
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
