using UnityEngine;
using System.Collections.Generic;

namespace SimpleGraph {
    public class DynamicNode : GraphNode
    {
        private List<Rect> inputPorts = new List<Rect>();
        private List<Rect> outputPorts = new List<Rect>();

        public override void DrawNodeWindow(int id)
        {
            GUILayout.Label("Dynamic Node");
            GUILayout.Label("HELLOW WORLD");
            

            // Draw input ports
            for (int i = 0; i < inputPorts.Count; i++)
            {
                if (GUI.Button(inputPorts[i], "In"))
                {
                    Debug.Log("Input port " + i + " clicked on node: " + nodeName);
                    //OnSetEndNode?.Invoke(this);
                }
            }

            // Draw output ports
            for (int i = 0; i < outputPorts.Count; i++)
            {
                if (GUI.Button(outputPorts[i], "Out"))
                {
                    Debug.Log("Output port " + i + " clicked on node: " + nodeName);
                    //OnSetStartNode?.Invoke(this);
                }
            }

            // Add buttons to add new ports
            if (GUILayout.Button("Add Input Port"))
            {
                AddInputPort();
            }
            if (GUILayout.Button("Add Output Port"))
            {
                AddOutputPort();
            }

            base.DrawNodeWindow(id);
        }

        
    

        private void AddInputPort()
        {
            float buttonWidth = 50;
            float buttonHeight = 20;
            float x = -buttonWidth; // Positioned just outside the left edge of the window
            float y = (inputPorts.Count * (buttonHeight + 5)) + 20; // Spaced vertically
            inputPorts.Add(new Rect(x, y, buttonWidth, buttonHeight));
        }

        private void AddOutputPort()
        {
            float buttonWidth = 50;
            float buttonHeight = 20;
            float x = windowRect.width; // Positioned just outside the right edge of the window
            float y = (outputPorts.Count * (buttonHeight + 5)) + 20; // Spaced vertically
            outputPorts.Add(new Rect(x, y, buttonWidth, buttonHeight));
        }
        
    }
}