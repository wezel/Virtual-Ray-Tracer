using System.Collections.Generic;

namespace _Project.Ray_Tracer.Scripts.Utility
{
    /// <summary>
    /// A simple tree class based on the one from https://gist.github.com/luke161/f0165d475f6f485202187e014d265139.
    /// </summary>
    /// <typeparam name="T"> The type of data stored in the tree. </typeparam>
    public class TreeNode<T>
    {
        /// <summary>
        /// The data in this node.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// The children of this node.
        /// </summary>
        public List<TreeNode<T>> Children { get; private set; }

        private TreeNode<T> parent;

        /// <summary>
        /// Construct a new <see cref="TreeNode{T}"/> from <paramref name="data"/>. It will have no parent and no
        /// children.
        /// </summary>
        /// <param name="data"> The data stored in the new node. </param>
        public TreeNode(T data)
        {
            Data = data;
            Children = new List<TreeNode<T>>();
        }

        /// <summary>
        /// Construct a new <see cref="TreeNode{T}"/> from <paramref name="data"/>. Its parent will be
        /// <paramref name="parent"/> and it will have no children.
        /// </summary>
        /// <param name="data"> The data stored in the new node. </param>
        /// <param name="parent"> The parent of the new node. </param>
        public TreeNode(T data, TreeNode<T> parent) : this(data)
        {
            this.parent = parent;
        }

        /// <summary>
        /// Check whether this <see cref="TreeNode{T}"/> is a root node.
        /// </summary>
        /// <returns> <c>true</c> if this node has no parent, <c>false</c> otherwise. </returns>
        public bool IsRoot()
        {
            return parent == null;
        }

        /// <summary>
        /// Check whether this <see cref="TreeNode{T}"/> is a leaf node.
        /// </summary>
        /// <returns> <c>true</c> if this node has no children, <c>false</c> otherwise. </returns>
        public bool IsLeaf()
        {
            return Children.Count == 0;
        }

        /// <summary>
        /// Clear this node's list of children.
        /// </summary>
        public void Clear()
        {
            Children.Clear();
        }

        /// <summary>
        /// Determine the depth of this tree. The depth is the length of the longest chain of nodes starting from this
        /// node and ending in a leaf.
        /// </summary>
        /// <returns> The depth of this tree. Will be 1 if this node has no children. </returns>
        public int Depth()
        {
            // We consider a tree of one level to have depth 1.
            if (IsLeaf())
                return 1;

            // Find the maximum depth of all children.
            int depth = 0;
            foreach (var child in Children)
                depth = child.Depth() > depth ? child.Depth() : depth;

            // Depth is max depth of all children plus 1 for this node.
            return depth + 1;
        }
    
        /// <summary>
        /// Add a child constructed from <paramref name="data"/> to this node's list of children.
        /// </summary>
        /// <param name="data"> The data with which to construct the new child node. </param>
        public void AddChild(T data)
        {
            TreeNode<T> child = new TreeNode<T>(data, this);
            Children.Add(child);
        }

        /// <summary>
        /// Add <paramref name="child"/> to this node's list of children. The child's parent will be set to this node.
        /// Make sure to remove the child from any existing trees first.
        /// </summary>
        /// <param name="child"> The new child node. </param>
        public void AddChild(TreeNode<T> child)
        {
            child.parent = this;
            Children.Add(child);
        }

        /// <summary>
        /// Remove <paramref name="child"/> from this node's list of children.
        /// </summary>
        /// <param name="child"> The child to remove. </param>
        /// <returns>
        /// <c>true</c> if <paramref name="child"/> was successfully removed, <c>false</c> otherwise.
        /// </returns>
        public bool RemoveChild(TreeNode<T> child)
        {
            return Children.Remove(child);
        }
    }
}
