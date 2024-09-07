using System.Drawing;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JSONCanvasDotNet.Models
{

    public class Canvas
    {
        public Canvas(List<Node>? nodes = null, List<Edge>? edges = null)
        {
            this.Nodes = nodes ?? new List<Node>();

            this.AddOrGetNodes(this.Nodes);

            this.Edges = edges ?? new List<Edge>();

            this.AddOrGetEdges(this.Edges);
        }

        #region Instance Methods
        public Node AddOrGetNode(string nodeId)
        {
            var existingNode = this.nodeLookup.GetValueOrDefault(nodeId);
            if (existingNode != null)
            {
                return existingNode;
            }

            var place = this.FindSpaceForNode();

            var newNode = new Node(
                x: place.X,
                y: place.Y,
                id: nodeId
            );

            return this.AddOrGetNode(newNode);
        }

        public Node AddOrGetNode(Node newNode)
        {
            var existingNode = this.nodeLookup.GetValueOrDefault(newNode.id);
            if (existingNode != null)
            {
                return existingNode;
            }

            Console.WriteLine(this.nodeLookup.Count);

            this.nodeLookup[newNode.id] = newNode;

            newNode.Canvas = this;

            this.AdjustBoundsForNode(newNode);

            var nodesContaining = this.GetNodesContainingBoundary(newNode.boundary);
            Console.WriteLine("Node {0} is contained by {1} nodes", newNode.id, nodesContaining.Count);

            newNode.z = nodesContaining.Count;
            Console.WriteLine("Setting Node {0} z index to {1}", newNode.id, newNode.z);

            foreach (var node in nodesContaining)
            {
                if (node is GroupNode)
                {
                    if (newNode.parentNode == null)
                    {
                        Console.WriteLine("Updating Node {0}'s parent to {1}", newNode.id, node.id);
                        ((GroupNode)node).AddOrGetChildNode(newNode);
                    }
                    else
                    {
                        var newNodeRoot = newNode.GetRoot();
                        if (newNodeRoot != node)
                        {
                            Console.WriteLine("Updating Node {0}'s parent to {1}", newNodeRoot.id, node.id);
                            ((GroupNode)node).AddOrGetChildNode(newNodeRoot);
                        }
                    }

                }

            }

            var nodesContained = this.GetNodesContainedByBoundary(newNode.boundary);
            Console.WriteLine("Node {0} has {1} nodes contained in it", newNode.id, nodesContained.Count);

            if (newNode is GroupNode)
            {
                foreach (var node in nodesContained)
                {
                    Console.WriteLine("Updating Node {0}'s parent to {1}", node.id, newNode.id);
                    ((GroupNode)newNode).AddOrGetChildNode(node);

                    // TODO: Figure out Z index adjustments of children
                    if (newNode.z >= node.z)
                    {
                        Console.WriteLine("Updating Node {0}'s z index to {1}", node.id, node.z + 1);
                        node.z = newNode.z + 1;
                    }

                }

            }

            else
            {
                foreach (var node in nodesContained)
                {
                    this.PlaceNode(node);
                }

            }

            this.Nodes.Add(newNode);
            return newNode;
        }

        public Rectangle AdjustBoundsForNode(Node node)
        {
            if (node.left < this.boundary.Left)
            {
                this.boundary.X = node.left - Canvas.Margin;
            }

            if (node.top < this.boundary.Top)
            {
                this.boundary.Y = node.top - Canvas.Margin;
            }

            if (node.bottom > this.boundary.Bottom)
            {
                this.boundary.Height = this.boundary.Height + (node.bottom - this.boundary.Bottom) + Canvas.Margin;
            }

            if (node.right > this.boundary.Right)
            {
                this.boundary.Width = this.boundary.Width + (node.right - this.boundary.Right) + Canvas.Margin;
            }

            return this.boundary;
        }

        public Node PlaceNode(Node proposedNode)
        {
            var nodeToPlace = this.AddOrGetNode(proposedNode);

            if (nodeToPlace.parentNode != null)
            {
                nodeToPlace.parentNode.PlaceNodeInGroup(proposedNode);
            }
            else
            {
                var place = this.FindSpaceForNode(nodeToPlace);

                nodeToPlace.x = place.X;
                nodeToPlace.y = place.Y;
            }

            this.AdjustBoundsForNode(nodeToPlace);

            return nodeToPlace;
        }

        public Rectangle FindSpaceForNode()
        {
            Rectangle proposedBoundary = new Rectangle(this.boundary.Left, this.boundary.Top, Node.DefaultWidth, Node.DefaultHeight);

            return this.FindSpaceForBoundary(proposedBoundary);
        }

        public Rectangle FindSpaceForNode(Node proposedNode)
        {
            Rectangle proposedBoundary = new Rectangle(this.boundary.Left, this.boundary.Top, proposedNode.width, proposedNode.height);

            return this.FindSpaceForBoundary(proposedBoundary);
        }

        public Rectangle FindSpaceForNode(int width, int height)
        {
            Rectangle proposedBoundary = new Rectangle(this.boundary.Left, this.boundary.Top, width, height);

            return this.FindSpaceForBoundary(proposedBoundary);
        }

        public Rectangle FindSpaceForBoundary(Rectangle boundary)
        {
            bool nodePlaced = false;
            List<Node> nodesOverlapping = new List<Node>();
            Rectangle proposedBoundary = boundary;

            foreach (var node in this.Nodes)
            {
                proposedBoundary.X = node.right + Canvas.Margin;
                proposedBoundary.Y = node.top;

                nodesOverlapping = this.GetNodesOverlappingBoundary(proposedBoundary);

                if (nodesOverlapping.Count == 0)
                {
                    nodePlaced = true;
                    break;
                }

            }

            if (!nodePlaced)
            {
                do
                {
                    proposedBoundary.X += Canvas.Margin;
                    nodesOverlapping = this.GetNodesOverlappingBoundary(proposedBoundary);
                }
                while (nodesOverlapping.Count > 0);
            }

            return proposedBoundary;
        }

        public List<Node> AddOrGetNodes(List<Node> newNodes)
        {
            List<Node> results = new List<Node>(newNodes.Count);
            foreach (var node in newNodes)
            {
                results.Add(this.AddOrGetNode(node));
            }

            return results;
        }

        public Edge AddOrGetEdge(Edge newEdge)
        {
            var existingEdge = this.edgeLookup.GetValueOrDefault(newEdge.id);
            if (existingEdge != null)
            {
                return existingEdge;
            }

            var fromNode = this.AddOrGetNode(newEdge.fromNodeId);
            if (newEdge.fromNode == null || newEdge.fromNode != fromNode)
            {
                newEdge.fromNode = fromNode;
            }
            var toNode = this.AddOrGetNode(newEdge.toNodeId);
            if (newEdge.toNode == null || newEdge.toNode != toNode)
            {
                newEdge.toNode = toNode;
            }

            this.edgeLookup[newEdge.id] = newEdge;

            this.Edges.Add(newEdge);

            newEdge.fromNode.Edges.Add(newEdge);
            newEdge.toNode.Edges.Add(newEdge);

            return newEdge;
        }

        public List<Edge> AddOrGetEdges(List<Edge> edges)
        {
            List<Edge> results = new List<Edge>();
            foreach (var edge in edges)
            {
                results.Add(this.AddOrGetEdge(edge));
            }

            return results;
        }

        public List<Node> GetNodesAtCoordinates(int x, int y, bool ignoreGroupNodes = false)
        {
            Console.WriteLine("{0},{1}", x, y);
            List<Node> nodes = new List<Node>();
            foreach (var node in this.Nodes)
            {
                if (ignoreGroupNodes && node is GroupNode)
                {
                    Console.WriteLine("Ignoring Node {0}", node.id);
                    continue;
                }

                Console.WriteLine(node.boundary);

                if (node.boundary.Contains(x, y))
                {
                    nodes.Add(node);
                }

            }

            return nodes;
        }

        public List<Node> GetNodesOverlappingBoundary(Rectangle boundary, bool ignoreGroupNodes = false)
        {
            List<Node> nodesOverlapping = new List<Node>();
            foreach (var node in this.Nodes)
            {
                if (ignoreGroupNodes && node is GroupNode)
                {
                    continue;
                }

                if (node.Overlaps(boundary))
                {
                    nodesOverlapping.Add(node);
                }

            }

            return nodesOverlapping;
        }

        public List<Node> GetNodesContainedByBoundary(Rectangle boundary, bool ignoreGroupNodes = false)
        {
            List<Node> nodesContained = new List<Node>();
            foreach (var node in this.Nodes)
            {
                if (ignoreGroupNodes && node is GroupNode)
                {
                    continue;
                }

                if (boundary.Contains(node.boundary))
                {
                    nodesContained.Add(node);
                }

            }

            return nodesContained;
        }

        public List<Node> GetNodesContainingBoundary(Rectangle boundary, bool ignoreGroupNodes = false)
        {
            List<Node> nodesContaining = new List<Node>();
            foreach (var node in this.Nodes)
            {
                if (ignoreGroupNodes && node is GroupNode)
                {
                    continue;
                }

                if (node.boundary.Contains(boundary))
                {
                    nodesContaining.Add(node);
                }

            }

            return nodesContaining;
        }

        #endregion

        #region JSON Fields
        [JsonInclude]
        [JsonPropertyName("nodes")]
        public List<Node> NodesByZIndex
        {
            get
            {
                foreach (var node in this.Nodes.OrderByDescending<Node, int>(node => node.z).ToList())
                {
                    Console.WriteLine("{0} -> {1}", node.id, node.z);
                }
                return this.Nodes.OrderByDescending<Node, int>(node => node.z).ToList();
            }
        }

        [JsonInclude]
        [JsonPropertyName("edges")]
        public List<Edge> Edges;

        #endregion

        #region Instance Properties

        [JsonIgnore]
        public Dictionary<string, Node> nodeLookup = new Dictionary<string, Node>();

        [JsonIgnore]
        public Dictionary<string, Edge> edgeLookup = new Dictionary<string, Edge>();

        [JsonIgnore]
        public List<Node> Nodes;

        [JsonIgnore]
        public Rectangle boundary = new Rectangle();

        #endregion

        #region Static Fields

        public static int Margin = 10;

        #endregion
    }

}