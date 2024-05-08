using System.Collections.Generic;
using UnityEngine;

public class PathfindingGrid : MonoBehaviour
{
    public LayerMask unwalkable;

    [SerializeField]
    bool regenerate_grid;

    // Grid's dimensions in world space
    public Vector2 world_dimensions; // In X - Z 
    public float node_radius;

    Node[,] grid;

    [Header("Details")]
    [SerializeField] private float node_diameter;
    [SerializeField] private Vector2Int grid_dimensions;
    public void GenerateGrid()
    {
        grid = new Node[grid_dimensions.x, grid_dimensions.y];
        Vector3 world_bottom_left = transform.position - 
                                    Vector3.right * (world_dimensions.x / 2.0f) - 
                                    Vector3.forward * (world_dimensions.y / 2.0f);

        for (int x = 0; x < grid_dimensions.x; x++)
        {
            for (int y = 0; y < grid_dimensions.y; y++)
            {
                Vector3 node_position = world_bottom_left + 
                                        Vector3.right * (x * node_diameter + node_radius) +
                                        Vector3.forward * (y * node_diameter + node_radius);

                grid[x, y] = new Node(node_position, 
                                      !Physics.CheckSphere(node_position, node_radius, unwalkable),
                                      x,y);
            }
        }
    }

    private void Update()
    {
        if (regenerate_grid)
        {
            GenerateGrid();
            regenerate_grid = false;
        }
    }

    public Node NodeFromWorldPoint(Vector3 world_point)
    {
        Debug.Assert(grid != null, "No grid generated");

        Vector3 grid_space_world_coords = world_point - transform.position;

        int x = Mathf.FloorToInt((grid_space_world_coords.x + world_dimensions.x / 2.0f) / world_dimensions.x * grid_dimensions.x);
        int y = Mathf.FloorToInt((grid_space_world_coords.z + world_dimensions.y / 2.0f) / world_dimensions.y * grid_dimensions.y);

        if (x < 0 || x >= grid_dimensions.x ||y < 0 || y >= grid_dimensions.y )
        {
            Debug.Log("Point out of of range of grid!");
            return null;
        }

        return grid[x, y];
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int check_x = node.grid_x + x;
                int check_y = node.grid_y + y;

                if (check_x >= 0 && check_x < grid_dimensions.x && check_y >= 0 && check_y < grid_dimensions.y)
                {
                    neighbors.Add(grid[check_x, check_y]);
                }
            }
        }

        return neighbors;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(world_dimensions.x, 1, world_dimensions.y));

        if (grid != null)
        {
            foreach (Node node in grid)
            {
                if (!node.walkable)
                {
                    Color color = node.walkable ? Color.white : Color.red;
                    Gizmos.color = color * new Vector4(1.0f, 1.0f, 1.0f, 0.5f);
                    Gizmos.DrawCube(node.world_position, new Vector3(1.0f, 0.1f, 1.0f) * (node_diameter));
                }
            }
        }
    }
    void Awake()
    {
        node_diameter = node_radius * 2.0f;
        grid_dimensions = Vector2Int.RoundToInt(world_dimensions / node_diameter);

        if (regenerate_grid)
        {
            GenerateGrid();
            regenerate_grid = false;
        }
    }
}
