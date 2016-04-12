using MySql.Data.MySqlClient;
using WeiBar.Model;

namespace WeiBar.BLL
{
    public class AuthorizeHelper
    {
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public static AuthorizeModel GetModelByAuthID(string AuthID)
        {
            string sql = @"
select * from `authorize` where `AuthID`=@AuthID limit 1;
";
            MySqlParameter[] _parameter =
            {
                new MySqlParameter("@AuthID", MySqlDbType.VarChar, 100) {Value = AuthID},
            };
            var _ds = DbHelperMySqlForWeiBar.Query(sql, _parameter);
            if (_ds.Tables[0].Rows.Count > 0)
            {
                return CollectionHelper.CreateItem<AuthorizeModel>(_ds.Tables[0].Rows[0]);
            }
            return null;
        }
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <returns></returns>
        public static bool Add(AuthorizeModel model)
        {
            string sql = @"
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
";
            MySqlParameter[] _parameter = {
                new MySqlParameter("@UserID", MySqlDbType.VarChar, 36) {Value = model.UserID},
                new MySqlParameter("@AuthID", MySqlDbType.VarChar, 100) {Value = model.AuthID},
                new MySqlParameter("@Data", MySqlDbType.LongText) {Value = model.Data},
            };
            var _rows = DbHelperMySqlForWeiBar.ExecuteSql(sql, _parameter);
            return _rows > 0;
        }
    }
}