using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
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
    public class PaintableV2 : MonoBehaviour, IMixedRealityPointerHandler
    {
        //set these values before calling Start()
        public int resolutionX = 16, resolutionY = 16;
        public int canvasNo = 0;
        public int layers = 8; //undo buffer size

        private Texture2D texture;

        private RaycastHit lastPosition;
        private RaycastHit currPosition;
        private Ray handRay;

        private int drawing = -1;
        private int lastDrawing = -1;

        private Color32 baseColor = new Color32(0, 0, 0, 0); //transparent


        private int currLayer = 0;
        private Color32[] layer;


        private Pinable pin;

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (drawing == -2) return;
            if (!pin.pinned)
            { //in case you're moving it around, pin it down and don't draw.
                pin.pin();
                pin.switchToCanvas(canvasNo);
                drawing = -2;
                return;
            }
            pin.switchToCanvas(canvasNo);
            RaycastHit hit;

            InputRayUtils.TryGetRay(eventData.InputSource.SourceType, eventData.Handedness, out Ray ray);
            if (UnityEngine.Physics.Raycast(ray, out hit))
            {
                lastPosition = hit;
                currPosition = hit;
                eventData.Pointer.IsFocusLocked = false;

                copyLayerTo(currLayer, getNext());
                currLayer = getNext();

                drawing = 1;
            }
        }

        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            if (drawing == -2) return;
            pin.switchToCanvas(canvasNo);

            RaycastHit hit;
            InputRayUtils.TryGetRay(eventData.InputSource.SourceType, eventData.Handedness, out Ray ray);
            if (UnityEngine.Physics.Raycast(ray, out hit))
            {

                lastPosition = currPosition;
                currPosition = hit;
                handRay = ray;
                eventData.Pointer.IsFocusLocked = false;


                drawing = drawing + 1;
            }
            else
            {
                Reset();
            }
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            pin.switchToCanvas(canvasNo);
            Reset();
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            pin.switchToCanvas(canvasNo);
        }

        // Start is called before the first frame update
        public void Start()
        {
            texture = new Texture2D(resolutionX, resolutionY, TextureFormat.RGBA32, false);
            GetComponent<Renderer>().material.mainTexture = texture;
            pin = GetComponentInParent<Pinable>();
            layer = new Color32[layers * resolutionX * resolutionY];
            clearCanvas();

            for (int i = 0; i < layers; i++)
            {
                for (int j = 0; j < resolutionY; j++)
                {
                    for (int k = 0; k < resolutionX; k++)
                    {
                        layer[i * resolutionY * resolutionX + j * resolutionX + k] = baseColor;
                    }
                }
            }
            Reset();
        }

        // Update is called once per frame
        void Update()
        {
            if (drawing > 1 && lastDrawing != drawing)
            {
                // load texture from canvas
                var data = texture.GetRawTextureData<Color32>();

                float X = currPosition.textureCoord.x * resolutionX;
                float Y = currPosition.textureCoord.y * resolutionY;

                int X1 = (int)(currPosition.textureCoord.x * resolutionX);
                int Y1 = (int)(currPosition.textureCoord.y * resolutionY);
                int X2 = (int)(lastPosition.textureCoord.x * resolutionX);
                int Y2 = (int)(lastPosition.textureCoord.y * resolutionY);

                float squaredDist = currPosition.distance * currPosition.distance;

                if (pin.brush)
                {


                    // line drawing algorithm
                    int dx = X2 - X1;
                    int dy = Y2 - Y1;

                    int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    int s2 = (int)pin.size / 2;

                    float xIncr = dx / (float)steps;
                    float yIncr = dy / (float)steps;




                    for (int k = 0; k <= steps; k++)
                    {

                        // apply 1 brush pattern, in this case it's just a circle
                        for (int i = 0; i < pin.size; i++)
                        {
                            for (int j = 0; j < pin.size; j++)
                            {
                                int iTexture = (int)X - s2 + i;
                                int jTexture = (int)Y - s2 + j;



                                if (!(iTexture < 0 || iTexture >= resolutionX || jTexture < 0 || jTexture >= resolutionY))
                                {
                                    Color new_color = pin.GetColor();
                                    Color old_color = data[iTexture + jTexture * resolutionX];

                                    if ((i - s2) * (i - s2) + (j - s2) * (j - s2) < s2 * s2)
                                        data[iTexture + jTexture * resolutionX] = new_color;

                                }
                            }
                        }

                        X = X + xIncr;
                        Y = Y + yIncr;

                    }



                }
                else
                {
                    Transform tf = currPosition.transform;
                    Vector3 normal = tf.InverseTransformVector(currPosition.normal).normalized;
                    Vector3 wi = tf.InverseTransformVector(handRay.origin - currPosition.point).normalized;

                    float phi = (float)Math.PI - (float)Math.Atan2(wi.z, wi.x);
                    float beta = (float)Math.Acos(Vector3.Dot(normal, wi));
                    float alpha = (float)Math.PI / 2 - beta;



                    float l = pin.L * currPosition.distance * (float)Math.Tan(pin.Theta);

                    float e = (alpha - (float)Math.PI / 2) * (-1.0f / ((float)Math.PI / 2 - pin.Theta));

                    //!TODO: Ellipse depends on resX, resY
                    int side = e >= 1 ? Math.Min(resolutionX, resolutionY) : (int)(l / (1 - e));
                    for (int y = Y1 - side; y < Y1 + side; y++)
                    {
                        for (int x = X1 - side; x < X1 + side; x++)
                        {
                            float prob = 1.0f / (squaredDist + (x - X) * (x - X) / (pin.L * pin.L / 2) + (y - Y) * (y - Y) / (pin.L * pin.L / 2));
                            if (x >= 0 && y >= 0 && x < resolutionX && y < resolutionY && (float)UnityEngine.Random.value < prob && conicSection(x - X, y - Y, l, e, phi))
                            {
                                Color new_color = pin.GetColor();
                                Color old_color = data[y * resolutionX + x];

                                //if (old_color.a == 1)
                                new_color = Color32.Lerp(new_color, old_color, (float)Math.Max(0.01*currPosition.distance*currPosition.distance, 0.8));
                                data[y * resolutionX + x] = new_color;
                            }
                        }
                    }

                    /*
                    int side = (int)Math.Max(A, B);

                    for (int y = Y1 - side; y < Y1 + side; y++)
                    {
                        for (int x = X1 - side; x < X1 + side; x++)
                        {
                            float b = (squaredDist - wi.z * (y - Y1) / B - wi.x * (x - X1) / A);
                            float prob = b >= 1 ? 0.2f / (b*b) : 0.3f;

                            

                            if (x > 0 && y > 0 && x < (resolutionX-1) && y < (resolutionX-1) && (float)UnityEngine.Random.value < prob && conicSection(x, y, A, B, X1, Y1, angle))
                            {
                                Color new_color = pin.PenMaterial.color;
                                Color old_color = data[y * resolutionX + x];


                                if (old_color.a == 1)
                                    new_color = Color32.Lerp(new_color, old_color, 0.2f);

                                data[y * resolutionX + x] = new_color;
                                data[(y + 1) * resolutionX + x  + 1] = new_color;
                                data[(y - 1) * resolutionX + x + 1] = new_color;
                                data[(y + 1) * resolutionX + x - 1] = new_color;
                                data[(y - 1) * resolutionX + x - 1] = new_color;
                            }
                                
                        }
                    }*/
                }
                texture.Apply();
            }
        }

        bool ellipse(float x, float y, float a, float b, float h, float k, float angle)
        {
            float t1 = ((x - h) * (float)Math.Cos(angle) + (y - k) * (float)Math.Sin(angle));
            float t2 = (t1 * t1) / (a * a);
            float t3 = ((x - h) * (float)Math.Sin(angle) - (y - k) * (float)Math.Cos(angle));
            float t4 = (t3 * t3) / (b * b);
            return ((t2 + t4) <= 1);
        }

        bool conicSection(float x, float y, float l, float e, float phi)
        {
            float theta = (float)Math.Atan2(y, x) + phi;
            float r = (float)Math.Sqrt(x * x + y * y);
            return r * (1 + e * (float)Math.Cos(theta)) <= l;
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


        public void Reset()
        {
            var data = texture.GetRawTextureData<Color32>();

            int index = 0;
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    layer[currLayer * resolutionY * resolutionX + index] = data[index++];
                }
            }

            drawing = -1;
            lastDrawing = -1;
            currPosition = lastPosition;

        }

        public void copyLayerTo(int from, int to)
        {
            for (int y = 0; y < resolutionY; y++)
            {
                for (int x = 0; x < resolutionX; x++)
                {
                    layer[to * resolutionY * resolutionX + y * resolutionX + x] = layer[from * resolutionY * resolutionX + y * resolutionX + x];
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

        public void Undo()
        {
            currLayer = getPrevious();
            var data = texture.GetRawTextureData<Color32>();

            int index = 0;
            for (int y = 0; y < resolutionY; y++)
            {
                for (int x = 0; x < resolutionX; x++)
                {
                    data[index] = layer[currLayer * resolutionY * resolutionX + index++];
                }
            }
            texture.Apply();
        }
    }
}