using System.Collections;
using Wrld;
using Wrld.Resources.Buildings;
using Wrld.Space;
using UnityEngine;

public class HighlightingBuildings : MonoBehaviour
{
    [SerializeField]
    private Material material;

    private LatLong buildingLocation = LatLong.FromDegrees(37.795189, -122.402777);


    private void OnEnable()
    {
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        Api.Instance.CameraApi.MoveTo(buildingLocation, distanceFromInterest: 1000, headingDegrees: 0, tiltDegrees: 45);

        while (true)
        {
            yield return new WaitForSeconds(4.0f);

            Api.Instance.BuildingsApi.HighlightBuildingAtLocation(buildingLocation, material, OnBuildingHighlighted);
        }
    }

    void OnBuildingHighlighted(bool success, Highlight highlight)
    {
        if (success)
        {
            Debug.Log(highlight.ToString());
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
