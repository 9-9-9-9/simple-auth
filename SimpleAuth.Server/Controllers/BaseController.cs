using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SimpleAuth.Core.Extensions;
using SimpleAuth.Repositories;
using SimpleAuth.Server.Extensions;
using SimpleAuth.Server.Models;
using SimpleAuth.Services;
using SimpleAuth.Services.Entities;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Exceptions;

namespace SimpleAuth.Server.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected IServiceProvider ServiceProvider;

        protected BaseController(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        private RequestAppHeaders _requestAppHeaders;

        protected RequestAppHeaders RequestAppHeaders
        {
            get
            {
                if (_requestAppHeaders == null)
                {
                    var requestAppToken = HttpContext.Items[Constants.Headers.AppPermission];
                    if (requestAppToken is RequestAppHeaders rah)
                        _requestAppHeaders = rah;
                }

                return _requestAppHeaders;
            }
        }

        private RequireCorpToken _requireCorpToken;

        protected RequireCorpToken RequireCorpToken
        {
            get
            {
                if (_requireCorpToken == null)
                {
                    var requestCorpToken = HttpContext.Items[Constants.Headers.CorpPermission];
                    if (requestCorpToken is RequireCorpToken rct)
                        _requireCorpToken = rct;
                }

                return _requireCorpToken;
            }
        }

        protected string GetHeader(string key)
        {
            var stringValues = HttpContext.Request.Headers[key];
            if (stringValues.IsEmpty())
                return null;
            return stringValues.FirstOrDefault(x => !x.IsBlank());
        }

        protected IActionResult CrossAppToken()
        {
            return StatusCodes.Status403Forbidden.WithMessage($"Cross app token by {Constants.Headers.AppPermission}");
        }

        protected void PushHeaderSize(int size, int collectionNo = 1)
        {
            Response.Headers.Add($"CSize{collectionNo}", size.ToString());
        }

        protected async Task<IActionResult> ProcedureDefaultResponseIfError(Func<Task<IActionResult>> valueFactory)
        {
            try
            {
                return await valueFactory();
            }
            catch (Exception e)
            {
                return DefaultExceptionHandler(e);
            }
        }

        protected IActionResult ProcedureDefaultResponseIfError(Func<IActionResult> valueFactory)
        {
            try
            {
                return valueFactory();
            }
            catch (Exception e)
            {
                return DefaultExceptionHandler(e);
            }
        }

        protected IActionResult DefaultExceptionHandler(Exception ex)
        {
            if (ex is EntityAlreadyExistsException || ex is ConstraintViolationException)
            {
                return StatusCodes.Status409Conflict.WithMessage(ex.Message);
            }

            if (ex is EntityNotExistsException)
            {
                return StatusCodes.Status404NotFound.WithMessage(ex.Message);
            }

            if (ex is AccessLockedEntityException)
            {
                return StatusCodes.Status423Locked.WithMessage(ex.Message);
            }

            if (ex is DataVerificationMismatchException)
            {
                return StatusCodes.Status406NotAcceptable.WithMessage(ex.Message);
            }

            if (ex is SimpleAuthSecurityException)
            {
                return StatusCodes.Status403Forbidden.WithMessage(ex.Message);
            }

            if (ex is ValidationException)
            {
                return StatusCodes.Status400BadRequest.WithMessage(ex.Message);
            }

            if (ex is ConcurrentUpdateException)
            {
                return StatusCodes.Status422UnprocessableEntity.WithMessage(ex.Message);
            }

            if (ex is SimpleAuthException)
            {
                return StatusCodes.Status500InternalServerError.WithMessage(ex.Message);
            }

            throw ex;
        }

        protected async Task<IActionResult> ProcedureResponseForPersistAction(Func<Task> actionFactory)
        {
            return await ProcedureDefaultResponseIfError(async () =>
            {
                await actionFactory();
                return StatusCodes.Status201Created.WithEmpty();
            });
        }

        protected async Task<IActionResult> ProcedureDefaultResponse(Func<Task> actionFactory)
        {
            return await ProcedureDefaultResponseIfError(async () =>
            {
                await actionFactory();
                return StatusCodes.Status200OK.WithEmpty();
            });
        }

        protected async Task<IActionResult> ProcedureResponseForArrayLookUp<T>(
            Func<Task<IEnumerable<T>>> lookupActionFactory)
        {
            return await ProcedureDefaultResponseIfError(async () =>
            {
                var response = (await lookupActionFactory()).OrEmpty().ToArray();
                PushHeaderSize(response.Length);

                if (!response.Any())
                    return StatusCodes.Status204NoContent.WithEmpty();

                return StatusCodes.Status200OK.WithJson(response);
            });
        }

        protected async Task<IActionResult> ProcedureResponseForLookUp<T>(Func<Task<T>> lookupActionFactory)
        {
            return await ProcedureDefaultResponseIfError(async () =>
            {
                var response = await lookupActionFactory();

                if (response == null)
                    return NotFound();

                return StatusCodes.Status200OK.WithJson(response);
            });
        }

        protected async Task<IActionResult> ProcedureResponseForLookUpArrayUsingTerm<T>(
            string term, int? skip, int? take,
            Func<FindOptions, Task<IEnumerable<T>>> lookupActionFactory)
        {
            return await ProcedureDefaultResponseIfError(async () =>
            {
                if ((term?.Length ?? 0) < Constants.Length.MinTerm)
                    return BadRequest(nameof(term));

                if (skip.HasValue && skip < 0)
                    return BadRequest(nameof(skip));

                if (take.HasValue && take < 0)
                    return BadRequest(nameof(take));

                if (take.HasValue && take > Constants.Length.MaxSearchResults)
                    take = Constants.Length.MaxSearchResults;

                var response = (await lookupActionFactory(new FindOptions
                {
                    Skip = skip ?? 0,
                    Take = take ?? 0,
                })).OrEmpty().ToArray();

                PushHeaderSize(response.Length);

                return (
                    response.Any()
                        ? StatusCodes.Status200OK
                        : StatusCodes.Status204NoContent
                ).WithJson(response);
            });
        }
    }

    public abstract class BaseController<TService, TRepo, TEntity> : BaseController
        where TService : IDomainService
        where TRepo : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected readonly TService Service;
        protected readonly TRepo Repository;

        protected BaseController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Repository = serviceProvider.GetRequiredService<TRepo>();
            Service = serviceProvider.GetRequiredService<TService>();
        }
    }
}