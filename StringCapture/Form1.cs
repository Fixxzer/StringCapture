using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StringCapture
{
    public partial class Form1 : Form
    {
        static readonly HashSet<string> Strings = new HashSet<string>();
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtRootFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void BtnProcess_Click(object sender, EventArgs e)
        {
            new Thread(ProcessFiles).Start();
        }

        private void ProcessFiles()
        {
            if (string.IsNullOrWhiteSpace(txtRootFolder.Text) || string.IsNullOrWhiteSpace(folderBrowserDialog1.SelectedPath))
                return;

            RecursivelyParseDirectory(folderBrowserDialog1.SelectedPath);

            // Write the results when no more files are found
            using (StreamWriter fout = new StreamWriter("C:\\Temp\\ParseResults2.txt", false))
            {
                foreach (string s in Strings)
                {
                    fout.WriteLine(s);
                }
            }

            Strings.Clear();
            MessageBox.Show("Parse Complete");
        }

        private void RecursivelyParseDirectory(string path)
        {
            // Get all the files in the directory
            IEnumerable<string> files = Directory.EnumerateFiles(path);

            // Output path and current file to listbox
            foreach (string file in files)
            {
                // Hard coded filter
                //if (Path.GetExtension(file) != ".cs")
                //    continue;

                UpdateListBox(file);

                // Add all the parse results to memory, write the results to file after reading is complete for file
                using (StreamReader fin = new StreamReader(file))
                {
                    var text = fin.ReadToEnd();

                    // Only search for strings if the word "SqlCommand" appears somewhere in the file
                    //if (!text.Contains("SqlCommand"))
                    //    continue;
                    
                    // Slow part, could use some optimizing
                    bool start = false;
                    int startIndex = 0;
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (text[i] == '\"')
                        {
                            start = !start;

                            // if start is true, we found the first one
                            if (start)
                                startIndex = i + 1; // increment by 1 to not include the quote
                            else // if start is not true, we found the last one, grab the substring between current index and start index
                            {
                                string substring = text.Substring(startIndex, i - startIndex);
                                if (!string.IsNullOrWhiteSpace(substring) &&
                                    //substring.Contains("_") ||
                                    //substring.ToLower().Contains("select") ||
                                    //substring.ToLower().Contains("insert") ||
                                    //substring.ToLower().Contains("update") ||
                                    //substring.ToLower().Contains("delete"))
                                    substring.ToLower().Contains("ipad"))
                                {
                                    string[] splitString = substring.Split('.');
                                    StringBuilder sb = new StringBuilder();
                                    int stringSplitIndex = 0; 
                                    for (int j = 0; j < 3; j++)
                                    {
                                        if (3 - j <= splitString.Length)
                                        {
                                            sb.Append(splitString[stringSplitIndex++]);
                                        }
                                        sb.Append("\t");
                                    }
                                    sb.Append(file);
                                    Strings.Add(sb.ToString());
                                }
                            }
                        }
                    }
                }
            }

            // Get the directories, pass directory to recursive method
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                RecursivelyParseDirectory(dir);
            }
        }

        private void UpdateListBox(string file)
        {
            Invoke((MethodInvoker)(() => listBox1.Items.Add(file)));
            Invoke((MethodInvoker)(() => listBox1.SelectedIndex = listBox1.Items.Count - 1));
            //if (listBox1.InvokeRequired)
            //{
            //    UpdateListBox(file);
            //}
            //else
            //{
            //    listBox1.Items.Add(file);
            //    listBox1.SelectedIndex = listBox1.Items.Count - 1;
            //}
        }
    }
}
