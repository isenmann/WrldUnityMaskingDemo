using System.Collections.Generic;


namespace Wrld.Space
{
    public class GeographicApi
    {
        List<GeographicTransform> m_geographicTransforms = new List<GeographicTransform>();

        internal void UpdateTransforms(ITransformUpdateStrategy updateStrategy)
        {
            foreach (var geographicTransform in m_geographicTransforms)
            {
                geographicTransform.UpdateTransform(updateStrategy);
            }
        }

        /// <summary>
        /// Register a GeographicTransform object to have its position updated by the API.
        /// </summary>
        /// <param name="geographicTransform">The GeographicTransform object to register and start updating.</param>
        public void RegisterGeographicTransform(GeographicTransform geographicTransform)
        {
            m_geographicTransforms.Add(geographicTransform);
        }

        /// <summary>
        /// Unregister a GeographicTransform and stop updating its position.
        /// </summary>
        /// <param name="geographicTransform"> The GeographicTransform object to stop updating.</param>
        public void UnregisterGeographicTransform(GeographicTransform geographicTransform)
        {
            m_geographicTransforms.Remove(geographicTransform);
        }
    }
}
