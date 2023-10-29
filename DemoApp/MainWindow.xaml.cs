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
        }

        private Thread? thread = null;
        private CancellationTokenSource cancelTokenSource;
        private List<Record> records = new List<Record>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            cancelTokenSource?.Cancel();
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
            if (fileStatus == true)
            {
                FileReaderArgs args = new FileReaderArgs();
                args.Filename = ofd.FileName;
                cancelTokenSource = new CancellationTokenSource();
                args.CancellationToken = cancelTokenSource.Token;

                thread = new Thread(OpenFile);
                thread.IsBackground = true;
                thread.Start(args);

                AbortButton.IsEnabled = true;
            }
        }

        private void FileAbort_Click(object sender, RoutedEventArgs e)
        {
            cancelTokenSource?.Cancel();
        }

        private void OpenFile(object o)
        {
            if (!(o is FileReaderArgs arg))
            {
                return;
            }
            string filename = arg.Filename;
            CancellationToken token = arg.CancellationToken;

            List<Record> list = new List<Record>();

            Dispatcher.Invoke(new Action<double>(ProgressBar_Update), DispatcherPriority.Normal, 0.0);

            using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    FileReader fileReader = new FileReader();
                    Stream stream = reader.BaseStream;
                    double progress = 0.0;

                    while ((!token.IsCancellationRequested) && (reader.PeekChar() >= 0))
                    {
                        Record record = fileReader.ReadRecord(reader);
                        Thread.Sleep(0);
                        list.Add(record);

                        progress = (double)stream.Position / (double)stream.Length;
                        Dispatcher.Invoke(new Action<double>(ProgressBar_Update), DispatcherPriority.Normal, progress);
                    }
                }
            }
            Dispatcher.Invoke(new Action<double>(ProgressBar_Update), DispatcherPriority.Normal, 1.0);

            Dispatcher.Invoke(new Action<IEnumerable<Record>>(RecordUpdate), list);
        }

        private void ProgressBar_Update(double progress)
        {
            ProgressBar.Value = progress;
        }

        /* Update ListBox with new Records read in from file. */
        private void RecordUpdate(IEnumerable<Record> list)
        {
            records = list.ToList<Record>();
            thread = null;
            AbortButton.IsEnabled = false;
            cancelTokenSource.Dispose();
            cancelTokenSource = null;

            ProgressBar.Value = 0;
            RecordSelector.Items.Clear();
            foreach (Record r in records)
            {
                // show the title of each record in the RecordSelector
                string title = "";
                DataField? df = (DataField)r.Fields.FirstOrDefault(f => f.Tag == "245");
                if (df != null)
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
                    title = sb.ToString();
                }
                RecordSelector.Items.Add(title);
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
                ViewRecord(record);
            }
            else
            {
                RecordGrid.DataContext = null;
            }
            LabelRecordSelection.Content = $"Record: {index + 1} / {RecordSelector.Items.Count}";
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

        private void ViewRecord(Record record)
        {
            if (record == null)
            {
                RecordGrid.ItemsSource = null;
            }

            RecordGrid.ItemsSource = RecordToRows(record);
        }
    }
}
