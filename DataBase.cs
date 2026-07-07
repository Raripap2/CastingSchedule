using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using Dapper;

namespace MyDataBase
{
    public class DataBase
    {
        protected readonly string _connectionString;

        public DataBase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int ExecuteNonQuery(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new OracleConnection(_connectionString))
            using (var command = new OracleCommand(sql, connection))
            {
                connection.Open();

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(new OracleParameter(param.Key, param.Value ?? DBNull.Value));
                    }
                }

                return command.ExecuteNonQuery();
            }
        }

        public DataTable ExecuteQuery(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new OracleConnection(_connectionString))
            using (var command = new OracleCommand(sql, connection))
            {
                connection.Open();

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(new OracleParameter(param.Key, param.Value ?? DBNull.Value));
                    }
                }

                using (var reader = command.ExecuteReader())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }

        public T ExecuteScalar<T>(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new OracleConnection(_connectionString))
            using (var command = new OracleCommand(sql, connection))
            {
                connection.Open();

                if (parameters != null)
                {
                    foreach (var param in parameters)
                        command.Parameters.Add(new OracleParameter(param.Key, param.Value ?? DBNull.Value));
                }

                object result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return default;

                Type targetType = typeof(T);
                Type underlyingType = Nullable.GetUnderlyingType(targetType);

                if (underlyingType != null)
                {
                    return (T)Convert.ChangeType(result, underlyingType);
                }

                return (T)Convert.ChangeType(result, targetType);
            }
        }

        public T QueryFirstOrDefault<T>(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<T>(sql, parameters);
            }
        }

        public List<T> QueryList<T>(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();

                var dynamicParams = new DynamicParameters();
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        dynamicParams.Add(param.Key, param.Value);
                    }
                }
                return connection.Query<T>(sql, dynamicParams).ToList();
            }
        }
    }
}