const int MinDepth = 4;
var pool = new NodePool();

int maxDepth = args.Length > 0 ? int.Parse(args[0]) : 5;

if (MinDepth + 2 > maxDepth)
    maxDepth = MinDepth + 2;

int stretchDepth = maxDepth + 1;

var stretchTree = BottomUpTree(stretchDepth);
int check = ItemCheck(stretchTree);
pool.ReturnTree(stretchTree);
Console.WriteLine($"stretch tree of depth {stretchDepth}\t check: {check}");

var longLivedTree = BottomUpTree(maxDepth);

for (int depth = MinDepth; depth <= maxDepth; depth += 2)
{
    int iterations = 1 << (maxDepth - depth + MinDepth);
    check = 0;

    for (int i = 0; i < iterations; i++)
    {
        var tree = BottomUpTree(depth);
        check += ItemCheck(tree);
        pool.ReturnTree(tree);
    }

    Console.WriteLine($"{iterations}\t trees of depth {depth}\t check: {check}");
}

Console.WriteLine($"long lived tree of depth {maxDepth}\t check: {ItemCheck(longLivedTree)}");

Node BottomUpTree(int depth)
{
    var node = pool.Rent();
    if (depth > 0)
    {
        node.Left = BottomUpTree(depth - 1);
        node.Right = BottomUpTree(depth - 1);
    }
    return node;
}

static int ItemCheck(Node node) =>
    node.Left == null ? 1 : 1 + ItemCheck(node.Left) + ItemCheck(node.Right!);

class Node
{
    public Node? Left;
    public Node? Right;
}

class NodePool
{
    private Node? _freeHead;

    public Node Rent()
    {
        if (_freeHead != null)
        {
            var node = _freeHead;
            _freeHead = node.Left;
            node.Left = null;
            return node;
        }
        return new Node();
    }

    public void Return(Node node)
    {
        node.Right = null;
        node.Left = _freeHead;
        _freeHead = node;
    }

    public void ReturnTree(Node? node)
    {
        if (node == null) return;
        var head = node;

        while (true)
        {
            var left = node.Left;
            var right = node.Right;
            node.Right = null;

            if (left != null)
            {
                var p = left;
                while (p.Right != null) p = p.Right;
                p.Right = right;
                node = left;
            }
            else if (right != null)
            {
                node.Left = right;
                node = right;
            }
            else
            {
                node.Left = _freeHead;
                _freeHead = head;
                return;
            }
        }
    }
}
