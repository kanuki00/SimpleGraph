using UnityEngine;
using System.Collections.Generic;

namespace SimpleGraph
{
    public static class GraphUtility
    {
        public static List<GraphNode> nodes = new List<GraphNode>();

        public static void RemoveNode(GraphNode node)
        {
            nodes.Remove(node);
            Object.DestroyImmediate(node.gameObject);
        }
    }
}