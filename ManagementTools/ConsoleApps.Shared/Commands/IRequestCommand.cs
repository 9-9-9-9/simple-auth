using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApps.Shared.Commands
{
    public interface ICommand
    {
    }

    public interface IRequestCommand : ICommand
    {
        Task Request(params string[] args);
    }

    public interface IRequestCommand<TResponse> : ICommand
    {
        Task<TResponse> Request(params string[] args);
    }

    public abstract class AbstractRequestCommand : ICommand
    {
        protected virtual IEnumerable<string> GetArgumentsProblems(params string[] args)
        {
            if (NumberOfParameters != args.Length)
                yield return $"Require {args.Length} arguments";

            foreach (var idx in IdxParametersCanNotBeBlank)
            {
                if (idx >= args.Length)
                    yield return $"Param idx {idx} not exists";
                else if (string.IsNullOrWhiteSpace(args[idx]))
                    yield return $"Param idx {idx} can not be blank";
            }

            foreach (var arg in GetOthersArgumentsProblems(args))
                yield return arg;
        }

        protected abstract IEnumerable<string> GetOthersArgumentsProblems(params string[] args);

        protected abstract int NumberOfParameters { get; }
        protected abstract int[] IdxParametersCanNotBeBlank { get; }

    }

    public abstract class RequestCommandWithoutResponse : AbstractRequestCommand, IRequestCommand
    {
        public async Task Request(params string[] args)
        {
            var argumentProblems = GetArgumentsProblems(args).ToList();
            if (argumentProblems.Any())
                throw new AggregateException(argumentProblems.Select(err => new ArgumentException(err)));
            await DoRequest(args);
        }

        protected abstract Task DoRequest(params string[] args);
    }

    public abstract class RequestCommandWithResponse<TResponse> : AbstractRequestCommand, IRequestCommand<TResponse>
    {
        public async Task<TResponse> Request(params string[] args)
        {
            var argumentProblems = GetArgumentsProblems(args).ToList();
            if (argumentProblems.Any())
                throw new AggregateException(argumentProblems.Select(err => new ArgumentException(err)));
            return await DoRequest(args);
        }

        protected abstract Task<TResponse> DoRequest(params string[] args);
    }

    public static class CommandExtensions
    {
        public static IServiceCollection RegisterCommandWOR<TClass>(this IServiceCollection services)
            where TClass : class, ICommand, IRequestCommand
        {
            return services
                .AddSingleton<ICommand, TClass>()
                .AddSingleton<IRequestCommand, TClass>();
        }

        public static IServiceCollection RegisterCommandWR<TClass, TResponse>(this IServiceCollection services)
            where TClass : class, ICommand, IRequestCommand<TResponse>
        {
            return services
                .AddSingleton<ICommand, TClass>()
                .AddSingleton<IRequestCommand<TResponse>, TClass>();
        }
    }
}