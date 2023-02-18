using System.Collections.Generic;

class TreeNode
{
    public string Name { get; set; }
    public List<TreeNode> Children { get; set; }
    public TreeNode Parent { get; set; }

    public TreeNode(string name)
    {
        Name = name;
        Children = new List<TreeNode>();
    }
}
