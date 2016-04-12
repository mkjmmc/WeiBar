using System.Web.Mvc;

namespace WeiBar.Web.Code
{
    public class NickNameCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (LoginHelper.IsLogin())
            {
                if (filterContext.RouteData.Values["action"].ToString() != "UpdateNickName"
                    && string.IsNullOrWhiteSpace(LoginHelper.LoginUser.NickName))
                {
                    filterContext.Result =  new RedirectResult("~/Home/UpdateNickName");
                }
            }
        }
    }
}