using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using PsefApi.Controllers;
using PsefApi.Models;

namespace PsefApi.Configuration
{
    /// <summary>
    /// Represents the model configuration for Kabupaten/Kota.
    /// </summary>
    public class KabKotaConfiguration : IModelConfiguration
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

            EntityTypeConfiguration<KabKota> kabKota = builder
                .EntitySet<KabKota>(nameof(KabKota))
                .EntityType;

            kabKota.Collection
                .Function(nameof(KabKotaController.TotalCount))
                .Returns(typeof(int));

            kabKota.HasKey(p => p.Id);
            kabKota
                .Expand(SelectExpandType.Disabled)
                .Filter()
                .OrderBy()
                .Select();
        }
    }
}