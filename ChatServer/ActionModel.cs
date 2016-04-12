namespace ChatServer
{
    /// <summary>
    /// 消息模型结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActionModel<T>
    {
        public string action { get; set; }
        public T data { get; set; }
    }
}