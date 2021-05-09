using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RLNET;
using RogueSharp;
using Projet.Core;
using Projet.Interfaces;
using System.Threading;

namespace Projet.Systems
{
    public class MessageLog
    {
        // Define the maximum number of lines to store
        private static int _maxLines;

        // Use a Queue to keep track of the lines of text
        // The first line added to the log will also be the first removed
        private readonly Queue<string> _lines;

        private string[] _asyncLines;

        public MessageLog()
        {
            _lines = new Queue<string>();
        }
        public MessageLog(int maxLines) : this()
        {
            _maxLines = maxLines;
        }

        // Add a line to the MessageLog queue
        public void Add(string message)
        {
            _lines.Enqueue(message);

            // When exceeding the maximum number of lines remove the oldest one.
            if (_lines.Count > _maxLines)
            {
                _lines.Dequeue();
            }
        }

        private void AsyncAddMessages()
        {
            foreach(string line in _asyncLines)
            {
                Add(line);
                Thread.Sleep(5000);
            }
            _asyncLines = null;
        }

        public void Add(string[] messages)
        {
            Thread thread = new Thread(AsyncAddMessages);
            thread.IsBackground = true;
            _asyncLines = messages;
            thread.Start();
        }

        // Draw each line of the MessageLog queue to the console
        public void Draw(RLConsole console)
        {
            string[] lines = _lines.ToArray();
            for (int i = 0; i < lines.Length; i++)
            {
                console.Print(1, i + 1, lines[i], RLColor.Black);
            }
        }
    }
}
