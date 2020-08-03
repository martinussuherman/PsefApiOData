using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using PsefApi.Misc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Microsoft.AspNetCore.Mvc.CompatibilityVersion;
using static Microsoft.OData.ODataUrlKeyDelimiter;

namespace PsefApi
{
    /// <summary>
    /// Represents the startup process for the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Application startup.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Application configuration.`
        /// </summary>
        /// <value>Application configuration.</value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures services for the application.
        /// </summary>
        /// <param name="services">The collection of services to configure the application with.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // the sample application always uses the latest version, but you may want an explicit version such as Version_2_2
            // note: Endpoint Routing is enabled by default; however, it is unsupported by OData and MUST be false
            services
                .AddMvc(options => options.EnableEndpointRouting = false)
                .SetCompatibilityVersion(Latest);

            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;

                // note: this is optional, but it will take away versioning by query string
                // the default behavior is a composite reader
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            ApiHelper.ReadConfiguration(Configuration);
            ApiHelper.InitializeRequirements();
            ConfigureOData(services);
            ConfigureSwaggerGen(services);

            services.AddHttpClient<IApiDelegateService, ApiDelegateService>();
            services.AddHttpClient<IIdentityApiService, IdentityApiService>();

            services.AddDbContextPool<Models.PsefMySqlContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("MySql"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            10,
                            System.TimeSpan.FromSeconds(30),
                            null);
                    }),
                16);

            // https://identityserver4.readthedocs.io/en/latest/topics/apis.html
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // base-address of your identityserver
                    options.Authority = ApiHelper.Authority;

                    // if you are using API resources, you can specify the name here
                    options.Audience = ApiHelper.Audience;
                });
        }

        /// <summary>
        /// Configures the application using the provided builder, hosting environment, and logging factory.
        /// </summary>
        /// <param name="app">The current application builder.</param>
        /// <param name="modelBuilder">The <see cref="VersionedODataModelBuilder">model builder</see> used to create OData entity data models (EDMs).</param>
        /// <param name="provider">The API version descriptor provider used to enumerate defined API versions.</param>
        public void Configure(
            IApplicationBuilder app,
            VersionedODataModelBuilder modelBuilder,
            IApiVersionDescriptionProvider provider)
        {
            string basePath = Configuration.GetValue<string>("BasePath");

            app
                .UsePathBase(basePath)
                .UseHttpsRedirection()
                .UseForwardedHeaders(new ForwardedHeadersOptions()
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                })
                .UseAuthentication()
                .UseAuthorization()
                .UseMvc(routeBuilder =>
                {
                    // global odata query options
                    routeBuilder.Count();
                    // the following will not work as expected
                    // BUG: https://github.com/OData/WebApi/issues/1837
                    // routeBuilder.SetDefaultODataOptions( new ODataOptions() { UrlKeyDelimiter = Parentheses } );
                    routeBuilder
                        .ServiceProvider
                        .GetRequiredService<ODataOptions>()
                        .UrlKeyDelimiter = Parentheses;

                    // register routes with and without the api version constraint
                    var models = modelBuilder.GetEdmModels();
                    routeBuilder.MapVersionedODataRoutes(
                        "explicit",
                        "api/v{version:apiVersion}",
                        models);
                    // routeBuilder.MapVersionedODataRoutes(
                    //     "implicit",
                    //     "api",
                    //     models);
                })
                .UseSwagger()
                .UseSwaggerUI(options =>
                {
                    options.OAuthClientId(Configuration.GetValue<string>("ClientId"));
                    options.OAuthAppName("PsefApiOdata Swagger");

                    // build a swagger endpoint for each discovered API version
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint(
                            $"{basePath}/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }
                });
        }

        private void ConfigureSwaggerGen(IServiceCollection services)
        {
            var authUrl = new Uri($"{ApiHelper.Authority}/connect/authorize");
            var implicitFlow = new OpenApiOAuthFlow
            {
                AuthorizationUrl = authUrl,
                Scopes = new Dictionary<string, string>
                {
                    { ApiHelper.Audience, "Api access" }
                }
            };

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            services.AddSwaggerGen(options =>
            {
                // add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();
                options.OperationFilter<SwaggerOdataAuthorization>();

                // integrate xml comments
                options.IncludeXmlComments(XmlCommentsFilePath);

                // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
                options.AddSecurityDefinition(
                    ApiInfo.SchemeOauth2,
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = implicitFlow
                        }
                    });
            });
        }

        private void ConfigureOData(IServiceCollection services)
        {
            services.AddOData().EnableApiVersioning();

            services.AddODataApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;

                // configure query options (which cannot otherwise be configured by OData conventions)
                // options.QueryOptions.Controller<V2.PeopleController>()
                //                     .Action( c => c.Get( default ) )
                //                         .Allow( Skip | Count )
                //                         .AllowTop( 100 )
                //                         .AllowOrderBy( "firstName", "lastName" );

                // options.QueryOptions.Controller<V3.PeopleController>()
                //                     .Action( c => c.Get( default ) )
                //                         .Allow( Skip | Count )
                //                         .AllowTop( 100 )
                //                         .AllowOrderBy( "firstName", "lastName" );
            });
        }

        private static string XmlCommentsFilePath
        {
            get
            {
                return Path.Combine(
                    PlatformServices.Default.Application.ApplicationBasePath,
                    typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml");
            }
        }
    }
}