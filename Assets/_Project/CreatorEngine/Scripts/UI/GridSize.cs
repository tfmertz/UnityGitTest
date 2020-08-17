using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Arkh.CreatorEngine;


public class GridSize : MonoBehaviour
{
    public InputField width;
    public InputField height;
    public CreationEditor creation;
    
    public void ResizeGrid()
    {
        creation.ChangeGridSize(int.Parse(width.text), int.Parse(height.text));
    }
}
