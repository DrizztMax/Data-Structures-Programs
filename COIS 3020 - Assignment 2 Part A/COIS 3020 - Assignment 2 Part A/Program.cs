using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrieRWayTree
{

    public interface IContainer<T>
    {
        void MakeEmpty();
        bool Empty();
        int Size();
    }

    //-------------------------------------------------------------------------

    public interface ITrie<T> : IContainer<T>
    {
        bool Insert(string key, T value);
        bool Remove(string key);
        T Value(string key);
    }

    //-------------------------------------------------------------------------

    class Trie<T> : ITrie<T>
    {
        private Node root;          // Root node of the Trie

        private class Node
        {
            public T value;         // Value associated with a key; otherwise default
            public int numValues;   // Number of descendent values of a Node 
            public Node[] child;    // Branch for each letter 'a' .. 'z'

            // Node
            // Creates an empty Node
            // All children are set to null by default
            // Time complexity:  O(1)

            public Node()
            {
                value = default(T);
                numValues = 0;
                child = new Node[26];
            }
        }

        // Trie
        // Creates an empty Trie
        // Time complexity:  O(1)

        public Trie()
        {
            MakeEmpty();
        }

        // Public Insert
        // Calls the private Insert which carries out the actual insertion
        // Returns true if successful; false otherwise

        public bool Insert(string key, T value)
        {
            return Insert(root, key, 0, value);
        }

        // Private Insert
        // Inserts the key/value pair into the Trie
        // Returns true if the insertion was successful; false otherwise
        // Note: Duplicate keys are ignored
        // Time complexity:  O(L) where L is the length of the key

        private bool Insert(Node p, string key, int j, T value)
        {
            int i;

            if (j == key.Length)
            {
                if (EqualityComparer<T>.Default.Equals(p.value, default(T)))
                {
                    p.value = value;
                    p.numValues++;
                    return true;
                }
                return false; // On unsuccessful insertion
            }
            else
            {
                i = Char.ToLower(key[j]) - 'a';

                if (p.child == null)
                    p.child = new Node[26]; // Lazy initialization of child nodes

                if (p.child[i] == null)
                    p.child[i] = new Node();

                if (Insert(p.child[i], key, j + 1, value))
                {
                    p.numValues++;
                    return true;
                }
                else
                    return false;
            }
        }

        // Value
        // Returns the value associated with a key; otherwise default
        // Time complexity:  O(min(L,M)) where L is the length of the given key and
        // M is the maximum length of a key in the trie

        // Public Value Method
        public T Value(string key)
        {
            return Value(root, key, 0);
        }

        // Private Value Method
        private T Value(Node p, string key, int j)
        {
            int i;

            if (p == null)
                return default(T); // Key not found

            if (j == key.Length)
                return p.value; // Return the value at the end of the key

            i = Char.ToLower(key[j]) - 'a';

            if (p.child == null || p.child[i] == null)
                return default(T); // Key not found

            return Value(p.child[i], key, j + 1);
        }

        // Public Remove
        // Calls the private Remove that carries out the actual deletion
        // Returns true if successful; false otherwise

        public bool Remove(string key)
        {
            return Remove(root, key, 0);
        }

        // Private Remove
        // Removes the value associated with the given key
        // Time complexity:  O(min(L,M)) where L is the length of the key
        // where M is the maximum length of a key in the trie

        private bool Remove(Node p, string key, int j)
        {
            int i;

            if (p == null)
                return false; // Key not found

            if (j == key.Length)
            {
                if (!EqualityComparer<T>.Default.Equals(p.value, default(T)))
                {
                    p.value = default(T);
                    p.numValues--;
                    return true; // Successfully removed
                }
                return false; // Key doesn't exist to remove
            }
            else
            {
                i = Char.ToLower(key[j]) - 'a';

                if (p.child == null || p.child[i] == null)
                    return false; // Key not found

                if (Remove(p.child[i], key, j + 1))
                {
                    if (IsLeafNode(p.child[i]))
                        p.child[i] = null; // Remove the child node if it's a leaf
                    p.numValues--;
                    return true;
                }
                return false;
            }
        }

        // Additional method to determine if a node is a leaf node
        private bool IsLeafNode(Node p)
        {
            return p != null && p.child == null;
        }

        // MakeEmpty
        // Creates an empty Trie
        // Time complexity:  O(1)

        public void MakeEmpty()
        {
            root = new Node();
        }

        // Empty
        // Returns true if the Trie is empty; false otherwise
        // Time complexity:  O(1)

        public bool Empty()
        {
            return root.numValues == 0;
        }

        // Size
        // Returns the number of Trie values
        // Time complexity:  O(1)

        public int Size()
        {
            return root.numValues;
        }

        // Public Print
        // Calls private Print to carry out the actual printing

        public void Print()
        {
            Print(root, "");
        }

        // Private Print
        // Outputs the key/value pairs ordered by keys
        // Time complexity:  O(n) where n is the number of nodes in the trie

        private void Print(Node p, string key)
        {
            int i;

            if (p != null)
            {
                if (!p.value.Equals(default(T)))
                    Console.WriteLine(key + " " + p.value);
                for (i = 0; i < 26; i++)
                    Print(p.child[i], key + (char)(i + 'a'));
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Trie<int> T = new Trie<int>();
            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("\n---- Trie Operations Menu ----");
                Console.WriteLine("1. Insert a word");
                Console.WriteLine("2. Remove a word");
                Console.WriteLine("3. Print Trie");
                Console.WriteLine("4. Exit");
                Console.Write("Choose an option (1-4): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter word to insert: ");
                        string word = Console.ReadLine();
                        Console.Write("Enter value (integer) for the word: ");
                        if (int.TryParse(Console.ReadLine(), out int value))
                        {
                            T.Insert(word, value);
                            Console.WriteLine($"Inserted: '{word}' with value {value}.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid value! Please enter an integer.");
                        }
                        break;

                    case "2":
                        Console.Write("Enter word to remove: ");
                        string removeKey = Console.ReadLine();
                        T.Remove(removeKey);
                        Console.WriteLine($"Attempted to remove: '{removeKey}'");
                        break;

                    case "3":
                        Console.WriteLine("\nTrie Contents:");
                        T.Print();
                        break;

                    case "4":
                        exit = true;
                        Console.WriteLine("Exiting program...");
                        break;

                    default:
                        Console.WriteLine("Invalid option! Please choose 1-4.");
                        break;
                }
            }
        }
    }
}
