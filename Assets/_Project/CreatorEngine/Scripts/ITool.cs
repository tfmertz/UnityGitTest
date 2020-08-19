using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITool
{
    void StartPreview(Vector3 pos);
    void StopPreview();
    void Apply();
}