using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    [SerializeField] 
    public PathfindingGrid grid;

    [SerializeField]
    int max_iterations = 1000;

    [SerializeField]
    public Transform seeker_transform;

    [SerializeField]
    public Transform target_transform;

    [Header("Details")]
    [SerializeField]
    Vector2Int from;
    [SerializeField]
    Vector2Int to;
    [SerializeField]
    int step_ctr = 0;

    public List<Node> path = new List<Node>();

    private Node start;
    private Node target;

    private void Start()
    {
        if (grid == null)
        {
            GetComponent<PathfindingGrid>();
        }
    }
    private void OnDrawGizmos()
    {
        foreach (Node node in path)
        {
            Gizmos.color = Color.green * new Vector4(1.0f, 1.0f, 1.0f, 0.5f);
            Gizmos.DrawCube(node.world_position, new Vector3(1.0f, 0.1f, 1.0f) * (grid.node_radius * 2.0f - 0.1f));
        }
    }

    public void FindPath()
    {
        start = grid.NodeFromWorldPoint(seeker_transform.position);
        target = grid.NodeFromWorldPoint(target_transform.position);

        if (start == null || target == null || !target.walkable)
        {
            Debug.Log("Invalid seeker or target!");
            return;
        }

        from = new Vector2Int(start.grid_x, start.grid_y);
        to = new Vector2Int(target.grid_x, target.grid_y);

        var open = new List<Node>();
        var closed = new HashSet<Node>();

        open.Add(start);
        step_ctr = 0;
        while (open.Count > 0)
        {
            if (++step_ctr > max_iterations) return;
            Node current = open[0];

            // Find node with lowest f cost
            for(int i = 0; i < open.Count; i++) 
            {
                if (open[i].f_cost < current.f_cost ||
                    (open[i].f_cost == current.f_cost && open[i].h_cost < current.h_cost))
                {
                    current = open[i];
                }
            }

            open.Remove(current);
            closed.Add(current);

            if (current == target)
            {
                path.Clear();
                {
                    Node n = target;
                    while(n != start)
                    {
                        path.Add(n);
                        n = n.parent;
                    }
                    path.Add(start);
                }
                path.Reverse();
                return;
            }

            foreach (Node neighbor in grid.GetNeighbors(current))
            {
                if (!neighbor.walkable || closed.Contains(neighbor))
                {
                    continue;
                }

                int g_cost = current.g_cost + GetDistance(current, neighbor);

                bool in_open = open.Contains(neighbor);
                if (g_cost < neighbor.g_cost || !in_open)
                {
                    neighbor.g_cost = g_cost;
                    neighbor.h_cost = GetDistance(neighbor, target);
                    neighbor.parent = current;

                    if (!in_open)
                    {
                        open.Add(neighbor);
                    }
                }
            }
        }

        Debug.Log($"Failed to find from {start.grid_x}, {start.grid_y}  to {target.grid_x}, {target.grid_y}");
    }

    int GetDistance(Node a, Node b)
    {
        int distance_x = Mathf.Abs(a.grid_x - b.grid_x);
        int distance_y = Mathf.Abs(a.grid_y - b.grid_y);

        if (distance_x > distance_y)
        {
            return 14 * distance_y + (10 * (distance_x - distance_y));
        }
        else
        {
            return 14 * distance_x + (10 * (distance_y - distance_x));
        }
    }
}
