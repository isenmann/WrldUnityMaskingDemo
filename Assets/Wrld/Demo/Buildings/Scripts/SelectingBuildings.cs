using System.Collections;
using Wrld;
using Wrld.Space;
using Wrld.Resources.Buildings;
using UnityEngine;

public class SelectingBuildings : MonoBehaviour
{
    [SerializeField]
    private GameObject boxPrefab;

    private LatLong buildingLocation = LatLong.FromDegrees(37.793988, -122.403390);


    private void OnEnable()
    {
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        Api.Instance.CameraApi.MoveTo(buildingLocation, distanceFromInterest: 500, headingDegrees: 0, tiltDegrees: 45);

        while (true)
        {
            yield return new WaitForSeconds(4.0f);

            Api.Instance.BuildingsApi.GetBuildingAtLocation(buildingLocation, OnBuildingSelected);
        }
    }

    void OnBuildingSelected(bool success, Building building)
    {
        if (success)
        {
            var boxLocation = LatLong.FromDegrees(building.Centroid.GetLatitude(), building.Centroid.GetLongitude());
            var boxAnchor = Instantiate(boxPrefab) as GameObject;
            boxAnchor.GetComponent<GeographicTransform>().SetPosition(boxLocation);

            var box = boxAnchor.transform.GetChild(0);
            box.localPosition = new Vector3(0.0f, (float)building.TopAltitude, 0.0f);
            Destroy(boxAnchor, 2.0f);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
