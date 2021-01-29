using UnityEngine;
using UnityEngine.UI;

public class DebugConsoleToUI : DebugObject
{
    public Text textfield;
    string myLog = "Console Output Start";
    bool doShow = true;
    public int kChars = 500;

    public void OnValidate() {
        if (textfield == null) { textfield = GetComponentInChildren<Text>(); }
    }

    public override void OnEnable() 
    { 
        base.OnEnable();
        Application.logMessageReceived += Log;
    }
    public override void OnDisable() 
    { 
        base.OnDisable();
        Application.logMessageReceived -= Log; 
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        myLog = myLog + "\n" + logString;
        if (myLog.Length > kChars) { myLog = myLog.Substring(myLog.Length - kChars); }
    }

    void OnGUI()
    {
        if (doShow) { textfield.text = myLog; }
    }
}