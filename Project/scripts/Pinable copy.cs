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

public class Pinablecopy : MonoBehaviour
{
    //accessable through canvas children
    public bool brush = true;
    public GameObject ColorSelector;

    //scene root for scene understanding walls
    public GameObject SceneRoot;

    public bool pinned;
    public float Theta = (float)Math.PI/10f;
    public float L = 20f;
    public int size = 10;
    
    private MeshRenderer mr;
    private MeshCollider mc;
    private Transform rootCanvases;
    private List<Transform> canvases;
    //private List<Transform> planes;

    private int totalCanvas;
    private int currentCanvas;

    // Start is called before the first frame update
    void Start()
    {
        canvases = new List<Transform>();
        //indicators = new List<Transform>();
        //planes = new List<Transform>();
        rootCanvases = Search(this.transform, "Canvases");
        canvases.Add(Search(this.transform, "Canvas"));
        //Assert.IsTrue(canvases[0] == rootCanvases.GetChild(0));
        //planes.Add(Search(this.transform, "Plane"));
        totalCanvas = 1;

        switchToCanvas(0);
        pinned = false;
        if (pinned)
        {
            pin();
        }
        else
        {
            unpin();
        }
    }

    public void switchToCanvas(int index)
    {
        if (index >= totalCanvas) return;
        currentCanvas = index;
        //the order matters!
        GameObject plane = canvases[currentCanvas].GetChild(0).gameObject;
        GameObject indicator = canvases[currentCanvas].GetChild(1).gameObject;

        if (!"Plane".Equals(plane.name)) { //double check order, delete for efficiency
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

    public void pin()
    {
        mr.enabled = false;
        mc.enabled = true;
        (canvases[currentCanvas].gameObject.GetComponent("SurfaceMagnetism") as MonoBehaviour).enabled = false;
    }

    public void unpin()
    {
        mr.enabled = true;
        mc.enabled = false;

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
        pin(); //make sure the previously selected canvas has been pinned down
        totalCanvas++;
        int canvasNo = totalCanvas - 1;
        GameObject newCanvas = Instantiate(canvases[0].gameObject);
        PaintableV2 paint = newCanvas.GetComponentInChildren<PaintableV2>();
        paint.canvasNo = canvasNo;
        newCanvas.transform.name = "Canvas" + canvasNo;
        newCanvas.transform.SetParent(rootCanvases);
        canvases.Add(newCanvas.transform);
        //planes.Add(newPlane.transform);
        switchToCanvas(canvasNo);
        unpin();
    }


    public void brushOn()
    {
        brush = true;
    }

    public void brushOff()
    {
        brush = false;
    }

    public void Undo() {
        PaintableV2 paint = canvases[currentCanvas].gameObject.GetComponentInChildren<PaintableV2>();
        paint.Undo();
    }



    private bool firstTimeAdding = true;
    public void SceneUnderstandingToCanvases()
    {
        //Debug.Log("first time scene understanding: " + firstTimeAdding.ToString());
        firstTimeAdding = false;
        //Debug.Log("placing canvases");
        pin(); //make sure the previously selected canvas has been pinned down
        int storedCanvasNo = currentCanvas;
        foreach (Transform child in SceneRoot.transform)
        {
            GameObject suObject = child.gameObject;
            string label = suObject.name;
            if (suObject == null || label == null) continue;
            if (
                label != "Wall" &&
                label != "Floor" &&
                label != "Ceiling" &&
                //label != "Unknown" &&
                label != "Platform"
                //label != "Background"
            ) continue;

            //Debug.Log("got object: " + label + " with id " + child.GetInstanceID().ToString() );
            totalCanvas++;
            GameObject newCanvas = Instantiate(canvases[0].gameObject);
            PaintableV2 paint = newCanvas.GetComponentInChildren<PaintableV2>();
            paint.canvasNo = totalCanvas - 1;
            //newCanvas.position = suObject.transform.position;
            //newCanvas.rotation = suObject.transform.rotation + suObject.transform.forward * 0.0001; //slightly infront of object.
            Vector3 pos = suObject.transform.position + 0.000001f * suObject.transform.forward;
            newCanvas.transform.SetPositionAndRotation(pos, suObject.transform.rotation);
            newCanvas.transform.name = "Canvas" + (totalCanvas - 1);
            newCanvas.transform.SetParent(rootCanvases);
            canvases.Add(newCanvas.transform);
            switchToCanvas(paint.canvasNo);
            pin(); //make sure the new transform is pinned.
            //Debug.Log("placed transform");
        }
        //Debug.Log("total canvases: " + totalCanvas.ToString());
        switchToCanvas(storedCanvasNo); //switch back to the original one.
    }

    public void OnSizeValueUpdate(SliderEventData eventData)
    {
        size = 1 + (int)(eventData.NewValue * 49);
    }

    public void OnAngleValueUpdate(SliderEventData eventData)
    {
        Theta = (5f + 65f * eventData.NewValue) / 360f * (float)Math.PI * 2f;
    }

    public void ClearCanvas()
    {
        PaintableV2 paint = canvases[currentCanvas].gameObject.GetComponentInChildren<PaintableV2>();
        paint.DeleteAll();
    }

    public Color GetColor()
    {
        return ColorSelector.GetComponent<MeshRenderer>().material.color;
    }
}