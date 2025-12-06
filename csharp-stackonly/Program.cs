const int MinDepth = 4;

int maxDepth = args.Length > 0 ? int.Parse(args[0]) : 5;

if (MinDepth + 2 > maxDepth)
    maxDepth = MinDepth + 2;

int stretchDepth = maxDepth + 1;

int check = BottomUpTree(stretchDepth).ItemCheck();
Console.WriteLine($"stretch tree of depth {stretchDepth}\t check: {check}");

var longLivedTree = BottomUpTree(maxDepth);

for (int depth = MinDepth; depth <= maxDepth; depth += 2)
{
    int iterations = 1 << (maxDepth - depth + MinDepth);
    check = 0;

    for (int i = 0; i < iterations; i++)
    {
        check += BottomUpTree(depth).ItemCheck();
    }

    Console.WriteLine($"{iterations}\t trees of depth {depth}\t check: {check}");
}

Console.WriteLine($"long lived tree of depth {maxDepth}\t check: {longLivedTree.ItemCheck()}");

static Node BottomUpTree(int depth) =>
    depth <= 0 ? new Node(null, null) : new Node(BottomUpTree(depth - 1), BottomUpTree(depth - 1));

record struct Node
{
    readonly int _result;

    public Node(Node? left, Node? right) =>
        _result = left is null ? 1 : 1 + left.Value.ItemCheck() + right!.Value.ItemCheck();

    public int ItemCheck() => _result;
}
