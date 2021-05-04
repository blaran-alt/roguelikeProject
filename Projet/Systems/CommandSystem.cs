using System;
using RLNET;
using RogueSharp.DiceNotation;
using Projet.Core;
using System.Text;
using Projet.Interfaces;
using RogueSharp;
using Projet.Items;


namespace Projet.Systems
{
    public class CommandSystem
    {
        // Return value is true if the player was able to move
        // false when the player couldn't move, such as trying to move into a wall
        public bool MovePlayer(Direction direction)
        {
            int x = Game.Player.X;
            int y = Game.Player.Y;

            switch (direction)
            {
                case Direction.Up:
                    {
                        y = Game.Player.Y - 1;
                        break;
                    }
                case Direction.Down:
                    {
                        y = Game.Player.Y + 1;
                        break;
                    }
                case Direction.Left:
                    {
                        x = Game.Player.X - 1;
                        break;
                    }
                case Direction.Right:
                    {
                        x = Game.Player.X + 1;
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            Box box = Game.Map.GetBoxAt(x, y);
            if(box != null)
            {
                Point _direction = new Point(x, y) - Game.Player.Coord;
                Game.Map.SetActorPosition(box, x + _direction.X, y + _direction.Y);
            }

            if (Game.Map.SetActorPosition(Game.Player, x, y))
            {
                Item item = Game.Map.GetItemAt(x, y);
                if(item != null)
                {
                    if (Game.Inventory.PickUp(item))
                    {
                        Game.MessageLog.Add($"{Game.Player.Name} picked up {item.Quantity} {item.Name}");
                        Game.Map.RemoveItem(item);
                    }
                    else
                    {
                        Game.MessageLog.Add($"You can't pick up this {item.Name}, your inventory must be full");
                    }
                }
                return true;
            }

            Monster monster = Game.Map.GetMonsterAt(x, y);
            if (monster != null)
            {
                Attack(Game.Player, monster);
                return true;
            }

            return false;
        }

        public bool IsPlayerTurn { get; set; }

        public void EndPlayerTurn()
        {
            IsPlayerTurn = false;
        }

        private int speedBoostTimer = 5;

        public void ActivateMonsters()
        {
            IScheduleable scheduleable = Game.SchedulingSystem.Get();
            if (scheduleable is Player)
            {
                IsPlayerTurn = true;
                Game.SchedulingSystem.Add(Game.Player);
                Player player = scheduleable as Player;
                if(player.Speed == 5)
                {
                    if(--speedBoostTimer == 0)
                    {
                        player.Speed = 10;
                        speedBoostTimer = 5;
                    }
                }
            }
            else
            {
                Monster monster = scheduleable as Monster;
                if (monster != null)
                {
                    monster.PerformAction(this);
                    Game.SchedulingSystem.Add(monster);
                }

                ActivateMonsters();
            }
        }

        public void MoveMonster(Monster monster, ICell cell)
        {
            if (!Game.Map.SetActorPosition(monster, cell.X, cell.Y))
            {
                if (Game.Player.X == cell.X && Game.Player.Y == cell.Y)
                {
                    Attack(monster, Game.Player);
                }
            }
        }

        public void Attack(Actor attacker, Actor defender)
        {
            StringBuilder attackMessage = new StringBuilder();
            StringBuilder defenseMessage = new StringBuilder();

            int hits = ResolveAttack(attacker, defender, attackMessage);

            int blocks = ResolveDefense(defender, hits, attackMessage, defenseMessage);

            Game.MessageLog.Add(attackMessage.ToString());
            if (!string.IsNullOrWhiteSpace(defenseMessage.ToString()))
            {
                Game.MessageLog.Add(defenseMessage.ToString());
            }

            int damage = hits - blocks;

            ResolveDamage(defender, damage);
        }

        // The attacker rolls based on his stats to see if he gets any hits
        private int ResolveAttack(Actor attacker, Actor defender, StringBuilder attackMessage)
        {
            int hits = 0;

            attackMessage.AppendFormat("{0} attacks {1} and rolls: ", attacker.Name, defender.Name);

            // Roll a number of 100-sided dice equal to the Attack value of the attacking actor
            DiceExpression attackDice = new DiceExpression().Dice(attacker.Attack, 100);
            DiceResult attackResult = attackDice.Roll();

            // Look at the face value of each single die that was rolled
            foreach (TermResult termResult in attackResult.Results)
            {
                attackMessage.Append(termResult.Value + ", ");
                // Compare the value to 100 minus the attack chance and add a hit if it's greater
                if (termResult.Value >= 100 - attacker.AttackChance)
                {
                    hits++;
                }
            }

            return hits;
        }

        // The defender rolls based on his stats to see if he blocks any of the hits from the attacker
        private  int ResolveDefense(Actor defender, int hits, StringBuilder attackMessage, StringBuilder defenseMessage)
        {
            int blocks = 0;

            if (hits > 0)
            {
                attackMessage.AppendFormat("scoring {0} hits.", hits);
                defenseMessage.AppendFormat("  {0} defends and rolls: ", defender.Name);

                // Roll a number of 100-sided dice equal to the Defense value of the defendering actor
                DiceExpression defenseDice = new DiceExpression().Dice(defender.Defense, 100);
                DiceResult defenseRoll = defenseDice.Roll();

                // Look at the face value of each single die that was rolled
                foreach (TermResult termResult in defenseRoll.Results)
                {
                    defenseMessage.Append(termResult.Value + ", ");
                    // Compare the value to 100 minus the defense chance and add a block if it's greater
                    if (termResult.Value >= 100 - defender.DefenseChance)
                    {
                        blocks++;
                    }
                }
                defenseMessage.AppendFormat("resulting in {0} blocks.", blocks);
            }
            else
            {
                attackMessage.Append("and misses completely.");
            }

            return blocks;
        }

        // Apply any damage that wasn't blocked to the defender
        public void ResolveDamage(Actor defender, int damage)
        {
            if (damage > 0)
            {
                defender.Health -= damage;

                Game.MessageLog.Add($"  {defender.Name} was hit for {damage} damage");

                if (defender.Health <= 0)
                {
                    defender.Health = 0;
                    ResolveDeath(defender);
                }
            }
            else
            {
                Game.MessageLog.Add($"  {defender.Name} blocked all damage");
            }
        }

        // Remove the defender from the map and add some messages upon death.
        private void ResolveDeath(Actor defender)
        {
            if (defender is Player)
            {
                Game.MessageLog.Add($"  {defender.Name} was killed, GAME OVER MAN!");
                Game.GameOver();
            }
            else if (defender is Monster)
            {
                Game.Map.RemoveMonster((Monster)defender);
                Gold gold = new Gold(defender.Gold, defender.X, defender.Y);
                Game.Map.AddItem(gold);
                Game.MessageLog.Add($"  {defender.Name} died and dropped {defender.Gold} gold");
            }
        }
    }
}
