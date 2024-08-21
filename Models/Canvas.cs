using System.Drawing;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JSONCanvasDotNet.Models
{

    public class Canvas
    {
        public Canvas(List<Node>? existingNodes = null, List<Edge>? existingEdges = null)
        {
            this.Nodes = existingNodes ?? new List<Node>();

            foreach (var node in this.Nodes)
            {
                if (this.nodeLookup.ContainsKey(node.id))
                {
                    throw new ArgumentException(string.Format("Two nodes have the same ID! Node IDs must be unique. {0}", node.id));
                }

                this.nodeLookup[node.id] = node;
                node.Canvas = this;
            }

            this.Edges = existingEdges ?? new List<Edge>();

            foreach (var edge in Edges)
            {
                if (!this.nodeLookup.ContainsKey(edge.fromNodeId))
                {
                    throw new ArgumentException(string.Format("Edge begins from non-existent Node {0}", edge.fromNode));
                }

                if (!this.nodeLookup.ContainsKey(edge.toNodeId))
                {
                    throw new ArgumentException(string.Format("Edge goes to non-existent Node {0}", edge.toNode));
                }

                if (this.edgeLookup.ContainsKey(edge.id))
                {
                    throw new ArgumentException(string.Format("Two edges have the same ID! Edge IDs must be unique. {0}", edge.id));
                }

                this.edgeLookup[edge.id] = edge;
            }

        }

        public void AddNode(Node newNode)
        {
            if (this.nodeLookup.ContainsKey(newNode.id))
            {
                throw new ArgumentException(string.Format("Node already exists with new Node's ID! Node IDs must be unique. {0}", newNode.id));
            }

            this.nodeLookup[newNode.id] = newNode;

            this.Nodes.Add(newNode);
            newNode.Canvas = this;
        }

        public void AddNodes(List<Node> newNodes)
        {
            foreach (var node in newNodes)
            {
                if (this.nodeLookup.ContainsKey(node.id))
                {
                    throw new ArgumentException(string.Format("Two nodes have the same ID! Node IDs must be unique. {0}", node.id));
                }

                this.nodeLookup[node.id] = node;
                node.Canvas = this;
            }

            this.Nodes.AddRange(newNodes);
        }

        public void AddEdge(Edge newEdge)
        {
            if (this.edgeLookup.ContainsKey(newEdge.id))
            {
                throw new ArgumentException(string.Format("Two edges have the same ID! Edge IDs must be unique. {0}", newEdge.id));
            }

            if (!nodeLookup.TryGetValue(newEdge.fromNodeId, out newEdge.fromNode))
            {
                throw new ArgumentException(string.Format("Edge begins from non-existent Node {0}", newEdge.fromNode));
            }

            if (!nodeLookup.TryGetValue(newEdge.toNodeId, out newEdge.toNode))
            {
                throw new ArgumentException(string.Format("Edge goes to non-existent Node {0}", newEdge.toNode));
            }

            this.edgeLookup[newEdge.id] = newEdge;

            this.Edges.Add(newEdge);

            newEdge.fromNode.Edges.Add(newEdge);
            newEdge.toNode.Edges.Add(newEdge);
        }

        public void AddEdges(List<Edge> newEdges)
        {
            foreach (var edge in newEdges)
            {
                if (edgeLookup.ContainsKey(edge.id))
                {
                    throw new ArgumentException(string.Format("Two edges have the same ID! Edge IDs must be unique. {0}", edge.id));
                }

                if (!nodeLookup.TryGetValue(edge.fromNodeId, out edge.fromNode))
                {
                    throw new ArgumentException(string.Format("Edge begins from non-existent Node {0}", edge.fromNode));
                }

                if (!nodeLookup.TryGetValue(edge.toNodeId, out edge.toNode))
                {
                    throw new ArgumentException(string.Format("Edge goes to non-existent Node {0}", edge.toNode));
                }

                this.edgeLookup[edge.id] = edge;

                edge.fromNode.Edges.Add(edge);
                edge.toNode.Edges.Add(edge);
            }

            this.Edges.AddRange(newEdges);
        }

        public Node? GetNodeAtCoordinates(int x, int y, bool ignoreGroupNodes = false)
        {
            foreach (var node in this.Nodes)
            {
                if (ignoreGroupNodes && node is GroupNode)
                {
                    continue;
                }

                if (node.boundingBox.Contains(x, y))
                {
                    return node;
                }

            }

            return null;
        }

        public List<Node> NodesOverlappingBoundary(Rectangle proposedBoundingBox, bool ignoreGroupNodes = false)
        {
            List<Node> nodesOverlapping = new List<Node>(); 
            foreach (var node in this.Nodes)
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

        private Dictionary<string, Node> nodeLookup = new Dictionary<string, Node>();
        private Dictionary<string, Edge> edgeLookup = new Dictionary<string, Edge>();

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("nodes")]
        public List<Node> Nodes;

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("edges")]
        public List<Edge> Edges;
    }

}