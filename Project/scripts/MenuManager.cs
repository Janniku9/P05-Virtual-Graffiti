using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private string[] views = new string[] { "ToolMenu", "ColorMenu", "CanvasMenu" };
    int currView = 0;
    
    void Start()
    {
        OpenView(0);
    }

    public void OpenView (int v)
    {
        Vector3 currPos = new Vector3(0, 0, 0);
        Quaternion currRot = new Quaternion();
        foreach (Transform child in transform)
        {
            if (child.gameObject.name == views[currView])
            {
                currPos = child.position;
                currRot = child.rotation;
            }
        }
        currView = v;
        foreach (Transform child in transform)
        {
            child.position = currPos;
            child.rotation = currRot;
            if (child.name == views[v])
            {   
                child.gameObject.SetActive(true);
            } else
            {
                child.gameObject.SetActive(false);
            }
        }

    }
}
