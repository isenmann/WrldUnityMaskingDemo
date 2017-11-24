using System.Collections;
using UnityEngine;
using Wrld;
using Wrld.Space;

public class MoveObjectOnMap: MonoBehaviour
{
    static LatLong startPosition = LatLong.FromDegrees(37.783372, -122.400834);
    static float movementSpeed = 100.0f;

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

        var direction = 1.0f;

        while (true)
        {
            float forwardTimeElapsed = 0.0f;
            while (forwardTimeElapsed < 2.0f)
            {
                forwardTimeElapsed += Time.deltaTime;
                var movement = Vector3.forward * movementSpeed * direction * Time.deltaTime;
                box.Translate(movement); 
                yield return null;
            }

            direction = -direction;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        Api.Instance.GeographicApi.UnregisterGeographicTransform(coordinateFrame);
    }
}

