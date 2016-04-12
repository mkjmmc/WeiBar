using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using Database;

namespace WeiBar.BLL
{
    public class DbHelperMySqlForWeiBar
    {
        private static string connectionString = ConfigurationManager.AppSettings["WeiBarConncetion"];
        private static DbHelperMySql dbHelperMySql = new DbHelperMySql(connectionString);

        public static DataSet Query(string sqlString)
        {
            return dbHelperMySql.Query(sqlString, null);
        }
        public static DataSet Query(string sqlString, params object[] cmdParms)
        {
            return dbHelperMySql.Query(sqlString, cmdParms);
        }
        public static int ExecuteSql(string sqlString, params object[] cmdParms)
        {
            return dbHelperMySql.ExecuteSql(sqlString, cmdParms);
        }
        public static int ExecuteSqlTran2(List<SqlItem> al)
        {
            return dbHelperMySql.ExecuteSqlTran2(al);
        }
    }
}
