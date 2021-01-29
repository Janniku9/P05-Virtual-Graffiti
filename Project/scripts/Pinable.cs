using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class Pinable : MonoBehaviour
{
    public DebugController debugController;

    //accessable through canvas children
    public bool brush = true;
    public GameObject ColorSelector;

    //scene root for scene understanding walls
    public GameObject SceneRoot;

    public float Theta = (float)Math.PI / 10f;
    public float L = 20f;
    public int size = 10;

    public GameObject canvasOriginal;
    public GameObject placeholderCanvas;
    public DebugSceneUnderstanding dbg;

    public bool pinned = true;
    private MeshRenderer mr; //reference to current canvas meshrenderer and meshcollider
    private MeshCollider mc;
    private Transform rootCanvases;
    private List<Transform> canvases;
    //private List<Transform> planes;

    private int totalCanvas;
    private int currentCanvas;

    private float defaultCanvasX = 1.0f;
    private float defaultCanvasY = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        canvases = new List<Transform>();
        rootCanvases = Search(this.transform, "Canvases");

        Debug.Log("before anything");
        GameObject plane = canvasOriginal.transform.GetChild(0).gameObject;
        GameObject indicator = canvasOriginal.transform.GetChild(1).gameObject;

        if (!"Plane".Equals(plane.name))
        { //double check order, delete for efficiency
            plane = canvasOriginal.transform.GetChild(1).gameObject;
            indicator = canvasOriginal.transform.GetChild(0).gameObject;
        }
        mc = plane.GetComponent<MeshCollider>();
        mr = indicator.GetComponent<MeshRenderer>();
        mr.enabled = false;
        mc.enabled = true;
        Debug.Log("Before component");
        (canvasOriginal.GetComponent("SurfaceMagnetism") as MonoBehaviour).enabled = false;
        Debug.Log("After component");
        //canvases.Add(Search(this.transform, "Canvas")); //add empty canvas

        totalCanvas = 0;
        currentCanvas = -1;

        switchToCanvas(0);
        pinned = false;
        /*
        if (pinned)
        {
            pin();
        }
        else
        {
            unpin();
        }*/
    }

    public void switchToCanvas(int index)
    {
        if (index >= totalCanvas) return;
        pin();
        currentCanvas = index;
        //the order matters!
        GameObject plane = canvases[currentCanvas].GetChild(0).gameObject;
        GameObject indicator = canvases[currentCanvas].GetChild(1).gameObject;

        if (!"Plane".Equals(plane.name))
        { //double check order, delete for efficiency
            plane = canvases[currentCanvas].GetChild(1).gameObject;
            indicator = canvases[currentCanvas].GetChild(0).gameObject;
        }

        mc = plane.GetComponent<MeshCollider>();
        mr = indicator.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowCanvases(bool b)
    {
        foreach (Transform canvas in canvases)
        {
            if (canvas != null && canvas.childCount > 1 && canvas.GetChild(1) != null)
            {
                GameObject indicator = canvas.GetChild(1).gameObject;
                mr = indicator.GetComponent<MeshRenderer>();
                mr.enabled = b;
            }
        }
    }

    public void pin()
    {
        pinned = true;
        if (currentCanvas < 0 || currentCanvas >= totalCanvas) return;
        mr.enabled = false;
        mc.enabled = true;
        (canvases[currentCanvas].gameObject.GetComponent("SurfaceMagnetism") as MonoBehaviour).enabled = false;
    }

    public void unpin()
    {
        pinned = false;
        if (currentCanvas < 0 || currentCanvas >= totalCanvas) return;
        mr.enabled = true;
        mc.enabled = true;

        (canvases[currentCanvas].gameObject.GetComponent("SurfaceMagnetism") as MonoBehaviour).enabled = true;
    }

    public Transform Search(Transform target, string name)
    {
        if (target.name == name) return target;

        for (int i = 0; i < target.childCount; ++i)
        {
            Transform result = Search(target.GetChild(i), name);

            if (result != null) return result;
        }
        return null;
    }

    public void createNewCanvas()
    {
        createNewCanvasDim(defaultCanvasX, defaultCanvasY, 256);
    }

    public void createNewCanvasDim(float width, float height, int quality)
    {
        pin(); //make sure the previously selected canvas has been pinned down
        GameObject newCanvas = createCanvasObject(width, height, quality);
        unpin();
    }

    public GameObject createCanvasObject(float width, float height, int quality)
    {
        pin();
        totalCanvas++;
        int canvasNo = totalCanvas - 1;
        //TODO: Change resolution of original so it is instantiated correctly!

        PaintableV2 paint = canvasOriginal.GetComponentInChildren<PaintableV2>();
        if (paint == null)
        {
            Debug.Log("Null paint");
            return null;
        }
        paint.resolutionX = (int)(quality * width);
        paint.resolutionY = (int)(quality * height);
        paint.canvasNo = canvasNo;

        GameObject canvas = Instantiate(canvasOriginal);
        if (canvas == null)
        {
            Debug.Log("new canvas");
            return null;
        }
        Debug.Log("ok then instantiated");
        canvas.transform.name = "Canvas" + canvasNo;
        canvas.transform.SetParent(rootCanvases);
        Debug.Log("blub blub curscale " + canvas.transform.localScale);
        canvas.transform.localScale = new Vector3(width, height, 1.0f);
        canvases.Add(canvas.transform);
        Debug.Log("after  scale" + canvas.transform.localScale);

        //PaintableV2 paint = canvas.GetComponentInChildren<PaintableV2>();
        //paint.canvasNo = canvasNo;
        //paint.Start();
        switchToCanvas(canvasNo);
        return canvas;
    }

    public void brushOn()
    {
        brush = true;
    }

    public void brushOff()
    {
        brush = false;
    }

    public void Undo()
    {
        if (currentCanvas < 0 || currentCanvas >= totalCanvas) return;
        PaintableV2 paint = canvases[currentCanvas].gameObject.GetComponentInChildren<PaintableV2>();
        paint.Undo();
    }

    public void SceneUnderstandingToCanvases()
    {
        int loopCount = 0;
        int walls = 0, floors = 0, ceilings = 0, platforms = 0;

        totalCanvas = 0;
        Debug.Log("Children destroyed in rootCanvases: " + loopCount);
        loopCount = 0;
        int rootChildren = 0;

        foreach (Transform SUobj in SceneRoot.transform)
        {
            SUobj.gameObject.SetActive(false);
            loopCount++;
            string label = SUobj.name;

            if (SUobj == null || label == null)
            {
                loopCount--;
                continue;
            }
            /*if (label != "Wall" &&
                label != "Floor" &&
                label != "Ceiling" &&
                label != "Platform")*/
            if (label != "Wall") //consider adding floor and ceiling
            {
                loopCount--;
                Destroy(SUobj.gameObject);
                continue;
            }

            //Add Placeholder Canvas, uncomment this for wall debugging!!
            /*
            GameObject placeholder = GameObject.Instantiate(placeholderCanvas, SUobj.position, SUobj.rotation, rootCanvases);
            placeholder.transform.position -= placeholder.transform.forward * 0.01f;
            placeholder.transform.localScale = SUobj.localScale * 0.01f; //lossyScale;
            Text text = placeholder.GetComponentInChildren<Text>();
            text.text = label + " " + mf.mesh.bounds.size;
            */

            if (label == "Wall") dbg.Log("Walls", walls++.ToString(), 0);
            if (label == "Floor") dbg.Log("Floor", floors++.ToString(), 1);
            if (label == "Ceiling") dbg.Log("Ceiling", ceilings++.ToString(), 2);
            if (label == "Platform") dbg.Log("Platform", platforms++.ToString(), 3);

            /*
            UnityEngine.Component[] components2 = SUobj.gameObject.GetComponentsInChildren(typeof(UnityEngine.Component));
            foreach (UnityEngine.Component component in components2)
            {
                Debug.Log(component.ToString());
            }*/

            //Debug.Log("Lossy Scales: "+ SUobj.lossyScale.x+ " "+ SUobj.lossyScale.y+ " "+ SUobj.lossyScale.z);

            MeshFilter mf = SUobj.gameObject.GetComponentInChildren<MeshFilter>();
            if (mf == null) continue;

            float canvasWidth = mf.mesh.bounds.size.x;
            float canvasHeight = mf.mesh.bounds.size.y;


            //continue;
            /* For global bounding box
            MeshRenderer renderer = SUobj.gameObject.GetComponentInChildren<MeshRenderer>();
            if (renderer == null)
                Debug.Log("Null renderer");
            else
                Debug.Log("Size of renderer: " + renderer.bounds.size + " label: " + label);*/



            /*
            //Create Canvas
            GameObject canvas = createCanvasObject();
            Vector3 pos = SUobj.position - 0.002f * SUobj.forward;
            canvas.transform.SetPositionAndRotation(pos, SUobj.rotation);
            pin();
            rootChildren++;
            */

            //max it out by 10x6 meters, will be placed in the center regardless.
            canvasWidth = canvasWidth < 10.0f ? canvasWidth : 10.0f;
            canvasHeight = canvasHeight < 6.0f ? canvasHeight : 6.0f;
            GameObject canvas = createCanvasObject(canvasWidth, canvasHeight, 128);
            Vector3 pos = SUobj.position - 0.002f * SUobj.forward;
            canvas.transform.SetPositionAndRotation(pos, SUobj.rotation);
            Debug.Log("Cancavas width: " + canvasWidth + " " + canvasHeight + " scale: " + canvas.transform.localScale);// + "Current localScale: " + canvas.transform.localScale);
            //canvas.transform.localScale = new Vector3(canvasWidth, canvasHeight, 1.0f);
            pin();
            rootChildren++;

            //destroy the sceneunderstanding object
            Destroy(SUobj.gameObject);
        }
        Debug.Log("Children created in rootCanvases: " + loopCount);
        if (debugController.debugEnabled) dbg.Log("SceneRoot children", rootChildren.ToString(), 4);
    }

    public void OnSizeValueUpdate(SliderEventData eventData)
    {
        size = 1 + (int)(eventData.NewValue * 49);
    }

    public void OnAngleValueUpdate(SliderEventData eventData)
    {
        Theta = (5f + 65f * eventData.NewValue) / 360f * (float)Math.PI * 2f;
    }

    public void ChangeDefaultX(SliderEventData eventData)
    {
        defaultCanvasX = eventData.NewValue * 7.9f + 0.1f;
    }

    public void ChangeDefaultY(SliderEventData eventData)
    {
        defaultCanvasY = eventData.NewValue * 7.9f + 0.1f;
    }

    public void ClearCanvas()
    {
        if (currentCanvas < 0 || currentCanvas >= totalCanvas) return;
        PaintableV2 paint = canvases[currentCanvas].gameObject.GetComponentInChildren<PaintableV2>();
        paint.DeleteAll();
    }

    public void DeleteCanvas()
    {
        if (currentCanvas < 0 || currentCanvas >= totalCanvas) return;

        //Transform TempParent = new GameObject().transform;
        //canvases[currentCanvas].transform.SetParent(TempParent);
        //Destroy(TempParent.gameObject);

        Destroy(canvases[currentCanvas].gameObject);
        canvases.RemoveAt(currentCanvas);
        currentCanvas--;
        totalCanvas--;
        if (currentCanvas < 0 && totalCanvas > 0) currentCanvas = 0;
    }

    public Color GetColor()
    {
        return ColorSelector.GetComponent<MeshRenderer>().material.color;
    }
}