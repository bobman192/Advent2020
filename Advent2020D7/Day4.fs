﻿namespace Advent2020.Day4

open System

[<Measure>] type cm
[<Measure>] type inch

type hexColor = 
    {
        red : byte
        green : byte
        blue : byte
    }

type passportFielded = 
    {
        byr : int option; //Birth year
        iyr : int option; //Issue year
        eyr : int option; //Expiration year
        hgt: float<cm> option; //Height
        hcl: hexColor option; //Hair color
        ecl: string option; //Eye color
        pid: int option; //Passport ID
        cid: int option; //Country ID
    }

type passportRaw = 
    {
        byr : string option; //Birth year
        iyr : string option; //Issue year
        eyr : string option; //Expiration year
        hgt: string option; //Height
        hcl: string option; //Hair color
        ecl: string option; //Eye color
        pid: string option; //Passport ID
        cid: string option; //Country ID
    }


module private Day4Helpers = 
    type fieldAndVal = 
        {
            field : string;
            stringVal : string;
        }


module Option =
    let (>>=) r f = Option.bind f r
    let rtn v     = Some v
     
    let traverseList f ls = 
        let folder head tail = f head >>= (fun h -> tail >>= (fun t -> h::t |> rtn))
        List.foldBack folder ls (rtn List.empty)
    let sequenceList ls = traverseList id ls
    // val traverseList : ('a -> 'b option) -> 'a list -> 'b list option
    // val sequenceList : 'a option list -> 'a list option

module Main = 

    let cmPerInch : float<cm/inch> = 2.54<cm/inch>

    let countValidPassports (passports : passportRaw list) : int =
        let checkPassportValid (passportToCheck : passportRaw) : bool = 
            match passportToCheck with 
            | {byr = byr; iyr = iyr; eyr = eyr; hgt = hgt; hcl = hcl; ecl = ecl; pid = pid; cid = cid;} ->
                let listOfValidChecks = [byr; iyr; eyr; hgt; hcl; ecl; pid]
                let listOfValidElements : bool list = 
                    listOfValidChecks
                    |> List.map Option.isSome
                (true, listOfValidElements)
                ||> List.fold (fun acc elem -> acc && elem )
        passports
        |> List.map checkPassportValid
        |> List.sumBy (fun b -> if b then 1 else 0)

    let countTrueValidPassports (passports : passportRaw list) : int =
        let checkPassportValid (passportToCheck : passportRaw) : bool = 
            match passportToCheck with 
            | {byr = byr; iyr = iyr; eyr = eyr; hgt = hgt; hcl = hcl; ecl = ecl; pid = pid; cid = cid;} ->
                let strToInt str =
                    match System.Int32.TryParse(str:string) with
                    | (true, int) -> Some(int)
                    | _ -> None
                let inRangeInts (min : int) (max : int) (valToCheck : int) : bool = 
                    valToCheck >= min && valToCheck <= max
                let parseInRangeValid (min : int) (max : int) (strToVerify : string) = 
                    let valValid = strToInt strToVerify
                    match valValid with 
                    | Some (valValid) -> inRangeInts min max valValid
                    | None -> false
                let parseInRangeValidOption (min : int) (max : int) (strToVerify : string option) = 
                    let valValid = Option.bind strToInt strToVerify
                    match valValid with 
                    | Some (valValid) -> inRangeInts min max valValid
                    | None -> false
                let byrValid = parseInRangeValidOption 1920 2002 byr
                let iyrValid = parseInRangeValidOption 2010 2020 iyr
                let eyrValid = parseInRangeValidOption 2020 2030 eyr
                let hgtValid = 
                    match hgt with 
                    | None -> false
                    | Some(hgt) ->
                        let re = new System.Text.RegularExpressions.Regex(@"(\d+)([a-zA-Z]+)")
                        let resultMatches = re.IsMatch(hgt)
                        match resultMatches with 
                        | false -> false
                        | true ->
                            let splitHeight = re.Match(hgt)
                            let height = splitHeight.Groups.[1].Value
                            let measure = splitHeight.Groups.[2].Value
                            match measure with 
                            | "cm" -> parseInRangeValid 150 193 height
                            | "in" -> parseInRangeValid 59 76 height
                            | _ -> false
                let hclValid = 
                    match hcl with 
                    | None -> false
                    | Some(hcl) -> 
                        let re = new System.Text.RegularExpressions.Regex(@"\B#([a-f0-9]{6})(?![~!@#$%^&*()=+_`\-\|\/'\[\]\{\}]|[?.,]*\w)")
                        re.IsMatch(hcl)
                let eclValid = 
                    match ecl with 
                    | None -> false
                    | Some(ecl) ->
                        match ecl with 
                        | "amb" -> true
                        | "blu" -> true
                        | "brn" -> true
                        | "gry" -> true
                        | "grn" -> true
                        | "hzl" -> true
                        | "oth" -> true
                        | _ -> false
                let pidValid = 
                    match pid with 
                    | None -> false
                    | Some (pid) ->
                        let re = new System.Text.RegularExpressions.Regex(@"\b([0-9]{9})(?![~!@#$%^&*()=+_`\-\|\/'\[\]\{\}]|[?.,]|[0-9]*\w)")
                        re.IsMatch(pid)
                 
                let listOfValidElements : bool list = 
                    [byrValid; iyrValid; eyrValid; hgtValid; hclValid; eclValid; pidValid]
                (true, listOfValidElements)
                ||> List.fold (fun acc elem -> acc && elem )
        passports
        |> List.map checkPassportValid
        |> List.sumBy (fun b -> if b then 1 else 0)

    let parse (fileInput : string list) : passportRaw list =
        let groupedStrings (rawInput : string list) : string list list = 
            let inputFolder (acc : string list list) (entryToAdd : string) : string list list =
                match entryToAdd with 
                | "" ->
                    let emptyStringList = List<string>.Empty //Note that this is a property with capital E of List<string>
                    let newEmptyStringList = List.singleton emptyStringList
                    List.append acc newEmptyStringList
                | entryToAdd ->
                    let lastStringListSplitIndex = 
                        ((acc |> List.length) - 1)
                    let front, last = acc |> List.splitAt lastStringListSplitIndex
                    let updateFinalString (stringListListToReplace : string list list) : string list list = 
                        stringListListToReplace
                        |> List.head
                        |> List.append (List.singleton entryToAdd)
                        |> List.singleton
                    last
                    |> updateFinalString
                    |> List.append front
            let initialAcc : string list list =
                List.empty<string>
                |> List.singleton
            (initialAcc, rawInput)
            ||> List.fold inputFolder
        let splitLinesIntoEntries (originalGrouping : string list) : string list = 
            let splitLineIntoEntries (lineToSplit : string) : string list = 
                lineToSplit.Split(' ')
                |> Array.toList
            let entryFolder (acc : string list) (entryToAcc : string) = 
                entryToAcc
                |> splitLineIntoEntries
                |> List.append acc
            let baseEntry = List.empty<string> //Note that this is a method which generates an empty list, of type string
            (baseEntry, originalGrouping)
            ||> List.fold entryFolder
        let parseStringIntoPassportRaw (organizedEntryLines : string list) : passportRaw = 
            let splitFieldAndVal (combinedFieldAndVal : string) : string * string = 
                let splitString = combinedFieldAndVal.Split(':')
                if (splitString.Length = 1) then (splitString.[0], String.Empty) else (splitString.[0], splitString.[1])
            let fieldAndValList = organizedEntryLines |> List.map splitFieldAndVal |> Map.ofList
            let passportCreator (inputKeys: Map<string, string>) : passportRaw = 
                let flip f a b = f b a
                let flippedMapTryFind = flip Map.tryFind
                let mapToFind = flippedMapTryFind inputKeys
                let byr = "byr" |> mapToFind
                let iyr = "iyr" |> mapToFind
                let eyr = "eyr" |> mapToFind
                let hgt = "hgt" |> mapToFind
                let hcl = "hcl" |> mapToFind
                let ecl = "ecl" |> mapToFind
                let pid = "pid" |> mapToFind
                let cid = "cid" |> mapToFind
                {byr = byr; iyr = iyr; eyr = eyr; hgt = hgt; hcl = hcl; ecl = ecl; pid = pid; cid = cid;}
            fieldAndValList
            |> passportCreator
        fileInput
        |> groupedStrings
        |> List.map splitLinesIntoEntries
        |> List.map parseStringIntoPassportRaw
                
    

    let run : unit = 
        let fileName = "Advent2020D4.txt"
        let fileInput = Advent2020.File.listedLines fileName
        let initialState = parse fileInput

        printfn "Sum: %i" (countTrueValidPassports initialState)