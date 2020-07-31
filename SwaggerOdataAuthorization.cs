using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using PsefApi.Misc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace PsefApi
{
    /// <summary>
    /// Represents the Swagger/Swashbuckle operation filter used to document the authorization requirement.
    /// </summary>
    public class SwaggerOdataAuthorization : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            IEnumerable<AuthorizeAttribute> authAttributes = context
                .MethodInfo
                .DeclaringType
                .GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AuthorizeAttribute>();

            if (authAttributes.Any())
            {
                operation.Security = ApiHelper.Requirements;
            }
        }
    }
}