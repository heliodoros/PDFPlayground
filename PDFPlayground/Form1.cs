using Aspose.Pdf;
using Aspose.Pdf.Optimization;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFPlayground
{
    public partial class Form1 : Form
    {
        private string fileName;
        private string outputDirectory;

        public Form1()
        {
            InitializeComponent();
            Aspose.Pdf.License license1 = new Aspose.Pdf.License();
            license1.SetLicense("C:\\Project_Horizon\\Files\\Application\\Aspose\\Aspose.Total.NET.lic");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            openFileDialog1.AddExtension = true;
            openFileDialog1.DefaultExt = "pdf";
            openFileDialog1.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog1.FileName;
                label1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                outputDirectory = folderBrowserDialog1.SelectedPath;
                label2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(outputDirectory))
            {
                MessageBox.Show("Please select both input PDF file and output directory.");
                return;
            }

            //DialogResult dialogResult = MessageBox.Show("Begin operation?", "Confirmation", MessageBoxButtons.YesNo);
            //if (dialogResult != DialogResult.Yes)
            //    return;

            try
            {
                progressBar1.ForeColor = System.Drawing.Color.BlanchedAlmond;
                progressBar1.Style = ProgressBarStyle.Continuous;
                
                Document pdfDocument = new Document(fileName);
                double maxSizeInKB = 14500;
                int totalPages = pdfDocument.Pages.Count;
                Debug.WriteLine(fileName);
                progressBar1.Minimum = 0;
                progressBar1.Maximum = totalPages;
                progressBar1.Value = totalPages;
                statusLabel.Text = "Compressing...";
                label4.Text = $"Total pages: {totalPages}";

                var optimizeOptions = new Aspose.Pdf.Optimization.OptimizationOptions();
                optimizeOptions.LinkDuplcateStreams = true;
                optimizeOptions.RemoveUnusedObjects = true;
                //optimizeOptions.RemoveUnusedStreams = true; <- performance drop, no benefit
                optimizeOptions.AllowReusePageContent = true;
                optimizeOptions.UnembedFonts = true;
                //optimizeOptions.SubsetFonts = true; <- extreme performance drop, no benefit
                //optimizeOptions.RemovePrivateInfo = true;
                //optimizeOptions.ImageCompressionOptions.CompressImages = true;
                //optimizeOptions.ImageCompressionOptions.ImageQuality = 50;
                //optimizeOptions.ImageCompressionOptions.ResizeImages = true;
                //optimizeOptions.ImageCompressionOptions.MaxResolution = 100;
                Console.WriteLine(pdfDocument.FileName);
                pdfDocument.OptimizeResources(optimizeOptions);
                //pdfDocument.Save(Path.Combine(outputDirectory, $"compressed_output.pdf"));
                GC.Collect();
                int splitIndex = 1;
                int pageCount = 0;
                Document currentPdfDocument = new Document();
                string directoryPath = $"{outputDirectory}\\{Path.GetFileNameWithoutExtension(fileName)}";

                // Check if the directory doesn't exist
                if (!Directory.Exists(directoryPath))
                {
                    // Create the directory
                    Directory.CreateDirectory(directoryPath);
                }

                foreach (Page pdfPage in pdfDocument.Pages)
                {
                    currentPdfDocument.Pages.Add(pdfPage);

                    pageCount++;
                    if (pageCount % 100 == 0 || pageCount == totalPages)
                    {
                        progressBar1.Invoke((Action)(() => progressBar1.Value = pageCount));
                        statusLabel.Invoke((Action)(() => statusLabel.Text = pageCount.ToString()));

                        // Calculate the size of the current document
                        using (MemoryStream ms = new MemoryStream())
                        {
                            currentPdfDocument.Save(ms);
                            long currentSizeInBytes = ms.Length;

                            long currentSizeInKB = currentSizeInBytes / 1000;

                            if (currentSizeInKB > maxSizeInKB || pageCount == totalPages)
                            {
                                string outputPath = Path.Combine(directoryPath, $"{splitIndex}_{Path.GetFileName(fileName)}");
                                currentPdfDocument.Save(outputPath);
                                splitIndex++;
                                currentPdfDocument = new Document();
                            }
                        }
                    }
                }
                //progressBar1.BackColor = System.Drawing.Color.Blue;
                //progressBar1.ForeColor = System.Drawing.Color.BlanchedAlmond;
                statusLabel.Text = "Completed";
                //play windows info sound
                System.Media.SystemSounds.Asterisk.Play();
                //MessageBox.Show("PDF splitting completed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                progressBar1.BackColor = System.Drawing.Color.Red;
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
