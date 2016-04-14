using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using WeiBar.Model;

namespace WeiBar.BLL
{
    public class UserHelper
    {
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public static UserModel GetModelByLoginKey(string loginKey)
        {
            string sql = @"
select * from user where LoginKey=@loginKey limit 1;
";
            MySqlParameter[] _parameter = {
                                        new MySqlParameter("@loginKey", MySqlDbType.VarChar, 36) {Value = loginKey},
                                    };
            var _ds = DbHelperMySqlForWeiBar.Query(sql, _parameter);
            if (_ds.Tables[0].Rows.Count > 0)
            {
                return CollectionHelper.CreateItem<UserModel>(_ds.Tables[0].Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public static UserModel GetModelByUserID(string userid)
        {
            string sql = @"
select * from user where UserID=@UserID limit 1;
";
            MySqlParameter[] _parameter = {
                                        new MySqlParameter("@UserID", MySqlDbType.VarChar, 36) {Value = userid},
                                    };
            var _ds = DbHelperMySqlForWeiBar.Query(sql, _parameter);
            if (_ds.Tables[0].Rows.Count > 0)
            {
                return CollectionHelper.CreateItem<UserModel>(_ds.Tables[0].Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <returns></returns>
        public static bool Add(UserModel model)
        {
            string sql = @"
insert into user
(UserID
,NickName
,CreateTime
,loginKey
)
values
(@UserID
,@NickName
,@CreateTime
,@loginKey
);
";
            MySqlParameter[] _parameter = {
                                        new MySqlParameter("@UserID", MySqlDbType.VarChar, 36) {Value = model.UserID},
                                        new MySqlParameter("@NickName", MySqlDbType.VarChar, 36) {Value = model.NickName},
                                        new MySqlParameter("@CreateTime", MySqlDbType.VarChar, 36) {Value = model.CreateTime},
                                        new MySqlParameter("@loginKey", MySqlDbType.VarChar, 36) {Value = model.LoginKey},
                                    };
            var _rows = DbHelperMySqlForWeiBar.ExecuteSql(sql, _parameter);
            return _rows > 0;
        }

        /// <summary>
        /// 修改用户昵称
        /// </summary>
        /// <returns></returns>
        public static bool UpdateNickName(string UserID, string NickName)
        {
            string sql = @"
update user set NickName=@NickName where UserID=@UserID;
";
            MySqlParameter[] _parameter = {
                                        new MySqlParameter("@UserID", MySqlDbType.VarChar, 36) {Value = UserID},
                                        new MySqlParameter("@NickName", MySqlDbType.VarChar, 36) {Value = NickName},
                                    };
            var _rows = DbHelperMySqlForWeiBar.ExecuteSql(sql, _parameter);
            return _rows > 0;
        }
        /// <summary>
        /// 修改LoginKey
        /// </summary>
        /// <returns></returns>
        public static bool UpdateLoginKey(string UserID, string LoginKey)
        {
            string sql = @"
update user set LoginKey=@LoginKey where UserID=@UserID;
";
            MySqlParameter[] _parameter = {
                                        new MySqlParameter("@UserID", MySqlDbType.VarChar, 36) {Value = UserID},
                                        new MySqlParameter("@LoginKey", MySqlDbType.VarChar, 100) {Value = LoginKey},
                                    };
            var _rows = DbHelperMySqlForWeiBar.ExecuteSql(sql, _parameter);
            return _rows > 0;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <returns></returns>
        public static bool Add(UserModel model, AuthorizeModel authmodel)
        {
            string sql = @"
insert into user
(UserID
,NickName
,CreateTime
,loginKey
)
values
(@UserID
,@NickName
,@CreateTime
,@loginKey
);

-- 授权
insert into authorize
(UserID
,AuthID
,Data
)
values
(@UserID
,@AuthID
,@Data
);

-- 关注官方的几个吧
insert into user_bar
(UserID
,BarID
,CreateTime
,IsFollow
)
values
(@UserID
,1
,@CreateTime
,1
);

";
            MySqlParameter[] _parameter =
            {
                new MySqlParameter("@UserID", MySqlDbType.VarChar, 36) {Value = model.UserID},
                new MySqlParameter("@NickName", MySqlDbType.VarChar, 36) {Value = model.NickName},
                new MySqlParameter("@CreateTime", MySqlDbType.VarChar, 36) {Value = model.CreateTime},
                new MySqlParameter("@loginKey", MySqlDbType.VarChar, 36) {Value = model.LoginKey},
                new MySqlParameter("@AuthID", MySqlDbType.VarChar, 100) {Value = authmodel.AuthID},
                new MySqlParameter("@Data", MySqlDbType.LongText) {Value = authmodel.Data},
            };
            var _rows = DbHelperMySqlForWeiBar.ExecuteSql(sql, _parameter);
            return _rows > 0;
        }

    }
}
