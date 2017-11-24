using System.Collections;
using Wrld;
using Wrld.Space;
using UnityEngine;

public class BuildingAltitudePicking : MonoBehaviour
{
    [SerializeField]
    private GameObject boxPrefab;

    private LatLong cameraLocation = LatLong.FromDegrees(37.795641, -122.404173);
    private LatLong boxLocation1 = LatLong.FromDegrees(37.795159, -122.404336);
    private LatLong boxLocation2 = LatLong.FromDegrees(37.795173, -122.404229);


    private void OnEnable()
    {
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        Api.Instance.CameraApi.MoveTo(cameraLocation, distanceFromInterest: 400, headingDegrees: 0, tiltDegrees: 45);

        while (true)
        {
            yield return new WaitForSeconds(4.0f);

            MakeBox(boxLocation1);
            MakeBox(boxLocation2);
        }
    }

    void MakeBox(LatLong latLong)
    {
        double altitude;
        var success = Api.Instance.BuildingsApi.TryGetAltitudeAtLocation(latLong, out altitude);
        if (success)
        {
            var boxLocation = LatLong.FromDegrees(latLong.GetLatitude(), latLong.GetLongitude());
            var boxAnchor = Instantiate(boxPrefab) as GameObject;
            boxAnchor.GetComponent<GeographicTransform>().SetPosition(boxLocation);

            var box = boxAnchor.transform.GetChild(0);
            box.localPosition = new Vector3(0.0f, (float)altitude, 0.0f);
            Destroy(boxAnchor, 2.0f);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
