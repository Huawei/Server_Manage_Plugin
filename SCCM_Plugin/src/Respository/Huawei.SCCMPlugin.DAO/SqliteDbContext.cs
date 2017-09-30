using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Huawei.SCCMPlugin.Models;
using SQLite.CodeFirst;

namespace Huawei.SCCMPlugin.DAO
{
    public class SqliteDbContext : DbContext
    {
        public SqliteDbContext()
       : base("DefaultConnection") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<SqliteDbContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        #region DbSet对象 
        public DbSet<CodeFirstTestModel> CodeFirstTestModels { get; set; }

        public DbSet<HWESightHost> HWESightHosts { get; set; }

        #endregion

    }
}
