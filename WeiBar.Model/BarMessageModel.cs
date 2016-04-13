using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeiBar.Model
{
    public class BarMessageModel
    {
        public long MessageID { get; set; }
        public long BarID { get; set; }
        public string UserID { get; set; }
        public string Content { get; set; }
        public long CreateTime { get; set; }
        public EnumBarMessageType Type { get; set; }
    }

    public enum EnumBarMessageType
    {
        系统消息 = 0,
        文字消息 = 1,
    }
}
