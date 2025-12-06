using System.Runtime.InteropServices;

const int MinDepth = 4;

int maxDepth = args.Length > 0 ? int.Parse(args[0]) : 5;

if (MinDepth + 2 > maxDepth)
    maxDepth = MinDepth + 2;

int stretchDepth = maxDepth + 1;

int stretchNodes = (1 << (stretchDepth + 1)) - 1;
using var stretchArena = new NativeArena(stretchNodes + 1);
unsafe
{
    Node* stretchTree = BottomUpTree(stretchArena, stretchDepth);
    int check = ItemCheck(stretchTree);
    Console.WriteLine($"stretch tree of depth {stretchDepth}\t check: {check}");

    int longLivedNodes = (1 << (maxDepth + 1)) - 1;
    using var longLivedArena = new NativeArena(longLivedNodes + 1);
    Node* longLivedTree = BottomUpTree(longLivedArena, maxDepth);

    for (int depth = MinDepth; depth <= maxDepth; depth += 2)
    {
        int iterations = 1 << (maxDepth - depth + MinDepth);
        check = 0;

        int nodesPerTree = (1 << (depth + 1)) - 1;
        using var iterArena = new NativeArena(nodesPerTree + 1);

        for (int i = 0; i < iterations; i++)
        {
            iterArena.Reset();
            Node* tree = BottomUpTree(iterArena, depth);
            check += ItemCheck(tree);
        }

        Console.WriteLine($"{iterations}\t trees of depth {depth}\t check: {check}");
    }

    Console.WriteLine($"long lived tree of depth {maxDepth}\t check: {ItemCheck(longLivedTree)}");
}

static unsafe Node* BottomUpTree(NativeArena arena, int depth)
{
    Node* node = arena.Allocate();
    if (depth > 0)
    {
        node->Left = BottomUpTree(arena, depth - 1);
        node->Right = BottomUpTree(arena, depth - 1);
    }
    return node;
}

static unsafe int ItemCheck(Node* node) =>
    node->Left == null ? 1 : 1 + ItemCheck(node->Left) + ItemCheck(node->Right);

unsafe struct Node
{
    public Node* Left;
    public Node* Right;
}

unsafe class NativeArena(int nodeCount) : IDisposable
{
    private byte* _memory = (byte*)NativeMemory.Alloc((nuint)(nodeCount * sizeof(Node)));
    private int _offset;
    private readonly int _capacity = nodeCount * sizeof(Node);

    public Node* Allocate()
    {
        if (_offset + sizeof(Node) > _capacity)
            throw new OutOfMemoryException("Arena capacity exceeded");

        Node* node = (Node*)(_memory + _offset);
        _offset += sizeof(Node);
        node->Left = null;
        node->Right = null;
        return node;
    }

    public void Reset() => _offset = 0;

    public void Dispose()
    {
        if (_memory != null)
        {
            NativeMemory.Free(_memory);
            _memory = null;
        }
    }
}
