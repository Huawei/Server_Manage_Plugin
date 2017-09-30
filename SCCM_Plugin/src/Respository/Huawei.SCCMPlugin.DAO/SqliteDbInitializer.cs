using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Models;
using SQLite.CodeFirst;

namespace Huawei.SCCMPlugin.DAO
{
    public class SqliteDbInitializer : SqliteDropCreateDatabaseAlways<SqliteDbContext>
    {
        public SqliteDbInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder) { }

        /// <summary>
        /// 此方法可以设置生成的数据库表初始值（如一些默认配置）
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void Seed(SqliteDbContext context)
        {
            //示例代码 
            //添加一条默认数据
            // context.Set<CodeFirstTestModel>().Add(new CodeFirstTestModel(){ Name = "DefaultName" , CreateTime = DateTime.Now}) ;//生成数据库表后添加默认数据
            //添加多条默认数据
            //var list = new List<CodeFirstTestModel>
            //{
            //    new CodeFirstTestModel() {Name = "DefaultName1", CreateTime = DateTime.Now},
            //    new CodeFirstTestModel() {Name = "DefaultName2", CreateTime = DateTime.Now}
            //};
            //context.Set<CodeFirstTestModel>().AddRange(list);
        }
    }
}
