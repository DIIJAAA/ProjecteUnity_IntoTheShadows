using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Range(5, 500)]
    public int mazeWidth = 10, mazeHeight = 10;
    public int startX = 0, startY = 0;
    public MazeCell[,] maze;
    
    private Vector2Int currentCell;
    private List<Direction> directions = new List<Direction>()
    {
        Direction.Up, Direction.Down, Direction.Left, Direction.Right
    };

    public MazeCell[,] GetMaze()
    {
        maze = new MazeCell[mazeWidth, mazeHeight];
        
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                maze[x, y] = new MazeCell(x, y);
            }
        }
        
        CarvedPath(startX, startY);
        return maze;
    }

    private List<Direction> GetRandomDirections()
    {
        List<Direction> dir = new List<Direction>(directions);
        List<Direction> randDir = new List<Direction>();

        while (dir.Count > 0)
        {
            int rnd = Random.Range(0, dir.Count);
            randDir.Add(dir[rnd]);
            dir.RemoveAt(rnd);
        }
        return randDir;
    }

    private bool IsCellValid(int x, int y)
    {
        return !(x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1 || maze[x, y].visited);
    }

    private Vector2Int CheckNeighbors()
    {
        List<Direction> randDir = GetRandomDirections();
        
        for (int i = 0; i < randDir.Count; i++)
        {
            Vector2Int neighbour = currentCell;

            switch (randDir[i])
            {
                case Direction.Up: neighbour.y++; break;
                case Direction.Down: neighbour.y--; break;
                case Direction.Right: neighbour.x++; break;
                case Direction.Left: neighbour.x--; break;
            }
            
            if (IsCellValid(neighbour.x, neighbour.y))
                return neighbour;
        }
        
        return currentCell;
    }
    
    private void BreakWalls(Vector2Int primaryCell, Vector2Int secondaryCell)
    {
        if (primaryCell.x > secondaryCell.x)
            maze[primaryCell.x, primaryCell.y].leftWall = false;
        else if (primaryCell.x < secondaryCell.x)
            maze[secondaryCell.x, secondaryCell.y].leftWall = false;
        else if (primaryCell.y < secondaryCell.y)
            maze[primaryCell.x, primaryCell.y].topWall = false;
        else if (primaryCell.y > secondaryCell.y)
            maze[secondaryCell.x, secondaryCell.y].topWall = false;
    }
    
    private void CarvedPath(int x, int y)
    {
        if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1)
        {
            x = y = 0;
        }

        currentCell = new Vector2Int(x, y);
        List<Vector2Int> path = new List<Vector2Int>();
        maze[currentCell.x, currentCell.y].visited = true;

        bool deadEnd = false;
        
        while (!deadEnd)
        {
            Vector2Int nextCell = CheckNeighbors();

            if (nextCell == currentCell)
            {
                for (int i = path.Count - 1; i >= 0; i--)
                {
                    currentCell = path[i];
                    path.RemoveAt(i);
                    nextCell = CheckNeighbors();
                    
                    if (nextCell != currentCell) break;
                }
                
                if (nextCell == currentCell)
                    deadEnd = true;
            }
            else
            {
                BreakWalls(currentCell, nextCell);
                maze[nextCell.x, nextCell.y].visited = true;
                currentCell = nextCell;
                path.Add(currentCell);
            }
        }
    }
}

public enum Direction { Up, Down, Left, Right }

public class MazeCell
{
    public bool visited;
    public int x, y;
    public bool topWall;
    public bool leftWall;

    public Vector2Int position => new Vector2Int(x, y);
    
    public MazeCell(int x, int y)
    {
        this.x = x;
        this.y = y;
        visited = false;
        topWall = leftWall = true;
    }
}

