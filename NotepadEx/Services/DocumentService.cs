using System.IO;
using System.Threading.Tasks;
using NotepadEx.MVVM.Models;
using NotepadEx.Services.Interfaces;
using System.Drawing.Printing;

namespace NotepadEx.Services
{
    public class DocumentService : IDocumentService
    {
        public async Task LoadDocumentAsync(string filePath, Document document)
        {
            if(!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            document.Content = await File.ReadAllTextAsync(filePath);
            document.FilePath = filePath;
            document.IsModified = false;
        }

        public async Task SaveDocumentAsync(Document document)
        {
            await File.WriteAllTextAsync(document.FilePath, document.Content);
            document.IsModified = false;
        }

        public void PrintDocument(Document document)
        {
            // Note: This is still synchronous. For very large documents,
            // this could also be a performance bottleneck.
            var printDialog = new System.Windows.Forms.PrintDialog();
            if(printDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var printDoc = new PrintDocument();
                printDoc.PrintPage += (sender, e) =>
                {
                    e.Graphics.DrawString(document.Content,
                        new System.Drawing.Font("Arial", 12),
                        System.Drawing.Brushes.Black,
                        new System.Drawing.RectangleF(100, 100, 700, 1000));
                };
                printDoc.Print();
            }
        }
    }
}