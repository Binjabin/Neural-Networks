using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class NeuralNetworkGraph : MonoBehaviour
{
    public bool randomiseNetwork;
    NeuralNetwork network;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 3;
    public int INPUT_COUNT = 2;
    public int OUTPUT_COUNT = 2;

    [Header("Visuals Options")]
    public float distanceBetweenNodes;
    public float distanceBetweenLayers;
    public Gradient nodeGradient;
    public Gradient weightGradient;
    public GameObject nodePrefab;
    public GameObject linePrefab;


    List<GameObject> graph = new List<GameObject>();

    public List<float> inputValues = new List<float>();
    List<float> currentInputValues = new List<float>();

    
    private void Update()
    {

        if(randomiseNetwork)
        {
            randomiseNetwork = false;
            NewGraph();
            
        }
        UpdateGraph();
        
    }

    private void Awake()
    {
        NewGraph();
    }

    void NewGraph()
    {
        network = new NeuralNetwork();
        network.Initialise(LAYERS, NEURONS, INPUT_COUNT, OUTPUT_COUNT);
        network.RandomiseNetwork();
        UpdateGraph();
    }

    void UpdateGraph()
    {
        network.RunNetwork(inputValues);
        ClearGraph();
        RenderGraph();
    }

    void RenderGraph()
    {
        //nodes for input layer
        for (int i = 0; i < INPUT_COUNT; i++)
        {
            float vert = (i * distanceBetweenNodes) - (0.5f * INPUT_COUNT * distanceBetweenNodes);
            Vector3 pos = new Vector3(0f, vert, 0f);
            GameObject newNode = Instantiate(nodePrefab, pos, Quaternion.identity);
            float value = network.inputLayer[0, i];
            newNode.GetComponentInChildren<TextMeshProUGUI>().text = "" + value;
            newNode.GetComponent<SpriteRenderer>().color = GetNodeColor(value);
            newNode.transform.parent = transform;
            newNode.transform.localPosition = pos;
            graph.Add(newNode);
        }

        //nodes for hidden layers
        for (int i = 0; i < LAYERS; i++)
        {
            //for each hidden layer
            for (int j = 0; j < NEURONS; j++)
            {
                float vert = (j * distanceBetweenNodes) - (0.5f * NEURONS * distanceBetweenNodes);
                float hori = ((i + 1) * distanceBetweenLayers);
                Vector3 pos = new Vector3(hori, vert, 0f);
                GameObject newNode = Instantiate(nodePrefab, pos, Quaternion.identity);
                float value = network.hiddenLayers[i][0, j];
                newNode.GetComponentInChildren<TextMeshProUGUI>().text = "" + value;
                newNode.GetComponent<SpriteRenderer>().color = GetNodeColor(value);
                newNode.transform.parent = transform;
                newNode.transform.localPosition = pos;
                graph.Add(newNode);
            }
        }

        //nodes for output layer

        for (int i = 0; i < OUTPUT_COUNT; i++)
        {
            float vert = (i * distanceBetweenNodes) - (0.5f * OUTPUT_COUNT * distanceBetweenNodes);
            Vector3 pos = new Vector3((LAYERS+1) * distanceBetweenLayers, vert, 0f);
            GameObject newNode = Instantiate(nodePrefab, pos, Quaternion.identity);
            float value = network.outputLayer[0, i];
            newNode.GetComponentInChildren<TextMeshProUGUI>().text = "" + value;
            newNode.GetComponent<SpriteRenderer>().color = GetNodeColor(value);
            newNode.transform.parent = transform;
            newNode.transform.localPosition = pos;
            graph.Add(newNode);
        }

        //connections 
        for (int i = 0; i < network.weights.Count; i++)
        {
            Debug.Log(network.weights.Count);
            //each layer
            for (int x = 0; x < network.weights[i].RowCount; x++)
            {
                //each column
                for (int y = 0; y < network.weights[i].ColumnCount; y++)
                {
                    //each row

                    int fromLayer = i;
                    int toLayer = i + 1;
                   
                    Vector3 pos1 = new Vector3(fromLayer*distanceBetweenLayers, (x * distanceBetweenNodes) - (0.5f * network.weights[i].RowCount * distanceBetweenNodes),0);
                    Vector3 pos2 = new Vector3(toLayer*distanceBetweenLayers, (y * distanceBetweenNodes) - (0.5f * network.weights[i].ColumnCount * distanceBetweenNodes),0);
                    Vector3 pos2Offset = pos2-pos1;
                    GameObject line = Instantiate(linePrefab, pos1, Quaternion.identity);
                    line.transform.parent = transform;
                    line.GetComponent<LineRenderer>().SetPosition(0, transform.position + pos1);
                    line.GetComponent<LineRenderer>().SetPosition(1, transform.position + pos2);
                    line.GetComponent<LineRenderer>().startColor = GetWeightColor(network.weights[i][x, y]);
                    line.GetComponent<LineRenderer>().endColor = GetWeightColor(network.weights[i][x, y]);
                    graph.Add(line);
                }
            }
        }

    }

    void ClearGraph()
    {
        for (int i = 0; i < graph.Count; i++)
        {
            Destroy(graph[i]);
        }
    }

    Color GetNodeColor(float value)
    {
        value = Mathf.Clamp(value, 0f, 1f);
        return nodeGradient.Evaluate(value);
    }

    Color GetWeightColor(float value)
    {
        value = (value + 1)/2f;
        return weightGradient.Evaluate(value);
    }
}
