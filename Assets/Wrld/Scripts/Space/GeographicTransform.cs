using UnityEngine;
using Wrld;
using Wrld.Common.Maths;


namespace Wrld.Space
{
    /// <summary>
    /// A GeographicTransform behaviour is used to place a GameObject somewhere on the globe.
    /// It will keep the object correctly positioned and oriented regardless of the coordinate system or camera location used by the map.
    /// This GameObject can then serve as a coordinate frame for its children which can be placed and moved as normal.
    ///
    /// In order for a GeographicTransform's position to be updated, the API must be made aware of it via the GeographicApi.RegisterGeographicTransform method.
    /// This is called automaticall OnEnable, but can also be called manually if more control over updating is required.
    /// </summary>
    public class GeographicTransform: MonoBehaviour
    {
        [SerializeField]
        /// <summary>
        /// The initial latitude of the object in degrees.
        /// </summary>
        private double InitialLatitude;

        [SerializeField]
        /// <summary>
        /// The initial longitude of the object in degrees.
        /// </summary>
        private double InitialLongitude;

        [SerializeField]
        /// <summary>
        /// The initial heading of the object in degrees, clockwise, relative to north.
        /// </summary>
        private float InitialHeadingInDegrees;


        public EcefTangentBasis TangentBasis { get; private set; }

        bool m_hasEverBeenRegistered = false;


        void RegisterSelf()
        {
            if (Api.Instance != null)
            {
                Api.Instance.GeographicApi.RegisterGeographicTransform(this);
                m_hasEverBeenRegistered = true;
            }
        }

        void UnregisterSelf()
        {
            if (Api.Instance != null)
            {
                Api.Instance.GeographicApi.UnregisterGeographicTransform(this);
            }
        }

        void OnEnable()
        {
            RegisterSelf();
        }

        void OnDisable()
        {
            UnregisterSelf();
        }

        void Awake()
        {
            var ecefPoint = LatLong.FromDegrees(InitialLatitude, InitialLongitude).ToECEF();
            var heading = InitialHeadingInDegrees;
            TangentBasis = EcefHelpers.EcefTangentBasisFromPointAndHeading(ecefPoint, heading);
        }

        void Start()
        {
            if (!m_hasEverBeenRegistered)
            {
                RegisterSelf();
            }
        }

        internal void UpdateTransform(ITransformUpdateStrategy updateStrategy)
        {
            updateStrategy.UpdateTransform(transform, TangentBasis.PointEcef, 0.0f);

            var rotation = Quaternion.LookRotation(TangentBasis.Forward, TangentBasis.Up);
            transform.localRotation *= rotation;
        }

        /// <summary>
        /// Set the position of this transform to the specified point.
        /// </summary>
        /// <param name="latLong">The new position of the transform.</param>
        public void SetPosition(LatLong latLong)
        {
            SetPosition(latLong.ToECEF());
        }

        /// <summary>
        /// Set the position of this transform to the specified point.
        /// </summary>
        /// <param name="ecefPosition">The new position of the object</param>
        public void SetPosition(DoubleVector3 ecefPosition)
        {
            TangentBasis.SetPoint(ecefPosition);
        }

        /// <summary>
        /// Set the heading in degrees of this transform, relative to north.
        /// </summary>
        /// <param name="headingInDegrees">The new heading of the transform.</param>
        public void SetHeading(float headingInDegrees)
        {
            TangentBasis = EcefHelpers.EcefTangentBasisFromPointAndHeading(TangentBasis.PointEcef, headingInDegrees);
        }

        /// <summary>
        /// Get the current latitude, longitude of this object.
        /// </summary>
        /// <returns>A LatLong representing this object's position</returns>
        public LatLong GetLatLong()
        {
            return CoordinateConversions.ConvertEcefToLatLongAltitude(TangentBasis.PointEcef).GetLatLong();
        }

        /// <summary>
        /// Get the current ECEF coordinate of this object.
        /// </summary>
        /// <returns>An ECEF coordinate representing this object's position</returns>
        public DoubleVector3 GetEcefPosition()
        {
            return TangentBasis.PointEcef;
        }
    }
}

