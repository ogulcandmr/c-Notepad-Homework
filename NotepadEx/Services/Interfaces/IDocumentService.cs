using NotepadEx.MVVM.Models;
using System.Threading.Tasks;

namespace NotepadEx.Services.Interfaces
{
    public interface IDocumentService
    {
        Task LoadDocumentAsync(string filePath, Document document);
        Task SaveDocumentAsync(Document document);
        void PrintDocument(Document document);
    }
}