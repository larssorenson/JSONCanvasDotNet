using System.Text.Json;
using System.Text.Json.Serialization;

namespace JSONCanvasDotNet.Models
{

    public class Edge
    {
        public Edge(
            string fromNodeId,
            string toNodeId,
            string? id = null,
            EdgeSide? fromEdgeSide = null,
            EdgeEnd? fromEdgeEnd = null,
            EdgeSide? toEdgeSide = null,
            EdgeEnd? toEdgeEnd = null,
            CanvasColor? color = null,
            string? label = null
        )
        {
            this.id = id ?? Guid.NewGuid().ToString();

            this.fromNodeId = fromNodeId;
            this.fromSide = fromEdgeSide;
            this.fromEnd = fromEdgeEnd;

            this.toNodeId = toNodeId;
            this.toSide = toEdgeSide;
            this.toEnd = toEdgeEnd;

            this.color = color;
            this.label = label;
        }

        public Edge(
            Node fromNode,
            Node toNode,
            string? id = null,
            EdgeSide? fromEdgeSide = null,
            EdgeEnd? fromEdgeEnd = null,
            EdgeSide? toEdgeSide = null,
            EdgeEnd? toEdgeEnd = null,
            CanvasColor? color = null,
            string? label = null
        ): this(
            fromNode.id,
            toNode.id,
            id: id,
            fromEdgeSide: fromEdgeSide,
            fromEdgeEnd: fromEdgeEnd,
            toEdgeSide: toEdgeSide,
            toEdgeEnd: toEdgeEnd,
            color: color,
            label: label
        )
        {
            this.fromNode = fromNode;
            this.fromNode.Edges.Add(this);

            this.toNode = toNode;
            this.toNode.Edges.Add(this);
        }

        [JsonInclude]
        public string id;

        [JsonIgnore]
        public Node? fromNode;

        [JsonInclude]
        [JsonPropertyName("fromNode")]
        public string fromNodeId;

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(EdgeSideJsonConverter))]
        public EdgeSide? fromSide;

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(EdgeEndJsonConverter))]
        public EdgeEnd? fromEnd;

        [JsonInclude]
        [JsonPropertyName("toNode")]
        public string toNodeId;

        [JsonIgnore]
        public Node? toNode;

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(EdgeSideJsonConverter))]
        public EdgeSide? toSide;

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(EdgeEndJsonConverter))]
        public EdgeEnd? toEnd;

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonConverter(typeof(CanvasColorJsonConverter))]
        public CanvasColor? color;

        [JsonInclude]
        public string? label;

        [JsonIgnore]
        private static readonly List<EdgeSide> allEdgeSides = new List<EdgeSide>([
            EdgeSide.top,
            EdgeSide.bottom,
            EdgeSide.left,
            EdgeSide.right
        ]);

        public static (EdgeSide fromSide, EdgeSide toSide)? ShortestEdgeSidesBetweenNodeAndNode(
            Node sourceNode,
            Node destinationNode,
            List<EdgeSide>? sourceEdgeSideHints = null,
            List<EdgeSide>? destinationEdgeSideHints = null
        )
        {
            List<EdgeSide> fromEdgeSidesToTry = new List<EdgeSide>(Edge.allEdgeSides.Count);

            if (sourceEdgeSideHints == null || sourceEdgeSideHints.Count == 0)
            {
                fromEdgeSidesToTry.AddRange(allEdgeSides);
            }
            else
            {
                fromEdgeSidesToTry.AddRange(sourceEdgeSideHints);
            }

            List<EdgeSide> toEdgeSidesToTry = new List<EdgeSide>(Edge.allEdgeSides.Count);

            if (destinationEdgeSideHints == null || destinationEdgeSideHints.Count == 0)
            {
                toEdgeSidesToTry.AddRange(allEdgeSides);
            }
            else
            {
                toEdgeSidesToTry.AddRange(destinationEdgeSideHints);
            }

            List<EdgeSide> fromEdgeSidesToIgnore = new List<EdgeSide>(Edge.allEdgeSides.Count);
            foreach (var edge in sourceNode.Edges)
            {
                if (edge.fromSide != null)
                {
                    fromEdgeSidesToIgnore.Add(edge.fromSide);
                }
            }
            List<EdgeSide> toEdgeSidesToIgnore = new List<EdgeSide>(Edge.allEdgeSides.Count);
            foreach (var edge in destinationNode.Edges)
            {
                if (edge.toSide != null)
                {
                    toEdgeSidesToIgnore.Add(edge.toSide);
                }
            }

            double shortestDistance = double.MaxValue;
            (EdgeSide fromSide, EdgeSide toSide)? shortestPath = null;

            foreach (var fromEdgeSide in fromEdgeSidesToTry)
            {
                if (fromEdgeSidesToIgnore.Contains(fromEdgeSide))
                {
                    Console.WriteLine($"From {fromEdgeSide} was ignored, skipping.");
                    continue;
                }

                Console.WriteLine($"Testing from EdgeSide {fromEdgeSide.Value}");

                var fromEdgeSidePosition = sourceNode.GetEdgeSidePosition(fromEdgeSide);
                Console.WriteLine($"\tPosition: {fromEdgeSidePosition}");

                foreach (var toEdgeSide in toEdgeSidesToTry)
                {

                    if (toEdgeSidesToIgnore.Contains(toEdgeSide))
                    {
                        Console.WriteLine($"To {toEdgeSide} is ignored, skipping.");
                        continue;
                    }

                    Console.WriteLine($"Testing to EdgeSide {toEdgeSide.Value}");

                    var toEdgeSidePosition = destinationNode.GetEdgeSidePosition(toEdgeSide);
                    Console.WriteLine($"\tPosition: {toEdgeSidePosition}");

                    var horizontalDistance = (double)fromEdgeSidePosition.X - toEdgeSidePosition.X;
                    Console.WriteLine($"Horizontal Distance: {horizontalDistance}");

                    var verticalDistance = (double)fromEdgeSidePosition.Y - toEdgeSidePosition.Y;
                    Console.WriteLine($"Vertical Distance: {verticalDistance}");

                    var distance = Math.Sqrt(
                        Math.Pow(
                            horizontalDistance,
                            2.0
                        ) +
                        Math.Pow(
                            verticalDistance,
                            2.0
                        )
                    );

                    Console.WriteLine($"Distance: {distance}");
                    if (distance == 0.0)
                    {
                        Console.WriteLine("This edge isn't visible. Ignoring these edge sides and skipping this result.");
                        fromEdgeSidesToIgnore.Add(fromEdgeSide);
                        toEdgeSidesToIgnore.Add(toEdgeSide);
                        continue;
                    }

                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        shortestPath = new (fromEdgeSide, toEdgeSide);
                        Console.WriteLine($"New shortest path {shortestPath} of {shortestDistance}");
                    }

                }

            }

            // On review, I'm wondering how this case could occur
            // How can we end up with a path that isn't shorter than int.Max?
            // Two equally sized nodes perfectly overlapping can still have an edge
            // One node larger than another (e.g., a group node) where one is perfectly encased in the other
            // can still have an edge
            // Any other scenarios should produce an edge regardless, even if they're touching on one side
            if (shortestPath == null)
            {
                throw new Exception("Somehow, there cannot be any path between two nodes.");

                /*Console.WriteLine("No visible shortest path was calculated. Trying again with all options, minus the ignored ones.");

                var newFromEdgeSidesToTry = new List<EdgeSide>(allEdgeSides.Where(edgeSide => !fromEdgeSidesToIgnore.Contains(edgeSide)));
                var newToEdgeSidesToTry = new List<EdgeSide>(allEdgeSides.Where(edgeSide => !toEdgeSidesToIgnore.Contains(edgeSide)));

                shortestPath = Edge.ShortestEdgeSidesBetweenNodeAndNode(
                    sourceNode,
                    destinationNode,
                    sourceEdgeSideHints: newFromEdgeSidesToTry,
                    destinationEdgeSideHints: newToEdgeSidesToTry
                );*/
            }

            Console.WriteLine($"{shortestPath} wins!");
            return shortestPath;
        }

        public static Edge ConnectNodeToNode(
            Node sourceNode,
            Node destinationNode,
            string? edgeId = null,
            string? edgeLabel = null,
            CanvasColor? edgeColor = null
        )
        {
            var sourceIsToTheLeft = Node.IsNodeLeftOfNode(sourceNode, destinationNode);
            var sourceIsToTheRight = Node.IsNodeRightOfNode(sourceNode, destinationNode);
            var sourceIsBelow = Node.IsNodeBelowNode(sourceNode, destinationNode);
            var sourceIsAbove = Node.IsNodeAboveNode(sourceNode, destinationNode);

            Console.WriteLine($"Is source left ({sourceIsToTheLeft}), right ({sourceIsToTheRight}), above ({sourceIsAbove}), or below ({sourceIsBelow})?");

            EdgeSide? fromSide = null;
            EdgeSide? toSide = null;
            List<EdgeSide> fromSidesToTry = new List<EdgeSide>(Edge.allEdgeSides.Count);
            List<EdgeSide> toSidesToTry = new List<EdgeSide>(Edge.allEdgeSides.Count);

            // Use naive assumptions about the positioning of the nodes
            // e.g., [ ] -> [ ] Source on the Left, Destination on the Right, so we use Right and Left sides respectively
            if (sourceIsToTheLeft)
            {
                Console.WriteLine($"Adding from {EdgeSide.right} to {EdgeSide.left} to test.");
                fromSidesToTry.Add(EdgeSide.right);
                toSidesToTry.Add(EdgeSide.left);
            }
            else if (sourceIsToTheRight)
            {
                Console.WriteLine($"Adding from {EdgeSide.left} to {EdgeSide.right} to test.");
                fromSidesToTry.Add(EdgeSide.left);
                toSidesToTry.Add(EdgeSide.right);
            }
            else
            {
                // If a node isn't to the left or right, either side might be viable
                fromSidesToTry.Add(EdgeSide.left);
                fromSidesToTry.Add(EdgeSide.right);
                toSidesToTry.Add(EdgeSide.left);
                toSidesToTry.Add(EdgeSide.right);
            }

            // e.g., [ ]  Source above, Destination below, so we use Bottom and Top sides respectively
            //        |
            //        v
            //       [ ]
            if (sourceIsBelow)
            {
                Console.WriteLine($"Adding from {EdgeSide.top} to {EdgeSide.bottom} to test.");
                fromSidesToTry.Add(EdgeSide.top);
                toSidesToTry.Add(EdgeSide.bottom);
            }
            else if (sourceIsAbove)
            {
                Console.WriteLine($"Adding from {EdgeSide.bottom} to {EdgeSide.top} to test.");
                fromSidesToTry.Add(EdgeSide.bottom);
                toSidesToTry.Add(EdgeSide.top);
            }
            else
            {
                // If a node isn't to above or below, either side might be viable
                fromSidesToTry.Add(EdgeSide.bottom);
                fromSidesToTry.Add(EdgeSide.top);
                toSidesToTry.Add(EdgeSide.bottom);
                toSidesToTry.Add(EdgeSide.top);
            }

            var results = Node.IsNodeTouchingNode(sourceNode, destinationNode);
            Console.WriteLine($"Is Node Touching on an Edge? {results}");

            if (results.isTouching)
            {
                var touchingEdge = results.touchingEdge;

                if (touchingEdge != null)
                {
                    fromSidesToTry.Remove(touchingEdge);
                    toSidesToTry.Remove(touchingEdge.opposite);
                }

            }

            // In the case of a Node, e.g., to the left of and below another Node
            // we end up with 2 possible sides to go from or to
            // This function will try to return the shortest path of the two which we use to settle
            // between one or the other  
            (EdgeSide fromSide, EdgeSide toSide)? shortestPath = Edge.ShortestEdgeSidesBetweenNodeAndNode(
                sourceNode,
                destinationNode,
                sourceEdgeSideHints: fromSidesToTry,
                destinationEdgeSideHints: toSidesToTry
            );

            if (shortestPath.HasValue)
            {
                fromSide = shortestPath.Value.fromSide;
                toSide = shortestPath.Value.toSide;
            }

            else
            {
                throw new Exception("This is odd. I couldn't find a path between the two nodes to connect an edge.");
            }

            var newEdge = new Edge(
                fromNode: sourceNode,
                toNode: destinationNode,
                id: edgeId,
                fromEdgeSide: fromSide,
                toEdgeSide: toSide,
                toEdgeEnd: EdgeEnd.arrow,
                color: edgeColor,
                label: edgeLabel
            );

            sourceNode.Edges.Add(newEdge);
            destinationNode.Edges.Add(newEdge);

            return newEdge;
        }
    }

    public class EdgeSide
    {
        private EdgeSide(string value)
        {
            this.Value = value;
        }

        public string Value
        {
            get;
            private set;
        }


        public static EdgeSide top
        {
            get
            {
                return new EdgeSide("top");
            }
        }

        public static EdgeSide right
        {
            get
            {
                return new EdgeSide("right");
            }
        }

        public static EdgeSide bottom
        {
            get
            {
                return new EdgeSide("bottom");
            }
        }

        public static EdgeSide left
        {
            get
            {
                return new EdgeSide("left");
            }
        }

        [JsonIgnore]
        public EdgeSide opposite
        {
            get
            {
                if (this.Equals(EdgeSide.top))
                {
                    return EdgeSide.bottom;
                }
                else if (this.Equals(EdgeSide.left))
                {
                    return EdgeSide.right;
                }
                else if (this.Equals(EdgeSide.right))
                {
                    return EdgeSide.left;
                }
                else
                {
                    return EdgeSide.top;
                }

            }

        }


        public override string ToString()
        {
            return this.Value;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is EdgeSide)
            {
                return this.Value.Equals(((EdgeSide)obj).Value);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Value);
        }

    }

    public class EdgeEnd
    {
        private EdgeEnd()
        {
            this.Value = "none";
        }

        private EdgeEnd(string value)
        {
            this.Value = value;
        }

        public string Value
        {
            get; private set;
        }

        public static EdgeEnd none
        {
            get
            {
                return new EdgeEnd("none");
            }

        }

        public static EdgeEnd arrow
        {
            get
            {
                return new EdgeEnd("arrow");
            }

        }

        public override string ToString()
        {
            return this.Value;
        }

    }


    public class EdgeSideJsonConverter : JsonConverter<EdgeSide>
    {
        public override EdgeSide? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            switch (reader.GetString()!)
            {
                case "top":
                {
                    return EdgeSide.top;
                }
                case "right":
                {
                    return EdgeSide.right;
                }
                case "bottom":
                {
                    return EdgeSide.bottom;
                }
                case "left":
                {
                    return EdgeSide.left;
                }
                default:
                {
                    return null;
                }

            }

        }

        public override void Write(
            Utf8JsonWriter writer,
            EdgeSide edgeSide,
            JsonSerializerOptions options
        )
        {
            writer.WriteStringValue(edgeSide.Value);
        }

    }
    public class EdgeEndJsonConverter : JsonConverter<EdgeEnd>
    {
        public override EdgeEnd? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            switch (reader.GetString()!)
            {
                case "none":
                {
                    return EdgeEnd.none;
                }
                case "arrow":
                {
                    return EdgeEnd.arrow;
                }
                default:
                {
                    return null;
                }

            }

        }

        public override void Write(
            Utf8JsonWriter writer,
            EdgeEnd edgeEnd,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(edgeEnd.Value);
        }

    }

}