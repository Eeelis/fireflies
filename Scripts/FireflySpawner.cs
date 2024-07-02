using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflySpawner : MonoBehaviour
{
    [SerializeField] private AudioClip[] fireflySounds;
    [SerializeField] private Firefly fireFlyPrefab;
    [SerializeField] private int numberOfFireflies;

    private Camera cam;

    private void Start()
    {
        cam = FindObjectOfType<Camera>();

        float screenLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.transform.position.z)).x;
        float screenRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, cam.transform.position.z)).x;
        float screenBottom = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.transform.position.z)).y;
        float screenTop = cam.ScreenToWorldPoint(new Vector3(0, Screen.height, cam.transform.position.z)).y;

        // Assign indices equally so each sound can be given to equally many fireflies
        int[] soundIndices = new int[numberOfFireflies];

        for (int i = 0; i < numberOfFireflies; i++)
        {
            soundIndices[i] = i % fireflySounds.Length;
        }

        for (int i = 0; i < numberOfFireflies; i++)
        {
            // Generate random position within screen boundaries
            float randomX = Random.Range(screenLeft, screenRight);
            float randomY = Random.Range(screenBottom, screenTop);

            Vector3 randomPosition = new Vector3(randomX, randomY, 0);

            // Instantiate and initialize firefly
            Firefly newFirefly = Instantiate(fireFlyPrefab, randomPosition, Quaternion.identity);

            newFirefly.name = newFirefly.name + "-" + i.ToString();
            newFirefly.SetAudioClip(fireflySounds[soundIndices[i]]);
            newFirefly.transform.SetParent(transform);
        }
    }
}
