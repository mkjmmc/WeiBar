using System.Linq;
using System.Web.Mvc;
using WeiBar.BLL;
using WeiBar.Web.Code;

namespace WeiBar.Web.Controllers
{
    [AuthorizationFilter]
    public class UserBarController : BaseController
    {
        public ActionResult ListMyFollowed()
        {
            var list = UserBarHelper.GetListFollowed(LoginHelper.LoginUser.UserID);
            // »ñÈ¡°ÉÃû
            var barinfo = BarHelper.GetList(list.Select(m => m.BarID).ToArray());
            return Json(list.Select(m =>
            {
                var bar = barinfo.First(b => b.ID == m.BarID);
                return new
                {
                    m.CreateTime,
                    bar.ID,
                    bar.Name
                };
            }));
        }
    }
}