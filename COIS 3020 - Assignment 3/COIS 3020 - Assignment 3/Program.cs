using System;
using System.Collections.Generic;
public class Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString() => $"({X}, {Y})";

    public override bool Equals(object obj)
    {
        if (obj is Point p)
            return X == p.X && Y == p.Y;
        return false;
    }

    public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();
}
public class PointQuadtreeNode
{
    public Point Point { get; set; }
    public int MinX, MaxX, MinY, MaxY;

    public PointQuadtreeNode NW, NE, SW, SE;

    public PointQuadtreeNode(Point point, int minX, int maxX, int minY, int maxY)
    {
        Point = point;
        MinX = minX; MaxX = maxX;
        MinY = minY; MaxY = maxY;
    }

    public bool IsLeaf => NW == null && NE == null && SW == null && SE == null;
}
public class PointQuadtree
{
    private PointQuadtreeNode root;
    private int size;

    public PointQuadtree(int spaceSize)
    {
        // Assume space goes from (0, 0) to (spaceSize, spaceSize)
        size = spaceSize;
        root = null;
    }
    public bool Insert(Point p)
    {
        if (root == null)
        {
            root = new PointQuadtreeNode(p, 0, size, 0, size);
            return true;
        }
        return Insert(root, p);
    }

    private bool Insert(PointQuadtreeNode node, Point p)
    {
        if (node.Point.Equals(p)) return false; // duplicate

        // Decide quadrant
        int midX = (node.MinX + node.MaxX) / 2;
        int midY = (node.MinY + node.MaxY) / 2;

        if (p.X < midX)
        {
            if (p.Y < midY)
            {
                if (node.SW == null)
                    node.SW = new PointQuadtreeNode(p, node.MinX, midX, node.MinY, midY);
                else
                    return Insert(node.SW, p);
            }
            else
            {
                if (node.NW == null)
                    node.NW = new PointQuadtreeNode(p, node.MinX, midX, midY, node.MaxY);
                else
                    return Insert(node.NW, p);
            }
        }
        else
        {
            if (p.Y < midY)
            {
                if (node.SE == null)
                    node.SE = new PointQuadtreeNode(p, midX, node.MaxX, node.MinY, midY);
                else
                    return Insert(node.SE, p);
            }
            else
            {
                if (node.NE == null)
                    node.NE = new PointQuadtreeNode(p, midX, node.MaxX, midY, node.MaxY);
                else
                    return Insert(node.NE, p);
            }
        }
        return true;
    }
    public bool Contains(Point p)
    {
        return Contains(root, p);
    }

    private bool Contains(PointQuadtreeNode node, Point p)
    {
        if (node == null) return false;

        if (node.Point.Equals(p)) return true;

        int midX = (node.MinX + node.MaxX) / 2;
        int midY = (node.MinY + node.MaxY) / 2;

        if (p.X < midX)
        {
            if (p.Y < midY)
                return Contains(node.SW, p);
            else
                return Contains(node.NW, p);
        }
        else
        {
            if (p.Y < midY)
                return Contains(node.SE, p);
            else
                return Contains(node.NE, p);
        }
    }
    public bool Delete(Point target)
    {
        bool deleted;
        root = Delete(root, target, out deleted);
        return deleted;
    }

    private PointQuadtreeNode Delete(PointQuadtreeNode node, Point target, out bool deleted)
    {
        deleted = false;
        if (node == null || node.Point == null) return null;

        if (node.Point.Equals(target))
        {
            // Gather all points from this subtree (excluding the one to delete)
            List<Point> remaining = new List<Point>();
            CollectPoints(node, remaining);
            remaining.RemoveAll(p => p.Equals(target));

            // Rebuild subtree from remaining points
            PointQuadtreeNode newNode = null;
            foreach (var pt in remaining)
            {
                if (newNode == null)
                    newNode = new PointQuadtreeNode(pt, node.MinX, node.MaxX, node.MinY, node.MaxY);
                else
                    Insert(newNode, pt);
            }

            deleted = true;
            return newNode;
        }

        // Recurse to find the node to delete
        int midX = (node.MinX + node.MaxX) / 2;
        int midY = (node.MinY + node.MaxY) / 2;

        if (target.X < midX)
        {
            if (target.Y < midY)
                node.SW = Delete(node.SW, target, out deleted);
            else
                node.NW = Delete(node.NW, target, out deleted);
        }
        else
        {
            if (target.Y < midY)
                node.SE = Delete(node.SE, target, out deleted);
            else
                node.NE = Delete(node.NE, target, out deleted);
        }

        return node;
    }

    private void CollectPoints(PointQuadtreeNode node, List<Point> list)
    {
        if (node == null || node.Point == null) return;

        list.Add(node.Point);
        CollectPoints(node.NW, list);
        CollectPoints(node.NE, list);
        CollectPoints(node.SW, list);
        CollectPoints(node.SE, list);
    }
    public List<Point> RangeSearch(Point topLeft, Point bottomRight)
    {
        List<Point> result = new List<Point>();
        RangeSearch(root, topLeft, bottomRight, result);
        return result;
    }

    private void RangeSearch(PointQuadtreeNode node, Point topLeft, Point bottomRight, List<Point> result)
    {
        if (node == null || node.Point == null)
            return;

        int x = node.Point.X;
        int y = node.Point.Y;

        // Check if point is within rectangle
        if (x >= topLeft.X && x <= bottomRight.X &&
            y >= bottomRight.Y && y <= topLeft.Y)
        {
            result.Add(node.Point);
        }

        int midX = (node.MinX + node.MaxX) / 2;
        int midY = (node.MinY + node.MaxY) / 2;

        // Visit only quadrants that intersect the query rectangle

        // NW
        if (topLeft.X < midX && topLeft.Y > midY)
            RangeSearch(node.NW, topLeft, bottomRight, result);

        // NE
        if (bottomRight.X >= midX && topLeft.Y > midY)
            RangeSearch(node.NE, topLeft, bottomRight, result);

        // SW
        if (topLeft.X < midX && bottomRight.Y <= midY)
            RangeSearch(node.SW, topLeft, bottomRight, result);

        // SE
        if (bottomRight.X >= midX && bottomRight.Y <= midY)
            RangeSearch(node.SE, topLeft, bottomRight, result);
    }
    public void Print()
    {
        Console.WriteLine("PointQuadtree:");
        Print(root, 0);
    }

    private void Print(PointQuadtreeNode node, int indent)
    {
        if (node == null || node.Point == null) return;

        string pad = new string(' ', indent * 2);
        Console.WriteLine($"{pad}{node.Point}");

        Print(node.NW, indent + 1);
        Print(node.NE, indent + 1);
        Print(node.SW, indent + 1);
        Print(node.SE, indent + 1);
    }
}
public class KDNode
{
    public Point Point;
    public KDNode Left;
    public KDNode Right;

    public KDNode(Point point)
    {
        Point = point;
    }
}
public class KDTree
{
    private KDNode root;

    public KDTree()
    {
        root = null;
    }
    public void Insert(Point p)
    {
        root = Insert(root, p, 0);
    }

    private KDNode Insert(KDNode node, Point p, int depth)
    {
        if (node == null) return new KDNode(p);

        int cd = depth % 2; // cd = current dimension (0 for x, 1 for y)

        if (node.Point.Equals(p))
            return node; // Duplicate, do nothing

        if ((cd == 0 && p.X < node.Point.X) || (cd == 1 && p.Y < node.Point.Y))
            node.Left = Insert(node.Left, p, depth + 1);
        else
            node.Right = Insert(node.Right, p, depth + 1);

        return node;
    }
    public bool Contains(Point p)
    {
        return Contains(root, p, 0);
    }

    private bool Contains(KDNode node, Point p, int depth)
    {
        if (node == null) return false;

        if (node.Point.Equals(p)) return true;

        int cd = depth % 2;

        if ((cd == 0 && p.X < node.Point.X) || (cd == 1 && p.Y < node.Point.Y))
            return Contains(node.Left, p, depth + 1);
        else
            return Contains(node.Right, p, depth + 1);
    }
    public void Delete(Point p)
    {
        root = Delete(root, p, 0);
    }

    private KDNode Delete(KDNode node, Point p, int depth)
    {
        if (node == null) return null;

        int cd = depth % 2;

        if (node.Point.Equals(p))
        {
            // Case 1: Node has right subtree
            if (node.Right != null)
            {
                KDNode min = FindMin(node.Right, cd, depth + 1);
                node.Point = min.Point;
                node.Right = Delete(node.Right, min.Point, depth + 1);
            }
            else if (node.Left != null)
            {
                KDNode min = FindMin(node.Left, cd, depth + 1);
                node.Point = min.Point;
                node.Right = Delete(node.Left, min.Point, depth + 1);
                node.Left = null;
            }
            else
            {
                return null; // Leaf node
            }
            return node;
        }

        if ((cd == 0 && p.X < node.Point.X) || (cd == 1 && p.Y < node.Point.Y))
            node.Left = Delete(node.Left, p, depth + 1);
        else
            node.Right = Delete(node.Right, p, depth + 1);

        return node;
    }
    private KDNode FindMin(KDNode node, int dim, int depth)
    {
        if (node == null) return null;

        int cd = depth % 2;

        if (cd == dim)
        {
            if (node.Left == null)
                return node;
            return FindMin(node.Left, dim, depth + 1);
        }

        KDNode leftMin = FindMin(node.Left, dim, depth + 1);
        KDNode rightMin = FindMin(node.Right, dim, depth + 1);

        KDNode min = node;
        if (leftMin != null && GetCoord(leftMin.Point, dim) < GetCoord(min.Point, dim))
            min = leftMin;
        if (rightMin != null && GetCoord(rightMin.Point, dim) < GetCoord(min.Point, dim))
            min = rightMin;

        return min;
    }

    private int GetCoord(Point p, int dim)
    {
        return dim == 0 ? p.X : p.Y;
    }
    public List<Point> RangeSearch(Point topLeft, Point bottomRight)
    {
        List<Point> result = new List<Point>();
        RangeSearch(root, topLeft, bottomRight, result, 0);
        return result;
    }

    private void RangeSearch(KDNode node, Point topLeft, Point bottomRight, List<Point> result, int depth)
    {
        if (node == null) return;

        int x = node.Point.X;
        int y = node.Point.Y;

        // Check if point is inside rectangle
        if (x >= topLeft.X && x <= bottomRight.X &&
            y >= bottomRight.Y && y <= topLeft.Y)
        {
            result.Add(node.Point);
        }

        int cd = depth % 2;

        // Decide whether to search left and/or right
        if ((cd == 0 && topLeft.X <= x) || (cd == 1 && bottomRight.Y <= y))
            RangeSearch(node.Left, topLeft, bottomRight, result, depth + 1);

        if ((cd == 0 && bottomRight.X >= x) || (cd == 1 && topLeft.Y >= y))
            RangeSearch(node.Right, topLeft, bottomRight, result, depth + 1);
    }
    public void Print()
    {
        Console.WriteLine("KDTree:");
        Print(root, 0);
    }

    private void Print(KDNode node, int depth)
    {
        if (node == null) return;

        string indent = new string(' ', depth * 2);
        Console.WriteLine($"{indent}{node.Point}");

        Print(node.Left, depth + 1);
        Print(node.Right, depth + 1);
    }
}
class Program
{
    static void Main(string[] args)
    {
        PointQuadtree quadTree = new PointQuadtree(16);  // 16x16 space
        KDTree kdTree = new KDTree();

        Console.WriteLine("== PointQuadtree & KDTree Tester ==");
        Console.WriteLine("Coordinate space is 0 ≤ x, y < 16");

        while (true)
        {
            Console.WriteLine("\n--- MENU ---");
            Console.WriteLine("1. Insert Point");
            Console.WriteLine("2. Delete Point");
            Console.WriteLine("3. Search Point (Contains)");
            Console.WriteLine("4. Range Search");
            Console.WriteLine("5. Print Quadtree");
            Console.WriteLine("6. Print KDTree");
            Console.WriteLine("7. Exit");
            Console.Write("Choose option: ");

            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    {
                        var p = ReadPoint("Enter point to insert (x y): ");
                        quadTree.Insert(p);
                        kdTree.Insert(p);
                        Console.WriteLine("Inserted into both trees.");
                        break;
                    }
                case "2":
                    {
                        var p = ReadPoint("Enter point to delete (x y): ");
                        quadTree.Delete(p);
                        kdTree.Delete(p);
                        Console.WriteLine("Deleted from both trees (if present).");
                        break;
                    }
                case "3":
                    {
                        var p = ReadPoint("Enter point to search (x y): ");
                        bool inQuad = quadTree.Contains(p);
                        bool inKD = kdTree.Contains(p);
                        Console.WriteLine($"Found in Quadtree: {inQuad}");
                        Console.WriteLine($"Found in KDTree  : {inKD}");
                        break;
                    }
                case "4":
                    {
                        Console.WriteLine("Define rectangle:");
                        var topLeft = ReadPoint("Top-left (x y): ");
                        var bottomRight = ReadPoint("Bottom-right (x y): ");

                        var resultsQuad = quadTree.RangeSearch(topLeft, bottomRight);
                        var resultsKD = kdTree.RangeSearch(topLeft, bottomRight);

                        Console.WriteLine("Points in Quadtree:");
                        foreach (var p in resultsQuad)
                            Console.WriteLine(p);

                        Console.WriteLine("Points in KDTree:");
                        foreach (var p in resultsKD)
                            Console.WriteLine(p);
                        break;
                    }
                case "5":
                    quadTree.Print();
                    break;
                case "6":
                    kdTree.Print();
                    break;
                case "7":
                    Console.WriteLine("Exiting. Bye!");
                    return;
                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
        }
    }

    static Point ReadPoint(string prompt)
    {
        Console.Write(prompt);
        string[] parts = Console.ReadLine().Split();
        return new Point(int.Parse(parts[0]), int.Parse(parts[1]));
    }
}
