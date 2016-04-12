using ChatServer;

namespace PeiduiServer
{
    /// <summary>
    /// 返回数据结构体
    /// </summary>
    public class ReturnMessage
    {
        //public ReturnMessage(CMD cmd, string message)
        //{
        //    this.CMD = cmd.ToString();
        //    this.Message = message;
        //}

        public string CMD { get; set; }
        public string Message { get; set; }
    }
}