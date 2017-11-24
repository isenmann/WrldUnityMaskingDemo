using AOT;
using Wrld.Common.Maths;
using Wrld.Materials;
using Wrld.Meshes;
using Wrld.Space;
using Wrld.Streaming;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Wrld
{ 
    public class MapGameObjectScene
    {
        public delegate void AddMeshCallback([MarshalAs(UnmanagedType.LPStr)] string id);
        public delegate void DeleteMeshCallback([MarshalAs(UnmanagedType.LPStr)] string id);
        public delegate void VisibilityCallback([MarshalAs(UnmanagedType.LPStr)] string id, [MarshalAs(UnmanagedType.I1)] bool visible);

        GameObjectStreamer m_terrainStreamer;
        GameObjectStreamer m_roadStreamer;
        GameObjectStreamer m_buildingStreamer;
        GameObjectStreamer m_highlightStreamer;
        MeshUploader m_meshUploader;
        bool m_enabled;

        private static MapGameObjectScene ms_instance;

        public MapGameObjectScene(GameObjectStreamer terrainStreamer, GameObjectStreamer roadStreamer, GameObjectStreamer buildingStreamer, GameObjectStreamer highlightStreamer)
        {
            m_terrainStreamer = terrainStreamer;
            m_roadStreamer = roadStreamer;
            m_buildingStreamer = buildingStreamer;
            m_highlightStreamer = highlightStreamer;
            m_meshUploader = new MeshUploader();
            m_enabled = true;
            ms_instance = this;
        }

        internal void SetEnabled(bool enabled)
        {
            m_enabled = enabled;
        }

        public void ChangeCollision(ConfigParams.CollisionConfig collisions)
        {
            var terrainCollision = (collisions.TerrainCollision) ? CollisionStreamingType.SingleSidedCollision : CollisionStreamingType.NoCollision;
            var roadCollision = (collisions.RoadCollision) ? CollisionStreamingType.DoubleSidedCollision : CollisionStreamingType.NoCollision;
            var buildingCollision = (collisions.BuildingCollision) ? CollisionStreamingType.SingleSidedCollision : CollisionStreamingType.NoCollision;
            m_terrainStreamer.ChangeCollision(terrainCollision);
            m_roadStreamer.ChangeCollision(roadCollision);
            m_buildingStreamer.ChangeCollision(buildingCollision);
        }
        
        public void UpdateTransforms(ITransformUpdateStrategy transformUpdateStrategy)
        {
            const float roadHeightOffset = 0.1f;
            m_terrainStreamer.UpdateTransforms(transformUpdateStrategy);
            m_roadStreamer.UpdateTransforms(transformUpdateStrategy, roadHeightOffset);
            m_buildingStreamer.UpdateTransforms(transformUpdateStrategy);
            m_highlightStreamer.UpdateTransforms(transformUpdateStrategy);
        }

        [MonoPInvokeCallback(typeof(DeleteMeshCallback))]
        public static void DeleteMesh([MarshalAs(UnmanagedType.LPStr)] string meshID)
        {
            if (ms_instance.m_enabled)
            {
                ms_instance.DeleteMeshInternal(meshID);
            }
        }

        private void DeleteMeshInternal(string id)
        {
            var streamer = GetStreamerFromName(id);
            streamer.RemoveObjects(id);
        }

        [MonoPInvokeCallback(typeof(AddMeshCallback))]
        public static void AddMesh([MarshalAs(UnmanagedType.LPStr)] string meshID)
        {
            if (ms_instance.m_enabled)
            {
                ms_instance.AddMeshInternal(meshID);
            }
        }

        private void AddMeshInternal(string id)
        {
            Mesh[] meshes;
            DoubleVector3 originECEF;
            string materialName;

            if (m_meshUploader.TryGetUnityMeshesForID(id, out meshes, out originECEF, out materialName))
            {
                var streamer = GetStreamerFromName(id);
                streamer.AddObjectsForMeshes(meshes, originECEF, materialName);
            }
            else
            {
                Debug.LogErrorFormat("ERROR: Could not get mesh for ID {0}.", id);
            }
        }

        [MonoPInvokeCallback(typeof(VisibilityCallback))]
        public static void SetVisible([MarshalAs(UnmanagedType.LPStr)] string meshID, [MarshalAs(UnmanagedType.I1)] bool visible)
        {
            if (ms_instance.m_enabled)
            {
                ms_instance.SetVisibleInternal(meshID, visible);
            }
        }

        private void SetVisibleInternal(string id, bool visible)
        {
            var streamer = GetStreamerFromName(id);
            streamer.SetVisible(id, visible);
        }

        private GameObjectStreamer GetStreamerFromName(string name)
        {
            // :TODO: replace this string lookup with a shared type enum or similar
            if (name.StartsWith("Raster") || name.StartsWith("Terrain"))
            {
                return m_terrainStreamer;
            }

            switch (name[0])
            {
                case 'M':
                case 'L':
                    return m_terrainStreamer;
                case 'R':
                    return m_roadStreamer;
                case 'B':
                    return m_buildingStreamer;
                case 'H':
                    return m_highlightStreamer;
                default:
                    throw new ArgumentException(string.Format("Unknown streamer for name: {0}", name), "name");
            }
        }
    }
}

