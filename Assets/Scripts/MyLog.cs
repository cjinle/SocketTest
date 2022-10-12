using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLog : MonoBehaviour
{
    string myLog;
    Queue myLogQueue = new Queue();
    int maxLen = 18;
    void Start()
    {
        Debug.Log("Log1");
        Debug.Log("Log2");
        Debug.Log("Log3");
        Debug.Log("Log4");
    }

    void Update()
    {
        
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLog = logString;
        string newString = "\n [" + type + "] : " + myLog;
        myLogQueue.Enqueue(newString);
        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            myLogQueue.Enqueue(newString);
        }
        if (myLogQueue.Count > maxLen)
        {
            myLogQueue.Dequeue();
        }
        myLog = string.Empty;
        foreach (string mylog in myLogQueue)
        {
            myLog += mylog;
        }
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        Application.logMessageReceivedThreaded += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), myLog);
    }
 
}
