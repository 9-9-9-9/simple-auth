using System.Collections.Generic;
using System.IO;

namespace ConsoleApps.Shared.Utils
{
    public static class FileUtil
    {
        public static IEnumerable<TModel> ReadTsvToModel<TModel>(string file, IModelConverter<TModel> converter)
        {
            foreach (var line in File.ReadLines(file))
                yield return converter.Convert(line.Split('\t'));
        }
        
        public interface IModelConverter<out TModel>
        {
            TModel Convert(string[] parts);
        }
    }
}