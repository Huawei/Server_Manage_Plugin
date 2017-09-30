using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Huawei.SCCMPlugin.Models;
using LogUtil;

namespace Huawei.SCCMPlugin.DAO
{
    public class CodeFirstTestDal: BaseRepository<CodeFirstTestModel>, ICodeFirstTestDal
    {
        public static CodeFirstTestDal Instance
        {
            get { return SingletonProvider<CodeFirstTestDal>.Instance; }
        }
       
        ///// <summary>
        ///// Inserts the entity.
        ///// </summary>
        ///// <param name="entity"></param>
        ///// <returns>System.Int32.</returns>
        //public int InsertEntity(CodeFirstTestModel entity)
        //{
        //    try
        //    {
        //        return DBUtility.Context.Insert<CodeFirstTestModel>(TableName, entity).AutoMap(x => x.ID).Execute();
        //    }
        //    catch (Exception ex)
        //    {
        //        HWLogger.DEFAULT.Error("InsertEntity Error:"+ex);
        //        return -1;
        //    }
        //}

        ///// <summary>
        ///// Inserts the entitys.
        ///// </summary>
        ///// <param name="entities">The entities.</param>
        ///// <returns>System.Int32.</returns>
        //public int InsertEntitys(IList<CodeFirstTestModel> entities)
        //{
        //    try
        //    {
        //        if (entities.Count == 0) return 0;
        //        using (var context = DBUtility.Context.UseTransaction(true))
        //        {
        //            try
        //            {
        //                foreach (var item in entities)
        //                {
        //                    context.Insert<CodeFirstTestModel>(TableName, item).AutoMap(x => x.ID).Execute();
        //                }
        //                context.Commit();

        //                return entities.Count;
        //            }
        //            catch (Exception)
        //            {
        //                context.Rollback();
        //                throw;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        HWLogger.DEFAULT.Error("InsertEntitys Error:" + ex);
        //        return -1;
        //    }
        //}

        ///// <summary>
        ///// Updates the entity.
        ///// </summary>
        ///// <param name="entity">The entity.</param>
        ///// <returns>System.Int32.</returns>
        //public int UpdateEntity(CodeFirstTestModel entity)
        //{
        //    try
        //    {
        //        return DBUtility.Context.Update<CodeFirstTestModel>(TableName, entity).AutoMap(x => x.ID).Where(x => x.ID).Execute();

        //    }
        //    catch (Exception ex)
        //    {
        //        HWLogger.DEFAULT.Error("UpdateEntity Error:" + ex);
        //        return -1;
        //    }
        //}

        ///// <summary>
        ///// Deletes the entity by identifier.
        ///// </summary>
        ///// <param name="id">The identifier.</param>
        ///// <returns>System.Int32.</returns>
        //public int DeleteEntityById(long id)
        //{
        //    try
        //    {
        //        return DBUtility.Context.Delete(TableName).Where("Id", id).Execute();
        //    }
        //    catch (Exception ex)
        //    {
        //        HWLogger.DEFAULT.Error("DeleteEntityById Error:" + ex);
        //        return -1;
        //    }
        //}

        ///// <summary>
        ///// Gets the entity by identifier.
        ///// </summary>
        ///// <param name="id">The identifier.</param>
        ///// <returns>CodeFirstTestModel.</returns>
        //public CodeFirstTestModel GetEntityById(long id)
        //{
        //    try
        //    {
        //        return DBUtility.Context.Sql("select * from  " + TableName + " where id=@0", id).QuerySingle<CodeFirstTestModel>();
        //    }
        //    catch (Exception ex)
        //    {
        //        HWLogger.DEFAULT.Error("GetEntityById Error:" + ex);
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// Gets the list.
        ///// </summary>
        ///// <param name="condition">The exprss where.</param>
        ///// <returns>IList&lt;CodeFirstTestModel&gt;.</returns>
        //public IList<CodeFirstTestModel> GetList(string condition)
        //{
        //    try
        //    {
        //        return DBUtility.Context.Sql(string.Format("select * from {0} where {1}", TableName, condition)).QueryMany<CodeFirstTestModel>();
        //    }
        //    catch (Exception ex)
        //    {
        //        HWLogger.DEFAULT.Error("GetList Error:" + ex);
        //        return null;
        //    }
        //}
    }
}
