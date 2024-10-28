using UnityEngine;
using System.Collections.Generic;

namespace SimpleGraph {
    public class GraphManager : MonoBehaviour
    {
        void Start() {
            ActivateStartNode();
        }

        public void CompleteNode(string nodeName)
        {
            Debug.Log($"[GraphManager] CompleteNode called with nodeName: {nodeName}");
            foreach (Transform child in transform)
            {
                GraphNode node = child.GetComponent<GraphNode>();
                if (node != null && node.nodeName == nodeName)
                {
                    Debug.Log($"[GraphManager] Found node: {nodeName}");
                    if (node.isActive)
                    {
                        Debug.Log($"[GraphManager] Node {nodeName} is active.");
                        // Check if all previous nodes are completed
                        if (ArePreviousNodesCompleted(node))
                        {
                            node.UpdateCompletionState(true);
                            Debug.Log($"[GraphManager] Node {nodeName} is now completed.");
                            ActivateNextNodes(node);
                        }
                        else
                        {
                            Debug.LogWarning($"[GraphManager] Not all previous nodes are completed for node {nodeName}.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[GraphManager] Node {nodeName} is not active.");
                    }
                    return;
                }
            }

            Debug.LogWarning($"[GraphManager] Node with name {nodeName} not found.");
        }

        private bool ArePreviousNodesCompleted(GraphNode node)
        {
            Debug.Log($"[GraphManager] Checking if previous nodes are completed for node: {node.nodeName}");
            foreach (GraphNode previousNode in node.previousNodes)
            {
                if (!previousNode.isCompleted)
                {
                    
                    return false;
                }
            }
            Debug.Log($"[GraphManager] All previous nodes are completed for node: {node.nodeName}");
            return true;
        }

        private void ActivateNextNodes(GraphNode node)
        {
            Debug.Log($"[GraphManager] Activating next nodes for node: {node.nodeName}");
            foreach (GraphNode nextNode in node.nextNodes)
            {
                if (nextNode.nodeName == "InverterNode")
                {
                    nextNode.isActive = !node.isActive;
                    Debug.Log("Inverter node " + nextNode.nodeName + " state is now " + nextNode.isActive);
                }
                else
                {
                    nextNode.isActive = true;
                    Debug.Log("Node " + nextNode.nodeName + " is now active.");
                }
            }
        }

        private void ActivateStartNode()
        {
            foreach (Transform child in transform)
            {
                GraphNode node = child.GetComponent<GraphNode>();
                if (node != null && node.nodeName == "StartNode")
                {
                    node.isActive = true;
                    Debug.Log($"[GraphManager] Start node {node.nodeName} is now active.");
                    return;
                }
            }

            Debug.LogWarning("[GraphManager] Start node with name 'StartNode' not found.");
        }
    }
}

