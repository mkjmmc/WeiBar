using ChatServer;

namespace PeiduiServer
{
    /// <summary>
    /// �������ݽṹ��
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