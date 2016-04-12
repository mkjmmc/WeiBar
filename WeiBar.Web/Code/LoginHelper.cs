using System;
using System.Web;
using BuTian.Utility;
using WeiBar.BLL;
using WeiBar.Model;
using WeiBar.Web.Models;

namespace WeiBar.Web.Code
{
    public class LoginHelper
    {
        public static UserModel LoginUser
        {
            get { return HttpContext.Current.Session["SessionLogonUser"] as UserModel; }
            private set { HttpContext.Current.Session["SessionLogonUser"] = value; }
        }

        public static bool IsLogin()
        {
            if (LoginUser == null)
            {
                var _loginkey = HttpContext.Current.Request.Cookies["id"];
                if (_loginkey != null && !string.IsNullOrEmpty(_loginkey.ToString()))
                {
                    return UserLogin(_loginkey.ToString());
                }
            }
            return LoginUser != null;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <returns></returns>
        public static bool UserLogin(OAuth_Token model)
        {
            if (model != null)
            {
                // 获取授权信息
                var _authmodel = AuthorizeHelper.GetModelByAuthID(model.openid);
                if (_authmodel == null)
                {
                    // 新用户注册
                    var user = new UserModel()
                    {
                        CreateTime = DateTimeUtility.GetTimeMilliseconds(DateTime.Now),
                        LoginKey = Guid.NewGuid().ToString(),
                        NickName = "",
                        UserID = Guid.NewGuid().ToString()
                    };
                    if (UserHelper.Add(user, new AuthorizeModel()
                    {
                        AuthID = model.openid,
                        Data = SerializeUtility.JavaScriptSerialize(model),
                    }))
                    {
                        return UserLogin(user);
                    }
                    // 显示添加用户昵称界面：
                    return false;
                }

                // 获取用户信息
                var _user = UserHelper.GetModelByUserID(_authmodel.UserID);
                return UserLogin(_user);
            }
            return false;
        }

        /// <summary>
        /// 快速登陆
        /// </summary>
        /// <param name="loginkey"></param>
        /// <returns></returns>
        public static bool UserLogin(string loginkey)
        {
            if (!string.IsNullOrWhiteSpace(loginkey))
            {
                // 获取用户信息
                var _user = UserHelper.GetModelByLoginKey(loginkey);
                if (_user != null)
                {
                    // 更新LoginKey
                    var newloginkey = Guid.NewGuid().ToString();
                    if (UserHelper.UpdateLoginKey(_user.UserID, newloginkey))
                    {
                        _user.LoginKey = newloginkey;
                    }
                }
                return UserLogin(_user);
            }
            return false;
        }

        public static bool UserLogin(UserModel user)
        {
            if (user != null)
            {
                LoginUser = user;
                // 刷入Cookie
                HttpCookie _cookieuserid = new HttpCookie("id", user.LoginKey)
                {
                    Expires = DateTime.Now.AddDays(10)
                };
                HttpContext.Current.Response.Cookies.Add(_cookieuserid);
                return true;
            }
            return false;
        }

    }
}