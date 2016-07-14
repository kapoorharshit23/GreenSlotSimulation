using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSlot
{
    class Program
    {
        static void Main(string[] args)
        {
            Program prog = new Program();

            Console.WriteLine("GREEN SLOT Algorithm Simulation..");
            Console.WriteLine("Window: 1 day");
            Console.WriteLine("Slot Duration: 15 minutes each");
            Console.WriteLine("Number of Slots: " + (4 * 24));

            /*
             *Picking up Task information:
             * Task Name, DeadLine, Estimated Runtime, Number of nodes required 
             */
            string[] lines = System.IO.File.ReadAllLines(@"C:\Users\Th3 Dark0\Documents\Visual Studio 2015\Projects\GreenSlot\UserInput.txt");
            int taskCount = lines.Length;

            string[] taskNamesArray = new string[taskCount];
            string[] deadLineArray = new string[taskCount];
            int[] runTimeArray = new int[taskCount];
            int[] reqArray = new int[taskCount];
            DateTime[] dateTimeTaskArray = new DateTime[taskCount];

            for(int i=0;i<taskCount;i++)
            {
                string[] tempArray = lines[i].Split(' ');
                taskNamesArray[i] = tempArray[0];
                deadLineArray[i] = tempArray[1]+" "+tempArray[2];
                runTimeArray[i] = int.Parse(tempArray[3]);
                reqArray[i] = int.Parse(tempArray[4]);
                dateTimeTaskArray[i] = Convert.ToDateTime(deadLineArray[i]);
            }
            Console.WriteLine("_________________________________________________________");
            Console.WriteLine("Task Information:");
            for(int i=0;i<taskCount;i++)
            {
                Console.WriteLine("Task Name: "+taskNamesArray[i]+" DeadLine: "+deadLineArray[i]+" Estimated Runtime: "+runTimeArray[i]+" Nodes Required: "+reqArray[i]);
            }

            //Calling function to schedule the jobs
            prog.scheduleJob(taskNamesArray, deadLineArray, runTimeArray, reqArray, dateTimeTaskArray,taskCount);
            
            Console.ReadKey();
        }
        public void scheduleJob(string[] tNamesArray, string[] taskDeadlineArray,int[] taskRunTimeArray,int[] taskReqArray,DateTime[] taskDateTimeArray,int tCount)
        {
            Console.WriteLine("Funct1");

            string[] jobQueue = new string[tCount];      //Actual sorted job queue
            TimeSpan[] jobSlack = new TimeSpan[tCount];  //sorted job slack time

            int totalBrownEnergy = 1000;
            int dayBrownEnergyPrice = 100;
            int nightBrownEnergyPrice = 70;

            /*
             *Calculating Energy requirement of a job 
             */
            int jobRunningTime = 105;
            int jobInitTime = 140;
            int jobSplitTime = 90;
            int jobGatherTime = 102;
            int switchEnergy = 55;
            int serverConsumption = 30;
            double JOBENERGY = jobGatherTime + jobInitTime + jobSplitTime + switchEnergy + serverConsumption;

            double[] taskEnergyRequirement = new double[tCount];    //Array with each task's energy requirement

            for (int i = 0; i < tCount; i++)
            {
                taskEnergyRequirement[i] = JOBENERGY + (jobRunningTime * taskRunTimeArray[i]);
            }
            Console.WriteLine("Energy Consumption of each task:");
            for (int i = 0; i < tCount; i++)
            {
                Console.WriteLine(taskRunTimeArray[i] + " : " + taskEnergyRequirement[i]);
            }

            /*
            *Green energy predictor
            *& Brown energy prices. 
            * 
            *Generates random energy values for 96 slots of a window
            */
            double[] gEnergyarray=calculateGreenEnery();        //Array with Green energy deposits for each slot
            double totalGreenEnergy = 0;
            for (int i = 0; i < 96; i++)
            {
                totalGreenEnergy = totalGreenEnergy + gEnergyarray[i];
            }

            Console.WriteLine("Total Green Energy:" + totalGreenEnergy);

            /*
             *Number of nodes per slot generator 
             * 
             */
            List<int> nodeArray= nodeGenerator();
            int totalNodes = 0;
            for (int i = 0; i < 96; i++)
            {
                totalNodes = totalNodes + nodeArray[i];
            }

            Console.WriteLine("Total Nodes Available:" + totalNodes);

            /*
             * Calculating slack time for each task, corresponding to current time
             * */
            TimeSpan[] slackTimeSpanArray = new TimeSpan[tCount];
            DateTime localDate = DateTime.Now;

            for (int i = 0; i < tCount; i++)
            {
                slackTimeSpanArray[i] = taskDateTimeArray[i].Subtract(localDate);
            }

            double[] taskSlackArray=calculateSlackTime(tCount, taskDateTimeArray, taskRunTimeArray,slackTimeSpanArray);
            
            for (int i=0;i<tCount;i++)
            {
                Console.WriteLine(tNamesArray[i] + " : " + taskSlackArray[i]);
            }

            /*
             * Sorting tasks as per slack time and storing them in
             * jobQueue and jobSlack arrays.
             * */
            TimeSpan smallestSlackTime;
            int sSlackTimeLoc;
            for (int j = 0; j < tCount; j++)
            {
                smallestSlackTime = slackTimeSpanArray[0];
                sSlackTimeLoc = 0;
                for (int i = 1; i < tCount; i++)
                {
                    if (slackTimeSpanArray[i] < smallestSlackTime)
                    {
                        sSlackTimeLoc = i;
                        smallestSlackTime = slackTimeSpanArray[i];
                    }
                }
                jobSlack[j] = smallestSlackTime;
                jobQueue[j] = tNamesArray[sSlackTimeLoc];
                /*
                 *We wont accept jobs which end beyond a certain window 
                 */
                slackTimeSpanArray[sSlackTimeLoc] = TimeSpan.Parse("86400");        //Since our window is one day long
            }
            for (int i = 0; i < tCount; i++)
            {
                for (int j = 0; j < tCount; j++)
                {
                    if (jobQueue[i].Equals(tNamesArray[j]))
                    {
                        Console.WriteLine(tNamesArray[j] + " " + taskDeadlineArray[j]);
                    }
                }
            }

            
            //Genereating a single workflow for all the jobs based on energy requirement
            Console.WriteLine("Task's workflow:");
            //jobsWorkflow(jobQueue,tNamesArray, taskDeadlineArray, taskEnergyRequirement, taskRunTimeArray, gEnergyarray, tCount);

            //Slots requirement calculation for each job
            int[] slotsRequired = slotCalculation(jobQueue, tNamesArray, taskRunTimeArray, tCount);
            for (int i = 0; i < tCount; i++)
            {
                Console.WriteLine(jobQueue[i] + " - " + slotsRequired[i]);
            }

            //Making cost array of a job
            Console.WriteLine("Task's workflow based on cost array:");
            jobCostArray(slotsRequired, nodeArray, jobQueue, tNamesArray, taskDeadlineArray, taskEnergyRequirement, taskRunTimeArray, gEnergyarray, taskReqArray, tCount);
        }
        /*
         * Generating cost array of a job to 
         * find the best possible slots for its
         * execution on cloud infrastructure.
         * 
         */
        public void jobCostArray(int[] slotsPerTask, List<int> nodeSlotArray, string[] sortedJobQueue, string[] tNames, string[] tDeadLines, double[] tEnergyReqs, int[] tRunTimes, double[] gEnergy,int[] tReq, int totalTasks)
        {
            List<Tuple<string, List<int>>> slotListPerTask = new List<Tuple<string, List<int>>>();
            List<int> availSlots = new List<int>();
            List<int> temp = new List<int>();           //Temporary array for making lists of slots for each task

            int numOfSlots = 96;
            int slotCounter = 1;
            

            for (int i = 1; i <= numOfSlots; i++) 
            {
                availSlots.Add(i);
            }

            //Generating lists where each task can be executed
            for (int m = 0; m < totalTasks; m++)
            {
                Console.WriteLine(sortedJobQueue[m] + " : ");
                for (int n = 0; n < totalTasks; n++)
                {
                    if(sortedJobQueue[m].Equals(tNames[n]))
                    {
                        //int count = slotsPerTask[m];      //while loop increment counter
                        int sum = 0;        //represents sum of nodes
                        int lastSumVal = 0;     //last iteration value of sum
                        int NumberOfNodesReq = tReq[n];     //number of the required nodes of the job

                        for (int i = 0; i < nodeSlotArray.Count; i++)
                        {
                            temp.RemoveRange(0, temp.Count);
                            sum = nodeSlotArray[i];
                            //Console.WriteLine("s=" + sum);
                            if (sum == NumberOfNodesReq)
                            {
                                Console.Write(nodeSlotArray[i]);
                                Console.WriteLine();
                            }
                            else
                            {
                                temp.Add(nodeSlotArray[i]);
                                for (int j = i + 1; j < nodeSlotArray.Count; j++)
                                {
                                    lastSumVal = sum;
                                    sum = temp.Sum() + nodeSlotArray[j];

                                    if (sum > NumberOfNodesReq)
                                    {
                                        sum = lastSumVal;

                                    }
                                    else
                                    {
                                        temp.Add(nodeSlotArray[j]);
                                    }

                                    if (sum == NumberOfNodesReq)
                                    {
                                        //Tuple<string,List<int>> tempTuple=(sortedJobQueue[m],);
                                        foreach (int x in temp)
                                        {
                                            Console.Write(x + " ");
                                        }
                                        //slotListPerTask.Add(Tuple.Create<string, List<int>>(sortedJobQueue[m], temp));

                                        Console.WriteLine();
                                        sum = nodeSlotArray[i];
                                        temp.RemoveRange(0, temp.Count);
                                        temp.Add(nodeSlotArray[i]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Printing the slot lists as per number of slots condition boundary

            //for (int i = 0; i < totalTasks; i++)
            //{
            //    Console.WriteLine(sortedJobQueue[i] + " list:");
            //    for (int j = 0; j < totalTasks; j++)
            //    {
            //        if (sortedJobQueue[i].Equals(tNames[j]))
            //        {
            //            foreach (Tuple<string, List<int>> x in slotListPerTask)
            //            {
            //                //Console.WriteLine(x);
            //                if (x.Item1.Equals(sortedJobQueue[i]))
            //                {
            //                    for (int k = 0; k < x.Item2.Count; k++)
            //                    {
            //                        Console.Write(x.Item2[k]+" ");
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    Console.WriteLine();
            //}

        }
        /*
         * Creating a slot requirement array for each job
         * */
         public int[] slotCalculation(string[] sortedJobQueue, string[] tNames, int[] tRunTimes, int taskCount)
        {
            double temp=0.00;
            double[] jobSlotReq = new double[taskCount];
            int[] jobSlots = new int[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                for (int j = 0; j < taskCount; j++)
                {
                    if(sortedJobQueue[i].Equals(tNames[j]))
                    {
                        jobSlotReq[i] = (((double)tRunTimes[j] / 60)/15);
                    }            
                }
            }
            for (int i = 0; i < taskCount; i++)
            {
                //Console.WriteLine(jobSlotReq[i]);
                if (jobSlotReq[i] % 1 != 0)
                {
                    temp = (1 - (jobSlotReq[i] % 1));
                    jobSlots[i] = (int)(jobSlotReq[i] + temp);
                }
                else
                {
                    jobSlots[i] = (int)(jobSlotReq[i]);
                }
            }
            return jobSlots;
        }

        /*
         * Generating a single workflow for all the jobs 
         * based on LSTF ordering on the basis of energy requirement of each job
         * and green energy present.
         * Not considering time or brown energy prices.
         */
        public void jobsWorkflow(string[] sortedJobQueue, string[] tNames, string[] tDeadLines, double[] tEnergyReqs, int[] tRunTimes, double[] gEnergy, int totalTasks)
        {
            //int consumedNodes = 0;
            int slotCounter = 0;
            double totalGEnergy = 0;        //Total green energy
            int totalSlots = 96;

            List<int> occupiedSlots = new List<int>();
            List<Tuple<string, int>> taskWorkflow = new List<Tuple<string, int>>();
            

            //these arrays will be as per the sortedJobQueue ordering
            double[] consumedGreenEnergy = new double[totalTasks];
            double[] consumedBrownEnergy = new double[totalTasks];
            double[] consumedEnergy = new double[totalTasks];
            
            //initalizing the arrays
            for (int i = 0; i < totalTasks; i++)
            {
                consumedBrownEnergy[i] = 0;
                consumedEnergy[i] = 0;
                consumedGreenEnergy[i] = 0;
            }
            
            //Calculating available total Green energy
            for (int i = 0; i < 96; i++)
            {
                totalGEnergy = totalGEnergy + gEnergy[i];
            }

            //WorkFlow Creation
            for (int i = 0; i < totalTasks; i++)
            {
                for (int j = 0; j < totalTasks; j++)
                {
                    if(sortedJobQueue[i].Equals(tNames[j]))
                    {
                        while(consumedEnergy[i]!=tEnergyReqs[j])
                        {
                            if(occupiedSlots.Count!=0)
                            {
                                while(occupiedSlots.Contains(slotCounter))
                                {
                                    slotCounter = slotCounter + 1;
                                }
                            }
                            if ((tEnergyReqs[j] - consumedEnergy[i]) > gEnergy[slotCounter])
                            {
                                //This means this slot has been occupied and cannot be used for further allocation
                                consumedGreenEnergy[i] = gEnergy[slotCounter];
                                totalGEnergy = totalGEnergy - gEnergy[slotCounter];
                                occupiedSlots.Add(slotCounter);
                                gEnergy[slotCounter] = 0;
                                //taskSlotOrder.Add(slotCounter,sortedJobQueue[i]);
                                taskWorkflow.Add(Tuple.Create(sortedJobQueue[i],slotCounter));
                                slotCounter = slotCounter + 1;
                            }
                            else
                            {
                                //This slot still has some potential to get jobs
                                //this slot should not be added to the list of occupied slots
                                taskWorkflow.Add(Tuple.Create(sortedJobQueue[i], slotCounter));
                                gEnergy[slotCounter] = gEnergy[slotCounter] - (tEnergyReqs[j]-consumedEnergy[i]);
                                consumedGreenEnergy[i] = (tEnergyReqs[j] - consumedEnergy[i]);
                                totalGEnergy = totalGEnergy - consumedGreenEnergy[i];
                            }
                            consumedEnergy[i] = consumedEnergy[i] + consumedBrownEnergy[i] + consumedGreenEnergy[i];
                                                        
                        }
                    }
                }
            }
            //printing the task order schedule list
            foreach(Tuple<string,int> x in taskWorkflow)
            {
                Console.WriteLine(x.Item1 + " " + x.Item2);
            }

            Console.WriteLine("Total Green energy left:" + totalGEnergy);

            Console.WriteLine("Total energy used:");
            for (int i = 0; i < totalTasks; i++)
            {
                Console.WriteLine(sortedJobQueue[i]+" : "+consumedEnergy[i]);
            }
        }
        
        public double[] calculateSlackTime(int tCount,DateTime[] taskDateTimeArray, int[] taskRunTimeArray,TimeSpan[] sTimeSpanArray)
        {
      
            double[] slackArray = new double[tCount];
            Console.WriteLine("_________________________________________________________");
            Console.WriteLine("Task Slack times:");
            for (int i = 0; i < tCount; i++)
            {
                //Console.WriteLine(slackArray[i]);
                slackArray[i] = sTimeSpanArray[i].TotalSeconds - (taskRunTimeArray[i]);
                //Console.WriteLine(slackTimeSpanArray[i].TotalSeconds);
            }
            return slackArray;
        }
        public double[] calculateGreenEnery()
        {
            Random rnd = new Random();
            double[] greenEnergyAvail = new double[96];
            double eVal;
            for (int i = 0; i < 96; i++)
            {
                eVal = rnd.Next(0, 1000);
                greenEnergyAvail[i] = eVal;
            }

            //Determining the max Green energy slot and min Green Energy slot
            double maxSlotEnergy = greenEnergyAvail[0];
            List<int> maxSlotPos = new List<int>();
            double minSlotEnergy = greenEnergyAvail[0];
            List<int> minSlotPos = new List<int>();

            for (int i = 0; i < 96; i++)
            {
                if (greenEnergyAvail[i] > maxSlotEnergy)
                {
                    maxSlotEnergy = greenEnergyAvail[i];
                }
                if (greenEnergyAvail[i] <= minSlotEnergy)
                {
                    minSlotEnergy = greenEnergyAvail[i];
                }
            }

            /*for (int i = 0; i < 96; i++)
            {
                Console.WriteLine(i + "--" + greenEnergyAvail[i]);
            }*/


            for (int i = 0; i < 96; i++)
            {
                if (greenEnergyAvail[i] == maxSlotEnergy)
                {
                    maxSlotPos.Add(i);
                }
                if (greenEnergyAvail[i] == minSlotEnergy)
                {
                    minSlotPos.Add(i);
                }
            }
            Console.WriteLine("Slot(s) with maximum green energy:");
            foreach (int i in maxSlotPos)
            {
                Console.WriteLine(i);
            }
            Console.WriteLine("Slot(s) with minimum green energy:");
            foreach (int i in minSlotPos)
            {
                Console.WriteLine(i);
            }

            return greenEnergyAvail;
        }
        /*
         * Random node generator for each slot
         * */
         public List<int> nodeGenerator()
        {
            Random rnd = new Random();
            List<int> nodePerSlotAvail = new List<int>();
            int eVal;
            int counter = 0;
            for (int i = 0; i < 96; i++)
            {
                eVal = rnd.Next(0, 10);
                nodePerSlotAvail.Add(eVal);
            }
            foreach(int slot in nodePerSlotAvail)
            {
                Console.WriteLine(counter+" has "+slot);
                counter = counter + 1;
            }
            return nodePerSlotAvail;
        }

    }
}
