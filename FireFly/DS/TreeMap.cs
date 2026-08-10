using System.Collections;
using System.Runtime.CompilerServices;

namespace FireFly.DS;

/// <summary>Represents a sorted dictionary implemented as a red-black tree with O(log n) lookup and update operations; keys must be nonnull and the comparer must define a consistent total order.</summary>
public class TreeMap<TKey, TValue> : IDictionary<TKey, TValue> {
    private enum NodeColor {
        Red,
        Black
    }

    // A. 内嵌的 private class Node 表示结点
    private class Node {
        public TKey Key;
        public TValue Value;
        public Node? Left;
        public Node? Right;
        public Node? Parent;
        public NodeColor Color;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node(TKey key, TValue value, NodeColor color) {
            Key = key;
            Value = value;
            Color = color;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node? Successor() {
            if (Right != null) {
                Node current = Right;
                while (current.Left != null)
                    current = current.Left;
                return current;
            }
            Node x = this;
            Node? y = Parent;
            while (y != null && x == y.Right) {
                x = y;
                y = y.Parent;
            }
            return y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node? Predecessor() {
            if (Left != null) {
                Node current = Left;
                while (current.Right != null)
                    current = current.Right;
                return current;
            }
            Node x = this;
            Node? y = Parent;
            while (y != null && x == y.Left) {
                x = y;
                y = y.Parent;
            }
            return y;
        }
    }

    // B. 内嵌的 public class Iterator 表示一个迭代器
    /// <summary>Represents a position in a tree map in ascending key order; iterators must not be reused after the map is structurally modified.</summary>
    public class Iterator {
        private readonly TreeMap<TKey, TValue> _tree;
        private Node? _node;

        // 构造函数是 internal 的，通过 object 传参以规避 CS0051 访问级别冲突
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Iterator(TreeMap<TKey, TValue> tree, object? node) {
            _tree = tree;
            _node = (Node?)node;
        }

        // Key 是只读属性
        /// <summary>Gets the key at the current position; the iterator must not be the past-the-end iterator.</summary>
        public TKey Key {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (_node == null) {
                    throw new InvalidOperationException("「迭代器当前为空，无法读取键。」");
                }
                return _node.Key;
            }
        }

        // Value 可读可写，与 Node 中的值绑定
        /// <summary>Gets or sets the value at the current position; the iterator must not be the past-the-end iterator.</summary>
        public TValue Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (_node == null) {
                    throw new InvalidOperationException("「迭代器当前为空，无法读取值。」");
                }
                return _node.Value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                if (_node == null) {
                    throw new InvalidOperationException("「迭代器当前为空，无法写入值。」");
                }
                _node.Value = value;
            }
        }

        /// <summary>Advances an iterator to its successor in O(log n) time.</summary>
        /// <param name="it">The iterator to advance; it must not be null or past the end.</param>
        /// <returns>An iterator to the next position, possibly the past-the-end position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Iterator operator ++(Iterator it) {
            if (it._node == null) {
                throw new InvalidOperationException("「迭代器已在末尾，无法向后移动。」");
            }
            return new Iterator(it._tree, it._node.Successor());
        }

        /// <summary>Moves an iterator to its predecessor in O(log n) time.</summary>
        /// <param name="it">The iterator to move; it must not be null, its tree must be nonempty, and a non-end iterator must not be at the beginning.</param>
        /// <returns>An iterator to the preceding position; the past-the-end iterator moves to the last element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Iterator operator --(Iterator it) {
            if (it._node == null) {
                Node? maxNode = it._tree.GetMaxNode();
                if (maxNode == null) {
                    throw new InvalidOperationException("「字典为空，无法向前移动。」");
                }
                return new Iterator(it._tree, maxNode);
            }

            Node? prev = it._node.Predecessor();
            if (prev == null) {
                throw new InvalidOperationException("「迭代器已在开头，无法向前移动。」");
            }
            return new Iterator(it._tree, prev);
        }

        /// <summary>Moves an iterator by an offset in O(abs(offset) log n) time.</summary>
        /// <param name="it">The starting iterator; it must not be null.</param>
        /// <param name="offset">The signed number of positions to move; it must not be int.MinValue, and the traversal must stay between the beginning and the past-the-end position.</param>
        /// <returns>An iterator at the resulting position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Iterator operator +(Iterator it, int offset) {
            if (offset == 0) return new Iterator(it._tree, it._node);
            if (offset < 0) return it - (-offset);

            Node? current = it._node;
            for (int i = 0; i < offset; i++) {
                if (current == null) throw new InvalidOperationException("「迭代器越界。」");
                current = current.Successor();
            }
            return new Iterator(it._tree, current);
        }

        /// <summary>Moves an iterator backward by an offset in O(abs(offset) log n) time.</summary>
        /// <param name="it">The starting iterator; it must not be null.</param>
        /// <param name="offset">The signed number of positions to move backward; it must not be int.MinValue, and the traversal must stay between the beginning and the past-the-end position.</param>
        /// <returns>An iterator at the resulting position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Iterator operator -(Iterator it, int offset) {
            if (offset == 0) return new Iterator(it._tree, it._node);
            if (offset < 0) return it + (-offset);

            Node? current = it._node;
            for (int i = 0; i < offset; i++) {
                if (current == null) {
                    current = it._tree.GetMaxNode();
                    if (current == null) throw new InvalidOperationException("「字典为空，无法向前移动。」");
                } else {
                    current = current.Predecessor();
                    if (current == null) {
                        throw new InvalidOperationException("「迭代器越界。」");
                    }
                }
            }
            return new Iterator(it._tree, current);
        }
        /// <summary>Determines whether another object refers to the same underlying node; past-the-end iterators from different maps compare equal.</summary>
        /// <param name="obj">The object to compare with this iterator.</param>
        /// <returns>Whether <paramref name="obj"/> refers to the same underlying node, including a null end node.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) {
            return obj is Iterator other && ReferenceEquals(_node, other._node);
        }

        /// <summary>Computes a hash code for the current node position.</summary>
        /// <returns>The hash code of the current node, or zero for the past-the-end position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {
            return _node?.GetHashCode() ?? 0;
        }

        /// <summary>Determines whether two iterators refer to the same underlying node; past-the-end iterators from different maps compare equal.</summary>
        /// <param name="left">The first iterator.</param>
        /// <param name="right">The second iterator.</param>
        /// <returns>Whether both iterators are null or their underlying nodes are reference-equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Iterator? left, Iterator? right) {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return ReferenceEquals(left._node, right._node);
        }

        /// <summary>Determines whether two iterators do not refer to the same underlying node.</summary>
        /// <param name="left">The first iterator.</param>
        /// <param name="right">The second iterator.</param>
        /// <returns>The negation of iterator equality.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Iterator? left, Iterator? right) {
            return !(left == right);
        }
    }

    private Node? _root;
    private int _count;
    private readonly IComparer<TKey> _comparer;

    /// <summary>Creates an empty tree map using the default key comparer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TreeMap() {
        _comparer = Comparer<TKey>.Default;
    }

    /// <summary>Creates an empty tree map using the given key comparer.</summary>
    /// <param name="comparer">The key comparer, or null to use the default comparer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TreeMap(IComparer<TKey>? comparer) {
        _comparer = comparer ?? Comparer<TKey>.Default;
    }

    // C. 左旋的 private 方法
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LeftRotate(Node x) {
        if (x.Right == null) return;

        Node y = x.Right;
        x.Right = y.Left;

        if (y.Left != null) {
            y.Left.Parent = x;
        }

        y.Parent = x.Parent;

        if (x.Parent == null) {
            _root = y;
        } else if (x == x.Parent.Left) {
            x.Parent.Left = y;
        } else {
            x.Parent.Right = y;
        }

        y.Left = x;
        x.Parent = y;
    }

    // C. 右旋的 private 方法
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RightRotate(Node x) {
        if (x.Left == null) return;

        Node y = x.Left;
        x.Left = y.Right;

        if (y.Right != null) {
            y.Right.Parent = x;
        }

        y.Parent = x.Parent;

        if (x.Parent == null) {
            _root = y;
        } else if (x == x.Parent.Right) {
            x.Parent.Right = y;
        } else {
            x.Parent.Left = y;
        }

        y.Right = x;
        x.Parent = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InsertFixup(Node z) {
        while (z.Parent != null && z.Parent.Color == NodeColor.Red) {
            if (z.Parent == z.Parent.Parent?.Left) {
                Node? y = z.Parent.Parent.Right;
                if (y != null && y.Color == NodeColor.Red) {
                    z.Parent.Color = NodeColor.Black;
                    y.Color = NodeColor.Black;
                    z.Parent.Parent.Color = NodeColor.Red;
                    z = z.Parent.Parent;
                } else {
                    if (z == z.Parent.Right) {
                        z = z.Parent;
                        LeftRotate(z);
                    }
                    z.Parent!.Color = NodeColor.Black;
                    z.Parent.Parent!.Color = NodeColor.Red;
                    RightRotate(z.Parent.Parent);
                }
            } else {
                Node? y = z.Parent.Parent?.Left;
                if (y != null && y.Color == NodeColor.Red) {
                    z.Parent.Color = NodeColor.Black;
                    y.Color = NodeColor.Black;
                    z.Parent.Parent!.Color = NodeColor.Red;
                    z = z.Parent.Parent;
                } else {
                    if (z == z.Parent.Left) {
                        z = z.Parent;
                        RightRotate(z);
                    }
                    z.Parent!.Color = NodeColor.Black;
                    z.Parent.Parent!.Color = NodeColor.Red;
                    LeftRotate(z.Parent.Parent);
                }
            }
        }
        _root!.Color = NodeColor.Black;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DeleteFixup(Node? x, Node? xParent) {
        while (x != _root && (x == null || x.Color == NodeColor.Black)) {
            if (x == xParent?.Left) {
                Node? w = xParent!.Right;
                if (w != null && w.Color == NodeColor.Red) {
                    w.Color = NodeColor.Black;
                    xParent.Color = NodeColor.Red;
                    LeftRotate(xParent);
                    w = xParent.Right;
                }
                if ((w?.Left == null || w.Left.Color == NodeColor.Black) &&
                    (w?.Right == null || w.Right.Color == NodeColor.Black)) {
                    if (w != null) w.Color = NodeColor.Red;
                    x = xParent;
                    xParent = x?.Parent;
                } else {
                    if (w?.Right == null || w.Right.Color == NodeColor.Black) {
                        if (w?.Left != null) w.Left.Color = NodeColor.Black;
                        if (w != null) w.Color = NodeColor.Red;
                        if (w != null) RightRotate(w);
                        w = xParent.Right;
                    }
                    if (w != null) {
                        w.Color = xParent.Color;
                        if (w.Right != null) w.Right.Color = NodeColor.Black;
                    }
                    xParent.Color = NodeColor.Black;
                    LeftRotate(xParent);
                    x = _root;
                }
            } else {
                Node? w = xParent?.Left;
                if (w != null && w.Color == NodeColor.Red) {
                    w.Color = NodeColor.Black;
                    xParent!.Color = NodeColor.Red;
                    RightRotate(xParent);
                    w = xParent.Left;
                }
                if ((w?.Right == null || w.Right.Color == NodeColor.Black) &&
                    (w?.Left == null || w.Left.Color == NodeColor.Black)) {
                    if (w != null) w.Color = NodeColor.Red;
                    x = xParent;
                    xParent = x?.Parent;
                } else {
                    if (w?.Left == null || w.Left.Color == NodeColor.Black) {
                        if (w?.Right != null) w.Right.Color = NodeColor.Black;
                        if (w != null) w.Color = NodeColor.Red;
                        if (w != null) LeftRotate(w);
                        w = xParent!.Left;
                    }
                    if (w != null) {
                        w.Color = xParent!.Color;
                        if (w.Left != null) w.Left.Color = NodeColor.Black;
                    }
                    if (xParent != null) {
                        xParent.Color = NodeColor.Black;
                        RightRotate(xParent);
                    }
                    x = _root;
                }
            }
        }
        if (x != null) {
            x.Color = NodeColor.Black;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Node? FindNode(TKey key) {
        Node? current = _root;
        while (current != null) {
            int cmp = _comparer.Compare(key, current.Key);
            if (cmp == 0) return current;
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Node? GetMinNode() {
        Node? current = _root;
        if (current != null) {
            while (current.Left != null) {
                current = current.Left;
            }
        }
        return current;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Node? GetMaxNode() {
        Node? current = _root;
        if (current != null) {
            while (current.Right != null) {
                current = current.Right;
            }
        }
        return current;
    }

    /// <summary>Finds the first position whose key is not less than the given key in O(log n) time.</summary>
    /// <param name="key">The search key; it must not be null.</param>
    /// <returns>The first matching position, or the past-the-end iterator when no such position exists.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Iterator LowerBound(TKey key) {
        if (key == null) throw new ArgumentNullException(nameof(key));
        Node? current = _root;
        Node? result = null;

        while (current != null) {
            int cmp = _comparer.Compare(key, current.Key);
            if (cmp <= 0) {
                result = current;
                current = current.Left;
            } else {
                current = current.Right;
            }
        }

        return new Iterator(this, result);
    }

    /// <summary>Finds the first position whose key is greater than the given key in O(log n) time.</summary>
    /// <param name="key">The search key; it must not be null.</param>
    /// <returns>The first matching position, or the past-the-end iterator when no such position exists.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Iterator UpperBound(TKey key) {
        if (key == null) throw new ArgumentNullException(nameof(key));
        Node? current = _root;
        Node? result = null;

        while (current != null) {
            int cmp = _comparer.Compare(key, current.Key);
            if (cmp < 0) {
                result = current;
                current = current.Left;
            } else {
                current = current.Right;
            }
        }

        return new Iterator(this, result);
    }

    /// <summary>Gets an iterator to the smallest key in O(log n) time, or the past-the-end iterator when the map is empty.</summary>
    public Iterator Begin => new(this, GetMinNode());
    /// <summary>Gets the past-the-end iterator.</summary>
    public Iterator End => new(this, null);

    // ====================================================================
    // 以下为 IDictionary<TKey, TValue> 的接口存根，用以保证代码能够顺利编译
    // ====================================================================

    /// <summary>Gets or sets a value in O(log n) time; getting a missing key inserts and returns the default value.</summary>
    /// <param name="key">The key; it must not be null.</param>
    /// <returns>The value associated with <paramref name="key"/>, or the inserted default value when the key was absent.</returns>
    public TValue this[TKey key] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (key == null) throw new ArgumentNullException(nameof(key));
            Node? node = FindNode(key);
            if (node != null) return node.Value;

            TValue defaultValue = default!;
            Add(key, defaultValue);
            return defaultValue;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set {
            if (key == null) throw new ArgumentNullException(nameof(key));
            Node? node = FindNode(key);
            if (node != null) {
                node.Value = value;
            } else {
                Add(key, value);
            }
        }
    }

    /// <summary>Gets a newly allocated snapshot of the keys in ascending order in O(n) time.</summary>
    public ICollection<TKey> Keys {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            List<TKey> keys = new List<TKey>(_count);
            foreach (var kvp in this) keys.Add(kvp.Key);
            return keys;
        }
    }

    /// <summary>Gets a newly allocated snapshot of the values in ascending key order in O(n) time.</summary>
    public ICollection<TValue> Values {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            List<TValue> values = new List<TValue>(_count);
            foreach (var kvp in this) values.Add(kvp.Value);
            return values;
        }
    }

    /// <summary>Gets the number of key-value pairs.</summary>
    public int Count => _count;

    /// <summary>Gets whether the map is read-only, which is always false.</summary>
    public bool IsReadOnly => false;

    /// <summary>Adds a unique key and value in O(log n) time.</summary>
    /// <param name="key">The key to add; it must not be null or already present.</param>
    /// <param name="value">The value to associate with the key.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, TValue value) {
        if (key == null) throw new ArgumentNullException(nameof(key));

        Node z = new Node(key, value, NodeColor.Red);
        Node? y = null;
        Node? x = _root;
        int cmp = 0;

        while (x != null) {
            y = x;
            cmp = _comparer.Compare(key, x.Key);
            if (cmp < 0) {
                x = x.Left;
            } else if (cmp > 0) {
                x = x.Right;
            } else {
                throw new ArgumentException("「已存在相同的键。」");
            }
        }

        z.Parent = y;
        if (y == null) {
            _root = z;
        } else if (cmp < 0) {
            y.Left = z;
        } else {
            y.Right = z;
        }

        InsertFixup(z);
        _count++;
    }

    /// <summary>Adds a key-value pair whose key is unique in O(log n) time.</summary>
    /// <param name="item">The pair to add; its key must not be null or already present.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(KeyValuePair<TKey, TValue> item) {
        Add(item.Key, item.Value);
    }

    /// <summary>Removes all key-value pairs in O(1) time.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() {
        _root = null;
        _count = 0;
    }

    /// <summary>Determines in O(log n) time whether the map contains an equal key-value pair.</summary>
    /// <param name="item">The pair to find; its key must not be null.</param>
    /// <returns>Whether the map contains the key with an equal value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(KeyValuePair<TKey, TValue> item) {
        if (item.Key == null) throw new ArgumentNullException(nameof(item.Key));
        Node? node = FindNode(item.Key);
        return node != null && EqualityComparer<TValue>.Default.Equals(node.Value, item.Value);
    }

    /// <summary>Determines in O(log n) time whether the map contains a key.</summary>
    /// <param name="key">The key to find; it must not be null.</param>
    /// <returns>Whether the map contains <paramref name="key"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) {
        if (key == null) throw new ArgumentNullException(nameof(key));
        return FindNode(key) != null;
    }

    /// <summary>Copies all key-value pairs in ascending key order to an array in O(n) time.</summary>
    /// <param name="array">The destination array; it must not be null and must have space for all pairs.</param>
    /// <param name="arrayIndex">The zero-based destination offset; it must be within the array and leave at least <see cref="Count"/> slots.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < _count) throw new ArgumentException("「目标数组空间不足。」");

        foreach (var kvp in this) {
            array[arrayIndex++] = kvp;
        }
    }

    /// <summary>Enumerates the key-value pairs in ascending key order in O(n) total time and O(log n) auxiliary space; the map must not be structurally modified during enumeration.</summary>
    /// <returns>An enumerator over the sorted key-value pairs.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
        if (_root == null) yield break;

        var stack = new Stack<Node>();
        var current = _root;

        while (stack.Count > 0 || current != null) {
            while (current != null) {
                stack.Push(current);
                current = current.Left;
            }
            current = stack.Pop();
            yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
            current = current.Right;
        }
    }

    /// <summary>Removes a key in O(log n) time.</summary>
    /// <param name="key">The key to remove; it must not be null.</param>
    /// <returns>Whether the key was present and removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key) {
        if (key == null) throw new ArgumentNullException(nameof(key));

        Node? z = FindNode(key);
        if (z == null) return false;

        Node? y = (z.Left == null || z.Right == null) ? z : z.Successor();
        Node? x = y!.Left ?? y.Right;
        Node? xParent = y.Parent;

        if (x != null) {
            x.Parent = y.Parent;
        }

        if (y.Parent == null) {
            _root = x;
        } else if (y == y.Parent.Left) {
            y.Parent.Left = x;
        } else {
            y.Parent.Right = x;
        }

        if (y != z) {
            z.Key = y.Key;
            z.Value = y.Value;
        }

        if (y.Color == NodeColor.Black) {
            DeleteFixup(x, xParent);
        }

        _count--;
        return true;
    }

    /// <summary>Removes an equal key-value pair in O(log n) time.</summary>
    /// <param name="item">The pair to remove; its key must not be null.</param>
    /// <returns>Whether an equal pair was present and removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(KeyValuePair<TKey, TValue> item) {
        Node? node = FindNode(item.Key);
        if (node != null && EqualityComparer<TValue>.Default.Equals(node.Value, item.Value)) {
            return Remove(item.Key);
        }
        return false;
    }

    /// <summary>Attempts to get the value associated with a key in O(log n) time.</summary>
    /// <param name="key">The key to find; it must not be null.</param>
    /// <param name="value">Receives the associated value when found, or the default value otherwise.</param>
    /// <returns>Whether the key was found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, out TValue value) {
        if (key == null) throw new ArgumentNullException(nameof(key));
        Node? node = FindNode(key);
        if (node != null) {
            value = node.Value;
            return true;
        }
        value = default!;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
