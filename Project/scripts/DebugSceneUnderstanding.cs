using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugSceneUnderstanding : DebugObject
{
    public bool enable = true;

    void Start()
    {
        foreach(Transform child in transform){
            if (child.name == "Panel") continue;
            Text txt = child.GetComponent<Text>();
            Text val = child.transform.GetChild(0).GetComponent<Text>();
            txt.text = "";
            val.text = "";
        }
    }

    public void Log(string text) {
        Log(text,"",0);
    }

    public void Log(string text, int line) {
        Log(text,"",line);
    }

    public void Log(string text, string value, int line) {
        if (line < 0 || line > 5) {
            line = 0;
        }
        string label = "Debug" + line;
        Transform dbgLine = null;
        foreach(Transform child in transform){
            if (child.name == label) {
                dbgLine = child;
                continue;
            }
        }
        Text txt = dbgLine.GetComponent<Text>();
        Text val = dbgLine.transform.GetChild(0).GetComponent<Text>();
        txt.text = text;
        val.text = value;
    }
}
