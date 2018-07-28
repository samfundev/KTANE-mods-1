using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.RuleGenerator
{
    public partial class MorseAMazeSwapRuleGenerator : AbstractRuleGenerator
    {
        public static MorseAMazeSwapRuleGenerator Instance { get { return (MorseAMazeSwapRuleGenerator) GetInstance<MorseAMazeSwapRuleGenerator>(); } }

        public override string GetModuleType()
        {
            return "MorseAMazeSwap";
        }

        public override string GetHTMLManual(out string filename)
        {
            filename = "Morse-A-Maze-Swap.html";
            if (!Initialized)
                throw new Exception("You must Initialize the Random number generator first.");
            if (!RulesGenerated)
                throw new Exception("You must genertate the rules first");

           

            var manual = Manual.Replace("VANILLARULEGENERATORSEED", Seed.ToString());
            for (var i = 17; i >= 0; i--)
            {
                if (Seed == 1 || Seed == -1)
                {
                    //Remove this code once the challenge has ended.
                    manual = manual.Replace(string.Format("MAZEWORD{0:00}", i + 1), string.Format("{0} - {1}<br>{2}", i, "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;", BlankMaze));
                    manual = manual.Replace(string.Format("EDGEWORKWORD{0:00}", i + 1), "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                }
                else
                {
                    manual = manual.Replace(string.Format("MAZEWORD{0:00}", i + 1), string.Format("{0} - {1}<br>{2}", i, Words[i].ToUpperInvariant(), Mazes[i].ToSVG()));
                    manual = manual.Replace(string.Format("EDGEWORKWORD{0:00}", i + 1), Words[i + 18].ToUpperInvariant());
                }
            }
            
            return manual;
        }

        public override string[] GetTextFiles(out string[] textFilePaths)
        {
            textFilePaths = TextAssetPaths;
            return TextAssets;
        }

        public override void CreateRules()
        {
            if (!Initialized)
                throw new Exception("You must initialize the Random number generator first");

            Mazes.Clear();
            Words.Clear();

            int numberToBurn;
            switch (Seed)
            {
                case 1:
                    numberToBurn = 9;
                    break;
                case 2:
                case -2:
                    numberToBurn = 27;
                    break;
                default:
                    numberToBurn = 18;
                    break;
            }
            for (int i = 0; i < numberToBurn; i++)
            {
                new Maze().BuildMaze(NextMinMax);
            }

            for (int i = 0; i < 18; i++)
            {
                var maze = new Maze();
                Mazes.Add(maze);
                maze.BuildMaze(NextMinMax);
            }
            
            string words = "bottle,brain,button,camera,charge,coffee,";
            words += "death,decoy,device,doctor,energy,flunk,";
            words += "handle,india,jewel,joule,lawyer,local,";
            words += "memory,mirror,module,murder,place,police,";
            words += "policy,prime,quick,react,rubiks,snake,";
            words += "thrown,wealth,where,whoops,xerox,zilch";
            Words = words.Split(',').OrderBy(x => NextDouble()).ToList();

            RulesGenerated = true;
        }

        public List<Maze> Mazes = new List<Maze>();
        public List<string> Words = new List<string>();
    }
}