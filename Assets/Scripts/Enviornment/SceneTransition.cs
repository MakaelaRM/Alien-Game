using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Mine";

    [Header("Spawn point (off = load the scene as it is)")]
    [Tooltip("On: after the scene loads, the player is moved to the object named below. Off: the player starts wherever they are placed in that scene.")]
    [SerializeField] private bool pickSpawnPoint = false;
    [Tooltip("Name of an empty GameObject in the scene you are loading, e.g. SpawnPoint_FromTown.")]
    [SerializeField] private string spawnPointName = "";
    [Tooltip("Also turn the player to match the spawn point's rotation.")]
    [SerializeField] private bool useSpawnRotation = true;

    // Remembered across the scene load.
    static string pendingSpawnName;
    static bool pendingUseRotation;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        pendingSpawnName = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pendingSpawnName = (pickSpawnPoint && !string.IsNullOrEmpty(spawnPointName)) ? spawnPointName : null;
            pendingUseRotation = useSpawnRotation;
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(pendingSpawnName)) return;

        string wanted = pendingSpawnName;
        pendingSpawnName = null; // only use it once

        var spawn = GameObject.Find(wanted);
        var player = GameObject.FindGameObjectWithTag("Player");

        if (spawn == null)
        {
            Debug.LogWarning("[SceneTransition] No object named '" + wanted + "' in scene '" + scene.name + "'. Player left where the scene put them.");
            return;
        }
        if (player == null)
        {
            Debug.LogWarning("[SceneTransition] No object tagged Player in scene '" + scene.name + "'.");
            return;
        }

        // A CharacterController would push the player back, so turn it off while moving.
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = spawn.transform.position;
        if (pendingUseRotation) player.transform.rotation = spawn.transform.rotation;

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = spawn.transform.position;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (cc != null) cc.enabled = true;
    }
}