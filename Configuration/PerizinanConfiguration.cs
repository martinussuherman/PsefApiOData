using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using PsefApiOData.Controllers;
using PsefApiOData.Models;

namespace PsefApiOData.Configuration
{
    /// <summary>
    /// Represents the model configuration for Perizinan.
    /// </summary>
    public class PerizinanConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
        {
            EntityTypeConfiguration<Perizinan> perizinan = builder
                .EntitySet<Perizinan>(nameof(Perizinan))
                .EntityType;

            perizinan.HasKey(p => p.Id);
            perizinan
                .Expand(SelectExpandType.Disabled)
                .Filter()
                .OrderBy()
                .Page(50, 50)
                .Select();
        }
    }
}