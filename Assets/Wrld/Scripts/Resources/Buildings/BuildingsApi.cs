using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Common.Maths;
using Wrld.Streaming;
using Wrld.Space;

namespace Wrld.Resources.Buildings
{
    /// <summary>
    /// Represents the minimum and maximum altitude of a buildings along with the latitude and longitude of its centroid.
    /// </summary>
    public struct Building
    {
        /// <summary>
        /// A string identifying the building.
        /// </summary>
        public string BuildingId;

        /// <summary>
        /// The altitude of the building at its lowest point.
        /// </summary>
        public double BaseAltitude;

        /// <summary>
        /// The altitude of the building at its highest point.
        /// </summary>
        public double TopAltitude;

        /// <summary>
        /// The centroid of the building.
        /// </summary>
        public LatLong Centroid;
    };


    /// <summary>
    /// A handle representing a building highlight. This can be passed to ClearHighlight.
    /// </summary>
    public class Highlight
    {
        internal string HighlightId;
        internal int HighlightRequestId;

        internal Highlight(string highlightId, int highlightRequestId)
        {
            this.HighlightId = highlightId;
            this.HighlightRequestId = highlightRequestId;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct BuildingInterop
    {
        public double BaseAltitude;
        public double TopAltitude;
        public double CentroidLatitude;
        public double CentroidLongitude;
        public IntPtr StringIdPtr;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HighlightInterop
    {
        public double OriginEcefX;
        public double OriginEcefY;
        public double OriginEcefZ;
        public int VertexCount;
        public int IndexCount;
        public IntPtr VertexPositions;
        public IntPtr Indices;
    }


    public class BuildingsApi
    {
        private static Dictionary<int, BuildingRequest> BuildingRequests;
        private static Dictionary<int, HighlightRequest> HighlightRequests;
        private static GameObjectStreamer HighlightStreamer;
        private static List<string> HighlightIdsToAdd;

        private int m_nextBuildingRequestId = 0;
        private int m_nextHighlightRequestId = 0;


        public BuildingsApi(GameObjectStreamer highlightStreamer)
        {
            BuildingRequests = new Dictionary<int, BuildingRequest>();
            HighlightRequests = new Dictionary<int, HighlightRequest>();
            HighlightStreamer = highlightStreamer;
            HighlightIdsToAdd = new List<string>();

            SetHighlightMeshUploadCallback(NativePluginRunner.API, OnHighlightMeshUpload);
            SetHighlightMeshClearCallback(NativePluginRunner.API, OnHighlightMeshClear);
        }


        public delegate void BuildingReceivedCallback(bool buildingReceived, Building building);
        public delegate void HighlightReceivedCallback(bool highlightReceived, Highlight handle);


        internal struct BuildingRequest
        {
            public BuildingReceivedCallback callback;
        }

        internal struct HighlightRequest
        {
            public Material material;
            public HighlightReceivedCallback callback;
        }


        /// <summary>
        /// Gets the building at a given location and returns it through the callback. This will only retrieve a building that has streamed in.
        /// </summary>
        /// <param name="location">The geographic point which intersects with the building to be returned.</param>
        /// <param name="callback">The callback where the building will be received.</param>
        public void GetBuildingAtLocation(LatLong location, BuildingReceivedCallback callback)
        {
            var latLongAlt = LatLongAltitude.FromDegrees(location.GetLatitude(), location.GetLongitude(), -1.0);
            GetBuildingAtLocation(latLongAlt, callback);
        }

        public void GetBuildingAtLocation(LatLongAltitude location, BuildingReceivedCallback callback)
        {
            int buildingRequestId = m_nextBuildingRequestId;
            BuildingRequest request;
            request.callback = callback;
            BuildingRequests.Add(buildingRequestId, request);
            NativeGetBuildingAtLocation(NativePluginRunner.API, location.GetLatitude(), location.GetLongitude(), location.GetAltitude(), OnBuildingReceived, buildingRequestId);
            m_nextBuildingRequestId += 1;
        }


        /// <summary>
        /// Gets the altitude of a building at a given location. This can only retreive the altitude for buildings which have streamed in.
        /// </summary>
        /// <param name="location">The geographic location to query the altitude of a building at.</param>
        /// <param name="out_altitude">The altitude of the building at the location</param>
        /// <returns>Whether there was a building to obtain a height from at the given location.</returns>
        public bool TryGetAltitudeAtLocation(LatLong location, out double out_altitude)
        {
            return GetBuildingAltitudeAtLatLong(NativePluginRunner.API, location.GetLatitude(), location.GetLongitude(), out out_altitude);
        }

        public bool TryGetAltitudeAtLocation(LatLongAltitude location, out double out_altitude)
        {
            return GetBuildingAltitudeAtLatLong(NativePluginRunner.API, location.GetLatitude(), location.GetLongitude(), out out_altitude);
        }


        /// <summary>
        /// Highlights a building at a given location and returns the highlight geometry as a GameObject through the callback. This can only generate highlight geometry for buildings which have streamed in.
        /// </summary>
        /// <param name="location">The geographic point which intersects with the building to be highlighted.</param>
        /// <param name="material">The material to assign the highlight geometry.</param>
        /// <param name="callback">The callback where the highlight will be received.</param>
        public void HighlightBuildingAtLocation(LatLong location, Material material, HighlightReceivedCallback callback)
        {
            var latLongAlt = LatLongAltitude.FromDegrees(location.GetLatitude(), location.GetLongitude(), -1.0);
            HighlightBuildingAtLocation(latLongAlt, material, callback);
        }

        public void HighlightBuildingAtLocation(LatLongAltitude location, Material material, HighlightReceivedCallback callback)
        {
            int highlightRequestId = m_nextHighlightRequestId;
            HighlightRequest request;
            request.material = material;
            request.callback = callback;
            HighlightRequests.Add(highlightRequestId, request);
            NativeHighlightBuildingAtLocation(NativePluginRunner.API, location.GetLatitude(), location.GetLongitude(), location.GetAltitude(), OnHighlightReceived, highlightRequestId);
            m_nextHighlightRequestId += 1;
        }


        /// <summary>
        /// Clears a given building highlight.
        /// </summary>
        /// <param name="highlight">The highight to clear.</param>
        public void ClearHighlight(Highlight highlight)
        {
            if (highlight != null)
            {
                NativeClearHighlight(NativePluginRunner.API, highlight.HighlightId);
                HighlightRequests.Remove(highlight.HighlightRequestId);
            }
        }


        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void SetHighlightMeshUploadCallback(IntPtr ptr, NativeHighlightMeshUploadCallback callback);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void SetHighlightMeshClearCallback(IntPtr ptr, NativeHighlightMeshClearCallback callback);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeGetBuildingAtLocation(IntPtr ptr, double latDegrees, double longDegrees, double altitude, NativeBuildingReceivedCallback nativeCallback, int buildingRequestId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeHighlightBuildingAtLocation(IntPtr ptr, double latDegrees, double longDegrees, double altitude, NativeHighlightReceivedCallback highlightCallback, int highlightRequestId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool GetBuildingAltitudeAtLatLong(IntPtr ptr, double latDegrees, double longDegrees, out double out_altitude);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeClearHighlight(IntPtr ptr, string highlightId);


        internal delegate void NativeHighlightMeshUploadCallback(IntPtr highlightId, ulong meshId, int highlightRequestId, IntPtr highlightPtr);

        internal delegate void NativeHighlightMeshClearCallback(IntPtr highlightId, ulong meshId);

        internal delegate void NativeBuildingReceivedCallback(bool buildingReceived, BuildingInterop building, int buildingRequestId);

        internal delegate void NativeHighlightReceivedCallback(bool highlightReceived, IntPtr highlightId, int highlightRequestId);


        [MonoPInvokeCallback(typeof(NativeBuildingReceivedCallback))]
        internal static void OnBuildingReceived(bool buildingReceived, BuildingInterop building, int buildingRequestId)
        {
            var request = BuildingRequests[buildingRequestId];

            Building result;
            result.BuildingId = Marshal.PtrToStringAnsi(building.StringIdPtr);
            result.BaseAltitude = building.BaseAltitude;
            result.TopAltitude = building.TopAltitude;
            result.Centroid = LatLong.FromDegrees(building.CentroidLatitude, building.CentroidLongitude);

            BuildingRequests.Remove(buildingRequestId);

            request.callback(buildingReceived, result);
        }

        [MonoPInvokeCallback(typeof(NativeHighlightReceivedCallback))]
        internal static void OnHighlightReceived(bool highlightReceived, IntPtr highlightId, int highlightRequestId)
        {
            var request = HighlightRequests[highlightRequestId];
            Highlight highlight;
            if (highlightReceived)
            {
                var highlightIdString = Marshal.PtrToStringAnsi(highlightId);
                highlight = new Highlight(highlightIdString, highlightRequestId);
            }
            else
            {
                HighlightRequests.Remove(highlightRequestId);
                highlight = null;
            }
            request.callback(highlightReceived, highlight);
        }

        [MonoPInvokeCallback(typeof(NativeHighlightMeshUploadCallback))]
        internal static void OnHighlightMeshUpload(IntPtr highlightIdPtr, ulong meshId, int highlightRequestId, IntPtr highlightPtr)
        {
            string highlightIdString = Marshal.PtrToStringAnsi(highlightIdPtr);
            
            HighlightInterop highlight = MarshalHighlightInterop(highlightPtr);
            string highlightMeshId = GetHighlightGameObjectId(highlightIdString, meshId);
            Mesh mesh = CreateHighlightMesh(highlightMeshId, highlight);

            var originEcef = new DoubleVector3(highlight.OriginEcefX, highlight.OriginEcefY, highlight.OriginEcefZ);
            HighlightStreamer.AddObjectsForMeshes(new Mesh[] { mesh }, originEcef, "highlight");

            var request = HighlightRequests[highlightRequestId];
            HighlightIdsToAdd.Add(highlightMeshId);
            var gameObject = HighlightStreamer.GetObjects(highlightMeshId)[0];
            gameObject.GetComponent<MeshRenderer>().sharedMaterial = request.material;
        }

        [MonoPInvokeCallback(typeof(NativeHighlightMeshClearCallback))]
        internal static void OnHighlightMeshClear(IntPtr highlightIdPtr, ulong meshId)
        {
            string gameObjectId = GetHighlightGameObjectId(Marshal.PtrToStringAnsi(highlightIdPtr), meshId);
            HighlightStreamer.RemoveObjects(gameObjectId);
        }

        internal void AddNewHighlights()
        {
            foreach (var highlightMeshId in HighlightIdsToAdd)
            {
                HighlightStreamer.SetVisible(highlightMeshId, true);
            }
            HighlightIdsToAdd.Clear();
        }

        private static HighlightInterop MarshalHighlightInterop(IntPtr ptr)
        {
            var result = (HighlightInterop) Marshal.PtrToStructure(ptr, typeof(HighlightInterop));
            return result;
        }

        private static Mesh CreateHighlightMesh(string meshName, HighlightInterop highlight)
        {
            var mesh = new Mesh();
            mesh.name = meshName;

            var positions = new float[highlight.VertexCount * 3];
            Marshal.Copy(highlight.VertexPositions, positions, 0, positions.Length);

            var vertices = new Vector3[highlight.VertexCount];
            for (int i=0; i<highlight.VertexCount; ++i)
            {
                vertices[i] = new Vector3(positions[i*3], positions[i*3 + 1], positions[i*3 + 2]);
            }
            var triangles = new int[highlight.IndexCount];
            for (int i=0; i<highlight.IndexCount; ++i)
            {
                short index = Marshal.ReadInt16(highlight.Indices, i * Marshal.SizeOf(typeof(short)));
                triangles[i] = (int) index;
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            return mesh;
        }

        private static string GetHighlightGameObjectId(string highlightId, ulong meshPartId)
        {
            var result = highlightId + meshPartId.ToString();
            return result;
        }
    }
}

