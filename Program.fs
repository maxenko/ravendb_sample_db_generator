open System
open System.IO

open NUlid
open Raven.Client.Documents

open RavenDBDataGenerator.Types
open RavenDBDataGenerator
open Raven.Client.Documents.Session

let touchTopic (sesh:IDocumentSession) title = 

    query {
        for topic in sesh.Query<Topic>() do
        where (topic.title = title)
        select topic } 
     |> Seq.toArray
     |> fun entries ->
        match entries.Length with
        | 0 ->
            let stub = {
                title       = title;
                status      = TopicStatus.Open;
                Id          = String.Empty; //NUlid.Ulid.NewUlid().ToString();
            }
            sesh.Store(stub, NUlid.Ulid.NewUlid().ToString())
            sesh.SaveChanges()
            stub
        | _ -> Array.head entries


let store = new DocumentStore()
let nodes = [
    "http://192.168.1.218:8080";
    //"http://192.168.1.209:8080";
    //"http://192.168.1.184:8080";
    ]

store.Urls <- Seq.toArray <| nodes
store.Database <- "pindap"
store.Initialize() |> ignore

let topicTitle = "testtopic"

let sesh = store.OpenSession(store.Database)
let topic = touchTopic sesh topicTitle

let dict = Generator.makeDictionaryFrom "dictionary.txt"
let bulkInsert = store.BulkInsert()

Generator.randomPosts dict topic {min = (DateTime.UtcNow.AddYears(-12)); max = DateTime.UtcNow} 0
|> Seq.take (50_000) // <- number of posts this will generate in one run
|> Seq.iter (fun p -> 
    let id = Ulid.NewUlid().ToString()
    p.Id <- id
    bulkInsert.Store(p, id)
    )

bulkInsert.Dispose()
sesh.Dispose()
store.Dispose()

exit 0