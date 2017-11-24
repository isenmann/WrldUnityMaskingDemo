using System.Collections;
using Wrld;
using Wrld.Resources.Buildings;
using Wrld.Space;
using UnityEngine;

public class PickingBuildings : MonoBehaviour
{
    public Material highlightMaterial;
    private Vector3 mouseDownPosition;


    void OnEnable()
    {
        var cameraLocation = LatLong.FromDegrees(37.795641, -122.404173);
        Api.Instance.CameraApi.MoveTo(cameraLocation, distanceFromInterest: 400, headingDegrees: 0, tiltDegrees: 45);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0) && Vector3.Distance(mouseDownPosition, Input.mousePosition) < 5.0f)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                var viewportPoint = Camera.main.WorldToViewportPoint(hit.point);
                var latLongAlt = Api.Instance.CameraApi.ViewportToGeographicPoint(viewportPoint, Camera.main);
                Api.Instance.BuildingsApi.HighlightBuildingAtLocation(latLongAlt, highlightMaterial, OnHighlightReceived);
            }
        }
    }

    void OnHighlightReceived(bool success, Highlight highlight)
    {
        if (success)
        {
            StartCoroutine(ClearHighlight(highlight));
        }
    }

    IEnumerator ClearHighlight(Highlight highlight)
    {
        yield return new WaitForSeconds(4.0f);
        Api.Instance.BuildingsApi.ClearHighlight(highlight);
    }
}
