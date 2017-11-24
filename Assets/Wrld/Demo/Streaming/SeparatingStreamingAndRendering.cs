using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wrld;


public class SeparatingStreamingAndRendering: MonoBehaviour
{
    public Camera renderingCamera;

    void Start()
    {
        Api.Instance.CameraApi.SetControlledCamera(renderingCamera);
    }
}
