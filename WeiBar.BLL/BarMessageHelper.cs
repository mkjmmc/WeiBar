using MySql.Data.MySqlClient;
using WeiBar.Model;

namespace WeiBar.BLL
{
    public class BarMessageHelper
    {
        /// <summary>
        /// Ìí¼Ó
        /// </summary>
        /// <returns></returns>
        public static bool Add(BarMessageModel model)
        {
            string sql = @"
insert into bar_message
(BarID
,UserID
,Content
,CreateTime
,Type
)
values
(@BarID
,@UserID
,@Content
,@CreateTime
,@Type
)
;
";
            MySqlParameter[] _parameter = {
                new MySqlParameter("@BarID", MySqlDbType.Int64) {Value = model.BarID},
                new MySqlParameter("@UserID", MySqlDbType.VarChar, 36) {Value = model.UserID},
                new MySqlParameter("@Content", MySqlDbType.VarChar, 500) {Value = model.Content},
                new MySqlParameter("@CreateTime", MySqlDbType.Int64) {Value = model.CreateTime},
                new MySqlParameter("@Type", MySqlDbType.Int32) {Value = model.Type},
            };
            var _rows = DbHelperMySqlForWeiBar.ExecuteSql(sql, _parameter);
            return _rows > 0;
        }
    }
}