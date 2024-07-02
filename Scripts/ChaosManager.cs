using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;

public class ChaosManager : MonoBehaviour
{
    public static ChaosManager Instance;

    [HideInInspector] public UnityEvent randomizeClocks = new UnityEvent();

    [SerializeField] private float syncCheckRate;

    private float timer;
    private Dictionary<Firefly, float> blinkTimes = new Dictionary<Firefly, float>();
    
    private void Awake()
    {
        Instance = this;
        timer = syncCheckRate;
    }

    public void ReportBlink(Firefly firefly)
    {
        if (blinkTimes.ContainsKey(firefly))
        {
            blinkTimes[firefly] = Time.time;
        }
        else
        {
            blinkTimes.Add(firefly, Time.time);
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            CheckSync();
            timer = syncCheckRate;
        }
    }

    private void CheckSync()
    {
        if (blinkTimes.Count == 0) { return; }

        float tolerance = 0.2f;

        // Calculate the average blink time.
        float averageBlinkTime = 0;
        foreach (var blinkTime in blinkTimes.Values)
        {
            averageBlinkTime += blinkTime;
        }
        averageBlinkTime /= blinkTimes.Count;

        // Check if all blink times are within the tolerance range of the average blink time.
        foreach (var blinkTime in blinkTimes.Values)
        {
            if (Mathf.Abs(blinkTime - averageBlinkTime) > tolerance)
            {
                // If any blink time is outside the tolerance range, they are not synchronized.
                return;
            }
        }

        // Othervise we reintroduce some chaos after a small delay
        StartCoroutine(ReIntroduceChaos());
    }

    private IEnumerator ReIntroduceChaos()
    {
        yield return new WaitForSeconds(5);

        randomizeClocks.Invoke();
    }
}
