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
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomObserver : MonoBehaviour
{   
    private bool background = true;
    
    public void toggleObserver() {
        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        if (background)
        {
            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
        }
        else {
            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
        }
        background = !background;
    }

    public void ObserverOn ()
    {
        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        background = true;
        observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
    }

    public void ObserverOff()
    {
        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        background = false;
        observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
    }
}
