# Description
This is a repository that holds source code for creating an inverted index and using it via client-server architecture.
Inverted index is a database index that stores a mapping of words with their presence in a set of documents

Inverted index is built on the given datasets with some filtering (skip html br-tags and numbers) using list of stop words (words that will not be included in index such as prepositions, pronouns, adverbs). 

Index can be built using multithreading.
_Amount of threads used to build an inverted index can vary by changing const variable in server's code (Server.cs), default value is 4._

# Project structure
### Top-level directory
    .                                           # Current directory
    ├── BinarySerializer                        
    │   └── BinarySerializer.cs                 # Class for serializing object to byte array
    ├── Client                                  
    │   ├── Client.cs                           # Client (index user)
    │   ├── ClientProgram.cs                    # Start client
    │   └── Client.csproj                       # Client project file
    ├── Server                                  
    │   ├── indexer-assets                      # Data file for index building
    │	│   ├── stopWords.txt					# List of words that will be skipped in index 
    │   │   └── datasets                        # .txt files for index building index
    │   ├── Indexer.cs                          # Builds index 
    │   ├── Server.cs                           # Uses index,works with clients 
    │   ├── ServerProgram.cs                    # Start server
    │   └── Server.csproj                       # Server project file
    ├── parallel-computing-inverted-index.sln   # MS VS solution
    └── README.md
    
# Requirements
- .NET 5
- Json.NET 13.0.1

# Project setup
### Clone repository
```sh
git clone https://github.com/Dekardt/parallel-computing-inverted-index.git
cd parallel-computing-inverted-index
```

### Prepare data
Inverted index uses files stored in "./Server/indexer-assets/datasets/" folder, filtering the source text text using "./Server/indexer-assets/stopWords.txt" file.

Next steps are mandatory:
- place .txt files in "./Server/indexer-assets/datasets/" directory
- add file with stop words to "./Server/indexer-assets/". If you don't wont filter text, add empty "stopWords.txt." file

After first index creation it would be saved as txt file in ./Server/indexer-assets/savedIndex.txt" for future using (if this file is in folder and is not empty, new index will not be created, but will be used saved one). If you want to create new index delete  savedIndex.txt from folder.

# Run programs
Execute the following commands in strict order:
1. Run server program
    1.1.  Move to the server directory (where Server.csproj located): "./Server/"
    1.2. Build project:
    ``` 
   dotnet build Server.csproj
    ```
    1.3. Run project
    ``` 
    dotnet run Server.csproj
    ```
    1.4. Wait for the notification that the server is waiting for a connection on server-port XXXX
2. Run client program
    2.1. Move to the client directory (where Client.csproj located): "./Client/"
    2.2. Build project
    ```sh
    dotnet build Client.csproj
    ```
    2.3. Run project
    ``` 
    dotnet run Client.csproj
    ```
3. Type message in client's program console. Enter _0_ to end work.

After executing this steps, for both server and client in directories "Server/bin/Debug/net5.0" and "Client/bin/Debug/net5.0" you can find files Server.exe and Client.exe respectively. Use them to run both programs skipping steps (1.1.-1.3. and 2.1.-2.3.), but step order still strip (run server -> wait for the notification -> run client) 
