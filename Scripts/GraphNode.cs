using UnityEngine;
using System.Collections.Generic;
using SimpleGraph; // Import the namespace where GraphUtility is defined

namespace SimpleGraph
{
    public class GraphNode : MonoBehaviour
    {
        public Rect windowRect;
        public string nodeName;

        [HideInInspector]
        public List<GraphNode> connectedNodes = new List<GraphNode>();
        public List<GraphNode> previousNodes = new List<GraphNode>(); // Add list for previous nodes
        public List<GraphNode> nextNodes = new List<GraphNode>(); // Add list for next nodes

        public bool isCompleted = false; // Add isCompleted state
        public bool isActive = false; // Add isActive state

        public delegate void NodeEventHandler(GraphNode node);
        public static event NodeEventHandler OnSetStartNode;
        public static event NodeEventHandler OnSetEndNode;

        public virtual void DrawConnectionPoints()
        {
            // Calculate positions for the buttons
            float buttonWidth = 50;
            float buttonHeight = 20;
            float inButtonX = windowRect.width - buttonWidth - 10; // 10 pixels from the right edge
            float inButtonY = 10;
            //float inButtonY = (windowRect.height / 2) - (buttonHeight / 2); // Centered vertically
            float outButtonX = 10; // 10 pixels from the left edge
            float outButtonY = 10;
            //float outButtonY = (windowRect.height / 2) - (buttonHeight / 2); // Centered vertically

            // Draw "In" button
            if (GUI.Button(new Rect(inButtonX, inButtonY, buttonWidth, buttonHeight), "Out"))
            {
                Debug.Log("In button clicked on node: " + nodeName);
                OnSetStartNode?.Invoke(this);
            }
            
            // Draw "Out" button
            if (GUI.Button(new Rect(outButtonX, outButtonY, buttonWidth, buttonHeight), "In"))
            {
                Debug.Log("Out button clicked on node: " + nodeName);
                OnSetEndNode?.Invoke(this);
            }
        }

        public virtual void DrawNodeWindow(int id)
        {
            // Add the "x" button in the top-right corner
            if (GUI.Button(new Rect(windowRect.width - 20, 0, 20, 20), "x"))
            {
                Debug.Log("Delete button clicked on node: " + nodeName);
                GraphUtility.RemoveNode(this);
            }

            GUILayout.BeginVertical();
            GUILayout.Label("Node: " + nodeName);

            // Display the status
            GUILayout.Label("Completed: " + isCompleted);
            GUILayout.Label("Active: " + isActive);

            GUILayout.EndVertical();

            DrawConnectionPoints();
            GUI.DragWindow();
        }
    }
}