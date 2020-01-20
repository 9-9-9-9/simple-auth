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
using SimpleAuth.Shared.Enums;
using SimpleAuth.Shared.Exceptions;
using SimpleAuth.Shared.Models;

namespace SimpleAuth.Server.Controllers
{
    /// <summary>
    /// Base controller, provide utilities for inherited controllers
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// To be used to resolve services
        /// </summary>
        protected IServiceProvider ServiceProvider;

        /// <summary>
        /// DI constructor
        /// </summary>
        protected BaseController(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        private RequestAppHeaders _requestAppHeaders;

        /// <summary>
        /// Decrypted content from x-app-token, this store details about Corp/App which requester is trying to interact with
        /// </summary>
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

        /// <summary>
        /// Decrypted content from x-corp-token, this store details about Corp which requester is trying to interact with
        /// </summary>
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

        /// <summary>
        /// Get a request header
        /// </summary>
        /// <param name="key">Name of the header</param>
        protected string GetHeader(string key)
        {
            var stringValues = HttpContext.Request.Headers[key];
            if (stringValues.IsEmpty())
                return null;
            return stringValues.FirstOrDefault(x => !x.IsBlank());
        }

        /// <summary>
        /// Procedure an Forbidden result, to be responded to client to notice that there is a mismatch between requested Corp/App resource and x-app-token/x-corp-token provided
        /// </summary>
        /// <returns>403 Forbidden</returns>
        protected IActionResult CrossAppToken()
        {
            return StatusCodes.Status403Forbidden.WithMessage($"Cross app token by {Constants.Headers.AppPermission}");
        }

        /// <summary>
        /// Push a header to response headers collection, indicate this is a response for a search action and provide information about how many the number of results found
        /// </summary>
        /// <param name="size">Size of result</param>
        /// <param name="collectionNo">In case multiple search, use this to indicate identity of the search</param>
        protected void PushHeaderSize(int size, int collectionNo = 1)
        {
            Response.Headers.Add($"CSize{collectionNo}", size.ToString());
        }

        /// <summary>
        /// Think this is a try/catch, which response http status code based on Exception if any, default response if no error is 200 OK
        /// </summary>
        /// <param name="valueFactory">Action to be executed</param>
        /// <returns>Http status code 200 if execute success, others http status code if exception occured</returns>
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
                    return StatusCodes.Status404NotFound.WithEmpty();

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

                if (!response.Any())
                    return StatusCodes.Status404NotFound.WithEmpty();

                return StatusCodes.Status200OK.WithJson(response);
            });
        }

        protected async Task<ResponseUserModel> GetBaseResponseUserModelAsync(string userId, IUserService userService)
        {
            var user = userService.GetUser(userId, RequestAppHeaders.Corp);
            if (user == default)
                throw new EntityNotExistsException(userId);

            var filterRoleEnv = GetHeader(Constants.Headers.FilterByEnv);
            var filterRoleTenant = GetHeader(Constants.Headers.FilterByTenant);

            var activeRoles = await userService.GetActiveRolesAsync(userId, RequestAppHeaders.Corp,
                RequestAppHeaders.App,
                filterRoleEnv, filterRoleTenant);
            
            return new ResponseUserModel
            {
                Id = userId,
                Corp = RequestAppHeaders.Corp,
                ActiveRoles = activeRoles.OrEmpty().Select(x => new RoleModel
                {
                    Role = x.RoleId,
                    Permission = x.Permission.Serialize()
                }).ToArray()
            };
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