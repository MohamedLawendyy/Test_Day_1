using UnityEngine;

public class PlayerPosition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject playerPrefab;
    public Transform spawnPoint;

    void Start()
    {
        Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
