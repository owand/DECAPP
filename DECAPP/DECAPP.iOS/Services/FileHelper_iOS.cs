using DECAPP.iOS.Services;
using DECAPP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileHelper_iOS))]

namespace DECAPP.iOS.Services
{
    public class FileHelper_iOS : IFileHelper
    {
        // проверка существования файла
        [SecurityCritical]
        public Task<bool> ExistsAsync(string filename)
        {
            bool exists;
            try
            {
                string filepath = GetFilePath(filename); // получаем путь к файлу
                exists = File.Exists(filepath); // существует ли файл?
            }
            catch (Exception)
            {
                exists = false;
            }
            return Task<bool>.FromResult(exists);
        }

        // сохранение текста в файл
        public async Task SaveTextAsync(string filename, string text)
        {
            string filepath = GetFilePath(filename);
            using (StreamWriter writer = File.CreateText(filepath))
            {
                await writer.WriteAsync(text);
            }
        }

        // загрузка текста из файла
        public async Task<string> LoadTextAsync(string filename)
        {
            string filepath = GetFilePath(filename);
            using StreamReader reader = File.OpenText(filepath);
            return await reader.ReadToEndAsync();
        }

        // получение файлов из определнного каталога
        [SecurityCritical]
        public Task<IEnumerable<string>> GetFilesAsync()
        {
            IEnumerable<string> filenames = from filepath in Directory.EnumerateFiles(GetPath())
                                            select Path.GetFileName(filepath);
            return Task<IEnumerable<string>>.FromResult(filenames);
        }

        // удаление файла
        [SecurityCritical]
        public Task DeleteAsync(string filename)
        {
            File.Delete(GetFilePath(filename));
            return Task.FromResult(true);
        }

        //// Get a file and return a Stream Write
        //public Task<Stream> GetStreamWriteAsync(string filename)
        //{
        //    Stream stream = null;
        //    try
        //    {
        //        string personalFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        //        string libraryFolder = Path.Combine(personalFolder, "..", "Library");
        //        var file = Path.Combine(libraryFolder, filename);
        //        stream = File.OpenWrite(file);
        //    }
        //    catch (Exception)
        //    {
        //        stream = null;
        //        return Task.FromResult(stream);
        //    }
        //    return Task.FromResult(stream);
        //}

        //// Get a file and return a Stream Read
        //public Task<Stream> GetStreamReadAsync(string filename)
        //{
        //    Stream stream = null;
        //    try
        //    {
        //        string personalFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        //        string libraryFolder = Path.Combine(personalFolder, "..", "Library");
        //        var file = Path.Combine(libraryFolder, filename);
        //        stream = File.OpenRead(file);
        //    }
        //    catch (Exception)
        //    {
        //        stream = null;
        //        return Task.FromResult(stream);
        //    }
        //    return Task.FromResult(stream);
        //}

        // вспомогательный метод для построения пути к файлу
        [SecurityCritical]
        public string GetFilePath(string filename)
        {
            return Path.Combine(GetPath(), filename);
        }

        // получаем путь к папке MyDocuments
        [SecurityCritical]
        public string GetPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }
}