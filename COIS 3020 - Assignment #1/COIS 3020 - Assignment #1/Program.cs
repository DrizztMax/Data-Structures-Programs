using static System.Net.Mime.MediaTypeNames;

namespace CircularStrings
{
    public class Vertex<T>
    {
        public T Name { get; set; }
        public List<T> Edges { get; set; }

        public Vertex(T name)
        {
            Name = name;
            Edges = new List<T>();
        }
    }

    public class DirectedGraph<T>
    {
        private readonly List<Vertex<T>> vertices;

        public DirectedGraph()
        {
            vertices = new List<Vertex<T>>();
        }

        private Vertex<T> FindVertex(T name)
        {
            foreach (var vertex in vertices)
            {
                if (vertex.Name.Equals(name))
                    return vertex;
            }
            return null;
        }

        public void AddVertex(T name)
        {
            if (FindVertex(name) == null)
                vertices.Add(new Vertex<T>(name));
        }

        public void AddEdge(T from, T to)
        {
            var fromVertex = FindVertex(from);
            if (fromVertex != null)
            {
                fromVertex.Edges.Add(to);
            }
        }

        public bool HasEulerianTour()
        {
            // Check in-degree and out-degree for all vertices
            var inDegree = new Dictionary<T, int>();
            var outDegree = new Dictionary<T, int>();

            foreach (var vertex in vertices)
            {
                outDegree[vertex.Name] = vertex.Edges.Count;
                foreach (var edge in vertex.Edges)
                {
                    if (!inDegree.ContainsKey(edge))
                        inDegree[edge] = 0;

                    inDegree[edge]++;
                }
            }

            foreach (var vertex in vertices)
            {
                int outDeg = outDegree.ContainsKey(vertex.Name) ? outDegree[vertex.Name] : 0;
                int inDeg = inDegree.ContainsKey(vertex.Name) ? inDegree[vertex.Name] : 0;
                if (inDeg != outDeg)
                    return false;
            }

            return IsStronglyConnected();
        }

        private bool IsStronglyConnected()
        {
            if (vertices.Count == 0) return true;

            // Perform Depth-First Search to check connectivity
            var visited = new HashSet<T>();
            DFS(vertices[0].Name, visited);

            if (visited.Count != vertices.Count)
                return false;

            // Reverse the graph and check connectivity again
            var reversedGraph = ReverseGraph();
            visited.Clear();
            reversedGraph.DFS(reversedGraph.vertices[0].Name, visited);

            return visited.Count == reversedGraph.vertices.Count;
        }

        private void DFS(T current, HashSet<T> visited)
        {
            visited.Add(current);
            var vertex = FindVertex(current);
            if (vertex != null)
            {
                foreach (var neighbor in vertex.Edges)
                {
                    if (!visited.Contains(neighbor))
                    {
                        DFS(neighbor, visited);
                    }
                }
            }
        }

        private DirectedGraph<T> ReverseGraph()
        {
            var reversedGraph = new DirectedGraph<T>();
            foreach (var vertex in vertices)
            {
                reversedGraph.AddVertex(vertex.Name);
            }

            foreach (var vertex in vertices)
            {
                foreach (var edge in vertex.Edges)
                {
                    reversedGraph.AddEdge(edge, vertex.Name);
                }
            }

            return reversedGraph;
        }

        public List<T> FindEulerianTour()
        {
            var stack = new Stack<T>();
            var circuit = new List<T>();
            var tempGraph = new Dictionary<T, List<T>>();

            // Create a temporary graph for modification
            foreach (var vertex in vertices)
            {
                tempGraph[vertex.Name] = new List<T>(vertex.Edges);
            }

            // Start with any vertex that has edges
            T currentVertex = vertices[0].Name;
            stack.Push(currentVertex);

            while (stack.Count > 0)
            {
                if (tempGraph[currentVertex].Count == 0)
                {
                    circuit.Add(currentVertex);
                    currentVertex = stack.Pop();
                }
                else
                {
                    stack.Push(currentVertex);
                    T nextVertex = tempGraph[currentVertex][0];
                    tempGraph[currentVertex].RemoveAt(0);
                    currentVertex = nextVertex;
                }
            }

            circuit.Reverse();
            return circuit;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the number of strings:");
            int n = int.Parse(Console.ReadLine());
            while (n <= 0)
            {
                Console.WriteLine("Invalid string amount");
                Console.WriteLine("Enter the number of strings:");
                n = int.Parse(Console.ReadLine());
            }
            Console.WriteLine("Enter the strings:");
            string[] strings = new string[n];
            var stringMap = new Dictionary<(char, char), string>();

            for (int i = 0; i < n; i++)
            {
                strings[i] = Console.ReadLine().ToLower();
                char start = strings[i][0];
                char end = strings[i][^1];

                if (!stringMap.ContainsKey((start, end)))
                    stringMap[(start, end)] = strings[i];
            }

            var graph = new DirectedGraph<char>();

            // Build the graph
            foreach (var s in strings)
            {
                char start = s[0];
                char end = s[^1];

                graph.AddVertex(start);
                graph.AddVertex(end);
                graph.AddEdge(start, end);
            }

            // Check if the graph has an Eulerian tour
            if (graph.HasEulerianTour())
            {
                Console.WriteLine("Circular order exists.");
                var eulerianTour = graph.FindEulerianTour();
                Console.WriteLine("Order:");
                for (int i = 0; i < eulerianTour.Count - 1; i++)
                {
                    char start = eulerianTour[i];
                    char end = eulerianTour[i + 1];
                    Console.Write(stringMap[(start, end)] + " ");
                }
            }
            else
            {
                Console.WriteLine("No circular order exists.");
            }
        }
    }
}
