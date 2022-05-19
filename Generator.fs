namespace RavenDBDataGenerator

open System
open System.IO

open Types

type ThreadSafeRandom() = 

    [<ThreadStatic>][<DefaultValue>] 
    static val mutable private _global : Random 

    static member init() =
        if ThreadSafeRandom._global = null then
            ThreadSafeRandom._global <- new Random()

    static member Next(min:int, max:int) =
        ThreadSafeRandom.init()
        let localRnd = new Random(ThreadSafeRandom._global.Next())
        localRnd.Next(min,max)

    static member Next(min:int64, max:int64) =
        ThreadSafeRandom.init()
        let localRnd = new Random(ThreadSafeRandom._global.Next())
        let n = localRnd.NextDouble()
        let diff = max - min
        min + (int64)(n * (float)diff)

    static member NextDate(min:DateTime, max:DateTime) =
        ThreadSafeRandom.init()
        let localRnd = new Random(ThreadSafeRandom._global.Next())
        let ticks = ThreadSafeRandom.Next(min.Ticks, max.Ticks)
        new DateTime(ticks)

module Generator = 

    let uppercaseFirstChar (sentence:string) = 
        match sentence.Length with
        | 0 -> ""
        | _ -> String.Concat(sentence[0].ToString().ToUpper(), sentence.AsSpan(1))

    let makeDictionaryFrom path =
        File.ReadAllText(path)
        |> (fun text -> text.Split("\n", StringSplitOptions.RemoveEmptyEntries))
        |> Array.map( fun line -> line.Trim())
        |> Array.filter( fun line -> line <> "")

    let randomWord (dictionary :string[]) = dictionary[ThreadSafeRandom.Next(0, dictionary.Length)]

    let randWords dict = seq { while true do yield (randomWord dict) }

    let makeSentence dict len =
        randWords dict 
        |> Seq.take len 
        |> String.concat " "
        |> uppercaseFirstChar
        |> fun s -> s + "."

    let randomSentences dict (minWords:int) (maxWords:int) = 
        seq {
            while true do 
                yield makeSentence dict (ThreadSafeRandom.Next(minWords, maxWords)) 
        }

    let makeParagraph dict minWordsPerSentence maxWordsPerSentence len = 
        randomSentences dict minWordsPerSentence maxWordsPerSentence
        |> Seq.take len 
        |> String.concat " "

    let makePost (dict:string[]) (topic:Topic) (range:DateRange) maxAttachments =
        {
            topicId     = topic.Id
            title       = makeSentence dict 6
            body        = makeParagraph dict 5 20 (ThreadSafeRandom.Next(10,30))
            tags        = Array.empty
            timeStamp   = ThreadSafeRandom.NextDate(new DateTime(2010,1,1), DateTime.Now)
            attachments = Array.empty

            Id          = String.Empty
        }

    let randomPosts dict topic range maxAttachments =
        seq {
            while true do
                yield makePost (dict:string[]) (topic:Topic) (range:DateRange) maxAttachments
        }