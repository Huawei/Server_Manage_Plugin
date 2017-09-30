
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Data.SQLite;
using CommonUtil.ModelHelper;

namespace FluentData
{
    internal class ActionsHandler
    {
        private bool _autoMappedAlreadyCalled = false;

        private readonly BuilderData _data;

        internal ActionsHandler(BuilderData data)
        {
            _data = data;
        }

        internal void ColumnValueAction(string columnName, object value, DataTypes parameterType, int size)
        {
            ColumnAction(columnName, value, typeof(object), parameterType, size);
        }

        private void ColumnAction(string columnName, object value, Type type, DataTypes parameterType, int size)
        {
            var parameterName = columnName;

            _data.Columns.Add(new BuilderColumn(columnName, value, parameterName));

            if (parameterType == DataTypes.Object)
                parameterType = _data.Command.Data.Context.Data.FluentDataProvider.GetDbTypeForClrType(type);

            ParameterAction(parameterName, value, parameterType, ParameterDirection.Input, size);
        }

        internal void ColumnValueAction<T>(Expression<Func<T, object>> expression, DataTypes parameterType, int size)
        {
            var parser = new PropertyExpressionParser<T>(_data.Item, expression);

            ColumnAction(parser.Name, parser.Value, parser.Type, parameterType, size);
        }

        internal void ColumnValueDynamic(ExpandoObject item, string propertyName, DataTypes parameterType, int size)
        {
            var propertyValue = (item as IDictionary<string, object>)[propertyName];

            ColumnAction(propertyName, propertyValue, typeof(object), parameterType, size);
        }

        private void VerifyAutoMapAlreadyCalled()
        {
            if (_autoMappedAlreadyCalled)
                throw new FluentDataException("AutoMap cannot be called more than once.");
            _autoMappedAlreadyCalled = true;
        }

        internal void AutoMapColumnsAction<T>(params Expression<Func<T, object>>[] ignorePropertyExpressions)
        {
            VerifyAutoMapAlreadyCalled();

            var properties = ReflectionHelper.GetProperties(_data.Item.GetType());
            var ignorePropertyNames = new HashSet<string>();
            if (ignorePropertyExpressions != null)
            {
                foreach (var ignorePropertyExpression in ignorePropertyExpressions)
                {
                    var ignorePropertyName = new PropertyExpressionParser<T>(_data.Item, ignorePropertyExpression).Name;
                    ignorePropertyNames.Add(ignorePropertyName);
                }
            }

            //foreach (var property in properties)
            //{
            //	var ignoreProperty = ignorePropertyNames.SingleOrDefault(x => x.Equals(property.Value.Name, StringComparison.CurrentCultureIgnoreCase));
            //	if (ignoreProperty != null)
            //		continue;

            //	var propertyType = ReflectionHelper.GetPropertyType(property.Value);

            //	var propertyValue = ReflectionHelper.GetPropertyValue(_data.Item, property.Value);
            //	ColumnAction(property.Value.Name, propertyValue, propertyType, DataTypes.Object, 0);
            //}
            foreach (var property in properties)
            {
                var ignoreProperty = ignorePropertyNames.SingleOrDefault(x => x.Equals(property.Value.Name, StringComparison.CurrentCultureIgnoreCase));
                if (ignoreProperty != null)
                    continue;

                /***********(insert or update)通过两个自定义特性实现：实体属性是否需要映射到数据库表字段；属性Name和数据库表字段Column不一样时创建对应映射关系   by suxiaobo  2017-05-02 19:26*************************/
                //实体属性是否需要映射到数据库表字段
                int i = property.Value.GetCustomAttributes(true).OfType<DbFieldAttribute>().Count(atr => !(atr).IsDbField);
                if (i > 0)
                    continue;

                //属性Name和数据库表字段Column不一样时创建对应映射关系
                var column = property.Value.GetCustomAttributes(true).OfType<DbColumnAttribute>().FirstOrDefault();
                var columnName = property.Value.Name;
                if (column != null)
                {
                    columnName = column.ColumnName;
                    var propertyType = ReflectionHelper.GetPropertyType(property.Value);
                    var propertyValue = ReflectionHelper.GetPropertyValue(_data.Item, property.Value);
                    ColumnAction(columnName, propertyValue, propertyType, DataTypes.Object, 0);
                }
            }
        }

        internal void AutoMapDynamicTypeColumnsAction(params string[] ignorePropertyExpressions)
        {
            VerifyAutoMapAlreadyCalled();

            var properties = (IDictionary<string, object>)_data.Item;
            var ignorePropertyNames = new HashSet<string>();
            if (ignorePropertyExpressions != null)
            {
                foreach (var ignorePropertyExpression in ignorePropertyExpressions)
                    ignorePropertyNames.Add(ignorePropertyExpression);
            }

            foreach (var property in properties)
            {
                var ignoreProperty = ignorePropertyNames.SingleOrDefault(x => x.Equals(property.Key, StringComparison.CurrentCultureIgnoreCase));

                if (ignoreProperty == null)
                    ColumnAction(property.Key, property.Value, typeof(object), DataTypes.Object, 0);
            }
        }

        private void ParameterAction(string name, object value, DataTypes dataType, ParameterDirection direction, int size)
        {
            _data.Command.Parameter(name, value, dataType, direction, size);
        }

        internal void ParameterOutputAction(string name, DataTypes dataTypes, int size)
        {
            ParameterAction(name, null, dataTypes, ParameterDirection.Output, size);
        }

        internal void WhereAction(string columnName, object value, DataTypes parameterType, int size)
        {
            var parameterName = columnName;
            ParameterAction(parameterName, value, parameterType, ParameterDirection.Input, 0);

            _data.Where.Add(new BuilderColumn(columnName, value, parameterName));
        }

        internal void WhereAction<T>(Expression<Func<T, object>> expression, DataTypes parameterType, int size)
        {
            var parser = new PropertyExpressionParser<T>(_data.Item, expression);
            WhereAction(parser.Name, parser.Value, parameterType, size);
        }
    }

    public class BuilderData
    {
        public List<BuilderColumn> Columns { get; set; }
        public object Item { get; set; }
        public string ObjectName { get; set; }
        public IDbCommand Command { get; set; }
        public List<BuilderColumn> Where { get; set; }

        public BuilderData(IDbCommand command, string objectName)
        {
            ObjectName = objectName;
            Command = command;
            Columns = new List<BuilderColumn>();
            Where = new List<BuilderColumn>();
        }
    }

    internal abstract class BaseDeleteBuilder
    {
        public BuilderData Data { get; set; }
        protected ActionsHandler Actions { get; set; }

        public BaseDeleteBuilder(IDbCommand command, string name)
        {
            Data = new BuilderData(command, name);
            Actions = new ActionsHandler(Data);
        }

        public int Execute()
        {
            Data.Command.Sql(Data.Command.Data.Context.Data.FluentDataProvider.GetSqlForDeleteBuilder(Data));

            return Data.Command.Execute();
        }
    }

    internal class DeleteBuilder : BaseDeleteBuilder, IDeleteBuilder
    {
        public DeleteBuilder(IDbCommand command, string tableName)
            : base(command, tableName)
        {
        }

        public IDeleteBuilder Where(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }
    }

    internal class DeleteBuilder<T> : BaseDeleteBuilder, IDeleteBuilder<T>
    {
        public DeleteBuilder(IDbCommand command, string tableName, T item)
            : base(command, tableName)
        {
            Data.Item = item;
        }
        public IDeleteBuilder<T> Where(Expression<Func<T, object>> expression, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(expression, parameterType, size);
            return this;
        }

        public IDeleteBuilder<T> Where(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }
    }

    public interface IDeleteBuilder : IExecute
    {
        BuilderData Data { get; }
        IDeleteBuilder Where(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
    }

    public interface IDeleteBuilder<T> : IExecute
    {
        BuilderData Data { get; }
        IDeleteBuilder<T> Where(Expression<Func<T, object>> expression, DataTypes parameterType = DataTypes.Object, int size = 0);
        IDeleteBuilder<T> Where(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
    }

    public interface IInsertUpdateBuilder
    {
        BuilderData Data { get; }
        IInsertUpdateBuilder Column(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
    }

    public interface IInsertUpdateBuilderDynamic
    {
        BuilderData Data { get; }
        dynamic Item { get; }
        IInsertUpdateBuilderDynamic Column(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IInsertUpdateBuilderDynamic Column(string propertyName, DataTypes parameterType = DataTypes.Object, int size = 0);
    }

    public interface IInsertUpdateBuilder<T>
    {
        BuilderData Data { get; }
        T Item { get; }
        IInsertUpdateBuilder<T> Column(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IInsertUpdateBuilder<T> Column(Expression<Func<T, object>> expression, DataTypes parameterType = DataTypes.Object, int size = 0);
    }

    internal abstract class BaseInsertBuilder
    {
        public BuilderData Data { get; set; }
        protected ActionsHandler Actions { get; set; }

        public BaseInsertBuilder(IDbCommand command, string name)
        {
            Data = new BuilderData(command, name);
            Actions = new ActionsHandler(Data);
        }

        private IDbCommand GetPreparedCommand()
        {
            Data.Command.ClearSql.Sql(Data.Command.Data.Context.Data.FluentDataProvider.GetSqlForInsertBuilder(Data));
            return Data.Command;
        }

        public int Execute()
        {
            return GetPreparedCommand().Execute();
        }

        public T ExecuteReturnLastId<T>(string identityColumnName = null)
        {
            return GetPreparedCommand().ExecuteReturnLastId<T>(identityColumnName);
        }
    }

    internal class InsertBuilder : BaseInsertBuilder, IInsertBuilder, IInsertUpdateBuilder
    {
        internal InsertBuilder(IDbCommand command, string name)
            : base(command, name)
        {
        }

        public IInsertBuilder Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        IInsertUpdateBuilder IInsertUpdateBuilder.Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        public IInsertBuilder Fill(Action<IInsertUpdateBuilder> fillMethod)
        {
            fillMethod(this);
            return this;
        }
    }

    internal class InsertBuilderDynamic : BaseInsertBuilder, IInsertBuilderDynamic, IInsertUpdateBuilderDynamic
    {
        public dynamic Item { get; private set; }

        internal InsertBuilderDynamic(IDbCommand command, string name, ExpandoObject item)
            : base(command, name)
        {
            Data.Item = item;
            Item = item;
        }

        public IInsertBuilderDynamic Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        public IInsertBuilderDynamic Column(string propertyName, DataTypes parameterType, int size)
        {
            Actions.ColumnValueDynamic((ExpandoObject)Data.Item, propertyName, parameterType, size);
            return this;
        }

        public IInsertBuilderDynamic AutoMap(params string[] ignoreProperties)
        {
            Actions.AutoMapDynamicTypeColumnsAction(ignoreProperties);
            return this;
        }

        public IInsertBuilderDynamic Fill(Action<IInsertUpdateBuilderDynamic> fillMethod)
        {
            fillMethod(this);
            return this;
        }

        IInsertUpdateBuilderDynamic IInsertUpdateBuilderDynamic.Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        IInsertUpdateBuilderDynamic IInsertUpdateBuilderDynamic.Column(string propertyName, DataTypes parameterType, int size)
        {
            Actions.ColumnValueDynamic((ExpandoObject)Data.Item, propertyName, parameterType, size);
            return this;
        }
    }

    internal class InsertBuilder<T> : BaseInsertBuilder, IInsertBuilder<T>, IInsertUpdateBuilder<T>
    {
        public T Item { get; private set; }

        internal InsertBuilder(IDbCommand command, string name, T item)
            : base(command, name)
        {
            Data.Item = item;
            Item = item;
        }

        public IInsertBuilder<T> Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        public IInsertBuilder<T> Column(Expression<Func<T, object>> expression, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(expression, parameterType, size);
            return this;
        }

        public IInsertBuilder<T> Fill(Action<IInsertUpdateBuilder<T>> fillMethod)
        {
            fillMethod(this);
            return this;
        }

        public IInsertBuilder<T> AutoMap(params Expression<Func<T, object>>[] ignoreProperties)
        {
            Actions.AutoMapColumnsAction(ignoreProperties);
            return this;
        }

        IInsertUpdateBuilder<T> IInsertUpdateBuilder<T>.Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        IInsertUpdateBuilder<T> IInsertUpdateBuilder<T>.Column(Expression<Func<T, object>> expression, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(expression, parameterType, size);
            return this;
        }
    }

    public interface IInsertBuilder : IExecute, IExecuteReturnLastId
    {
        BuilderData Data { get; }
        IInsertBuilder Column(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IInsertBuilder Fill(Action<IInsertUpdateBuilder> fillMethod);
    }

    public interface IInsertBuilder<T> : IExecute, IExecuteReturnLastId
    {
        BuilderData Data { get; }
        T Item { get; }
        IInsertBuilder<T> AutoMap(params Expression<Func<T, object>>[] ignoreProperties);
        IInsertBuilder<T> Column(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IInsertBuilder<T> Column(Expression<Func<T, object>> expression, DataTypes parameterType = DataTypes.Object, int size = 0);
        IInsertBuilder<T> Fill(Action<IInsertUpdateBuilder<T>> fillMethod);
    }

    public interface IInsertBuilderDynamic : IExecute, IExecuteReturnLastId
    {
        BuilderData Data { get; }
        dynamic Item { get; }
        IInsertBuilderDynamic AutoMap(params string[] ignoreProperties);
        IInsertBuilderDynamic Column(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IInsertBuilderDynamic Column(string propertyName, DataTypes parameterType = DataTypes.Object, int size = 0);
        IInsertBuilderDynamic Fill(Action<IInsertUpdateBuilderDynamic> fillMethod);
    }

    public interface ISelectBuilder<TEntity>
    {
        SelectBuilderData Data { get; set; }
        ISelectBuilder<TEntity> Select(string sql);
        ISelectBuilder<TEntity> From(string sql);
        ISelectBuilder<TEntity> Where(string sql);
        ISelectBuilder<TEntity> AndWhere(string sql);
        ISelectBuilder<TEntity> OrWhere(string sql);
        ISelectBuilder<TEntity> GroupBy(string sql);
        ISelectBuilder<TEntity> OrderBy(string sql);
        ISelectBuilder<TEntity> Having(string sql);
        ISelectBuilder<TEntity> Paging(int currentPage, int itemsPerPage);
        ISelectBuilder<TEntity> Parameter(string name, object value, DataTypes parameterType = DataTypes.Object, ParameterDirection direction = ParameterDirection.Input, int size = 0);
        ISelectBuilder<TEntity> Parameters(params object[] parameters);

        List<TEntity> QueryMany(Action<TEntity, IDataReader> customMapper = null);
        List<TEntity> QueryMany(Action<TEntity, dynamic> customMapper);
        TList QueryMany<TList>(Action<TEntity, IDataReader> customMapper = null) where TList : IList<TEntity>;
        TList QueryMany<TList>(Action<TEntity, dynamic> customMapper) where TList : IList<TEntity>;
        void QueryComplexMany(IList<TEntity> list, Action<IList<TEntity>, IDataReader> customMapper);
        void QueryComplexMany(IList<TEntity> list, Action<IList<TEntity>, dynamic> customMapper);
        TEntity QuerySingle(Action<TEntity, IDataReader> customMapper = null);
        TEntity QuerySingle(Action<TEntity, dynamic> customMapper);
        TEntity QueryComplexSingle(Func<IDataReader, TEntity> customMapper);
        TEntity QueryComplexSingle(Func<dynamic, TEntity> customMapper);
    }

    internal class SelectBuilder<TEntity> : ISelectBuilder<TEntity>
    {
        public SelectBuilderData Data { get; set; }
        protected ActionsHandler Actions { get; set; }

        public SelectBuilder(IDbCommand command)
        {
            Data = new SelectBuilderData(command, "");
            Actions = new ActionsHandler(Data);
        }

        private IDbCommand GetPreparedDbCommand()
        {
            if (Data.PagingItemsPerPage > 0
                    && string.IsNullOrEmpty(Data.OrderBy))
                throw new FluentDataException("Order by must defined when using Paging.");

            Data.Command.ClearSql.Sql(Data.Command.Data.Context.Data.FluentDataProvider.GetSqlForSelectBuilder(Data));
            return Data.Command;
        }

        public ISelectBuilder<TEntity> Select(string sql)
        {
            Data.Select += sql;
            return this;
        }

        public ISelectBuilder<TEntity> From(string sql)
        {
            Data.From += sql;
            return this;
        }

        public ISelectBuilder<TEntity> Where(string sql)
        {
            Data.WhereSql += sql;
            return this;
        }

        public ISelectBuilder<TEntity> AndWhere(string sql)
        {
            if (Data.WhereSql.Length > 0)
                Data.WhereSql += " and ";
            Data.WhereSql += sql;
            return this;
        }

        public ISelectBuilder<TEntity> OrWhere(string sql)
        {
            if (Data.WhereSql.Length > 0)
                Data.WhereSql += " or ";
            Data.WhereSql += sql;
            return this;
        }

        public ISelectBuilder<TEntity> OrderBy(string sql)
        {
            Data.OrderBy += sql;
            return this;
        }

        public ISelectBuilder<TEntity> GroupBy(string sql)
        {
            Data.GroupBy += sql;
            return this;
        }

        public ISelectBuilder<TEntity> Having(string sql)
        {
            Data.Having += sql;
            return this;
        }

        public ISelectBuilder<TEntity> Paging(int currentPage, int itemsPerPage)
        {
            Data.PagingCurrentPage = currentPage;
            Data.PagingItemsPerPage = itemsPerPage;
            return this;
        }

        public ISelectBuilder<TEntity> Parameter(string name, object value, DataTypes parameterType, ParameterDirection direction, int size)
        {
            Data.Command.Parameter(name, value, parameterType, direction, size);
            return this;
        }

        public ISelectBuilder<TEntity> Parameters(params object[] parameters)
        {
            Data.Command.Parameters(parameters);
            return this;
        }
        public List<TEntity> QueryMany(Action<TEntity, IDataReader> customMapper = null)
        {
            return GetPreparedDbCommand().QueryMany<TEntity>(customMapper);
        }

        public List<TEntity> QueryMany(Action<TEntity, dynamic> customMapper)
        {
            return GetPreparedDbCommand().QueryMany<TEntity>(customMapper);
        }

        public TList QueryMany<TList>(Action<TEntity, IDataReader> customMapper = null) where TList : IList<TEntity>
        {
            return GetPreparedDbCommand().QueryMany<TEntity, TList>(customMapper);
        }

        public TList QueryMany<TList>(Action<TEntity, dynamic> customMapper) where TList : IList<TEntity>
        {
            return GetPreparedDbCommand().QueryMany<TEntity, TList>(customMapper);
        }

        public void QueryComplexMany(IList<TEntity> list, Action<IList<TEntity>, IDataReader> customMapper)
        {
            GetPreparedDbCommand().QueryComplexMany<TEntity>(list, customMapper);
        }

        public void QueryComplexMany(IList<TEntity> list, Action<IList<TEntity>, dynamic> customMapper)
        {
            GetPreparedDbCommand().QueryComplexMany<TEntity>(list, customMapper);
        }

        public TEntity QuerySingle(Action<TEntity, IDataReader> customMapper = null)
        {
            return GetPreparedDbCommand().QuerySingle<TEntity>(customMapper);
        }

        public TEntity QuerySingle(Action<TEntity, dynamic> customMapper)
        {
            return GetPreparedDbCommand().QuerySingle<TEntity>(customMapper);
        }

        public TEntity QueryComplexSingle(Func<IDataReader, TEntity> customMapper)
        {
            return GetPreparedDbCommand().QueryComplexSingle(customMapper);
        }

        public TEntity QueryComplexSingle(Func<dynamic, TEntity> customMapper)
        {
            return GetPreparedDbCommand().QueryComplexSingle(customMapper);
        }
    }

    public class SelectBuilderData : BuilderData
    {
        public int PagingCurrentPage { get; set; }
        public int PagingItemsPerPage { get; set; }
        public string Having { get; set; }
        public string GroupBy { get; set; }
        public string OrderBy { get; set; }
        public string From { get; set; }
        public string Select { get; set; }
        public string WhereSql { get; set; }

        public SelectBuilderData(IDbCommand command, string objectName) : base(command, objectName)
        {
            Having = "";
            GroupBy = "";
            OrderBy = "";
            From = "";
            Select = "";
            WhereSql = "";
            PagingCurrentPage = 1;
            PagingItemsPerPage = 0;
        }

        internal int GetFromItems()
        {
            return (GetToItems() - PagingItemsPerPage + 1);
        }

        internal int GetToItems()
        {
            return (PagingCurrentPage * PagingItemsPerPage);
        }
    }

    internal abstract class BaseStoredProcedureBuilder
    {
        public BuilderData Data { get; set; }
        protected ActionsHandler Actions { get; set; }

        public BaseStoredProcedureBuilder(IDbCommand command, string name)
        {
            Data = new BuilderData(command, name);
            Actions = new ActionsHandler(Data);
        }

        private IDbCommand GetPreparedDbCommand()
        {
            Data.Command.CommandType(DbCommandTypes.StoredProcedure);
            Data.Command.ClearSql.Sql(Data.Command.Data.Context.Data.FluentDataProvider.GetSqlForStoredProcedureBuilder(Data));
            return Data.Command;
        }

        public void Dispose()
        {
            Data.Command.Dispose();
        }

        public TParameterType ParameterValue<TParameterType>(string outputParameterName)
        {
            return Data.Command.ParameterValue<TParameterType>(outputParameterName);
        }

        public int Execute()
        {
            return GetPreparedDbCommand().Execute();
        }

        public List<TEntity> QueryMany<TEntity>(Action<TEntity, IDataReader> customMapper = null)
        {
            return GetPreparedDbCommand().QueryMany<TEntity>(customMapper);
        }

        public List<TEntity> QueryMany<TEntity>(Action<TEntity, dynamic> customMapper)
        {
            return GetPreparedDbCommand().QueryMany<TEntity>(customMapper);
        }

        public TList QueryMany<TEntity, TList>(Action<TEntity, IDataReader> customMapper = null) where TList : IList<TEntity>
        {
            return GetPreparedDbCommand().QueryMany<TEntity, TList>(customMapper);
        }

        public TList QueryMany<TEntity, TList>(Action<TEntity, dynamic> customMapper) where TList : IList<TEntity>
        {
            return GetPreparedDbCommand().QueryMany<TEntity, TList>(customMapper);
        }

        public void QueryComplexMany<TEntity>(IList<TEntity> list, Action<IList<TEntity>, IDataReader> customMapper)
        {
            GetPreparedDbCommand().QueryComplexMany<TEntity>(list, customMapper);
        }

        public void QueryComplexMany<TEntity>(IList<TEntity> list, Action<IList<TEntity>, dynamic> customMapper)
        {
            GetPreparedDbCommand().QueryComplexMany<TEntity>(list, customMapper);
        }

        public TEntity QuerySingle<TEntity>(Action<TEntity, IDataReader> customMapper = null)
        {
            return GetPreparedDbCommand().QuerySingle<TEntity>(customMapper);
        }

        public TEntity QuerySingle<TEntity>(Action<TEntity, dynamic> customMapper)
        {
            return GetPreparedDbCommand().QuerySingle<TEntity>(customMapper);
        }

        public TEntity QueryComplexSingle<TEntity>(Func<IDataReader, TEntity> customMapper)
        {
            return GetPreparedDbCommand().QueryComplexSingle(customMapper);
        }

        public TEntity QueryComplexSingle<TEntity>(Func<dynamic, TEntity> customMapper)
        {
            return GetPreparedDbCommand().QueryComplexSingle(customMapper);
        }
    }

    public interface IStoredProcedureBuilder : IExecute, IQuery, IParameterValue, IDisposable
    {
        BuilderData Data { get; }
        IStoredProcedureBuilder Parameter(string name, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IStoredProcedureBuilder ParameterOut(string name, DataTypes parameterType, int size = 0);
        IStoredProcedureBuilder UseMultiResult(bool useMultipleResultsets);
    }

    public interface IStoredProcedureBuilderDynamic : IExecute, IQuery, IParameterValue, IDisposable
    {
        BuilderData Data { get; }
        IStoredProcedureBuilderDynamic AutoMap(params string[] ignoreProperties);
        IStoredProcedureBuilderDynamic Parameter(string name, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IStoredProcedureBuilderDynamic ParameterOut(string name, DataTypes parameterType, int size = 0);
        IStoredProcedureBuilderDynamic UseMultiResult(bool useMultipleResultsets);
    }

    public interface IStoredProcedureBuilder<T> : IExecute, IQuery, IParameterValue, IDisposable
    {
        BuilderData Data { get; }
        IStoredProcedureBuilder<T> AutoMap(params Expression<Func<T, object>>[] ignoreProperties);
        IStoredProcedureBuilder<T> Parameter(Expression<Func<T, object>> expression, DataTypes parameterType = DataTypes.Object, int size = 0);
        IStoredProcedureBuilder<T> Parameter(string name, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IStoredProcedureBuilder<T> ParameterOut(string name, DataTypes parameterType, int size = 0);
        IStoredProcedureBuilder<T> UseMultiResult(bool useMultipleResultsets);
    }

    internal class StoredProcedureBuilder : BaseStoredProcedureBuilder, IStoredProcedureBuilder
    {
        internal StoredProcedureBuilder(IDbCommand command, string name)
            : base(command, name)
        {
        }

        public IStoredProcedureBuilder Parameter(string name, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(name, value, parameterType, size);
            return this;
        }

        public IStoredProcedureBuilder ParameterOut(string name, DataTypes parameterType, int size = 0)
        {
            Actions.ParameterOutputAction(name, parameterType, size);
            return this;
        }

        public IStoredProcedureBuilder UseMultiResult(bool useMultipleResultsets)
        {
            Data.Command.UseMultiResult(useMultipleResultsets);
            return this;
        }
    }

    internal class StoredProcedureBuilderDynamic : BaseStoredProcedureBuilder, IStoredProcedureBuilderDynamic
    {
        internal StoredProcedureBuilderDynamic(IDbCommand command, string name, ExpandoObject item)
            : base(command, name)
        {
            Data.Item = (IDictionary<string, object>)item;
        }

        public IStoredProcedureBuilderDynamic Parameter(string name, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(name, value, parameterType, size);
            return this;
        }

        public IStoredProcedureBuilderDynamic AutoMap(params string[] ignoreProperties)
        {
            Actions.AutoMapDynamicTypeColumnsAction(ignoreProperties);
            return this;
        }

        public IStoredProcedureBuilderDynamic ParameterOut(string name, DataTypes parameterType, int size = 0)
        {
            Actions.ParameterOutputAction(name, parameterType, size);
            return this;
        }

        public IStoredProcedureBuilderDynamic UseMultiResult(bool useMultipleResultsets)
        {
            Data.Command.UseMultiResult(useMultipleResultsets);
            return this;
        }
    }

    internal class StoredProcedureBuilder<T> : BaseStoredProcedureBuilder, IStoredProcedureBuilder<T>
    {
        internal StoredProcedureBuilder(IDbCommand command, string name, T item)
            : base(command, name)
        {
            Data.Item = item;
        }

        public IStoredProcedureBuilder<T> Parameter(string name, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(name, value, parameterType, size);
            return this;
        }

        public IStoredProcedureBuilder<T> AutoMap(params Expression<Func<T, object>>[] ignoreProperties)
        {
            Actions.AutoMapColumnsAction(ignoreProperties);
            return this;
        }

        public IStoredProcedureBuilder<T> Parameter(Expression<Func<T, object>> expression, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(expression, parameterType, size);

            return this;
        }

        public IStoredProcedureBuilder<T> ParameterOut(string name, DataTypes parameterType, int size = 0)
        {
            Actions.ParameterOutputAction(name, parameterType, size);
            return this;
        }

        public IStoredProcedureBuilder<T> UseMultiResult(bool useMultipleResultsets)
        {
            Data.Command.UseMultiResult(useMultipleResultsets);
            return this;
        }
    }

    public class BuilderColumn
    {
        public string ColumnName { get; set; }
        public string ParameterName { get; set; }
        public object Value { get; set; }

        public BuilderColumn(string columnName, object value, string parameterName)
        {
            ColumnName = columnName;
            Value = value;
            ParameterName = parameterName;
        }
    }

    internal abstract class BaseUpdateBuilder
    {
        public BuilderData Data { get; set; }
        protected ActionsHandler Actions { get; set; }

        public BaseUpdateBuilder(IDbProvider provider, IDbCommand command, string name)
        {
            Data = new BuilderData(command, name);
            Actions = new ActionsHandler(Data);
        }

        public int Execute()
        {
            if (Data.Columns.Count == 0
                       || Data.Where.Count == 0)
                throw new FluentDataException("Columns or where filter have not yet been added.");

            Data.Command.ClearSql.Sql(Data.Command.Data.Context.Data.FluentDataProvider.GetSqlForUpdateBuilder(Data));

            return Data.Command.Execute();
        }
    }

    public interface IUpdateBuilder : IExecute
    {
        BuilderData Data { get; }
        IUpdateBuilder Column(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IUpdateBuilder Where(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IUpdateBuilder Fill(Action<IInsertUpdateBuilder> fillMethod);
    }

    public interface IUpdateBuilderDynamic : IExecute
    {
        BuilderData Data { get; }
        dynamic Item { get; }
        IUpdateBuilderDynamic AutoMap(params string[] ignoreProperties);
        IUpdateBuilderDynamic Column(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IUpdateBuilderDynamic Column(string propertyName, DataTypes parameterType = DataTypes.Object, int size = 0);
        IUpdateBuilderDynamic Where(string name, DataTypes parameterType = DataTypes.Object, int size = 0);
        IUpdateBuilderDynamic Where(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IUpdateBuilderDynamic Fill(Action<IInsertUpdateBuilderDynamic> fillMethod);
    }

    public interface IUpdateBuilder<T> : IExecute
    {
        BuilderData Data { get; }
        T Item { get; }
        IUpdateBuilder<T> AutoMap(params Expression<Func<T, object>>[] ignoreProperties);
        IUpdateBuilder<T> Where(Expression<Func<T, object>> expression, DataTypes parameterType = DataTypes.Object, int size = 0);
        IUpdateBuilder<T> Where(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IUpdateBuilder<T> Column(string columnName, object value, DataTypes parameterType = DataTypes.Object, int size = 0);
        IUpdateBuilder<T> Column(Expression<Func<T, object>> expression, DataTypes parameterType = DataTypes.Object, int size = 0);
        IUpdateBuilder<T> Fill(Action<IInsertUpdateBuilder<T>> fillMethod);
    }

    internal class UpdateBuilder : BaseUpdateBuilder, IUpdateBuilder, IInsertUpdateBuilder
    {
        internal UpdateBuilder(IDbProvider dbProvider, IDbCommand command, string name)
            : base(dbProvider, command, name)
        {
        }

        public virtual IUpdateBuilder Where(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.WhereAction(columnName, value, parameterType, size);
            return this;
        }
        
        public IUpdateBuilder Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        IInsertUpdateBuilder IInsertUpdateBuilder.Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        public IUpdateBuilder Fill(Action<IInsertUpdateBuilder> fillMethod)
        {
            fillMethod(this);
            return this;
        }
    }

    internal class UpdateBuilderDynamic : BaseUpdateBuilder, IUpdateBuilderDynamic, IInsertUpdateBuilderDynamic
    {
        public dynamic Item { get; private set; }

        internal UpdateBuilderDynamic(IDbProvider dbProvider, IDbCommand command, string name, ExpandoObject item)
            : base(dbProvider, command, name)
        {
            Data.Item = item;
            Item = item;
        }

        public virtual IUpdateBuilderDynamic Where(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.WhereAction(columnName, value, parameterType, size);
            return this;
        }

        public IUpdateBuilderDynamic Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        public IUpdateBuilderDynamic Column(string propertyName, DataTypes parameterType, int size)
        {
            Actions.ColumnValueDynamic((ExpandoObject)Data.Item, propertyName, parameterType, size);
            return this;
        }

        public IUpdateBuilderDynamic Where(string name, DataTypes parameterType, int size)
        {
            var propertyValue = ReflectionHelper.GetPropertyValueDynamic(Data.Item, name);
            Where(name, propertyValue, parameterType, size);
            return this;
        }

        public IUpdateBuilderDynamic AutoMap(params string[] ignoreProperties)
        {
            Actions.AutoMapDynamicTypeColumnsAction(ignoreProperties);
            return this;
        }

        IInsertUpdateBuilderDynamic IInsertUpdateBuilderDynamic.Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        IInsertUpdateBuilderDynamic IInsertUpdateBuilderDynamic.Column(string propertyName, DataTypes parameterType, int size)
        {
            Actions.ColumnValueDynamic((ExpandoObject)Data.Item, propertyName, parameterType, size);
            return this;
        }

        public IUpdateBuilderDynamic Fill(Action<IInsertUpdateBuilderDynamic> fillMethod)
        {
            fillMethod(this);
            return this;
        }
    }

    internal class UpdateBuilder<T> : BaseUpdateBuilder, IUpdateBuilder<T>, IInsertUpdateBuilder<T>
    {
        public T Item { get; private set; }

        internal UpdateBuilder(IDbProvider provider, IDbCommand command, string name, T item)
            : base(provider, command, name)
        {
            Data.Item = item;
            Item = item;
        }

        public IUpdateBuilder<T> Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        public IUpdateBuilder<T> AutoMap(params Expression<Func<T, object>>[] ignoreProperties)
        {
            Actions.AutoMapColumnsAction(ignoreProperties);
            return this;
        }

        public IUpdateBuilder<T> Column(Expression<Func<T, object>> expression, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(expression, parameterType, size);
            return this;
        }

        public virtual IUpdateBuilder<T> Where(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.WhereAction(columnName, value, parameterType, size);
            return this;
        }

        public IUpdateBuilder<T> Where(Expression<Func<T, object>> expression, DataTypes parameterType, int size)
        {
            Actions.WhereAction(expression, parameterType, size);
            return this;
        }

        IInsertUpdateBuilder<T> IInsertUpdateBuilder<T>.Column(string columnName, object value, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(columnName, value, parameterType, size);
            return this;
        }

        IInsertUpdateBuilder<T> IInsertUpdateBuilder<T>.Column(Expression<Func<T, object>> expression, DataTypes parameterType, int size)
        {
            Actions.ColumnValueAction(expression, parameterType, size);
            return this;
        }

        public IUpdateBuilder<T> Fill(Action<IInsertUpdateBuilder<T>> fillMethod)
        {
            fillMethod(this);
            return this;
        }
    }

    public enum DataTypes
    {
        // Summary:
        //     A variable-length stream of non-Unicode characters ranging between 1 and
        //     8,000 characters.
        AnsiString = 0,
        //
        // Summary:
        //     A variable-length stream of binary data ranging between 1 and 8,000 bytes.
        Binary = 1,
        //
        // Summary:
        //     An 8-bit unsigned integer ranging in value from 0 to 255.
        Byte = 2,
        //
        // Summary:
        //     A simple type representing Boolean values of true or false.
        Boolean = 3,
        //
        // Summary:
        //     A currency value ranging from -2 63 (or -922,337,203,685,477.5808) to 2 63
        //     -1 (or +922,337,203,685,477.5807) with an accuracy to a ten-thousandth of
        //     a currency unit.
        Currency = 4,
        //
        // Summary:
        //     A type representing a date value.
        Date = 5,
        //
        // Summary:
        //     A type representing a date and time value.
        DateTime = 6,
        //
        // Summary:
        //     A simple type representing values ranging from 1.0 x 10 -28 to approximately
        //     7.9 x 10 28 with 28-29 significant digits.
        Decimal = 7,
        //
        // Summary:
        //     A floating point type representing values ranging from approximately 5.0
        //     x 10 -324 to 1.7 x 10 308 with a precision of 15-16 digits.
        Double = 8,
        //
        // Summary:
        //     A globally unique identifier (or GUID).
        Guid = 9,
        //
        // Summary:
        //     An integral type representing signed 16-bit integers with values between
        //     -32768 and 32767.
        Int16 = 10,
        //
        // Summary:
        //     An integral type representing signed 32-bit integers with values between
        //     -2147483648 and 2147483647.
        Int32 = 11,
        //
        // Summary:
        //     An integral type representing signed 64-bit integers with values between
        //     -9223372036854775808 and 9223372036854775807.
        Int64 = 12,
        //
        // Summary:
        //     A general type representing any reference or value type not explicitly represented
        //     by another DataTypes value.
        Object = 13,
        //
        // Summary:
        //     An integral type representing signed 8-bit integers with values between -128
        //     and 127.
        SByte = 14,
        //
        // Summary:
        //     A floating point type representing values ranging from approximately 1.5
        //     x 10 -45 to 3.4 x 10 38 with a precision of 7 digits.
        Single = 15,
        //
        // Summary:
        //     A type representing Unicode character strings.
        String = 16,
        //
        // Summary:
        //     A type representing a SQL Server DateTime value. If you want to use a SQL
        //     Server time value, use System.Data.SqlDbType.Time.
        Time = 17,
        //
        // Summary:
        //     An integral type representing unsigned 16-bit integers with values between
        //     0 and 65535.
        UInt16 = 18,
        //
        // Summary:
        //     An integral type representing unsigned 32-bit integers with values between
        //     0 and 4294967295.
        UInt32 = 19,
        //
        // Summary:
        //     An integral type representing unsigned 64-bit integers with values between
        //     0 and 18446744073709551615.
        UInt64 = 20,
        //
        // Summary:
        //     A variable-length numeric value.
        VarNumeric = 21,
        //
        // Summary:
        //     A fixed-length stream of non-Unicode characters.
        AnsiStringFixedLength = 22,
        //
        // Summary:
        //     A fixed-length string of Unicode characters.
        StringFixedLength = 23,
        //
        // Summary:
        //     A parsed representation of an XML document or fragment.
        Xml = 25,
        //
        // Summary:
        //     Date and time data. Date value range is from January 1,1 AD through December
        //     31, 9999 AD. Time value range is 00:00:00 through 23:59:59.9999999 with an
        //     accuracy of 100 nanoseconds.
        DateTime2 = 26,
        //
        // Summary:
        //     Date and time data with time zone awareness. Date value range is from January
        //     1,1 AD through December 31, 9999 AD. Time value range is 00:00:00 through
        //     23:59:59.9999999 with an accuracy of 100 nanoseconds. Time zone value range
        //     is -14:00 through +14:00.
        DateTimeOffset = 27,
    }

    internal partial class DbCommand : IDbCommand
    {
        public DbCommandData Data { get; private set; }

        public DbCommand(
            DbContext dbContext,
            System.Data.IDbCommand innerCommand)
        {
            Data = new DbCommandData(dbContext, innerCommand);
            Data.ExecuteQueryHandler = new ExecuteQueryHandler(this);
        }

        public IDbCommand UseMultiResult(bool useMultipleResultset)
        {
            if (useMultipleResultset && !Data.Context.Data.FluentDataProvider.SupportsMultipleResultsets)
                throw new FluentDataException("The selected database does not support multiple resultset");

            Data.UseMultipleResultsets = useMultipleResultset;
            return this;
        }

        public IDbCommand CommandType(DbCommandTypes dbCommandType)
        {
            Data.InnerCommand.CommandType = (System.Data.CommandType)dbCommandType;
            return this;
        }

        internal void ClosePrivateConnection()
        {
            if (!Data.Context.Data.UseTransaction
                && !Data.Context.Data.UseSharedConnection)
            {
                Data.InnerCommand.Connection.Close();

                if (Data.Context.Data.OnConnectionClosed != null)
                    Data.Context.Data.OnConnectionClosed(new ConnectionEventArgs(Data.InnerCommand.Connection));
            }
        }

        public void Dispose()
        {
            if (Data.Reader != null)
                Data.Reader.Close();

            ClosePrivateConnection();
        }
    }

    public class DbCommandData
    {
        public DbContext Context { get; private set; }
        public System.Data.IDbCommand InnerCommand { get; private set; }
        public bool UseMultipleResultsets { get; set; }
        public IDataReader Reader { get; set; }
        internal ExecuteQueryHandler ExecuteQueryHandler;
        public StringBuilder Sql { get; private set; }

        public DbCommandData(DbContext context, System.Data.IDbCommand innerCommand)
        {
            Context = context;
            InnerCommand = innerCommand;
            InnerCommand.CommandType = (System.Data.CommandType)DbCommandTypes.Text;
            Sql = new StringBuilder();
        }
    }

    public enum DbCommandTypes
    {
        // Summary:
        //     An SQL text command. (Default.)
        Text = 1,
        //
        // Summary:
        //     The name of a stored procedure.
        StoredProcedure = 4,
        //
        // Summary:
        //     The name of a table.
        TableDirect = 512,
    }

    internal class QueryDataTableHandler<TEntity> : IQueryTypeHandler<TEntity>
    {
        private readonly DbCommandData _data;

        public bool IterateDataReader { get { return false; } }

        public QueryDataTableHandler(DbCommandData data)
        {
            _data = data;
        }

        public object HandleType(Action<TEntity, IDataReader> customMapperReader, Action<TEntity, dynamic> customMapperDynamic)
        {
            var dataTable = new DataTable();
            dataTable.Load(_data.Reader.InnerReader, LoadOption.OverwriteChanges);

            return dataTable;
        }
    }

    internal interface IQueryTypeHandler<TEntity>
    {
        bool IterateDataReader { get; }
        object HandleType(Action<TEntity, IDataReader> customMapperReader, Action<TEntity, dynamic> customMapperDynamic);
    }

    internal class QueryCustomEntityHandler<TEntity> : IQueryTypeHandler<TEntity>
    {
        private readonly AutoMapper _autoMapper;
        private readonly DbCommandData _data;

        public QueryCustomEntityHandler(DbCommandData data)
        {
            _data = data;
            _autoMapper = new AutoMapper(_data, typeof(TEntity));
        }

        public bool IterateDataReader { get { return true; } }

        public object HandleType(Action<TEntity, IDataReader> customMapperReader, Action<TEntity, dynamic> customMapperDynamic)
        {
            var item = (TEntity)_data.Context.Data.EntityFactory.Create(typeof(TEntity));

            if (customMapperReader != null)
                customMapperReader(item, _data.Reader);
            else if (customMapperDynamic != null)
                customMapperDynamic(item, new DynamicDataReader(_data.Reader.InnerReader));
            else
                _autoMapper.AutoMap(item);
            return item;
        }
    }

    internal class QueryDynamicHandler<TEntity> : IQueryTypeHandler<TEntity>
    {
        private readonly DbCommandData _data;
        private readonly DynamicTypeAutoMapper _autoMapper;

        public bool IterateDataReader { get { return true; } }

        public QueryDynamicHandler(DbCommandData data)
        {
            _data = data;
            _autoMapper = new DynamicTypeAutoMapper(_data.Reader.InnerReader);
        }

        public object HandleType(Action<TEntity, IDataReader> customMapperReader, Action<TEntity, dynamic> customMapperDynamic)
        {
            var item = _autoMapper.AutoMap();
            return item;
        }
    }

    internal class QueryScalarHandler<TEntity> : IQueryTypeHandler<TEntity>
    {
        private readonly DbCommandData _data;
        private Type _fieldType;

        public bool IterateDataReader { get { return true; } }

        public QueryScalarHandler(DbCommandData data)
        {
            _data = data;
        }

        public object HandleType(Action<TEntity, IDataReader> customMapperReader, Action<TEntity, dynamic> customMapperDynamic)
        {
            var value = _data.Reader.GetValue(0);
            if (_fieldType == null)
                _fieldType = _data.Reader.GetFieldType(0);

            if (value == null)
                value = default(TEntity);
            else if (_fieldType != typeof(TEntity))
                value = (Convert.ChangeType(value, typeof(TEntity)));
            return (TEntity)value;
        }
    }

    public interface IDbCommand : IExecute, IExecuteReturnLastId, IQuery, IParameterValue, IDisposable
    {
        DbCommandData Data { get; }
        IDbCommand ParameterOut(string name, DataTypes parameterType, int size = 0);
        IDbCommand Parameter(string name, object value, DataTypes parameterType = DataTypes.Object, ParameterDirection direction = ParameterDirection.Input, int size = 0);
        IDbCommand Parameters(params object[] parameters);
        IDbCommand Sql(string sql);
        IDbCommand ClearSql { get; }
        IDbCommand CommandType(DbCommandTypes dbCommandType);
        IDbCommand UseMultiResult(bool useMultipleResultsets);
    }

    public interface IExecute
    {
        int Execute();
    }

    public interface IExecuteReturnLastId
    {
        T ExecuteReturnLastId<T>(string identityColumnName = null);
    }

    public interface IParameterValue
    {
        TParameterType ParameterValue<TParameterType>(string outputParameterName);
    }

    public interface IQuery
    {
        List<TEntity> QueryMany<TEntity>(Action<TEntity, IDataReader> customMapper = null);
        List<TEntity> QueryMany<TEntity>(Action<TEntity, dynamic> customMapper);
        TList QueryMany<TEntity, TList>(Action<TEntity, IDataReader> customMapper = null) where TList : IList<TEntity>;
        TList QueryMany<TEntity, TList>(Action<TEntity, dynamic> customMapper) where TList : IList<TEntity>;
        void QueryComplexMany<TEntity>(IList<TEntity> list, Action<IList<TEntity>, IDataReader> customMapper);
        void QueryComplexMany<TEntity>(IList<TEntity> list, Action<IList<TEntity>, dynamic> customMapper);
        TEntity QuerySingle<TEntity>(Action<TEntity, IDataReader> customMapper = null);
        TEntity QuerySingle<TEntity>(Action<TEntity, dynamic> customMapper);
        TEntity QueryComplexSingle<TEntity>(Func<IDataReader, TEntity> customMapper);
        TEntity QueryComplexSingle<TEntity>(Func<dynamic, TEntity> customMapper);
    }

    internal class AutoMapper
    {
        private readonly DbCommandData _dbCommandData;
        private readonly Dictionary<string, PropertyInfo> _properties;
        private readonly List<DataReaderField> _fields;
        private readonly System.Data.IDataReader _reader;

        internal AutoMapper(DbCommandData dbCommandData, Type itemType)
        {
            _dbCommandData = dbCommandData;
            _reader = dbCommandData.Reader.InnerReader;
            _properties = ReflectionHelper.GetProperties(itemType);
            _fields = DataReaderHelper.GetDataReaderFields(_reader);
        }

        public void AutoMap(object item)
        {
            foreach (var field in _fields)
            {
                if (field.IsSystem)
                    continue;

                var value = _reader.GetValue(field.Index);
                var wasMapped = false;

                PropertyInfo property = null;

                if (_properties.TryGetValue(field.LowerName, out property))
                {
                    SetPropertyValue(field, property, item, value);
                    wasMapped = true;
                }
                else
                {
                    if (field.LowerName.IndexOf('_') != -1)
                        wasMapped = HandleComplexField(item, field, value);
                }

                if (!wasMapped && !_dbCommandData.Context.Data.IgnoreIfAutoMapFails)
                    throw new FluentDataException("Could not map: " + field.Name);
            }
        }

        private bool HandleComplexField(object item, DataReaderField field, object value)
        {
            string propertyName = null;

            for (var level = 0; level <= field.NestedLevels; level++)
            {
                if (string.IsNullOrEmpty(propertyName))
                    propertyName = field.GetNestedName(level);
                else
                    propertyName += "_" + field.GetNestedName(level);

                PropertyInfo property = null;
                var properties = ReflectionHelper.GetProperties(item.GetType());
                if (properties.TryGetValue(propertyName, out property))
                {
                    if (level == field.NestedLevels)
                    {
                        SetPropertyValue(field, property, item, value);
                        return true;
                    }
                    else
                    {
                        item = GetOrCreateInstance(item, property);
                        if (item == null)
                            return false;
                        propertyName = null;
                    }
                }
            }

            return false;
        }

        private object GetOrCreateInstance(object item, PropertyInfo property)
        {
            object instance = ReflectionHelper.GetPropertyValue(item, property);

            if (instance == null)
            {
                instance = _dbCommandData.Context.Data.EntityFactory.Create(property.PropertyType);

                property.SetValue(item, instance, null);
            }

            return instance;
        }

        private void SetPropertyValue(DataReaderField field, PropertyInfo property, object item, object value)
        {
            try
            {
                if (value == DBNull.Value)
                {
                    if (ReflectionHelper.IsNullable(property))
                        value = null;
                    else
                        value = ReflectionHelper.GetDefault(property.PropertyType);
                }
                else
                {
                    var propertyType = ReflectionHelper.GetPropertyType(property);

                    if (propertyType != field.Type)
                    {
                        if (propertyType.IsEnum)
                        {
                            if (field.Type == typeof(string))
                                value = Enum.Parse(propertyType, (string)value, true);
                            else
                                value = Enum.ToObject(propertyType, value);
                        }
                        else if (!ReflectionHelper.IsBasicClrType(propertyType))
                            return;
                        else if (propertyType == typeof(string))
                            value = value.ToString();
                        else
                            value = Convert.ChangeType(value, property.PropertyType);
                    }
                }

                property.SetValue(item, value, null);
            }
            catch (Exception exception)
            {
                throw new FluentDataException("Could not map: " + property.Name, exception);
            }
        }
    }

    internal class DataReaderField
    {
        public int Index { get; private set; }
        public string LowerName { get; private set; }
        public string Name { get; private set; }
        public Type Type { get; private set; }
        private readonly string[] _nestedPropertyNames;
        private readonly int _nestedLevels;

        public DataReaderField(int index, string name, Type type)
        {
            Index = index;
            Name = name;
            LowerName = name.ToLower();
            Type = type;
            _nestedPropertyNames = LowerName.Split('_');
            _nestedLevels = _nestedPropertyNames.Count() - 1;
        }

        public string GetNestedName(int level)
        {
            return _nestedPropertyNames[level];
        }

        public int NestedLevels
        {
            get
            {
                return _nestedLevels;
            }
        }

        public bool IsSystem
        {
            get
            {
                return Name.IndexOf("FLUENTDATA_") > -1;
            }
        }
    }

    internal class DynamicTypeAutoMapper
    {
        private readonly List<DataReaderField> _fields;
        private readonly System.Data.IDataReader _reader;

        public DynamicTypeAutoMapper(System.Data.IDataReader reader)
        {
            _reader = reader;
            _fields = DataReaderHelper.GetDataReaderFields(_reader);
        }

        public ExpandoObject AutoMap()
        {
            var item = new ExpandoObject();

            var itemDictionary = (IDictionary<string, object>)item;

            foreach (var column in _fields)
            {
                if (_reader.IsDBNull(column.Index))
                    itemDictionary.Add(column.Name, null);
                else
                    itemDictionary.Add(column.Name, _reader[column.Index]);
            }

            return item;
        }
    }

    public enum ParameterDirection
    {
        // The parameter is an input parameter.
        Input = 1,

        // The parameter is an output parameter.
        Output = 2,

        // The parameter is capable of both input and output.
        InputOutput = 3,

        // The parameter represents a return value from an operation such as a stored
        // procedure, built-in function, or user-defined function.
        ReturnValue = 6,
    }

    internal partial class DbCommand
    {
        public TEntity QueryComplexSingle<TEntity>(Func<IDataReader, TEntity> customMapper)
        {
            var item = default(TEntity);

            Data.ExecuteQueryHandler.ExecuteQuery(true, () =>
            {
                if (Data.Reader.Read())
                    item = customMapper(Data.Reader);
            });

            return item;
        }

        public TEntity QueryComplexSingle<TEntity>(Func<dynamic, TEntity> customMapper)
        {
            var item = default(TEntity);

            Data.ExecuteQueryHandler.ExecuteQuery(true, () =>
            {
                if (Data.Reader.Read())
                    item = customMapper(new DynamicDataReader(Data.Reader));
            });

            return item;
        }
    }

    internal partial class DbCommand
    {
        public T ExecuteReturnLastId<T>(string identityColumnName = null)
        {
            if (Data.Context.Data.FluentDataProvider.RequiresIdentityColumn && string.IsNullOrEmpty(identityColumnName))
                throw new FluentDataException("The identity column must be given");

            var value = Data.Context.Data.FluentDataProvider.ExecuteReturnLastId<T>(this, identityColumnName);
            T lastId;

            if (value!=null && value.GetType() == typeof(T))
                lastId = (T)value;
            else
                lastId = (T)Convert.ChangeType(value, typeof(T));

            return lastId;
        }
    }

    internal partial class DbCommand
    {
        public TEntity QuerySingle<TEntity>(Action<TEntity, IDataReader> customMapper)
        {
            var item = default(TEntity);

            Data.ExecuteQueryHandler.ExecuteQuery(true, () =>
            {
                item = new QueryHandler<TEntity>(Data).ExecuteSingle(customMapper, null);
            });

            return item;
        }

        public TEntity QuerySingle<TEntity>(Action<TEntity, dynamic> customMapper)
        {
            var item = default(TEntity);

            Data.ExecuteQueryHandler.ExecuteQuery(true, () =>
            {
                item = new QueryHandler<TEntity>(Data).ExecuteSingle(customMapper, null);
            });

            return item;
        }
    }

    internal partial class DbCommand
    {
        public IDbCommand Sql(string sql)
        {
            Data.Sql.Append(sql);
            return this;
        }

        public IDbCommand ClearSql
        {
            get
            {
                Data.Sql.Clear();
                return this;
            }
        }
    }

    internal partial class DbCommand
    {
        public int Execute()
        {
            var recordsAffected = 0;

            Data.ExecuteQueryHandler.ExecuteQuery(false, () =>
            {
                recordsAffected = Data.InnerCommand.ExecuteNonQuery();
            });
            return recordsAffected;
        }
    }

    internal partial class DbCommand
    {
        public IDbCommand Parameter(string name, object value, DataTypes parameterType = DataTypes.Object, ParameterDirection direction = ParameterDirection.Input, int size = 0)
        {
            if (parameterType != DataTypes.Binary
                && !(value is byte[])
                && ReflectionHelper.IsList(value))
                AddListParameterToInnerCommand(name, value);
            else
                AddParameterToInnerCommand(name, value, parameterType, direction, size);

            return this;
        }

        private int _currentIndex = 0;
        public IDbCommand Parameters(params object[] parameters)
        {
            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count(); i++)
                {
                    Parameter(_currentIndex.ToString(), parameters[_currentIndex]);
                    _currentIndex++;
                }
            }
            return this;
        }

        private void AddListParameterToInnerCommand(string name, object value)
        {
            var list = (IEnumerable)value;

            var newInStatement = new StringBuilder();

            var k = -1;
            foreach (var item in list)
            {
                k++;
                if (k == 0)
                    newInStatement.Append(" in(");
                else
                    newInStatement.Append(",");

                var parameter = AddParameterToInnerCommand("p" + name + "p" + k.ToString(), item);

                newInStatement.Append(parameter.ParameterName);
            }
            newInStatement.Append(")");

            var oldInStatement = string.Format(" in({0})", Data.Context.Data.FluentDataProvider.GetParameterName(name));
            Data.Sql.Replace(oldInStatement, newInStatement.ToString());
        }

        private IDbDataParameter AddParameterToInnerCommand(string name, object value, DataTypes parameterType = DataTypes.Object, ParameterDirection direction = ParameterDirection.Input, int size = 0)
        {
            if (value == null)
                value = DBNull.Value;

            if (value.GetType().IsEnum)
                value = (int)value;

            var dbParameter = Data.InnerCommand.CreateParameter();
            if (parameterType == DataTypes.Object)
                dbParameter.DbType = (System.Data.DbType)Data.Context.Data.FluentDataProvider.GetDbTypeForClrType(value.GetType());
            else
                dbParameter.DbType = (System.Data.DbType)parameterType;

            dbParameter.ParameterName = Data.Context.Data.FluentDataProvider.GetParameterName(name);
            dbParameter.Direction = (System.Data.ParameterDirection)direction;
            dbParameter.Value = value;
            if (size > 0)
                dbParameter.Size = size;
            Data.InnerCommand.Parameters.Add(dbParameter);

            return dbParameter;
        }

        public IDbCommand ParameterOut(string name, DataTypes parameterType, int size)
        {
            if (!Data.Context.Data.FluentDataProvider.SupportsOutputParameters)
                throw new FluentDataException("The selected database does not support output parameters");
            Parameter(name, null, parameterType, ParameterDirection.Output, size);
            return this;
        }

        public TParameterType ParameterValue<TParameterType>(string outputParameterName)
        {
            outputParameterName = Data.Context.Data.FluentDataProvider.GetParameterName(outputParameterName);
            if (!Data.InnerCommand.Parameters.Contains(outputParameterName))
                throw new FluentDataException(string.Format("Parameter {0} not found", outputParameterName));
             System.Data.IDataParameter dataParameter = Data.InnerCommand.Parameters[outputParameterName] as System.Data.IDataParameter;
            if (dataParameter == null)
                throw new FluentDataException(string.Format("Parameter {0} not found", outputParameterName));
            else {
                object value = null;
                System.Data.IDbCommand dbCommand = Data.InnerCommand;

                if (Data != null && dbCommand != null && Data.InnerCommand.Parameters != null)
                {                    
                    System.Data.IDataParameter parameter = dbCommand.Parameters[outputParameterName] as System.Data.IDataParameter;
                    value = parameter.Value;
                }

                if (value == DBNull.Value)
                    return default(TParameterType);

                return (TParameterType)value;
            }
        }
    }

    internal partial class DbCommand
    {
        public TList QueryMany<TEntity, TList>(Action<TEntity, IDataReader> customMapper = null)
            where TList : IList<TEntity>
        {
            var items = default(TList);

            Data.ExecuteQueryHandler.ExecuteQuery(true, () =>
            {
                items = new QueryHandler<TEntity>(Data).ExecuteMany<TList>(customMapper, null);
            });

            return items;
        }

        public TList QueryMany<TEntity, TList>(Action<TEntity, dynamic> customMapper) where TList : IList<TEntity>
        {
            var items = default(TList);

            Data.ExecuteQueryHandler.ExecuteQuery(true, () =>
            {
                items = new QueryHandler<TEntity>(Data).ExecuteMany<TList>(null, customMapper);
            });

            return items;
        }

        public List<TEntity> QueryMany<TEntity>(Action<TEntity, IDataReader> customMapper)
        {
            return QueryMany<TEntity, List<TEntity>>(customMapper);
        }

        public List<TEntity> QueryMany<TEntity>(Action<TEntity, dynamic> customMapper)
        {
            return QueryMany<TEntity, List<TEntity>>(customMapper);
        }

        public DataTable QueryManyDataTable()
        {
            var dataTable = new DataTable();

            Data.ExecuteQueryHandler.ExecuteQuery(true, () => dataTable.Load(Data.Reader.InnerReader, LoadOption.OverwriteChanges));

            return dataTable;
        }
    }

    internal partial class DbCommand
    {
        public void QueryComplexMany<TEntity>(IList<TEntity> list, Action<IList<TEntity>, IDataReader> customMapper)
        {
            Data.ExecuteQueryHandler.ExecuteQuery(true, () =>
            {
                while (Data.Reader.Read())
                    customMapper(list, Data.Reader);
            });
        }

        public void QueryComplexMany<TEntity>(IList<TEntity> list, Action<IList<TEntity>, dynamic> customMapper)
        {
            Data.ExecuteQueryHandler.ExecuteQuery(true, () =>
            {
                while (Data.Reader.Read())
                    customMapper(list, Data.Reader);
            });
        }
    }

    internal class ExecuteQueryHandler
    {
        private readonly DbCommand _command;
        private bool _queryAlreadyExecuted;

        public ExecuteQueryHandler(DbCommand command)
        {
            _command = command;
        }

        internal void ExecuteQuery(bool useReader, Action action)
        {
            try
            {
                PrepareDbCommand(useReader);
                action();

                if (_command.Data.Context.Data.OnExecuted != null)
                    _command.Data.Context.Data.OnExecuted(new CommandEventArgs(_command.Data.InnerCommand));
            }
            catch (FluentDataException fdEx) {
                HandleQueryException(fdEx);                
            }
            catch (Exception exception)
            {                
                HandleQueryException(exception);
                LogUtil.HWLogger.API.Error(exception);
            }
            finally
            {
                HandleQueryFinally();
            }
        }

        private void PrepareDbCommand(bool useReader)
        {
            if (_queryAlreadyExecuted)
            {
                if (_command.Data.UseMultipleResultsets)
                    _command.Data.Reader.NextResult();
                else
                    throw new FluentDataException("A query has already been executed on this command object. Please create a new command object.");
            }
            else
            {
                _command.Data.InnerCommand.CommandText = _command.Data.Sql.ToString();

                if (_command.Data.Context.Data.CommandTimeout != Int32.MinValue)
                    _command.Data.InnerCommand.CommandTimeout = _command.Data.Context.Data.CommandTimeout;

                if (_command.Data.InnerCommand.Connection.State != ConnectionState.Open)
                    OpenConnection();

                if (_command.Data.Context.Data.UseTransaction)
                {
                    if (_command.Data.Context.Data.Transaction == null)
                        _command.Data.Context.Data.Transaction = _command.Data.Context.Data.Connection.BeginTransaction((System.Data.IsolationLevel)_command.Data.Context.Data.IsolationLevel);

                    _command.Data.InnerCommand.Transaction = _command.Data.Context.Data.Transaction;
                }

                if (_command.Data.Context.Data.OnExecuting != null)
                    _command.Data.Context.Data.OnExecuting(new CommandEventArgs(_command.Data.InnerCommand));

                if (useReader)
                    _command.Data.Reader = new DataReader(_command.Data.InnerCommand.ExecuteReader());

                _queryAlreadyExecuted = true;
            }
        }

        private void OpenConnection()
        {
            if (_command.Data.Context.Data.OnConnectionOpening != null)
                _command.Data.Context.Data.OnConnectionOpening(new ConnectionEventArgs(_command.Data.InnerCommand.Connection));

            _command.Data.InnerCommand.Connection.Open();

            if (_command.Data.Context.Data.OnConnectionOpened != null)
                _command.Data.Context.Data.OnConnectionOpened(new ConnectionEventArgs(_command.Data.InnerCommand.Connection));
        }

        private void HandleQueryFinally()
        {
            if (!_command.Data.UseMultipleResultsets)
            {
                if (_command.Data.Reader != null)
                    _command.Data.Reader.Close();

                _command.ClosePrivateConnection();
            }
        }

        private void HandleQueryException(Exception exception)
        {
            if (_command.Data.Reader != null)
                _command.Data.Reader.Close();

            _command.ClosePrivateConnection();
            if (_command.Data.Context.Data.UseTransaction)
                _command.Data.Context.CloseSharedConnection();

            if (_command.Data.Context.Data.OnError != null)
                _command.Data.Context.Data.OnError(new ErrorEventArgs(_command.Data.InnerCommand, exception));

            throw exception;
        }
    }

    internal class QueryHandler<TEntity>
    {
        private readonly DbCommandData _data;
        private readonly IQueryTypeHandler<TEntity> _typeHandler;

        public QueryHandler(DbCommandData data)
        {
            _data = data;
            if (typeof(TEntity) == typeof(object) || typeof(TEntity) == typeof(ExpandoObject))
                _typeHandler = new QueryDynamicHandler<TEntity>(data);
            else if (typeof(TEntity) == typeof(DataTable))
                _typeHandler = new QueryDataTableHandler<TEntity>(data);
            else if (ReflectionHelper.IsCustomEntity<TEntity>())
                _typeHandler = new QueryCustomEntityHandler<TEntity>(data);
            else
                _typeHandler = new QueryScalarHandler<TEntity>(data);
        }

        internal TList ExecuteMany<TList>(Action<TEntity, IDataReader> customMapperReader,
                                    Action<TEntity, dynamic> customMapperDynamic
                    )
            where TList : IList<TEntity>
        {
            var items = (TList)_data.Context.Data.EntityFactory.Create(typeof(TList));
            var reader = _data.Reader.InnerReader;

            if (_typeHandler.IterateDataReader)
            {
                while (reader.Read())
                {
                    var item = (TEntity)_typeHandler.HandleType(customMapperReader, customMapperDynamic);
                    items.Add(item);
                }
            }
            else
            {
                var item = (TEntity)_typeHandler.HandleType(customMapperReader, customMapperDynamic);
                items.Add(item);
            }

            return items;
        }

        internal TEntity ExecuteSingle(Action<TEntity, IDataReader> customMapperReader,
                                   Action<TEntity, dynamic> customMapperDynamic)
        {
            var item = default(TEntity);
            if (!_typeHandler.IterateDataReader || _data.Reader.InnerReader.Read())
                item = (TEntity)_typeHandler.HandleType(customMapperReader, customMapperDynamic);

            return item;
        }
    }

    internal class DataReader : IDataReader
    {
        public System.Data.IDataReader InnerReader { get; private set; }

        public DataReader(System.Data.IDataReader reader)
        {
            InnerReader = reader;
        }

        public void Close()
        {
            InnerReader.Close();
        }

        private T GetValue<T>(int i)
        {
            var value = InnerReader.GetValue(i);
            if (value == DBNull.Value)
                return default(T);
            return (T)value;
        }

        public int Depth
        {
            get { return InnerReader.Depth; }
        }

        public DataTable GetSchemaTable()
        {
            return InnerReader.GetSchemaTable();
        }

        public bool IsClosed
        {
            get { return InnerReader.IsClosed; }
        }

        public bool NextResult()
        {
            return InnerReader.NextResult();
        }

        public bool Read()
        {
            return InnerReader.Read();
        }

        public int RecordsAffected
        {
            get { return InnerReader.RecordsAffected; }
        }

        public void Dispose()
        {
            InnerReader.Dispose();
        }

        public int FieldCount
        {
            get { return InnerReader.FieldCount; }
        }

        public bool GetBoolean(int i)
        {
            return GetValue<bool>(i);
        }

        public bool GetBoolean(string name)
        {
            return GetBoolean(GetOrdinal(name));
        }

        public byte GetByte(int i)
        {
            return GetValue<byte>(i);
        }

        public byte GetByte(string name)
        {
            return GetByte(GetOrdinal(name));
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return IsDBNull(i) ? 0 : InnerReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public long GetBytes(string name, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return GetBytes(GetOrdinal(name), fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return GetValue<char>(i);
        }

        public char GetChar(string name)
        {
            return GetChar(GetOrdinal(name));
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return IsDBNull(i) ? 0 : InnerReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public long GetChars(string name, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return GetChars(GetOrdinal(name), fieldoffset, buffer, bufferoffset, length);
        }

        public System.Data.IDataReader GetData(int i)
        {
            return InnerReader.GetData(i);
        }

        public System.Data.IDataReader GetData(string name)
        {
            return GetData(GetOrdinal(name));
        }

        public string GetDataTypeName(int i)
        {
            return InnerReader.GetDataTypeName(i);
        }

        public string GetDataTypeName(string name)
        {
            return GetDataTypeName(GetOrdinal(name));
        }

        public DateTime GetDateTime(int i)
        {
            return GetValue<DateTime>(i);
        }

        public DateTime GetDateTime(string name)
        {
            return GetDateTime(GetOrdinal(name));
        }

        public decimal GetDecimal(int i)
        {
            return GetValue<decimal>(i);
        }

        public decimal GetDecimal(string name)
        {
            return GetDecimal(GetOrdinal(name));
        }

        public double GetDouble(int i)
        {
            return GetValue<double>(i);
        }

        public double GetDouble(string name)
        {
            return GetDouble(GetOrdinal(name));
        }

        public Type GetFieldType(int i)
        {
            return InnerReader.GetFieldType(i);
        }

        public Type GetFieldType(string name)
        {
            return GetFieldType(GetOrdinal(name));
        }

        public float GetFloat(int i)
        {
            return GetValue<float>(i);
        }

        public float GetFloat(string name)
        {
            return GetFloat(GetOrdinal(name));
        }

        public Guid GetGuid(int i)
        {
            return GetValue<Guid>(i);
        }

        public Guid GetGuid(string name)
        {
            return GetGuid(GetOrdinal(name));
        }

        public short GetInt16(int i)
        {
            return GetValue<Int16>(i);
        }

        public short GetInt16(string name)
        {
            return GetInt16(GetOrdinal(name));
        }

        public int GetInt32(int i)
        {
            return GetValue<int>(i);
        }

        public int GetInt32(string name)
        {
            return GetInt32(GetOrdinal(name));
        }

        public long GetInt64(int i)
        {
            return GetValue<long>(i);
        }

        public long GetInt64(string name)
        {
            return GetInt64(GetOrdinal(name));
        }

        public string GetName(int i)
        {
            return InnerReader.GetName(i);
        }

        public string GetName(string name)
        {
            return InnerReader.GetName(GetOrdinal(name));
        }

        public int GetOrdinal(string name)
        {
            return InnerReader.GetOrdinal(name);
        }

        public string GetString(int i)
        {
            return GetValue<string>(i);
        }

        public string GetString(string name)
        {
            return GetString(GetOrdinal(name));
        }

        public object GetValue(int i)
        {
            return GetValue<object>(i);
        }

        public object GetValue(string name)
        {
            return GetValue(GetOrdinal(name));
        }

        public int GetValues(object[] values)
        {
            return InnerReader.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return InnerReader.IsDBNull(i);
        }

        public bool IsDBNull(string name)
        {
            return IsDBNull(GetOrdinal(name));
        }

        public object this[string name]
        {
            get { return this[GetOrdinal(name)]; }
        }

        public object this[int i]
        {
            get
            {
                return IsDBNull(i) ? null : InnerReader[i];
            }
        }
    }

    internal class DynamicDataReader : DynamicObject
    {
        private readonly System.Data.IDataReader _dataReader;

        internal DynamicDataReader(System.Data.IDataReader dataReader)
        {
            _dataReader = dataReader;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _dataReader[binder.Name];
            if (result == DBNull.Value)
                result = null;

            return true;
        }
    }

    public interface IDataReader : System.Data.IDataReader
    {
        System.Data.IDataReader InnerReader { get; }
        bool GetBoolean(string name);
        byte GetByte(string name);
        long GetBytes(string name, long fieldOffset, byte[] buffer, int bufferoffset, int length);
        char GetChar(string name);
        long GetChars(string name, long fieldoffset, char[] buffer, int bufferoffset, int length);
        string GetDataTypeName(string name);
        DateTime GetDateTime(string name);
        decimal GetDecimal(string name);
        double GetDouble(string name);
        Type GetFieldType(string name);
        float GetFloat(string name);
        Guid GetGuid(string name);
        short GetInt16(string name);
        int GetInt32(string name);
        long GetInt64(string name);
        string GetName(string name);
        string GetString(string name);
        object GetValue(string name);
        bool IsDBNull(string name);
    }

    public class ConnectionEventArgs : EventArgs
    {
        public IDbConnection Connection { get; private set; }

        public ConnectionEventArgs(System.Data.IDbConnection connection)
        {
            Connection = connection;
        }
    }

    public class CommandEventArgs : EventArgs
    {
        public System.Data.IDbCommand Command { get; private set; }

        public CommandEventArgs(System.Data.IDbCommand command)
        {
            Command = command;
        }
    }

    public class ErrorEventArgs : EventArgs
    {
        public System.Data.IDbCommand Command { get; private set; }
        public Exception Exception { get; set; }

        public ErrorEventArgs(System.Data.IDbCommand command, Exception exception)
        {
            Command = command;
            Exception = exception;
        }
    }

    public partial class DbContext : IDbContext
    {
        public DbContextData Data { get; private set; }

        public DbContext()
        {
            Data = new DbContextData();
        }

        internal void CloseSharedConnection()
        {
            if (Data.Connection == null)
                return;

            if (Data.UseTransaction
                && Data.Transaction != null)
                Rollback();

            Data.Connection.Close();

            if (Data.OnConnectionClosed != null)
                Data.OnConnectionClosed(new ConnectionEventArgs(Data.Connection));
        }

        public void Dispose()
        {
            CloseSharedConnection();
        }
    }

    public class DbContextData
    {
        public bool UseTransaction { get; set; }
        public bool UseSharedConnection { get; set; }
        public System.Data.IDbConnection Connection { get; set; }
        public System.Data.Common.DbProviderFactory AdoNetProvider { get; set; }
        public IsolationLevel IsolationLevel { get; set; }
        public System.Data.IDbTransaction Transaction { get; set; }
        public IDbProvider FluentDataProvider { get; set; }
        public string ConnectionString { get; set; }
        public IEntityFactory EntityFactory { get; set; }
        public bool IgnoreIfAutoMapFails { get; set; }
        public int CommandTimeout { get; set; }
        public Action<ConnectionEventArgs> OnConnectionOpening { get; set; }
        public Action<ConnectionEventArgs> OnConnectionOpened { get; set; }
        public Action<ConnectionEventArgs> OnConnectionClosed { get; set; }
        public Action<CommandEventArgs> OnExecuting { get; set; }
        public Action<CommandEventArgs> OnExecuted { get; set; }
        public Action<ErrorEventArgs> OnError { get; set; }

        public DbContextData()
        {
            IgnoreIfAutoMapFails = false;
            UseTransaction = false;
            IsolationLevel = IsolationLevel.ReadCommitted;
            EntityFactory = new EntityFactory();
            CommandTimeout = Int32.MinValue;
        }
    }

    public class EntityFactory : IEntityFactory
    {
        public virtual object Create(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }

    public interface IDbContext : IDisposable
    {
        DbContextData Data { get; }
        IDbContext IgnoreIfAutoMapFails(bool ignoreIfAutoMapFails);
        IDbContext UseTransaction(bool useTransaction);
        IDbContext UseSharedConnection(bool useSharedConnection);
        IDbContext CommandTimeout(int timeout);
        IDbCommand Sql(string sql, params object[] parameters);
        IDbCommand MultiResultSql { get; }
        ISelectBuilder<TEntity> Select<TEntity>(string sql);
        IInsertBuilder Insert(string tableName);
        IInsertBuilder<T> Insert<T>(string tableName, T item);
        IInsertBuilderDynamic Insert(string tableName, ExpandoObject item);
        IUpdateBuilder Update(string tableName);
        IUpdateBuilder<T> Update<T>(string tableName, T item);
        IUpdateBuilderDynamic Update(string tableName, ExpandoObject item);
        IDeleteBuilder Delete(string tableName);
        IDeleteBuilder<T> Delete<T>(string tableName, T item);
        IStoredProcedureBuilder StoredProcedure(string storedProcedureName);
        IStoredProcedureBuilder<T> StoredProcedure<T>(string storedProcedureName, T item);
        IStoredProcedureBuilderDynamic StoredProcedure(string storedProcedureName, ExpandoObject item);
        IDbContext EntityFactory(IEntityFactory entityFactory);
        IDbContext ConnectionString(string connectionString, IDbProvider fluentDataProvider, string providerName = null);
        IDbContext ConnectionString(string connectionString, IDbProvider fluentDataProvider, System.Data.Common.DbProviderFactory adoNetProviderFactory);
        IDbContext ConnectionStringName(string connectionstringName, IDbProvider dbProvider);
        IDbContext IsolationLevel(IsolationLevel isolationLevel);
        IDbContext Commit();
        IDbContext Rollback();
        IDbContext OnConnectionOpening(Action<ConnectionEventArgs> action);
        IDbContext OnConnectionOpened(Action<ConnectionEventArgs> action);
        IDbContext OnConnectionClosed(Action<ConnectionEventArgs> action);
        IDbContext OnExecuting(Action<CommandEventArgs> action);
        IDbContext OnExecuted(Action<CommandEventArgs> action);
        IDbContext OnError(Action<ErrorEventArgs> action);
    }

    public interface IEntityFactory
    {
        object Create(Type type);
    }

    public enum IsolationLevel
    {
        Unspecified = -1,
        Chaos = 16,
        ReadUncommitted = 256,
        ReadCommitted = 4096,
        RepeatableRead = 65536,
        Serializable = 1048576,
        Snapshot = 16777216,
    }

    public partial class DbContext
    {
        public IDbContext IgnoreIfAutoMapFails(bool ignoreIfAutoMapFails)
        {
            Data.IgnoreIfAutoMapFails = true;
            return this;
        }
    }

    public partial class DbContext
    {
        public ISelectBuilder<TEntity> Select<TEntity>(string sql)
        {
            return new SelectBuilder<TEntity>(CreateCommand).Select(sql);
        }

        public IInsertBuilder Insert(string tableName)
        {
            return new InsertBuilder(CreateCommand, tableName);
        }

        public IInsertBuilder<T> Insert<T>(string tableName, T item)
        {
            return new InsertBuilder<T>(CreateCommand, tableName, item);
        }

        public IInsertBuilderDynamic Insert(string tableName, ExpandoObject item)
        {
            return new InsertBuilderDynamic(CreateCommand, tableName, item);
        }

        public IUpdateBuilder Update(string tableName)
        {
            return new UpdateBuilder(Data.FluentDataProvider, CreateCommand, tableName);
        }

        public IUpdateBuilder<T> Update<T>(string tableName, T item)
        {
            return new UpdateBuilder<T>(Data.FluentDataProvider, CreateCommand, tableName, item);
        }

        public IUpdateBuilderDynamic Update(string tableName, ExpandoObject item)
        {
            return new UpdateBuilderDynamic(Data.FluentDataProvider, CreateCommand, tableName, item);
        }

        public IDeleteBuilder Delete(string tableName)
        {
            return new DeleteBuilder(CreateCommand, tableName);
        }

        public IDeleteBuilder<T> Delete<T>(string tableName, T item)
        {
            return new DeleteBuilder<T>(CreateCommand, tableName, item);
        }

        private void VerifyStoredProcedureSupport()
        {
            if (!Data.FluentDataProvider.SupportsStoredProcedures)
                throw new FluentDataException("The selected database does not support stored procedures.");
        }

        public IStoredProcedureBuilder StoredProcedure(string storedProcedureName)
        {
            VerifyStoredProcedureSupport();
            return new StoredProcedureBuilder(CreateCommand, storedProcedureName);
        }

        public IStoredProcedureBuilder<T> StoredProcedure<T>(string storedProcedureName, T item)
        {
            VerifyStoredProcedureSupport();
            return new StoredProcedureBuilder<T>(CreateCommand, storedProcedureName, item);
        }

        public IStoredProcedureBuilderDynamic StoredProcedure(string storedProcedureName, ExpandoObject item)
        {
            VerifyStoredProcedureSupport();
            return new StoredProcedureBuilderDynamic(CreateCommand, storedProcedureName, item);
        }
    }

    public partial class DbContext
    {
        public IDbContext CommandTimeout(int timeout)
        {
            Data.CommandTimeout = timeout;
            return this;
        }
    }

    public partial class DbContext
    {
        public IDbContext ConnectionString(string connectionString, IDbProvider fluentDataProvider, string providerName = null)
        {
            if (string.IsNullOrEmpty(providerName))
                providerName = fluentDataProvider.ProviderName;
            //var adoNetProvider = System.Data.Common.DbProviderFactories.GetFactory(providerName);
            //return ConnectionString(connectionString, fluentDataProvider, adoNetProvider);
            /****************返回指定的DBProvider 不在需要从配置文件里面得到DBProvider  by suxiaobo  2017-05-03 10:28   ****************/
            if (providerName == "System.Data.SQLite")
            {
                var adoNetProvider = new SQLiteFactory();
                return ConnectionString(connectionString, fluentDataProvider, adoNetProvider);
            }
            else
            {
                var adoNetProvider = System.Data.Common.DbProviderFactories.GetFactory(providerName);
                return ConnectionString(connectionString, fluentDataProvider, adoNetProvider);
            }
           
        }

        public IDbContext ConnectionString(string connectionString, IDbProvider fluentDataProvider, System.Data.Common.DbProviderFactory adoNetProviderFactory)
        {
            Data.ConnectionString = connectionString;
            Data.FluentDataProvider = fluentDataProvider;
            Data.AdoNetProvider = adoNetProviderFactory;
            return this;
        }

        public IDbContext ConnectionStringName(string connectionstringName, IDbProvider dbProvider)
        {
            var settings = ConfigurationManager.ConnectionStrings[connectionstringName];
            if (settings == null)
                throw new FluentDataException("A connectionstring with the specified name was not found in the .config file");

            ConnectionString(settings.ConnectionString, dbProvider, settings.ProviderName);
            return this;
        }
    }

    public partial class DbContext
    {
        public IDbContext EntityFactory(IEntityFactory entityFactory)
        {
            Data.EntityFactory = entityFactory;
            return this;
        }
    }

    public partial class DbContext
    {
        public IDbContext OnConnectionOpening(Action<ConnectionEventArgs> action)
        {
            Data.OnConnectionOpening = action;
            return this;
        }

        public IDbContext OnConnectionOpened(Action<ConnectionEventArgs> action)
        {
            Data.OnConnectionOpened = action;
            return this;
        }

        public IDbContext OnConnectionClosed(Action<ConnectionEventArgs> action)
        {
            Data.OnConnectionClosed = action;
            return this;
        }

        public IDbContext OnExecuting(Action<CommandEventArgs> action)
        {
            Data.OnExecuting = action;
            return this;
        }

        public IDbContext OnExecuted(Action<CommandEventArgs> action)
        {
            Data.OnExecuted = action;
            return this;
        }

        public IDbContext OnError(Action<ErrorEventArgs> action)
        {
            Data.OnError = action;
            return this;
        }
    }

    public partial class DbContext
    {
        private DbCommand CreateCommand
        {
            get
            {
                IDbConnection connection = null;

                if (Data.UseTransaction
                    || Data.UseSharedConnection)
                {
                    if (Data.Connection == null)
                    {
                        Data.Connection = (IDbConnection)Data.AdoNetProvider.CreateConnection();
                        Data.Connection.ConnectionString = Data.ConnectionString;
                    }
                    connection = Data.Connection;
                }
                else
                {
                    connection = (IDbConnection)Data.AdoNetProvider.CreateConnection();
                    connection.ConnectionString = Data.ConnectionString;
                }
                var cmd = connection.CreateCommand();
                cmd.Connection = connection;

                return new DbCommand(this, cmd);
            }
        }

        public IDbCommand Sql(string sql, params object[] parameters)
        {
            var command = CreateCommand.Sql(sql).Parameters(parameters);
            return command;
        }

        public IDbCommand MultiResultSql
        {
            get
            {
                var command = CreateCommand.UseMultiResult(true);
                return command;
            }
        }
    }

    public partial class DbContext
    {
        public IDbContext UseTransaction(bool useTransaction)
        {
            Data.UseTransaction = useTransaction;
            return this;
        }

        public IDbContext UseSharedConnection(bool useSharedConnection)
        {
            Data.UseSharedConnection = useSharedConnection;
            return this;
        }

        public IDbContext IsolationLevel(IsolationLevel isolationLevel)
        {
            Data.IsolationLevel = isolationLevel;
            return this;
        }

        public IDbContext Commit()
        {
            TransactionAction(() => Data.Transaction.Commit());
            return this;
        }

        public IDbContext Rollback()
        {
            TransactionAction(() => Data.Transaction.Rollback());
            return this;
        }

        private void TransactionAction(Action action)
        {
            if (Data.Transaction == null)
                return;
            if (!Data.UseTransaction)
                throw new FluentDataException("Transaction support has not been enabled.");
            action();
            Data.Transaction = null;
        }
    }

    internal class DataReaderHelper
    {
        internal static List<DataReaderField> GetDataReaderFields(System.Data.IDataReader reader)
        {
            var columns = new List<DataReaderField>();

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var column = new DataReaderField(i, reader.GetName(i), reader.GetFieldType(i));

                if (columns.SingleOrDefault(x => x.LowerName == column.LowerName) == null)
                    columns.Add(column);
            }

            return columns;
        }
    }

    internal class PropertyExpressionParser<T>
    {
        private readonly object _item;
        private readonly PropertyInfo _property;

        public PropertyExpressionParser(object item, Expression<Func<T, object>> propertyExpression)
        {
            _item = item;
            _property = GetProperty(propertyExpression);
        }

        private static PropertyInfo GetProperty(Expression<Func<T, object>> exp)
        {
            PropertyInfo result;
            if (exp.Body.NodeType == ExpressionType.Convert)
                result = ((MemberExpression)((UnaryExpression)exp.Body).Operand).Member as PropertyInfo;
            else result = ((MemberExpression)exp.Body).Member as PropertyInfo;

            Type t = typeof(T);
            if (result != null && t != null)
            {                
                return t.GetProperty(result.Name);
            }               

            throw new ArgumentException(string.Format("Expression '{0}' does not refer to a property.", exp.ToString()));
        }

        public object Value
        {
            get { return ReflectionHelper.GetPropertyValue(_item, _property); }
        }

        public string Name
        {
            get { return _property.Name; }
        }

        public Type Type
        {
            get { return ReflectionHelper.GetPropertyType(_property); }
        }
    }
    [Serializable]
    public class FluentDataException : Exception
    {
        public FluentDataException(string message)
            : base(message)
        {
        }
        public FluentDataException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    internal static class ReflectionHelper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _cachedProperties = new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();

        public static string GetPropertyNameFromExpression<T>(Expression<Func<T, object>> expression)
        {
            string propertyPath = null;
            if (expression.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)expression.Body;
                if (unaryExpression.NodeType == ExpressionType.Convert)
                    propertyPath = unaryExpression.Operand.ToString();
            }

            if (propertyPath == null)
                propertyPath = expression.Body.ToString();

            propertyPath = propertyPath.Replace(expression.Parameters[0] + ".", string.Empty);

            return propertyPath;
        }

        public static List<string> GetPropertyNamesFromExpressions<T>(Expression<Func<T, object>>[] expressions)
        {
            var propertyNames = new List<string>();
            foreach (var expression in expressions)
            {
                var propertyName = GetPropertyNameFromExpression(expression);
                propertyNames.Add(propertyName);
            }
            return propertyNames;
        }

        public static object GetPropertyValue(object item, PropertyInfo property)
        {
            var value = property.GetValue(item, null);

            return value;
        }

        public static object GetPropertyValue(object item, string propertyName)
        {
            PropertyInfo property;
            foreach (var part in propertyName.Split('.'))
            {
                if (item == null)
                    return null;

                var type = item.GetType();

                property = type.GetProperty(part);
                if (property == null)
                    return null;

                item = GetPropertyValue(item, property);
            }
            return item;
        }

        public static object GetPropertyValueDynamic(object item, string name)
        {
            var dictionary = (IDictionary<string, object>)item;

            return dictionary[name];
        }

        public static Dictionary<string, PropertyInfo> GetProperties(Type type)
        {
            var properties = _cachedProperties.GetOrAdd(type, BuildPropertyDictionary);

            return properties;
        }

        private static Dictionary<string, PropertyInfo> BuildPropertyDictionary(Type type)
        {
            var result = new Dictionary<string, PropertyInfo>();

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                /***********(query)属性Name和数据库表字段Column不一样时创建对应映射关系   by suxiaobo  2017-05-02 19:26***************/
                var column = property.GetCustomAttributes(true).OfType<DbColumnAttribute>().FirstOrDefault();
                var columnName = property.Name;
                if (column != null)
                {
                    columnName = column.ColumnName;
                    result.Add(columnName.ToLower(), property);
                }
            }
            return result;
        }

        public static bool IsList(object item)
        {
            if (item is ICollection)
                return true;

            return false;
        }

        public static bool IsNullable(PropertyInfo property)
        {
            if (property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return true;

            return false;
        }

        /// <summary>
        /// Includes a work around for getting the actual type of a Nullable type.
        /// </summary>
        public static Type GetPropertyType(PropertyInfo property)
        {
            if (IsNullable(property))
                return property.PropertyType.GetGenericArguments()[0];

            return property.PropertyType;
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }

        public static bool IsBasicClrType(Type type)
        {
            if (type.IsEnum
                || type.IsPrimitive
                || type.IsValueType
                || type == typeof(string)
                || type == typeof(DateTime))
                return true;

            return false;
        }

        public static bool IsCustomEntity<T>()
        {
            var type = typeof(T);
            if (type.IsClass && Type.GetTypeCode(type) == TypeCode.Object)
                return true;
            return false;
        }
    }

    public class AccessProvider : IDbProvider
    {
        public string ProviderName
        {
            get
            {
                return "System.Data.OleDb";
            }
        }

        public bool SupportsOutputParameters
        {
            get { return false; }
        }

        public bool SupportsMultipleQueries
        {
            get { return false; }
        }

        public bool SupportsMultipleResultsets
        {
            get { return false; }
        }

        public bool SupportsStoredProcedures
        {
            get { return false; }
        }

        public bool RequiresIdentityColumn
        {
            get { return false; }
        }

        public string GetParameterName(string parameterName)
        {
            return "@" + parameterName;
        }

        public string GetSelectBuilderAlias(string name, string alias)
        {
            return name + " as " + alias;
        }

        public string GetSqlForSelectBuilder(SelectBuilderData data)
        {
            throw new NotImplementedException();
        }

        public string GetSqlForInsertBuilder(BuilderData data)
        {
            return new InsertBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForUpdateBuilder(BuilderData data)
        {
            return new UpdateBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForDeleteBuilder(BuilderData data)
        {
            return new DeleteBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForStoredProcedureBuilder(BuilderData data)
        {
            throw new NotImplementedException();
        }

        public DataTypes GetDbTypeForClrType(Type clrType)
        {
            return new DbTypeMapper().GetDbTypeForClrType(clrType);
        }

        public object ExecuteReturnLastId<T>(IDbCommand command, string identityColumnName = null)
        {
            object lastId = null;

            command.Data.ExecuteQueryHandler.ExecuteQuery(false, () =>
            {
                var recordsAffected = command.Data.InnerCommand.ExecuteNonQuery();

                if (recordsAffected > 0)
                {
                    command.Data.InnerCommand.CommandText = "select @@Identity";

                    lastId = command.Data.InnerCommand.ExecuteScalar();
                }
            });

            return lastId;
        }

        public void OnCommandExecuting(IDbCommand command)
        {

        }

        public string EscapeColumnName(string name)
        {
            return "[" + name + "]";
        }

        public bool IsColumnNameEscaped(string name)
        {
            if (name.Contains("["))
                return true;
            return false;
        }
    }

    internal class ConnectionFactory
    {
        public static IDbConnection CreateConnection(string providerName, string connectionString)
        {
            var factory = DbProviderFactories.GetFactory(providerName);

            var connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }
    }

    internal class DbTypeMapper
    {
        private static Dictionary<Type, DataTypes> _types;
        private static readonly object _locker = new object();

        public DataTypes GetDbTypeForClrType(Type clrType)
        {
            if (_types == null)
            {
                lock (_locker)
                {
                    if (_types == null)
                    {
                        _types = new Dictionary<Type, DataTypes>();
                        _types.Add(typeof(Int16), DataTypes.Int16);
                        _types.Add(typeof(Int32), DataTypes.Int32);
                        _types.Add(typeof(Int64), DataTypes.Int64);
                        _types.Add(typeof(string), DataTypes.String);
                        _types.Add(typeof(DateTime), DataTypes.DateTime);
                        _types.Add(typeof(XDocument), DataTypes.Xml);
                        _types.Add(typeof(decimal), DataTypes.Decimal);
                        _types.Add(typeof(Guid), DataTypes.Guid);
                        _types.Add(typeof(Boolean), DataTypes.Boolean);
                        _types.Add(typeof(char), DataTypes.String);
                        _types.Add(typeof(DBNull), DataTypes.String);
                        _types.Add(typeof(float), DataTypes.Single);
                        _types.Add(typeof(double), DataTypes.Double);
                        _types.Add(typeof(byte[]), DataTypes.Binary);
                    }
                }
            }

            if (!_types.ContainsKey(clrType))
                return DataTypes.Object;

            var dbType = _types[clrType];
            return dbType;
        }
    }

    internal class DeleteBuilderSqlGenerator
    {
        public string GenerateSql(IDbProvider provider, string parameterPrefix, BuilderData data)
        {
            var whereSql = "";
            foreach (var column in data.Columns)
            {
                if (whereSql.Length > 0)
                    whereSql += " and ";

                whereSql += string.Format("{0} = {1}{2}",
                                                provider.EscapeColumnName(column.ColumnName),
                                                parameterPrefix,
                                                column.ParameterName);
            }

            var sql = string.Format("delete from {0} where {1}", data.ObjectName, whereSql);
            return sql;
        }
    }

    internal class InsertBuilderSqlGenerator
    {
        public string GenerateSql(IDbProvider provider, string parameterPrefix, BuilderData data)
        {
            var insertSql = "";
            var valuesSql = "";
            foreach (var column in data.Columns)
            {
                if (insertSql.Length > 0)
                {
                    insertSql += ",";
                    valuesSql += ",";
                }

                insertSql += provider.EscapeColumnName(column.ColumnName);
                valuesSql += parameterPrefix + column.ParameterName;
            }

            var sql = string.Format("insert into {0}({1}) values({2})",
                                        data.ObjectName,
                                        insertSql,
                                        valuesSql);
            return sql;
        }
    }

    internal class UpdateBuilderSqlGenerator
    {
        public string GenerateSql(IDbProvider provider, string parameterPrefix, BuilderData data)
        {
            var setSql = "";
            foreach (var column in data.Columns)
            {
                if (setSql.Length > 0)
                    setSql += ", ";

                setSql += string.Format("{0} = {1}{2}",
                                    provider.EscapeColumnName(column.ColumnName),
                                    parameterPrefix,
                                    column.ParameterName);
            }

            var whereSql = "";
            foreach (var column in data.Where)
            {
                if (whereSql.Length > 0)
                    whereSql += " and ";

                whereSql += string.Format("{0} = {1}{2}",
                                    provider.EscapeColumnName(column.ColumnName),
                                    parameterPrefix,
                                    column.ParameterName);
            }

            var sql = string.Format("update {0} set {1} where {2}",
                                        data.ObjectName,
                                        setSql,
                                        whereSql);
            return sql;
        }
    }

    public class DB2Provider : IDbProvider
    {
        public string ProviderName
        {
            get
            {
                return "IBM.Data.DB2";
            }
        }

        public bool SupportsOutputParameters
        {
            get { return true; }
        }

        public bool SupportsMultipleResultsets
        {
            get { return true; }
        }

        public bool SupportsMultipleQueries
        {
            get { return true; }
        }

        public bool SupportsStoredProcedures
        {
            get { return true; }
        }

        public bool RequiresIdentityColumn
        {
            get { return false; }
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return ConnectionFactory.CreateConnection(ProviderName, connectionString);
        }

        public string GetParameterName(string parameterName)
        {
            return "@" + parameterName;
        }

        public string GetSelectBuilderAlias(string name, string alias)
        {
            return name + " as " + alias;
        }

        public string GetSqlForSelectBuilder(SelectBuilderData data)
        {
            var sql = "";
            sql = "select " + data.Select;
            sql += " from " + data.From;
            if (data.WhereSql.Length > 0)
                sql += " where " + data.WhereSql;
            if (data.GroupBy.Length > 0)
                sql += " group by " + data.GroupBy;
            if (data.Having.Length > 0)
                sql += " having " + data.Having;
            if (data.OrderBy.Length > 0)
                sql += " order by " + data.OrderBy;
            if (data.PagingItemsPerPage > 0
                && data.PagingCurrentPage > 0)
            {
                sql += string.Format(" limit {0}, {1}", data.GetFromItems() - 1, data.GetToItems());
            }

            return sql;
        }

        public string GetSqlForInsertBuilder(BuilderData data)
        {
            return new InsertBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForUpdateBuilder(BuilderData data)
        {
            return new UpdateBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForDeleteBuilder(BuilderData data)
        {
            return new DeleteBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForStoredProcedureBuilder(BuilderData data)
        {
            return data.ObjectName;
        }

        public DataTypes GetDbTypeForClrType(Type clrType)
        {
            return new DbTypeMapper().GetDbTypeForClrType(clrType);
        }

        public object ExecuteReturnLastId<T>(IDbCommand command, string identityColumnName = null)
        {
            if (command.Data.Sql[command.Data.Sql.Length - 1] != ';')
                command.Sql(";");

            command.Sql("select IDENTITY_VAL_LOCAL() as LastId from sysibm.sysdummy1;");

            object lastId = null;

            command.Data.ExecuteQueryHandler.ExecuteQuery(false, () =>
            {
                lastId = command.Data.InnerCommand.ExecuteScalar();
            });

            return lastId;
        }

        public void OnCommandExecuting(IDbCommand command)
        {
        }

        public string EscapeColumnName(string name)
        {
            return name;
        }
    }

    public class PostgreSqlProvider : IDbProvider
    {
        public string ProviderName
        {
            get
            {
                return "Npgsql";
            }
        }

        public bool SupportsOutputParameters
        {
            get { return true; }
        }

        public bool SupportsMultipleResultsets
        {
            get { return true; }
        }

        public bool SupportsMultipleQueries
        {
            get { return true; }
        }

        public bool SupportsStoredProcedures
        {
            get { return true; }
        }

        public bool RequiresIdentityColumn
        {
            get { return false; }
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return ConnectionFactory.CreateConnection(ProviderName, connectionString);
        }

        public string GetParameterName(string parameterName)
        {
            return ":" + parameterName;
        }

        public string GetSelectBuilderAlias(string name, string alias)
        {
            return name + " as " + alias;
        }

        public string GetSqlForSelectBuilder(SelectBuilderData data)
        {
            var sql = "";
            sql = "select " + data.Select;
            sql += " from " + data.From;
            if (data.WhereSql.Length > 0)
                sql += " where " + data.WhereSql;
            if (data.GroupBy.Length > 0)
                sql += " group by " + data.GroupBy;
            if (data.Having.Length > 0)
                sql += " having " + data.Having;
            if (data.OrderBy.Length > 0)
                sql += " order by " + data.OrderBy;
            if (data.PagingItemsPerPage > 0)
            {
                if (data.PagingItemsPerPage > 0)
                    sql += " limit " + data.PagingItemsPerPage;
                sql += " offset " + (data.GetFromItems() - 1);
            }


            return sql;
        }

        public string GetSqlForInsertBuilder(BuilderData data)
        {
            return new InsertBuilderSqlGenerator().GenerateSql(this, ":", data);
        }

        public string GetSqlForUpdateBuilder(BuilderData data)
        {
            return new UpdateBuilderSqlGenerator().GenerateSql(this, ":", data);
        }

        public string GetSqlForDeleteBuilder(BuilderData data)
        {
            return new DeleteBuilderSqlGenerator().GenerateSql(this, ":", data);
        }

        public string GetSqlForStoredProcedureBuilder(BuilderData data)
        {
            return data.ObjectName;
        }

        public DataTypes GetDbTypeForClrType(Type clrType)
        {
            return new DbTypeMapper().GetDbTypeForClrType(clrType);
        }

        public object ExecuteReturnLastId<T>(IDbCommand command, string identityColumnName = null)
        {
            if (command.Data.Sql[command.Data.Sql.Length - 1] != ';')
                command.Sql(";");
            command.Data.InnerCommand.CommandText += "select lastval();";

            object lastId = null;

            command.Data.ExecuteQueryHandler.ExecuteQuery(false, () =>
            {
                lastId = command.Data.InnerCommand.ExecuteScalar();
            });

            return lastId;
        }

        public void OnCommandExecuting(IDbCommand command)
        {
        }

        public string EscapeColumnName(string name)
        {
            return name;
        }
    }

    public class MySqlProvider : IDbProvider
    {
        public string ProviderName
        {
            get
            {
                return "MySql.Data.MySqlClient";
            }
        }

        public bool SupportsOutputParameters
        {
            get { return true; }
        }

        public bool SupportsMultipleResultsets
        {
            get { return true; }
        }

        public bool SupportsMultipleQueries
        {
            get { return true; }
        }

        public bool SupportsStoredProcedures
        {
            get { return true; }
        }

        public bool RequiresIdentityColumn
        {
            get { return false; }
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return ConnectionFactory.CreateConnection(ProviderName, connectionString);
        }

        public string GetParameterName(string parameterName)
        {
            return "@" + parameterName;
        }

        public string GetSelectBuilderAlias(string name, string alias)
        {
            return name + " as " + alias;
        }

        public string GetSqlForSelectBuilder(SelectBuilderData data)
        {
            var sql = "";
            sql = "select " + data.Select;
            sql += " from " + data.From;
            if (data.WhereSql.Length > 0)
                sql += " where " + data.WhereSql;
            if (data.GroupBy.Length > 0)
                sql += " group by " + data.GroupBy;
            if (data.Having.Length > 0)
                sql += " having " + data.Having;
            if (data.OrderBy.Length > 0)
                sql += " order by " + data.OrderBy;
            if (data.PagingItemsPerPage > 0
                && data.PagingCurrentPage > 0)
            {
                sql += string.Format(" limit {0}, {1}", data.GetFromItems() - 1, data.GetToItems());
            }

            return sql;
        }

        public string GetSqlForInsertBuilder(BuilderData data)
        {
            return new InsertBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForUpdateBuilder(BuilderData data)
        {
            return new UpdateBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForDeleteBuilder(BuilderData data)
        {
            return new DeleteBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForStoredProcedureBuilder(BuilderData data)
        {
            return data.ObjectName;
        }

        public DataTypes GetDbTypeForClrType(Type clrType)
        {
            return new DbTypeMapper().GetDbTypeForClrType(clrType);
        }

        public object ExecuteReturnLastId<T>(IDbCommand command, string identityColumnName = null)
        {
            if (command.Data.Sql[command.Data.Sql.Length - 1] != ';')
                command.Sql(";");

            command.Sql("select LAST_INSERT_ID() as `LastInsertedId`");

            object lastId = null;

            command.Data.ExecuteQueryHandler.ExecuteQuery(false, () =>
            {
                lastId = command.Data.InnerCommand.ExecuteScalar();
            });

            return lastId;
        }

        public void OnCommandExecuting(IDbCommand command)
        {
        }

        public string EscapeColumnName(string name)
        {
            if (name.Contains("`"))
                return name;
            return "`" + name + "`";
        }
    }

    public interface IDbProvider
    {
        string ProviderName { get; }
        bool SupportsMultipleResultsets { get; }
        bool SupportsMultipleQueries { get; }
        bool SupportsOutputParameters { get; }
        bool SupportsStoredProcedures { get; }
        bool RequiresIdentityColumn { get; }
        string GetParameterName(string parameterName);
        string GetSelectBuilderAlias(string name, string alias);
        string GetSqlForSelectBuilder(SelectBuilderData data);
        string GetSqlForInsertBuilder(BuilderData data);
        string GetSqlForUpdateBuilder(BuilderData data);
        string GetSqlForDeleteBuilder(BuilderData data);
        string GetSqlForStoredProcedureBuilder(BuilderData data);
        DataTypes GetDbTypeForClrType(Type clrType);
        object ExecuteReturnLastId<T>(IDbCommand command, string identityColumnName);
        void OnCommandExecuting(IDbCommand command);
        string EscapeColumnName(string name);
    }

    public class OracleProvider : IDbProvider
    {
        public string ProviderName
        {
            get
            {
                return "Oracle.DataAccess.Client";
            }
        }

        public bool SupportsOutputParameters
        {
            get { return true; }
        }

        public bool SupportsMultipleResultsets
        {
            get { return false; }
        }

        public bool SupportsMultipleQueries
        {
            get { return true; }
        }

        public bool SupportsStoredProcedures
        {
            get { return true; }
        }

        public bool RequiresIdentityColumn
        {
            get { return true; }
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return ConnectionFactory.CreateConnection(ProviderName, connectionString);
        }

        public string GetParameterName(string parameterName)
        {
            return ":" + parameterName;
        }

        public string GetSelectBuilderAlias(string name, string alias)
        {
            return name + " " + alias;
        }

        public string GetSqlForSelectBuilder(SelectBuilderData data)
        {
            var sql = "";
            if (data.PagingItemsPerPage == 0)
            {
                sql = "select " + data.Select;
                sql += " from " + data.From;
                if (data.WhereSql.Length > 0)
                    sql += " where " + data.WhereSql;
                if (data.GroupBy.Length > 0)
                    sql += " group by " + data.GroupBy;
                if (data.Having.Length > 0)
                    sql += " having " + data.Having;
                if (data.OrderBy.Length > 0)
                    sql += " order by " + data.OrderBy;
            }
            else if (data.PagingItemsPerPage > 0)
            {
                sql += " from " + data.From;
                if (data.WhereSql.Length > 0)
                    sql += " where " + data.WhereSql;
                if (data.GroupBy.Length > 0)
                    sql += " group by " + data.GroupBy;
                if (data.Having.Length > 0)
                    sql += " having " + data.Having;

                sql = string.Format(@"select * from
										(
											select {0}, 
												row_number() over (order by {1}) FLUENTDATA_ROWNUMBER
											{2}
										)
										where fluentdata_RowNumber between {3} and {4}
										order by fluentdata_RowNumber",
                                            data.Select,
                                            data.OrderBy,
                                            sql,
                                            data.GetFromItems(),
                                            data.GetToItems());
            }

            return sql;
        }

        public string GetSqlForInsertBuilder(BuilderData data)
        {
            return new InsertBuilderSqlGenerator().GenerateSql(this, ":", data);
        }

        public string GetSqlForUpdateBuilder(BuilderData data)
        {
            return new UpdateBuilderSqlGenerator().GenerateSql(this, ":", data);
        }

        public string GetSqlForDeleteBuilder(BuilderData data)
        {
            return new DeleteBuilderSqlGenerator().GenerateSql(this, ":", data);
        }

        public string GetSqlForStoredProcedureBuilder(BuilderData data)
        {
            return data.ObjectName;
        }

        public DataTypes GetDbTypeForClrType(Type clrType)
        {
            return new DbTypeMapper().GetDbTypeForClrType(clrType);
        }

        public object ExecuteReturnLastId<T>(IDbCommand command, string identityColumnName = null)
        {
            command.ParameterOut("FluentDataLastId", command.Data.Context.Data.FluentDataProvider.GetDbTypeForClrType(typeof(T)));
            command.Sql(" returning " + identityColumnName + " into :FluentDataLastId");


            object lastId = null;

            command.Data.ExecuteQueryHandler.ExecuteQuery(false, () =>
            {
                command.Data.InnerCommand.ExecuteNonQuery();

                lastId = command.ParameterValue<object>("FluentDataLastId");
            });

            return lastId;
        }

        public void OnCommandExecuting(IDbCommand command)
        {
            if (command.Data.InnerCommand.CommandType == CommandType.Text)
            {
                dynamic innerCommand = command.Data.InnerCommand;
                innerCommand.BindByName = true;
            }
        }

        public string EscapeColumnName(string name)
        {
            return name;
        }
    }

    public class SqlAzureProvider : SqlServerProvider
    {
    }

    public class SqliteProvider : IDbProvider
    {
        public string ProviderName
        {
            get
            {
                return "System.Data.SQLite";
            }
        }

        public bool SupportsOutputParameters
        {
            get { return true; }
        }

        public bool SupportsMultipleResultsets
        {
            get { return true; }
        }

        public bool SupportsMultipleQueries
        {
            get { return true; }
        }

        public bool SupportsStoredProcedures
        {
            get { return false; }
        }

        public bool RequiresIdentityColumn
        {
            get { return false; }
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return ConnectionFactory.CreateConnection(ProviderName, connectionString);
        }

        public string GetParameterName(string parameterName)
        {
            return "@" + parameterName;
        }

        public string GetSelectBuilderAlias(string name, string alias)
        {
            return name + " as " + alias;
        }

        public string GetSqlForSelectBuilder(SelectBuilderData data)
        {
            var sql = "";
            sql = "select " + data.Select;
            sql += " from " + data.From;
            if (data.WhereSql.Length > 0)
                sql += " where " + data.WhereSql;
            if (data.GroupBy.Length > 0)
                sql += " group by " + data.GroupBy;
            if (data.Having.Length > 0)
                sql += " having " + data.Having;
            if (data.OrderBy.Length > 0)
                sql += " order by " + data.OrderBy;
            if (data.PagingItemsPerPage > 0
                && data.PagingCurrentPage > 0)
            {
                sql += string.Format(" limit {0}, {1}", data.GetFromItems() - 1, data.GetToItems());
            }

            return sql;
        }

        public string GetSqlForInsertBuilder(BuilderData data)
        {
            return new InsertBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForUpdateBuilder(BuilderData data)
        {
            return new UpdateBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForDeleteBuilder(BuilderData data)
        {
            return new DeleteBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForStoredProcedureBuilder(BuilderData data)
        {
            throw new NotImplementedException();
        }

        public DataTypes GetDbTypeForClrType(Type clrType)
        {
            return new DbTypeMapper().GetDbTypeForClrType(clrType);
        }

        public object ExecuteReturnLastId<T>(IDbCommand command, string identityColumnName = null)
        {
            if (command.Data.Sql[command.Data.Sql.Length - 1] != ';')
                command.Sql(";");

            command.Sql("select last_insert_rowid();");

            object lastId = null;

            command.Data.ExecuteQueryHandler.ExecuteQuery(false, () =>
            {
                lastId = command.Data.InnerCommand.ExecuteScalar();
            });

            return lastId;
        }

        public void OnCommandExecuting(IDbCommand command)
        {
        }

        public string EscapeColumnName(string name)
        {
            if (name.Contains("["))
                return name;
            return "[" + name + "]";
        }
    }

    public class SqlServerCompactProvider : IDbProvider
    {
        public string ProviderName
        {
            get
            {
                return "System.Data.SqlServerCe.4.0";
            }
        }

        public bool SupportsOutputParameters
        {
            get { return false; }
        }

        public bool SupportsMultipleQueries
        {
            get { return false; }
        }

        public bool SupportsMultipleResultsets
        {
            get { return false; }
        }

        public bool SupportsStoredProcedures
        {
            get { return false; }
        }

        public bool RequiresIdentityColumn
        {
            get { return false; }
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return ConnectionFactory.CreateConnection(ProviderName, connectionString);
        }

        public string GetParameterName(string parameterName)
        {
            return "@" + parameterName;
        }

        public string GetSelectBuilderAlias(string name, string alias)
        {
            return name + " as " + alias;
        }

        public string GetSqlForSelectBuilder(SelectBuilderData data)
        {
            var sql = "";
            sql = "select " + data.Select;
            sql += " from " + data.From;
            if (data.WhereSql.Length > 0)
                sql += " where " + data.WhereSql;
            if (data.GroupBy.Length > 0)
                sql += " group by " + data.GroupBy;
            if (data.Having.Length > 0)
                sql += " having " + data.Having;
            if (data.OrderBy.Length > 0)
                sql += " order by " + data.OrderBy;
            if (data.PagingItemsPerPage > 0)
            {
                sql += " offset " + (data.GetFromItems() - 1) + " rows";
                if (data.PagingItemsPerPage > 0)
                    sql += " fetch next " + data.PagingItemsPerPage + " rows only";
            }

            return sql;
        }

        public string GetSqlForInsertBuilder(BuilderData data)
        {
            return new InsertBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForUpdateBuilder(BuilderData data)
        {
            return new UpdateBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForDeleteBuilder(BuilderData data)
        {
            return new DeleteBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForStoredProcedureBuilder(BuilderData data)
        {
            throw new NotImplementedException();
        }

        public DataTypes GetDbTypeForClrType(Type clrType)
        {
            return new DbTypeMapper().GetDbTypeForClrType(clrType);
        }

        public object ExecuteReturnLastId<T>(IDbCommand command, string identityColumnName = null)
        {
            object lastId = null;

            command.Data.ExecuteQueryHandler.ExecuteQuery(false, () =>
            {
                var recordsAffected = command.Data.InnerCommand.ExecuteNonQuery();

                if (recordsAffected > 0)
                {
                    command.Data.InnerCommand.CommandText = "select cast(@@identity as int)";

                    lastId = command.Data.InnerCommand.ExecuteScalar();
                }
            });

            return lastId;
        }

        public void OnCommandExecuting(IDbCommand command)
        {

        }

        public string EscapeColumnName(string name)
        {
            if (name.Contains("["))
                return name;
            return "[" + name + "]";
        }
    }

    public class SqlServerProvider : IDbProvider
    {
        public string ProviderName
        {
            get
            {
                return "System.Data.SqlClient";
            }
        }

        public bool SupportsOutputParameters
        {
            get { return true; }
        }

        public bool SupportsMultipleResultsets
        {
            get { return true; }
        }

        public bool SupportsMultipleQueries
        {
            get { return true; }
        }

        public bool SupportsStoredProcedures
        {
            get { return true; }
        }

        public bool RequiresIdentityColumn
        {
            get { return false; }
        }

        public IDbConnection CreateConnection(string connectionString)
        {
            return ConnectionFactory.CreateConnection(ProviderName, connectionString);
        }

        public string GetParameterName(string parameterName)
        {
            return "@" + parameterName;
        }

        public string GetSelectBuilderAlias(string name, string alias)
        {
            return name + " as " + alias;
        }

        public string GetSqlForSelectBuilder(SelectBuilderData data)
        {
            var sql = new StringBuilder();
            if (data.PagingCurrentPage == 1)
            {
                if (data.PagingItemsPerPage == 0)
                    sql.Append("select");
                else
                    sql.Append("select top " + data.PagingItemsPerPage.ToString());
                sql.Append(" " + data.Select);
                sql.Append(" from " + data.From);
                if (data.WhereSql.Length > 0)
                    sql.Append(" where " + data.WhereSql);
                if (data.GroupBy.Length > 0)
                    sql.Append(" group by " + data.GroupBy);
                if (data.Having.Length > 0)
                    sql.Append(" having " + data.Having);
                if (data.OrderBy.Length > 0)
                    sql.Append(" order by " + data.OrderBy);
                return sql.ToString();
            }
            else
            {
                sql.Append(" from " + data.From);
                if (data.WhereSql.Length > 0)
                    sql.Append(" where " + data.WhereSql);
                if (data.GroupBy.Length > 0)
                    sql.Append(" group by " + data.GroupBy);
                if (data.Having.Length > 0)
                    sql.Append(" having " + data.Having);

                var pagedSql = string.Format(@"with PagedPersons as
								(
									select top 100 percent {0}, row_number() over (order by {1}) as FLUENTDATA_ROWNUMBER
									{2}
								)
								select *
								from PagedPersons
								where fluentdata_RowNumber between {3} and {4}",
                                             data.Select,
                                             data.OrderBy,
                                             sql,
                                             data.GetFromItems(),
                                             data.GetToItems());
                return pagedSql;
            }
        }

        public string GetSqlForInsertBuilder(BuilderData data)
        {
            return new InsertBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForUpdateBuilder(BuilderData data)
        {
            return new UpdateBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForDeleteBuilder(BuilderData data)
        {
            return new DeleteBuilderSqlGenerator().GenerateSql(this, "@", data);
        }

        public string GetSqlForStoredProcedureBuilder(BuilderData data)
        {
            return data.ObjectName;
        }

        public DataTypes GetDbTypeForClrType(Type clrType)
        {
            return new DbTypeMapper().GetDbTypeForClrType(clrType);
        }

        public object ExecuteReturnLastId<T>(IDbCommand command, string identityColumnName = null)
        {
            if (command.Data.Sql[command.Data.Sql.Length - 1] != ';')
                command.Sql(";");

            command.Sql("select SCOPE_IDENTITY()");

            object lastId = null;

            command.Data.ExecuteQueryHandler.ExecuteQuery(false, () =>
            {
                lastId = command.Data.InnerCommand.ExecuteScalar();
            });

            return lastId;
        }

        public void OnCommandExecuting(IDbCommand command)
        {
        }

        public string EscapeColumnName(string name)
        {
            if (name.Contains("["))
                return name;
            return "[" + name + "]";
        }
    }

}

