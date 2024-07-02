using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Firefly : MonoBehaviour
{
    [SerializeField] private Light2D pointLight;
    [SerializeField] private float blinkInterval;
    [SerializeField] private float speed;
    [SerializeField] private float noiseScale;
    [SerializeField] private float directionChangeRate;
    [SerializeField] private float nudgeAmount;

    private Camera cam;
    private AudioSource audioSource;
    private Vector2 screenBounds;
    private float noiseTimeX;
    private float noiseTimeY;
    public float clock;
    private bool allowNudging;

   
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        cam = FindObjectOfType<Camera>();

        screenBounds = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 15));

        RandomizeStartingParameters();
        RandomizeClock();
    }

    private void Start()
    {
        ChaosManager.Instance.randomizeClocks.AddListener(RandomizeClock);
    }

    void Update()
    {
        Move();

        clock += Time.deltaTime / blinkInterval;

        clock = Mathf.Clamp01(clock);

        if (clock >= 1)
        {
            Blink();
            clock = 0;
        }
    }

    private void Move()
    {
        // Generate smooth directions using Perlin noise
        float directionX = Mathf.PerlinNoise(noiseTimeX, 0) * 2 - 1;
        float directionY = Mathf.PerlinNoise(noiseTimeY, 0) * 2 - 1;

        Vector2 direction = new Vector2(directionX, directionY).normalized;
        
        Vector2 position = transform.position;
        position += direction * speed * Time.deltaTime;

        // Check if the fly is outside the screen bounds and wrap it around if necessary
        if (position.x > screenBounds.x) 
        {
            position.x = -screenBounds.x;
        }
        if (position.x < -screenBounds.x)
        {
            position.x = screenBounds.x;
        }
        if (position.y > screenBounds.y)
        {
            position.y = -screenBounds.y;
        }
        if (position.y < -screenBounds.y)
        {
            position.y = screenBounds.y;
        }

        transform.position = position;

        // Increment noise time
        noiseTimeX += noiseScale * Time.deltaTime * directionChangeRate;
        noiseTimeY += noiseScale * Time.deltaTime * directionChangeRate;
    }

    private void Blink()
    {
        LeanTween.cancel(pointLight.gameObject);
        pointLight.intensity = 0;

        LeanTween.value(pointLight.gameObject, 0f, 0.5f, 0.3f).setOnUpdate
        (
            (float val) =>
            {
                if (val < 0) { val = 0; }
                pointLight.intensity = val;
            }
        ).setEaseOutBack().setOnComplete(() =>
            {
                LeanTween.value(pointLight.gameObject, 0.5f, 0f, 0.5f).setOnUpdate(
                (float val) =>
                {
                    if (val < 0) { val = 0; }
                    pointLight.intensity = val;
                });
            }
        );

        NudgeNearbyFireflies();

        if (audioSource && audioSource.clip)
        {
            float halfScreenWidth = Screen.width / 2f;
            float distanceFromCenter = cam.WorldToScreenPoint(transform.position).x - halfScreenWidth;
            float panning = distanceFromCenter / halfScreenWidth;

            audioSource.panStereo = panning;
            audioSource.Play();
        }

        ChaosManager.Instance.ReportBlink(this);
    }

    private void NudgeNearbyFireflies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2.25f);

        foreach (Collider2D collider in colliders)
        {
            Firefly firefly = collider.GetComponent<Firefly>();

            if (firefly != null && firefly != this && firefly.allowNudging)
            {
                float pull = firefly.clock/1;
                firefly.clock += pull * nudgeAmount;

                if (firefly.clock > 1)
                {
                    firefly.clock = 1;
                }
            }
        }
    }

    private void RandomizeStartingParameters()
    {
        noiseTimeX = Random.Range(0f, 100f);
        noiseTimeY = Random.Range(0f, 100f);

        speed = speed + (Random.Range(-0.2f, 0.2f));
        noiseScale = noiseScale + (Random.Range(-0.02f, 0.02f));
        directionChangeRate = directionChangeRate + (Random.Range(-0.4f, 0.4f));
    }

    public void RandomizeClock()
    {
        clock = Random.Range(0f, 1f);
        allowNudging = false;
        StartCoroutine(AllowNudging());
    }

    // This ensures that the fireflies dont sync up too quickly after having their clocks randomized
    private IEnumerator AllowNudging()
    {
        yield return new WaitForSeconds(Random.Range(4, 10));

        allowNudging = true;
    }

    public void SetAudioClip(AudioClip clip)
    {
        audioSource.clip = clip;
    }

}
