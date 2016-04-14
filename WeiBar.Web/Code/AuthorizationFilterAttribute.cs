// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthorizationFilterAttribute.cs" company="德清女娲网络科技有限公司">
//   Copyright (c) 德清女娲网络科技有限公司 版权所有
// </copyright>
// <summary>
//   权限判断 特性
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WeiBar.BLL;
using WeiBar.Web.Models;

namespace WeiBar.Web.Code
{
    /// <summary>
    /// 权限判断
    /// </summary>
    public class AuthorizationFilterAttribute : ActionFilterAttribute
    {

        /// <summary>
        /// Called by the ASP.NET MVC framework before the action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (ConfigSetting.isDebug)
            {
                if (!LoginHelper.IsLogin())
                {
                    LoginHelper.UserLogin(UserHelper.GetModelByUserID("6c188a35-68c9-45f5-a5df-adfccc357daf"));
                }
                base.OnActionExecuting(filterContext);
                return;
            }
            if (filterContext.HttpContext.Request.UserAgent.ToLower().Contains("micromessenger"))
            {
                #region 微信登陆验证

                if (LoginHelper.IsLogin())
                {
                    base.OnActionExecuting(filterContext);
                    return;
                }
                else
                {
                    string code = filterContext.HttpContext.Request["code"];
                    if (string.IsNullOrEmpty(code))
                    {
                        string url =
                            string.Format(
                                "{0}?appid={1}&redirect_uri={2}&response_type=code&response_type=code&scope=snsapi_base&state=STATE#wechat_redirect"
                                , ConfigSetting.WeiXinauthUrl
                                , ConfigSetting.WeiXinAppID
                                , HttpUtility.UrlEncode(filterContext.HttpContext.Request.Url.AbsoluteUri));
                        filterContext.Result = new RedirectResult(url);
                        return;
                    }
                    //获取Token
                    OAuth_Token token = Get_token1(code);
                    if (token != null)
                    {
                        if (LoginHelper.UserLogin(token))
                        {
                            base.OnActionExecuting(filterContext);
                        }
                        else
                        {
                            filterContext.Result = new ContentResult() { Content = "登录失败" };
                            return;
                        }
                    }
                    else
                    {
                        filterContext.Result = new ContentResult() { Content = "授权失败" };
                        return;
                    }
                }
                #endregion
            }
            else
            {
                filterContext.Result = new ContentResult() { Content = "请在微信里打开" };
            }
            return;
        }

        #region 微信授权


        /// <summary>
        /// 获取授权token
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public OAuth_Token Get_token1(string Code)
        {
            string url = "https://api.weixin.qq.com/sns/oauth2/access_token?appid=" + ConfigSetting.WeiXinAppID + "&secret=" + ConfigSetting.WeiXinAppSecret + "&code=" + Code + "&grant_type=authorization_code ";
            string str = GetJson(url);
            OAuth_Token Oauth_Token_Model = SerializeUtility.JavaScriptDeserialize<OAuth_Token>(str);
            return Oauth_Token_Model;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="REFRESH_TOKEN"></param>
        /// <param name="OPENID"></param>
        /// <returns></returns>
        public OAuthUser Get_UserInfo1(string access_token, string openid)
        {
            string url = "https://api.weixin.qq.com/sns/userinfo?access_token=" + access_token + "&openid=" + openid + "";
            string str = GetJson(url);
            OAuthUser OAuthUser_Model = SerializeUtility.JavaScriptDeserialize<OAuthUser>(str);
            return OAuthUser_Model;
        }


        #endregion

        public string GetJson(string url)
        {
            WebClient wc = new WebClient();
            wc.Credentials = CredentialCache.DefaultCredentials;
            wc.Encoding = Encoding.UTF8;
            string returnText = wc.DownloadString(url);
            if (returnText.Contains("errcode"))
            {
                //可能发生错误returnText={"errcode":40029,"errmsg":"invalid code"} 
            }
            return returnText;

        }

    }
}