using Wrld.Common.Maths;
using Wrld.Materials;
using Wrld.Space;
using UnityEngine;

namespace Wrld.Streaming
{
    public class GameObjectStreamer
    {
        GameObjectRepository m_gameObjectRepository;
        MaterialRepository m_materialRepository;
        GameObjectFactory m_gameObjectCreator;

        private CollisionStreamingType m_collisions;

        public GameObjectStreamer(string rootObjectName, MaterialRepository materialRepository, Transform parentForStreamedObjects, CollisionStreamingType collisions)
        {
            m_materialRepository = materialRepository;
            m_gameObjectRepository = new GameObjectRepository(rootObjectName, parentForStreamedObjects, materialRepository);
            m_gameObjectCreator = new GameObjectFactory(m_gameObjectRepository.Root.transform);
            m_collisions = collisions;
        }

        public void Destroy()
        {
            Object.Destroy(m_gameObjectRepository.Root);
        }

        public void AddObjectsForMeshes(Mesh[] meshes, DoubleVector3 originECEF, string materialName)
        {
            var id = meshes[0].name;
            if (m_gameObjectRepository.Contains(id))
            {
                return;
            }

            var material = m_materialRepository.LoadOrCreateMaterial(id, materialName);
            var gameObjects = m_gameObjectCreator.CreateGameObjects(meshes, material, m_collisions); 
            m_gameObjectRepository.Add(id, originECEF, gameObjects);
        }

        public bool RemoveObjects(string id)
        {
            return m_gameObjectRepository.Remove(id);
        }

        public GameObject[] GetObjects(string id)
        {
            GameObject[] gameObjects;
            m_gameObjectRepository.TryGetGameObjects(id, out gameObjects);
            return gameObjects;
        }

        public void UpdateTransforms(ITransformUpdateStrategy transformUpdateStrategy, float heightOffset = 0.0f)
        {
            m_gameObjectRepository.UpdateTransforms(transformUpdateStrategy, heightOffset);
        }

        public void SetVisible(string id, bool visible)
        {
            GameObject[] gameObjects;

            if (m_gameObjectRepository.TryGetGameObjects(id, out gameObjects))
            {
                foreach (var gameObject in gameObjects)
                {
                    gameObject.SetActive(visible);
                }
            }
        }

        public void ChangeCollision(CollisionStreamingType collision)
        {
            m_collisions = collision;
        }
    }
}
