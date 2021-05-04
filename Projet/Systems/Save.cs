using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RLNET;
using RogueSharp;
using Projet.Core;

namespace Projet.Systems
{
    public class Save
    {
        public string Name { get; set; }
        public List<int> Seeds { get; set; }
        public int NbOfLevels
        {
            get
            {
                return Seeds.Count();
            }
        }
        public Save()
        {
            Name = "Test";
            Seeds = new List<int>();
        }
        public Save(string name) : this()
        {
            using(StreamReader streamReader = new StreamReader("Saves.txt"))
            {
                Name = name;
                string line;
                bool loadLevels = false;
                while((line = streamReader.ReadLine()) != null)
                {
                    if(!loadLevels && line[0] == '.' && line.Substring(1) == Name)
                    {
                        loadLevels = true;
                    }
                    else if (loadLevels)
                    {
                        if(line[0] == '.')
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine(line);
                            Seeds.Add(int.Parse(line));
                        }
                    }
                }
            }
        }

        public void SaveInFile()
        {
            using (StreamWriter streamWriter = File.AppendText("Saves.txt"))
            {
                streamWriter.WriteLine("." + Name);
                foreach (int seed in Seeds)
                {
                    streamWriter.WriteLine(seed);
                }
            }
        }

        public void Draw(RLConsole console)
        {
            console.Print(10, 20, Name, Colors.TextHeading);
            for (int i = 0; i < Seeds.Count(); i++)
            {
                console.Print(20, 30 + i, $"Level {i + 1} : seed = {Seeds[i]}", Colors.Text);
            }
        }
    }
}
