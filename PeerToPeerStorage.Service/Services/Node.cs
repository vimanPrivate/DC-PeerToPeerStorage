using PeerToPeerStorage.Common.Enums;
using PeerToPeerStorage.Common.Models;
using PeerToPeerStorage.Service.Services.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeerToPeerStorage.Service.Services
{
    public class Node
    {
        private List<NodeTable> _nodeTable;
        public List<ValueTable> _valueTable;

        public int NodeId { get; set; }
        public string Name { get; set; }
        public List<Node> ConnectedNodes { get; }
        public NodeRoleEnum NodeRole { get; private set; }
        public bool IsLeader { get; set; }

        LoggingService logs;


        public Node()
        {
            ConnectedNodes = new List<Node>();
            _nodeTable = new List<NodeTable>();
            _valueTable = new List<ValueTable>();
            logs = new LoggingService();
        }

        public Node(int nodeId)
        {
            NodeId = nodeId;
            ConnectedNodes = new List<Node>();
            _nodeTable = new List<NodeTable>();
            _valueTable = new List<ValueTable>();
            logs = new LoggingService();
        }

        public void SetupNodeConnection(Node node)
        {
            logs.InfoLog("SetupNodeConnection()");

            if (ConnectedNodes.Count == 0)
            {
                ConnectedNodes.Add(node);
                node.ConnectedNodes.Add(this);
            }
            else
            {
                foreach (var nodeFromList in ConnectedNodes)
                {
                    nodeFromList.ConnectedNodes.Add(node);
                    node.ConnectedNodes.Add(nodeFromList);
                }

                ConnectedNodes.Add(node);
                node.ConnectedNodes.Add(this);
            }
        }

        public void DisconnectNode(int nodeId = -99)
        {
            logs.InfoLog("DisconnectNode");

            if (nodeId == -99)
            {
                Console.WriteLine("\n\t Please enter node Id which you want to remove : \n\t Or Press -99 to go back");
                int readVal = int.Parse(Console.ReadLine());

                if (readVal == -99)
                    return;

                nodeId = readVal;
            }

            if (nodeId == NodeId)
            {
                int nextNodeId = nodeId;
                Node replacingNode = null;

                bool isNextNodeAvailable = false;

                while (!isNextNodeAvailable)
                {
                    nextNodeId++;

                    if (ConnectedNodes.Any(node => node.NodeId == nextNodeId))
                    {
                        replacingNode = ConnectedNodes.Where(node => node.NodeId == nextNodeId).FirstOrDefault();
                        isNextNodeAvailable = true;
                    }
                }

                foreach (var node in ConnectedNodes)
                {
                    var innerNodeList = node.ConnectedNodes;

                    if (innerNodeList.Any(e => e.NodeId == nodeId))
                    {
                        var innerRemovebleNode = innerNodeList.Where(i => i.NodeId == nodeId).First();
                        innerNodeList.Remove(innerRemovebleNode);
                    }
                }

                ConnectedNodes.Remove(replacingNode);

                if (_nodeTable.Count > 0)
                    _nodeTable.Clear();

                if (_valueTable.Count > 0)
                    _valueTable.Clear();

                this.NodeId = replacingNode.NodeId;
                this.Name = replacingNode.Name;
                this.NodeRole = replacingNode.NodeRole;
                this.IsLeader = replacingNode.IsLeader;
                this._nodeTable = replacingNode._nodeTable;
                this._valueTable = replacingNode._valueTable;
            }

            if (ConnectedNodes.Any(node => node.NodeId == nodeId))
            {
                var removebleNode = ConnectedNodes.Where(node => node.NodeId == nodeId).First();

                ConnectedNodes.Remove(removebleNode);
                removebleNode.ConnectedNodes.Remove(this);

                foreach (var node in ConnectedNodes)
                {
                    var innerNodeList = node.ConnectedNodes;

                    if (innerNodeList.Any(e => e.NodeId == nodeId))
                    {
                        var innerRemovebleNode = innerNodeList.Where(i => i.NodeId == nodeId).First();
                        innerNodeList.Remove(innerRemovebleNode);
                    }
                }
            }
            else
            {
                Console.WriteLine($"{nodeId} is not connected to {nodeId}");
            }

            Console.WriteLine("\n\t Node '" + nodeId + "' Removed from System");
        }

        public void ElectLeader()
        {
            logs.InfoLog("ElectLeader()");

            Console.WriteLine("Electing a leader .... ");
            int leaderNodeId = ConnectedNodes.Select(node => node.NodeId).Max();

            //broadcasting the leader
            if (leaderNodeId == this.NodeId)
                this.IsLeader = true;

            foreach (var node in ConnectedNodes)
            {
                if (node.NodeId == leaderNodeId)
                    node.IsLeader = true;                    

                foreach (var innerNode in node.ConnectedNodes)
                {
                    if (innerNode.NodeId == leaderNodeId)
                        innerNode.IsLeader = true;
                        
                }
            }

            if(this.IsLeader)
                Console.WriteLine("Node Id '" + this.NodeId + "' elected as the leader !!!!!");
            else
            {
                int nodeId = ConnectedNodes.Where(node => node.IsLeader == true).Select(x => x.NodeId).FirstOrDefault();
                Console.WriteLine("Node Id '" + nodeId + "' elected as the leader !!!!!");
            }
        }

        public void AssigningRoles()
        {
            logs.InfoLog("AssigningRoles()");

            if (IsLeader)
            {
                if (NodeId % 2 == 0)
                    NodeRole = NodeRoleEnum.Receiver;
                else
                    NodeRole = NodeRoleEnum.Hasher;

                foreach (var node in ConnectedNodes)
                {
                    if (node.NodeId % 2 == 0)
                        node.NodeRole = NodeRoleEnum.Receiver;
                    else
                        node.NodeRole = NodeRoleEnum.Hasher;

                    foreach (var innerNode in node.ConnectedNodes)
                    {
                        if (innerNode.NodeId % 2 == 0)
                            innerNode.NodeRole = NodeRoleEnum.Receiver;
                        else
                            innerNode.NodeRole = NodeRoleEnum.Hasher;
                    }
                }
            }
            else
            {
                foreach (var node in ConnectedNodes)
                {
                    if (node.IsLeader)
                    {
                        node.AssigningRoles();
                    }
                }
            }          
        }

        public void DisplayNodeInfo()
        {
            Console.WriteLine("\n## Node information as follows \n");

            Console.WriteLine("\n-----------------------------------------------------------");
            Console.WriteLine("NodeID\t|\tRole\t\t|\tIsLeader");
            Console.WriteLine("-----------------------------------------------------------");

            if (this.NodeRole == NodeRoleEnum.Receiver)
                Console.WriteLine(this.NodeId + "\t|\t" + this.NodeRole + "\t|\t" + this.IsLeader.ToString());
            else
                Console.WriteLine(this.NodeId + "\t|\t" + this.NodeRole + "\t\t|\t" + this.IsLeader.ToString());

            foreach (var node in ConnectedNodes)
            {
                if(node.NodeRole == NodeRoleEnum.Receiver)
                    Console.WriteLine(node.NodeId + "\t|\t" + node.NodeRole.ToString() + "\t|\t" + node.IsLeader.ToString());
                else
                    Console.WriteLine(node.NodeId + "\t|\t" + node.NodeRole.ToString() + "\t\t|\t" + node.IsLeader.ToString());
            }
            Console.WriteLine("-----------------------------------------------------------");
        }

        public void AddNewNode(Node node)
        {
            node.NodeId = AssigningId();
            node.NodeRole = AssigningRole(node.NodeId);
            SetupNodeConnection(node);
            Console.WriteLine("\n\tNode added succesfully. Node Id is '"+node.NodeId+"' and role is '"+node.NodeRole+"'");
        }

        public NodeRoleEnum AssigningRole(int nodeId)
        {
            var leaderNode = NavigateToLeaderNode();
            return leaderNode.SetNewRole(nodeId);
        }

        private NodeRoleEnum SetNewRole(int nodeId)
        {
            if (nodeId % 2 == 0)
                return NodeRoleEnum.Receiver;
            else
                return NodeRoleEnum.Hasher;
        }

        public int AssigningId()
        {
            var leaderNode = NavigateToLeaderNode();
            return leaderNode.SetNewId();
        }

        private int SetNewId()
        {
            int maxNodeId = ConnectedNodes.Max(x => x.NodeId);
            maxNodeId = this.NodeId > maxNodeId ? this.NodeId : maxNodeId;

            return maxNodeId + 1;
        }

        private bool IsLeaderExists()
        {
            if (this.IsLeader)
                return true;
            else if (ConnectedNodes.Any(x => x.IsLeader == true))
                return true;

            return false;
        }

        private Node NavigateToLeaderNode()
        {
            logs.InfoLog("NavigateToLeaderNode()");
            if (IsLeaderExists())
            {
                if (this.IsLeader)
                    return this;
                else
                    return ConnectedNodes.Where(x => x.IsLeader).FirstOrDefault();
            }
            else
            {
                ElectLeader();
                return NavigateToLeaderNode();
            }
        }

        public void StoreTextValuesRequest(string sentence)
        {
            if (this.NodeRole == NodeRoleEnum.Receiver)
            {
                foreach (var node in ConnectedNodes)
                {
                    if (node.NodeRole == NodeRoleEnum.Hasher)
                    {
                        node.StoreTextValuesInReceiver(sentence);
                        break;
                    }
                }
            }
            else
            {
                StoreTextValuesInReceiver(sentence);
            }
        }

        public void StoreTextValuesInReceiver(string sentence)
        {
            logs.InfoLog("StoreTextValuesInReceiver()");

            var hasherService = new HasingService(NodeId, ConnectedNodes, _valueTable, NodeRole);
            hasherService.StoreTextValuesInReceiver(sentence);
        }

        public string GetStoredTextValueRequest(string firstTenCharacters = null)
        {
            if(firstTenCharacters == null)
            {
                Console.WriteLine("\n\t Please enter first Ten characters : \n\t Or Press -99 to go back");
                string input = Console.ReadLine();

                if (input == "-99")
                    return "Note :- User exit from the Process!!!!";

                if (input.Length != 10)
                    return "Note :- Please enter exacly 10 characters.";

                firstTenCharacters = input;            
            }

            if (this.NodeRole == NodeRoleEnum.Receiver)
            {
                foreach (var node in ConnectedNodes)
                {
                    if (node.NodeRole == NodeRoleEnum.Hasher)
                    {
                        return node.GetStoredTextValue(firstTenCharacters);
                    }
                }
            }
            else
            {
                return GetStoredTextValue(firstTenCharacters);
            }

            return null;
        }

        public string GetStoredTextValue(string firstTenCharacters)
        {
            var hasherService = new HasingService(NodeId, ConnectedNodes, _valueTable, NodeRole);
            string textValue = hasherService.GetStoredTextValue(firstTenCharacters);

            return textValue;
        }
    }
}
