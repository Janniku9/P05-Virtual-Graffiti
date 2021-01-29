using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Microsoft.MixedReality.Toolkit.Input
{
    public class PaintableV2Copy : MonoBehaviour, IMixedRealityPointerHandler
    {
        public int resolution;
        private Texture2D texture;

        private Vector2 lastPosition;
        private Vector2 currPosition;
        private Ray handRay;

        private int drawing = -1;
        private int lastDrawing = -1;
        private bool brush = true;

        private Color32 baseColor = new Color32(0, 0, 0, 0);

        public int[] PenShape;
        public int layers = 4;

        private int currLayer = 0;
        public Color32[] layer;

        public Material PenMaterial;
        public int size;

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            RaycastHit hit;

            InputRayUtils.TryGetRay(eventData.InputSource.SourceType, eventData.Handedness, out Ray ray);
            if (UnityEngine.Physics.Raycast(ray, out hit))
            {
                lastPosition = hit.textureCoord * resolution;
                currPosition = hit.textureCoord * resolution;
                eventData.Pointer.IsFocusLocked = false;

                copyLayerTo(currLayer, getNext());
                currLayer = getNext();

                drawing = 1;
            }
        }

        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            if (!brush)
            {
                if (drawing % 5 == 0)
                {
                    RaycastHit hit;
                    var data = texture.GetRawTextureData<Color32>();
                    InputRayUtils.TryGetRay(eventData.InputSource.SourceType, eventData.Handedness, out Ray ray);
                    Vector3 n = ray.direction;

                    for (int samples = 0; samples < 200; samples++)
                    {

                        Vector3 sample = sampleSphereCap(0.95f);


                        Vector3 local = localToWorld(sample, n);
                        Ray sampledRay = new Ray(ray.origin, local);
                        UnityEngine.Debug.DrawRay(sampledRay.origin, sampledRay.origin + sampledRay.direction, new Color(255f, 0f, 0f), 10000f, false);


                        if (UnityEngine.Physics.Raycast(sampledRay, out hit))
                        {
                            currPosition = hit.textureCoord * resolution;

                            int spray = 3;
                            for (int i = 0; i < spray; i++)
                            {
                                for (int j = 0; j < spray; j++)
                                {
                                    int index = (int)currPosition.x + i - spray / 2 + ((int)currPosition.y + j + spray / 2) * resolution;
                                    if (i * i + j * j < spray * spray && index >= 0 && index < resolution * resolution)
                                    {
                                        data[index] = PenMaterial.color;
                                    }
                                }
                            }

                        }

                    }
                    texture.Apply();
                }
                drawing++;
            }
            else
            {

                RaycastHit hit;
                InputRayUtils.TryGetRay(eventData.InputSource.SourceType, eventData.Handedness, out Ray ray);
                if (UnityEngine.Physics.Raycast(ray, out hit))
                {

                    lastPosition = currPosition;
                    currPosition = hit.textureCoord * resolution;
                    eventData.Pointer.IsFocusLocked = false;


                    drawing = drawing + 1;
                }
                else
                {
                    Reset();
                }
            }
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            Reset();
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {

        }

        // Start is called before the first frame update
        void Start()
        {
            texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            GetComponent<Renderer>().material.mainTexture = texture;

            layer = new Color32[layers * resolution * resolution];
            clearCanvas();

            for (int i = 0; i < layers; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    for (int k = 0; k < resolution; k++)
                    {
                        layer[i * resolution * resolution + j * resolution + k] = baseColor;
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

            if (brush && drawing > 1 && lastDrawing != drawing)
            {
                // load texture from canvas
                var data = texture.GetRawTextureData<Color32>();

                // line drawing algorithm
                int x1 = (int)lastPosition.x;
                int y1 = (int)lastPosition.y;
                int x2 = (int)currPosition.x;
                int y2 = (int)currPosition.y;

                int dx = x2 - x1;
                int dy = y2 - y1;

                int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
                int s2 = (int)size / 2;

                float xIncr = dx / (float)steps;
                float yIncr = dy / (float)steps;

                float x = lastPosition.x;
                float y = lastPosition.y;


                for (int k = 0; k <= steps; k++)
                {

                    // apply 1 brush pattern, in this case it's just a rectangle
                    for (int i = 0; i < size; i++)
                    {
                        for (int j = 0; j < size; j++)
                        {
                            int iTexture = (int)(x) - s2 + i;
                            int jTexture = (int)(y) - s2 + j;



                            if (!(iTexture < 0 || iTexture >= resolution || jTexture < 0 || jTexture >= resolution))
                            {
                                Color new_color = PenMaterial.color;
                                Color old_color = data[iTexture + jTexture * resolution];

                                if ((i - s2) * (i - s2) + (j - s2) * (j - s2) < s2 * s2)
                                    data[iTexture + jTexture * resolution] = new_color;

                            }
                        }
                    }

                    x = x + xIncr;
                    y = y + yIncr;

                }


                texture.Apply();
            }



        }

        void clearCanvas()
        {
            var data = texture.GetRawTextureData<Color32>();

            int index = 0;
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    data[index++] = baseColor;
                }
            }
            texture.Apply();
            lastDrawing = drawing;
        }

        public void DeleteAll()
        {
            Reset();
            clearCanvas();
        }

        public void ChangeColor(int num)
        {
            Reset();
            PenMaterial = new Material(PenMaterial);
            if (num == 0)
                PenMaterial.color = new Color(1, 0, 0);
            if (num == 1)
                PenMaterial.color = new Color(0, 1, 0);
            if (num == 2)
                PenMaterial.color = new Color(0, 0, 1);
        }

        public void Reset()
        {
            var data = texture.GetRawTextureData<Color32>();

            int index = 0;
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    layer[currLayer * resolution * resolution + index] = data[index++];
                }
            }

            drawing = -1;
            lastDrawing = -1;
            currPosition = lastPosition;
        }

        public void copyLayerTo(int from, int to)
        {
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    layer[to * resolution * resolution + x * resolution + y] = layer[from * resolution * resolution + x * resolution + y];
                }
            }
        }

        public int getNext()
        {
            return (currLayer + 1) % layers;
        }

        public int getPrevious()
        {
            return (currLayer - 1 < 0) ? layers - 1 : currLayer - 1;

        }

        public Vector3 sampleSphereCap(float cosThetaMax)
        {
            float u = (float)UnityEngine.Random.value;
            float v = (float)UnityEngine.Random.value;

            float z = cosThetaMax + u * (1 - cosThetaMax);
            float r = (float)Math.Sqrt(1 - z * z);
            float theta = 2f * (float)Math.PI * v;

            float x = r * (float)Math.Cos(theta);
            float y = r * (float)Math.Sin(theta);

            return new Vector3(x, y, z);
        }

        public Vector3 localToWorld(Vector3 sample, Vector3 n)
        {
            n = Vector3.Normalize(n);
            Vector3 v = new Vector3((float)UnityEngine.Random.value, (float)UnityEngine.Random.value, (float)UnityEngine.Random.value);
            v -= Vector3.Dot(v, n) * n;
            v = Vector3.Normalize(v);

            Vector3 b = Vector3.Cross(v, n);
            b = Vector3.Normalize(b);

            return new Vector3(Vector3.Dot(sample, b), Vector3.Dot(sample, v), Vector3.Dot(sample, n));
        }

        public void undo()
        {
            currLayer = getPrevious();
            var data = texture.GetRawTextureData<Color32>();

            int index = 0;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    data[index] = layer[currLayer * resolution * resolution + index++];
                }
            }
            texture.Apply();
        }

        public void brushOn()
        {
            brush = true;
        }

        public void brushOff()
        {
            brush = false;
        }
    }
}