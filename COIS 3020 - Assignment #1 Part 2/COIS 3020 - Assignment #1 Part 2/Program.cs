using System;
using System.Collections.Generic;

public interface IBoardGame
{
    int FindMinimumRolls();
    void PrintResult();
}

// Board class to represent the Snakes and Ladders board
public class Board
{
    private int size;
    private Dictionary<int, int> transitions;
    private Random random;

    public Board(int boardSize)
    {
        size = boardSize;
        transitions = new Dictionary<int, int>();
        random = new Random();
    }

    // Adds a ladder or snake (mapping from one square to another)
    public void AddTransition(int start, int end)
    {
        transitions[start] = end;
    }

    // Returns the position after considering snakes/ladders
    public int GetNewPosition(int position)
    {
        return transitions.ContainsKey(position) ? transitions[position] : position;
    }

    public int GetSize()
    {
        return size;
    }

    // Randomly generates ladders on the board
    public void GenerateLadders(int numLadders)
    {
        Console.WriteLine("\nGenerated Ladders:");
        for (int i = 0; i < numLadders; i++)
        {
            int start, end;
            do
            {
                start = random.Next(2, size - 1);  // Avoid square 1 & last square
                end = random.Next(start + 1, size); // Ensure ladder goes up
            } while (transitions.ContainsKey(start)); // Avoid duplicates

            AddTransition(start, end);
            Console.WriteLine($"Ladder: {start} --> {end}");
        }
    }

    // Randomly generates snakes on the board
    public void GenerateSnakes(int numSnakes)
    {
        Console.WriteLine("\nGenerated Snakes:");
        for (int i = 0; i < numSnakes; i++)
        {
            int head, tail;
            do
            {
                head = random.Next(2, size);  // Avoid square 1 & last square
                tail = random.Next(1, head); // Ensure snake goes down
            } while (transitions.ContainsKey(head)); // Avoid duplicates

            AddTransition(head, tail);
            Console.WriteLine($"Snake: {head} --> {tail}");
        }
    }
}

// Game class implementing the BFS search for minimum dice rolls
public class Game : IBoardGame
{
    private Board board;
    private Dictionary<int, int> visited;
    private List<int> moveSequence;

    public Game(Board gameBoard)
    {
        board = gameBoard;
        visited = new Dictionary<int, int>();
        moveSequence = new List<int>();
    }

    // Uses BFS to find the shortest path to the end
    public int FindMinimumRolls()
    {
        int boardSize = board.GetSize();
        Queue<(int position, int rolls, List<int> path)> queue = new Queue<(int, int, List<int>)>();
        queue.Enqueue((1, 0, new List<int>()));

        visited[1] = 0;

        while (queue.Count > 0)
        {
            var (currentSquare, currentRolls, path) = queue.Dequeue();

            // If we reached the final square, store the path and return rolls
            if (currentSquare == boardSize)
            {
                moveSequence = new List<int>(path);
                return currentRolls;
            }

            // Try rolling the dice (1-6)
            for (int dice = 1; dice <= 6; dice++)
            {
                int nextSquare = currentSquare + dice;

                // If moving beyond the board, stay in place
                if (nextSquare > boardSize)
                    continue;

                nextSquare = board.GetNewPosition(nextSquare);

                if (!visited.ContainsKey(nextSquare))
                {
                    visited[nextSquare] = currentRolls + 1;
                    List<int> newPath = new List<int>(path) { dice };
                    queue.Enqueue((nextSquare, currentRolls + 1, newPath));
                }
            }
        }

        return -1; // No solution found
    }

    // Prints the shortest sequence of dice rolls
    public void PrintResult()
    {
        if (moveSequence.Count > 0)
        {
            Console.WriteLine("\nMinimum number of rolls: " + moveSequence.Count);
            Console.WriteLine("Dice sequence: " + string.Join(", ", moveSequence));
        }
    }
}

// Main program with user input handling
class Program
{
    static void Main()
    {
        Console.Write("Enter the board size: ");
        int boardSize = int.Parse(Console.ReadLine());
        while (boardSize <= 1)
        {
            Console.WriteLine("Invalid amount");
            Console.Write("Enter the board size: ");
            boardSize = int.Parse(Console.ReadLine());
        }
        // Creating a board
        Board board = new Board(boardSize);

        // Getting number of ladders and snakes
        Console.Write("Enter the number of ladders: ");
        int numLadders = int.Parse(Console.ReadLine());
        Console.Write("Enter the number of snakes: ");
        int numSnakes = int.Parse(Console.ReadLine());
        while ((numLadders + numSnakes) > (boardSize / 2))
        {
            Console.WriteLine("Invalid amount of snakes and ladders");
            Console.Write("Enter the number of ladders: ");
            numLadders = int.Parse(Console.ReadLine());
            Console.Write("Enter the number of snakes: ");
            numSnakes = int.Parse(Console.ReadLine());
        }

        // Generate random ladders and snakes
        board.GenerateLadders(numLadders);
        board.GenerateSnakes(numSnakes);

        // Running the game
        Game game = new Game(board);
        int result = game.FindMinimumRolls();
        game.PrintResult();
    }
}

