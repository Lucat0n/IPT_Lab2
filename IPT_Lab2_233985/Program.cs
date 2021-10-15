using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IPT_Lab2_233985
{
    class Program
    {
        static void Main(string[] args)
        {
            /**
             * arg0 - plik ze wzorcami
             * arg1 - plik wejściowy
             * arg2 - plik wyjściowy (wyniki)
             * arg3 - liczba iteracji
             */

            int itercount;

            if (args.Length != 4)
                Console.WriteLine("Invalid args count.");
            else if (!File.Exists(args[0]) || !File.Exists(args[1]))
                Console.WriteLine("One of input files path is invalid.");
            else if (!Int32.TryParse(args[3], out itercount))
                Console.WriteLine("Invalid index.");
            else
            {
                string[] patterns = File.ReadAllLines(args[0]);
                string text = File.ReadAllText(args[1]);
                //TrieMatch(text, patterns);
                TestMultiplePatterns(args, itercount);
            }
            Console.WriteLine("Done. Press any key to continue...");
            Console.ReadLine();
        }

        #region algs
        static void KMPMatch(string text, string pattern, bool logging = false)
        {
            int[] T = GenerateKMPTable(pattern);
            int i = 0;
            int j = 0;

            while (i < text.Length)
            {
                if (pattern[j] == text[i])
                {
                    i++;
                    j++;
                }
                if (j == pattern.Length)
                {
                    if (logging)
                        Console.WriteLine("Pattern found in " + i);
                    j = T[j - 1];
                }
                else if (i < text.Length && pattern[j] != text[i])
                {
                    if (j > 0)
                        j = T[j - 1];
                    else
                        i++;
                }
            }
        }

        static void NaiveMatch(string text, string pattern, bool logging = false)
        {
            int n = text.Length;
            int m = pattern.Length;
            for (int s = 0; s <= n - m; s++)
            {
                if (CompareStrings(text, s, pattern) && logging)
                    Console.WriteLine("Pattern found in " + (s + m));
            }
        }

        static void RabinKarpMatch(string text, string pattern, bool logging = false)
        {
            ulong Q = 100007;
            ulong D = 256;

            ulong tpattern = ComputeHash(text, 0, pattern.Length);
            ulong hpattern = ComputeHash(pattern, 0, pattern.Length);

            if (tpattern == hpattern && logging)
                Console.WriteLine("Pattern found in 0");

            ulong pow = 1;

            for (int k = 1; k <= pattern.Length - 1; ++k)
                pow = (pow * D) % Q;

            for (int j = 1; j <= text.Length - pattern.Length; ++j)
            {
                tpattern = (tpattern + Q - pow * (ulong)text[j - 1] % Q) % Q;
                tpattern = (tpattern * D + (ulong)text[j + pattern.Length - 1]) % Q;

                if (tpattern == hpattern)
                    if (CompareStrings(text, j, pattern) && logging)
                    {
                        Console.WriteLine(text.Substring(j, pattern.Length) + " " + pattern);
                        Console.WriteLine("Pattern found in " + j);
                    }
            }
        }

        static void TrieMatch(string text, string[] patterns, bool logging = false)
        {

            Trie trie = new Trie(patterns);
            int[] len = new int[patterns.Length];
            for (int i = 0; i < patterns.Length; i++)
                len[i] = patterns[i].Length;

            for (int i = 0; i < text.Length - patterns[0].Length; i++)
            {
                foreach (int length in len)
                {
                    if (i + length > text.Length)
                        continue;
                    if (trie.Find(text.AsSpan().Slice(start: i, length: length)) && logging)
                        Console.WriteLine("Pattern found at: " + i);
                }
            }
        }

        #endregion
        #region utilities

        private static bool CompareStrings(string str, int index, string pattern)
        {
            for (int i = index; i < index + pattern.Length; i++)
                if (str[i] != pattern[i - index])
                    return false;
            return true;
        }

        static ulong ComputeHash(string text, int n, int m)
        {
            const ulong p = 100007;
            ulong hash = 0;
            ulong D = 256;
            for (int i = n; i < m + n; i++)
            {
                hash = (hash * D + (ulong)text[i]) % p;
            }
            //Console.WriteLine("COMP: " + (m));
            return hash;
        }

        static int[] GenerateKMPTable(string pattern)
        {
            int[] T = new int[pattern.Length];
            int i = 1;
            int j = 0;

            while (i < pattern.Length)
            {
                if (pattern[i] == pattern[j])
                {
                    j++;
                    T[i] = j;
                    i++;
                }
                else
                {
                    if (j > 0)
                        j = T[j - 1];
                    else
                    {
                        T[i] = j;
                        i++;
                    }
                }
            }

            return T;
        }

        private static void TestMultiplePatterns(String[] args, int itercount)
        {
            string[] size_ext = new string[] { ".1MB", ".2MB", ".3MB", ".4MB", ".5MB", ".10MB" };
            string[] m = new string[] { "8", "16", "32", "64" };
            Stopwatch stopwatch = new Stopwatch();
            StreamWriter streamWriter_Naive = new StreamWriter("Output_Naive.csv");
            StreamWriter streamWriter_KMP = new StreamWriter("Output_KMP.csv");
            StreamWriter streamWriter_RK = new StreamWriter("Output_RK.csv");
            streamWriter_Naive.WriteLine("m/mode;file_size;time(ms)");
            streamWriter_KMP.WriteLine("m/mode;file_size;time(ms)");
            streamWriter_RK.WriteLine("m/mode;file_size;time(ms)");
            foreach (string ext in size_ext)
            {
                String text = File.ReadAllText(System.IO.Path.ChangeExtension(args[1], ext));
                foreach (string m_size in m)
                {

                    String patt = File.ReadAllText(System.IO.Path.ChangeExtension(args[1], null) + "_patt_" + ext.Substring(1) + "_" + m_size);
                    int pattern_length = Int32.Parse(m_size);
                    int pattern_count = patt.Length / pattern_length;
                    String[] patterns = new string[pattern_count];
                    for (int i = 0; i < pattern_count; i++)
                        patterns[i] = patt.Substring(i * pattern_length, pattern_length);
                    //FileStream fs = File.OpenWrite(args[2]);
                    //StreamWriter sw = new StreamWriter(fs);
                    long total = 0;
                    Console.WriteLine("Naive");
                    for (int i = 0; i < itercount; i++)
                    {
                        stopwatch.Start();
                        int j = 0;
                        foreach (string pattern in patterns)
                            NaiveMatch(text, pattern);
                        stopwatch.Stop();
                        total += stopwatch.ElapsedMilliseconds;
                        stopwatch.Reset();
                    }
                    streamWriter_Naive.WriteLine(m_size + ";" + ext.Substring(1) + ";" + (total / itercount));
                    streamWriter_Naive.Flush();
                    total = 0;
                    Console.WriteLine("KMP");
                    for (int i = 0; i < itercount; i++)
                    {
                        stopwatch.Start();
                        foreach (string pattern in patterns)
                            KMPMatch(text, pattern);
                        stopwatch.Stop();
                        total += stopwatch.ElapsedMilliseconds;
                        stopwatch.Reset();
                    }
                    streamWriter_KMP.WriteLine(m_size + ";" + ext.Substring(1) + ";" + (total / itercount));
                    streamWriter_KMP.Flush();
                    total = 0;
                    Console.WriteLine("Rabin-Karp");
                    for (int i = 0; i < itercount; i++)
                    {
                        stopwatch.Start();
                        foreach (string pattern in patterns)
                            RabinKarpMatch(text, pattern);
                        stopwatch.Stop();
                        total += stopwatch.ElapsedMilliseconds;
                        stopwatch.Reset();
                    }
                    streamWriter_RK.WriteLine(m_size + ";" + ext.Substring(1) + ";" + (total / itercount));
                    streamWriter_RK.Flush();
                    Console.WriteLine("Done");
                }
            }
            streamWriter_Naive.Close();
            streamWriter_KMP.Close();
            streamWriter_RK.Close();
        }


        /*private static void TestMultiplePatterns(String[] args, int itercount)
        {
            Stopwatch stopwatch = new Stopwatch();
            string[] patterns = File.ReadAllLines(args[0]);
            string text = File.ReadAllText(args[1]);
            FileStream fs = File.OpenWrite(args[2]);
            StreamWriter sw = new StreamWriter(fs);
            long total = 0;
            Console.WriteLine("Naive");
            for (int i = 0; i < itercount; i++)
            {
                stopwatch.Start();
                int j = 0;
                foreach (string pattern in patterns)
                    NaiveMatch(text, pattern);
                stopwatch.Stop();
                total += stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
            }
            sw.WriteLine("naive," + (total / itercount));
            sw.Flush();
            total = 0;
            Console.WriteLine("KMP");
            for (int i = 0; i < itercount; i++)
            {
                stopwatch.Start();
                foreach (string pattern in patterns)
                    KMPMatch(text, pattern);
                stopwatch.Stop();
                total += stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
            }
            sw.WriteLine("kmp," + (total / itercount));
            sw.Flush();
            total = 0;
            Console.WriteLine("Rabin-Karp");
            for (int i = 0; i < itercount; i++)
            {
                stopwatch.Start();
                foreach (string pattern in patterns)
                    RabinKarpMatch(text, pattern);
                stopwatch.Stop();
                total += stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
            }
            sw.WriteLine("rabinkarp," + (total / itercount));
            sw.Flush();
            total = 0;
            Console.WriteLine("Trie");
            for (int i = 0; i < itercount; i++)
            {
                stopwatch.Start();
                TrieMatch(text, patterns);
                stopwatch.Stop();
                total += stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
            }
            sw.WriteLine("trie," + (total / itercount));
            sw.Flush();
            Console.WriteLine("Done");
            sw.Close();
        }*/


        private static void TestSinglePatterns(String[] args, int itercount)
        {
            Stopwatch stopwatch = new Stopwatch();
            string text = File.ReadAllText(args[1]);
            FileStream fs = File.OpenWrite(args[2]);
            StreamWriter sw = new StreamWriter(fs);
            long total = 0;
            foreach (string pattern in File.ReadAllLines(args[0]))
            {
                Console.WriteLine("Naive " + pattern.Length);
                for (int i = 0; i < itercount; i++)
                {
                    stopwatch.Start();
                    NaiveMatch(text, pattern);
                    stopwatch.Stop();
                    total += stopwatch.ElapsedMilliseconds;
                    stopwatch.Reset();
                }
                sw.WriteLine("naive," + pattern.Length + "," + (total / itercount));
                sw.Flush();
                total = 0;
                Console.WriteLine("KMP " + pattern.Length);
                for (int i = 0; i < itercount; i++)
                {
                    stopwatch.Start();
                    KMPMatch(text, pattern);
                    stopwatch.Stop();
                    total += stopwatch.ElapsedMilliseconds;
                    stopwatch.Reset();
                }
                sw.WriteLine("kmp," + pattern.Length + "," + (total / itercount));
                sw.Flush();
                total = 0;
                Console.WriteLine("Rabin-Karp " + pattern.Length);
                for (int i = 0; i < itercount; i++)
                {
                    stopwatch.Start();
                    RabinKarpMatch(text, pattern, true);
                    stopwatch.Stop();
                    total += stopwatch.ElapsedMilliseconds;
                    stopwatch.Reset();
                }
                sw.WriteLine("rabinkarp," + pattern.Length + "," + (total / itercount));
                sw.Flush();
                Console.WriteLine("Done with " + pattern.Length + " chars long pattern");
            }
            sw.Close();
        }
        #endregion
    }
}
