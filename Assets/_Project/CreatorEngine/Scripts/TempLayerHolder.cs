using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Arkh.CreatorEngine;

public class TempLayerHolder : MonoBehaviour
{
    public Button LayerButton;
    public CreationEditor Editor;
    public LayerManager Layers;
    int i;

    public void Start()
    {
        LayerManager.LayersUpdated += UpdateLayers;
    }

    // Start is called before the first frame update
    void UpdateLayers()
    {
        Debug.Log("Clean old Buttons");
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("Add New Buttons:" + LayerManager.CreationLayers.Count);
        i = 1;
        foreach (var layer in LayerManager.CreationLayers)
        {
            string vName = layer.Name;
            Debug.Log("building: " + vName);
            Button b = Instantiate(LayerButton, this.transform);
            LayerButton l = b.gameObject.AddComponent<LayerButton>();
            b.GetComponentInChildren<Text>().text = vName;
            l.Layer = layer;
            b.onClick.AddListener(() => { SelectLayer(l.Layer); });
            SelectLayer(l.Layer);
            i++;
        }
    }
    public void SelectLayer(CreationLayer l)
    {
        Debug.Log(l.ID);
        Layers.SelectLayer(l.ID);
        Editor.ChangeLayer();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
