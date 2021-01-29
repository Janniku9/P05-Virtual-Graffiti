using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    public bool debugEnabled = false;
    public bool debugMenuEnabled = false;
    public bool consoleEnabled = true;
    public bool controlsSUEnabled = true;
    public GameObject console;
    public GameObject controlsSU;
    public Microsoft.MixedReality.SceneUnderstanding.Samples.Unity.SceneUnderstandingManager SUManager;
    public Pinable canvases;

    private bool canvasesShown = false;

    void Start()
    {
        if (SUManager == null) { SUManager = GetComponentInChildren<Microsoft.MixedReality.SceneUnderstanding.Samples.Unity.SceneUnderstandingManager>(); }
        ToggleDebugMenu(); //disable it on start for final version.
    }

    void Update() 
    {
        foreach(Transform obj in transform) { obj.gameObject.SetActive(debugMenuEnabled); }
        if (debugMenuEnabled)
        {
            if (console != null) console.SetActive(consoleEnabled);
            if (controlsSU != null) controlsSU.SetActive(controlsSUEnabled);
        }
        // show canvases in debug mode
        if (debugEnabled && !canvasesShown) { canvases.ShowCanvases(true); canvasesShown = true; }
        if (!debugEnabled && canvasesShown) { canvases.ShowCanvases(false); canvasesShown = false; }
    }

    private void Toggle(ref bool b, string varName) 
    {
        b = !b;
        if (b) { Debug.Log(varName + " enabled."); }
        else { Debug.Log(varName + " disabled."); }
    }

    public void ToggleDebug() { Toggle(ref debugEnabled, "Debug"); }

    public void ToggleDebugMenu() { Toggle(ref debugMenuEnabled, "Debug menu"); }

    /* Debug Controls */
    public void ToggleConsole() { Toggle(ref consoleEnabled, "Console"); }
    public void ToggleControlsSU() { Toggle(ref controlsSUEnabled, "ControlsSU"); }
    /* Debug Controls */
    
    /* SCENE UNDERSTANDING CONTROLS*/
    public void ToggleSceneUnderstanding() 
    { 
        SUManager.enabled = !SUManager.enabled; 
        if (SUManager.enabled) Debug.Log("Skript SceneUnderstandingManager enabled");
        else Debug.Log("Skript SceneUnderstandingManager disabled");
    }
    public void SURefresh() 
    { 
        SUManager.StartDisplay(); 
        Debug.Log("Refresh Scene Understanding"); 
    }
    public void SUAutoRefresh() { Toggle(ref SUManager.AutoRefresh, "SUManager.AutoRefresh"); }

    public void SURenderSceneObjects() { Toggle(ref SUManager.RenderSceneObjects, "SUManager.RenderSceneObjects"); }
    public void SURenderPlatformSceneObjects() { Toggle(ref SUManager.RenderPlatformSceneObjects, "SUManager.RenderPlatformSceneObjects"); }
    public void SURenderCompletelyInferredSceneObjects() { Toggle(ref SUManager.RenderCompletelyInferredSceneObjects, "SUManager.RenderCompletelyInferredSceneObjects"); }
    public void SURenderWorldMesh() { Toggle(ref SUManager.RenderWorldMesh, "SUManager.RenderWorldMesh"); }

    public void SUChangeObjectRequestMode()
    {
        SUManager.SceneObjectRequestMode = SUManager.SceneObjectRequestMode++;
        Debug.Log("SUManager.SceneObjectRequestMode = " + SUManager.SceneObjectRequestMode);
    }
    /* SCENE UNDERSTANDING CONTROLS */
}
