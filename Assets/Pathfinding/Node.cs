using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node 
{
    public Vector3 world_position;
    public bool walkable;

    public int g_cost;
    public int h_cost;

    public int grid_x;
    public int grid_y;

    public Node parent = null;

    public Vector2 Cell()
    {
        return new Vector2(grid_x, grid_y);
    }

    public int f_cost
    {
        get { return g_cost + h_cost;}
    }

    public Node(Vector3 world_position_, bool walkable_, int x, int y)
    {
        world_position = world_position_;
        walkable = walkable_;

        grid_x = x;
        grid_y = y;
    }
}
