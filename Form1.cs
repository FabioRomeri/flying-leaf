using DataVisualizer;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Linq;

namespace DataVisualizer
{
    public partial class Form1 : Form
    {
        public double[] testVett = new double[25]; //{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
        public char[] buffer = new char[15];

        //public SerialPort myPort;

        public double[,] SerialData = new double[10, 3];

        int mainCicleCounter = -1;
        public Form1()
        {
            InitializeComponent();
            try
            {
                serialPort1.Open();
            }catch(IOException e)
            {
                MessageBox.Show(e.Message);
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //Inizializzazione porta seriale
            //myPort = new SerialPort();
            //myPort.BaudRate = 9600;
            //myPort.PortName = "serialPort1";
            //try
            //{
            //    myPort.Open();
            //    myPort.WriteLine("A");
            //}
            //catch (Exception exc)
            //{
            //    MessageBox.Show(exc.Message, "Errore");
            //}

            dataBindingSource.DataSource = new List<Data>();

            //Temperature chart
            cartesianChart1.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Time",
                Labels = new[] { "100s", "90s", "80s", "70s", "60s", "50s", "40s", "30s", "20s", "10s", "0s" }
            });
            cartesianChart1.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Temperature",
                LabelFormatter = value => value + "°"
            });
            cartesianChart1.LegendLocation = LiveCharts.LegendLocation.Right;

            //AirMoisture chart
            cartesianChart2.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Time",
                Labels = new[] { "100s", "90s", "80s", "70s", "60s", "50s", "40s", "30s", "20s", "10s", "0s" }
            });
            cartesianChart2.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Air Moisture",
                LabelFormatter = value => value + "%"
            });
            cartesianChart2.LegendLocation = LiveCharts.LegendLocation.Right;

            //SoilMoisture chart
            cartesianChart3.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Time",
                Labels = new[] { "100s", "90s", "80s", "70s", "60s", "50s", "40s", "30s", "20s", "10s", "0s" }
            });
            cartesianChart3.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Soil Moisture",
                LabelFormatter = value => value + "%"
            });
            cartesianChart3.LegendLocation = LiveCharts.LegendLocation.Right;  
        }

        private async void loop()
        {
            while (true)
            {
                //wait
                await Task.Delay(5000);
                //Thread.Sleep(1000);

                //Read from serial
                if (serialPort1.IsOpen)
                {
                    serialPort1.Read(buffer, 0, 10);
                    int x = 0, tempValue = 0;
                    string temp = "";
                    for (int y = 0; y < 3; y++)
                    {
                        while (buffer[x] != ';')
                        {
                            temp += buffer[x];
                            x++;
                        }
                        x++;
                        int.TryParse(temp, out tempValue);
                        testVett[y] = tempValue;
                        temp = "";
                        tempValue = 0;
                    }

                    //Load data structure
                    mainCicleCounter++;
                    if (mainCicleCounter < 10)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            SerialData[mainCicleCounter, i] = testVett[i];
                        }
                        dataBindingSource.Add(new Data() { Id = "COM8", Time = mainCicleCounter, Temperature = SerialData[mainCicleCounter, 0], AirMoisture = SerialData[mainCicleCounter, 1], SoilMoisture = SerialData[mainCicleCounter, 2] });
                    }
                    else
                    {
                        int j = 0;
                        dataBindingSource.Clear();
                        for (j = 0; j < 9; j++)
                        {
                            dataBindingSource.Add(new Data() { Id = "COM8", Time = j, Temperature = SerialData[j + 1, 0], AirMoisture = SerialData[j + 1, 1], SoilMoisture = SerialData[j + 1, 2] });
                            SerialData[j, 0] = SerialData[j + 1, 0];
                            SerialData[j, 1] = SerialData[j + 1, 1];
                            SerialData[j, 2] = SerialData[j + 1, 2];
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            SerialData[j, i] = testVett[i];
                        }
                        dataBindingSource.Add(new Data() { Id = "COM8", Time = j, Temperature = SerialData[j, 0], AirMoisture = SerialData[j, 1], SoilMoisture = SerialData[j, 2] });

                    }

                    //Save data to file
                    //string text = $"Temperature: {testVett[0]}; Air moisture: {testVett[1]}; Soil moisture: {testVett[2]};";
                    //File.AppendAllText(@"C: \Users\fabio\Documents\Visual studio projects\DataVisualizer\DataVisualizer\TextFile1.txt",text);

                    //Chart creation
                    //cartesianChart1.Series.Clear();
                    SeriesCollection TempSeries = new SeriesCollection();
                    SeriesCollection AirMoistureSeries = new SeriesCollection();
                    SeriesCollection SoilMoistureSeries = new SeriesCollection();

                    var ids = (from o in dataBindingSource.DataSource as List<Data>
                               select new { Id = o.Id }).Distinct();
                    foreach (var id in ids)
                    {
                        List<double> TempValues = new List<double>();
                        List<double> AirMoistureValues = new List<double>();
                        List<double> SoilMoistureValues = new List<double>();
                        for (int time = 0; time < 10; time++)
                        {
                            double TempValue = 0;
                            double AirMoistureValue = 0;
                            double SoilMoistureValue = 0;
                            //Temp
                            var TempData = from o in dataBindingSource.DataSource as List<Data>
                                           where o.Id.Equals(id.Id) && o.Time.Equals(time)
                                           orderby o.Time ascending
                                           select new { o.Temperature, o.Time };
                            if (TempData.SingleOrDefault() != null)
                                TempValue = TempData.SingleOrDefault().Temperature;
                            //AirMoisture
                            var AirMoistureData = from o in dataBindingSource.DataSource as List<Data>
                                                  where o.Id.Equals(id.Id) && o.Time.Equals(time)
                                                  orderby o.Time ascending
                                                  select new { o.AirMoisture, o.Time };
                            if (AirMoistureData.SingleOrDefault() != null)
                                AirMoistureValue = AirMoistureData.SingleOrDefault().AirMoisture;
                            //SoilMoisture
                            var SoilMoistureData = from o in dataBindingSource.DataSource as List<Data>
                                                   where o.Id.Equals(id.Id) && o.Time.Equals(time)
                                                   orderby o.Time ascending
                                                   select new { o.SoilMoisture, o.Time };
                            if (SoilMoistureData.SingleOrDefault() != null)
                                SoilMoistureValue = SoilMoistureData.SingleOrDefault().SoilMoisture;

                            TempValues.Add(TempValue);
                            AirMoistureValues.Add(AirMoistureValue);
                            SoilMoistureValues.Add(SoilMoistureValue);
                        }
                        TempSeries.Add(new LineSeries() { Title = id.Id.ToString(), Values = new ChartValues<double>(TempValues) });
                        AirMoistureSeries.Add(new LineSeries() { Title = id.Id.ToString(), Values = new ChartValues<double>(AirMoistureValues) });
                        SoilMoistureSeries.Add(new LineSeries() { Title = id.Id.ToString(), Values = new ChartValues<double>(SoilMoistureValues) });
                    }
                    cartesianChart1.Series = TempSeries;
                    cartesianChart2.Series = AirMoistureSeries;
                    cartesianChart3.Series = SoilMoistureSeries;
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            loop();
        }
    }
}
