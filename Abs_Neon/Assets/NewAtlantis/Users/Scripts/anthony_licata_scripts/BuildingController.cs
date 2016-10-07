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

    // Use this for initialization
    void Start () {
        rend = GetComponent<Renderer>();
        materials = rend.materials;
    }

    // Update is called once per frame
    void Update () {
        float lerpMain = Mathf.PingPong(Time.time, durationMain) / durationMain;
        float lerpBevel = Mathf.PingPong(Time.time, durationBevel) / durationBevel;
        materials[0].color = Color.Lerp(colorStartMain, colorEndMain, lerpMain);
        if(materials.Length > 1)
        {
            materials[1].color = Color.Lerp(colorStartBevel, colorEndBevel, lerpBevel);
        }

    }
}
