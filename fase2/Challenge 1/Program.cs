using System;
using System.Collections.Generic;

namespace DesafioStone
{
    internal class Program
    {
        static void findPath()
        {
            Board board = new Board();
            Node startNode = new Node();
            List<Movement> possibleMoves = new();
            possibleMoves.Add(new Movement('U', -1, 0));
            possibleMoves.Add(new Movement('D', 1, 0));
            possibleMoves.Add(new Movement('L', 0, -1));
            possibleMoves.Add(new Movement('R', 0, 1));

            startNode.H = board.HeuristicsGrid[startNode.Row][startNode.Column];

            bool foundSolution = false;

            PriorityQueue<Node, int> openSet = new PriorityQueue<Node, int>();
            openSet.Enqueue(startNode, startNode.H + startNode.Turn);
            Node closestNode = startNode;

            HashSet<string> closedSet = new HashSet<string>();

            var watch = System.Diagnostics.Stopwatch.StartNew();

            System.Console.WriteLine("Finding path...");

            while (openSet.Count > 0 && !foundSolution)
            {
                Node selectedNode;

                selectedNode = openSet.Dequeue();

                // Generate new propagations on demand
                if (selectedNode.Turn >= board.LastTurnGenerated)
                {
                    System.Console.WriteLine("Cleaning memory...");
                    // Clear memory
                    // Remove boards that will no longer be used
                    var topElement = openSet.Peek();

                    var minTurn = openSet.UnorderedItems.MinBy(x => x.Element.Turn).Element.Turn;
                    // var minTurn = topElement.Turn / 2;

                    board.RemoveUntilTurn(minTurn - 1);
                    closedSet.RemoveWhere(el => int.Parse(el.Split("t")[1]) <= minTurn);

                    // var filteredItems = openSet.UnorderedItems.ToList().Where(el => el.Element.Turn > minTurn).ToList();
                    // openSet.Clear();
                    // foreach (var filteredItem in filteredItems)
                    // {
                    //     openSet.Enqueue(filteredItem.Element, filteredItem.Element.H + filteredItem.Element.Turn);
                    // }

                    board.GetNextPropagations(200);

                    closestNode.WriteNodePathFile("closest.txt");

                    System.Console.WriteLine($"{watch.ElapsedMilliseconds}ms running");
                    System.Console.WriteLine($"Last node: R{topElement.Row} C{topElement.Column} H{topElement.H}");
                    System.Console.WriteLine($"Closest node: R{closestNode.Row} C{closestNode.Column} H{closestNode.H}");
                    System.Console.WriteLine($"Open set: {openSet.Count} \nClosed set: {closedSet.Count}");
                    System.Console.WriteLine("Finding path...");
                }

                var nextNodes = selectedNode.getNextNodes(board, possibleMoves);

                foreach (Node nextNode in nextNodes)
                {
                    bool alreadyVisited = closedSet.Contains(nextNode.ToString());

                    if (alreadyVisited) continue;

                    if (board.GridList[nextNode.Turn][nextNode.Row][nextNode.Column] == 4)
                    {
                        watch.Stop();
                        foundSolution = true;
                        System.Console.WriteLine("Solution found!!");
                        System.Console.WriteLine($"Turn {nextNode.Turn}");
                        System.Console.WriteLine(watch.ElapsedMilliseconds + "ms");

                        nextNode.WriteNodePathFile("solution.txt");

                        break;
                    }
                    if (nextNode.H < closestNode.H)
                        closestNode = nextNode;
                    openSet.Enqueue(nextNode, nextNode.H + nextNode.Turn);
                    closedSet.Add(nextNode.ToString());
                }
            }
        }

        //TODO: Solution tester
        static void Main(string[] args)
        {
            try
            {
                findPath();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }
}
