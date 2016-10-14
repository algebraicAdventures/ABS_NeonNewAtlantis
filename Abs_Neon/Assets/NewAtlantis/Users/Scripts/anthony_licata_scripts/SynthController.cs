﻿using UnityEngine;
using System.Collections;

public class SynthController : MonoBehaviour {

    public int noteLength = 1;
    // 1 = short, 2 = med, 3 = long
    public int noteMaxTriggerSpace;
    public int noteMinTriggerSpace;
    // likelihood that a note will be played on the next tick (according to noteTriggerResolution)
    public int octaveRange;
    // 1 = 1 octave, 2 = 2 mid/high, 3 = 3 low/mid/high
    public float chordFrequency;
    // likelihood that tne next note played will be a chord
    // range 0.0F-1.0F
    public int masterKey;
    // 1 - major, 2 - minor
    // implement more later
    public int keyRoot;
    // 1 - C, 2 - C#, 3 - D...
    int ticksUntilPlay;
    Chord currChord;
    public float intensity = 0.0F;

    // 4-voice polyphony possible
    public AudioSource[] voices = new AudioSource[4];

    // AudioClips
    public AudioClip[] clipsShort = new AudioClip[36];
    public AudioClip[] clipsMed = new AudioClip[36];
    public AudioClip[] clipsLong = new AudioClip[36];
    public AudioClip testclip;

    // metronome
    Metronome metro;

    // control points
    public GameObject[] controls = new GameObject[8];
    public Vector3[] initialPosns = new Vector3[8];
    private float[] distances = new float[8];

    public float distClamp = 3f;

    // Use this for initialization
    void Start () {
        metro = FindObjectOfType<Metronome>();
        metro.OnTick += Tick;

        for(int i = 0;  i < controls.Length; i++)
        {
            initialPosns[i] = controls[i].transform.localPosition;
        }

        for(int i = 0; i < 4; i++)
        {
            voices[i] = gameObject.AddComponent<AudioSource>();
        }
        generateNewChord();
    }
	
	// Update is called once per frame
	void Update () {
        intensity = metro.BPM / 8 + chordFrequency * 50 + (5 - noteMaxTriggerSpace - noteMinTriggerSpace) * 5 ;
        CalculateClampedDistances();
        noteLength = (int) map(distances[0], 0f, distClamp, 0f, 3f);
        noteMaxTriggerSpace = (int)map(distances[1], 0f, distClamp, 0f, 6f);
        noteMinTriggerSpace = (int)map(distances[2], 0f, distClamp, 0f, 6f);
        octaveRange = (int)map(distances[3], 0f, distClamp, 1f, 2f);
        chordFrequency = (int)map(distances[4], 0f, distClamp, 0f, 1f);
        masterKey = (int)map(distances[5], 0f, distClamp, 1f, 2f);
        keyRoot = (int)map(distances[6], 0f, distClamp, 1f, 12f);
        int tempBPM = (int)map(distances[7], 0f, distClamp, 50, 400);
        if(tempBPM != metro.BPM)
        {
            metro.BPM = tempBPM;
            metro.StopAllCoroutines();
            metro.StartMetronome();
        }
        
    }

    void CalculateClampedDistances()
    {
        for(int i = 0; i < controls.Length; i++)
        {
            distances[i] = Mathf.Clamp(Vector3.Distance(controls[i].transform.localPosition, initialPosns[i]), 0, distClamp);
        }
    }

    public static float map( float value, float leftMin, float leftMax, float rightMin, float rightMax )  
    {
        return rightMin + ( value - leftMin ) * ( rightMax - rightMin ) / ( leftMax - leftMin );
    }

    int[] KeyToInts() {
        int length = 12;
        if (masterKey > 0)
        {
            length = 7;
        }
        int[] ints = new int[length];
        switch (masterKey)
        {
            case 0:
                for(int i = 0; i < length; i++)
                {
                    ints[i] = i + 1;
                }
                break;
            case 1:
                int[] major = new int[7] { keyRoot, keyRoot + 2, keyRoot + 4, keyRoot + 5, keyRoot + 7, keyRoot + 9, keyRoot + 11};
                major.CopyTo(ints, 0);
                break;
            case 2:
                int[] minor = new int[7] { keyRoot, keyRoot + 2, keyRoot + 3, keyRoot + 5, keyRoot + 7, keyRoot + 8, keyRoot + 10};
                minor.CopyTo(ints, 0);
                break;
        }
        return ints;
    }

    int RestrictedRandom(int[] allowed)
    {
        return allowed[Random.Range(0, allowed.Length)];
    }

    public AudioSource AddAudio(AudioClip clip, bool loop, bool playAwake, float vol)
    {
        AudioSource audio = gameObject.AddComponent<AudioSource>();
        audio.clip = clip;
        audio.loop = loop;
        audio.playOnAwake = playAwake;
        audio.volume = vol;
        return audio;
    }

    public void generateNewChord()
    {
        int prevLength = noteLength;
        noteLength = Random.Range(1, 4);
        ticksUntilPlay = prevLength + Random.Range(noteMinTriggerSpace, noteMaxTriggerSpace);
        Note rootNote = new Note(RestrictedRandom(KeyToInts()), Random.Range(1, octaveRange + 1), Random.Range(1, 4));
        float isChord = Random.Range(0F, 1F);
        int form = 1;
        if(isChord <= chordFrequency)
        {
            form = Random.Range(1, 5);
        }
        currChord = new Chord(rootNote, form, 1);
    }

    public void Tick(Metronome metro)
    {
        Debug.Log("tick");
        if(ticksUntilPlay <= 0)
        {
            for (int i = 0; i < currChord.chordForm; i++)
            {
                voices[i].timeSamples = 0;
                switch (currChord.notes[0].length)
                {
                    case 1:
                        voices[i].PlayOneShot(clipsShort[(currChord.notes[i].octave - 1) * 12 + currChord.notes[i].degree - 1], 1.0F);
                        break;
                    case 2:
                        voices[i].PlayOneShot(clipsMed[(currChord.notes[i].octave - 1) * 12 + currChord.notes[i].degree - 1], 1.0F);
                        break;
                    case 3:
                        voices[i].PlayOneShot(clipsLong[(currChord.notes[i].octave - 1) * 12 + currChord.notes[i].degree - 1], 1.0F);
                        break;
                }
                //voice1.PlayOneShot(testclip, 1.0F);
                Debug.Log("Playing " + currChord.notes[i].octave + "-" + currChord.notes[i].degree + "----" + currChord.chordForm + "-" + currChord.chordHarmonicContent);
            }
            generateNewChord();
        }
        else
        {
            ticksUntilPlay--;

        }

    }
}

class Chord {

    public Note[] notes;
    public int chordForm;
    // 1 = monad, 2 = dyad, 3 = triad, 4 = tetrad
    public int chordHarmonicContent;
    // 1 = Major, 2 = minor, 3 = augmented, 4 = diminished
    // (in dyads these only reflect the major & minor third rn to match up with the triads)
    // (sevenths) 5 = Minor major, 6 = dominant, 7 = augmented major, 8 = half-diminished

    public Chord(Note rootNote, int chordForm, int chordHarmonicContent)
    {
        this.chordForm = chordForm;
        notes = new Note[chordForm];
        this.chordHarmonicContent = chordHarmonicContent;
        notes[0] = rootNote;
        if(chordForm > 1)
        {
            int third = rootNote.degree, octave = rootNote.octave;
            switch (chordHarmonicContent)
            {
                case 1:
                    third += 4;
                    break;
                case 2:
                    third += 3;
                    break;
                case 3:
                    third += 4;
                    break;
                case 4:
                    third += 3;
                    break;
                case 5:
                    third += 3;
                    break;
                case 6:
                    third += 4;
                    break;
                case 7:
                    third += 4;
                    break;
                case 8:
                    third += 3;
                    break;
            }
            if (third > 12)
            {
                octave += 1;
                third %= 12;
            }
                notes[1] = new Note(third, octave, rootNote.length);
            if (chordForm > 2)
            {
                int fifth = rootNote.degree;
                octave = rootNote.octave;
                switch (chordHarmonicContent)
                {
                    case 1:
                        fifth += 7;
                        break;
                    case 2:
                        fifth += 7;
                        break;
                    case 3:
                        fifth += 8;
                        break;
                    case 4:
                        fifth += 6;
                        break;
                    case 5:
                        fifth += 7;
                        break;
                    case 6:
                        fifth += 7;
                        break;
                    case 7:
                        fifth += 7;
                        break;
                    case 8:
                        fifth += 7;
                        break;
                }
                if (fifth > 12)
                {
                    octave += 1;
                    fifth %= 12;
                }
                notes[2] = new Note(fifth, octave, rootNote.length);
                if (chordForm > 3)
                {
                    int seventh = rootNote.degree;
                    octave = rootNote.octave;
                    switch (chordHarmonicContent)
                    {
                        case 1:
                            seventh += 11;
                            break;
                        case 2:
                            seventh += 10;
                            break;
                        case 3:
                            seventh += 11;
                            break;
                        case 4:
                            seventh += 9;
                            break;
                        case 5:
                            seventh += 11;
                            break;
                        case 6:
                            seventh += 10;
                            break;
                        case 7:
                            seventh += 11;
                            break;
                        case 8:
                            seventh += 11;
                            break;
                    }
                    if (seventh > 12)
                    {
                        octave += 1;
                        seventh %= 12;
                    }
                    notes[3] = new Note(seventh, octave, rootNote.length);
                }
            }
        }
    }
}

class Note
{
    public int degree;
    public int octave;
    public int length;
    public Note(int deg, int oct, int len)
    {
        this.degree = deg;
        this.octave = oct;
        this.length = len;
    }
    public Note() { }
}