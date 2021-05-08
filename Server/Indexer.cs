using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;

namespace Server {
    class Indexer {
        // file with words that will not be included in inverted index, as articles or pronouns
        private readonly string[] _excludedWords;

        // amount of threads used for building index
        private const int _threadsAmount = 4;

        // datasets
        private readonly FileInfo[] _filesList;

        // parallel safe dict for creating index
        private ConcurrentDictionary<string, ConcurrentQueue<string>> _invertedIndex;


        public Indexer(string dirPath) {
            _excludedWords = File.ReadAllText("indexer-assets/stopWords.txt").Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var dir = new DirectoryInfo(dirPath);
            _filesList = dir.GetFiles();
        }

        public void CreateIndex() {
            if (!File.Exists("indexer-assets/savedIndex.txt") || new FileInfo("indexer-assets/savedIndex.txt").Length == 0) {
                _invertedIndex = new ConcurrentDictionary<string, ConcurrentQueue<string>>();

                // if conditional unreachable for most cases, but useful in case sequential execution
                if (_threadsAmount == 1) {
                    ParseBlock(0, this._filesList.Length);
                } else {
                    float filePerThread = _filesList.Length / (float)_threadsAmount;

                    Task[] tasks = new Task[_threadsAmount];

                    for (int iter = 0; iter < _threadsAmount; iter++) {
                        int startFile = (int)(filePerThread * iter);
                        int endFile = (int)(filePerThread * (iter + 1));
                        tasks[iter] = Task.Run(() => ParseBlock(startFile, endFile));
                    }
                    Task.WaitAll(tasks);
                }

                SaveIndexToFile();

                Console.WriteLine("New index has been created created.");
            } else {
                Console.WriteLine("New index wasn't created, used saved one.");
                _invertedIndex = JsonConvert.DeserializeObject<ConcurrentDictionary<string, ConcurrentQueue<string>>>(File.ReadAllText("indexer-assets/savedIndex.txt"));
            }
        }


        // for each lexem in input text, give every file it is presented in
        public Dictionary<string, List<string>> AnalyzeInput(string inputText) {
            string[] lexems = LexemsPreprocessing(inputText);

            Dictionary<string, List<string>> result = new();

            foreach (string lexem in lexems) {
                if (_invertedIndex.ContainsKey(lexem)) {
                    result[lexem] = this._invertedIndex[lexem].ToList();
                } else if (_excludedWords.Contains(lexem)) {
                    result[lexem] = new List<string> { "This is stop word" };
                } else {
                    result[lexem] = new List<string> { "There are no such files" };
                }
            }

            if (result.Count > 1) {
                List<string> lexemesIntersection;
                lexemesIntersection = result.Values.Aggregate((previousList, nextList) => previousList.Intersect(nextList).ToList());

                if (lexemesIntersection.Count == 0) {
                    result["All words"] = new List<string> { "There are no common files" };
                } else if (lexemesIntersection.Count == 1 && lexemesIntersection[0] == "This is stop word") {
                    result["All words"] = new List<string> { "All words are stop words" };
                }  else {
                    result["All words"] = lexemesIntersection;
                }
            }

            return result;
        }


        private static string[] LexemsPreprocessing(string sourceText) {
            // remove html new-line teg 
            string fileText = Regex.Replace(sourceText, "< *br */ *>", "");

            fileText = Regex.Replace(fileText, "[0-9]+", "");

            // match all words, but don't capture "_", transform all to lowercase and take only unique 
            string[] lexemLists = Regex.Matches(fileText, @"[^_\W]+['`]*[^_\W]*").Cast<Match>()
                .Select(m => m.Value.ToLower())
                .Distinct()
                .ToArray();

            return lexemLists;
        }


        private void ParseBlock(int startFileIndex, int endFileIndex) {
            for(int i=startFileIndex; i<endFileIndex; i++) {
                FileInfo curFile = this._filesList[i];
                string[] lexemLists = LexemsPreprocessing(File.ReadAllText(curFile.FullName));

                foreach(string lexem in lexemLists) {
                    if (!_excludedWords.Contains(lexem)) {
                        _invertedIndex.AddOrUpdate(
                            lexem,
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


        private void SaveIndexToFile() {
            string jsonIndex = JsonConvert.SerializeObject(this._invertedIndex, Formatting.Indented);
            File.WriteAllText("indexer-assets/savedIndex.txt", jsonIndex);
        }
    }
}
