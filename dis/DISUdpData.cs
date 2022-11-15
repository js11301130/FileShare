using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class DISUdpData
{
    //收到的字节数据
    public byte[] data;
    //发送方的地址
    public IPEndPoint senderIPEndPoint;

}

public class DISBigPackage
{
    private bool m_bFull;
    private int m_dataSizeReceived;
    private byte[] m_data;
    private string m_pkgId;
    //大包总长度
    private int m_pkgSize;

    public IPEndPoint senderIPEndPoint;

    public DISBigPackage(DISUdpData udpData)
    {
        senderIPEndPoint = udpData.senderIPEndPoint;
        DISByteArray bta = new DISByteArray(udpData.data);
        m_pkgId = bta.ReadString();
        m_pkgSize = bta.ReadInt();
        m_data = new byte[m_pkgSize];
        //
        _addData(bta);
    }

    private void _addData(DISByteArray bta)
    {
        //当前包位置
        int i = bta.ReadInt();
        //当前包内容
        byte[] btaBody = bta.ReadBytes();
        Buffer.BlockCopy(btaBody, 0, m_data, i, btaBody.Length);
        //判断当前包是否已经满了
        m_dataSizeReceived += btaBody.Length;
        m_bFull = m_dataSizeReceived >= m_pkgSize;
    } 
    public bool tryAddData(DISUdpData udpData)
    {
        if (m_bFull)
        {
            return false;
        }
        if (!udpData.senderIPEndPoint.Equals(senderIPEndPoint))
        {
            return false;
        }
        DISByteArray bta = new DISByteArray(udpData.data);
        string pkgId = bta.ReadString();
        if (m_pkgId != pkgId)
        {
            return false;
        }
        int pkgSize = bta.ReadInt();
        if (pkgSize != m_pkgSize)
        {
            return false;
        } 
        _addData(bta);
        return true;
    } 
    public byte[] getData()
    {
        return m_data;
    }

    public bool isFull()
    {
        return m_bFull;
    }

    public int packageSize()
    {
        return m_pkgSize;
    }
}
