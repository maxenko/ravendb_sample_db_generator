namespace RavenDBDataGenerator

open System

module Types = 

    type DateRange = {
        min             : DateTime
        max             : DateTime
    }

    type TopicStatus =
        | Open
        | Closed
        | Locked
        | Blog

    type Topic = {
        title           : string
        status          : TopicStatus
        mutable Id      : string
    }

    type Attachment = {
        path            : string;
        order           : int;
        original        : string;

        timeStamp       : DateTime

        classifications : string[]

        mutable Id      : string
    }

    type Post = {
        topicId         : string

        title           : string
        body            : string
        attachments     : Attachment[]
        tags            : string[]

        timeStamp       : DateTime

        mutable Id      : string
    }