using System;
using System.IO;
using System.Threading.Tasks;

namespace DevTest
{
    internal static class Program
    {
        internal static async Task Main(string[] args)
        {
            var code = await ProcedureDefaultResponse(async () =>
            {
                await AsyncMethod();
            });
            Console.WriteLine(code);
        }

        static async Task AsyncMethod()
        {
            await Task.CompletedTask;
            throw new FileNotFoundException();
        }

        static async Task<int> ProcedureDefaultResponse(Func<Task> valueFactory)
        {
            return await ProcedureDefaultResponseIfError(async () =>
            {
                await valueFactory();
                return 200;
            });
        }

        static async Task<int> ProcedureDefaultResponseIfError(Func<Task<int>> valueFactory)
        {
            try
            {
                return await valueFactory();
            }
            catch (ArrayTypeMismatchException)
            {
                Console.WriteLine("Handled 2");
                return 500;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Handled 2");
                return 404;
            }
        }
    }
}