const int MinDepth = 4;

int maxDepth = args.Length > 0 ? int.Parse(args[0]) : 5;

if (MinDepth + 2 > maxDepth)
    maxDepth = MinDepth + 2;

int stretchDepth = maxDepth + 1;

int estimatedNodes = 1 << (maxDepth + 2);
var arena = new TreeArena(estimatedNodes);

int stretchTree = arena.BottomUpTree(stretchDepth);
int check = arena.ItemCheck(stretchTree);
Console.WriteLine($"stretch tree of depth {stretchDepth}\t check: {check}");

var longLivedArena = new TreeArena(1 << (maxDepth + 1));
int longLivedTree = longLivedArena.BottomUpTree(maxDepth);

for (int depth = MinDepth; depth <= maxDepth; depth += 2)
{
    int iterations = 1 << (maxDepth - depth + MinDepth);
    check = 0;

    int nodesPerTree = (1 << (depth + 1)) - 1;
    var iterArena = new TreeArena(nodesPerTree + 1);

    for (int i = 0; i < iterations; i++)
    {
        iterArena.Reset();
        int tree = iterArena.BottomUpTree(depth);
        check += iterArena.ItemCheck(tree);
    }

    Console.WriteLine($"{iterations}\t trees of depth {depth}\t check: {check}");
}

Console.WriteLine($"long lived tree of depth {maxDepth}\t check: {longLivedArena.ItemCheck(longLivedTree)}");

struct Node
{
    public int Left;
    public int Right;
}

class TreeArena(int capacity)
{
    private Node[] _nodes = new Node[capacity];
    private int _count;

    public int Allocate()
    {
        if (_count >= _nodes.Length)
            Array.Resize(ref _nodes, _nodes.Length * 2);

        int index = _count++;
        _nodes[index].Left = -1;
        _nodes[index].Right = -1;
        return index;
    }

    public void Reset() => _count = 0;

    public int BottomUpTree(int depth)
    {
        int index = Allocate();
        if (depth > 0)
        {
            _nodes[index].Left = BottomUpTree(depth - 1);
            _nodes[index].Right = BottomUpTree(depth - 1);
        }
        return index;
    }

    public int ItemCheck(int index) =>
        _nodes[index].Left == -1 ? 1 : 1 + ItemCheck(_nodes[index].Left) + ItemCheck(_nodes[index].Right);
}
