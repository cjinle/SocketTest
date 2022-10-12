using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

public enum DisType
{
    Exception,
    Disconnect,
}

public class SocketClient
{
    private TcpClient client = null;
    private NetworkStream outStream = null;
    private MemoryStream memStream;
    private BinaryReader reader;

    private const int MAX_READ = 8192;
    private byte[] byteBuffer = new byte[MAX_READ];
    public static bool loggedIn = false;

    public NetMgr netMgr = null;

    // Use this for initialization
    public SocketClient()
    {
    }

    public TcpClient GetClient()
    {
        return client;
    }


    public void OnRegister()
    {
        memStream = new MemoryStream();
        reader = new BinaryReader(memStream);
    }


    public void OnRemove()
    {
        this.Close();
        reader.Close();
        memStream.Close();
    }


    void ConnectServer(string host, int port)
    {
        client = null;
        client = new TcpClient();
        client.SendTimeout = 1000;
        client.ReceiveTimeout = 1000;
        client.NoDelay = true;
        try
        {
            client.BeginConnect(host, port, new AsyncCallback(OnConnect), null);
        }
        catch (Exception e)
        {
            Close(); Debug.LogError(e.Message);
        }
    }


    void OnConnect(IAsyncResult asr)
    {
        Debug.Log("Connect Suc");
        outStream = client.GetStream();
        client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
    }


    void WriteMessage(byte[] message)
    {
        MemoryStream ms = null;
        using (ms = new MemoryStream())
        {
            ms.Position = 0;
            BinaryWriter writer = new BinaryWriter(ms);
            ushort msglen = (ushort)message.Length;
            //writer.Write(msglen);
            writer.Write(message);
            writer.Flush();
            if (client != null && client.Connected)
            {
                //NetworkStream stream = client.GetStream(); 
                byte[] payload = ms.ToArray();
                outStream.BeginWrite(payload, 0, payload.Length, new AsyncCallback(OnWrite), null);
            }
            else
            {
                Debug.LogError("client.connected----->>false");
            }
        }
    }

    void OnRead(IAsyncResult asr)
    {
        OnReceivedMessage(byteBuffer);
        lock (client.GetStream())
        {         //分析完，再次监听服务器发过来的新消息
            Array.Clear(byteBuffer, 0, byteBuffer.Length);   //清空数组
            client.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
        }
    }

    void OnDisconnected(DisType dis, string msg)
    {
        Close();   //关掉客户端链接
        Debug.LogError("Connection was closed by the server:>" + msg + " Distype:>" + dis);
    }


    void PrintBytes()
    {
        string returnStr = string.Empty;
        for (int i = 0; i < byteBuffer.Length; i++)
        {
            returnStr += byteBuffer[i].ToString("X2");
        }
        Debug.LogError(returnStr);
    }

    void OnWrite(IAsyncResult r)
    {
        try
        {
            outStream.EndWrite(r);
        }
        catch (Exception ex)
        {
            Debug.LogError("OnWrite--->>>" + ex.Message);
        }
    }

    void OnReceive(byte[] bytes, int length)
    {
        memStream.Seek(0, SeekOrigin.End);
        memStream.Write(bytes, 0, length);
    }

    private long RemainingBytes()
    {
        return memStream.Length - memStream.Position;
    }


    void OnRecieveMessageDeal(ByteBuffer buffer, uint len = 0)
    {
        uint length;
        uint mainId;
        if (len != 0)
        {
            length = len;
        }
        else
        {
            length = buffer.ReadUInt32(); //数据字节长度
        }
        mainId = buffer.ReadUInt32(); //msgid
        byte[] b = buffer.ReadBytes((int)length); //数据字节

        NetLogic((int)mainId, b);

        int next = (int)buffer.ReadUInt32();
        if (next != 0)
        {
            OnRecieveMessageDeal(buffer, (uint)next);
        }
    }

    void NetLogic(int mid, byte[] bytes)
    {
        Debug.Log("mid: "+ mid);
        Debug.Log("bytes: "+System.Text.Encoding.Default.GetString(bytes));
        if (netMgr != null)
        {
            netMgr.Output(mid, bytes);
        }
    }

    void OnReceivedMessage(byte[] bytes)
    {
        ByteBuffer buffer = new ByteBuffer(bytes);

        OnRecieveMessageDeal(buffer);
    }

    void SessionSend(byte[] bytes)
    {
        WriteMessage(bytes);
    }


    public void Close()
    {
        if (client != null)
        {
            if (client.Connected) client.Close();
            client = null;
        }
        loggedIn = false;
    }


    public void SendConnect(string url, int port)
    {
        ConnectServer(url, port);
        Debug.Log("Connect: " + url + ":" + port);
    }


    public void SendMessage(ByteBuffer buffer)
    {
        SessionSend(buffer.ToBytes());
        buffer.Close();
    }
}
