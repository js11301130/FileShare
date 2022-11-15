using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 /// <summary>
 /// 通信包
 /// </summary>
public class DISCommunicationPackage
{
    /// <summary>
    /// 发送方名称
    /// </summary>
    public string senderName;
    /// <summary>
    /// 发送方ip(保留字段),固定为127.0.0.1
    /// </summary>
    public string senderIp;
    /// <summary>
    /// 发送方端口
    /// </summary>
    public int senderPort;
    /// <summary>
    /// 时间戳
    /// </summary>
    public long ts; 
    /// <summary>
    /// 需要执行的指令
    /// </summary>
    public string methodName;
    /// <summary>
    /// 指令需要的额外参数,可以配合DISByteArray解析
    /// </summary>
    public byte[] paramsData;

}
 
