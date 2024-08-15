using System.Drawing;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        public string id { get; set; }

        [JsonInclude]
        public int x { get; set; }

        [JsonInclude]
        public int y { get; set; }

        [JsonInclude]
        public int width { get; set; }

        [JsonInclude]
        public int height { get; set; }

        [JsonInclude]
        public CanvasColor? color { get; set; }

        #endregion

        #region Object Reference Fields

        [JsonIgnore]
        public List<Edge> Edges { get; set; }

        [JsonIgnore]
        public GroupNode? parentNode { get; set; }

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
            if (
                firstNode.boundingBox.IntersectsWith(secondNode.boundingBox) ||
                secondNode.boundingBox.IntersectsWith(firstNode.boundingBox)
            )
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
        public string? text { get; set; }

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
        public string file { get; set; }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? subpath { get; set; }

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
        public string url { get; set; }

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

            this.nodes = new List<Node>();
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
        public List<Node> nodes;
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