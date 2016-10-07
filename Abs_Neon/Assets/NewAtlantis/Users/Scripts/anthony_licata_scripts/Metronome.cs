using UnityEngine;
using System.Collections;

// Metronome.cs by Julien Noble, 2013
// http://cubeslam.net/2013/12/19/unity-metronome-like-a-pro/

public delegate void MetronomeEvent(Metronome metronome);

public class Metronome : MonoBehaviour
{
    public int Base;
    public int Step;
    public float BPM;
    public int CurrentStep = 1;
    public int CurrentMeasure;

    private float interval;
    private float nextTime;

    public event MetronomeEvent OnTick;
    public event MetronomeEvent OnNewMeasure;

    // Use this for initialization
    void Start()
    {
        StartMetronome();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartMetronome()
    {
        StopCoroutine("DoTick");
        CurrentStep = 1;
        var multiplier = Base / 4f;
        var tmpInterval = 60f / BPM;
        interval = tmpInterval / multiplier;
        nextTime = Time.time;
        StartCoroutine("DoTick");
    }

    IEnumerator DoTick()
    {
        for (;;)
        {
            if (CurrentStep == 1 && OnNewMeasure != null)
                OnNewMeasure(this);
            if (OnTick != null)
                OnTick(this);
            nextTime += interval;
            yield return new WaitForSeconds(nextTime - Time.time);
            CurrentStep++;
            if (CurrentStep > Step)
            {
                CurrentStep = 1;
                CurrentMeasure++;
            }
        }
    }
}