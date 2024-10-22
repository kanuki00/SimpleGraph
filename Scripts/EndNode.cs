using UnityEngine;

namespace SimpleGraph {
    public class EndNode : GraphNode
    {
        public override void DrawNodeWindow(int id)
        {
            GUILayout.Label("End Node");
            base.DrawNodeWindow(id);
        }
    }
}