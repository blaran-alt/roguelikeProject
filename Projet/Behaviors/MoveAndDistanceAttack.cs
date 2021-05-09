using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet.Interfaces;
using RogueSharp;
using RLNET;
using Projet.Core;
using Projet.Systems;

namespace Projet.Behaviors
{
    public class MoveAndDistanceAttack : IBehavior
    {
        public bool Act(Monster monster, CommandSystem commandSystem)
        {
            GameMap map = Game.Map;
            Player player = Game.Player;
            FieldOfView monsterFov = new FieldOfView(map);

            // If the monster has not been alerted, compute a field-of-view 
            // Use the monster's Awareness value for the distance in the FoV check
            // If the player is in the monster's FoV then alert it
            // Add a message to the MessageLog regarding this alerted status
            if (!monster.TurnsAlerted.HasValue)
            {
                monsterFov.ComputeFov(monster.X, monster.Y, monster.Awareness, true);
                if (monsterFov.IsInFov(player.X, player.Y))
                {
                    Game.MessageLog.Add($"{monster.Name} a envie de combattre {player.Name}");
                    monster.TurnsAlerted = 1;
                }
            }

            if (monster.TurnsAlerted.HasValue)
            {
                monsterFov.ComputeFov(monster.X, monster.Y, monster.Awareness, true);

                int deltaX = player.X - monster.X;
                int deltaY = player.Y - monster.Y;
                if (Math.Abs(deltaX) == 1 && Math.Abs(deltaY)  == 1)
                {
                    commandSystem.MoveMonster(monster, map.GetCell(monster.X - deltaX, monster.Y - deltaY));
                    return true;
                }

                if(map.IsInFov(player.X, player.Y) && Math.Abs(deltaX) <= 3 && Math.Abs(deltaY) <= 3)
                {
                    commandSystem.Attack(monster, player);
                    return true;
                }

                // Before we find a path, make sure to make the monster and player Cells walkable
                map.SetIsWalkable(monster.X, monster.Y, true);
                map.SetIsWalkable(player.X, player.Y, true);

                PathFinder pathFinder = new PathFinder(map);
                Path path = null;

                bool pathFound = true;
                try
                {
                    path = pathFinder.ShortestPath(map.GetCell(monster.X, monster.Y), map.GetCell(player.X, player.Y));
                }
                catch (PathNotFoundException)
                {
                    // The monster can see the player, but cannot find a path to him
                    // This could be due to other monsters blocking the way
                    pathFound = false;
                }
                if (!pathFound)
                {
                    // Add a message to the message log that the monster is waiting
                    Game.MessageLog.Add($"{monster.Name} attend le prochain tour");
                }
                // Don't forget to set the walkable status back to false
                map.SetIsWalkable(monster.X, monster.Y, false);
                map.SetIsWalkable(player.X, player.Y, false);

                // In the case that there was a path, tell the CommandSystem to move the monster
                if (path != null)
                {
                    try
                    {
                        // Take the first step of the path
                        commandSystem.MoveMonster(monster, path.StepForward());
                    }
                    catch (NoMoreStepsException)
                    {
                        Game.MessageLog.Add($"{monster.Name} growls in frustration");
                    }
                }

                monster.TurnsAlerted++;

                // Lose alerted status every 15 turns. 
                // As long as the player is still in FoV the monster will stay alert
                // Otherwise the monster will quit chasing the player.
                if (monster.TurnsAlerted > 15)
                {
                    monster.TurnsAlerted = null;
                }
            }
            return true;
        }
    }
}
