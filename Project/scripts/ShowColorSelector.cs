using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class ShowColorSelector : MonoBehaviour
{       
    private Material color;
    private MeshRenderer m;
    public float R = 0.5f, G = 0.5f, B = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        color = new Material(Shader.Find("Unlit/Color"));
        color.color = new Color(R, G, B);
        m = this.transform.GetComponent<MeshRenderer>();
        m.material = color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeR(SliderEventData eventData)
    {
        R = eventData.NewValue;
        color.color = new Color(R, G, B);
        m.material = color;
    }

    public void ChangeG(SliderEventData eventData)
    {
        G = eventData.NewValue;
        color.color = new Color(R, G, B);
        m.material = color;
    }

    public void ChangeB(SliderEventData eventData)
    {
        B = eventData.NewValue;
        color.color = new Color(R, G, B);
        m.material = color;
    }
}
