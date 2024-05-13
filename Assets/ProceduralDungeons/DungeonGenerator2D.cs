using DelaunatorSharp;
using DelaunatorSharp.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DelaunatorSharp.Unity.Extensions;
using System;


[Serializable]
public class RoomGenerationInfo
{
    public Room room_type;
    public float chance;
};

public class DungeonGenerator2D : MonoBehaviour
{
    [SerializeField]
    bool regenerate = true;

    [SerializeField]
    public float seed = 0;

    [SerializeField]
    public Vector2 map_size = new Vector2();

    [SerializeField]
    public float max_room_dim;

    [SerializeField]
    public float min_room_size;

    [SerializeField]
    List<RoomGenerationInfo> room_types;

    [SerializeField]
    List<GameObject> hallway_types;

    [SerializeField]
    public Material line_material;

    public List<Room> rooms = new List<Room>();

    [SerializeField]
    private Transform triangles_container;
    [SerializeField]
    private Transform rooms_container;

    [SerializeField] 
    Color triangle_edge_color = Color.black;

    [SerializeField] 
    float triangle_edge_width = 1.0f;

    private Delaunator delaunator;
    private List<IPoint> points = new List<IPoint>();

    private List<Edge> mst_edges;
    private int edge_step_ctr = 0;

    private List<Edge> free_edges;
    private int free_edge_step_ctr = 0;

    public void Generate()
    {
        Clear();

        var sampler = UniformPoissonDiskSampler.SampleRectangle(Vector2.zero, map_size, min_room_size);
        points = sampler.Select(point => new Vector2(point.x - map_size.x / 2.0f, point.y - map_size.y / 2.0f)).ToPoints().ToList();
        if (points.Count < 3)
        {
            Debug.Log($"Failed to generate points, count {points.Count}");
            return;
        };

        delaunator = new Delaunator(points.ToArray());
        CreateTriangle();

        var triangles = delaunator.Triangles;

        // Convert the triangulation edges into an adjacency list representation
        Dictionary<int, List<int>> graph = new Dictionary<int, List<int>>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int a = triangles[i];
            int b = triangles[i + 1];
            int c = triangles[i + 2];

            AddEdge(graph, a, b);
            AddEdge(graph, b, c);
            AddEdge(graph, c, a);
        }

        // Apply Prim's algorithm to find the Minimum Spanning Tree
        mst_edges = PrimMST(graph);

        // Get free edges
        free_edges = GetNonMSTEdges(graph, mst_edges);

        GenerateHallways();
    }

    public void GenerateHallways()
    {
        foreach(Edge edge in mst_edges)
        {
            Vector3 position_p = new Vector3(points[edge.A].ToVector3().x, 0, points[edge.A].ToVector3().y);
            Vector3 position_q = new Vector3(points[edge.B].ToVector3().x, 0, points[edge.B].ToVector3().y);
            Vector3 mid_point = (position_p + position_q) / 2.0f;

            // Hook up teleporters
            {
                Teleporter a = null;
                Teleporter b = null;

                float min_midpoint_distance = float.MaxValue;
                foreach(Teleporter teleporter in rooms[edge.A].teleporters)
                {
                    float mid_point_distance = (mid_point - teleporter.transform.position).magnitude;
                    if (mid_point_distance < min_midpoint_distance)
                    {
                        if (teleporter.destination != null) continue;
                        a = teleporter;
                    }
                }

                min_midpoint_distance = float.MaxValue;
                foreach(Teleporter teleporter in rooms[edge.B].teleporters)
                {
                    float mid_point_distance = (mid_point - teleporter.transform.position).magnitude;
                    if (mid_point_distance < min_midpoint_distance)
                    {
                        if (teleporter.destination != null) continue;
                        b = teleporter;
                    }
                }

                if (a != null && b != null)
                {
                    Color color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                    a.GetComponent<Renderer>().material.color = color;
                    b.GetComponent<Renderer>().material.color = color;

                    a.destination = b;
                    b.destination = a;
                }
            }

            //PathfindingGrid grid = GetComponent<PathfindingGrid>();
            //grid.GenerateGrid();

            //Pathfinding pathfinding = GetComponent<Pathfinding>();

            //// Find Closest entrance
            //{
            //    float min_midpoint_distance = float.MaxValue;
            //    foreach(RoomEntrance entrance in rooms[edge.A].entrances)
            //    {
            //        float mid_point_distance = (mid_point - entrance.transform.position).magnitude;
            //        if (mid_point_distance < min_midpoint_distance)
            //        {
            //            pathfinding.seeker_transform = entrance.transform;
            //        }
            //    }
            //}

            //{
            //    float min_midpoint_distance = float.MaxValue;
            //    foreach(RoomEntrance entrance in rooms[edge.B].entrances)
            //    {
            //        float mid_point_distance = (mid_point - entrance.transform.position).magnitude;
            //        if (mid_point_distance < min_midpoint_distance)
            //        {
            //            pathfinding.target_transform = entrance.transform;
            //        }
            //    }
            //}

            //pathfinding.FindPath();
            //Debug.Assert(pathfinding.path.Count > 0, "Path between dungeons failed!");
            //var hallway = new GameObject($"from {edge.A} to {edge.B} ({grid.NodeFromWorldPoint(position_p).Cell()} to {grid.NodeFromWorldPoint(position_q).Cell()} )");
            //hallway.transform.SetPositionAndRotation((position_p + position_q) / 2.0f, Quaternion.identity);
            //foreach (Node node in pathfinding.path)
            //{
            //    Instantiate(hallway_types[0], node.world_position, Quaternion.identity, hallway.transform);
            //}
        }
    }

    void Step()
    {
        // Output or further processing of MST edges
        if (edge_step_ctr < mst_edges.Count)
        {
            var edge = mst_edges[edge_step_ctr++];
            Vector3 spawn_position_p = new Vector3(points[edge.A].ToVector3().x, 0, points[edge.A].ToVector3().y);
            Vector3 spawn_position_q = new Vector3(points[edge.B].ToVector3().x, 0, points[edge.B].ToVector3().y);
            CreateLine(triangles_container, "{edge.A} to {edge.B}", new Vector3[] { spawn_position_p, spawn_position_q}, Color.green, 2.0f);
        }
        else if (free_edge_step_ctr < free_edges.Count)
        {
            Edge free_edge = free_edges[free_edge_step_ctr++];
            if (mst_edges.Any(edge => edge.A == free_edge.A && edge.B == free_edge.B || edge.A == free_edge.B && edge.B == free_edge.A))
            {
                return;
            }
            Vector3 spawn_position_p = new Vector3(points[free_edge.A].ToVector3().x, 0, points[free_edge.A].ToVector3().y);
            Vector3 spawn_position_q = new Vector3(points[free_edge.B].ToVector3().x, 0, points[free_edge.B].ToVector3().y);
            CreateLine(triangles_container, "{edge.A} to {edge.B}", new Vector3[] { spawn_position_p, spawn_position_q }, Color.yellow, 2.0f);
        }

        Debug.Log("No more edges!");
    }

    // Update is called once per frame
    void Update()
    {
        if (regenerate)
        {
            Generate();
            regenerate = false;
        }

        //if(Input.GetKeyDown(KeyCode.Space)) 
        //{
        //    Step();
        //}
    }

    void Clear()
    {
        foreach(Room room in rooms)
        {
            Destroy(room.gameObject);
        }

        foreach (Transform child in triangles_container)
        {
            Destroy(child.gameObject);
        }

        rooms.Clear();
    }

    private void CreateLine(Transform container, string name, Vector3[] points, Color color, float width, int order = 1)
    {
        var lineGameObject = new GameObject(name);
        lineGameObject.transform.parent = container;
        var lineRenderer = lineGameObject.AddComponent<LineRenderer>();

        lineRenderer.SetPositions(points);

        lineRenderer.material = line_material ?? new Material(Shader.Find("Standard"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = 2;
        lineRenderer.endWidth = 2;
        lineRenderer.sortingOrder = order;
    }
    private void CreateTriangle()
    {
        if (delaunator == null) return;

        delaunator.ForEachTriangleEdge(edge =>
        {
            if (true)
            {
                Vector3 spawn_position_p = new Vector3(edge.P.ToVector3().x, 0, edge.P.ToVector3().y);
                Vector3 spawn_position_q = new Vector3(edge.P.ToVector3().x, 0, edge.P.ToVector3().y);
                CreateLine(triangles_container, $"TriangleEdge - {edge.Index}", new Vector3[] { spawn_position_p, spawn_position_q }, triangle_edge_color, triangle_edge_width, 1);
            }
        });

        foreach (IPoint point in points)
        {
            int room_type_index = UnityEngine.Random.Range(0, room_types.Count);
            while (room_types[room_type_index].chance < UnityEngine.Random.Range(0.0f, 1.0f))
            {
                room_type_index = UnityEngine.Random.Range(0, room_types.Count);
            }

            Vector3 spawn_position = new Vector3((float)point.X, 0, (float)point.Y);
            Vector3 euler_rotation = Vector3.zero;

            float[] angles = new float[]{ 0, 90, 180, 270 };
            euler_rotation.y = angles[UnityEngine.Random.Range(0, 3)];
            var room = Instantiate(room_types[room_type_index].room_type,spawn_position, Quaternion.Euler(euler_rotation), rooms_container);
            rooms.Add(room);
        }

    }
    private class Edge
    {
        public int A { get; set; }
        public int B { get; set; }
    }
    private static void AddEdge(Dictionary<int, List<int>> graph, int a, int b)
    {
        if (!graph.ContainsKey(a))
            graph[a] = new List<int>();
        graph[a].Add(b);
    }

    // Prim's algorithm implementation
    private static List<Edge> PrimMST(Dictionary<int, List<int>> graph)
    {
        HashSet<int> visited = new HashSet<int>();
        List<Edge> mstEdges = new List<Edge>();

        // Start from any vertex (here, we pick the first one)
        visited.Add(graph.Keys.First());

        while (visited.Count < graph.Count)
        {
            Edge minEdge = null;
            double minWeight = double.PositiveInfinity;

            foreach (var node in visited)
            {
                foreach (var neighbor in graph[node])
                {
                    if (!visited.Contains(neighbor))
                    {
                        // Consider adding the edge to the MST
                        double weight = Math.Abs(node - neighbor); // Or compute weight as needed
                        if (weight < minWeight)
                        {
                            minWeight = weight;
                            minEdge = new Edge { A = node, B = neighbor };
                        }
                    }
                }
            }

            // Add the minimum-weight edge to the MST
            mstEdges.Add(minEdge);
            visited.Add(minEdge.B); // Add the new vertex to the visited set
        }

        return mstEdges;
    }

    // Helper function to get edges not included in the MST
    private static List<Edge> GetNonMSTEdges(Dictionary<int, List<int>> graph, List<Edge> mstEdges)
    {
        List<Edge> nonMSTEdges = new List<Edge>();

        foreach (var node in graph)
        {
            foreach (var neighbor in node.Value)
            {
                Edge edge = new Edge { A = node.Key, B = neighbor };
                if (!mstEdges.Contains(edge) && !nonMSTEdges.Contains(edge))
                {
                    nonMSTEdges.Add(edge);
                }
            }
        }

        return nonMSTEdges;
    }
}
