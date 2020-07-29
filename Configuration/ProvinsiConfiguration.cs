using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using PsefApi.Models;

namespace PsefApi.Configuration
{
    /// <summary>
    /// Represents the model configuration for Provinsi.
    /// </summary>
    public class ProvinsiConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
        {
            if (apiVersion < ApiInfo.Ver1_0)
            {
                return;
            }

            EntityTypeConfiguration<Provinsi> provinsi = builder
                .EntitySet<Provinsi>(nameof(Provinsi))
                .EntityType;

            provinsi.HasKey(p => p.Id);
            provinsi
                .Filter(
                    QueryOptionSetting.Allowed,
                    new string[]
                    {
                        nameof(Provinsi.Name).ToLowerInvariant()
                    })
                .Expand(SelectExpandType.Disabled)
                .Select()
                .OrderBy();
        }
    }
}