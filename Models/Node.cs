using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;


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
            int nodeX,
            int nodeY,
            int nodeWidth,
            int nodeHeight,
            string? nodeId = null
        )
        {
            if (nodeId == null)
            {
                this.id = Guid.NewGuid().ToString();
            }
            else
            {
                this.id = nodeId;
            }

            this.x = nodeX;
            this.y = nodeY;

            this.width = nodeWidth;
            this.height = nodeHeight;

            this.Edges = new List<Edge>();
        }

        #region JSON fields

        [JsonInclude]
        public string id
        {
            get;
            set;
        }

        [JsonInclude]
        public int x
        {
            get;
            set;
        }

        [JsonInclude]
        public int y
        {
            get;
            set;
        }

        [JsonInclude]
        public int width { get; set; }

        [JsonInclude]
        public int height { get; set; }

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
        public GroupNode? parentNode
        {
            get;
            set;
        }

        [JsonIgnore]
        public Canvas? Canvas
        {
            get;
            set;
        }

        #endregion

        #region Derived Properties

        [JsonIgnore]
        public int bottom
        {
            get
            {
                return this.y + this.height;
            }

        }

        [JsonIgnore]
        public int top
        {
            get
            {
                return this.y;
            }

        }

        [JsonIgnore]
        public int left
        {
            get
            {
                return this.x;
            }

        }

        [JsonIgnore]
        public int right
        {
            get
            {
                return this.x + this.width;
            }

        }

        [JsonIgnore]
        public Tuple<int, int> topRight
        {
            get
            {
                return new Tuple<int, int>(this.right, this.top);
            }
            set
            {
                this.x = value.Item1 - this.width;
                this.y = value.Item2;
            }

        }

        [JsonIgnore]
        public Tuple<int, int> bottomRight
        {
            get
            {
                return new Tuple<int, int>(this.right, this.bottom);
            }
            set
            {
                this.x = value.Item1 - this.width;
                this.y = value.Item2 - this.height;
            }

        }

        [JsonIgnore]
        public Tuple<int, int> bottomLeft
        {
            get
            {
                return new Tuple<int, int>(this.left, this.bottom);
            }
            set
            {
                this.x = value.Item1;
                this.y = value.Item2 - this.height;
            }

        }

        [JsonIgnore]
        public Tuple<int, int> topLeft
        {
            get
            {
                return new Tuple<int, int>(this.left, this.top);
            }
            set
            {
                this.x = value.Item1;
                this.y = value.Item2;
            }

        }

        [JsonIgnore]
        public Tuple<int, int> leftEdgePosition
        {
            get
            {
                return new Tuple<int, int>(this.left, this.top + (this.height / 2));
            }

        }

        [JsonIgnore]
        public Tuple<int, int> rightEdgePosition
        {
            get
            {
                return new Tuple<int, int>(this.right, this.top + (this.height / 2));
            }

        }

        [JsonIgnore]
        public Tuple<int, int> topEdgePosition
        {
            get
            {
                return new Tuple<int, int>(this.left + (this.width / 2), this.top);
            }

        }

        [JsonIgnore]
        public Tuple<int, int> bottomEdgePosition
        {
            get
            {
                return new Tuple<int, int>(this.left + (this.width / 2), this.bottom);
            }

        }

        [JsonIgnore]
        public Rectangle boundingBox
        {
            get
            {
                return new Rectangle(this.left, this.top, this.width, this.height);
            }

        }

        #endregion

        #region Instance Methods

        public Tuple<int, int> GetEdgeSidePosition(EdgeSide edgeSide)
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
            return group.Contains(this);
        }

        public bool OverlapsNode(Node targetNode)
        {
            return Node.DoesNodeOverlapNode(this, targetNode);
        }

        public static Tuple<bool, EdgeSide?> IsNodeTouchingNode(Node sourceNode, Node destinationNode)
        {
            var bottomTouchingTop = (
                sourceNode.bottom >= destinationNode.top
            ) &&
            (
                sourceNode.top <= destinationNode.top
            ) &&
            (
                (
                    sourceNode.right > destinationNode.left &&
                    sourceNode.right <= destinationNode.right
                ) ||
                (
                    sourceNode.left < destinationNode.right &&
                    sourceNode.left >= destinationNode.left
                )
            );

            var topTouchingBottom = (
                sourceNode.top <= destinationNode.bottom
            ) &&
            (
                sourceNode.bottom >= destinationNode.bottom
            ) &&
            (
                (
                    sourceNode.right > destinationNode.left &&
                    sourceNode.right <= destinationNode.right
                ) ||
                (
                    sourceNode.left < destinationNode.right &&
                    sourceNode.left >= destinationNode.left
                )
            );

            var leftTouchingRight = (
                sourceNode.left <= destinationNode.right
            ) &&
            (
                sourceNode.right >= destinationNode.right
            ) &&
            (
                (
                    sourceNode.top < destinationNode.bottom &&
                    sourceNode.bottom >= destinationNode.bottom
                ) ||
                (
                    sourceNode.bottom > destinationNode.top &&
                    sourceNode.bottom <= destinationNode.bottom
                )
            );

            var rightTouchingLeft = (
                sourceNode.right >= destinationNode.left
            ) &&
            (
                sourceNode.left <= destinationNode.left
            ) &&
            (
                (
                    sourceNode.top <= destinationNode.bottom &&
                    sourceNode.bottom >= destinationNode.bottom
                ) ||
                (
                    sourceNode.bottom >= destinationNode.top &&
                    sourceNode.bottom <= destinationNode.bottom
                )
            );

            if (bottomTouchingTop)
            {
                return new Tuple<bool, EdgeSide?>(true, EdgeSide.bottom);
            }
            if (topTouchingBottom)
            {
                return new Tuple<bool, EdgeSide?>(true, EdgeSide.top);
            }
            if (rightTouchingLeft)
            {
                return new Tuple<bool, EdgeSide?>(true, EdgeSide.right);
            }
            if (leftTouchingRight)
            {
                return new Tuple<bool, EdgeSide?>(true, EdgeSide.left);
            }

            return new Tuple<bool, EdgeSide?>(false, null);
        }

        #endregion

        #region Static Methods

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

        public static bool DoesNodeOverlapNode(Node firstNode, Node secondNode)
        {
            if (firstNode.boundingBox.IntersectsWith(secondNode.boundingBox))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
    public class TextNode : Node
    {
        public TextNode(
            int nodeX,
            int nodeY,
            int nodeWidth,
            int nodeHeight,
            string? nodeText = null,
            string? nodeId = null
        ) : base(
            nodeX,
            nodeY,
            nodeWidth,
            nodeHeight,
            nodeId: nodeId
        )
        {
            this.text = nodeText;
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
            int nodeX,
            int nodeY,
            int nodeWidth,
            int nodeHeight,
            string? subPath = null,
            string? nodeId = null
        ) : base(
            nodeX,
            nodeY,
            nodeWidth,
            nodeHeight,
            nodeId: nodeId
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
            string nodeUrl,
            int nodeX,
            int nodeY,
            int nodeWidth,
            int nodeHeight,
            string? nodeId = null
        ) : base(
            nodeX,
            nodeY,
            nodeWidth,
            nodeHeight,
            nodeId: nodeId
        )
        {
            this.url = nodeUrl;
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
            int nodeX,
            int nodeY,
            int nodeWidth,
            int nodeHeight,
            string? nodeLabel = null,
            string? nodeBackground = null,
            GroupNodeBackgroundStyle? nodeBackgroundStyle = null,
            string? nodeId = null
        ) : base(
            nodeX,
            nodeY,
            nodeWidth,
            nodeHeight,
            nodeId: nodeId
        )
        {
            this.label = nodeLabel;

            this.background = nodeBackground;
            this.backgroundStyle = nodeBackgroundStyle;

            this.children = new List<Node>();
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

        public void AddChildNode(Node node, bool updateChildPosition = true)
        {
            if (this.Canvas == null && updateChildPosition)
            {
                throw new InvalidOperationException("Group Nodes must be on a Canvas to update a child node's position");
            }

            this.children.Add(node);

            node.parentNode = this;
            node.Canvas = this.Canvas;
        }

        public bool Contains(Node node)
        {
            return this.children.Contains(node);
        }

        public List<Node> ChildrenOverlappingBoundary(Rectangle proposedBoundingBox, bool ignoreGroupNodes = false)
        {
            List<Node> nodesOverlapping = new List<Node>();
            foreach (var node in this.children)
            {
                if (ignoreGroupNodes && node is GroupNode)
                {
                    continue;
                }

                if (node.boundingBox.IntersectsWith(proposedBoundingBox))
                {
                    nodesOverlapping.Add(node);
                }

            }

            return nodesOverlapping;
        }

        public static Tuple<int, int> FindSpaceInGroupForNode(GroupNode parent, Node child)
        {
            // Group Nodes contain other Nodes by virtue of their bounding box enclosing
            // the other Node (and also being earlier in the List of Nodes in the Canvas, but
            // we don't control that here)

            // If the child node doesn't overlap with the parent group node, we need to do *math*
            // and figure out where to put it
            // We also want to avoid overlapping with other nodes, since it'll be disruptive
            // visually _and_ we could accidentally place it into another Group Node
            if (parent.OverlapsNode(child))
            {
                return child.topLeft;
            }

            List<Node> overlappingNodes = new List<Node>();
            
            // Add a small margin of 8 units
            int proposedNewY = parent.top + 8;
            int proposedNewX = parent.left + 8;

            Rectangle proposedBoundingBox = new Rectangle(proposedNewX, proposedNewY, child.width, child.height);
            bool nodePlaced = false;

            while (!nodePlaced)
            {
                proposedBoundingBox.X = proposedNewX;
                proposedBoundingBox.Y = proposedNewY;

                // Get any Nodes that overlap with our proposed bounding box
                overlappingNodes = parent.ChildrenOverlappingBoundary(proposedBoundingBox);
                    
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
                    // Of course, we have > 0 Nodes and one of them has a right side
                    // that is farther than the rest
                    Node farthestRightNode = overlappingNodes.First();
                    int farthestRightPosition = int.MinValue;
                        
                    // So we do a simple loop to find the max X position of the overlapping nodes
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
                    proposedNewX = farthestRightNode.right + 8;
                }

                // If we've hit the right end of the Group Node
                // wrap around and bump down
                if (proposedNewX + child.width > parent.right + 8)
                {
                    proposedNewY += 8;
                    proposedNewX = parent.left + 8;
                }

                // We've reached the bottom
                // This means (if we've done everything right) there is no gap that can fit the node within the existing Group's
                // boundary box
                // We need to expand the Group Node to accomodate
                if (proposedNewY + child.height > parent.bottom + 8)
                {
                    // We're going to aim for maintaining the current ratio of width to height
                    // The Group Node presumably has its current size for a reason
                    // and while we're expanding it to make room, we want to make sure its
                    // shape is preserved
                    var sizeRatio = (double)parent.width / (double)parent.height;

                    // We'll increase the height
                    var newHeight = parent.height + child.height + 16;

                    // we're converting double to int and will lose some precision but it's fine for now
                    var newWidth = (int)(sizeRatio * newHeight);

                    // However we can try to be clever and predict if the node will fit in the new width
                    // after adjusting to maintain the ratio
                    var addedWidth = newWidth - parent.width;

                    // If the ratio created width to accomodate the child, we can put it in that column
                    // and start from the top
                    if (addedWidth > child.width + 16)
                    {
                        // parent.right is the current right border, before adding the new width
                        proposedNewX = parent.right + 8;
                        proposedNewY = parent.top + 8;
                    }
                    else
                    {
                        proposedNewX = parent.left + 8;
                        proposedNewY = parent.bottom - child.height - 8;
                    }

                    nodePlaced = true;
                    continue;
                }

            }

            return new Tuple<int, int>(proposedNewX, proposedNewY);

        }

        public void ResizeForNode(Node node)
        {
            // We're going to aim for maintaining our current ratio of width to height
            // The current Group Node presumably has its current size for a reason
            // and while we're expanding it to make room, we want to make sure its
            // shape is preserved
            var sizeRatio = (double)this.width / (double)this.height;

            // We'll increase the height
            this.height += node.height + 16;
            // Then use the ratio to determine the new width
            this.width = (int)(sizeRatio * this.height);
        }

        public void CheckChildNodes()
        {
            foreach (var node in this.children)
            {
                if (!node.OverlapsNode(this) ||
                    node.x > this.right ||
                    node.y > this.bottom ||
                    node.x < this.left ||
                    node.y < this.top
                )
                {

                }
            }
        }

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