using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageRenderer : MonoBehaviour
{
    public AudioSource audioSource;
    private float amplitude;
    public Texture2D normal;
    public Texture2D hover;
    public Texture2D active;
    public GUIStyle buttonStyle;
    public Shader imageShader;
    private Texture2D generatedTexture; // Dynamically generated texture

    private Material material;
    private Vector4[] metaballs = new Vector4[1];
    private ComputeBuffer arrayBuffer;

    public int arraySize = 10;
    private float[] dataArray;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.Play();
        // Create material with the shader
        material = new Material(imageShader);
        material.SetColor("_Color", Color.red);
    	buttonStyle.alignment = TextAnchor.MiddleCenter;
        // Create a texture with the dimensions of the game view
        generatedTexture = new Texture2D(Screen.width, Screen.height);

        // Apply the generated texture to the material
        material.SetTexture("_MainTex", generatedTexture);
        metaballs[0] = new Vector4(1, 1, 1, 1);
        arrayBuffer = new ComputeBuffer(arraySize, sizeof(float));
        GenerateArrayData();
        arrayBuffer.SetData(dataArray);
        material.SetBuffer("_Array", arrayBuffer);
        material.SetInt("_ArraySize", arraySize);
    }

    private void Update(){
        float timeSinceSceneLoad = Time.timeSinceLevelLoad;
        this.material.SetFloat("_TimeSince", timeSinceSceneLoad);
        this.material.SetFloat("_Amplitude", amplitude);
        material.SetVectorArray("_Metaballs", metaballs);
        arrayBuffer.SetData(dataArray);
        //Audio
    }

    private void OnGUI()
    {
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.background = normal;
        buttonStyle.hover.background = hover;
        buttonStyle.active.background = active;
        amplitude = GetCurrentAmplitude(audioSource);
        // Draw the image using the shader and material directly to the screen
        Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), generatedTexture, material);
        Button(Screen.width/2,Screen.height/3,200,100,"New Game");
        Button(Screen.width/2,Screen.height/2,200,100, "Load Game");
        Button(Screen.width/2,2*Screen.height/3,200,100, "Quit");
        //GUI.Label(new Rect(10, 10, 100, 20), amplitude+"");
    }
    private void GenerateArrayData()
    {
        // Generate example array data (you can replace this with your own logic)
        dataArray = new float[arraySize];
        for (int i = 0; i < arraySize; i++)
        {
            dataArray[i] = Random.Range(0f, 1f);
        }
    }
    private void OnDestroy() {
        arrayBuffer.Release();
    }
    private void Button(int x, int y, int width, int height, string text){
        GUI.Button (new Rect (x-width/2,y-height/2,width,height), text, buttonStyle);
    }
    private float GetCurrentAmplitude(AudioSource audioSource){
        float[] samples = new float[1024];
        audioSource.GetOutputData(samples, 0); // fill array with samples
        return samples[0]; // return rms value
    }
}