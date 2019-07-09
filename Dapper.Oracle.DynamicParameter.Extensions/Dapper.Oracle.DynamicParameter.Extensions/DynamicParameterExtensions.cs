using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Dynamic;
using Oracle.ManagedDataAccess.Types;

namespace Dapper.Oracle.DynamicParameter.Extensions
{
    /// <summary>
    /// DynamicParameterの拡張クラス
    /// </summary>
    /// <remarks>
    /// https://gist.github.com/ttlatex/10ce7f2cd1f44815727925f4474dd34c
    /// </remarks>
    public static class DynamicParameterExtensions
    {
        /// <summary>
        /// int型のValueを取得する。
        /// </summary>
        /// <param name="parameters">OracleDynamicParameters</param>
        /// <param name="parameterName">パラメータ名</param>
        /// <returns>Value</returns>
        public static int GetIntValue(this OracleDynamicParameters parameters, string parameterName)
            => parameters.Get<OracleDecimal>(parameterName).ToInt32();

        /// <summary>
        /// string型のValueを取得する。
        /// </summary>
        /// <param name="parameters">OracleDynamicParameters</param>
        /// <param name="parameterName">パラメータ名</param>
        /// <returns>Value</returns>
        public static string GetStringValue(this OracleDynamicParameters parameters, string parameterName)
        {
            var value = parameters.Get<OracleString>(parameterName);
            return (value.IsNull) ? "" : value.ToString();
        }

        /// <summary>
        /// RefCursor型のValueを取得する。
        /// </summary>
        /// <typeparam name="T">カーソルをマップするクラスの型</typeparam>
        /// <param name="parameters">OracleDynamicParameters</param>
        /// <param name="parameterName">パラメータ名</param>
        /// <returns>Value</returns>
        public static List<T> GetRefCursorValue<T>(this OracleDynamicParameters parameters, string parameterName)
        {
            var value = parameters.Get<OracleRefCursor>(parameterName);

            if (value.IsNull)
                return new List<T>();

            var reader = value.GetDataReader();
            var columnNames = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToList(); //受信したカーソルの列名

            var result = new List<T>();
            while (reader.Read())
            {
                var t = Activator.CreateInstance<T>();
                foreach (var p in typeof(T).GetProperties())
                    if (columnNames.Any(c => c == p.Name))
                        p.SetValue(t, reader[p.Name]);

                result.Add(t);
            }

            return result;
        }
    }
}
