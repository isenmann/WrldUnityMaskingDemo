using System.Collections;
using UnityEngine;
using Wrld;
using Wrld.Space;

public class RotateObjectOnMap: MonoBehaviour
{
    static LatLong startPosition = LatLong.FromDegrees(37.783372, -122.400834);
    static float rotationSpeed = 180.0f;

    public GeographicTransform coordinateFrame;
    public Transform box;
    
    private void OnEnable()
    {
        Api.Instance.GeographicApi.RegisterGeographicTransform(coordinateFrame);
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        Api.Instance.CameraApi.MoveTo(startPosition, distanceFromInterest: 1000, headingDegrees: 0, tiltDegrees: 45);
        coordinateFrame.SetPosition(startPosition);
        box.SetParent(coordinateFrame.transform);
        box.localPosition = new Vector3(0.0f, 80.0f, 0.0f);
        box.localRotation = Quaternion.identity;

        while (true)
        {
            box.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Api.Instance.GeographicApi.UnregisterGeographicTransform(coordinateFrame);
    }
}

