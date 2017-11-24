using AOT;
using Wrld.Space;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Helpers;

namespace Wrld.MapCamera
{
    /// <summary>
    /// Contains endpoints to interact with the map.
    /// </summary>
    public class CameraApi
    {
        private const string m_nonNullCameraMessage = "A non-null camera must be supplied, or have been set via SetControlledCamera.";

        public delegate void TransitionStartHandler(CameraApi cameraApi, UnityEngine.Camera camera);
        public delegate void TransitionEndHandler(CameraApi cameraApi, UnityEngine.Camera camera);

        public event TransitionStartHandler OnTransitionStart;
        public event TransitionEndHandler OnTransitionEnd;

        /// <summary>
        ///  Checks to see if the Wrld map controlled camera is currently undergoing a transition.
        /// </summary>
        public bool IsTransitioning { get; private set; }

        public CameraApi(ApiImplementation apiImplementation)
        {
            m_apiImplementation = apiImplementation;
            OnTransitionStart += TransitionStarted;
            OnTransitionEnd += TransitionEnded;
            m_inputHandler = new CameraInputHandler();
        }

        /// <summary>
        /// Sets the camera that is then controlled by the Wrld map.
        /// </summary>
        /// <param name="camera">A Unity camera that can provide the frustum for streaming and is controlled by the Wrld map</param>
        public void SetControlledCamera(UnityEngine.Camera camera)
        {
            m_controlledCamera = camera;
        }

        /// <summary>
        /// Returns the camera that is currently being controlled by the map
        /// </summary>
        public UnityEngine.Camera GetControlledCamera()
        {
            return m_controlledCamera;
        }

        /// <summary>
        /// Removes any Unity camera under control of the Wrld Map.
        /// </summary>
        public void ClearControlledCamera()
        {
            m_controlledCamera = null;
        }


        /// <summary>
        /// Transforms a point from local Unity space to a geographic coordinate using the supplied camera. If no camera is specified, the currently controlled camera will be used.
        /// Note: If using the ECEF coordinate system, the returned position will only be valid until the camera is moved. To robustly position an object on the map, use the GeographicTransform component.
        /// </summary>
        /// <param name="position">The geographical coordinates of the position to transform to local space.</param>
        /// <param name="camera">The camera for which to perform the transform. If this is left out, any camera which has previously been passed to SetControlledCamera will be used.</param>
        /// <returns>The transformed geographic LatLongAltitude in local space.</returns>
        public Vector3 GeographicToWorldPoint(LatLongAltitude position, Camera camera = null)
        {
            camera = camera ?? m_controlledCamera;
            if (camera == null)
            {
                throw new ArgumentNullException("camera", m_nonNullCameraMessage);
            }
            return m_apiImplementation.GeographicToWorldPoint(position, camera);
        }

        /// <summary>
        /// Transforms a point from local Unity space to a geographic coordinate using the supplied camera. If no camera is specified, the currently controlled camera will be used.
        /// Note: If using the ECEF coordinate system, the returned position will only be valid until the camera is moved. To robustly position an object on the map, use the GeographicTransform component.
        /// </summary>
        /// <param name="position">The world position to transform into a geographic coordinate.</param>
        /// <param name="camera">The camera for which to perform the transform. If this is left out, any camera which has previously been passed to SetControlledCamera will be used.</param>
        /// <returns>The transformed world position as a LatLongAltitude.</returns>
        public LatLongAltitude WorldToGeographicPoint(Vector3 position, Camera camera = null)
        {
            camera = camera ?? m_controlledCamera;
            if (camera == null)
            {
                throw new ArgumentNullException("camera", m_nonNullCameraMessage);
            }
            return m_apiImplementation.WorldToGeographicPoint(position, camera);
        }


        /// <summary>
        /// Transforms the supplied geographical coordinates into viewport space, using the supplied camera (if specified) or the camera previously set via SetControlledCamera if not.
        /// </summary>
        /// <param name="position">The geographical coordinates of the position to transform to viewport space.</param>
        /// <param name="camera">The camera for which to perform the transform. If this is left out, any camera which has previously been passed to SetControlledCamera will be used.</param>
        /// <returns>The transformed geographic LatLongAltitude in viewport space.</returns>
        public Vector3 GeographicToViewportPoint(LatLongAltitude position, Camera camera = null)
        {
            camera = camera ?? m_controlledCamera;

            if (camera == null)
            {
                throw new ArgumentNullException("camera", m_nonNullCameraMessage);
            }

            return m_apiImplementation.GeographicToViewportPoint(position, camera);
        }

        /// <summary>
        /// Transforms the supplied viewport space coordinates into LatLongAltitude geographical coordinates, using the supplied camera (if specified) or the camera previously set via SetControlledCamera if not.
        /// </summary>
        /// <param name="viewportSpacePosition">The viewport-space coordinates to transform to geographical coordinates.</param>
        /// <param name="camera">The camera for which to perform the transform. If this is left out, any camera which has previously been passed to SetControlledCamera will be used.</param>
        /// <returns>The transformed viewport space coordinates as a LatLongAltitude.</returns>
        public LatLongAltitude ViewportToGeographicPoint(Vector3 viewportSpacePosition, Camera camera = null)
        {
            camera = camera ?? m_controlledCamera;

            if (camera == null)
            {
                throw new ArgumentNullException("camera", m_nonNullCameraMessage);
            }

            return m_apiImplementation.ViewportToGeographicPoint(viewportSpacePosition, camera);
        }


        /// <summary>
        /// Transforms the supplied screen space coordinates into LatLongAltitude geographical coordinates, using the supplied camera (if specified) or the camera previously set via SetControlledCamera if not.
        /// </summary>
        /// <param name="screenSpacePosition">The screen space coordinates of the position to transform to geographical coordinates.</param>
        /// <param name="camera">The camera for which to perform the transform. If this is left out, any camera which has previously been passed to SetControlledCamera will be used.</param>
        /// <returns>The transformed screen space coordinates as a LatLongAltitude.</returns>
        public LatLongAltitude ScreenToGeographicPoint(Vector3 screenSpacePosition, Camera camera = null)
        {
            camera = camera ?? m_controlledCamera;

            if (camera == null)
            {
                throw new ArgumentNullException("camera", m_nonNullCameraMessage);
            }

            return ViewportToGeographicPoint(camera.ScreenToViewportPoint(screenSpacePosition), camera);
        }

        /// <summary>
        /// Transforms the supplied geographical coordinates into screen space, using the supplied camera (if specified) or the camera previously set via SetControlledCamera if not.
        /// </summary>
        /// <param name="position">The geographical coordinates of the position to transform to screen space.</param>
        /// <param name="camera">The camera for which to perform the transform. If this is left out, any camera which has previously been passed to SetControlledCamera will be used.</param>
        /// <returns>The transformed geographic LatLongAltitude in screen space.</returns>
        public Vector3 GeographicToScreenPoint(LatLongAltitude position, Camera camera = null)
        {
            camera = camera ?? m_controlledCamera;

            if (camera == null)
            {
                throw new ArgumentNullException("camera", m_nonNullCameraMessage);
            }

            return camera.ViewportToScreenPoint(GeographicToViewportPoint(position, camera));
        }


        public void Update()
        {
            if (m_controlledCamera != null)
            {
                m_inputHandler.Update();
                var cameraState = new NativeCameraState();
                GetCurrentCameraState(NativePluginRunner.API, ref cameraState);
                m_apiImplementation.ApplyNativeCameraState(cameraState, m_controlledCamera);
            }
        }

        /// <summary>
        /// Moves the camera to view the supplied interest point instantaneously, without any animation.
        /// Requires that a camera has been set using SetControlledCamera.
        /// </summary>
        /// <param name="interestPoint">The latitude and longitude of the point on the ground which the camera should look at.</param>
        /// <param name="distanceFromInterest">Optional. The distance in metres from the interest point at which the camera should sit. If unspecified/null the altitude is set to 0.0.</param>
        /// <param name="headingDegrees">Optional. The heading in degrees (0, 360) with which to view the target point, with 0 facing north, 90 east, etc. If unspecified/null the heading with which the camera&apos;s previous interest point was viewed will be maintained.</param>
        /// <param name="tiltDegrees">Optional. The camera tilt in degrees, where a value of 0 represents a camera looking straight down at the interest point, along the direction of gravity.</param>
        /// <returns>Weather the camera sucessfully moved or not.</returns>
        public bool MoveTo(
            LatLong interestPoint,
            double? distanceFromInterest = null,
            double? headingDegrees = null,
            double? tiltDegrees = null)
        {
            if (m_controlledCamera == null)
            {
                throw new ArgumentNullException("Camera", m_nonNullCameraMessage);
            }

            const bool animated = false;
            const bool modifyPosition = true;
            const double transitionDuration = 0.0;
            const bool hasTransitionDuration = false;
            const bool jumpIfFarAway = true;
            const bool allowInterruption = true;
            const double interestAltitude = 0.0;

            return SetView(
                NativePluginRunner.API,
                animated,
                interestPoint.GetLatitude(), interestPoint.GetLongitude(), interestAltitude, modifyPosition,
                distanceFromInterest.HasValue ? distanceFromInterest.Value : 0.0, distanceFromInterest.HasValue,
                headingDegrees.HasValue ? headingDegrees.Value : 0.0, headingDegrees.HasValue,
                tiltDegrees.HasValue ? tiltDegrees.Value : 0.0, tiltDegrees.HasValue,
                transitionDuration, hasTransitionDuration,
                jumpIfFarAway, allowInterruption
                );
        }

        /// <summary>
        /// Moves the camera to view the supplied interest point instantaneously, without any animation. Requires that a camera has been set using SetControlledCamera.
        /// </summary>
        /// <param name="interestPoint">The latitude and longitude of the point on the ground which the camera should look at.</param>
        /// <param name="cameraPosition">The latitude, longitude and altitude from which the camera will look at the interest point.</param>
        /// <returns>Weather the camera sucessfully moved or not.</returns>
        public bool MoveTo(
            LatLong interestPoint,
            LatLongAltitude cameraPosition)
        {
            if (m_controlledCamera == null)
            {
                throw new ArgumentNullException("Camera", m_nonNullCameraMessage);
            }

            double distance;
            double headingDegrees;
            double tiltDegrees;
            GetTiltHeadingAndDistanceFromCameraAndTargetPosition(interestPoint, cameraPosition, out tiltDegrees, out headingDegrees, out distance);

            return MoveTo(interestPoint, distance, headingDegrees, tiltDegrees);
        }

        /// <summary>
        /// Smoothly animates the camera to view the supplied interest point. Requires that a camera has been set using SetControlledCamera.
        /// </summary>
        /// <param name="interestPoint">The latitude and longitude of the point on the ground which the camera should be looking at once the transition is complete.</param>
        /// <param name="distanceFromInterest">Optional. The distance in metres from the interest point at which the camera should sit. If unspecified/null the distance to the previous interest point is maintained.</param>
        /// <param name="headingDegrees">Optional. The heading in degrees (0, 360) with which to view the target point, with 0 facing north, 90 east, etc. If unspecified/null the heading with which the camera&apos;s previous interest point was viewed will be maintained.</param>
        /// <param name="tiltDegrees">Optional. The camera tilt in degrees, where a value of 0 represents a camera looking straight down at the interest point, along the direction of gravity.</param>
        /// <param name="transitionDuration">Optional. The total duration of the transition, in seconds. If not specified the duration will be calculated from the distance to be travelled and the camera&apos;s maximum speed.</param>
        /// <param name="jumpIfFarAway">Optional. By default AnimateTo will provide a smooth transition for short distances, but an instantaneous transition if there is a large distance to be covered (rather than waiting for a lengthy animation to play). If you want to override this behaviour and force an animation (even over large distances), you can set this to false.</param>
        /// <returns>Weather the camera sucessfully amimated or not.</returns>
        public bool AnimateTo(
            LatLong interestPoint,
            double? distanceFromInterest = null,
            double? headingDegrees = null,
            double? tiltDegrees = null,
            double? transitionDuration = null,
            bool jumpIfFarAway = true)
        {
            if (m_controlledCamera == null)
            {
                throw new ArgumentNullException("Camera", m_nonNullCameraMessage);
            }

            const bool animated = true;
            const bool modifyPosition = true;
            const bool allowInterruption = true;
            const double interestAltitude = 0.0;

            return SetView(
                NativePluginRunner.API,
                animated,
                interestPoint.GetLatitude(), interestPoint.GetLongitude(), interestAltitude, modifyPosition,
                distanceFromInterest.HasValue ? distanceFromInterest.Value : 0.0, distanceFromInterest.HasValue,
                headingDegrees.HasValue ? headingDegrees.Value : 0.0, headingDegrees.HasValue,
                tiltDegrees.HasValue ? tiltDegrees.Value : 0.0, tiltDegrees.HasValue,
                transitionDuration.HasValue ? transitionDuration.Value : 0.0, transitionDuration.HasValue,
                jumpIfFarAway,
                allowInterruption
                );
        }

        /// <summary>
        /// Smoothly animates the camera to view the supplied interest point. Requires that a camera has been set using SetControlledCamera.
        /// </summary>
        /// <param name="interestPoint">The latitude and longitude of the point on the ground which the camera should be looking at once the transition is complete.</param>
        /// <param name="cameraPosition">The latitude, longitude and altitude from which the camera will look at the interest point when the transition is complete.</param>
        /// <param name="transitionDuration">Optional. The total duration of the transition, in seconds. If not specified the duration will be calculated from the distance to be travelled and the camera&apos;s maximum speed.</param>
        /// <param name="jumpIfFarAway">Optional. By default AnimateTo will provide a smooth transition for short distances, but an instantaneous transition if there is a large distance to be covered (rather than waiting for a lengthy animation to play). If you want to override this behaviour and force an animation (even over large distances), you can set this to false.</param>
        /// <returns>Weather the camera sucessfully amimated or not.</returns>
        public bool AnimateTo(
            LatLong interestPoint,
            LatLongAltitude cameraPosition,
            double? transitionDuration = null,
            bool jumpIfFarAway = true)
        {
            double distance;
            double headingDegrees;
            double tiltDegrees;
            GetTiltHeadingAndDistanceFromCameraAndTargetPosition(interestPoint, cameraPosition, out tiltDegrees, out headingDegrees, out distance);

            return AnimateTo(interestPoint, distance, headingDegrees, tiltDegrees, transitionDuration, jumpIfFarAway);
        }

        public bool HasControlledCamera { get { return m_controlledCamera != null; } }

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool SetView(
            IntPtr ptr,
            [MarshalAs(UnmanagedType.I1)] bool animated,
            double latDegrees, double longDegrees, double altitude, [MarshalAs(UnmanagedType.I1)] bool modifyPosition,
            double distance, [MarshalAs(UnmanagedType.I1)] bool modifyDistance,
            double headingDegrees, [MarshalAs(UnmanagedType.I1)] bool modifyHeading,
            double tiltDegrees, [MarshalAs(UnmanagedType.I1)] bool modifyTilt,
            double transitionDurationSeconds, [MarshalAs(UnmanagedType.I1)] bool hasTransitionDuration,
            [MarshalAs(UnmanagedType.I1)] bool jumpIfFarAway,
            [MarshalAs(UnmanagedType.I1)] bool allowInterruption
            );

        [DllImport(NativePluginRunner.DLL)]
        private static extern void GetCurrentCameraState(IntPtr ptr, ref NativeCameraState cameraState);

        private void TransitionStarted(CameraApi controller, UnityEngine.Camera camera)
        {
            IsTransitioning = true;
        }

        private void TransitionEnded(CameraApi controller, UnityEngine.Camera camera)
        {
            IsTransitioning = false;
        }
        private static void GetTiltHeadingAndDistanceFromCameraAndTargetPosition(LatLong interestPoint, LatLongAltitude cameraPosition, out double tiltDegrees, out double headingDegrees, out double distance)
        {
            double distanceAlongGround = LatLong.EstimateGreatCircleDistance(interestPoint, cameraPosition.GetLatLong());
            double cameraAltitude = cameraPosition.GetAltitude();
            distance = Math.Sqrt(distanceAlongGround * distanceAlongGround + cameraAltitude * cameraAltitude);
            headingDegrees = cameraPosition.BearingTo(interestPoint);
            tiltDegrees = MathsHelpers.Rad2Deg(Math.PI * 0.5 - Math.Atan2(cameraAltitude, distanceAlongGround));
        }

        internal enum CameraEventType
        {
            Move,
            MoveStart,
            MoveEnd,
            Drag,
            DragStart,
            DragEnd,
            Pan,
            PanStart,
            PanEnd,
            Rotate,
            RotateStart,
            RotateEnd,
            Tilt,
            TiltStart,
            TiltEnd,
            Zoom,
            ZoomStart,
            ZoomEnd,
            TransitionStart,
            TransitionEnd
        };

        internal delegate void CameraEventCallback(CameraEventType eventId);

        [MonoPInvokeCallback(typeof(CameraEventCallback))]
        internal static void OnCameraEvent(CameraEventType eventID)
        {
            var controller = Api.Instance.CameraApi;

            if (eventID == CameraEventType.TransitionStart)
            {
                controller.OnTransitionStart.Invoke(controller, controller.m_controlledCamera);
            }
            else if (eventID == CameraEventType.TransitionEnd)
            {
                controller.OnTransitionEnd.Invoke(controller, controller.m_controlledCamera);
            }
            // :TODO: handle other events
        }

        private UnityEngine.Camera m_controlledCamera;
        private ApiImplementation m_apiImplementation;
        private CameraInputHandler m_inputHandler;
    }
}
