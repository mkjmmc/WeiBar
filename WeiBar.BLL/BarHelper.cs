using MySql.Data.MySqlClient;
using WeiBar.Model;

namespace WeiBar.BLL
{
    public class BarHelper
    {
        /// <summary>
        /// 获取吧信息
        /// </summary>
        /// <returns></returns>
        public static BarModel GetModelByName(string Name)
        {
            string sql = @"
select * from bar where Name=@Name limit 1;
";
            MySqlParameter[] _parameter = {
                new MySqlParameter("@Name", MySqlDbType.VarChar, 100) {Value = Name},
            };
            var _ds = DbHelperMySqlForWeiBar.Query(sql, _parameter);
            if (_ds.Tables[0].Rows.Count > 0)
            {
                return CollectionHelper.CreateItem<BarModel>(_ds.Tables[0].Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// 添加吧
        /// </summary>
        /// <returns></returns>
        public static bool Add(BarModel model)
        {
            string sql = @"
insert into bar
(Name)
values
(@Name);
select @@identity;
";
            MySqlParameter[] _parameter = {
                                        new MySqlParameter("@Name", MySqlDbType.VarChar, 100) {Value = model.Name},
                                    };
            var _ds = DbHelperMySqlForWeiBar.Query(sql, _parameter);
            if (_ds.Tables[0].Rows.Count > 0)
            {
                model.ID = long.Parse(_ds.Tables[0].Rows[0][0].ToString());
                return true;
            }
            return false;
        }


    }
}