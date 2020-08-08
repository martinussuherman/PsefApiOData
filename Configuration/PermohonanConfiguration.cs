using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;
using PsefApiOData.Models;
using PsefApiOData.Controllers;

namespace PsefApiOData.Configuration
{
    /// <summary>
    /// Represents the model configuration for Permohonan.
    /// </summary>
    public class PermohonanConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
        {
            EntityTypeConfiguration<Permohonan> permohonan = builder
                .EntitySet<Permohonan>(nameof(Permohonan))
                .EntityType;

            permohonan.Collection
                .Function(ApiInfo.CurrentUser)
                .ReturnsFromEntitySet<Permohonan>(nameof(Permohonan));
            permohonan.Collection
                .Function(nameof(PermohonanController.TotalCount))
                .Returns<long>();

            permohonan.HasKey(p => p.Id);
            permohonan
                .Filter()
                .OrderBy()
                .Page(50, 50)
                .Select();
        }
    }
}