/*
using UnityEngine;

[CreateAssetMenu(menuName = "SimpleGraph/RuntimeGraph")]
public class RuntimeGraph : ScriptableObject
{
    public GraphNodeData startNode;

    public void Execute()
    {
        GraphNodeData currentNode = startNode;

        while (currentNode != null)
        {
            currentNode.Execute(); // Assuming each node has its own Execute method
            currentNode = currentNode.nextNode; // Move to the next node
        }
    }
}
*/