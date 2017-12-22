using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Assets.Scripts.RuleGenerator
{
    public class MorseAMazeRuleGenerator : AbstractRuleGenerator
    {
        public override string GetHTMLManual(out string filename)
        {
            filename = "Morse-A-Maze.html";
            if (!Initialized)
                throw new Exception("You must Initialize the Random number generator first.");
            if (!RulesGenerated)
                throw new Exception("You must genertate the rules first");

            var mazenames = new[]
            {
                "KABOOMMAZE","UNICORNMAZE","QUEBECMAZE",
                "BASHLYMAZE","SLICKMAZE","VECTORMAZE",
                "FLICKMAZE","TIMWIMAZE","STROBEMAZE",
                "BOMBSMAZE","BRAVOMAZE","LAUNDRYMAZE",
                "BRICKMAZE","KITTYMAZE","HALLSMAZE",
                "STEAKMAZE","BREAKMAZE","BEATSMAZE"
            };
            var manual = new MorseAMazeManual().Manual.Replace("VANILLARULEGENERATORSEED", Seed.ToString());
            for (var i = 0; i < 18; i++)
            {
                manual = manual.Replace(mazenames[i], Mazes[i].ToSVG());
            }
            
            return manual;
        }

        public override string[] GetTextFiles(out string[] textFilePaths)
        {
            textFilePaths = MorseAMazeManual.TextAssetPaths;
            return MorseAMazeManual.TextAssets;
        }

        public override void CreateRules()
        {
            if (!Initialized)
                throw new Exception("You must initialize the Random number generator first");

            Mazes.Clear();
            switch (Seed)
            {
                case 1:
                    for (var i = 0; i < 9; i++)
                    {
                        var maze = new Maze();
                        Mazes.Add(maze);
                        maze.BuildMaze(NextMinMax);
                    }
                    InitializeRNG(2);
                    Seed = 1;
                    var list = new List<Maze>();
                    for (var i = 0; i < 9; i++)
                    {
                        var maze = new Maze();
                        list.Add(maze);
                        maze.BuildMaze(NextMinMax);
                    }
                    Mazes.Add(list[2]);
                    Mazes.Add(list[3]);
                    Mazes.Add(list[8]);
                    Mazes.Add(list[6]);
                    Mazes.Add(list[1]);
                    Mazes.Add(list[0]);
                    Mazes.Add(list[4]);
                    Mazes.Add(list[5]);
                    Mazes.Add(list[7]);
                    break;
                case 2:
                    for (var i = 0; i < 9; i++)
                    {
                        new Maze().BuildMaze(NextMinMax); //Burn the first 9 mazes out of seed 2. Seed 1 ruleset already used them.
                    }
                    for (var i = 0; i < 18; i++)
                    {
                        var maze = new Maze();
                        Mazes.Add(maze);
                        maze.BuildMaze(NextMinMax);
                    }
                    break;
                default:
                    for (var i = 0; i < 18; i++)
                    {
                        var maze = new Maze();
                        Mazes.Add(maze);
                        maze.BuildMaze(NextMinMax);
                    }
                    break;
            }
            RulesGenerated = true;
        }

        public List<Maze> Mazes = new List<Maze>();
    }
}