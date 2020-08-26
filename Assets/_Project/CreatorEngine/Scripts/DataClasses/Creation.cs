﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arkh.CreatorEngine
{
    /// <summary>
    /// Houses the data associated with a Creation inside the creation tool
    /// </summary>
    public class Creation
    {
        public string name;
        //public Voxel[] voxels;
        public List<CreationLayer> Layers;
    }

    [System.Serializable]
    public class Voxel
    {
        public Vector3 position;
        public Color color;

        public Voxel Clone()
        {
            return new Voxel
            {
                position = this.position,
                color = this.color,
            };
        }
    }
}