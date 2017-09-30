using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Huawei.SCCMPlugin.DAO
{
    /// <summary>
    /// 数据库 DAO 基类
    /// </summary>
    /// <typeparam name="T">实体Model类</typeparam>
    public interface IBaseRepository<T>
    {
        /// <summary>
        /// 插入实体类数据到数据库。
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <returns>插入数量</returns>
        int InsertEntity(T entity);
        /// <summary>
        /// 批量插入实体类数据到数据库。
        /// </summary>
        /// <param name="entities">实体类 列表</param>
        /// <returns>插入数量</returns>
        int InsertEntitys(IList<T> entities);
        /// <summary>
        /// 更新实体类
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <returns>更新数量</returns>
        int UpdateEntity(T entity);
        /// <summary>
        /// 通过id删除数据。
        /// </summary>
        /// <param name="id">实体类ID</param>
        /// <returns>删除的数量</returns>
        int DeleteEntityById(int id);
        /// <summary>
        /// 通过ID查询实体类。
        /// </summary>
        /// <param name="id">实体类ID</param>
        /// <returns>填充好数据的实体类</returns>
        T GetEntityById(int id);
        /// <summary>
        /// 根据条件返回实体类列表。
        /// </summary>
        /// <param name="condition">条件</param>
        /// <returns>实体类列表。</returns>
        IList<T> GetList(string condition);
        /// <summary>
        /// 根据条件返回实体类分页。
        /// </summary>
        /// <param name="totalRows">返回总记录数</param>
        /// <param name="condition">条件</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageNum">页码</param>
        /// <param name="orderStr">排序字段</param>
        /// <param name="desc">是否倒序</param>
        /// <returns></returns>
        IList<T> GetList(out int totalRows, string condition = "1=1 ", int pageSize = 20, int pageNum = 1, string orderStr = "id", bool desc = false);
        /// <summary>
        /// 批量更新实体类数据到数据库
        /// </summary>
        /// <param name="entities">实体类列表</param>
        /// <returns></returns>
        int UpdateEntitys(IList<T> entities);
        /// <summary>
        /// 执行SQL，返回影响的行数。
        /// </summary>
        /// <param name="sql">SQL脚本</param>
        /// <returns></returns>
        int ExecuteSql(string sql);


    }
}
