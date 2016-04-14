using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WeiBar.BLL;
using WeiBar.Model;
using WeiBar.Web.Code;

namespace WeiBar.Web.Controllers
{
    [AuthorizationFilter]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }


        // todo 关注，取消关注
        //public 

        [HttpGet]
        public ActionResult UpdateNickName()
        {
            return View(LoginHelper.LoginUser);
        }

        [HttpPost]
        public ActionResult UpdateNickName(UserModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NickName))
            {
                ModelState.AddModelError("NickName", "请设置昵称");
                return View(model);
            }
            //更新用户昵称
            if (UserHelper.UpdateNickName(LoginHelper.LoginUser.UserID, model.NickName))
            {
                LoginHelper.LoginUser.NickName = model.NickName;
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("NickName", "昵称修改失败");
            return View(model);
        }

    }
}
