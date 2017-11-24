using System.Collections;
using UnityEngine;
using Wrld;
using Wrld.Space;

public class FlyObjectOverMap: MonoBehaviour
{
    static LatLong startPosition = LatLong.FromDegrees(37.783372, -122.400834);
    static float movementSpeed = 400.0f;
    static float turningSpeed = 270.0f;

    public GeographicTransform coordinateFrame;
    public Transform box;
    
    void OnEnable()
    {
        Api.Instance.GeographicApi.RegisterGeographicTransform(coordinateFrame);

        Api.Instance.CameraApi.MoveTo(startPosition, distanceFromInterest: 1700, headingDegrees: 0, tiltDegrees: 45);
        coordinateFrame.SetPosition(startPosition);
        box.SetParent(coordinateFrame.transform);
        box.localPosition = new Vector3(0.0f, 300.0f, 0.0f);
        box.localRotation = Quaternion.identity;
    }

    void Update()
    {
        float rotationDelta = Input.GetAxis("Horizontal") * turningSpeed * Time.deltaTime;
        box.Rotate(Vector3.up, rotationDelta);

        float movementDelta = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        Vector3 movement = Vector3.forward * movementDelta;
        box.Translate(movement);
    }

    void OnDisable()
    {
        Api.Instance.GeographicApi.UnregisterGeographicTransform(coordinateFrame);
    }
}

