﻿using System.Linq;
using System.Web.Mvc;
using WeiBar.BLL;
using WeiBar.Model;
using WeiBar.Web.Code;

namespace WeiBar.Web.Controllers
{
    [AuthorizationFilter]
    public class BarController : BaseController
    {
        public ActionResult Index(string id)
        {
            // 查询吧信息
            var bar = BarHelper.GetModelByName(id);
            if (bar == null)
            {
                // 吧不存在，创建一个
                bar = new BarModel()
                {
                    Name = id
                };
                BarHelper.Add(bar);
            }
            return View(bar);
        }

        /// <summary>
        /// 获取消息列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetMessageList(long id)
        {
            var list = BarMessageHelper.GetList(id).OrderBy(m=>m.MessageID).ToList();
            return Json(list);
        }
    }
}