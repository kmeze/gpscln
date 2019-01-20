using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace gpscln
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourceFileName = args[0];

            if (args.Count() == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("gpscln fileName");
            }

            try
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Lodaing file {0}...", sourceFileName);

                // Load data
                var lines = File.ReadLines(sourceFileName).ToArray();
                lines = lines.Skip(2).ToArray();

                // Count satelites
                Console.WriteLine("Analysing data....");

                var linesWithSatelites = lines.Where(s => s.Contains("G")).ToArray();
                var linesWithSatsCnt = linesWithSatelites.Count();
                var linesWithTimeCnt = lines.Where(ln => ln.EndsWith("-1")).ToArray().Count();
                var linesWithValuesCnt = lines.Where(s => !(s.Contains("G") || s.EndsWith("-1"))).ToArray().Count();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(Environment.NewLine);

                Console.WriteLine("Total lines {0}.", lines.Count() + 2);
                Console.WriteLine("Lines with satelites {0}.", linesWithSatsCnt);
                Console.WriteLine("Lines with time {0}.", linesWithTimeCnt);
                Console.WriteLine("Lines with values {0}.", linesWithValuesCnt);

                if(linesWithSatsCnt + linesWithTimeCnt == linesWithValuesCnt)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Validating counters: OK");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Validating counters error: Invalid file format: Sum of lines with satelites and lines with time does'n match count of lines with values");
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(Environment.NewLine);


                var sats = String.Join(" ", linesWithSatelites)
                                .Split(' ')
                                .Where(s => s.StartsWith("G"))
                                .Distinct()
                                .OrderBy(s => s)
                                .ToArray();

                var maxSatNumber = int.Parse(sats.Last().Replace("G", ""));

                Console.WriteLine("Total satelies {0}: {1}.", sats.Count(), String.Join(" ", sats));
                Console.WriteLine(Environment.NewLine);

                // Create memory matrix
                string[,] matrix = new string[linesWithValuesCnt + 1, maxSatNumber + 1];
                matrix[0, 0] = "Time";
                for (int i = 1; i <= maxSatNumber; i++)
                    matrix[0, i] = String.Format("G{0}", i);

                // Process data line by line
                Console.WriteLine("Processing data...");

                var currentTime = "";
                var currentSatelites = new List<string>();
                var currentValCnt = 0;

                for (int i = 0; i < lines.Count(); i++)
                {
                    string line = lines[i].Trim();
                    var lineSegments = line.Split(' ').Where(s => !String.IsNullOrWhiteSpace(s)).ToArray();

                    Console.Write("\rProcessing line {0}", i + 3);

                    if (line.Contains("G"))                                         // Line with satelites
                    {
                        currentTime = lineSegments.First();
                        currentSatelites = lineSegments.Where(ls => ls.StartsWith("G")).ToList();
                    }
                    else if (line.EndsWith("-1"))                                   // Line with time
                    {
                        currentTime = lineSegments[0];
                    }
                    else                                                            // Line with values
                    {
                        currentValCnt++;

                        matrix[currentValCnt, 0] = currentTime;

                        for (int v = 0; v < lineSegments.Count(); v++)
                        {
                            var gnx = int.Parse(currentSatelites[v].Replace("G", ""));
                            var value = lineSegments[v];
                            matrix[currentValCnt, gnx] = value;
                        }
                    }
                }

                Console.WriteLine("\nSaving data...");

                var outputFile = sourceFileName + ".output";
                var allLines = new List<string>();

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    var outputLine = new List<string>();

                    for (int j = 0; j < matrix.GetLength(1); j++)
                        outputLine.Add(matrix[i, j]);

                    allLines.Add(String.Join("\t", outputLine));
                }

                // Write file
                if (File.Exists(outputFile))
                    File.Delete(outputFile);

                File.WriteAllLines(outputFile, allLines);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(String.Format("DONE. Result saved to file: {0}", outputFile));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(String.Format("Error: {0}", ex.Message));
            }
            finally
            {
                // Wait to exit
                Console.WriteLine(Environment.NewLine);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
