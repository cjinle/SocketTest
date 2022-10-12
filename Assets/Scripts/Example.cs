using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour
{

    //private float _curSeconds = 0f;

    [SerializeField] private InputField InputIP;
    [SerializeField] private InputField InputPort;
    [SerializeField] private InputField InputMsg;
    [SerializeField] private Text TextHello;

    public NetMgr netMgr;


    private void Start()
    {
        Debug.Log("hello world");
        InputIP.text = "192.168.56.101";
        InputPort.text = "8888";

        netMgr = GameObject.Find("NetMgr").GetComponent<NetMgr>();
    }

    private void Update()
    {
        //if (_curSeconds > 1)
        //{
        //    //Debug.Log("hello world" + Time.deltaTime);
        //    _curSeconds = 0;
        //} else
        //{
        //    _curSeconds += Time.deltaTime;
        //}
    }

    public void Connect()
    {
        Debug.Log("IP: " + InputIP.text);
        Debug.Log("Port: " + InputPort.text);

        netMgr.SendConnect(InputIP.text, int.Parse(InputPort.text));
        TextHello.text = "Server Connected";

        StartCoroutine(BackLogin());
        //InputPort.text = "123123213";

    }

    IEnumerator BackLogin()
    {
        if (netMgr.GetClient() == null || !netMgr.GetClient().Connected)
        {
            yield return new WaitForSeconds(.25f);
        }
        TextHello.text = "Start Login...";
        Login();
    }

    public void Login()
    {
        ByteBuffer b = new ByteBuffer();
        byte[] bytes = System.Text.Encoding.Default.GetBytes("hello login");
        b.WriteInt(bytes.Length);
        b.WriteInt(0x01);
        b.WriteBytes(bytes);
        netMgr.SendMessage(b);
    }

    public void Send()
    {
        Debug.Log("msg: " + InputMsg.text);
        ByteBuffer b = new ByteBuffer();

        //MemoryStream ms = new MemoryStream();
        byte[] bytes = System.Text.Encoding.Default.GetBytes(InputMsg.text);

        b.WriteInt(bytes.Length);
        b.WriteInt(0x02);
        b.WriteBytes(bytes);
        netMgr.SendMessage(b);

        InputMsg.text = "";
    }
}
