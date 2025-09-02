using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrieRWayTree;

namespace TrieRWayTree
{
    public interface IContainer<T>
    {
        void MakeEmpty();
        bool Empty();
        int Size();
    }

    public interface ITrie<T> : IContainer<T>
    {
        bool Insert(string key, T value);
        bool Remove(string key);
        T Value(string key);
        List<string> PartialMatch(string pattern);
        List<string> Autocomplete(string prefix);
        List<string> Autocorrect(string key);
        int InRange(string lower, string upper);
    }

    class Trie<T> : ITrie<T>
    {
        private Node root;

        private class Node
        {
            public T value;
            public int numValues;
            public Node[] child;

            public Node()
            {
                value = default(T);
                numValues = 0;
                child = new Node[26];
            }
        }

        public Trie()
        {
            MakeEmpty();
        }

        //public insert method
        public bool Insert(string key, T value)
        {
            return Insert(root, key, 0, value);
        }

        //private insert method
        //inserts a key-value pair into the trie.
        //If the key already exists in the trie, the insertion is ignored.
        //The method traverses the trie according to each character in the key and creates new nodes as necessary.
        //Time Complexity: O(L) where L is the lenght of the key.
        private bool Insert(Node p, string key, int j, T value)
        {
            int i;

            if (j == key.Length)
            {
                if (p.value.Equals(default(T)))
                {
                    p.value = value;
                    p.numValues++;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                i = Char.ToLower(key[j]) - 'a';

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

        //searches for the value associated with the given key in the trie.
        //Time complexity: O(L) where L is the lenght of the key
        public T Value(string key)
        {
            int i;
            Node p = root;

            foreach (char ch in key)
            {
                i = Char.ToLower(ch) - 'a';
                if (p.child[i] == null)
                    return default(T);
                else
                    p = p.child[i];
            }
            return p.value;
        }

        //public remove method
        public bool Remove(string key)
        {
            return Remove(root, key, 0);
        }

        //private remove method
        //removes the key-value pair associated with the given key from the trie.
        //deletes child nodes if they are no longer needed.
        //Time Complexity: O(L) where L is the length of the key
        private bool Remove(Node p, string key, int j)
        {
            int i;

            if (p == null)
                return false;

            else if (j == key.Length)
            {
                if (!p.value.Equals(default(T)))
                {
                    p.value = default(T);
                    p.numValues--;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                i = Char.ToLower(key[j]) - 'a';

                if (Remove(p.child[i], key, j + 1))
                {
                    if (p.child[i].numValues == 0)
                        p.child[i] = null;
                    p.numValues--;
                    return true;
                }
                else
                    return false;
            }
        }

        //resets the trie by creating a new root node.
        //Time Complecity: O(1)
        public void MakeEmpty()
        {
            root = new Node();
        }

        //checks whether the trie is empty.
        //Time Complexity: O(1)
        public bool Empty()
        {
            return root.numValues == 0;
        }

        //returns the number of values stored in the trie.
        //Time Complexity: O(1)
        public int Size()
        {
            return root.numValues;
        }

        //public print method.
        public void Print()
        {
            Print(root, "");
        }

        //private print method.
        //recursively prints all the key-value pairs in the trie.
        //traverses the trie in lexicographical order and prints the key along with the associated value.
        //TIme Complexity: O(n) where n is the total number of nodes in the trie.
        private void Print(Node p, string key)
        {
            int i;

            if (p != null)
            {
                if (!p.value.Equals(default(T)))
                    Console.WriteLine(key + " " + p.value + " " + p.numValues);
                for (i = 0; i < 26; i++)
                    Print(p.child[i], key + (char)(i + 'a'));
            }
        }

        //public PartialMatch method
        public List<string> PartialMatch(string pattern)
        {
            List<string> result = new List<string>();
            PartialMatch(root, pattern, 0, "", result);
            return result;
        }

        //private PartialMatch method
        //finds all keys that match the given pattern.
        //where * is used as a wildcard to match any character.
        //Time Complexity: O(26^L) where L is the lenght of the pattern.
        private void PartialMatch(Node p, string pattern, int index, string currentKey, List<string> result)
        {
            if (p == null) return;

            if (index == pattern.Length)
            {
                if (!p.value.Equals(default(T)))
                    result.Add(currentKey);
                return;
            }

            char ch = pattern[index];
            if (ch == '*') // Wildcard, try all children
            {
                for (int i = 0; i < 26; i++)
                {
                    PartialMatch(p.child[i], pattern, index + 1, currentKey + (char)(i + 'a'), result);
                }
            }
            else // Exact match
            {
                int i = Char.ToLower(ch) - 'a';
                PartialMatch(p.child[i], pattern, index + 1, currentKey + ch, result);
            }
        }

        //public Autocomplete method
        //Traverses the trie for a prefix.
        //Then recursively searches for matching words.
        //Time Complexity: O(L + n)
        public List<string> Autocomplete(string prefix)
        {
            List<string> result = new List<string>();
            Node p = TraverseToPrefix(root, prefix);
            if (p != null)
                Autocomplete2(p, prefix, result);
            return result;
        }

        //private method used by autocomplete to traverse the trie for a prefix.
        //Time Complexity: O(L) where L is the lenght of the prefix.
        private Node TraverseToPrefix(Node p, string prefix)
        {
            foreach (char ch in prefix)
            {
                int i = Char.ToLower(ch) - 'a';
                if (p.child[i] == null) return null;
                p = p.child[i];
            }
            return p;
        }

        //private autocomplete method
        //recursively searches for matching words given a prefix.
        //Time Complexity: O(n) where n is the number of keys that start with the prefix.
        private void Autocomplete2(Node p, string prefix, List<string> result)
        {
            if (p != null)
            {
                if (!p.value.Equals(default(T)))
                    result.Add(prefix);
                for (int i = 0; i < 26; i++)
                    Autocomplete2(p.child[i], prefix + (char)(i + 'a'), result);
            }
        }

        //public autocorrect method
        public List<string> Autocorrect(string key)
        {
            List<string> result = new List<string>();
            Autocorrect(root, key, 0, "", result);
            return result;
        }

        //private autocorrect method
        //suggests words that are close to the given key,
        //allowing one character correction per position.
        //returns valid words that differ from the input key by a single character.
        //Time Complexity: O(26^L) where L is the length of the key.
        private void Autocorrect(Node p, string key, int index, string currentKey, List<string> result)
        {
            if (p == null || index > key.Length + 1) return;

            if (index == key.Length)
            {
                if (!p.value.Equals(default(T)) && !currentKey.Equals(key))
                    result.Add(currentKey);
                return;
            }

            for (int i = 0; i < 26; i++)
            {
                char ch = (char)(i + 'a');
                if (ch == key[index]) // Correct character
                {
                    Autocorrect(p.child[i], key, index + 1, currentKey + ch, result);
                }
                else if (index + 1 == key.Length) // Allow one edit at the end
                {
                    Autocorrect(p.child[i], key, index + 1, currentKey + ch, result);
                }
            }
        }

        //public InRange method
        public int InRange(string lower, string upper)
        {
            int count = 0;
            InRange(root, lower, upper, "", ref count);
            return count;
        }

        //private InRange method
        //counts how many keys in the trie are lexicographically between the given upper and lower bounds.
        //Time Complexity 
        private void InRange(Node p, string lower, string upper, string currentKey, ref int count)
        {
            if (p == null) return;

            if (string.Compare(currentKey, lower) >= 0 && string.Compare(currentKey, upper) <= 0)
            {
                if (!p.value.Equals(default(T))) count++;
            }

            for (int i = 0; i < 26; i++)
            {
                InRange(p.child[i], lower, upper, currentKey + (char)(i + 'a'), ref count);
            }
        }
    }
}

class Program
    {
        static void Main()
        {
            Trie<int> trie = new Trie<int>();

            // **Populate with 1000 common English words**
            try
            {
                string[] words = File.ReadAllLines("common_words.txt"); // Assumes words are in a file
                int value = 10;
                foreach (string word in words)
                {
                    trie.Insert(word.ToLower(), value);
                    value += 10;
                }
                Console.WriteLine("Trie populated with 1000 words.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading the file: " + ex.Message);
                return;
            }

            while (true)
            {
                Console.WriteLine("\nChoose an operation:");
                Console.WriteLine("1. Partial Match Search");
                Console.WriteLine("2. Autocomplete");
                Console.WriteLine("3. Autocorrect");
                Console.WriteLine("4. InRange Count");
                Console.WriteLine("5. Exit");
                Console.Write("Enter choice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter pattern with '*': ");
                        string pattern = Console.ReadLine();
                        List<string> matches = trie.PartialMatch(pattern);
                        Console.WriteLine("Matches: " + string.Join(", ", matches));
                        break;

                    case "2":
                        Console.Write("Enter prefix: ");
                        string prefix = Console.ReadLine();
                        List<string> autocompleteResults = trie.Autocomplete(prefix);
                        Console.WriteLine("Autocomplete Suggestions: " + string.Join(", ", autocompleteResults));
                        break;

                    case "3":
                        Console.Write("Enter word for autocorrect: ");
                        string incorrectWord = Console.ReadLine();
                        List<string> autocorrectResults = trie.Autocorrect(incorrectWord);
                        Console.WriteLine("Autocorrect Suggestions: " + string.Join(", ", autocorrectResults));
                        break;

                    case "4":
                        Console.Write("Enter lower bound word: ");
                        string lower = Console.ReadLine();
                        Console.Write("Enter upper bound word: ");
                        string upper = Console.ReadLine();
                        int count = trie.InRange(lower, upper);
                        Console.WriteLine($"Number of words in range: {count}");
                        break;

                    case "5":
                        return;

                    default:
                        Console.WriteLine("Invalid choice! Try again.");
                        break;
                }
            }
        }
    }
