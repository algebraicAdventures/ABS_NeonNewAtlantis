using UnityEngine;
using System.Collections;

public class BuildingController : MonoBehaviour {

    public Color colorStartMain = Color.red;
    public Color colorEndMain = Color.green;
    public Color colorStartBevel = Color.blue;
    public Color colorEndBevel = Color.yellow;
    public float durationMain = 1.0F;
    public float durationBevel = 1.0F;
    public Material[] materials;
    public Renderer rend;
    SynthController synth;

    // Use this for initialization
    void Start () {
        rend = GetComponent<Renderer>();
        synth = FindObjectOfType<SynthController>();
        materials = rend.materials;
    }

    float Limit(float num, float limit)
    {
        if(num > limit)
        {
            return limit;
        } else
        {
            return num;
        }
    }

    // Update is called once per frame
    void Update () {
        float timeMult = -Limit(synth.intensity, 100) + 100;
        float lerpMain = Mathf.PingPong(Time.time, durationMain * (timeMult + 1)/10) / (durationMain * (timeMult + 1) / 10);
        float lerpBevel = Mathf.PingPong(Time.time, durationBevel * (timeMult + 1) / 10) / (durationBevel * (timeMult + 1) / 10);
        materials[0].color = Color.Lerp(colorStartMain, colorEndMain, lerpMain);
        if(materials.Length > 1)
        {
            materials[1].color = Color.Lerp(colorStartBevel, colorEndBevel, lerpBevel);
        }

    }
}
