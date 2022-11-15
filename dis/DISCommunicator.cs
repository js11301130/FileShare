using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

public class DISCommunicator
{

    /// <summary>
    /// 收到数据的回调
    /// </summary> 
    public delegate void OnDataDelegate(DISCommunicationPackage pkgData);

    /// <summary>
    /// 收到数据的回调委托
    /// </summary>
    public OnDataDelegate onData;
    /// <summary>
    /// 收到ping的反馈,即对方反馈了本机的Ping指令时会调用
    /// </summary>
    public OnDataDelegate onPingResponse;

    private int m_nPkgCounts = 0;
    private UdpClient m_updClient;
    private bool m_bListening = false;
    private string m_strName;
    private IPEndPoint m_myIP;
    private Queue<DISUdpData> m_queueUdpData;
    private List<DISBigPackage> m_packgeForMaking;
    private Queue<DISBigPackage> m_queueForUnpacking;
    private readonly int _UDP_PKG_SIZE = (63 * 1024);

    public DISCommunicator(int port, string strName)
    {
        m_strName = strName;
        m_myIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
        m_updClient = new UdpClient(m_myIP);
        m_bListening = true;
        m_queueUdpData = new Queue<DISUdpData>();
        m_packgeForMaking = new List<DISBigPackage>();
        m_queueForUnpacking = new Queue<DISBigPackage>();
        Task.Factory.StartNew(_receiving);
        Task.Factory.StartNew(_makeBigPackage);
        Task.Factory.StartNew(_unPacking);

    }

    private void _makeBigPackage()
    {
        while (m_bListening)
        {
            //从收取到的队列中取包
            DISUdpData udpData = null;
            lock (m_queueUdpData)
            {
                if (m_queueUdpData.Count > 0)
                {
                    udpData = m_queueUdpData.Dequeue();
                }
            }

            if (udpData == null || udpData.data.Length == 0)
            {
                continue;
            }
            //拼大包
            lock (m_packgeForMaking)
            {
                DISBigPackage currentPkg = null;
                DISUdpData ud = udpData;
                //从已有列表中找到匹配的大包加入数据
                foreach (DISBigPackage pkg in m_packgeForMaking)
                {
                    bool bAdded = pkg.tryAddData(ud);
                    if (bAdded)
                    {
                        currentPkg = pkg;
                        break;
                    }
                }
                //如果在列表中没有找到，则创建新的待建包，加入列表
                if (currentPkg == null)
                {
                    currentPkg = new DISBigPackage(ud);
                    m_packgeForMaking.Add(currentPkg);
                }
                //如果包已经满了，则从列表中移出，放入带解压队列
                if (currentPkg.isFull())
                {
                    m_packgeForMaking.Remove(currentPkg);
                    lock (m_queueForUnpacking)
                    {
                        m_queueForUnpacking.Enqueue(currentPkg);
                    }
                }
            }
        }
    }

    private void _unPacking()
    {
        while (m_bListening)
        {
            DISBigPackage pkgRaw = null;
            lock (m_queueUdpData)
            {
                if (m_queueForUnpacking.Count > 0)
                {
                    pkgRaw = m_queueForUnpacking.Dequeue();
                }
            }
            //
            //解包
            if (pkgRaw == null)
            {
                continue;
            }
            byte[] packageResult = pkgRaw.getData();
            //
            DISByteArray bytes = new DISByteArray(packageResult);
            int pkgLen = bytes.ReadInt();
            DISCommunicationPackage pkgData = new DISCommunicationPackage();
            //发送方ip
            pkgData.senderIp = pkgRaw.senderIPEndPoint.Address.ToString();
            //发送方端口
            pkgData.senderPort = pkgRaw.senderIPEndPoint.Port;
            //发送方名字
            pkgData.senderName = bytes.ReadString();
            //64位，即8字节是时间戳
            pkgData.ts = bytes.ReadLong();
            //通信指令本身
            pkgData.methodName = bytes.ReadString();

            //如果是ping命令则直接回去
            if (pkgData.methodName == "ping_request")
            {
                SendMsg("ping_response", pkgData.senderIp, pkgData.senderPort);
                continue;
            }
            if (pkgData.methodName == "ping_response")
            {
                //如果收到的是ping的反馈,爆出ping反馈了
                if (onPingResponse != null)
                {
                    onPingResponse(pkgData);
                }
                continue;
            }

            //数据体的长度
            int nParamLen = bytes.ReadInt();
            //数据体本身
            pkgData.paramsData = bytes.ReadBytes();

            //爆出数据 
            if (onData != null)
            {
                onData(pkgData);
            }
        }

    }



    /// <summary>
    /// 监听读取数据
    /// </summary>
    async private void _receiving()
    {
        while (m_bListening)
        {

            var res = await m_updClient.ReceiveAsync();
            DISUdpData updData = new DISUdpData();
            updData.data = res.Buffer;
            updData.senderIPEndPoint = res.RemoteEndPoint;
            lock (m_queueUdpData)
            {
                m_queueUdpData.Enqueue(updData);
            }
        }
    }
    /// <summary>
    /// 发送普通指令
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="strTargetIp"></param>
    /// <param name="nPort"></param>
    public void SendMsg(string methodName, string strTargetIp, int nPort)
    {
        DISByteArray btaData = new DISByteArray();
        _packData(ref btaData, methodName);
        _writeData(btaData, strTargetIp, nPort);
        //  btaData.position = 0;
        //   byte[] bta = btaData.ConstData();
        //  m_updClient.Send(bta, bta.Length, strTargetIp, nPort);
    }
    /// <summary>
    /// 发送带参数的命令
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="paraData"></param>
    /// <param name="strTargetIp"></param>
    /// <param name="nPort"></param>
    public void SendMsg(string methodName, DISByteArray paraData, string strTargetIp, int nPort)
    {
        DISByteArray btaData = new DISByteArray();
        _packData(ref btaData, methodName, paraData);
        _writeData(btaData, strTargetIp, nPort);
        //btaData.position = 0;
        //byte[] bta = btaData.ConstData();
        //m_updClient.Send(bta, bta.Length, strTargetIp, nPort);
    }
    /// <summary>
    /// 发送带参数的命令
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="bytesPara"></param>
    /// <param name="strTargetIp"></param>
    /// <param name="nPort"></param>
    public void SendMsg(string methodName, byte[] bytesPara, string strTargetIp, int nPort)
    {
        DISByteArray btaData = new DISByteArray();
        DISByteArray paraData = new DISByteArray(bytesPara);
        _packData(ref btaData, methodName, paraData);
        _writeData(btaData, strTargetIp, nPort);
        //btaData.position = 0;
        //byte[] bta = btaData.ConstData();
        //m_updClient.Send(bta, bta.Length, strTargetIp, nPort);
    }

    private void _writeData(DISByteArray btaData, string strTargetIp, int nPort)
    {
        m_nPkgCounts++;
        //大包拆分发送  生成大包id: 时间+当前端发包计数+总大包长度
        //
        long ts = _GetTimeStamp();
        int tlen = btaData.bytesTotal;
        string pkgIdStr = string.Format("{0}.{1}.{2}", ts, m_nPkgCounts, tlen);
        //大包发送的数据是：大包id，大包总长度，当前包位置，当前包内容
        int cntPkgs = 1 + tlen / _UDP_PKG_SIZE;
        for (int i = 0; i < cntPkgs; i++)
        {
            DISByteArray btaSending = new DISByteArray();
            btaSending.WriteString(pkgIdStr);
            btaSending.WriteInt(tlen);
            btaSending.WriteInt(i * _UDP_PKG_SIZE);
            btaData.position=(i * _UDP_PKG_SIZE);
            btaSending.WriteBytes(btaData.ReadBytes(_UDP_PKG_SIZE));
            m_updClient.Send(btaSending.ConstData(), btaSending.bytesTotal, strTargetIp, nPort);
            //避免发送过快，则暂停一下
           

            //qDebug() << "DISCommunicator::writeDatagram:" << i << "," << btaSending.bytesTotal() << "," << n;
        }
    }

    /// <summary>
    /// 发送一个ping命令看对方是否反馈,可以用来检测目标是否在线
    /// </summary>
    /// <param name="strTargetIp"></param>
    /// <param name="nPort"></param>
    public void Ping(string strTargetIp, int nPort)
    {
        SendMsg("ping_request", strTargetIp, nPort);
    }

    private void _packData(ref DISByteArray packageResult, string methodName)
    {
        _packData(ref packageResult, methodName, new DISByteArray());
    }
    private void _packData(ref DISByteArray packageResult, string methodName, DISByteArray btaData)
    {
        int bodySize = btaData.bytesTotal;
        // 创建打包的时间戳 
        long ts = _GetTimeStamp();
        //创建数据体
        DISByteArray pakcageBody = new DISByteArray();
        //发送方名字
        pakcageBody.WriteString(m_strName);
        //64位，即8字节是时间戳
        pakcageBody.WriteLong(ts);
        //通信指令本身
        pakcageBody.WriteString(methodName);
        //数据体的长度
        pakcageBody.WriteInt(bodySize);
        //数据体本身
        pakcageBody.WriteBytes(btaData);
        //整个包的长度,写在最前面(32位，即4字节),值不包含此4字节，UDP不存在粘包，所以此值暂无用处，改成tcp则需要用此拆包
        int pkgLen = pakcageBody.bytesTotal;
        //先清空用于接收数据的包
        packageResult.Clear();
        packageResult.WriteInt(pkgLen);
        //写入数据体
        packageResult.WriteBytes(pakcageBody);
        pakcageBody.Clear();

    }
    /// <summary>
    /// 获取时间戳
    /// </summary>
    /// <returns></returns>
    private long _GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return (long)ts.TotalMilliseconds;
    }
    /// <summary>
    /// 释放资源 
    /// </summary>
    public void dispose()
    {
        onData = null;
        m_bListening = false;
        m_queueUdpData.Clear();
        m_updClient.Close();

        m_updClient.Dispose();
    }
}
