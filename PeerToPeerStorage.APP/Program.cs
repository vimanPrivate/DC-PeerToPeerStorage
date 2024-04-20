using Grpc.Net.Client;
using GRPC_Server;
using PeerToPeerStorage.Service.Services;
using PeerToPeerStorage.Service.Services.Process;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeerToPeerStorage.APP
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //LoggingService req = new LoggingService();
            //req.InfoLog("1");
            //req.InfoLog("2");


            /*
             * Initial Process goes here
             * Letting user select how many nodes does user wants ( for Demo only )
             * Seperating entire paragraph by '.'
             * Generating and linking nodes each other according to the user entered node count
             * Selecting leader
             * Assigning roles
             * storing values to the nodes
             */


            Console.Write("Enter number of Receiver nodes : ");
            int noOfNodes = int.Parse(Console.ReadLine());

            string text = "The Mapogo Lions, also known as the Mapogo Coalition or the Mapogo pride, were a legendary coalition of" +
                            "male lions in the Sabi Sand Game Reserve in South Africa. The coalition gained fame for their dominance and" +
                            "ruthless behavior, particularly in their efforts to assert control over territory and prides." +
                            "The coalition consisted of six to eight male lions at its peak, and they were known for their exceptional " +
                            "hunting skills and aggressive tactics. They would often target rival male lions, as well as lionesses and" +
                            "cubs from other prides, in order to expand their territory and ensure their dominance." +
                            "The Mapogo Lions became the subject of numerous documentaries and wildlife films due to" +
                            "their remarkable story and behavior. However, their reign eventually came to an end as they " +
                            "grew older and weaker, and new coalitions of younger lions challenged their dominance. Today, they are " +
                            "remembered as one of the most formidable lion coalitions in African wildlife history";

            var textList = text.Split('.').ToList();

            /*
             * Initiating Nodes 
            */

            int i = 0;
            Node initNode = null;
            do
            {
                if (i == 0)
                    initNode = new Node(i);
                else
                    initNode.SetupNodeConnection(new Node(i));

                i++;
            }
            while (i < (noOfNodes * 2));

            Console.WriteLine("Total " + (initNode.ConnectedNodes.Count + 1) + " are created!");

            initNode.ElectLeader();

            Console.WriteLine("Assigning Roles to the Nodes....");
            initNode.AssigningRoles();
            //initNode.DisplayNodeInfo();
            //initNode.DisconnectNode(1);
            //initNode.AddNewNode(new Node());
            //initNode.AddNewNode(new Node());
            Console.WriteLine("Adding values to nodes ........");

            Console.WriteLine("\n\n\n*************************************************************************");
            Console.WriteLine("\nInitial Stage complete. \nPlease follow below instruction to see the DEMO \n");
            Console.WriteLine("*************************************************************************");

            foreach (string sentence in textList)
            {
                initNode.StoreTextValuesRequest(sentence);
            }

            bool isContinue = false;
            do
            {
                int command = DisplayMenu();
                StartConsoleProcess(command, textList,initNode);

                Console.WriteLine("\n\t>> To Continue Press 1.\n\t>> Exit press -99");
                int continueCommand = int.Parse(Console.ReadLine());
                isContinue = continueCommand == 1 ? true : false;
            }
            while (isContinue);

            //initNode.DisconnectNode(2);
            //initNode.DisconnectNode(4);
            //initNode.DisconnectNode(6);

            //var val1 = initNode.GetStoredTextValueRequest("The Mapogo");
            //var val2 = initNode.GetStoredTextValueRequest(" The coali");
        }

        public static int DisplayMenu()
        {
            Console.WriteLine("\n--------------------- MENU --------------------\n");
            Console.WriteLine("\tPress 1\t\t: View first 10 digits of paragraph");
            Console.WriteLine("\tPress 2\t\t: Display nodes info");
            Console.WriteLine("\tPress 3\t\t: Remove a node");
            Console.WriteLine("\tPress 4\t\t: Add new node");
            Console.WriteLine("\tPress 5\t\t: Read value from distributed Nodes");
            Console.WriteLine("\tPress -99\t: Exit");

            Console.Write("\n\tEnter .. : ");
            int command = int.Parse(Console.ReadLine());
            Console.WriteLine("-----------------------------------------------\n");
            return command;
        }

        public static void StartConsoleProcess(int commandNum, List<string> paraList, Node initNode)
        {
            switch (commandNum)
            {
                case 1:
                    DisplayFistTenCharacters(paraList);
                    break;
                case 2:
                    initNode.DisplayNodeInfo();
                    break;
                case 3:
                    initNode.DisconnectNode();
                    break;
                case 4:
                    initNode.AddNewNode(new Node());
                    break;
                case 5:
                    Console.WriteLine("\n### Text is : \n '"+ (String.IsNullOrEmpty(initNode.GetStoredTextValueRequest())? "No text OR text cannot be found": initNode.GetStoredTextValueRequest()) + "'");
                    break;
                case -99:
                    Environment.Exit(0);
                    break;
            }
        }

        public static void DisplayFistTenCharacters(List<string> list)
        {
            Console.WriteLine("\n## First 10 characters of the sentences as follows\n");
            foreach (var paragraph in list)
            {
                Console.WriteLine("\t'" + paragraph.Substring(0, Math.Min(paragraph.Length, 10))+"'");
            }
        }
    }
}
