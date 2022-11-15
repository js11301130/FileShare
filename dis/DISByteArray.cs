using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DISByteArray
{

    private int m_nPos = 0;
    private byte[] m_btaData;

    /// <summary>
    /// 当前数据头位置，读数据会是位置往后移动，写数据会使得位置直接到末尾
    /// </summary>
    public int position
    {
        get => m_nPos;
        set
        {
            if (value < 0)
            {
                m_nPos = 0;
            }
            else if (value > bytesTotal)
            {
                m_nPos = bytesTotal;
            }
            else
            {
                m_nPos = value;
            }

        }
    }


    /// <summary>
    /// 总字节数
    /// </summary>
    public int bytesTotal
    {
        get
        {
            if (m_btaData == null)
            {
                return 0;
            }
            return m_btaData.Length;
        }
    }

    /// <summary>
    /// 可以读取的字节数
    /// </summary>
    public int bytesAvarible
    {
        get
        {
            return bytesTotal - m_nPos;
        }
    }





    public DISByteArray()
    {
        SetData(null);
    }
    public DISByteArray(byte[] bta)
    {
        SetData(bta);
    }
    /// <summary>
    /// 设置数据，设置null即可清空数据
    /// </summary>
    /// <param name="bta"></param>
    public void SetData(byte[] bta)
    {
        m_nPos = 0;
        if (bta == null)
        {
            m_btaData = new byte[0];
        }
        else
        {
            m_btaData = bta;
        }
    }
    /// <summary>
    /// 返回原始数据的一个副本
    /// </summary>
    /// <returns></returns>
    public byte[] ConstData()
    {
        if (m_btaData == null)
        {
            return null;
        }
        byte[] bta = new byte[m_btaData.Length];
        m_btaData.CopyTo(bta, 0);
        return bta;
    }

    /// <summary>
    /// 浅拷贝，共享一个byte[]
    /// </summary>
    /// <returns></returns>
    public DISByteArray ShallowCopy()
    {
        return new DISByteArray(m_btaData);
    }
    /// <summary>
    /// 清空
    /// </summary>
    public void Clear()
    {
        m_nPos = 0;
        Array.Resize(ref m_btaData, 0);

    }

    //======================读区域===================
    /// <summary>
    /// 读取一个byte
    /// </summary>
    /// <returns></returns>
    public byte ReadByte()
    {
        byte n = m_btaData[m_nPos];
        position += 1;
        return n;
    }
    /// <summary>
    /// 读取一个UInt32的数据
    /// </summary>
    /// <returns></returns>
    public uint ReadUInt()
    {
        uint n = BitConverter.ToUInt32(m_btaData, m_nPos);
        position += sizeof(uint);
        return n;
    }

    /// <summary>
    /// 读取一个int32数据
    /// </summary>
    /// <returns></returns>
    public int ReadInt()
    {
        int n = BitConverter.ToInt32(m_btaData, m_nPos);
        position += sizeof(int);
        return n;
    }

    /// <summary>
    /// 读取一个short数据
    /// </summary>
    /// <returns></returns>
    public short ReadShort()
    {
        short n = BitConverter.ToInt16(m_btaData, m_nPos);
        position += sizeof(short);
        return n;
    }
    /// <summary>
    /// 读取一个ushort数据
    /// </summary>
    /// <returns></returns>
    public ushort ReadUShort()
    {
        ushort n = BitConverter.ToUInt16(m_btaData, m_nPos);
        position += sizeof(ushort);
        return n;
    }


    /// <summary>
    /// 读取一个ULong数据
    /// </summary>
    /// <returns></returns>
    public ulong ReadULong()
    {
        ulong n = BitConverter.ToUInt64(m_btaData, m_nPos);
        position += sizeof(ulong);
        return n;
    }



    /// <summary>
    /// 读取一个long
    /// </summary> 
    /// <returns></returns>
    public long ReadLong()
    {
        long n = BitConverter.ToInt64(m_btaData, m_nPos);
        position += sizeof(long);
        return n;
    }
    /// <summary>
    /// 读取一个float
    /// </summary> 
    /// <returns></returns>
    public float ReadFloat()
    {
        float n = BitConverter.ToSingle(m_btaData, m_nPos);
        position += sizeof(float);
        return n;
    }
    /// <summary>
    /// 读取一个double
    /// </summary> 
    /// <returns></returns>
    public double ReadDouble()
    {
        double n = BitConverter.ToDouble(m_btaData, m_nPos);
        position += sizeof(double);
        return n;
    }

    /// <summary>
    /// 读取一个boolean
    /// </summary> 
    /// <returns></returns>
    public bool ReadBoolean()
    {
        bool n = BitConverter.ToBoolean(m_btaData, m_nPos);
        position += sizeof(bool);
        return n;
    }


    /// <summary>
    /// 读取一个字符串
    /// </summary>
    /// <returns></returns>
    public string ReadString()
    {
        int len = ReadInt();
        string str = Encoding.UTF8.GetString(m_btaData, m_nPos, len);
        position += len;
        return str;

    }

    /// <summary>
    /// 读取指定长度的字节
    /// </summary>
    /// <param name="len"></param>
    /// <returns></returns>
    public byte[] ReadBytes(int len)
    {
        if(len>bytesAvarible)
        {
            len = bytesAvarible;
        }
        byte[] bta = new byte[len];
        Array.Copy(m_btaData, m_nPos, bta, 0, len);
        return bta;
    }
    /// <summary>
    /// 读出全部剩余字节
    /// </summary>
    /// <returns></returns>
    public byte[] ReadBytes()
    {
        return ReadBytes(bytesAvarible);
    }

    //===============写区域=================

    /// <summary>
    /// 在末尾写入一个byte
    /// </summary>
    /// <returns></returns>
    public void WriteByte(byte n)
    {
        int pos = m_btaData.Length;
        Array.Resize(ref m_btaData, pos + 1);
        m_btaData[pos] = n;
        m_nPos = m_btaData.Length;

    }
    /// <summary>
    /// 在末尾写入一个UInt32的数据
    /// </summary>
    /// <returns></returns>
    public void WriteUInt(uint n)
    {
        byte[] btas = BitConverter.GetBytes(n);
        WriteBytes(btas);

    }

    /// <summary>
    /// 在末尾写入一个int32数据
    /// </summary>
    /// <returns></returns>
    public void WriteInt(int n)
    {
        byte[] btas = BitConverter.GetBytes(n);
        WriteBytes(btas);
    }

    /// <summary>
    /// 在末尾写入一个short数据
    /// </summary>
    /// <returns></returns>
    public void WriteShort(short n)
    {
        byte[] btas = BitConverter.GetBytes(n);
        WriteBytes(btas);
    }
    /// <summary>
    /// 在末尾写入一个ushort数据
    /// </summary>
    /// <returns></returns>
    public void WriteUShort(ushort n)
    {
        byte[] btas = BitConverter.GetBytes(n);
        WriteBytes(btas);
    }


    /// <summary>
    /// 在末尾写入一个ULong数据
    /// </summary>
    /// <returns></returns>
    public void WriteULong(ulong n)
    {
        byte[] btas = BitConverter.GetBytes(n);
        WriteBytes(btas);
    }
    /// <summary>
    /// 在末尾写入一个long
    /// </summary> 
    /// <returns></returns>
    public void WriteLong(long n)
    {
        byte[] btas = BitConverter.GetBytes(n);
        WriteBytes(btas);
    }
    /// <summary>
    /// 在末尾写入一个float
    /// </summary> 
    /// <returns></returns>
    public void WriteFloat(float n)
    {
        byte[] btas = BitConverter.GetBytes(n);
        WriteBytes(btas);
    }
    /// <summary>
    /// 在末尾写入一个double
    /// </summary> 
    /// <returns></returns>
    public void WriteDouble(double n)
    {
        byte[] btas = BitConverter.GetBytes(n);
        WriteBytes(btas);
    }

    /// <summary>
    /// 在末尾写入一个boolean
    /// </summary> 
    /// <returns></returns>
    public void WriteBoolean(bool n)
    {
        byte[] btas = BitConverter.GetBytes(n);
        WriteBytes(btas);
    }


    /// <summary>
    /// 在末尾写入一个字符串
    /// </summary> 
    /// <returns></returns>
    public void WriteString(string str)
    {
        byte[] btas = Encoding.UTF8.GetBytes(str);
        WriteInt(btas.Length);
        WriteBytes(btas);
    }

    /// <summary>
    /// 在末尾写入指定长度的字节
    /// </summary> 
    /// <returns></returns>
    public void WriteBytes(byte[] btas)
    {
        if (btas == null)
        {
            return;
        }
        int pos = m_btaData.Length;
        Array.Resize(ref m_btaData, pos + btas.Length);
        Array.Copy(btas, 0, m_btaData, pos, btas.Length);
        m_nPos = m_btaData.Length;
    }
    /// <summary>
    /// 在末尾写入指定长度的字节
    /// </summary> 
    /// <returns></returns>
    public void WriteBytes(DISByteArray btas)
    {
        if (btas != null)
        {
            btas.WriteTo(this);
        }
        m_nPos = m_btaData.Length;
    }
    /// <summary>
    /// 将自己的全部数据写入另一个DISByteArray
    /// </summary> 
    /// <returns></returns>
    public void WriteTo(DISByteArray target)
    {
        target.WriteBytes(m_btaData);
    }


}

