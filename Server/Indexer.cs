using System;
using System.Collections.Concurrent;
using System.IO;
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
        private const int ThreadsAmount = 2;

        // datasets
        FileInfo[] filesList;

        public ConcurrentDictionary<string, List<string>> invertedIndex = new ConcurrentDictionary<string, List<string>>();


        public Indexer(string dirPath)
        {
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            this.filesList = dir.GetFiles();
        }


        private static string[] LexemsPreprocessing(string sourceFileText)
        {
            string fileText = Regex.Replace(sourceFileText, "< *br */ *>", "");
            fileText = Regex.Replace(fileText, "[0-9]+", "");

            string[] lexemLists = Regex.Matches(fileText, @"\w+").Cast<Match>().Select(m => m.Value).Distinct().ToArray();

            return lexemLists;
        }


        private void ParseBlock(int startFileIndex, int endFileIndex)
        {
            for(int i=startFileIndex; i<endFileIndex; i++)
            {
                FileInfo curFile = this.filesList[i];
                string[] lexemLists = LexemsPreprocessing(File.ReadAllText(curFile.FullName));

                foreach(string lexem in lexemLists)
                {
                    if (!ExcludedWords.Contains(lexem))
                    {
                        this.invertedIndex.AddOrUpdate(lexem, new List<string> { curFile.Name }, (k, v) => v.Append(curFile.Name).ToList());
                    }
                }
            }
        }


        public void CreateIndex()
        {
            if(ThreadsAmount == 1)
            {
                ParseBlock(0, this.filesList.Length);
            }
            else
            {
                float filePerThread = this.filesList.Length / (float)ThreadsAmount;

                Task[] tasks = new Task[ThreadsAmount];

                for(int iter=0; iter<ThreadsAmount; iter++)
                {
                    int startFile = (int)(filePerThread * iter);
                    int endFile = (int)(filePerThread * (iter + 1));
                    tasks[iter] = Task.Run(() => ParseBlock(startFile, endFile));
                }
                Task.WaitAll(tasks);
            }
        }
    }
}
