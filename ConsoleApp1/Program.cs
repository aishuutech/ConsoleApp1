using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp1
{
    internal class Program
    {
        static double findAverage(double[] values)
        {
            double len = values.Length;
            double sum = 0.00D;
            foreach (var value in values)
            {
                sum += value;
            }
            double avg = sum / len;
            return avg;
        }

        static double findMedian(double[] items)
        {
            double len = items.Length;
            double summ = 0.00D;
            Array.Sort(items);
            if (len % 2 != 0)
            {
                double mid = (len / 2) + 0.5;
                return mid;
            }
            else
            {
                double firstmid = len / 2;
                double secondmid = (len / 2) + 1;
                return (firstmid + secondmid) / 2;
            }

        }

        static void readCSV(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Headings>().ToList();
                foreach (var record in records)
                {
                    // Console.WriteLine(record.ToString());   
                    string date = String.Format("{0:dd/MM/yyyy}", record.Date);
                    Console.WriteLine(date + "\t" + record.Grain + "\t" + record.BeerProduction + "\t" + record.FactorManagerName);
                }

            }
        }

        static void writeCSV(string filepath)
        {
            using (var fs = File.Open("Input_v1.0.csv", FileMode.Open, FileAccess.Read, FileShare.Read))
            {

                using (var reader = new StreamReader(fs, Encoding.UTF8))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<Headings>().ToList();

                    var values = new List<OutFileHeaders>();
                    foreach (var record in records)
                    {
                        OutFileHeaders outfile = new OutFileHeaders();
                        outfile.Grain = record.Grain;
                        string date = String.Format("{0:yyyy/MM/dd}", record.Date);
                        Console.WriteLine(date);
                        outfile.Date = date;
                        outfile.Manager = $"\"{record.FactorManagerName}\"";
                        //outfile.Manager = $@"""{record.FactorManagerName}""";
                        //outfile.Manager = String.Format(@"""{0}""",record.FactorManagerName);
                        string BeerProduction=string.Format("{0:0.00}", record.BeerProduction);
                        outfile.BeerProduction = BeerProduction;
                        int year = record.Date.Year;
                        int i = 0;
                        double[] mean = new double[500];
                        double[] median = new double[500];
                        int z = 0;
                        for (i = 0; i < records.Count; i++)
                        {
                            int years = records[i].Date.Year;
                            if (years == year)
                            {
                                mean[z] = (double)records[i].BeerProduction;
                                median[z] = (double)records[i].BeerProduction;
                                z++;
                            }
                        }
                        // outfile.YearMean = findAverage(mean);
                        outfile.YearMean = Queryable.Average(mean.AsQueryable());
                        outfile.YearMedian = findMedian(median);
                        if (record.BeerProduction > outfile.YearMean)
                        {
                            outfile.IsBeerProduct = true;
                        }
                        else
                        {
                            outfile.IsBeerProduct = false;
                        }
                        values.Add(outfile);
                    }
                    using (var fstr = File.Create(filepath))
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            ShouldQuote = (field) => false
                        };
                        using (var writer = new StreamWriter(fstr, Encoding.UTF8))
                        //using (var csvWritter = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        using (var csvWritter = new CsvWriter(writer, config))
                        {
                            csvWritter.WriteRecords(values);

                        }
                        foreach (var value in values)
                        {
                            Console.WriteLine(value.Date + "\t" + value.Grain + "\t" + value.BeerProduction + "\t" + value.Manager + "\t" + value.YearMean + "\t" + value.YearMedian);
                        }
                    }
                }
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the filename to Read:");
            string filename = Console.ReadLine();
            string director = Directory.GetCurrentDirectory();
            Console.WriteLine(director + "\\" + filename);
            string filePath = director + "\\" + filename;
            readCSV(filePath);

            Console.WriteLine("Enter the output filename:");
            string outputFilename = Console.ReadLine();
            string outputfilePath = director + "\\" + outputFilename;
            writeCSV(outputfilePath);
        }
    }

    public class Headings
    {
        [Name("DATE")]
        public DateTime Date { get; set; }
        [Name("grain")]
        public string Grain { get; set; }
        [Name("BeerProduction")]
        public int BeerProduction { get; set; }
        [Name("FactoryManagerName")]
        public string FactorManagerName { get; set; }
    }

    public class OutFileHeaders
    {
        [Name("Grain")]
        public string Grain { get; set; }
        [Name("Date")]
        public string Date { get; set; }
        [Name("Manager")]
        public string Manager { get; set; }
        [Name("BeerProduction")]
        public string BeerProduction { get; set; }
        [Name("YearMean")]
        public double YearMean { get; set; }
        [Name("YearMedian")]
        public double YearMedian { get; set; }
        [Name("IsBeerProduct\r\nionGreaterTh\r\nanYearMean")]
        [BooleanTrueValues("\"yes\"")]
        [BooleanFalseValues("\"no\"")]
        public bool IsBeerProduct { get; set; }
    }
}
