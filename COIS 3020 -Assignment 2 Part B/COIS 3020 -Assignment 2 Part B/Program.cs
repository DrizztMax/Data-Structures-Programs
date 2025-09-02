using System;
using System.Collections.Generic;
using System.IO;

namespace TrieEnhancements
{
    public class LazyTrie
    {
        private class Node
        {
            public string word;   // Full word stored at this node (if applicable)
            public Dictionary<char, Node> children; // Dynamically allocated children

            public Node()
            {
                word = null;
                children = new Dictionary<char, Node>();
            }
        }

        private Node root;

        public LazyTrie()
        {
            root = new Node();
        }

        // Insert method
        public void Insert(string key)
        {
            Node current = root;
            foreach (char c in key)
            {
                if (!current.children.ContainsKey(c))
                    current.children[c] = new Node();

                current = current.children[c];
            }
            current.word = key;  // Store the full word at the last node
        }

        // Value lookup
        public bool Contains(string key)
        {
            Node current = root;
            foreach (char c in key)
            {
                if (!current.children.ContainsKey(c))
                    return false;

                current = current.children[c];
            }
            return current.word != null;
        }

        // **1. Partial Match Search (Supports Wildcards '*')**
        public List<string> PartialMatch(string pattern)
        {
            List<string> result = new List<string>();
            PartialMatchHelper(root, pattern, 0, "", result);
            return result;
        }

        private void PartialMatchHelper(Node node, string pattern, int index, string currentWord, List<string> result)
        {
            if (node == null) return;

            if (index == pattern.Length)
            {
                if (node.word != null)
                    result.Add(currentWord);
                return;
            }

            char c = pattern[index];
            if (c == '*')
            {
                foreach (var entry in node.children)
                    PartialMatchHelper(entry.Value, pattern, index + 1, currentWord + entry.Key, result);
            }
            else if (node.children.ContainsKey(c))
            {
                PartialMatchHelper(node.children[c], pattern, index + 1, currentWord + c, result);
            }
        }

        // **2. Autocomplete (Prefix Search)**
        public List<string> Autocomplete(string prefix)
        {
            List<string> result = new List<string>();
            Node current = root;

            foreach (char c in prefix)
            {
                if (!current.children.ContainsKey(c))
                    return result;  // Prefix not found
                current = current.children[c];
            }

            CollectWords(current, prefix, result);
            return result;
        }

        private void CollectWords(Node node, string currentWord, List<string> result)
        {
            if (node == null) return;
            if (node.word != null) result.Add(currentWord);

            foreach (var entry in node.children)
                CollectWords(entry.Value, currentWord + entry.Key, result);
        }

        // **3. Autocorrect (Find words with one different letter)**
        public List<string> Autocorrect(string key)
        {
            List<string> result = new List<string>();
            AutocorrectHelper(root, key, 0, "", result, false);
            return result;
        }

        private void AutocorrectHelper(Node node, string key, int index, string currentWord, List<string> result, bool changed)
        {
            if (node == null) return;

            if (index == key.Length)
            {
                if (node.word != null && changed) // Only add if a change was made
                    result.Add(currentWord);
                return;
            }

            char c = key[index];

            // Exact match case
            if (node.children.ContainsKey(c))
                AutocorrectHelper(node.children[c], key, index + 1, currentWord + c, result, changed);

            // Allow a single-letter change
            if (!changed)
            {
                foreach (var entry in node.children)
                {
                    if (entry.Key != c) // Modify one letter
                        AutocorrectHelper(entry.Value, key, index + 1, currentWord + entry.Key, result, true);
                }
            }
        }

        // **4. InRange (Count words between two lexicographical bounds)**
        public int InRange(string lower, string upper)
        {
            List<string> result = new List<string>();
            CollectWordsInRange(root, "", lower, upper, result);
            return result.Count;
        }

        private void CollectWordsInRange(Node node, string currentWord, string lower, string upper, List<string> result)
        {
            if (node == null) return;
            if (node.word != null && node.word.CompareTo(lower) >= 0 && node.word.CompareTo(upper) <= 0)
                result.Add(currentWord);

            foreach (var entry in node.children)
                CollectWordsInRange(entry.Value, currentWord + entry.Key, lower, upper, result);
        }
    }

    class Program
    {
        static void Main()
        {
            LazyTrie trie = new LazyTrie();

            // **Populate with 1000 common English words**
            string[] words = File.ReadAllLines("common_words.txt"); // Assumes words are in a file
            foreach (string word in words)
                trie.Insert(word.ToLower());

            Console.WriteLine("Trie populated with 1000 words.\n");

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
                        Console.WriteLine("Matches: " + string.Join(", ", trie.PartialMatch(pattern)));
                        break;

                    case "2":
                        Console.Write("Enter prefix: ");
                        string prefix = Console.ReadLine();
                        Console.WriteLine("Autocomplete Suggestions: " + string.Join(", ", trie.Autocomplete(prefix)));
                        break;

                    case "3":
                        Console.Write("Enter word for autocorrect: ");
                        string incorrectWord = Console.ReadLine();
                        Console.WriteLine("Autocorrect Suggestions: " + string.Join(", ", trie.Autocorrect(incorrectWord)));
                        break;

                    case "4":
                        Console.Write("Enter lower bound word: ");
                        string lower = Console.ReadLine();
                        Console.Write("Enter upper bound word: ");
                        string upper = Console.ReadLine();
                        Console.WriteLine($"Number of words in range: {trie.InRange(lower, upper)}");
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
}
