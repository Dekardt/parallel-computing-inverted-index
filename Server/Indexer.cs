using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Server
{
    class Indexer
    {
        // file with words that will not be included in inverted index, as articles or pronouns
        private static string[] ExcludedWords = File.ReadAllText("..//..//..//stop_words.txt").Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // amount of threads used for building index
        const int ThreadsAmount = 1;

        // datasets
        FileInfo[] filesList;

        private ConcurrentDictionary<string, List<string>> invertedIndex;


        public Indexer(string dirPath)
        {
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            this.filesList = dir.GetFiles();
        }


        // pre
        private static string[] LexemsPreproccessing(string sourceFileText)
        {
            string fileText = Regex.Replace(sourceFileText, "< *br */ *>", "");
            fileText = Regex.Replace(fileText, "[0-9]+", "");

            string[] lexemLists = Regex.Matches(fileText, @"\w+").Cast<Match>().Select(m => m.Value).Distinct().ToArray();

            return lexemLists;
        }


        public void CreateInvertIndex(int startFile, int endFile)
        {
            for (int i = startFile; i < endFile; i++)
            {
                Console.WriteLine(filesList[i].FullName);
            }
        }

    }
}
