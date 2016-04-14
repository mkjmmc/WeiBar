using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using WeiBar.Model;

namespace WeiBar.BLL
{
    public class BarMessageHelper
    {
        /// <summary>
        /// 添加
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

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public static List<BarMessageModel> GetList(long BarID)
        {
            string sql = @"
select * from bar_message 
where BarID=@BarID and Type=1
order by MessageID desc
limit 50;
";
            MySqlParameter[] _parameter = {
                new MySqlParameter("@BarID", MySqlDbType.Int64) {Value = BarID},
            };
            var _ds = DbHelperMySqlForWeiBar.Query(sql, _parameter);
            return CollectionHelper.ConvertTo<BarMessageModel>(_ds.Tables[0]).ToList();
        }
    }
}