using System;
using System.IO;
using System.IO.Ports;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace USTCRVSoC_tool
{
    public partial class MainForm : Form
    {
        private const string RISCV_TOOLS_PATH = ".\\riscv32-elf-tools-windows\\";    // RISC-V Path to the tool chain

        #region Control the count of received bytes
        private uint _userPortCount;
        private uint userPortCount    // Receive byte count property
        {
            get
            {
                return _userPortCount;
            }
            set
            {
                _userPortCount = value;
                changeCountText(String.Format("Receive.: {0:D} B", _userPortCount));
            }
        }
        #endregion

        public MainForm()    // Form constructor
        {
            InitializeComponent();
            InitializeCurrentPort(null, null);
        }

        #region Automatically check the existence of the string
        private void InitializeCurrentPort(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            portSelectionBox.Items.Clear();
            portSelectionBox.Items.AddRange(ports);
            if (portSelectionBox.Items.Count > 0)
            {
                portSelectionBox.SelectedIndex = 0;
            }
            else
            {
                compilePromptText.Text = "The serial port is not found, please insert the device, or check whether the serial driver is installed";
            }
        }
        #endregion

        #region Open, save, and save assembly code files
        private void fileSelectionBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = ".\\";   //Note that the path is written here using c:\\instead c:\
            openFileDialog.Filter = "Assembly language files|*.S";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileSelectionText.Text = openFileDialog.FileName;
                try
                {
                    codeText.Text = System.IO.File.ReadAllText(openFileDialog.FileName);
                    compilePromptText.Text = "Opened file";
                    saveBtn.Enabled = true;
                }
                catch (Exception ex)
                {
                    compilePromptText.Text = "Failed to open a file\n  " + ex.Message;
                }
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.File.WriteAllText(fileSelectionText.Text, codeText.Text);
                compilePromptText.Text = "  Saved file";
            }
            catch (Exception ex)
            {
                compilePromptText.Text = "  Failed to save the file\r\n" + ex.Message;
            }
        }

        private void otherSaveBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = ".\\";   //Note that the path is written here using c:\\instead of c:\
            saveFileDialog.Filter = "Assembly language files|*.S";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FilterIndex = 1;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileSelectionText.Text = saveFileDialog.FileName;
                try
                {
                    System.IO.File.WriteAllText(fileSelectionText.Text, codeText.Text);
                    compilePromptText.Text = "Saved file";
                }
                catch (Exception ex)
                {
                    compilePromptText.Text = "Failed to save the file\n  " + ex.Message;
                }
            }
        }
        #endregion

        #region Compilation
        public bool RunCmd(string path, string command, ref string msg)     // Call CMD to run a command
        {
            try
            {
                msg = ">" + command + "\r\n\r\n";
                System.Diagnostics.Process pro = new System.Diagnostics.Process();
                pro.StartInfo.FileName = "cmd.exe";
                pro.StartInfo.CreateNoWindow = true;         // Do not create new Windows    
                pro.StartInfo.UseShellExecute = false;       // Shell startup process is not enabled
                pro.StartInfo.RedirectStandardInput = true;  // Redirected input   
                pro.StartInfo.RedirectStandardOutput = true; // Redirect standard output    
                pro.StartInfo.RedirectStandardError = true;
                pro.StartInfo.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
                pro.StartInfo.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;  // Redirect error output  
                pro.StartInfo.WorkingDirectory = path;
                pro.Start();               //Open cmd
                pro.StandardInput.WriteLine(command);
                pro.StandardInput.AutoFlush = true;
                pro.StandardInput.WriteLine("exit"); //f the run time is short, this command can be added
                pro.WaitForExit();//If the running time is long, use this, wait for the program to execute the exit process
                string errorStr = pro.StandardError.ReadToEnd();
                msg += errorStr;
                pro.Close();
                return errorStr.Trim().Length == 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\nMake sure that the toolchain directory is in the same directory as this program");
                return false;
            }
        }
        private string dumpBin(string bin_file_path)   // Read the compilation out of.bin file and adjust the byte order, converted to a single line instruction
        {
            StringBuilder strbuild = new StringBuilder();
            byte[] bin = System.IO.File.ReadAllBytes(bin_file_path);
            for (int i = 0; i < bin.Length - 3; i += 4)
            {
                for (int j = 3; j >= 0; j--)
                    strbuild.Append(bin[i + j].ToString("x2"));
                strbuild.AppendLine();
            }
            return strbuild.ToString();
        }
        private void compileBtn_Click(object sender, EventArgs e)   // Click the "assembly" button, complete a series of CMD commands, and compile the results into binText this control
        {
            bool stat;
            string msg = "";
            string asm_command = "riscv32-elf-as " + fileSelectionText.Text + " -o compile_tmp.o -march=rv32im";
            string ld_command = "riscv32-elf-ld compile_tmp.o -o compile_tmp.om";
            compilePromptText.Clear();

            try
            {
                System.IO.File.WriteAllText(fileSelectionText.Text, codeText.Text);
            }
            catch (Exception ex)
            {
                compilePromptText.Text = "Failed to save the file\n  " + ex.Message;
                return;
            }

            stat = RunCmd(RISCV_TOOLS_PATH, asm_command, ref msg);
            compilePromptText.AppendText(msg);
            if (!stat)
            {
                compilePromptText.AppendText("  *** Compilation error! ***");
                return;
            }

            stat = RunCmd(RISCV_TOOLS_PATH, ld_command, ref msg);
            compilePromptText.AppendText(msg);
            if (!stat)
            {
                compilePromptText.AppendText("  *** Error generating om file! ***");
                return;
            }

            stat = RunCmd(RISCV_TOOLS_PATH, "del compile_tmp.o", ref msg);
            compilePromptText.AppendText(msg);
            if (!stat)
            {
                compilePromptText.AppendText("  *** Error deleting intermediate files! ***");
                return;
            }

            stat = RunCmd(RISCV_TOOLS_PATH, "riscv32-elf-objcopy -O binary compile_tmp.om compile_tmp.bin", ref msg);
            compilePromptText.AppendText(msg);
            if (!stat)
            {
                compilePromptText.AppendText("  *** Error generating bin file! ***");
                return;
            }

            stat = RunCmd(RISCV_TOOLS_PATH, "del compile_tmp.om", ref msg);
            compilePromptText.AppendText(msg);
            if (!stat)
            {
                compilePromptText.AppendText("  *** Error deleting intermediate files! ***");
                return;
            }

            try
            {
                binText.Text = dumpBin(RISCV_TOOLS_PATH + "compile_tmp.bin");
                compilePromptText.AppendText("  *** Compile complete! ***");
            }
            catch
            {
                compilePromptText.AppendText("  *** Error reading bin file! ***");
                return;
            }
        }
        #endregion

        #region 生成 Verilog InstrROM 代码
        private const string VerilogHead = "module instr_rom(\n    input  logic clk, rst_n,\n    naive_bus.slave  bus\n);\nlocalparam  INSTR_CNT = 30'd";
        private const string VerilogMid = ";\nwire [0:INSTR_CNT-1] [31:0] instr_rom_cell = {\n";
        private const string VerilogTail = "};\n\nlogic [29:0] cell_rd_addr;\n\nassign bus.rd_gnt = bus.rd_req;\nassign bus.wr_gnt = bus.wr_req;\nassign cell_rd_addr = bus.rd_addr[31:2];\nalways @ (posedge clk or negedge rst_n)\n    if(~rst_n)\n        bus.rd_data <= 0;\n    else begin\n        if(bus.rd_req)\n            bus.rd_data <= (cell_rd_addr>=INSTR_CNT) ? 0 : instr_rom_cell[cell_rd_addr];\n        else\n            bus.rd_data <= 0;\n        end\n\nendmodule\n\n";

        private string genVerilogRom()
        {
            StringBuilder strBuilder = new StringBuilder();
            int index = 0;
            string[] lines = binText.Text.Trim().Split();
            for (int idx = 0; idx < lines.Length; idx++)
            {
                string line = lines[idx];
                string hex_num = line.Trim();
                if (hex_num.Length <= 0)
                    continue;
                if (idx < lines.Length - 2)
                    strBuilder.Append(String.Format("    32'h{1:S},   // 0x{0:x8}\n", index * 4, hex_num));
                else
                    strBuilder.Append(String.Format("    32'h{1:S}    // 0x{0:x8}\n", index * 4, hex_num));
                index += 1;
            }
            strBuilder.Insert(0, VerilogMid);
            strBuilder.Insert(0, index.ToString());
            strBuilder.Insert(0, VerilogHead);
            strBuilder.Append(VerilogTail);
            return strBuilder.ToString();
        }

        private void saveVerilog_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = ".\\";   //Note that the path is written here using c:\\instead of c:\
            saveFileDialog.Filter = "SystemVerilog Source files||*.sv";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FilterIndex = 1;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.IO.File.WriteAllText(saveFileDialog.FileName, genVerilogRom());
                    compilePromptText.Text = "Saved Verilog ROM Documentation";
                }
                catch (Exception ex)
                {
                    compilePromptText.Text = "Save Verilog ROM File failed\r\n" + ex.Message;
                }
            }
        }
        #endregion

        #region Serial port command function
        private bool serialSessionA(string send, ref string response)    // Send a command and get the response string
        {
            return serialSessionTry(send, ref response, "");
        }

        private bool serialSessionB(string send, string respectResponse)   // Sends a command and waits for the specified response string to arrive
        {
            string response = "";
            return serialSessionTry(send, ref response, respectResponse);
        }

        private bool serialSessionTry(string send, ref string response, string respectResponse, int try_time = 3)    // When multiple requests fail all, it returns a failure, otherwise it returns a success
        {
            for (int i = 0; i < try_time; i++)
            {
                try { serialPort.ReadExisting(); }// Empty the receive buffer
                catch { }   
                if (serialSend(send))
                {
                    if (serialRead(ref response, respectResponse))
                        return true;
                }
            }
            compilePromptText.AppendText("  *** Serial debugging failed multiple attempts ***\r\n");
            return false;
        }

        private bool serialSend(string send)
        {
            compilePromptText.AppendText("send: " + send);
            try
            {
                serialPort.Write(send + "\n");
            }
            catch (Exception ex)
            {
                compilePromptText.AppendText("    " + ex.Message + "\r\n");
                return false;
            }
            return true;
        }

        private bool serialRead(ref string response, string respectResponse)
        {
            try
            {
                for (int i = 0; i < 8; i++)
                {
                    response = serialPort.ReadLine().Trim();
                    bool is_respect = respectResponse.Equals("") || respectResponse.Equals(response);
                    if (is_respect)
                    {
                        compilePromptText.AppendText("    response: " + response + "\r\n");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                compilePromptText.AppendText("    " + ex.Message + "\r\n");
                return false;
            }
            compilePromptText.AppendText("    response: *** Timeout ***\r\n" + response);
            return false;
        }
        #endregion

        #region Serial open
        private bool refreshSerial()
        {
            if (serialPort.IsOpen)
                serialPort.Close();
            try
            {
                serialPort.PortName = portSelectionBox.Text;
                serialPort.Open();
            }
            catch (Exception ex)
            {
                compilePromptText.AppendText("  *** Error opening the serial port ***\r\n  " + ex.Message);
                refreshPortStatus();
                return false;
            }
            return true;
        }
        private void refreshPortStatus()
        {
            if (serialPort.IsOpen)
                userPortOpenCloseBtn.Text = "Close";
            else
                userPortOpenCloseBtn.Text = "Open it.";
        }
        private void userPortOpenCloseBtn_Click(object sender, EventArgs e)
        {
            if (userPortOpenCloseBtn.Text == "Open it.")
            {
                compilePromptText.Clear();
                refreshSerial();
                serialSessionB("s", "debug");
                serialSessionB("o", "user");
            }
            else
            {
                serialPort.Close();
            }
            refreshPortStatus();
        }
        #endregion

        #region Burning program
        private void programBtn_Click(object sender, EventArgs e)    // Burning program
        {
            enableUartDisplay = false;
            userPortTextBox.Clear();
            compilePromptText.Clear();

            uint boot_addr;
            try
            {
                boot_addr = Convert.ToUInt32(bootAddrTextBox.Text, 16);
            }
            catch (Exception ex)
            {
                compilePromptText.AppendText("  *** Boot addr wrong format ***\r\n  " + ex.Message);
                return;
            }

            if (!refreshSerial())
                return;

            if (!serialSessionB("s", ""))
                return;

            uint index = 0;
            foreach (string line in binText.Text.Split())
            {
                string hex_num = line.Trim();
                if (hex_num.Length <= 0)
                    continue;
                string send_str = String.Format("{0:x8} {1:S}", boot_addr + index * 4, hex_num);
                index++;

                if (!serialSessionB(send_str, "wr done"))
                    return;
            }

            if (!serialSessionB(string.Format("r{0:x8}", boot_addr), "rst done"))
                return;

            compilePromptText.AppendText(" *** Burn finished ***\r\n");
            try { serialPort.ReadExisting(); }// Empty the receive buffer
            catch { }   
            userPortTextBox.Clear();
            enableUartDisplay = true;
        }
        #endregion

        #region DUMPMemory
        private void DUMPMemory_Click(object sender, EventArgs e)     // View memory
        {
            enableUartDisplay = false;
            userPortTextBox.Clear();
            compilePromptText.Clear();

            uint start, len;
            try
            {
                start = Convert.ToUInt32(StartAddr.Text, 16);
                len = Convert.ToUInt32(Length.Text, 16);
            }
            catch (Exception ex)
            {
                compilePromptText.AppendText("  *** Wrong start address format ***\r\n  " + ex.Message);
                return;
            }
            start = 4 * (start / 4);   // Start address is automatically aligned with 4
            if (len > 0x1000)
            {
                compilePromptText.AppendText("  *** Length can not be greater than 0x1000 ***\r\n  ");
                return;
            }
            len /= 4;

            if (!refreshSerial())
                return;
            string response = "";
            if (!serialSessionB("s", ""))
                return;

            MemContents.Clear();

            uint index = 0;
            for (index = 0; index < len; index++)
            {
                string send_str = String.Format("{0:x8}", start + index * 4);
                response = "";
                if (!serialSessionA(send_str, ref response))
                    return;
                MemContents.AppendText(String.Format("{0:x8} : {1:S}\r\n", start + index * 4, response.Trim()));
            }

            serialSessionB("o", "user");
            compilePromptText.AppendText(" *** Dump Memory complete ***\r\n");
            try { serialPort.ReadExisting(); }// Empty the receive buffer
            catch { }   
            userPortTextBox.Clear();
            enableUartDisplay = true;
        }
        #endregion

        #region Real-time display of the right side Serial Monitor window
        bool enableUartDisplay = true;
        public delegate void changeTextHandler(object str);

        private void appendUserPortText(object str)
        {
            if (userPortTextBox.InvokeRequired == true)
            {
                changeTextHandler ct = new changeTextHandler(appendUserPortText);
                userPortTextBox.Invoke(ct, new object[] { str });
            }
            else
            {
                userPortTextBox.AppendText(str.ToString());
            }
        }

        private void changeCountText(object str)
        {
            if (UserPortRecvCountLabel.InvokeRequired == true)
            {
                changeTextHandler ct = new changeTextHandler(changeCountText);
                UserPortRecvCountLabel.Invoke(ct, new object[] { str });
            }
            else
            {
                UserPortRecvCountLabel.Text = str.ToString();
            }
        }

        private void userPortClearBtn_Click(object sender, EventArgs e)
        {
            userPortTextBox.Clear();
        }

        private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (enableUartDisplay)
            {
                SerialPort sp = (SerialPort)sender;
                try
                {
                    string recvdata = sp.ReadExisting();
                    if (userPortShowHex.Checked)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (byte ch in recvdata)
                        {
                            sb.Append(String.Format("{0:X2} ", ch));
                        }
                        appendUserPortText(sb.ToString());
                    }
                    else
                    {
                        appendUserPortText(recvdata);
                    }
                    userPortCount += (uint)recvdata.Length;
                }
                catch { }
            }
        }
        #endregion

        private void tableLayoutPanel6_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
