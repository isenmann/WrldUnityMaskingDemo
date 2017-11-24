using Wrld.MapCamera;
using Wrld.Resources.Buildings;
using Wrld.Space;
using Wrld.Scripts.Utilities;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Wrld
{
    /// <summary>
    /// The Wrld API defines two different map behaviors / systems which allow different ways
    /// to interact with Unity. Each system has its pros and cons outlined in the Examples page.
    /// </summary>
    public enum CoordinateSystem
    {
        /// <summary>
        /// This system allows easier interaction with Unity by grounding all meshes as per Unitys coordinate system.
        /// The main disadvantage is that is doesn't support interacting with faraway map points very well.
        /// </summary>
        UnityWorld,

        /// <summary>
        /// This system very closely represents the curvature of the earth and has accurate rendering and manipulation of meshes.
        /// It does not work seamlessly with the Unity coordinate system and requires additional calculation to manipulate objects.
        /// </summary>
        ECEF
    }

    public class Api
    {
        // TODO: Should be private with an accessor
        
        /// <summary>
        /// Single static instance of the Api class. Is used to access other APIs and methods.
        /// </summary>
        public static Api Instance = null;

        private ConfigParams.CollisionConfig CollisionStates ;
        const string ApiNullAssertMessage = "API is uninitialized. Please call CreateApi(...) before making any calls to the API";
        const string InvalidApiKeyExceptionMessage = "\"{0}\" is not a valid API key.  Please get a key from https://wrld3d.com/developers/apikeys.";

        private ApiImplementation m_implementation;

        private Api(string apiKey, CoordinateSystem coordinateSystem, Transform parentTransformForStreamedObjects, ConfigParams configParams)
        {
            CollisionStates = configParams.Collisions;

            if (!APIKeyHelpers.AppearsValid(apiKey))
            {
                throw new InvalidApiKeyException(string.Format(InvalidApiKeyExceptionMessage, apiKey));
            }

            try
            {
                m_implementation = new ApiImplementation(apiKey, coordinateSystem, parentTransformForStreamedObjects, configParams);
            }
            catch (DllNotFoundException dllNotFound)
            {
                bool couldNotFindWRLDBinary = dllNotFound.Message.ToLower().Contains("streamalpha");
                bool is32Bit = IntPtr.Size == 4;

                if (couldNotFindWRLDBinary && is32Bit)
                {
                    var guiTextObject = new GameObject("OtherErrorMessage");
                    var errorMessage = guiTextObject.AddComponent<ErrorMessage>();
                    errorMessage.Title = "WRLD Error: Unsupported Build Architecture";
                    errorMessage.Text =
                        "It looks like you're trying to run a 32 bit build of the game.  Unfortunately that isn't currently supported.\n\n" +
                        "Please go to 'File->Build Settings' in the Unity menu and select 'x86_64' as your Architecture to continue.";
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Initializes the API instance. This starts up the streaming system. Preferably, this should be called from within Awake() and before accessing the Api.Instance. Any subsequent calls to Api.Create will throw an exception.
        /// </summary>
        /// <param name="apikey">Your WRLD API key</param>
        /// <param name="coordinateSystem">The world space map behaviour. Cannot be changed once map is loaded.</param>
        /// <param name="parentTransformForStreamedObjects">Parent object to attach streamed objects to.</param>
        /// <param name="configParams">Configuration data needed by the map when loading.</param>
        public static void Create(string apikey, CoordinateSystem coordinateSystem, Transform parentTransformForStreamedObjects, ConfigParams configParams)
        {
            if (Instance == null)
            {
                Instance = new Api(apikey, coordinateSystem, parentTransformForStreamedObjects, configParams);
            }
            else
            {
                throw new ArgumentException("Api has already been initialized. Use Api.Instance to access it.");
            }
        }

        /// <summary>
        /// Calculates the ECEF origin point from the camera, builds up a frustum and updates these values in the native plugin which then uses this data to make streaming requests. This should be called once per frame in the Update() method.
        /// </summary>
        /// <param name="camera">A Unity camera that can provide the frustum for streaming.</param>
        public void StreamResourcesForCamera(UnityEngine.Camera camera)
        {
            m_implementation.StreamResourcesForCamera(camera);
        }

        /// <summary>
        /// Updates Wrld map resources. Should be called once per frame in Unity Mono Update().
        /// </summary>
        public void Update()
        {
            m_implementation.Update();
        }

        internal void SetEnabled(bool enabled)
        {
            m_implementation.SetEnabled(enabled);
        }

        /// <summary>
        /// Uninitializes the API instance. This frees up any resources allocated by the plugins including all streamed meshes in the scene. Preferably, this should be called from within the OnApplicationQuit() method.
        /// </summary>
        public void Destroy()
        {
            m_implementation.Destroy();
            Instance = null;
        }

        /// <summary>
        /// This function has different behaviour depending on the coordinate system in use.
        /// UnityWorld - Resets the root latitude and longitude of the world such that Unity&apos;s origin(0,0) is centered at that point.
        /// ECEF - Sets the maps world position to that latitude and longitude such that the camera is positioned at that point.
        /// </summary>
        /// <param name="lla">the latitude, longitude and altitude of the new point.</param>
        public void SetOriginPoint(Space.LatLongAltitude lla)
        {
            m_implementation.SetOriginPoint(lla);
        }

        public void OnApplicationPaused()
        {
            m_implementation.OnApplicationPaused();
        }

        public void OnApplicationResumed()
        {
            m_implementation.OnApplicationResumed();
        }

        /// <summary>
        ///  Allows accessing the camera API endpoints to interact with the map.
        /// </summary>
        public CameraApi CameraApi
        {
            get
            {
                return m_implementation.CameraApi;
            }
        }

        /// <summary>
        ///  Allows accessing the buildings API endpoints to interact with buildings on the map.
        /// </summary>
        public BuildingsApi BuildingsApi
        {
            get
            {
                return m_implementation.BuildingsApi;
            }
        }


        public GeographicApi GeographicApi
        {
            get
            {
                return m_implementation.GeographicApi;
            }
        }

        /// <summary>
        /// Allows you to enable or disable the generation of collision meshes for terrain, road and buildings when they stream in. This does not enable or disable collision meshes for terrain, road and buildings which have already streamed in.
        /// </summary>
        /// <param name="terrain">Whether to enable or disable the generation of collision meshes for terrain.</param>
        /// <param name="road">Whether to enable or disable the generation of collision meshes for road.</param>
        /// <param name="buildings">Whether to enable or disable the generation of collision meshes for buildings.</param>
        public void UpdateCollisions(bool terrain, bool road, bool buildings)
        {
            CollisionStates.TerrainCollision = terrain;
            CollisionStates.RoadCollision = road;
            CollisionStates.BuildingCollision = buildings;

            m_implementation.UpdateCollision(CollisionStates);
        }

    }
}
