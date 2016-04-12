// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbHelperMySql.cs" company="德清女娲网络科技有限公司">
//   Copyright (c) 德清女娲网络科技有限公司 版权所有
// </copyright>
// <summary>
//   C#操作MYSQL数据库基类
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace Database
{
    /// <summary>
    /// C#操作MYSQL数据库基类
    /// </summary>
    public class DbHelperMySql
    {
        /// <summary>
        /// 本地数据库连接字符串(web.config来配置)
        /// </summary>
        private readonly string connectionString = string.Empty;

        public DbHelperMySql(string connectionString)
        {
            this.connectionString = connectionString;
        }

        #region  执行简单SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string sqlString)
        {
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand _cmd = new MySqlCommand(sqlString, _connection))
                {
                    try
                    {
                        _connection.Open();
                        _cmd.CommandTimeout = 999999999;
                        int _rows = _cmd.ExecuteNonQuery();
                        return _rows;
                    }
                    catch (MySqlException _e)
                    {
                        throw new Exception(_e.Message);
                    }
                    finally
                    {
                        _cmd.Dispose();
                        _connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询条数
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int ExecuteScalarSql(String sql)
        {
            int num = 0;
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand _cmd = new MySqlCommand(sql, _connection))
                {
                    try
                    {
                        _connection.Open();
                        _cmd.CommandTimeout = 999999999;
                        num = (int)_cmd.ExecuteScalar();
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        _cmd.Dispose();
                        _connection.Close();
                    }
                }
            }
            return num;
        }

        /// <summary>
        /// 执行SQL语句，设置命令的执行等待时间
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="times">超时时间</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSqlByTime(string sqlString, int times)
        {
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand _cmd = new MySqlCommand(sqlString, _connection))
                {
                    try
                    {
                        _connection.Open();
                        _cmd.CommandTimeout = times;
                        int _rows = _cmd.ExecuteNonQuery();
                        return _rows;
                    }
                    catch (MySqlException _e)
                    {
                        throw new Exception(_e.Message);
                    }
                    finally
                    {
                        _cmd.Dispose();
                        _connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="sqlStringList">多条SQL语句</param>  
        public void ExecuteSqlTran(List<string> sqlStringList)
        {
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                _connection.Open();
                MySqlCommand _cmd = new MySqlCommand();
                _cmd.Connection = _connection;
                _cmd.CommandTimeout = 999999999;
                MySqlTransaction _tx = _connection.BeginTransaction();
                _cmd.Transaction = _tx;
                try
                {
                    for (int _n = 0; _n < sqlStringList.Count; _n++)
                    {
                        string _strsql = sqlStringList[_n];
                        if (_strsql.Trim().Length > 1)
                        {
                            _cmd.CommandText = _strsql;
                            _cmd.ExecuteNonQuery();
                        }
                    }
                    _tx.Commit();
                }
                catch (MySqlException _e)
                {
                    _tx.Rollback();
                    throw new Exception(_e.Message);
                }
                finally
                {
                    _cmd.Dispose();
                    _connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string sqlString, string content)
        {
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand _cmd = new MySqlCommand(sqlString, _connection))
                {
                    MySqlParameter _myParameter = new MySqlParameter("@content", MySqlDbType.Text);
                    _myParameter.Value = content;
                    _cmd.Parameters.Add(_myParameter);
                    try
                    {
                        _connection.Open();
                        _cmd.CommandTimeout = 999999999;
                        int _rows = _cmd.ExecuteNonQuery();
                        return _rows;
                    }
                    catch (MySqlException _e)
                    {
                        throw new Exception(_e.Message);
                    }
                    finally
                    {
                        _cmd.Dispose();
                        _connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public object ExecuteSqlGet(string sqlString, string content)
        {
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand _cmd = new MySqlCommand(sqlString, _connection))
                {
                    MySqlParameter _myParameter = new MySqlParameter("@content", MySqlDbType.Text);
                    _myParameter.Value = content;
                    _cmd.Parameters.Add(_myParameter);
                    try
                    {
                        _connection.Open();
                        _cmd.CommandTimeout = 999999999;
                        object obj = _cmd.ExecuteScalar();
                        if (object.Equals(obj, null) || object.Equals(obj, System.DBNull.Value))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (MySqlException _e)
                    {
                        throw new Exception(_e.Message);
                    }
                    finally
                    {
                        _cmd.Dispose();
                        _connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand _cmd = new MySqlCommand(strSQL, _connection))
                {
                    MySqlParameter _myParameter = new MySqlParameter("@fs", MySqlDbType.Binary);
                    _myParameter.Value = fs;
                    _cmd.Parameters.Add(_myParameter);
                    try
                    {
                        _connection.Open();
                        _cmd.CommandTimeout = 999999999;
                        int _rows = _cmd.ExecuteNonQuery();
                        return _rows;
                    }
                    catch (MySqlException _e)
                    {
                        throw new Exception(_e.Message);
                    }
                    finally
                    {
                        _cmd.Dispose();
                        _connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="sqlString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string sqlString)
        {
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand _cmd = new MySqlCommand(sqlString, _connection))
                {
                    try
                    {
                        _connection.Open();
                        _cmd.CommandTimeout = 999999999;
                        object _obj = _cmd.ExecuteScalar();
                        if (object.Equals(_obj, null) || object.Equals(_obj, System.DBNull.Value))
                        {
                            return null;
                        }
                        else
                        {
                            return _obj;
                        }
                    }
                    catch (MySqlException _e)
                    {
                        throw new Exception(_e.Message);
                    }
                    finally
                    {
                        _cmd.Dispose();
                        _connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回MySqlDataReader(使用该方法切记要手工关闭MySqlDataReader和连接)
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public MySqlDataReader ExecuteReader(string strSQL)
        {
            MySqlConnection _connection = new MySqlConnection(connectionString);
            MySqlCommand _cmd = new MySqlCommand(strSQL, _connection);
            try
            {
                _connection.Open();
                _cmd.CommandTimeout = 999999999;
                MySqlDataReader _myReader = _cmd.ExecuteReader();
                return _myReader;
            }
            catch (MySqlException _e)
            {
                throw new Exception(_e.Message);
            }
            ////finally //不能在此关闭，否则，返回的对象将无法使用
            ////{
            //// cmd.Dispose();
            //// connection.Close();
            ////} 
        }
       
        /// <summary>
        /// 执行查询语句，返回int
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>int</returns>
        public int ExecuteReaderCount(string strSQL)
        {
            int RecordCount = 0;
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand(strSQL, connection);
            try
            {
                connection.Open();
                cmd.CommandTimeout = 999999999;
                MySqlDataReader myReader = cmd.ExecuteReader();
                if (myReader.Read())
                    RecordCount = int.Parse(myReader[0].ToString());
                myReader.Close();
                cmd.Dispose();
                connection.Close();
                return RecordCount;
            }
            catch (MySqlException e)
            {
                throw new Exception(e.Message);
            }
        }


        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string sqlString)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(sqlString, connection);
                    da.Fill(ds);
                }
                catch (MySqlException ex)
                {
                    connection.Close();
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                return ds;
            }
        }

        /// <summary>
        /// 执行查询语句，返回DataSet,设置命令的执行等待时间
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <param name="times">超时时间</param>
        /// <returns>结果集</returns>
        public DataSet Query(string sqlString, int times)
        {
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    _connection.Open();
                    MySqlDataAdapter _command = new MySqlDataAdapter(sqlString, _connection);
                    _command.SelectCommand.CommandTimeout = times;
                    _command.Fill(ds, "ds");
                }
                catch (MySqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    _connection.Close();
                }
                return ds;
            }
        }

        /// <summary>
        /// 获取SQL查询记录条数
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <returns>记录条数</returns>
        public int GetRowsNum(string sqlString)
        {
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    _connection.Open();
                    MySqlDataAdapter _command = new MySqlDataAdapter(sqlString, _connection);
                    _command.Fill(ds, "ds");
                    return ds.Tables[0].Rows.Count;
                }
                catch (MySqlException _ex)
                {
                    throw new Exception(_ex.Message);
                }
                finally
                {
                    _connection.Close();
                }
            }
        }

        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string sqlString, params object[] cmdParms)
        {
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand _cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(_cmd, _connection, null, sqlString, cmdParms);
                        int rows = _cmd.ExecuteNonQuery();
                        _cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (MySqlException _e)
                    {
                        throw new Exception(_e.Message);
                    }
                    finally
                    {
                        _cmd.Dispose();
                        _connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="sqlStringList">SQL语句的哈希表（key为sql语句，value是该语句的 Object[]）</param>
        public void ExecuteSqlTran(Hashtable sqlStringList)
        {
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                _connection.Open();
                using (MySqlTransaction trans = _connection.BeginTransaction())
                {
                    MySqlCommand cmd = new MySqlCommand();
                    try
                    {
                        // 循环
                        foreach (DictionaryEntry myDE in sqlStringList)
                        {
                            string cmdText = myDE.Key.ToString();
                            object[] cmdParms = (Object[])myDE.Value;
                            PrepareCommand(cmd, _connection, trans, cmdText, cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();

                            trans.Commit();
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                    finally
                    {
                        cmd.Dispose();
                        _connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="al">SQL语句的哈希表（key为sql语句，value是该语句的 Object[]）</param>
        public int ExecuteSqlTran2(List<SqlItem> al)
        {
            int i = 0;
            using (MySqlConnection _connection = new MySqlConnection(connectionString))
            {
                _connection.Open();
                using (MySqlTransaction trans = _connection.BeginTransaction())
                {
                    MySqlCommand cmd = new MySqlCommand();
                    try
                    {
                        // 循环
                        foreach (SqlItem _item in al)
                        {
                            PrepareCommand(cmd, _connection, trans, _item.sql, _item.cmdParms);
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                        i = 1;
                    }
                    catch
                    {
                        i = 0;
                        trans.Rollback();
                    }
                    finally
                    {
                        cmd.Dispose();
                        _connection.Close();
                    }
                }
            }
            return i;
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="sqlString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string sqlString, params object[] cmdParms)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((object.Equals(obj, null)) || (object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (MySqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="sqlString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string sqlString, params MySqlParameter[] cmdParms)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((object.Equals(obj, null)) || (object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (MySqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回MySqlDataReader (使用该方法切记要手工关闭MySqlDataReader和连接)
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public MySqlDataReader ExecuteReader(string SQLString, params object[] cmdParms)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                MySqlDataReader myReader = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (MySqlException e)
            {
                throw new Exception(e.Message);
            }
            //finally //不能在此关闭，否则，返回的对象将无法使用
            //{
            // cmd.Dispose();
            // connection.Close();
            //} 

        }
      
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string sqlString, params object[] cmdParms)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand();
                PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (MySqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                    return ds;
                }
            }
        }

        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, object[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandTimeout = 999999999;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text; //cmdType;
            if (cmdParms != null)
            {


                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }
        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandTimeout = 999999999;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text; //cmdType;
            if (cmdParms != null)
            {


                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }
        #endregion

        #region 存储过程操作

        /// <summary>
        /// 执行存储过程  (使用该方法切记要手工关闭MySqlDataReader和连接)
        /// 手动关闭不了，所以少用，MySql.Data组组件还没解决该问题
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>MySqlDataReader</returns>
        public MySqlDataReader RunProcedure(string storedProcName, object[] parameters)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlDataReader returnReader;
            connection.Open();
            MySqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 999999999;
            returnReader = command.ExecuteReader();
            //Connection.Close(); 不能在此关闭，否则，返回的对象将无法使用            
            return returnReader;

        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public DataSet RunProcedure(string storedProcName, object[] parameters, string tableName)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }

        public DataSet RunProcedure(string storedProcName, object[] parameters, string tableName, int Times)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                DataSet dataSet = new DataSet();
                connection.Open();
                MySqlDataAdapter sqlDA = new MySqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                sqlDA.SelectCommand.CommandTimeout = Times;
                sqlDA.Fill(dataSet, tableName);
                connection.Close();
                return dataSet;
            }
        }

        /// <summary>
        /// 执行存储过程 
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns></returns>
        public void RunProcedureNull(string storedProcName, object[] parameters)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {

                connection.Open();
                MySqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                command.CommandTimeout = 999999999;
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="CommandText">T-SQL语句；例如："pr_shell 'dir *.exe'"或"select * from sysobjects where xtype=@xtype"</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>返回第一行第一列</returns>
        public object ExecuteScaler(string storedProcName, object[] parameters)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                object returnObjectValue;
                connection.Open();
                MySqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
                command.CommandTimeout = 999999999;
                returnObjectValue = command.ExecuteScalar();
                connection.Close();
                return returnObjectValue;
            }
        }

        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand</returns>
        private static MySqlCommand BuildQueryCommand(MySqlConnection connection, string storedProcName, object[] parameters)
        {
            MySqlCommand command = new MySqlCommand(storedProcName, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 999999999;
            foreach (MySqlParameter parameter in parameters)
            {
                if (parameter != null)
                {
                    // 检查未分配值的输出参数,将其分配以DBNull.Value.
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        /// <summary>
        /// 创建 MySqlCommand 对象实例(用来返回一个整数值) 
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>MySqlCommand 对象实例</returns>
        private static MySqlCommand BuildIntCommand(MySqlConnection connection, string storedProcName, object[] parameters)
        {
            MySqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandTimeout = 999999999;
            command.Parameters.Add(new MySqlParameter("ReturnValue",
                                                      MySqlDbType.Int32, 4, ParameterDirection.ReturnValue,
                                                      false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }

        #endregion

        /// <summary>
        /// MySqlDataReader转换成DataTable
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static DataTable GetNewDataTable(MySqlDataReader dataReader)
        {
            DataTable datatable = new DataTable();
            DataTable schemaTable = dataReader.GetSchemaTable();

            //动态添加列
            try
            {
                foreach (DataRow myRow in schemaTable.Rows)
                {
                    DataColumn myDataColumn = new DataColumn();
                    myDataColumn.DataType = myRow.GetType();
                    myDataColumn.ColumnName = myRow[0].ToString();
                    datatable.Columns.Add(myDataColumn);
                }
                //添加数据
                while (dataReader.Read())
                {
                    DataRow myDataRow = datatable.NewRow();
                    for (int i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        myDataRow[i] = dataReader[i].ToString();
                    }
                    datatable.Rows.Add(myDataRow);
                    myDataRow = null;
                }
                schemaTable = null;
                dataReader.Close();
                return datatable;
            }
            catch (Exception ex)
            {
                throw new Exception("转换出错出错!", ex);
            }
        }
    }

    public class SqlItem
    {
        /// <summary>
        /// SQL语句
        /// </summary>
        public string sql { get; set; }

        /// <summary>
        /// 参数列表
        /// </summary>
        public MySqlParameter[] cmdParms { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlItem"/> class. 
        /// 构造
        /// </summary>
        /// <param name="sql">
        /// SQL语句
        /// </param>
        /// <param name="cmdParms">
        /// 参数列表
        /// </param>
        public SqlItem(string sql, MySqlParameter[] cmdParms)
        {
            this.sql = sql;
            this.cmdParms = cmdParms;
        }
        public SqlItem()
        {
        }
    }
}