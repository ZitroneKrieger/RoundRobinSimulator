using RoundRobinSimulator;
using System.Diagnostics;
using System;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
using System.Text.RegularExpressions;
using System.Numerics;

namespace MyApp
{
    internal class Program
    {
        const int AUFTEILUNG_SCHWARZ_WEISS_FAIRNESS_PARAM = 1; // 1 am fairsten ... 3 unfair // 1 langsam ... 3 Schnell
        const int BOARDS_TO_BE_PLAYED_ON = 2; // 2 oder 3
        const int MAX_TRIES_TO_GET_LUCKY = 10000; // timeout damit nicht endlosschleife passiert weil keine SchachMatches mehr passieren können
        static void Main(string[] args)
        {
            //Marcel Michael Angelo Nathalie Boris Harpreet Kim Oliver Pavle Kevin Kama Zeljko Luca Denise Benedikt

            Introduction();

            //Preparation(out List<Player> players);

            List<Player> players = GetPlayers();

            PermutationsCombinationsCalculation(players);

            List<List<Tuple<Player, Player>>> allRoundParings = GetAllRoundPairings(players);

            //PrintTable(players, AllRoundParings);

            GiveStatistics(players);

            DownloadAsCsv(allRoundParings);

            var numericInput = true;


            var orderedPlayersByName = players.OrderBy(x => x.Name).ToList();

            var listOfSchachMatchingsPerPlayer = new Dictionary<Player, List<SchachMatch>>();
            FillSchachMatches(allRoundParings, orderedPlayersByName, listOfSchachMatchingsPerPlayer);

            while (numericInput)
            {
                PrintMenu();
                var input = Console.ReadLine();

                numericInput = int.TryParse(input, out var intInput);

                if (numericInput == false)
                {
                    Console.WriteLine("ungültige Eingabe. Bitte 1, 2 oder 3 tippen und dann Enter.");
                    numericInput = true;
                    continue;
                }

                switch (intInput)
                {
                    case 1:
                        {
                            PrintTable(players, allRoundParings);
                            break;
                        }
                    case 2:
                        {
                            AddResult(players, allRoundParings, listOfSchachMatchingsPerPlayer);
                            break;
                        }
                    case 3:
                        {
                            ShowStandings(orderedPlayersByName, listOfSchachMatchingsPerPlayer);
                            break;
                        }
                    case 4:
                        {
                            CreatePredictions(listOfSchachMatchingsPerPlayer, orderedPlayersByName);
                            break;
                        }
                    case 9:
                        {
                            numericInput = false;
                            break;
                        }

                }
            }



        }

        private static void CreatePredictions(Dictionary<Player, List<SchachMatch>> listOfSchachMatchingsPerPlayer, List<Player> orderedPlayersByName)
        {
            Dictionary<Player, List<SchachMatch>> copy = new(listOfSchachMatchingsPerPlayer);


            foreach (var player in listOfSchachMatchingsPerPlayer.Keys)
            {
                foreach (var item in listOfSchachMatchingsPerPlayer[player])
                {
                    Random rnd = new Random();
                    int luckyNumber = rnd.Next(0, 101);

                    

                    decimal likelyNess = 0M;

                    if (item.Weiss.Elo >= item.Schwarz.Elo)
                    {
                        likelyNess = (decimal)(1 / (1 + Math.Pow((double)10, (double)((item.Schwarz.Elo - item.Weiss.Elo) / 400M)))) * 100;
                        decimal zehnProzentVonLikelyNess = likelyNess * 0.10M;

                        GetIndexByName(orderedPlayersByName, item.Weiss.Name, out int indexWeiss);
                        GetIndexByName(orderedPlayersByName, item.Schwarz.Name, out int indexSchwarz);

                        indexWeiss++;
                        indexSchwarz++;

                        if (luckyNumber > likelyNess)
                        {
                            SaveResult(copy, orderedPlayersByName, indexSchwarz, indexWeiss, 1);
                        }
                        else if (luckyNumber >= likelyNess - zehnProzentVonLikelyNess)
                        {
                            SaveResult(copy, orderedPlayersByName, indexSchwarz, indexWeiss, 3);
                        }
                        else
                        {
                            SaveResult(copy, orderedPlayersByName, indexSchwarz, indexWeiss, 2);
                        }
                    }
                    else
                    {
                        likelyNess = (decimal)(1 / (1 + Math.Pow((double)10, (double)((item.Weiss.Elo - item.Schwarz.Elo) / 400M)))) * 100;
                        decimal zehnProzentVonLikelyNess = likelyNess * 0.10M;

                        GetIndexByName(orderedPlayersByName, item.Weiss.Name, out int indexWeiss);
                        GetIndexByName(orderedPlayersByName, item.Schwarz.Name, out int indexSchwarz);

                        indexWeiss++;
                        indexSchwarz++;

                        if (luckyNumber > likelyNess)
                        {
                            SaveResult(copy, orderedPlayersByName, indexWeiss, indexSchwarz, 1);
                        }
                        else if (luckyNumber >= likelyNess - zehnProzentVonLikelyNess)
                        {
                            SaveResult(copy, orderedPlayersByName, indexWeiss, indexSchwarz, 3);
                        }
                        else
                        {
                            SaveResult(copy, orderedPlayersByName, indexWeiss, indexSchwarz, 2);
                        }
                    }
                }
            }

            ShowStandings(orderedPlayersByName, copy);

        }

        private static void GetIndexByName(List<Player> orderedPlayersByName, string name, out int index)
        {
            index = orderedPlayersByName
            .Select((player, idx) => new { Player = player, Index = idx }) 
            .FirstOrDefault(x => x.Player.Name == name)?.Index ?? -1;
        }

        private static void FillSchachMatches(List<List<Tuple<Player, Player>>> allRoundParings, List<Player> orderedPlayersByName, Dictionary<Player, List<SchachMatch>> listOfSchachMatchingsPerPlayer)
        {
            foreach (var player in orderedPlayersByName)
            {
                var playersOpponentsList = new List<SchachMatch>();
                foreach (var round in allRoundParings)
                {
                    foreach (var pairing in round)
                    {
                        if (pairing.Item1.Name == player.Name)
                        {
                            playersOpponentsList.Add(new SchachMatch() { Schwarz = player, Weiss = pairing.Item2 });
                        }
                        else if (pairing.Item2.Name == player.Name)
                        {
                            playersOpponentsList.Add(new SchachMatch() { Schwarz = pairing.Item1, Weiss = player });
                        }
                    }
                }

                listOfSchachMatchingsPerPlayer.Add(player, playersOpponentsList);
            }
        }

        private static void ShowStandings(List<Player> orderedPlayersByName, Dictionary<Player, List<SchachMatch>> listOfSchachMatchingsPerPlayer)
        {
            Dictionary<Player, List<SchachMatch>> copy = new(listOfSchachMatchingsPerPlayer);

            List<SchachMatch> SchachMatchesCopy = new List<SchachMatch>();

            for (int i = 0; i < orderedPlayersByName.Count; i++)
            {
                Player? player = orderedPlayersByName[i];
                SchachMatchesCopy = listOfSchachMatchingsPerPlayer[player].OrderBy(x => (x.Schwarz.Name == player.Name ? x.Weiss.Name : x.Schwarz.Name)).ToList();
                copy[player] = SchachMatchesCopy;
            }
            //var sortedSchachMatches = SchachMatches.OrderBy(SchachMatch => SchachMatch.Player1 == oliver ? SchachMatch.Player2.Name : SchachMatch.Player1.Name)
            //                       .ToList();

            CreateHeaderForStandings(orderedPlayersByName);
            for (int i = 0; i < orderedPlayersByName.Count; i++)
            {
                Player? player = orderedPlayersByName[i];
                var cell = "";

                for (int j = 0; j < copy[player].Count; j++)
                {
                    if (j == i)
                    {
                        cell += "".PadRight(11,'-') + " | ";
                    }
                    if (copy[player][j].Weiss.Name == player.Name)
                    {
                        if (copy[player][j].ResultatWeiß != null)
                        {
                            cell += copy[player][j].ResultatWeiß?.ToString().PadRight(12) + "| ";
                        }
                        else
                        {
                            cell += "".PadRight(12) + "| ";
                        }
                    }
                    else if (copy[player][j].Schwarz.Name == player.Name)
                    {
                        if (copy[player][j].ResultatSchwarz != null)
                        {
                            cell += copy[player][j].ResultatSchwarz?.ToString().PadRight(12) + "| ";
                        }
                        else
                        {
                            cell += "".PadRight(12) + "| ";
                        }
                    }
                    else
                    {
                        cell += "".PadRight(12) + "| ";
                    }


                    if (i == orderedPlayersByName.Count - 1 && j == copy[player].Count - 1)
                    {
                        cell += "".PadRight(11, '-') + " | ";
                    }
                }

                Console.WriteLine($"| {player.Name,-12}| {cell}");
                Console.WriteLine($"+{new System.Text.StringBuilder().Insert(0, "-------------+", orderedPlayersByName.Count + 1)}");
            }

            Console.WriteLine();

            CreateHeaderForStandings(orderedPlayersByName);
            var sumCells = "";

            foreach (var player in copy.Keys)
            {
                sumCells += player.GetResult().ToString().PadRight(12) + "| ";
            }

            Console.WriteLine($"| {"Summe:",-12}| {sumCells}");
            Console.WriteLine($"+{new System.Text.StringBuilder().Insert(0, "-------------+", orderedPlayersByName.Count + 1)}");
        }

        private static void CreateHeaderForStandings(List<Player> orderedPlayersByName)
        {
            var allPlayers = "";
            foreach (var player in orderedPlayersByName)
            {
                allPlayers += " ";
                allPlayers += player.Name.PadRight(12, ' ');
                allPlayers += "|";
            }


            Console.WriteLine($"+-------------+{new System.Text.StringBuilder().Insert(0, "-------------+", orderedPlayersByName.Count)}");
            Console.WriteLine($"|             |{allPlayers}");
            Console.WriteLine($"+-------------+{new System.Text.StringBuilder().Insert(0, "-------------+", orderedPlayersByName.Count)}");
        }

        private static void AddResult(List<Player> players, List<List<Tuple<Player, Player>>> allRoundParings, Dictionary<Player, List<SchachMatch>> listOfSchachMatchingsPerPlayer)
        {
            var orderedPlayersByName = players.OrderBy(x => x.Name).ToList();


           
            Console.WriteLine("Welcher Spieler bekommt ein Resultat?");
            for (int i = 0; i < orderedPlayersByName.Count; i++)
            {
                Player? player = orderedPlayersByName[i];
                Console.WriteLine($"{i + 1} ... {player.Name}");
            }
            
            var eingabe1Falsch = true;
            while (eingabe1Falsch)
            {
                var playerFigure = Console.ReadLine();
                
                if (!int.TryParse(playerFigure, out var intPlayerFigure))
                {
                    continue;
                }
                var eingabe2Falsch = true;
                while (eingabe2Falsch)
                {
                    Console.WriteLine("Gegen wen wurde gespielt?");
                    for (int i = 0; i < orderedPlayersByName.Count; i++)
                    {
                        Player? player = orderedPlayersByName[i];
                        //if(intPlayerFigure != i + 1)
                        //{
                            Console.WriteLine($"{i + 1} ... {player.Name}");
                        //}
                    }

                    var secondPlayerFigure = Console.ReadLine();
                    if (!int.TryParse(secondPlayerFigure, out var intSecondPlayerFigure))
                    {
                        continue;
                    }


                    var eingabe3Falsch = true;
                    while(eingabe3Falsch)
                    {
                        Console.WriteLine($"Resultat: {orderedPlayersByName[intPlayerFigure - 1].Name} vs {orderedPlayersByName[intSecondPlayerFigure - 1].Name}");
                        Console.WriteLine($"1 ... {orderedPlayersByName[intPlayerFigure - 1].Name} hat gewonnen");
                        Console.WriteLine($"2 ... {orderedPlayersByName[intSecondPlayerFigure - 1].Name} hat gewonnen");
                        Console.WriteLine($"3 ... unendschieden");

                        var result = Console.ReadLine();

                        if (!int.TryParse(result, out var intResult))
                        {
                            continue;
                        }

                        SaveResult(listOfSchachMatchingsPerPlayer, orderedPlayersByName, intPlayerFigure, intSecondPlayerFigure, intResult);

                        eingabe1Falsch = false;
                        eingabe2Falsch = false;
                        eingabe3Falsch = false;
                    }

                }
            }
            


        }

        private static void SaveResult(Dictionary<Player, List<SchachMatch>> listOfSchachMatchingsPerPlayer, List<Player> orderedPlayersByName, int intPlayerFigure, int intSecondPlayerFigure, int intResult)
        {
            switch (intResult)
            {
                case 1:
                    // erster spieler hat gewonnen

                    foreach (var SchachMatch in listOfSchachMatchingsPerPlayer[orderedPlayersByName[intPlayerFigure - 1]])
                    {
                        if (SchachMatch.Schwarz.Name == orderedPlayersByName[intPlayerFigure - 1].Name && SchachMatch.Weiss.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 1;
                            SchachMatch.ResultatWeiß = 0;
                            SchachMatch.Schwarz.AddWin();
                            SchachMatch.Weiss.AddLose();
                            break;
                        }
                        if (SchachMatch.Weiss.Name == orderedPlayersByName[intPlayerFigure - 1].Name && SchachMatch.Schwarz.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 0;
                            SchachMatch.ResultatWeiß = 1;
                            SchachMatch.Weiss.AddWin();
                            SchachMatch.Schwarz.AddLose();
                            break;
                        }
                    }
                    foreach (var SchachMatch in listOfSchachMatchingsPerPlayer[orderedPlayersByName[intSecondPlayerFigure - 1]])
                    {

                        if (SchachMatch.Schwarz.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name && SchachMatch.Weiss.Name == orderedPlayersByName[intPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 0;
                            SchachMatch.ResultatWeiß = 1;
                            SchachMatch.Weiss.AddWin();
                            SchachMatch.Schwarz.AddLose();
                            break;
                        }
                        if (SchachMatch.Weiss.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name && SchachMatch.Schwarz.Name == orderedPlayersByName[intPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 1;
                            SchachMatch.ResultatWeiß = 0;
                            SchachMatch.Schwarz.AddWin();
                            SchachMatch.Weiss.AddLose();
                            break;
                        }
                    }
                    break;
                case 2:
                    // zweiter spieler hat gewonnen
                    foreach (var SchachMatch in listOfSchachMatchingsPerPlayer[orderedPlayersByName[intSecondPlayerFigure - 1]])
                    {
                        if (SchachMatch.Schwarz.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name && SchachMatch.Weiss.Name == orderedPlayersByName[intPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 1;
                            SchachMatch.ResultatWeiß = 0;
                            SchachMatch.Schwarz.AddWin();
                            SchachMatch.Weiss.AddLose();
                            break;
                        }
                        if (SchachMatch.Weiss.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name && SchachMatch.Schwarz.Name == orderedPlayersByName[intPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 0;
                            SchachMatch.ResultatWeiß = 1;
                            SchachMatch.Weiss.AddWin();
                            SchachMatch.Schwarz.AddLose();
                            break;
                        }
                    }
                    foreach (var SchachMatch in listOfSchachMatchingsPerPlayer[orderedPlayersByName[intPlayerFigure - 1]])
                    {

                        if (SchachMatch.Schwarz.Name == orderedPlayersByName[intPlayerFigure - 1].Name && SchachMatch.Weiss.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 0;
                            SchachMatch.ResultatWeiß = 1;
                            SchachMatch.Weiss.AddWin();
                            SchachMatch.Schwarz.AddLose();
                            break;
                        }
                        if (SchachMatch.Weiss.Name == orderedPlayersByName[intPlayerFigure - 1].Name && SchachMatch.Schwarz.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 1;
                            SchachMatch.ResultatWeiß = 0;
                            SchachMatch.Schwarz.AddWin();
                            SchachMatch.Weiss.AddLose();
                            break;
                        }
                    }
                    break;
                case 3:
                    foreach (var SchachMatch in listOfSchachMatchingsPerPlayer[orderedPlayersByName[intSecondPlayerFigure - 1]])
                    {
                        if (SchachMatch.Schwarz.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name && SchachMatch.Weiss.Name == orderedPlayersByName[intPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 0.5M;
                            SchachMatch.ResultatWeiß = 0.5M;
                            SchachMatch.Schwarz.AddDraw();
                            SchachMatch.Weiss.AddDraw();
                            break;
                        }
                        if (SchachMatch.Weiss.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name && SchachMatch.Schwarz.Name == orderedPlayersByName[intPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 0.5M;
                            SchachMatch.ResultatWeiß = 0.5M;
                            SchachMatch.Schwarz.AddDraw();
                            SchachMatch.Weiss.AddDraw();
                            break;
                        }
                    }
                    foreach (var SchachMatch in listOfSchachMatchingsPerPlayer[orderedPlayersByName[intPlayerFigure - 1]])
                    {

                        if (SchachMatch.Schwarz.Name == orderedPlayersByName[intPlayerFigure - 1].Name && SchachMatch.Weiss.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 0.5M;
                            SchachMatch.ResultatWeiß = 0.5M;
                            SchachMatch.Schwarz.AddDraw();
                            SchachMatch.Weiss.AddDraw();
                            break;
                        }
                        if (SchachMatch.Weiss.Name == orderedPlayersByName[intPlayerFigure - 1].Name && SchachMatch.Schwarz.Name == orderedPlayersByName[intSecondPlayerFigure - 1].Name)
                        {
                            SchachMatch.ResultatSchwarz = 0.5M;
                            SchachMatch.ResultatWeiß = 0.5M;
                            SchachMatch.Schwarz.AddDraw();
                            SchachMatch.Weiss.AddDraw();
                            break;
                        }
                    }
                    break;
            }
        }

        private static void PrintMenu()
        {
            Console.WriteLine("\n+-------------+-------------+-------------+\n");
            Console.WriteLine("1 ... Show the Pairing");
            Console.WriteLine("2 ... Add Result");
            Console.WriteLine("3 ... Show current Results");
            Console.WriteLine("4 ... Create Predictions");
            Console.WriteLine("9 ... Exit");
            Console.WriteLine("\n+-------------+-------------+-------------+\n");

        }

        private static List<Player> GetPlayers()
        {
            var players = new List<Player>();
            players.Add(new Player() { Name = "Marcel", Elo = 1850 });
            players.Add(new Player() { Name = "Kevin", Elo = 900 });
            players.Add(new Player() { Name = "Nathalie", Elo = 1100 });
            players.Add(new Player() { Name = "Michael", Elo = 1000 });
            players.Add(new Player() { Name = "Kim", Elo = 1000 });
            players.Add(new Player() { Name = "Boris", Elo = 450 });
            players.Add(new Player() { Name = "Kama", Elo = 1400 });
            //players.Add(new Player() { Name = "Angelo", Elo = 350 });
            //players.Add(new Player() { Name = "Harpreet", Elo = 700 });
            //players.Add(new Player() { Name = "Oliver", Elo = 400 });
            //players.Add(new Player() { Name = "Pavle", Elo = 1400 });
            //players.Add(new Player() { Name = "Zeljko", Elo = 1100 });
            //players.Add(new Player() { Name = "Denise", Elo = 300 });
            //players.Add(new Player() { Name = "Benedikt", Elo = 800 });
            //players.Add(new Player() { Name = "Luca", Elo = 1200 });
            return players;
        }

        private static void DownloadAsCsv(List<List<Tuple<Player, Player>>> allRoundParings)
        {
            using (var writer = new StreamWriter($"C:\\Users\\marce\\Desktop\\TestExcel\\test_{DateTime.UtcNow.Ticks}.csv"))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";"}))
            {
                csv.WriteField("Round 0");
                csv.WriteField("Board 1");
                csv.WriteField("");
                csv.WriteField("Board 2");
                csv.WriteField("");
                if (BOARDS_TO_BE_PLAYED_ON == 3)
                {
                    csv.WriteField("Board 3");
                    csv.WriteField("");
                }
                csv.NextRecord();
                csv.WriteField("Round 0");
                csv.WriteField("Black");
                csv.WriteField("White");
                csv.WriteField("Black");
                csv.WriteField("White");
                if(BOARDS_TO_BE_PLAYED_ON == 3)
                {
                    csv.WriteField("Black");
                    csv.WriteField("White");
                }
                csv.NextRecord();

                // Write data
                for (int i = 0; i < allRoundParings.Count; i++)
                {
                    if(BOARDS_TO_BE_PLAYED_ON == 2)
                    {
                        for (int k = 0; k < allRoundParings[i].Count - 1; k += 2)
                        {
                            csv.WriteField($"Round {i + 1}");
                            csv.WriteField(allRoundParings[i][k].Item1.Name);
                            csv.WriteField(allRoundParings[i][k].Item2.Name);
                            csv.WriteField(allRoundParings[i][k + 1].Item1.Name);
                            csv.WriteField(allRoundParings[i][k + 1].Item2.Name);
                            csv.NextRecord();
                        }
                        if (allRoundParings[i].Count % 2 == 1)
                        {
                            csv.WriteField($"Round {i + 1}");
                            csv.WriteField(allRoundParings[i][allRoundParings[i].Count - 1].Item1.Name);
                            csv.WriteField(allRoundParings[i][allRoundParings[i].Count - 1].Item2.Name);
                            csv.NextRecord();

                        }
                    }
                    if(BOARDS_TO_BE_PLAYED_ON == 3)
                    {
                        for (int j = 0; j < allRoundParings[i].Count - 2; j += 3)
                        {
                            csv.WriteField($"Round {i + 1}");
                            csv.WriteField(allRoundParings[i][j].Item1.Name);
                            csv.WriteField(allRoundParings[i][j].Item2.Name);
                            csv.WriteField(allRoundParings[i][j + 1].Item1.Name);
                            csv.WriteField(allRoundParings[i][j + 1].Item2.Name);
                            csv.WriteField(allRoundParings[i][j + 2].Item1.Name);
                            csv.WriteField(allRoundParings[i][j + 2].Item2.Name);

                            csv.NextRecord();
                        }
                        if (allRoundParings[i].Count % 3 == 1)
                        {
                            csv.WriteField($"Round {i + 1}");
                            csv.WriteField(allRoundParings[i][allRoundParings[i].Count - 1].Item1.Name);
                            csv.WriteField(allRoundParings[i][allRoundParings[i].Count - 1].Item2.Name);
                            csv.NextRecord();

                        }
                        if (allRoundParings[i].Count % 3 == 2)
                        {
                            csv.WriteField($"Round {i + 1}");
                            csv.WriteField(allRoundParings[i][allRoundParings[i].Count - 2].Item1.Name);
                            csv.WriteField(allRoundParings[i][allRoundParings[i].Count - 2].Item2.Name);
                            csv.WriteField(allRoundParings[i][allRoundParings[i].Count - 1].Item1.Name);
                            csv.WriteField(allRoundParings[i][allRoundParings[i].Count - 1].Item2.Name);
                            csv.NextRecord();
                        }

                    }

                }
                writer.Flush();
            }
            
        }

        static void GiveStatistics(List<Player> players)
        {
            Console.WriteLine($"Statistiken zur Fairnessbewertung:");
            foreach (var item in players.OrderBy(x => x.Name))
            {
                Console.WriteLine($"{item.Name.PadRight(10)}:\tSchwarz {item.SchwarzGewesen} Weiß {item.WeißGewesen}");
            }

        }

        static void PrintTable(List<Player> players, List<List<Tuple<Player, Player>>> Rounds)
        {
            Console.WriteLine("\nErgbnis: ");
            if (BOARDS_TO_BE_PLAYED_ON == 2)
            {
                Console.WriteLine("+----------+-------------+-------------+-------------+-------------+");
                Console.WriteLine("| Round 0  |          Board 1          |           Board 2         |");
                Console.WriteLine("+----------+-------------+-------------+-------------+-------------+");
                Console.WriteLine("| Round 0  |    Black    |    White    |    Black    |    White    |");
                Console.WriteLine("+----------+-------------+-------------+-------------+-------------+");
                for (int i = 0; i < Rounds.Count; i++)
                {
                    for (int j = 0; j < Rounds[i].Count - 1; j += 2)
                    {
                        Console.WriteLine($"| Round {i + 1,-3}| {Rounds[i][j].Item1.Name,-12}| {Rounds[i][j].Item2.Name,-12}| {Rounds[i][j + 1].Item1.Name,-12}| {Rounds[i][j + 1].Item2.Name,-12}|");
                        Console.WriteLine("+----------+-------------+-------------+-------------+-------------+");
                    }
                    if (Rounds[i].Count % 2 == 1)
                    {
                        Console.WriteLine($"| Round {i + 1,-3}| {Rounds[i][Rounds[i].Count - 1].Item1.Name,-12}| {Rounds[i][Rounds[i].Count - 1].Item2.Name,-12}| {"",-12}| {"",-12}|");
                        Console.WriteLine("+----------+-------------+-------------+-------------+-------------+");
                    }
                }
            }
            else if (BOARDS_TO_BE_PLAYED_ON == 3)
            {
                Console.WriteLine("+----------+-------------+-------------+-------------+-------------+-------------+-------------+");
                Console.WriteLine("| Round 0  |          Board 1          |           Board 2         |           Board 3         |");
                Console.WriteLine("+----------+-------------+-------------+-------------+-------------+-------------+-------------+");
                Console.WriteLine("| Round 0  |    Black    |    White    |    Black    |    White    |    Black    |    White    |");
                Console.WriteLine("+----------+-------------+-------------+-------------+-------------+-------------+-------------+");
                for (int i = 0; i < Rounds.Count; i++)
                {
                    for (int j = 0; j < Rounds[i].Count - 2; j += 3)
                    {
                        Console.WriteLine($"| Round {i + 1,-3}| {Rounds[i][j].Item1.Name,-12}| {Rounds[i][j].Item2.Name,-12}| {Rounds[i][j + 1].Item1.Name,-12}| {Rounds[i][j + 1].Item2.Name,-12}| {Rounds[i][j + 2].Item1.Name,-12}| {Rounds[i][j + 2].Item2.Name,-12}|");
                        Console.WriteLine("+----------+-------------+-------------+-------------+-------------+-------------+-------------+");
                    }
                    if (Rounds[i].Count % 3 == 1)
                    {
                        Console.WriteLine($"| Round {i + 1,-3}| {Rounds[i][Rounds[i].Count - 1].Item1.Name,-12}| {Rounds[i][Rounds[i].Count - 1].Item2.Name,-12}| {"",-12}| {"",-12}| {"",-12}| {"",-12}|");
                        Console.WriteLine("+----------+-------------+-------------+-------------+-------------+-------------+-------------+");
                    }
                    if (Rounds[i].Count % 3 == 2)
                    {
                        Console.WriteLine($"| Round {i + 1,-3}| {Rounds[i][Rounds[i].Count - 2].Item1.Name,-12}| {Rounds[i][Rounds[i].Count - 2].Item2.Name,-12}| {Rounds[i][Rounds[i].Count - 1].Item1.Name,-12}| {Rounds[i][Rounds[i].Count - 1].Item2.Name,-12}| {"",-12}| {"",-12}|");
                        Console.WriteLine("+----------+-------------+-------------+-------------+-------------+-------------+-------------+");
                    }
                }
            }
        }

        static void Preparation(out List<Player> contenders)
        {
            contenders = new List<Player>();

            Console.WriteLine("How many players are going to be playing today \n");
            var strPlayerCount = Console.ReadLine();

            int.TryParse(strPlayerCount, out int playerCount);

            if (playerCount <= 0)
            {
                Preparation(out List<Player> players);
                contenders = players;
            }

            Console.WriteLine("Whats the first players name \n");
            var firstname = Console.ReadLine();

            contenders.Add(new Player { Name = firstname });

            Console.WriteLine($"Successfully added Player {firstname} \n");

            for (int i = 1; i < playerCount; i++)
            {
                Console.WriteLine("Whats the next players name \n");
                var name = Console.ReadLine();
                contenders.Add(new Player { Name = name });
                Console.WriteLine($"Successfully added Player {name} \n");
            }

        }

        static int uniqueCombinations(List<Player> players)
        {
            // C = n! / r! (n - r)!
            //   = 7 players / 2 subsets times () 

            int n = 1;
            for (int i = players.Count; i > 0; i--)
                n *= i;

            int r = 1;
            for (int i = 2; i > 0; i--)
                r *= i;

            int nMinusR = 1;
            for (int i = players.Count - 2; i > 0; i--)
                r *= i;


            var erg = n / (r * nMinusR);

            return erg;
        }

        static int Combinations(List<Player> players)
        {
            // C = n! / (n - r)!
            //   = 7 players / 2 subsets times () 

            int n = 1;
            for (int i = players.Count; i > 0; i--)
                n *= i;


            int nMinusR = 1;
            for (int i = players.Count - 2; i > 0; i--)
                nMinusR *= i;


            var erg = n / nMinusR;

            return erg;
        }

        static void PermutationsCombinationsCalculation(List<Player> players)
        {
            var uniqueCombinationsErg = uniqueCombinations(players);
            var combinationsErg = Combinations(players);
            Console.WriteLine($"Player Count: {players.Count} Combinations: {uniqueCombinationsErg} Permutations: {combinationsErg}");
        }

        static List<List<Tuple<Player, Player>>> GetAllRoundPairings(List<Player> players)
        {
            int versuch = 1;

            var playerCopy = new List<Player>(players.Count);

            if (players.Count % 2 == 1)
            {
                players.Add(new Player() { Name = "-----", Elo = 0 });
            }
            var statistics = new StatistcsObject(players);

            var unfairDivision = true;

            var AllRoundParings = new List<List<Tuple<Player, Player>>>();

            while (unfairDivision)
            {
                Console.WriteLine($"Versuch Nr.: {versuch}");

                AllRoundParings = new List<List<Tuple<Player, Player>>>();
                statistics = new StatistcsObject(players);
                statistics.InitAllPlayersSeiten();

                var unterVersuche = 0;

                for (int j = 0; j < players.Count - 1; j++, unterVersuche++)
                {
                    var roundParings = new List<Tuple<Player, Player>>();

                    Random rng = new();
                    var shuffledPlayers = players.OrderBy(_ => rng.Next()).ToList();
                    shuffledPlayers.ForEach((item) =>
                    {
                        playerCopy.Add(new Player(item));
                    });

                    for (int i = 0; i < players.Count / 2; i++)
                    {

                        Random rnd = new Random();
                        int position = rnd.Next(0, playerCopy.Count / 2);
                        int secondPosition = (playerCopy.Count - 1) - position;

                        var blackPlayer = playerCopy[position];
                        var whitePLayer = playerCopy[secondPosition];
                        roundParings.Add(new Tuple<Player, Player>(blackPlayer, whitePLayer));


                        playerCopy.Remove(playerCopy[secondPosition]);
                        playerCopy.Remove(playerCopy[position]);

                    }

                    bool paringsAreUnique = true;

                    if (AllRoundParings.Count > 0)
                    {

                        for (int i = 0; i < roundParings.Count; i++)
                        {
                            Tuple<Player, Player> pairing = roundParings[i];
                            if (!paringsAreUnique)
                            {
                                break;
                            }

                            for (int i1 = 0; i1 < AllRoundParings.Count; i1++)
                            {
                                List<Tuple<Player, Player>> round = AllRoundParings[i1];
                                if (!paringsAreUnique)
                                {
                                    break;
                                }

                                for (int i2 = 0; i2 < round.Count; i2++)
                                {
                                    Tuple<Player, Player> prePairing = round[i2];
                                    if ((pairing.Item2.Name == prePairing.Item2.Name && pairing.Item1.Name == prePairing.Item1.Name)
                                        || pairing.Item2.Name == prePairing.Item1.Name && prePairing.Item2.Name == pairing.Item1.Name)
                                    {
                                        paringsAreUnique = false;
                                        break;
                                    }
                                }


                            }

                        }
                    }

                    if (paringsAreUnique)
                    {

                        Tuple<Player, Player> thelastPairing;

                        foreach (var pairing in roundParings)
                        {
                            if (pairing.Item2.Name == "-----" || pairing.Item1.Name == "-----")
                            {
                                thelastPairing = new Tuple<Player, Player>(pairing.Item1, pairing.Item2);
                                roundParings.Remove(pairing);
                                roundParings.Add(thelastPairing);
                                break;
                            }
                        }

                        AllRoundParings.Add(roundParings);
                        statistics.AddAllStanding(roundParings);
                    }
                    else
                    {
                        j--;
                    }

                    if (unterVersuche == MAX_TRIES_TO_GET_LUCKY) // unlucky weil 
                    {
                        unfairDivision = true;
                        versuch++;
                        Console.WriteLine("Keine ganzen Runden mehr zu vervollständigen. Nächster Versuch"); ;
                        break;
                    }
                }

                if (unterVersuche < MAX_TRIES_TO_GET_LUCKY)
                {
                    foreach (var player in statistics.playerList)
                    {

                        if (player.SchwarzGewesen > ((statistics.playerList.Count - 1) / 2) + AUFTEILUNG_SCHWARZ_WEISS_FAIRNESS_PARAM
                            || player.WeißGewesen < ((statistics.playerList.Count - 1) / 2) - AUFTEILUNG_SCHWARZ_WEISS_FAIRNESS_PARAM)
                        {
                            Console.WriteLine($"Leider unfair: Spieler {player.Name} hätte {player.SchwarzGewesen} mal Schwarz und nur {player.WeißGewesen} mal Weiß gespielt");
                            unfairDivision = true;
                            versuch++;
                            break;
                        }
                        else
                        {
                            unfairDivision = false;
                        }

                    }
                }
            }

            return AllRoundParings;
        }

        static void Introduction()
        {
            Console.WriteLine("Welcome to The Round Robin Simulator \nEin Jeder gegen Jeden Schach Format \n");


            Console.WriteLine("+----------+-------------+-------------+-------------+-------------+\n");


            Console.WriteLine("For reasons of faster SchachMatching we are trying to have an equal distribution of black an white games \nfor every player, therefore its not always possible to find the best SchachMatchings and an outcome is accepted if \n" +
                "for example 8 games are gonna be played and a player will be black 3 times and 5 times white. \n");

            Console.WriteLine("+----------+-------------+-------------+-------------+-------------+\n");
        }
    }



   
}