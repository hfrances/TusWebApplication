using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TusWebApplication.Controllers.Base
{

    [ApiController, Route("api/[controller]")]
    public abstract class ApiControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {

        private IMediator? _mediator;
        private ILogger? _logger;

        protected IMediator Mediator
            => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        protected ILogger Logger
            => _logger ??= (ILogger)HttpContext.RequestServices.GetRequiredService(typeof(ILogger<>).MakeGenericType(this.GetType()));

        /// <summary>
        /// Redirects to <see cref="Mediator.Send"/>. Handles localization of <see cref="AppException"/>.
        /// </summary>
        protected virtual async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Mediator.Send(request, cancellationToken);
            }
            catch (qckdev.AspNetCore.HttpHandledException)
            {
                throw;
            }
            catch (Exception ex)
            {
#if NET5_0_OR_GREATER
                throw new BadHttpRequestException(ex.Message, ex);
#else
                throw new System.Net.Http.HttpRequestException(ex.Message, ex);
#endif
            }
        }
        
    }
}
