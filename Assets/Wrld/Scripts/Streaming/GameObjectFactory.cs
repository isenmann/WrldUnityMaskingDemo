using UnityEngine;

namespace Wrld.Streaming
{
    class GameObjectFactory
    {
        private Transform m_parentTransform;

        public GameObjectFactory(Transform parentTransform)
        {
            m_parentTransform = parentTransform;
        }

        private static string CreateGameObjectName(string baseName, int meshIndex)
        {
            return string.Format("{0}_INDEX{1}", baseName, meshIndex);
        }

        private GameObject CreateGameObject(Mesh mesh, Material material, string objectName, CollisionStreamingType collisionType)
        {
            var gameObject = new GameObject(objectName);
            gameObject.SetActive(false);

            gameObject.transform.parent = m_parentTransform;
            gameObject.transform.localScale = Vector3.one;
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;

            switch (collisionType)
            {
                case CollisionStreamingType.SingleSidedCollision:
                {
                    gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
                    break;
                }
                case CollisionStreamingType.DoubleSidedCollision:
                {
                    gameObject.AddComponent<MeshCollider>().sharedMesh = CreateDoubleSidedCollisionMesh(mesh);
                    break;
                }
            }
            return gameObject;
        }

        public GameObject[] CreateGameObjects(Mesh[] meshes, Material material, CollisionStreamingType collisionType)
        {
            var gameObjects = new GameObject[meshes.Length];

            for (int meshIndex = 0; meshIndex < meshes.Length; ++meshIndex)
            {
                var name = CreateGameObjectName(meshes[meshIndex].name, meshIndex);
                gameObjects[meshIndex] = CreateGameObject(meshes[meshIndex], material, name, collisionType);
            }

            return gameObjects;
        }


        private static Mesh CreateDoubleSidedCollisionMesh(Mesh sourceMesh)
        {
            Mesh mesh = new Mesh();
            mesh.name = sourceMesh.name;
            mesh.vertices = sourceMesh.vertices;

            int[] sourceTriangles = sourceMesh.triangles;
            int triangleCount = sourceTriangles.Length;
            int[] triangles = new int[triangleCount * 2];

            for (int index=0; index<triangleCount; index += 3)
            {
                // Copy all source triangles into first half of array
                triangles[index+0] = sourceTriangles[index+0];
                triangles[index+1] = sourceTriangles[index+1];
                triangles[index+2] = sourceTriangles[index+2];

                // Insert flipped triangles into second half of array
                triangles[triangleCount + index + 0] = sourceTriangles[index+0];
                triangles[triangleCount + index + 1] = sourceTriangles[index+2];
                triangles[triangleCount + index + 2] = sourceTriangles[index+1];
            }

            mesh.triangles = triangles;
            return mesh;
        }
    }
}
