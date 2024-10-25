using UnityEngine;
using System.Collections.Generic;

namespace SimpleGraph {
    public class GraphManager : MonoBehaviour
    {
        public void CompleteNode(string nodeName)
        {
            foreach (Transform child in transform)
            {
                GraphNode node = child.GetComponent<GraphNode>();
                if (node != null && node.nodeName == nodeName)
                {
                    if (node.isActive)
                    {
                        // Check if all previous nodes are completed
                        if (ArePreviousNodesCompleted(node))
                        {
                            node.isCompleted = true;
                            Debug.Log("Node " + nodeName + " is now completed.");
                            ActivateNextNodes(node);
                        }
                        else
                        {
                            Debug.LogWarning("Not all previous nodes are completed.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Node " + nodeName + " is not active.");
                    }
                    return;
                }
            }

            Debug.LogWarning("Node with name " + nodeName + " not found.");
        }

        private bool ArePreviousNodesCompleted(GraphNode node)
        {
            foreach (GraphNode previousNode in node.previousNodes)
            {
                if (!previousNode.isCompleted)
                {
                    
                    return false;
                }
            }
            return true;
        }

        private void ActivateNextNodes(GraphNode node)
        {
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
    }
}

