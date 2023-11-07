using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MARC;
using Microsoft.Win32;

namespace DemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private class FileReaderArgs
        {
            public string Filename { get; set; }
            public CancellationToken CancellationToken { get; set; }

            public FileReaderArgs()
            {
                Filename = String.Empty;
                CancellationToken = CancellationToken.None;
            }
        }

        private Thread? thread = null;
        private CancellationTokenSource cancelTokenSource;

        private ProgressDialog progressDialog;
        private List<Record> records = new List<Record>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                cancelTokenSource?.Cancel();
            }
            catch { }
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            // file reading thread is still active in the background
            if (thread != null)
            {
                MessageBoxResult result = MessageBox.Show("File operation in progress.", "File Operation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            bool? fileStatus = ofd.ShowDialog();
            if (fileStatus != true)
            {
                return;
            }

            RecordSelector.Items.Clear();

            FileReaderArgs args = new FileReaderArgs();
            args.Filename = ofd.FileName;
            cancelTokenSource = new CancellationTokenSource();
            args.CancellationToken = cancelTokenSource.Token;

            progressDialog = new ProgressDialog()
            {
                Owner = this,
                Title = $"File: {ofd.FileName}",
                Message = "Reading MARC records...",
            };

            thread = new Thread(ReadFile);
            thread.IsBackground = true;
            thread.Start(args);

            bool? dialogStatus = progressDialog.ShowDialog();
            if (dialogStatus != true)
            {
                cancelTokenSource.Cancel();
            }
        }

        private void ReadFile(object? o)
        {
            if (o is not FileReaderArgs arg)
            {
                return;
            }

            CancellationToken token = arg.CancellationToken;
            string filename = arg.Filename;
            double progress = 0.0;
            List<Record> list = new List<Record>();

            Dispatcher.Invoke(new Action<double>(progressDialog.ProgressBar_Update), DispatcherPriority.Normal, progress);
            using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    FileReader fileReader = new FileReader();
                    Stream stream = reader.BaseStream;

                    while ((!token.IsCancellationRequested) && (reader.PeekChar() >= 0))
                    {
                        Record record = fileReader.ReadRecord(reader);
                        Thread.Sleep(0);
                        list.Add(record);

                        progress = (double)stream.Position / (double)stream.Length;
                        Dispatcher.Invoke(new Action<double>(progressDialog.ProgressBar_Update), DispatcherPriority.Normal, progress);
                    }
                }
            }
            Dispatcher.Invoke(new Action<double>(progressDialog.ProgressBar_Update), DispatcherPriority.Normal, 1.0);

            Dispatcher.Invoke(new Action<IEnumerable<Record>>(RecordUpdate), list);
        }

        /* Update ListBox with new Records read in from file. */
        private void RecordUpdate(IEnumerable<Record> list)
        {
            records = list.ToList<Record>();
            progressDialog.Close();
            thread = null;

            foreach (Record r in records)
            {
                // show the title of each record in the RecordSelector
                DataField? df = r.Fields.FirstOrDefault(f => f.Tag.Equals("245")) as DataField;
                if (df == null)
                {
                    RecordSelector.Items.Add("");
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    char[] titleCodes = { 'a', 'b', 'c' };
                    foreach (Subfield sub in df.Subfields)
                    {
                        if (titleCodes.Contains(sub.Code))
                        {
                            sb.Append(sub.Data);
                        }
                    }
                    RecordSelector.Items.Add(sb.ToString());
                }
            }

            // select the first record
            RecordSelector.SelectedIndex = (RecordSelector.Items.Count > 0) ? 0 : -1;
        }

        private void RecordSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = RecordSelector.SelectedIndex;
            if (index >= 0 && index < RecordSelector.Items.Count)
            {
                Record record = records[index];
                DisplayRecord(record);
            }
            else
            {
                RecordGrid.DataContext = null;
            }
            LabelRecordSelection.Content = $"Record: {index + 1} / {RecordSelector.Items.Count}";
        }

        private void DisplayRecord(Record record)
        {
            if (record == null)
            {
                RecordGrid.ItemsSource = null;
            }
            else
            {
                RecordGrid.ItemsSource = RecordToRows(record);
            }
        }

        private IEnumerable<RecordFieldDataRow> RecordToRows(Record record)
        {
            List<RecordFieldDataRow> rows = new List<RecordFieldDataRow>();

            RecordFieldDataRow ldrRow = new RecordFieldDataRow();

            // add record Leader as a pseudo-row
            ldrRow.Tag = "LDR";
            ldrRow.Ind1 = ' ';
            ldrRow.Ind2 = ' ';
            ldrRow.Data = record.Leader;
            rows.Add(ldrRow);

            // show one Field per row
            foreach (Field field in record.Fields)
            {
                RecordFieldDataRow row = new RecordFieldDataRow();
                row.Tag = field.Tag;

                if (field is ControlField)
                {
                    row.Ind1 = ' ';
                    row.Ind2 = ' ';
                    row.Data = ((ControlField)field).Data;
                }
                if (field is DataField)
                {
                    DataField dataField = (DataField)field;
                    row.Ind1 = dataField.Indicator1;
                    row.Ind2 = dataField.Indicator2;
                    StringBuilder output = new StringBuilder();
                    
                    foreach (Subfield s in dataField.Subfields)
                    {
                        output.AppendFormat("${0}{1}", s.Code, s.Data);
                    }
                    row.Data = output.ToString();
                }

                rows.Add(row);
            }

            return rows;
        }
    }
}
