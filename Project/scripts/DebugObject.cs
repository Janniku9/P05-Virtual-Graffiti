using UnityEngine;

public class DebugObject : MonoBehaviour
{

    private DebugController controller;

    public virtual void OnEnable() 
    {
        foreach (Transform child in transform) { child.gameObject.SetActive(true); }
    }

    public virtual void OnDisable() 
    {
        foreach (Transform child in transform) { child.gameObject.SetActive(false); }
    }

    public void Log() {}

}
