using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPT_Lab2_233985
{
    class Trie
    {
        private static readonly int ALPHABET_SIZE = char.MaxValue;
        private static TrieNode root;

        public Trie()
        {
            root = new TrieNode();
        }

        public Trie(string[] words)
        {
            root = new TrieNode();
            foreach (string word in words)
                Insert(word);
        }

        private class TrieNode
        {
            private TrieNode[] children = new TrieNode[ALPHABET_SIZE];
            private bool isLeaf = false;

            public bool IsWordEnd { get => isLeaf; set => isLeaf = value; }
            public TrieNode[] Children { get => children; set => children = value; }
        }

        public void Insert(String key)
        {
            //int index;
            TrieNode temp = root;
            for(int i = 0; i < key.Length; i++)
            {
                if (temp.Children[key[i]] == null)
                    temp.Children[key[i]] = new TrieNode();
                temp = temp.Children[key[i]];
            }
            temp.IsWordEnd = true;
        }

        public bool Find(String key)
        {
            int index;
            TrieNode temp = root;
            for (int i = 0; i < key.Length; i++)
            {
                index = (int)key[i];
                if (temp.Children[index] == null)
                    return false;
                temp = temp.Children[index];
            }

            return (temp != null && temp.IsWordEnd);
        }

        public bool Find(ReadOnlySpan<char> key)
        {
            int index;
            TrieNode temp = root;
            for (int i = 0; i < key.Length; i++)
            {
                index = (int)key[i];
                if (temp.Children[index] == null)
                    return false;
                temp = temp.Children[index];
            }

            return (temp != null && temp.IsWordEnd);
        }
    }
}
