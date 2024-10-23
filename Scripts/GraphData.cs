using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleGraph
{
    [Serializable]
    public class GraphNodeData
    {
        public string nodeName;
        public Rect windowRect;
        public List<int> connectedNodeIndices = new List<int>();
    }

    [Serializable]
    public class GraphData
    {
        public List<GraphNodeData> nodes = new List<GraphNodeData>();
    }
}
