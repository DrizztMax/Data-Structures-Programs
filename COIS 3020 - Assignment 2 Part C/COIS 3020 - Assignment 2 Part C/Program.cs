using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Based on http://igoro.com/archive/skip-lists-are-fascinating/

namespace SkipLists
{
    // Interface for SkipList
    interface ISkipList<T> where T : IComparable
    {
        void Insert(T item);     // Inserts item into the skip list (duplicates are permitted)
        bool Contains(T item);   // Returns true if item is found; false otherwise
        void Remove(T item);     // Removes one occurrence of item (if possible) from the skip list
    }

    // Class SkipList
    class SkipList<T> : ISkipList<T> where T : IComparable
    {
        private Node head;          // Header node of height 32
        private int maxHeight;     // Maximum height among non-header nodes
        private Random rand;       // For generating random heights

        // Class Node (used by SkipList)
        private class Node
        {
            public T Item { get; set; }
            public int Height { get; set; }
            public Node[] Next { get; set; }
            public int Size { get; set; } // The size of the sublist under this node

            // Constructor
            public Node(T item, int height)
            {
                Item = item;
                Height = height;
                Next = new Node[Height];
                Size = 1; // Each node starts with size 1, as it points to itself initially
            }
        }

        // Constructor
        public SkipList()
        {
            head = new Node(default(T), 32);  // Set to NIL by default
            maxHeight = 0;                    // Current maximum height of the skip list
            rand = new Random();
        }

        // Insert
        // Time Complexity: O(log n), where n is the number of elements in the skip list.
        // Method to insert an item into the skip list. It generates a random height for the node,
        // and inserts it in the appropriate positions across multiple levels.
        public void Insert(T item)
        {
            // Check if the item is already present in the list using Contains
            if (Contains(item))
                return; // Item already exists, do nothing

            // Randomly determine height of a new node
            int height = 0;
            int R = rand.Next();  // R is a random 32-bit positive integer
            while ((height < maxHeight) && ((R & 1) == 1))
            {
                height++;  // Increment height for each trailing 1 bit
                R >>= 1;   // Right shift one bit
            }
            if (height == maxHeight) maxHeight++;  // Increase max height if necessary

            // Create and insert the new node
            Node newNode = new Node(item, height + 1);
            Node cur = head;
            for (int i = maxHeight - 1; i >= 0; i--)  // Traverse each level
            {
                while (cur.Next[i] != null && cur.Next[i].Item.CompareTo(item) < 0)
                    cur = cur.Next[i];

                // Insert the new node at level i if its height is >= i
                if (i < newNode.Height)
                {
                    newNode.Next[i] = cur.Next[i];
                    cur.Next[i] = newNode;

                    // Update the size of the sublist
                    newNode.Size = cur.Size - (cur.Next[i] == null ? 0 : cur.Next[i].Size);
                }
            }
        }

        // Contains
        // Time Complexity: O(log n), where n is the number of elements in the skip list.
        // The contains method traverses the list from top to bottom, checking each level. 
        // Method to check if an item exists in the skip list.
        public bool Contains(T item)
        {
            Node cur = head;
            for (int i = maxHeight - 1; i >= 0; i--)  // for each level
            {
                while (cur.Next[i] != null && cur.Next[i].Item.CompareTo(item) < 0)
                    cur = cur.Next[i];

                if (cur.Next[i] != null && cur.Next[i].Item.CompareTo(item) == 0)
                    return true;
            }
            return false;
        }

        // Remove
        // Time Complexity: O(log n), where n is the number of elements in the skip list.
        // Method to remove an item from the skip list if it exists.
        public void Remove(T item)
        {
            // Check if the item exists before attempting to remove
            if (!Contains(item))
                return;  // Item does not exist, do nothing

            Node cur = head;
            for (int i = maxHeight - 1; i >= 0; i--)  // Traverse each level
            {
                while (cur.Next[i] != null && cur.Next[i].Item.CompareTo(item) < 0)
                    cur = cur.Next[i];

                // If the item is found at this level, remove it
                if (cur.Next[i] != null && cur.Next[i].Item.CompareTo(item) == 0)
                {
                    cur.Next[i] = cur.Next[i].Next[i];  // Remove reference at level i
                    cur.Size -= cur.Next[i] == null ? 0 : cur.Next[i].Size;  // Adjust the size of the sublist
                }
            }

            // Decrease maxHeight if the highest level has no nodes
            while (maxHeight > 0 && head.Next[maxHeight - 1] == null)
                maxHeight--;
        }

        // Rank(T item)
        // Time Complexity: O(log n), where n is the number of elements in the skip list.
        // The rank method traverses the list from top to bottom, accumulating the rank for each level.
        // Method to calculate the rank of an item in the skip list, i.e., the number of items smaller than it.
        public int Rank(T item)
        {
            int rank = 0;
            Node cur = head;
            for (int i = maxHeight - 1; i >= 0; i--)  // Traverse each level
            {
                while (cur.Next[i] != null && cur.Next[i].Item.CompareTo(item) < 0)
                {
                    rank += cur.Next[i].Size;  // Add the size of the sublist below the current node
                    cur = cur.Next[i];
                }

                if (cur.Next[i] != null && cur.Next[i].Item.CompareTo(item) == 0)
                {
                    rank += cur.Size - cur.Next[i].Size;  // Add the position in the current level
                    break;
                }
            }
            return rank;
        }

        // Rank(int i)
        // Time Complexity: O(log n), where n is the number of elements in the skip list.
        // The rank method that takes an index uses a similar traversal as the other rank method, but instead of 
        // calculating the rank of an element, it finds the element at a given rank.
        public T Rank(int i)
        {
            // Start from the head node (header node)
            Node cur = head;
            int currentRank = 0;  // This will track the rank as we traverse the list

            // Traverse the skip list by moving down from level maxHeight to level 0
            while (cur != null)
            {
                // Move along the next node at the current level
                while (cur.Next[0] != null && currentRank < i)
                {
                    currentRank++;  // Increment rank as we move to the next node
                    cur = cur.Next[0];  // Move to the next node at level 0
                }

                // If we've reached the desired rank, return the item
                if (currentRank == i && cur != null)
                {
                    return cur.Item;  // Return the item at the current node
                }

                // Move down to the next level
                cur = cur.Next[0];  // Proceed to the next level
            }

            // If we reached the end of the list or the rank is out of bounds, return default
            return default(T);
        }

        // Print
        // Time Complexity: O(n), where n is the number of elements in the skip list.
        // Method to print all the elements in the skip list at level 0.
        public void Print()
        {
            Node cur = head.Next[0];
            while (cur != null)
            {
                Console.Write(cur.Item + " ");
                cur = cur.Next[0];
            }
            Console.WriteLine();
        }

        // Profile
        // Time Complexity: O(n), where n is the number of elements in the skip list.
        // The profile method traverses all elements at level 0, printing a string of '*' for each node
        // to represent the height of the node.
        // Method to print the profile of the skip list showing the height of each node.
        public void Profile()
        {
            Node cur = head;
            while (cur != null)
            {
                Console.WriteLine(new string('*', cur.Height));  // Outputs a string of *s
                cur = cur.Next[0];
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            SkipList<int> S = new SkipList<int>();

            Console.WriteLine("Enter numbers to insert into the SkipList (comma-separated):");
            string input = Console.ReadLine();
            string[] numbers = input.Split(',');

            foreach (string num in numbers)
            {
                if (int.TryParse(num.Trim(), out int value))
                {
                    S.Insert(value);
                }
                else
                {
                    Console.WriteLine($"Invalid input ignored: {num}");
                }
            }

            while (true)
            {
                Console.WriteLine("Enter a rank to get its number (or type 'exit' to quit):");
                string rankInput = Console.ReadLine();

                if (rankInput.ToLower() == "exit")
                    break;

                if (int.TryParse(rankInput, out int rank))
                {
                    Console.WriteLine($"Rank {rank}: {S.Rank(rank)}");
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }
            }

            Console.WriteLine("Exiting program...");
        }
    }
}