using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.Input
{
    public class Paintable : MonoBehaviour, IMixedRealityPointerHandler
    {
        private LineRenderer line;

        private int lineCount = 0;

        public Material Pen;
        public float size;
        public void ChangeColor(int num) {
            Pen = new Material(Pen);
            if (num == 0)
                Pen.color = new Color(1, 0, 0);
            if (num == 1)
                Pen.color = new Color(0, 1, 0);
            if (num == 2)
                Pen.color = new Color(0, 0, 1);
        }

        public void DeleteAll() { 
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("Line"))
                    GameObject.Destroy(child.gameObject);
            }
        }
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            RaycastHit hit;
            
            InputRayUtils.TryGetRay(eventData.InputSource.SourceType, eventData.Handedness, out Ray ray);
            if (UnityEngine.Physics.Raycast(ray, out hit) && !line)
            {
                //create a line if no line exists
                createLine();
                line.SetPosition(0, hit.point + transform.up * 0.01f);
                line.SetPosition(1, hit.point + transform.up * 0.01f);
                //paint.transform.localScale = Vector3.one * size;
            }
        }

        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            RaycastHit hit;
            InputRayUtils.TryGetRay(eventData.InputSource.SourceType, eventData.Handedness, out Ray ray);

            if (UnityEngine.Physics.Raycast(ray, out hit) && line)
            {
                updateLine(hit.point + transform.up * 0.01f);
                //update the cursor
                eventData.Pointer.IsFocusLocked = false;
                //paint.transform.localScale = Vector3.one * size;
            }
            else if (!UnityEngine.Physics.Raycast(ray, out hit) && line)
            {
                line = null;
            }
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            line = null;
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            /*
            RaycastHit hit;
            InputRayUtils.TryGetHandRay(Handedness.Right, out Ray RightHandRay);
            if (UnityEngine.Physics.Raycast(RightHandRay, out hit) && !line)
            {
                //create a line if no line exists
                createLine();
                line.SetPosition(0, hit.point + transform.up * 0.01f);
                line.SetPosition(1, hit.point + transform.up * 0.01f);
                //paint.transform.localScale = Vector3.one * size;
            }
            else if (UnityEngine.Physics.Raycast(RightHandRay, out hit) && line)
            {
                updateLine(hit.point + transform.up * 0.01f);
                //paint.transform.localScale = Vector3.one * size;
            }
            else if (!UnityEngine.Physics.Raycast(RightHandRay, out hit))
            {
                line = null;
            }
            */
        }

        void createLine()
        {
            
            line = new GameObject("Line"+lineCount++).AddComponent<LineRenderer>();
            line.material = Pen;
            line.positionCount = 2;
            line.startWidth = size; 
            line.endWidth = size;
            line.startColor = new Color(1, 0, 0, 1);
            line.endColor = new Color(1, 0, 0, 1);
            line.numCapVertices = 10;
            line.numCornerVertices = 5;
            line.alignment = LineAlignment.TransformZ;
            line.transform.parent = this.transform;
            line.useWorldSpace = false;
            //Debug.Log(line.transform.localPosition);
        }
        void updateLine(Vector3 newPoint)
        {
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, newPoint);
        }
    }
}