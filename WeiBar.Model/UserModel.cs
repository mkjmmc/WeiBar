using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeiBar.Model
{
    public class UserModel
    {
        public string UserID { get; set; }
        public string NickName { get; set; }
        public long CreateTime { get; set; }
        public string LoginKey { get; set; }
    }
}
