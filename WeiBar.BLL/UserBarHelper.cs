using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using WeiBar.Model;

namespace WeiBar.BLL
{
    public class UserBarHelper
    {
        /// <summary>
        /// 添加吧
        /// </summary>
        /// <returns></returns>
        public static bool Add(UserBarModel model)
        {
            string sql = @"
insert into user_bar
(UserID
,BarID
,CreateTime
,IsFollow
)
values
(@UserID
,@BarID
,@CreateTime
,@IsFollow
);
";
            MySqlParameter[] _parameter = {
                new MySqlParameter("@UserID", MySqlDbType.VarChar, 36) {Value = model.UserID},
                new MySqlParameter("@BarID", MySqlDbType.Int64) {Value = model.BarID},
                new MySqlParameter("@CreateTime", MySqlDbType.Int64) {Value = model.CreateTime},
                new MySqlParameter("@IsFollow", MySqlDbType.Int32) {Value = model.IsFollow},
            };
            var _rows = DbHelperMySqlForWeiBar.ExecuteSql(sql, _parameter);
            return _rows > 0;
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public static List<UserBarModel> GetListFollowed(string UserID)
        {
            string sql = @"
select * from user_bar
where UserID=@UserID
and IsFollow=1
order by FollowTime desc;
";
            MySqlParameter[] _parameter = {
                new MySqlParameter("@UserID", MySqlDbType.VarChar, 36) {Value = UserID},
            };
            var _ds = DbHelperMySqlForWeiBar.Query(sql, _parameter);
            return CollectionHelper.ConvertTo<UserBarModel>(_ds.Tables[0]).ToList();
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public static UserBarModel GetModel(string UserID, long BarID)
        {
            string sql = @"
select * from user_bar
where UserID=@UserID
and BarID=@BarID
limit 1;
";
            MySqlParameter[] _parameter = {
                new MySqlParameter("@UserID", MySqlDbType.VarChar, 36) {Value = UserID},
                new MySqlParameter("@BarID", MySqlDbType.Int32) {Value = BarID},
            };
            var _ds = DbHelperMySqlForWeiBar.Query(sql, _parameter);
            if (_ds.Tables[0].Rows.Count>0)
            {
                return CollectionHelper.CreateItem<UserBarModel>(_ds.Tables[0].Rows[0]);
            }
            return null;
        }
    }
}