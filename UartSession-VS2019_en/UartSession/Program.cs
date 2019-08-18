using System;
using System.IO.Ports;

namespace UartSession
{
    class Program
    {
        static SerialPort port = new SerialPort();

        static void DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {
                string recvdata = sp.ReadExisting();
                Console.Write(recvdata);
            }
            catch { }
        }

        static void Main(string[] args)
        {
            int index;
            string input;

            port.BaudRate = 115200;
            port.DataBits = 8;
            port.Parity = Parity.None;
            port.StopBits = StopBits.One;
            port.DtrEnable = false;
            port.RtsEnable = false;
            port.ReadTimeout = 1000;
            port.WriteTimeout = 500;
            port.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
            
            while (true)
            {
                int set_baud = -1;
                int ser_no = -1;
                string[] ser_names = { };

                Console.WriteLine("\n\nList of commands:");
                try { ser_names = SerialPort.GetPortNames(); }catch { }
                for (index = 0; index < ser_names.Length; index++)
                    Console.WriteLine("    {0:#0} : Open it. {1:S}", index, ser_names[index]);
                if(index<=0)
                    Console.WriteLine("      (* Port not found *)");
                Console.WriteLine("    baud [Number] : Settings COMPort baud rate，For example, baud 9600 Indicates that the baud rate is set to 9600");
                Console.WriteLine("    refresh  : RefreshCOMPort list");
                Console.WriteLine("    exit  : Quit");
                
                Console.Write("\nThe current baud rate is {0:D}\nPlease enter your command:", port.BaudRate);
                input = Console.ReadLine().Trim();
                try { ser_no = Convert.ToInt32(input); } catch {}
                try{
                    string[] tmps = input.Split();
                    if (tmps.Length == 2 && tmps[0] == "baud")
                        set_baud = Convert.ToInt32(tmps[1]);
                }catch{}

                if (input == "exit")
                    break;
                else if (input == "refresh")
                {
                    Console.WriteLine("\n\n");
                    continue;
                }
                else if (set_baud>0)
                {
                    try
                    {
                        port.BaudRate = set_baud;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("  *** Error: {0:S} ***", ex.Message);
                        continue;
                    }
                }
                else if (ser_no >= 0 && ser_no < index)
                {
                    string ser_name = ser_names[ser_no];
                    try
                    {
                        port.PortName = ser_name;
                        port.Open();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("  *** Open serial error: {0:S} ***", ex.Message);
                        continue;
                    }
                    Console.WriteLine("  It's open.{0:S}，Please enter send data, enter exit for quit", ser_name);
                    while (true)
                    {
                        input = Console.ReadLine().Trim();
                        if (input == "exit")
                            break;
                        try { port.WriteLine(input); }
                        catch { }
                    }
                    port.Close();
                    break;
                }
                else
                    Console.WriteLine("  *** Format error ***");
            }
        }
    }
}
