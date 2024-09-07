using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace JSONCanvasDotNet.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(TextNode), typeDiscriminator: "text")]
    [JsonDerivedType(typeof(GroupNode), typeDiscriminator: "group")]
    [JsonDerivedType(typeof(LinkNode), typeDiscriminator: "link")]
    [JsonDerivedType(typeof(FileNode), typeDiscriminator: "file")]
    public class Node
    {
        public Node(
            int x,
            int y,
            int? width = null,
            int? height = null,
            string? id = null
        )
        {
            this.id = id ?? Guid.NewGuid().ToString();

            this.x = x;
            this.y = y;

            this.width = width ?? Node.DefaultWidth;
            this.height = height ?? Node.DefaultHeight;

            this.Edges = new List<Edge>();
        }

        public Node(
            Rectangle boundary,
            string? id = null
        ): this(
            x: boundary.X,
            y: boundary.Y,
            width: boundary.Width,
            height: boundary.Height,
            id: id
        )
        {
        }

        #region JSON fields

        [JsonInclude]
        public string id
        {
            get;
            set;
        }

        [JsonIgnore]
        private int _x;

        [JsonInclude]
        public int x
        {
            get
            {
                return this._x;
            }
            set
            {
                this._x = value;
                this.boundary = new Rectangle(this.x, this.y, this.width, this.height);
            }
        }

        [JsonIgnore]
        private int _y;

        [JsonInclude]
        public int y
        {
            get
            {
                return this._y;
            }
            set
            {
                this._y = value;
                this.boundary = new Rectangle(this.x, this.y, this.width, this.height);
            }
        }

        [JsonIgnore]
        private int _width;

        [JsonInclude]
        public int width
        {
            get
            {
                return this._width;
            }
            set
            {
                this._width = value;
                this.boundary = new Rectangle(this.x, this.y, this.width, this.height);
            }
        }

        [JsonIgnore]
        private int _height;

        [JsonInclude]
        public int height
        {
            get
            {
                return this._height;
            }
            set
            {
                this._height = value;
                this.boundary = new Rectangle(this.x, this.y, this.width, this.height);
            }
        }

        [JsonInclude]
        public CanvasColor? color { get; set; }

        #endregion

        #region Object Reference Fields

        [JsonIgnore]
        public List<Edge> Edges
        {
            get;
            set;
        }

        [JsonIgnore]
        private GroupNode? _parentNode;

        [JsonIgnore]
        public GroupNode? parentNode
        {
            get
            {
                return this._parentNode;
            }
            set
            {
                if (this == value)
                {
                    throw new Exception("A node cannot be its own parent");
                }

                this._parentNode = value;
            }

        }

        [JsonIgnore]
        public Canvas? Canvas
        {
            get;
            set;
        }

        [JsonIgnore]
        public int z = 0;

        #endregion

        #region Derived Properties

        [JsonIgnore]
        public int bottom
        {
            get
            {
                return this.boundary.Bottom;
            }

        }

        [JsonIgnore]
        public int top
        {
            get
            {
                return this.boundary.Top;
            }

        }

        [JsonIgnore]
        public int left
        {
            get
            {
                return this.boundary.Left;
            }

        }

        [JsonIgnore]
        public int right
        {
            get
            {
                return this.boundary.Right;
            }

        }

        [JsonIgnore]
        public (int X, int Y) topRight
        {
            get
            {
                return new (this.right, this.top);
            }
            set
            {
                this.x = value.X - this.width;
                this.y = value.Y;
            }

        }

        [JsonIgnore]
        public (int X, int Y) bottomRight
        {
            get
            {
                return new (this.right, this.bottom);
            }
            set
            {
                this.x = value.X - this.width;
                this.y = value.Y - this.height;
            }

        }

        [JsonIgnore]
        public (int X, int Y) bottomLeft
        {
            get
            {
                return new (this.left, this.bottom);
            }
            set
            {
                this.x = value.X;
                this.y = value.Y - this.height;
            }

        }

        [JsonIgnore]
        public (int X, int Y) topLeft
        {
            get
            {
                return new (this.left, this.top);
            }
            set
            {
                this.x = value.X;
                this.y = value.Y;
            }

        }

        [JsonIgnore]
        public (int X, int Y) leftEdgePosition
        {
            get
            {
                return new (this.left, this.top + (this.height / 2));
            }

        }

        [JsonIgnore]
        public (int X, int Y) rightEdgePosition
        {
            get
            {
                return new (this.right, this.top + (this.height / 2));
            }

        }

        [JsonIgnore]
        public (int X, int Y) topEdgePosition
        {
            get
            {
                return new (this.left + (this.width / 2), this.top);
            }

        }

        [JsonIgnore]
        public (int X, int Y) bottomEdgePosition
        {
            get
            {
                return new (this.left + (this.width / 2), this.bottom);
            }

        }

        [JsonIgnore]
        private Rectangle _boundary;

        [JsonIgnore]
        public Rectangle boundary
        {
            get
            {
                return this._boundary;
            }
            set
            {
                this._boundary = value;
            }

        }

        #endregion

        #region Instance Methods

        public bool HasAncestor(Node ancestor)
        {
            if (ancestor == this.parentNode)
            {
                return true;
            }

            var parentNode = this.parentNode;
            while (parentNode != null)
            {
                parentNode = parentNode.parentNode;

                if (parentNode == ancestor)
                {
                    return true;
                }

            }

            return false;

        }

        public Node GetRoot()
        {
            if (this.parentNode == null)
            {
                return this;
            }

            var parentNode = this.parentNode;
            var lastValidParent = parentNode;
            while (parentNode != null)
            {
                lastValidParent = parentNode;
                parentNode = parentNode.parentNode;
            }

            return lastValidParent;
        }

        public (int X, int Y) GetEdgeSidePosition(EdgeSide edgeSide)
        {
            if (edgeSide.Equals(EdgeSide.bottom))
            {
                return this.bottomEdgePosition;
            }
            else if (edgeSide.Equals(EdgeSide.top))
            {
                return this.topEdgePosition;
            }
            else if (edgeSide.Equals(EdgeSide.left))
            {
                return this.leftEdgePosition;
            }
            else if (edgeSide.Equals(EdgeSide.right))
            {
                return this.rightEdgePosition;
            }
            else
            {
                throw new ArgumentException($"Invalid EdgeSide {edgeSide.Value} provided.");
            }

        }

        public bool IsInGroup(GroupNode group)
        {
            return group.NodeIsChild(this);
        }

        public bool Overlaps(Node targetNode)
        {
            return Node.NodesOverlap(this, targetNode);
        }
        public bool Overlaps(Rectangle targetBoundary)
        {
            return this.boundary.IntersectsWith(targetBoundary);
        }
        public bool Contains(Node targetNode)
        {
            return this.boundary.Contains(targetNode.boundary);
        }
        public bool Contains(Rectangle targetBoundary)
        {
            return this.boundary.Contains(targetBoundary);
        }

        public bool IsTouching(Node targetNode)
        {
            var isTouching = Node.IsNodeTouchingNode(this, targetNode);

            return isTouching.Item1;
        }

        public EdgeSide? GetSideTouching(Node targetNode)
        {
            var isTouching = Node.IsNodeTouchingNode(this, targetNode);

            return isTouching.Item2;
        }

        #endregion

        #region Static Methods

        public static (bool isTouching, EdgeSide? touchingEdge) IsNodeTouchingNode(Node sourceNode, Node destinationNode)
        {
            var sourceIsVerticallyAligned = (
                sourceNode.left <= destinationNode.right &&
                sourceNode.right >= destinationNode.left
            );

            var sourceIsHorizontallyAligned = (
                sourceNode.bottom >= destinationNode.top &&
                sourceNode.top <= destinationNode.bottom
            );

            // Source Node's bottom is touching the Destination Node's top
            if (sourceNode.bottom == destinationNode.top)
            {
                if (sourceIsVerticallyAligned)
                {
                    return new (true, EdgeSide.bottom);
                }
                
            }

            else if (sourceNode.top == destinationNode.bottom)
            {
                if (sourceIsVerticallyAligned)
                {
                    return new (true, EdgeSide.top);
                }
            }

            else if (sourceNode.left == destinationNode.right)
            {
                if (sourceIsHorizontallyAligned)
                {
                    return new (true, EdgeSide.left);
                }

            }

            else if (sourceNode.right == destinationNode.left)
            {
                if (sourceIsHorizontallyAligned)
                {
                    return new (true, EdgeSide.right);
                }

            }

            return new (false, null);
        }

        public static bool IsNodeBelowNode(Node sourceNode, Node destinationNode)
        {
            return sourceNode.top >= destinationNode.bottom;
        }

        public static bool IsNodeAboveNode(Node sourceNode, Node destinationNode)
        {
            return sourceNode.bottom <= destinationNode.top;
        }

        public static bool IsNodeLeftOfNode(Node sourceNode, Node destinationNode)
        {
            return sourceNode.right <= destinationNode.left;
        }

        public static bool IsNodeRightOfNode(Node sourceNode, Node destinationNode)
        {
            return sourceNode.left >= destinationNode.right;
        }

        public static bool NodesOverlap(Node firstNode, Node secondNode)
        {
            if (firstNode.boundary.IntersectsWith(secondNode.boundary))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Static Properties

        [JsonIgnore]
        public static int DefaultWidth = 200;

        [JsonIgnore]
        public static int DefaultHeight = 200;

        #endregion
    }

    public class TextNode : Node
    {
        public TextNode(
            int x,
            int y,
            int width,
            int height,
            string? text = null,
            string? id = null
        ) : base(
            x,
            y,
            width,
            height,
            id: id
        )
        {
            this.text = text;
        }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? text
        {
            get;
            set;
        }

    }

    public class FileNode : Node
    {
        public FileNode(
            string filePath,
            int x,
            int y,
            int width,
            int height,
            string? subPath = null,
            string? id = null
        ) : base(
            x,
            y,
            width,
            height,
            id: id
        )
        {
            this.file = filePath;
            this.subpath = subPath;
        }

        [JsonInclude]
        public string file
        {
            get;
            set;
        }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? subpath
        {
            get;
            set;
        }

    }

    public class LinkNode : Node
    {
        public LinkNode(
            string url,
            int x,
            int y,
            int width,
            int height,
            string? id = null
        ) : base(
            x,
            y,
            width,
            height,
            id: id
        )
        {
            this.url = url;
        }

        [JsonInclude]
        public string url
        {
            get;
            set;
        }

    }

    public class GroupNode : Node
    {
        public GroupNode(
            int x,
            int y,
            int width,
            int height,
            string? label = null,
            string? background = null,
            GroupNodeBackgroundStyle? backgroundStyle = null,
            string? id = null
        ) : base(
            x,
            y,
            width,
            height,
            id: id
        )
        {
            this.label = label;

            this.background = background;
            this.backgroundStyle = backgroundStyle;

            this.children = new List<Node>();
            this.childLookup = new Dictionary<string, Node>();
        }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? label;

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? background;

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public GroupNodeBackgroundStyle? backgroundStyle;

        [JsonIgnore]
        public List<Node> children
        {
            get;
        }

        [JsonIgnore]
        public Dictionary<string, Node> childLookup
        {
            get;
        }

        #region Instance Methods

        public Node? AddOrGetChildNode(Node node)
        {
            if (node == this)
            {
                return null;
            }

            var result = this.childLookup.GetValueOrDefault(node.id);
            if (result != null)
            {
                return result;
            }

            this.children.Add(node);
            this.childLookup[node.id] = node;

            node.parentNode = this;
            node.z = this.z + 1;

            node.Canvas = this.Canvas;

            if (!this.Contains(node))
            {
                this.PlaceNodeInGroup(node);
            }

            return node;
        }

        public Node PlaceNodeInGroup(Node node)
        {
            if (!this.NodeIsChild(node))
            {
                this.AddOrGetChildNode(node);
            }
            else
            {
                var place = this.FindSpaceForNode(node);
                node.x = place.X;
                node.y = place.Y;

                this.ResizeForNode(node);
            }

            return node;
        }

        public (int X, int Y) FindSpaceForNode(Node node)
        {
            return GroupNode.FindSpaceInGroupForNode(this, node);
        }

        public bool NodeIsChild(Node node)
        {
            return this.children.Contains(node);
        }

        public bool ContainsChildNode(Node node)
        {
            if (!this.NodeIsChild(node))
            {
                return false;
            }

            if (!this.Contains(node))
            {
                return false;
            }

            return true;
        }

        public List<Node> ChildrenOverlappingNode(Node node, bool ignoreGroupNodes = false)
        {
            return this.ChildrenOverlappingBoundary(node.boundary, ignoreGroupNodes);
        }

        public List<Node> ChildrenOverlappingBoundary(Rectangle proposedBoundary, bool ignoreGroupNodes = false)
        {
            List<Node> nodesOverlapping = new List<Node>();
            foreach (var node in this.children)
            {
                if (ignoreGroupNodes && node is GroupNode)
                {
                    continue;
                }

                if (node.Overlaps(proposedBoundary))
                {
                    nodesOverlapping.Add(node);
                }

            }

            return nodesOverlapping;
        }

        public void ResizeForNode(Node node)
        {
            if (this.Contains(node))
            {
                return;
            }
            // We're going to aim for maintaining our current ratio of width to height
            // The current Group Node presumably has its current size for a reason
            // and while we're expanding it to make room, we want to make sure its
            // shape is preserved
            var sizeRatio = (double)this.width / (double)this.height;

            // If the node's bottom is below ours (its Y value + height is a higher positive value)
            // then the node is below our bounddary
            // we'll adjust our height (implicitly adjusting our bottom)
            // such that our bottom is at least 1 Margin lower
            if (node.bottom > this.bottom)
            {
                this.height = (node.bottom - this.top) + Canvas.Margin;
            }

            // If the node's top is above our top, then its
            // position is above our boundary
            // So we'll resize _and_ move
            // Resizing height only adjusts our bottom
            // so to grow upward we need to move our Y
            if (node.top < this.top)
            {
                var currentBottom = this.bottom;
                var newTop = node.top - Canvas.Margin;
                var newHeight = currentBottom - newTop;
                this.height = newHeight;
                this.y = newTop;
            }

            // Then use the ratio to determine the new width
            this.width = (int)(sizeRatio * this.height);

            // Now, we need to check to see if the node is 
            // further left than us
            // If so, we'll move and resize again
            if (node.left < this.left)
            {
                var currentRight = this.right;
                var newLeft = node.left - Canvas.Margin;
                var newWidth = currentRight - newLeft;
                this.width = newWidth;
            }

            // Finally, same as the node being below us, if it's to the right
            // we just grow to encompass it
            if (node.right > this.right)
            {
                this.width = (node.right - this.left) + Canvas.Margin;
            }

            // Then use the ratio to determine the new height
            this.height = (int)(sizeRatio * this.width);

            Console.WriteLine($"Resized Group Node to {this.width}x{this.height}");

            this.ResizeForChildren();
        }

        public void ResizeForChildren()
        {
            foreach (var node in this.children)
            {
                if (!this.Contains(node))
                {
                    this.ResizeForNode(node);
                }

            }

        }

        public void AdjustZForChildren()
        {
            foreach (var node in this.children)
            {
                if (node.z != this.z + 1)
                {
                    node.z = this.z + 1;
                }

            }

        }

        #endregion

        #region Static Methods
        public static bool IsNodeAncestorOfNode(Node ancestor, Node node)
        {
            if (node.parentNode == ancestor)
            {
                return true;
            }

            var parent = node.parentNode;
            while (parent != null)
            {
                parent = parent.parentNode;
                if (parent == ancestor)
                {
                    return true;
                }

            }

            return false;
        }

        public static (int X, int Y) FindSpaceInGroupForNode(GroupNode parent, Node child)
        {

            Rectangle proposedBoundary = new Rectangle(parent.left + Canvas.Margin, parent.top + Canvas.Margin, child.width, child.height);
            bool nodePlaced = false;

            // Group Nodes contain other Nodes by virtue of their bounding box enclosing
            // the other Node (and also being earlier in the List of Nodes in the Canvas, but
            // we don't control that here)

            // If the child node doesn't overlap with the parent group node, we need to do *math*
            // and figure out where to put it
            // We also want to avoid overlapping with other nodes, since it'll be disruptive
            // visually _and_ we could accidentally place it into another Group Node
            List<Node> overlappingNodes = parent.ChildrenOverlappingBoundary(proposedBoundary);

            if (overlappingNodes.Count == 0)
            {
                if (parent.Contains(child))
                {
                    return child.topLeft;
                }
                else
                {
                    return (proposedBoundary.Left, proposedBoundary.Top);
                }
            }

            while (!nodePlaced)
            {
                Console.WriteLine($"Proposed Position: ({proposedBoundary.X}, {proposedBoundary.Y})");

                // Get any Nodes that overlap with our proposed boundary, but aren't an ancestor
                overlappingNodes = parent.ChildrenOverlappingBoundary(proposedBoundary).Where(x => !GroupNode.IsNodeAncestorOfNode(x, child)).ToList();

                Console.WriteLine($"{overlappingNodes.Count} nodes overlap proposed position");

                // We win!
                if (overlappingNodes.Count == 0)
                {
                    nodePlaced = true;
                    continue;
                }

                // We didn't win
                // So, we scoot to the right and try again
                else
                {
                    // We're hunting for the farthest right node
                    // so we set our Right to Int.Min and grab a node
                    Node farthestRightNode = overlappingNodes.First();
                    int farthestRightPosition = int.MinValue;

                    // Then we do a simple loop to find the max X position of the overlapping nodes
                    // We can't guarantee that the vertical position is relevant
                    // i.e., one of the overlapping nodes could be above or below the center
                    // of the proposed bounding box, but we don't know if using one of their Y positions
                    // is useful. e.g., there could be a solution that is down from our current position
                    // but to the left of an overlapping node
                    // this is why we ignore the Y positions: we wouldn't want to jump over a possible
                    // (or potentially only) solution
                    foreach (var overlappingNode in overlappingNodes)
                    {
                        if (overlappingNode.right > farthestRightPosition)
                        {
                            farthestRightPosition = overlappingNode.right;
                            farthestRightNode = overlappingNode;
                        }
                    }

                    // Now all we do is use the furthest right position and add a small margin
                    proposedBoundary.X = farthestRightNode.right + Canvas.Margin;
                    Console.WriteLine($"Farthest Right Node position is {farthestRightNode.right}");
                }

                // If we've hit the right end of the Group Node
                // wrap around and bump down
                if (proposedBoundary.Right > parent.right)
                {
                    Console.WriteLine($"{proposedBoundary.Right} is outside the Group Node's right edge of {parent.right}");
                    proposedBoundary.Y += Canvas.Margin;
                    proposedBoundary.X = parent.left + Canvas.Margin;
                    Console.WriteLine($"Wrapping around to ({proposedBoundary.X},{proposedBoundary.Y})");
                }

                // We've reached the bottom
                // This means (if we've done everything right) there is no gap that can fit the node within the existing Group's
                // boundary box
                // We need to expand the Group Node to accomodate
                if (proposedBoundary.Bottom > parent.bottom + Canvas.Margin)
                {
                    Console.WriteLine("Reached the bottom of the Group Node.");
                    // We're going to aim for maintaining the current ratio of width to height
                    // The Group Node presumably has its current size for a reason
                    // and while we're expanding it to make room, we want to make sure its
                    // shape is preserved
                    var sizeRatio = (double)parent.width / (double)parent.height;

                    // We'll increase the height
                    var newHeight = parent.height + child.height + (Canvas.Margin * 2);

                    // we're converting double to int and will lose some precision but it's fine for now
                    var newWidth = (int)(sizeRatio * newHeight);

                    // However we can try to be clever and predict if the node will fit in the new width
                    // after adjusting to maintain the ratio
                    var addedWidth = newWidth - parent.width;

                    // If the ratio created width to accomodate the child, we can put it in that column
                    // and start from the top
                    if (addedWidth > child.width + (Canvas.Margin * 2))
                    {
                        // parent.right is the current right border, before adding the new width
                        proposedBoundary.X = parent.right + Canvas.Margin;
                        proposedBoundary.Y = parent.top + Canvas.Margin;
                    }
                    else
                    {
                        proposedBoundary.X = parent.left + Canvas.Margin;
                        // parent.bottom is the _current_ bottom, but we're adding child.height + Canvas.Margin
                        // meaning the current paren.bottom + Canvas.Margin will be empty on resizing
                        proposedBoundary.Y = parent.bottom + Canvas.Margin;
                    }

                    nodePlaced = true;
                    continue;
                }

            }

            Console.WriteLine($"Node placed! ({proposedBoundary.X},{proposedBoundary.Y})");

            return new (proposedBoundary.X, proposedBoundary.Y);
        }

        #endregion

    }

    public class GroupNodeBackgroundStyle
    {
        private GroupNodeBackgroundStyle(string value)
        {
            this.Value = value;
        }

        public string Value
        {
            get;
            private set;
        }

        public static GroupNodeBackgroundStyle cover
        {
            get
            {
                return new GroupNodeBackgroundStyle("cover");
            }

        }

        public static GroupNodeBackgroundStyle ratio
        {
            get
            {
                return new GroupNodeBackgroundStyle("ratio");
            }

        }

        public static GroupNodeBackgroundStyle repeat
        {
            get
            {
                return new GroupNodeBackgroundStyle("repeat");
            }

        }


    }

    public class GroupNodeBackgroundStyleJsonConverter : JsonConverter<GroupNodeBackgroundStyle>
    {
        public override GroupNodeBackgroundStyle? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            switch (reader.GetString()!)
            {
                case "cover":
                {
                    return GroupNodeBackgroundStyle.cover;
                }
                case "ratio":
                {
                    return GroupNodeBackgroundStyle.ratio;
                }
                case "repeat":
                {
                    return GroupNodeBackgroundStyle.repeat;
                }
                default:
                {
                    return null;
                }

            }

        }

        public override void Write(
            Utf8JsonWriter writer,
            GroupNodeBackgroundStyle groupNodeBackgroundStyle,
            JsonSerializerOptions options
        )
        {
            writer.WriteStringValue(groupNodeBackgroundStyle.Value);
        }

    }

}