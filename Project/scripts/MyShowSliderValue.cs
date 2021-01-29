//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// This is not copy pasted
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos
{
    [AddComponentMenu("Scripts/MRTK/Examples/ShowSliderValue")]
    public class MyShowSliderValue : MonoBehaviour
    {
        public float from = 0f;
        public float to = 1f;
        public bool toInt = false;

        [SerializeField]
        private TextMeshPro textMesh = null;

        public void OnSliderUpdated(SliderEventData eventData)
        {
            float value = from + eventData.NewValue * (to - from);
            if (textMesh == null)
            {
                textMesh = GetComponent<TextMeshPro>();
            }

            if (textMesh != null)
            {
                textMesh.text = (toInt? $"{(int)value}" : $"{value:2f}");
            }
        }
    }
}
