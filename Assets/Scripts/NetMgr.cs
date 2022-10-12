using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.Sockets;
using System.Collections.Generic;

public class NetMgr : MonoBehaviour
{

    [SerializeField] private Text _HelloText;

    private bool _updateTextFlg = false;
    private string _updateText = "";

    private SocketClient socket;

    //Init
    public static Action<int> OnBorn;
    //leave
    public static Action<int> OnOver;


    SocketClient SocketClient
    {
        get
        {
            if (socket == null)
                socket = new SocketClient();
            return socket;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }

    void Init()
    {
        SocketClient.OnRegister();
        SocketClient.netMgr = this;
    }

    void LateUpdate()
    {
        if (_updateTextFlg)
        {
            _HelloText.text = _updateText;
            _updateTextFlg = false;
        }
    }

    public TcpClient GetClient()
    {
        return SocketClient.GetClient();
    }

    public void SendConnect(string url, int port)
    {
        SocketClient.SendConnect(url, port);
    }

    public void SendMessage(ByteBuffer buffer)
    {
        SocketClient.SendMessage(buffer);
    }

    public void Output(int mainId, byte[] bytes)
    {
        _updateText = string.Format("ID: {0}, Content: {1}", mainId, System.Text.Encoding.Default.GetString(bytes));
        _updateTextFlg = true;
    }

    void OnDestroy()
    {
        SocketClient.OnRemove();
    }
}
