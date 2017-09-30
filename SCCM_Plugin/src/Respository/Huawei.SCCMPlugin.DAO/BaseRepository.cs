using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtil.ModelHelper;
using Huawei.SCCMPlugin.Models;
using LogUtil;

namespace Huawei.SCCMPlugin.DAO
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseModel
    {
        private const int NO_RS_CNT = -1;
        private string TableName
        {
            get
            {
                return TableConvention.Resolve(typeof(T));
            }
        }
        /// <summary>
        /// Inserts the entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>System.Int32.</returns>
        public int InsertEntity(T entity)
        {
            try
            {
                return DBUtility.Context.Insert<T>(TableName, entity).AutoMap(x => x.ID).ExecuteReturnLastId<int>();
            }
            catch (Exception ex)
            {
                HWLogger.DEFAULT.Error("InsertEntity Error:" + ex);
                throw;
                return NO_RS_CNT;
            }
        }

        /// <summary>
        /// Inserts the entitys.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns>System.Int32.</returns>
        public int InsertEntitys(IList<T> entities)
        {
            try
            {
                if (entities.Count == 0) return 0;
                using (var context = DBUtility.Context.UseTransaction(true))
                {
                    try
                    {
                        foreach (var item in entities)
                        {
                            context.Insert<T>(TableName, item).AutoMap(x => x.ID).Execute();
                        }
                        context.Commit();
                        return entities.Count;
                    }
                    catch (Exception)
                    {
                        context.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                HWLogger.DEFAULT.Error("InsertEntitys Error:" + ex);
                throw;
                return NO_RS_CNT;
            }
        }

        /// <summary>
        /// Updates the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>System.Int32.</returns>
        public int UpdateEntity(T entity)
        {
            try
            {
                return DBUtility.Context.Update<T>(TableName, entity).AutoMap(x => x.ID).Where(x => x.ID).Execute();
            }
            catch (Exception ex)
            {
                HWLogger.DEFAULT.Error("UpdateEntity Error:" + ex);
                throw;
                return NO_RS_CNT;
            }
        }

        /// <summary>
        /// Deletes the entity by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>System.Int32.</returns>
        public int DeleteEntityById(int id)
        {
            try
            {
                return DBUtility.Context.Delete(TableName).Where("ID", id).Execute();
            }
            catch (Exception ex)
            {
                HWLogger.DEFAULT.Error("DeleteEntityById Error:" + ex);
                throw;
                return NO_RS_CNT;
            }
        }

        /// <summary>
        /// Gets the entity by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>CodeFirstTestModel.</returns>
        public T GetEntityById(int id)
        {
            try
            {
                return DBUtility.Context.Sql("select * from  " + TableName + " where id=@0", id).QuerySingle<T>();
            }
            catch (Exception ex)
            {
                HWLogger.DEFAULT.Error("GetEntityById Error:" + ex);
                throw;
                return default(T);
            }
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="condition">condition</param>
        /// <returns>IList&lt;CodeFirstTestModel&gt;.</returns>
        public IList<T> GetList(string condition = "1=1 ")
        {
            try
            {
                return DBUtility.Context.Sql(string.Format("select * from {0} where {1}", TableName, condition)).QueryMany<T>();
            }
            catch (Exception ex)
            {
                HWLogger.DEFAULT.Error("GetList Error:" + ex);
                throw;
                return null;
            }
        }
        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="condition">condition</param>
        /// <returns>IList&lt;CodeFirstTestModel&gt;.</returns>
        public IList<T> GetList(out int totalRows, string condition = "1=1 ", int pageSize = 20, int pageNum = 1, string orderStr = "id", bool desc = false)
        {
            totalRows = 0;
            string countSql = "";
            string selSql = "";
            if (string.IsNullOrEmpty(orderStr)) orderStr = "id";
            string orderString = DBUtility.GetColumnByPropertyName(typeof(T), orderStr);
            if (desc) orderString = orderString + " desc";
            try
            {
                countSql = string.Format("select count(*) from {0}  where {1}", TableName, condition);
                selSql = string.Format("select * from {0} where {3} order by {4} limit {1} offset {2}", TableName, pageSize, (pageNum - 1) * pageSize, condition,
                    orderString);
                totalRows = DBUtility.Context.Sql(countSql).QuerySingle<int>();
                return DBUtility.Context.Sql(selSql).QueryMany<T>();

            }
            catch (Exception ex)
            {
                HWLogger.DEFAULT.Error("countSql:" + countSql);
                HWLogger.DEFAULT.Error("selSql:" + selSql);
                HWLogger.DEFAULT.Error("GetList Error:" + ex);
                throw;
                return null;
            }
        }

        /// <summary>
        /// update the entitys.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns>System.Int32.</returns>
        public int UpdateEntitys(IList<T> entities)
        {
            try
            {
                if (entities.Count == 0) return 0;
                using (var context = DBUtility.Context.UseTransaction(true))
                {
                    try
                    {
                        foreach (var item in entities)
                        {
                            context.Update<T>(TableName, item).AutoMap(x => x.ID).Where(x => x.ID).Execute(); ;
                        }
                        context.Commit();
                        return entities.Count;
                    }
                    catch (Exception)
                    {
                        context.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                HWLogger.DEFAULT.Error("UpdateEntitys Error:" + ex);
                throw;
                return NO_RS_CNT;
            }
        }


        /// <summary>
        /// Executes the SQL.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns>System.Int32.</returns>
        public int ExecuteSql(string sql)
        {
            try
            {
                return DBUtility.Context.Sql(sql).Execute();
            }
            catch (Exception ex)
            {
                HWLogger.DEFAULT.Error("ExecuteSql Error:" + ex);
                throw;
                return NO_RS_CNT;
            }
        }


    }
}
