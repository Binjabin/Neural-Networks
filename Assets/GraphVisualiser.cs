using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphVisualiser : MonoBehaviour
{
    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;
    public int INPUT_COUNT = 3;
    public int OUTPUT_COUNT = 2;

    List<float> inputValueList = new List<float>();
    List<float> outputValueList = new List<float>();

    NeuralNetwork network;

    [SerializeField, Range(-1f, 1f)] float x, y;

    public ComputeShader computeShader;
    public RenderTexture renderTexture;

    // Start is called before the first frame update
    void Start()
    {
        //network = new NeuralNetwork();
        //network.Initialise(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);
       
       

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if(renderTexture == null)
        {
            renderTexture = new RenderTexture(1024, 1024, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetFloat("Resolution", renderTexture.width);
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);

        Graphics.Blit(renderTexture, dest);

    }
}
