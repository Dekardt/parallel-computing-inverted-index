using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;

namespace Server
{
    class Indexer
    {
        // file with words that will not be included in inverted index, as articles or pronouns
        private static string[] ExcludedWords = File.ReadAllText("..//..//..//stop_words.txt").Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // amount of threads used for building index
        private const int ThreadsAmount = 4;

        // datasets
        private FileInfo[] filesList;

        // parallel safe dict to creating index
        private ConcurrentDictionary<string, ConcurrentQueue<string>> invertedIndex;


        public Indexer(string dirPath)
        {
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            this.filesList = dir.GetFiles();
        }


        private static string[] LexemsPreprocessing(string sourceText)
        {
            // remove html new-line teg 
            string fileText = Regex.Replace(sourceText, "< *br */ *>", "");
            fileText = Regex.Replace(fileText, "[0-9]+", "");

            // match all words, but no capture "_", transform all to lowercase and take only unique 
            string[] lexemLists = Regex.Matches(fileText, @"[^_\W]+['`]*[^_\W]*").Cast<Match>().Select(m => m.Value.ToLower()).Distinct().ToArray();

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
                        this.invertedIndex.AddOrUpdate(lexem,
                            _ => { 
                                ConcurrentQueue<string> newQ = new ConcurrentQueue<string>(); 
                                newQ.Enqueue(curFile.Name); 
                                return newQ; 
                            },
                            (k, v) => { 
                                v.Enqueue(curFile.Name); 
                                return v; 
                            });
                    }
                }
            }
        }


        public void CreateIndex()
        {
            if(!File.Exists("savedIndex.txt"))
            {
                this.invertedIndex = new ConcurrentDictionary<string, ConcurrentQueue<string>>();

                if (ThreadsAmount == 1)
                {
                    ParseBlock(0, this.filesList.Length);
                }
                else
                {
                    float filePerThread = this.filesList.Length / (float)ThreadsAmount;

                    Task[] tasks = new Task[ThreadsAmount];

                    for (int iter = 0; iter < ThreadsAmount; iter++)
                    {
                        int startFile = (int)(filePerThread * iter);
                        int endFile = (int)(filePerThread * (iter + 1));
                        tasks[iter] = Task.Run(() => ParseBlock(startFile, endFile));
                    }
                    Task.WaitAll(tasks);
                }

                SaveIndexToFile();

                Console.WriteLine("New index has been created created.");
            }
            else
            {
                Console.WriteLine("New index wasn't created, used saved one.");
                this.invertedIndex = JsonConvert.DeserializeObject<ConcurrentDictionary<string, ConcurrentQueue<string>>>(File.ReadAllText("savedIndex.txt"));
            }
        }


        // for each lexem in input text, give every file it is presented in
        public Dictionary<string, List<string>> AnalyzeInput(string inputText)
        {
            string[] lexems = LexemsPreprocessing(inputText);

            Dictionary<string, List<string>> result = new();

            foreach (string lexem in lexems)
            {
                if (this.invertedIndex.ContainsKey(lexem))
                {
                    result[lexem] = this.invertedIndex[lexem].ToList();
                }
                else if (ExcludedWords.Contains(lexem))
                {
                    result[lexem] = new List<string>{ "This is stop word" };
                }
                else
                {
                    result[lexem] = new List<string> { "There are no such files" };
                }
            }
                
            return result;
        }


        private void SaveIndexToFile()
        {
            string jsonIndex = JsonConvert.SerializeObject(this.invertedIndex, Formatting.Indented);
            File.WriteAllText("savedIndex.txt", jsonIndex);
        }
    }
}
