using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;
using PsefApi.Models;
using PsefApi.Controllers;

namespace PsefApi.Configuration
{
    /// <summary>
    /// Represents the model configuration for Pemohon.
    /// </summary>
    public class PemohonConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
        {
            EntityTypeConfiguration<Pemohon> pemohon = builder
                .EntitySet<Pemohon>(nameof(Pemohon))
                .EntityType;

            pemohon.Collection
                .Function(ApiInfo.CurrentUser)
                .ReturnsFromEntitySet<Pemohon>(nameof(Pemohon));
            pemohon.Collection
                .Function(nameof(PemohonController.TotalCount))
                .Returns(typeof(long));

            pemohon.HasKey(p => p.Id);
            pemohon
                .Filter()
                .Select()
                .OrderBy()
                .Expand();
        }
    }
}