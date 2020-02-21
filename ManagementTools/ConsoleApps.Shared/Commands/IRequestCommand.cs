using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Shared.Extensions;

namespace ConsoleApps.Shared.Commands
{
    public interface ICommand
    {
        Task Process(params string[] args);
        string[] GetParametersName();
    }

    public abstract class AbstractCommand : ICommand
    {
        public virtual Task Process(params string[] args)
        {
            var argumentProblems = GetArgumentsProblems(args).ToList();
            if (argumentProblems.Any())
                throw new AggregateException(argumentProblems.Select(err => new ArgumentException(err)));

            return DoMainJob(args);
        }

        protected abstract Task DoMainJob(string[] args);

        public abstract string[] GetParametersName();

        protected virtual IEnumerable<string> GetArgumentsProblems(params string[] args)
        {
            if (GetParametersName().Length != args.Length)
                yield return $"Require {args.Length} arguments";

            foreach (var idx in IdxParametersCanNotBeBlank)
                if (idx >= args.Length)
                    yield return $"Param idx {idx} not exists";
                else if (string.IsNullOrWhiteSpace(args[idx]))
                    yield return $"Param idx {idx} can not be blank";

            foreach (var arg in GetOthersArgumentsProblems(args))
                yield return arg;
        }

        // Do not make this method default with virtual here, developer will forgot additional important check
        protected abstract IEnumerable<string> GetOthersArgumentsProblems(params string[] args);

        protected abstract int[] IdxParametersCanNotBeBlank { get; }

        protected Task Print(Task<string> valueFactory)
        {
            return valueFactory.ContinueWith(x => Print(x.Result));
        }
        
        protected void Print(string message)
        {
            message.Write();
        }

        protected bool? ParseBoolean(string input)
        {
            switch (input?.Trim().ToLowerInvariant())
            {
                case "y":
                case "yes":
                case "true":
                case "ok":
                case "1":
                    return true;
                case "n":
                case "no":
                case "false":
                case "0":
                    return false;
                default:
                    return null;
            }
        }

        protected bool IsCorrectBool(string input)
        {
            return ParseBoolean(input).HasValue;
        }
    }

    public static class CommandExtensions
    {
        public static IServiceCollection RegisterCommand<TClass>(this IServiceCollection services)
            where TClass : class, ICommand
        {
            return services.AddSingleton<ICommand, TClass>();
        }
    }
}