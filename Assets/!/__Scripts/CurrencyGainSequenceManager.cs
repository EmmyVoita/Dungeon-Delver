using System.Collections.Generic;
using UnityEngine;

public class CurrencyGainSequenceManager : RuntimeModifierManager<ICurrencyGainSequenceEffect>
{
    public static CurrencyGainSequenceManager Instance { get; private set; }

    private readonly Dictionary<ICurrencyGainSequenceEffect, GainTracker> _trackers = new();

    private class GainTracker
    {
        public readonly Queue<float> gainTimes = new();
    }

    protected void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void Subscribe()
    {
        CurrencyManager.OnCurrencyAdded += HandleCurrencyAdded;
    }

    protected override void Unsubscribe()
    {
        CurrencyManager.OnCurrencyAdded -= HandleCurrencyAdded;
        base.Unsubscribe();
    }

    private void HandleCurrencyAdded(int amount)
    {
        float currentTime = Time.time;

        foreach (ICurrencyGainSequenceEffect effect in activeModifiers)
        {
            if (effect == null)
                continue;

            if (!_trackers.TryGetValue(effect, out GainTracker tracker))
            {
                tracker = new GainTracker();
                _trackers.Add(effect, tracker);
            }

            RemoveExpiredGains(effect, tracker, currentTime);

            tracker.gainTimes.Enqueue(currentTime);

            if (tracker.gainTimes.Count < effect.RequiredInstances)
                continue;

            ConsumeRequiredGains(effect, tracker);
            effect.OnSequenceCompleted();
        }
    }

    private static void RemoveExpiredGains(
        ICurrencyGainSequenceEffect effect,
        GainTracker tracker,
        float currentTime)
    {
        while (tracker.gainTimes.Count > 0 &&
               currentTime - tracker.gainTimes.Peek() > effect.TimeWindow)
        {
            tracker.gainTimes.Dequeue();
        }
    }

    private static void ConsumeRequiredGains(ICurrencyGainSequenceEffect effect, GainTracker tracker)
    {
        for (int i = 0; i < effect.RequiredInstances; i++)
        {
            if (tracker.gainTimes.Count == 0)
                break;

            tracker.gainTimes.Dequeue();
        }
    }

    protected override void OnModifierActivated(ICurrencyGainSequenceEffect modifier)
    {
        if (!_trackers.ContainsKey(modifier))
            _trackers.Add(modifier, new GainTracker());
    }

    public void ResetTrackers()
    {
        foreach (GainTracker tracker in _trackers.Values)
            tracker.gainTimes.Clear();
    }
}