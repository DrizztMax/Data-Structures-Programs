using System;
using System.Collections.Generic;

namespace Treap
{
    // Interfaces used for a Treap

    public interface IContainer<T>
    {
        void MakeEmpty();         // Reset to empty
        bool Empty();             // Return true if empty; false otherwise 
        int Size();               // Return size 
    }

    //-------------------------------------------------------------------------

    public interface ISearchable<T> : IContainer<T>
    {
        void Add(T item);         // Add item to the treap (duplicates are not permitted)     
        void Remove(T item);      // Remove item from the treap
        bool Contains(T item);    // Return true if item found; false otherwise
    }

    //-------------------------------------------------------------------------

    // Generic node class for a Treap
    public class Node<T> where T : IComparable
    {
        private static Random R = new Random();

        // Read/write properties
        public T Item { get; set; }
        public int Priority { get; set; }  // Randomly generated
        public Node<T> Left { get; set; }
        public Node<T> Right { get; set; }

        // Node constructor
        public Node(T item)
        {
            Item = item;
            Priority = R.Next(10, 100);
            Left = Right = null;
        }
    }

    //-------------------------------------------------------------------------

    // Implementation:  Treap
    class Treap<T> : ISearchable<T> where T : IComparable
    {
        private Node<T> Root;  // Reference to the root of the Treap

        // Constructor Treap
        // Creates an empty Treap
        // Time complexity:  O(1)
        public Treap()
        {
            MakeEmpty();
        }

        // LeftRotate
        // Performs a left rotation around the given root
        // Time complexity:  O(1)
        private Node<T> LeftRotate(Node<T> root)
        {
            Node<T> temp = root.Right;
            root.Right = temp.Left;
            temp.Left = root;
            return temp;
        }

        // RightRotate
        // Performs a right rotation around the given root
        // Time complexity:  O(1)
        private Node<T> RightRotate(Node<T> root)
        {
            Node<T> temp = root.Left;
            root.Left = temp.Right;
            temp.Right = root;
            return temp;
        }

        // Public Add
        // Inserts the given item into the Treap
        // Calls Private Add to carry out the actual insertion
        // Expected time complexity:  O(log n)
        public void Add(T item)
        {
            Root = Add(item, Root);
        }

        // private Add method
        // Time complexity:  O(log n)
        private Node<T> Add(T item, Node<T> root)
        {
            int cmp;

            if (root == null)
                return new Node<T>(item);
            else
            {
                cmp = item.CompareTo(root.Item);
                if (cmp > 0)
                {
                    root.Right = Add(item, root.Right);  // Move right
                    if (root.Right.Priority > root.Priority)  // Rotate left if necessary
                        root = LeftRotate(root);
                }
                else if (cmp < 0)
                {
                    root.Left = Add(item, root.Left);  // Move left
                    if (root.Left.Priority > root.Priority)  // Rotate right if necessary
                        root = RightRotate(root);
                }
                return root;
            }
        }

        // Public Remove
        // Removes the given item from the Treap
        // Calls Private Remove to carry out the actual removal
        // Expected time complexity:  O(log n)
        public void Remove(T item)
        {
            Root = Remove(item, Root);
        }

        // private Remove method
        // Time complexity:  O(log n)
        private Node<T> Remove(T item, Node<T> root)
        {
            int cmp;

            if (root == null)  // Item not found
                return null;
            else
            {
                cmp = item.CompareTo(root.Item);
                if (cmp < 0)
                    root.Left = Remove(item, root.Left);  // Move left
                else if (cmp > 0)
                    root.Right = Remove(item, root.Right);  // Move right
                else  // Item found
                {
                    // Case: Two children
                    if (root.Left != null && root.Right != null)
                    {
                        if (root.Left.Priority > root.Right.Priority)
                            root = RightRotate(root);
                        else
                            root = LeftRotate(root);
                    }
                    // Case: One child
                    else if (root.Left != null)
                        root = RightRotate(root);
                    else if (root.Right != null)
                        root = LeftRotate(root);
                    // Case: No children (leaf node)
                    else
                        return null;

                    // Recursively remove item
                    root = Remove(item, root);
                }
                return root;
            }
        }

        // Contains
        // Returns true if the given item is found in the Treap; false otherwise
        // Expected Time complexity:  O(log n)
        public bool Contains(T item)
        {
            Node<T> curr = Root;

            while (curr != null)
            {
                if (item.CompareTo(curr.Item) == 0)     // Found
                    return true;
                else
                    if (item.CompareTo(curr.Item) < 0)
                    curr = curr.Left;               // Move left
                else
                    curr = curr.Right;              // Move right
            }
            return false;
        }

        // MakeEmpty
        // Creates an empty Treap
        // Time complexity: O(1)
        public void MakeEmpty()
        {
            Root = null;
        }

        // Empty
        // Returns true if the Treap is empty; false otherwise
        // Time complexity: O(1)
        public bool Empty()
        {
            return Root == null;
        }

        // Public Size
        // Returns the number of items in the Treap
        // Calls Private Size to carry out the actual calculation
        // Time complexity: O(n)
        public int Size()
        {
            return Size(Root);
        }

        // Size
        // Returns the number of items in the given Treap
        // Time complexity: O(n)
        private int Size(Node<T> root)
        {
            if (root == null)
                return 0;
            else
                return 1 + Size(root.Left) + Size(root.Right);
        }

        // Public Height
        // Returns the height of the Treap
        // Calls Private Height to carry out the actual calculation
        // Time complexity: O(n)
        public int Height()
        {
            return Height(Root);
        }

        // Private Height
        // Returns the height of the given Treap
        // Time complexity: O(n)
        private int Height(Node<T> root)
        {
            if (root == null)
                return -1;    // By default for an empty Treap
            else
                return 1 + Math.Max(Height(root.Left), Height(root.Right));
        }

        // Public MinGap
        // Returns the minimum gap between any two elements in the Treap
        // If there are fewer than two elements, returns 0
        // Time complexity: O(n) (single traversal)
        public int MinGap()
        {
            if (Root == null || (Root.Left == null && Root.Right == null))
                return 0;  // No gap in an empty or single-element Treap

            int minGap = int.MaxValue;
            Node<T> prev = null;

            // Use an in-order traversal to visit nodes in sorted order
            Stack<Node<T>> stack = new Stack<Node<T>>();
            Node<T> current = Root;

            // Traverse the tree
            while (stack.Count > 0 || current != null)
            {
                while (current != null)
                {
                    stack.Push(current);
                    current = current.Left;
                }

                current = stack.Pop();

                // Compare the current node with the previous node to find the gap
                if (prev != null)
                {
                    // Correct gap calculation using subtraction
                    int gap = Math.Abs(Convert.ToInt32(current.Item) - Convert.ToInt32(prev.Item));
                    minGap = Math.Min(minGap, gap);
                }

                // Update the previous node
                prev = current;

                current = current.Right;
            }

            return minGap;
        }


        // Public Print
        // Prints out the items of the Treap inorder
        // Calls Private Print to carry out the actual printing
        // Time complexity: O(n)
        public void Print()
        {
            Print(Root, 0);
        }

        // Print
        // Inorder traversal of the BST
        // Time complexity: O(n)
        private void Print(Node<T> root, int index)
        {
            if (root != null)
            {
                Print(root.Right, index + 5);
                Console.WriteLine(new String(' ', index) + root.Item.ToString() + " " + root.Priority.ToString());
                Print(root.Left, index + 5);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Treap<int> treap = new Treap<int>();
            bool running = true;

            while (running)
            {
                Console.WriteLine("\nTreap Operations:");
                Console.WriteLine("1. Add an element");
                Console.WriteLine("2. Remove an element");
                Console.WriteLine("3. Check if an element exists");
                Console.WriteLine("4. Print the Treap");
                Console.WriteLine("5. Get Treap size");
                Console.WriteLine("6. Get Treap height");
                Console.WriteLine("7. Get minimum gap");
                Console.WriteLine("8. Exit");
                Console.Write("Select an option: ");

                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        Console.Write("Enter an element to add: ");
                        if (int.TryParse(Console.ReadLine(), out int addValue))
                        {
                            treap.Add(addValue);
                            Console.WriteLine($"{addValue} added to the Treap.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid input.");
                        }
                        break;

                    case "2":
                        Console.Write("Enter an element to remove: ");
                        if (int.TryParse(Console.ReadLine(), out int removeValue))
                        {
                            treap.Remove(removeValue);
                            Console.WriteLine($"{removeValue} removed from the Treap.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid input.");
                        }
                        break;

                    case "3":
                        Console.Write("Enter an element to check: ");
                        if (int.TryParse(Console.ReadLine(), out int checkValue))
                        {
                            Console.WriteLine(treap.Contains(checkValue) ? "Element exists in the Treap." : "Element not found in the Treap.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid input.");
                        }
                        break;

                    case "4":
                        Console.WriteLine("Treap structure:");
                        treap.Print();
                        break;

                    case "5":
                        Console.WriteLine($"Treap size: {treap.Size()}");
                        break;

                    case "6":
                        Console.WriteLine($"Treap height: {treap.Height()}");
                        break;

                    case "7":
                        Console.WriteLine($"Minimum gap between elements: {treap.MinGap()}");
                        break;

                    case "8":
                        running = false;
                        Console.WriteLine("Exiting...");
                        break;

                    default:
                        Console.WriteLine("Invalid option, please try again.");
                        break;
                }
            }
        }
    }
}