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
            Name = "Unnamed";
            Seeds = new List<int>();
        }
        public Save(int saveIndex, out bool last) : this()
        {
            using(StreamReader streamReader = new StreamReader("Saves.txt"))
            {
                string line;
                bool loadLevels = false;
                int i = 0;
                last = true;
                while((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                    {
                        continue;
                    }
                    if(!loadLevels && line[0] == '.')
                    {
                        if(i == saveIndex)
                        {
                            last = false;
                            loadLevels = true;
                            Name = line.Substring(1);
                        }
                        i++;
                    }
                    else if (loadLevels)
                    {
                        if(line[0] == '.')
                        {
                            break;
                        }
                        else
                        {
                            Seeds.Add(int.Parse(line));
                        }
                    }
                }
            }
        }

        public void SaveInFile(string name)
        {
            Name = name;
            using (StreamWriter streamWriter = File.AppendText("Saves.txt"))
            {
                streamWriter.WriteLine("\n." + Name);
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
