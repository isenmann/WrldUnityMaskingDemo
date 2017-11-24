using System.Collections;
using UnityEngine;
using Wrld;
using Wrld.Space;

public class PositionObjectAtECEFCoordinate: MonoBehaviour
{
    static LatLong cameraPosition = LatLong.FromDegrees(37.783372, -122.400834);

    public GeographicTransform coordinateFrame;
    public Transform box;

    public GeographicTransform targetCoordinateFrame;

    private void OnEnable()
    {
        Api.Instance.GeographicApi.RegisterGeographicTransform(coordinateFrame);
        Api.Instance.GeographicApi.RegisterGeographicTransform(targetCoordinateFrame);
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        Api.Instance.CameraApi.MoveTo(cameraPosition, distanceFromInterest: 1000, headingDegrees: 0, tiltDegrees: 45);
        box.localPosition = new Vector3(0.0f, 40.0f, 0.0f);

        var ecefPointA = coordinateFrame.GetEcefPosition();
        var ecefPointB = targetCoordinateFrame.GetEcefPosition();

        while (true)
        {
            yield return new WaitForSeconds(2.0f);
            coordinateFrame.SetPosition(ecefPointB);
            yield return new WaitForSeconds(2.0f);
            coordinateFrame.SetPosition(ecefPointA);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Api.Instance.GeographicApi.UnregisterGeographicTransform(coordinateFrame);
        Api.Instance.GeographicApi.UnregisterGeographicTransform(targetCoordinateFrame);
    }
}

