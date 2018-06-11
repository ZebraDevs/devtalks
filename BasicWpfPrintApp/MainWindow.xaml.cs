using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Zebra.Sdk.Comm;
using Zebra.Sdk.Printer;
using Zebra.Sdk.Printer.Discovery;

namespace BasicWpfPrintApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)this.FindName("textBox");
            string address = tb.Text;
            if ((address.Length > 0) && (IPAddress.TryParse(address, out IPAddress ipAddress)))
            {
                await PrintUSBTask(address);
            }
            else
            {
                Console.WriteLine($"Enter a valid IP Address");
            }
        }

        private void SendZplOverTcp(string theIpAddress)
        {
            // Instantiate connection for ZPL TCP port at given address
            Connection thePrinterConn = new TcpConnection(theIpAddress, TcpConnection.DEFAULT_ZPL_TCP_PORT);

            try
            {
                // Open the connection - physical connection is established here.
                thePrinterConn.Open();

                // This example prints "This is a ZPL test." near the top of the label.
                string zplData = "^XA^FO20,20^A0N,25,25^FDThis is a ZPL test.^FS^XZ";

                // Send the data to printer as a byte array.
                thePrinterConn.Write(Encoding.UTF8.GetBytes(zplData));
            }
            catch (ConnectionException e)
            {
                // Handle communications error here.
                Console.WriteLine(e.ToString());
            }
            finally
            {
                // Close the connection to release resources.
                thePrinterConn.Close();
            }
        }
        
        private async Task PrintOneLabelTask(string theIpAddress)
        {
            string ZPL_STRING = "^XA^FO50,50^A0N,50,50^FDHello World^FS^XZ";
            await Task.Run(() =>
            {
                // Instantiate connection for ZPL TCP port at given address
                Connection thePrinterConn = new TcpConnection(theIpAddress, TcpConnection.DEFAULT_ZPL_TCP_PORT);

                ZebraPrinter printer = PrintHelper.Connect(thePrinterConn, PrinterLanguage.ZPL);
                PrintHelper.SetPageLanguage(printer);
                if (PrintHelper.CheckStatus(printer))
                {
                    PrintHelper.Print(printer, ZPL_STRING);
                    if (PrintHelper.CheckStatusAfter(printer))
                    {
                        Console.WriteLine($"Label Printed");
                    }
                }
                printer = PrintHelper.Disconnect(printer);
                Console.WriteLine("Done Printing");
            });
        }

        private async Task PrintImageTask(string theIpAddress)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JPG files (*.jpg)|*.jpg|PNG files (*.png)|*.png|BMP files (*.bmp)|*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                await Task.Run(() =>
                {

                    // Instantiate connection for ZPL TCP port at given address
                    Connection thePrinterConn = new TcpConnection(theIpAddress, TcpConnection.DEFAULT_ZPL_TCP_PORT);

                    ZebraPrinter printer = PrintHelper.Connect(thePrinterConn, PrinterLanguage.ZPL);
                    PrintHelper.SetPageLanguage(printer);
                    if (PrintHelper.CheckStatus(printer))
                    {
                        printer.PrintImage(path, 20, 20);
                        if (PrintHelper.CheckStatusAfter(printer))
                        {
                            Console.WriteLine($"Label Printed");
                        }
                    }
                    printer = PrintHelper.Disconnect(printer);
                    Console.WriteLine("Done Printing");
                });
            }
        }

        private async Task PrintUSBTask(string theIpAddress)
        {
            string ZPL_STRING = "^XA^FO50,50^A0N,50,50^FDHello World^FS^XZ";
            await Task.Run(() =>
            {
                DiscoveredPrinter discoveredPrinter = GetUSBPrinter();

                ZebraPrinter printer = PrintHelper.Connect(discoveredPrinter, PrinterLanguage.ZPL);
                PrintHelper.SetPageLanguage(printer);
                if (PrintHelper.CheckStatus(printer))
                {
                    PrintHelper.Print(printer, ZPL_STRING);
                    if (PrintHelper.CheckStatusAfter(printer))
                    {
                        Console.WriteLine($"Label Printed");
                    }
                }
                printer = PrintHelper.Disconnect(printer);
                Console.WriteLine("Done Printing");
            });
        }
        public DiscoveredPrinter GetUSBPrinter()
        {
            DiscoveredPrinter discoveredPrinter = null;
            try
            {
                foreach (DiscoveredUsbPrinter usbPrinter in UsbDiscoverer.GetZebraUsbPrinters())
                {
                    discoveredPrinter = usbPrinter;
                    Console.WriteLine(usbPrinter);
                }
            }
            catch (ConnectionException e)
            {
                Console.WriteLine($"Error discovering local printers: {e.Message}");
            }

            Console.WriteLine("Done discovering local printers.");
            return discoveredPrinter;
        }
    }

    public class PrintHelper
    {
        public static bool CheckStatusAfter(ZebraPrinter printer)
        {
            PrinterStatus printerStatus = null;
            try
            {
                VerifyConnection(printer);
                printerStatus = printer.GetCurrentStatus();
                while ((printerStatus.numberOfFormatsInReceiveBuffer > 0) && (printerStatus.isReadyToPrint))
                {
                    Thread.Sleep(500);
                    printerStatus = printer.GetCurrentStatus();
                }
            }
            catch (ConnectionException e)
            {
                Console.WriteLine($"Error getting status from printer: {e.Message}");
            }

            if (printerStatus.isReadyToPrint)
            {
                Console.WriteLine($"Ready To Print");
                return true;
            }
            else if (printerStatus.isPaused)
            {
                Console.WriteLine($"Cannot Print because the printer is paused.");
            }
            else if (printerStatus.isHeadOpen)
            {
                Console.WriteLine($"Cannot Print because the printer head is open.");
            }
            else if (printerStatus.isPaperOut)
            {
                Console.WriteLine($"Cannot Print because the paper is out.");
            }
            else
            {
                Console.WriteLine($"Cannot Print.");
            }
            return false;
        }
        public static bool Print(ZebraPrinter printer, string printstring)
        {
            bool sent = false;
            try
            {
                VerifyConnection(printer);
                printer.Connection.Write(Encoding.UTF8.GetBytes(printstring));
                sent = true;
            }
            catch (ConnectionException e)
            {
                Console.WriteLine($"Unable to write to printer: {e.Message}");
            }
            return sent;
        }
        public static bool SetPageLanguage(ZebraPrinter printer)
        {
            bool set = false;
            string setLang = "zpl";
            if (PrinterLanguage.ZPL != printer.PrinterControlLanguage)
            {
                setLang = "line_print";
            }

            try
            {
                VerifyConnection(printer);
                SGD.SET("device.languages", setLang, printer.Connection);
                string getLang = SGD.GET("device.languages", printer.Connection);
                if (getLang.Contains(setLang))
                {
                    set = true;
                }
                else
                {
                    Console.WriteLine($"This is not a {setLang} printer.");
                }
            }
            catch (ConnectionException e)
            {
                Console.WriteLine($"Unable to set print language: {e.Message}");
            }
            return set;
        }
        public static bool CheckStatus(ZebraPrinter printer)
        {
            PrinterStatus printerStatus = null;
            try
            {
                VerifyConnection(printer);
                printerStatus = printer.GetCurrentStatus();
            }
            catch (ConnectionException e)
            {
                Console.WriteLine($"Error getting status from printer: {e.Message}");
            }

            if (null == printerStatus)
            {
                Console.WriteLine($"Unable to get status.");
            }
            else if (printerStatus.isReadyToPrint)
            {
                Console.WriteLine($"Ready To Print");
                return true;
            }
            else if (printerStatus.isPaused)
            {
                Console.WriteLine($"Cannot Print because the printer is paused.");
            }
            else if (printerStatus.isHeadOpen)
            {
                Console.WriteLine($"Cannot Print because the printer head is open.");
            }
            else if (printerStatus.isPaperOut)
            {
                Console.WriteLine($"Cannot Print because the paper is out.");
            }
            else
            {
                Console.WriteLine($"Cannot Print.");
            }
            return false;
        }
        public static bool VerifyConnection(ZebraPrinter printer)
        {
            bool ok = false;
            try
            {
                if (!printer.Connection.Connected)
                {
                    printer.Connection.Open();
                    if (printer.Connection.Connected)
                        ok = true;
                }
                else ok = true;
            }
            catch (ConnectionException e)
            {
                Console.WriteLine($"Unable to connect to printer: {e.Message}");
            }
            return ok;
        }
        public static ZebraPrinter Connect(Connection connection, PrinterLanguage language)
        {
            ZebraPrinter printer = null;
            try
            {
                connection.Open();
                if (connection.Connected)
                {
                    printer = ZebraPrinterFactory.GetInstance(language, connection);
                    Console.WriteLine($"Printer Connected");
                }
                else Console.WriteLine($"Printer Not Connected!");
            }
            catch (ConnectionException e)
            {
                Console.WriteLine($"Error connecting to printer: {e.Message}");
            }
            return printer;
        }
        public static ZebraPrinter Connect(DiscoveredPrinter discoveredPrinter, PrinterLanguage language)
        {
            ZebraPrinter printer = null;
            try
            {
                Connection connection = discoveredPrinter.GetConnection();
                printer = ZebraPrinterFactory.GetInstance(language, connection);
                printer.Connection.Open();
                if (printer.Connection.Connected)
                    Console.WriteLine($"Printer Connected");
                else Console.WriteLine($"Printer Not Connected!");
            }
            catch (ConnectionException e)
            {
                Console.WriteLine($"Error connecting to printer: {e.Message}");
            }
            return printer;
        }
        public static ZebraPrinter Disconnect(ZebraPrinter printer)
        {
            try
            {
                printer.Connection.Close();
                Console.WriteLine($"Printer Disconnected");
            }
            catch (ConnectionException e)
            {
                Console.WriteLine($"Error disconnecting from printer: {e.Message}");
            }
            return printer;
        }
    }
}
