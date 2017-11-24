using UnityEngine;

public class WRLDStreamingCameraHandler : MonoBehaviour
{
    [SerializeField]
    private Transform m_wrldMapTransform;

    //We want to make sure that our Streaming Camera streams that same area of the map
    //that is being observer by the ARKit Camera. To achieve this we will set the position and
    //rotation of our Streaming Camera according to the ARKit Camera.
    void Update()
    {
        //Getting the main camera. In our case this will be the ARKit Camera
        Transform mainCameraTransform = Camera.main.transform;

        //We want our streaming camera to look where ever we are looking
        transform.rotation = mainCameraTransform.rotation;
        //Transforming the ARKit camera position so our Streaming Camera
        //positions itself according to our WRLD Scale
        transform.position = m_wrldMapTransform.InverseTransformPoint(mainCameraTransform.position);
    }
}