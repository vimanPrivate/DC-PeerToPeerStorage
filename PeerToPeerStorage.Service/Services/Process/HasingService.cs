using PeerToPeerStorage.Common.Enums;
using PeerToPeerStorage.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeerToPeerStorage.Service.Services.Process
{
    public class HasingService
    {
        private int NodeId;
        private List<Node> ConnectedNodes;
        private List<ValueTable> _valueTable;
        private NodeRoleEnum NodeRole;
        private int noOfReceiverNodes;

        public HasingService(int nodeId, List<Node> connectedNodes, List<ValueTable> valueTable, NodeRoleEnum role)
        {
            this.NodeId = nodeId;
            this.ConnectedNodes = connectedNodes;
            this._valueTable = valueTable;
            this.NodeRole = role;
            this.noOfReceiverNodes = 5;
        }

        public void StoreTextValuesInReceiver(string sentence)
        {
            string firstTenCharacters = sentence.Substring(0, Math.Min(sentence.Length, 10));
            int originalNode = GetOriginalNodeId(firstTenCharacters);
            var storingNodesArray = GetPossibleNodeReceiverNodeList(originalNode);

            foreach (var node in ConnectedNodes)
            {
                if (node.NodeId == storingNodesArray[0] || node.NodeId == storingNodesArray[1])
                {
                    if (!node._valueTable.Any(row => row.Id == firstTenCharacters))
                    {
                        node._valueTable.Add(new ValueTable()
                        {
                            Id = firstTenCharacters,
                            Value = sentence
                        });
                    }
                }

                foreach (var innderNode in node.ConnectedNodes)
                {
                    if (innderNode.NodeId == storingNodesArray[0] || innderNode.NodeId == storingNodesArray[1])
                    {
                        if (!innderNode._valueTable.Any(row => row.Id == firstTenCharacters))
                        {
                            innderNode._valueTable.Add(new ValueTable()
                            {
                                Id = firstTenCharacters,
                                Value = sentence
                            });
                        }
                    }
                }
            }
        }

        private int GetOriginalNodeId(string firstTenCharacters)
        {
            int originalNode = 0;
            int totalAsciiValue = 0;

            foreach (char c in firstTenCharacters)
            {
                totalAsciiValue += Convert.ToInt32(c);
            }

            //int noOfReceiverNodes = this.NodeRole == NodeRoleEnum.Receiver ? 1 : 0;
            //noOfReceiverNodes = this.ConnectedNodes.Count(node => node.NodeRole == NodeRoleEnum.Receiver);

            originalNode = totalAsciiValue % noOfReceiverNodes;

            return originalNode;
        }

        private int[] GetPossibleNodeReceiverNodeList(int originalNodeId)
        {
            var receiverNodeList = new int[2];

            var availableReceiverNodeList = ConnectedNodes
                                            .Where(node => node.NodeRole == NodeRoleEnum.Receiver)
                                            .Select(x => x.NodeId).ToList();

            if (this.NodeRole == NodeRoleEnum.Receiver)
                availableReceiverNodeList.Add(this.NodeId);

            availableReceiverNodeList = availableReceiverNodeList.OrderBy(id => id).ToList();

            int firstNode;
            int backupNode;

            if (availableReceiverNodeList.Contains(originalNodeId))
            {
                firstNode = originalNodeId;

                if (availableReceiverNodeList.Any(id => id > firstNode))
                    backupNode = availableReceiverNodeList.Where(id => id > firstNode).OrderBy(id => id).FirstOrDefault();
                else
                    backupNode = availableReceiverNodeList[0];
            }
            else
            {
                if (availableReceiverNodeList.Any(id => id > originalNodeId))
                    firstNode = availableReceiverNodeList.Where(id => id > originalNodeId).OrderBy(id => id).FirstOrDefault();
                else
                    firstNode = availableReceiverNodeList[0];

                if (availableReceiverNodeList.Any(id => id > firstNode))
                    backupNode = availableReceiverNodeList.Where(id => id > firstNode).OrderBy(id => id).FirstOrDefault();
                else
                    backupNode = availableReceiverNodeList[0];
            }

            receiverNodeList[0] = firstNode;
            receiverNodeList[1] = backupNode;

            return receiverNodeList;
        }

        public string GetStoredTextValue(string firstTenCharacters)
        {
            string textValue = "";
            int originalNodeId = GetOriginalNodeId(firstTenCharacters);
            var storingNodesArray = GetPossibleNodeReceiverNodeList(originalNodeId);

            foreach (var node in ConnectedNodes)
            {
                if (node.NodeId == storingNodesArray[0] || node.NodeId == storingNodesArray[1])
                {
                    if (node._valueTable.Any(tbl => tbl.Id == firstTenCharacters))
                        textValue = node._valueTable
                                            .Where(tbl => tbl.Id == firstTenCharacters)
                                            .Select(row => row.Value)
                                            .FirstOrDefault();

                    return textValue;
                }

                foreach (var innerNode in ConnectedNodes)
                {
                    if (innerNode.NodeId == storingNodesArray[0] || innerNode.NodeId == storingNodesArray[1])
                    {
                        if (innerNode._valueTable.Any(tbl => tbl.Id == firstTenCharacters))
                            textValue = innerNode._valueTable
                                                    .Where(tbl => tbl.Id == firstTenCharacters)
                                                    .Select(row => row.Value)
                                                    .FirstOrDefault();

                        return textValue;
                    }
                }
            }

            return textValue;
        }
    }
}
