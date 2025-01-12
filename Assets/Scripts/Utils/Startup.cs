using UnityEngine;

namespace FusionTutorial
{
    public class Startup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InstantiatePrefab()
        {
            Debug.Log("-- Instantiating objects --");

            GameObject[] prefabsToInstantiate = Resources.LoadAll<GameObject>("InstantiateOnLoad/");

            foreach (GameObject prefab in prefabsToInstantiate)
            {
                Debug.Log($"Creating {prefab.name}");

                GameObject.Instantiate(prefab);
            }

            Debug.Log("-- Instantiating objects done --");
        }
    }
}
