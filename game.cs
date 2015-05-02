using System;
using System.Collections.Generic;

class TileInfo
{
    public TileInfo(char t, ConsoleColor c)
    {
        type = t;
        highlightColor = c;
    }

    public char type;
    public ConsoleColor highlightColor;
}

// Tile class: Contains various information about 
// a given tile on the board layout. 
// and provides properties for each variable.
public class Tile
{
    private TileInfo mPiece;

    public int X { get; set; }
    public int Y { get; set; }
    public bool IsVisited { get; set; }
    public bool HasUp { get; set; }
    public bool HasDown { get; set; }
    public bool HasLeft { get; set; }
    public bool HasRight { get; set; }
    public bool IsContiguous { get; set; }
    public char type;
    public ConsoleColor highlightColor;


    public Tile(int x, int y, bool isVisited,
                char tileType, ConsoleColor color,
                bool hasUp, bool hasDown, bool hasLeft,
                bool hasRight, bool isContiguous)
    {
        this.X = x;
        this.Y = y;
        this.IsVisited = isVisited;
        this.HasUp = hasUp;
        this.HasDown = hasDown;
        this.HasLeft = hasLeft;
        this.HasRight = hasRight;
        this.IsContiguous = isContiguous;

        mPiece = new TileInfo(tileType, color);
    }

    public ConsoleColor Color
    {
        get { return mPiece.highlightColor; }
        set { mPiece.highlightColor = value; }
    }

    public char TileType
    {
        get { return mPiece.type; }
        set { mPiece.type = value; }
    }
}

class Game
{
    //-------------------------------------------------------------------------
    // Initialize internal data

    private TileInfo[] mPieces;
    private Tile[,] mTiles;
    private int[][] mBoardLayout;
    private Dictionary<string, List<Tile>> mContiguousChunks;

    public Game(int[][] layout, TileInfo[] pieces)
    {
        mBoardLayout = layout;
        mPieces = pieces;
        mContiguousChunks = new Dictionary<string, List<Tile>>();
        InitBoards();
    }
    // Initializes a new square board consisting of "Tile" objectsusing the height 
    // and length of the widest row in the given layout.
    // Randomly assigns valid tiles with color and
    // type, checks and flags for valid existing up down left right neighbors, 
    // and creates
    public void InitBoards()
    {
        Random rnd = new Random();
        int height = mBoardLayout.Length;
        int width = findMaxWidth();
        mTiles = new Tile[height, width];

        // set up mTiles and tile types.
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int widthBounds = mBoardLayout[i].Length;
                if (j >= widthBounds || mBoardLayout[i][j] == 0)
                {
                    mTiles[i, j] = new Tile(j, i, false, '0', ConsoleColor.Black,
                                           false, false, false, false, false);
                    continue;
                }

                int randIndex = rnd.Next(0, mPieces.Length);
                TileInfo tilePieceInfo = mPieces[randIndex];
                mTiles[i, j] = new Tile(j, i, false, tilePieceInfo.type,
                                       tilePieceInfo.highlightColor, false,
                                        false, false, false, false);
            }
        }
        //loop through and check and subsequently flag neighbors.
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                bool hasLeft = checkLeft(i, j);
                bool hasRight = checkRight(i, j);
                bool hasUp = checkUp(i, j);
                bool hasDown = checkDown(i, j);
                 
                mTiles[i, j].HasLeft = hasLeft;
                mTiles[i, j].HasRight = hasRight;
                mTiles[i, j].HasUp = hasUp;
                mTiles[i, j].HasDown = hasDown;
            }
        }
    }

    //-------------------------------------------------------------------------
    // Find maximum width of board we're going to create.
    public int findMaxWidth()
    {
        int maxWidth = 0;
        int temp = 0;
        for (int i = 0; i < mBoardLayout.Length; i++)
        {
            temp = mBoardLayout[i].Length;
            if (temp > maxWidth)
            {
                maxWidth = temp;
            }
        }
        return maxWidth;

    }

    //checks for valid left child.
    public bool checkLeft(int i, int j)
    {
        if (j != 0)
        {
            if (mTiles[i, j - 1].TileType == '0')
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    //checks for valid right child.
    public bool checkRight(int i, int j)
    {

        if (j != mTiles.GetLength(0) - 1)
        {
            if (mTiles[i, j + 1].TileType == '0')
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    //checks for valid up child.
    public bool checkUp(int i, int j)
    {
        if (i != 0)
        {
            if (mTiles[i - 1, j].TileType == '0')
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    //checks for valid down child.
    public bool checkDown(int i, int j)
    {
        if (i != mTiles.GetLength(0) - 1)
        {
            if (mTiles[i + 1, j].TileType == '0')
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    //-------------------------------------------------------------------------
    // Find largest contiguous blocks, using BeginSearch() helper function, then mark 
    // the largest results as contiguous.
    public void FindBlocks()
    {
        for (int i = 0; i < mBoardLayout.Length; i++)
        {
            for (int j = 0; j < mBoardLayout[i].Length; j++)
            {
                if (mTiles[i, j].TileType == '0' || mTiles[i, j].IsVisited)
                {
                    continue;
                }
                else
                {
                    BeginSearch(mTiles[i, j]);
                }
            }
        }

        foreach (KeyValuePair<string, List<Tile>> entry in mContiguousChunks)
        {
            List<Tile> currList = entry.Value;
            for (int i = 0; i < currList.Count; i++)
            {
                currList[i].IsContiguous = true;
            }
        }
    }


    //-------------------------------------------------------------------------
    // Checks for contiguous chunks using a Breadth-first search style approach
    // maintains lists of tile "types" and yields a Dictionary with the largest
    // contiguous chunks of each Type that is at least size 3.
    public void BeginSearch(Tile startTile)
    {
        List<Tile> block = new List<Tile>();
        Queue<Tile> tileQ = new Queue<Tile>();
        char type = ' ';
        tileQ.Enqueue(startTile);
        while (tileQ.Count != 0)
        {
            Tile curTile = tileQ.Dequeue();
            if (curTile.IsVisited)
            {
                continue;
            }
            type = curTile.TileType;
            curTile.IsVisited = true;
            block.Add(curTile);
            if (curTile.HasLeft)
            {
                Tile left = mTiles[curTile.Y, curTile.X - 1];
                if (left.TileType == curTile.TileType && !left.IsVisited)
                {
                    tileQ.Enqueue(left);
                }
            }
            if (curTile.HasRight)
            {
                Tile right = mTiles[curTile.Y, curTile.X + 1];
                if (right.TileType == curTile.TileType && !right.IsVisited)
                {
                    tileQ.Enqueue(right);
                }
            }
            if (curTile.HasUp)
            {
                Tile up = mTiles[curTile.Y - 1, curTile.X];
                if (up.TileType == curTile.TileType && !up.IsVisited)
                {
                    tileQ.Enqueue(up);
                }
            }
            if (curTile.HasDown)
            {
                Tile down = mTiles[curTile.Y + 1, curTile.X];
                if (down.TileType == curTile.TileType && !down.IsVisited)
                {
                    tileQ.Enqueue(down);
                }
            }
        }
        int listSize = block.Count;
        string key = type.ToString();
        if (listSize >= 3)
        {
            if (mContiguousChunks.ContainsKey(key))
            {
                int currentChunkSize = mContiguousChunks[key].Count;
                if (listSize > currentChunkSize)
                {
                    mContiguousChunks[key] = block;
                }
            }
            else
            {
                mContiguousChunks.Add(key, block);
            }
        }
    }

    //-------------------------------------------------------------------------
    // Print the game board to the console
    // changes some of the formatting to be easier on the eyes. (Except blues =) )

    public void Print()
    {
        int bounds = mTiles.GetLength(0);
        for (int i = 0; i < bounds; i++)
        {
            for (int j = 0; j < bounds; j++)
            {

                if (mTiles[i, j].IsContiguous)
                {
                    Console.BackgroundColor = mTiles[i, j].Color;
                    Console.ForegroundColor = ConsoleColor.Black;
                    if (mTiles[i, j].TileType == '0')
                    {
                        Console.Write("  ");
                    }
                    else
                    {
                        Console.Write(string.Format("{0}", mTiles[i, j].TileType));
                    }
                    Console.ResetColor();
                    Console.Write(" ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ResetColor();
                    if (mTiles[i, j].TileType == '0')
                    {
                        Console.Write("  ");
                    }
                    else
                    {
                        Console.Write(string.Format("{0} ", mTiles[i, j].TileType));
                    }
                }

            }
            Console.WriteLine(Environment.NewLine);
        }
        /* IMPLEMENT ME -- See README-Instructions */
    }

    //-------------------------------------------------------------------------
    // Process user keyboard input
    // -- Checks dictionary for existence of key input
    // then changes the type and color to a new random type and associated color
    // then unflags contiguous and visited. Afterwards, it removes the visisted 
    // from the remainder of the board, and resets the contiuous chunks dictionary.
    //
    public void ProcessInput(char c)
    {       
        string removeKey = c.ToString().ToUpper();
        if (!mContiguousChunks.ContainsKey(removeKey))
        {
            return;           
        }

        Random rnd = new Random();
        List<Tile> targetList = mContiguousChunks[removeKey];
        for (int i = 0; i < targetList.Count; i++)
        {
            Tile currTile = targetList[i];
            int randIndex = rnd.Next(0, mPieces.Length);
            TileInfo tilePieceInfo = mPieces[randIndex];
            mTiles[currTile.Y, currTile.X].TileType = tilePieceInfo.type;
            mTiles[currTile.Y, currTile.X].IsContiguous = false;
            mTiles[currTile.Y, currTile.X].Color = tilePieceInfo.highlightColor;
            mTiles[currTile.Y, currTile.X].IsVisited = false;
        }
        mContiguousChunks.Clear();

        int bounds = mTiles.GetLength(0);
        for (int i = 0; i < bounds; i++)
        {
            for (int j = 0; j < bounds; j++)
            {
                Tile currTile = mTiles[i, j];
                if(currTile.TileType == '0')
                {
                    continue;
                }
                currTile.IsVisited = false;
                currTile.IsContiguous = false;
            }
        }
    }
}