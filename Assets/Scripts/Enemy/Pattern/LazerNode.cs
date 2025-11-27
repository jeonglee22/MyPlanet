using System.Collections.Generic;
using UnityEngine;

public class LazerNode
{
    public Vector3 Position { get; private set; }
    public LazerNode Parent { get; private set; }
    public List<LazerNode> Children { get; private set; }

    public LazerNode(Vector3 position, LazerNode parent = null)
    {
        Position = position;
        Parent = parent;
        Children = new List<LazerNode>();
    }

    public void AddChild(LazerNode child)
    {
        Children.Add(child);
    }
}
