using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;
using PsefApiOData.Models;
using PsefApiOData.Controllers;
using System;

namespace PsefApiOData.Configuration
{
    /// <summary>
    /// Represents the model configuration for Dashboard Info.
    /// </summary>
    public class DashboardInfoConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
        {
            ComplexTypeConfiguration<DashboardInfo> info = builder
                .ComplexType<DashboardInfo>();

            builder.Function(nameof(DashboardInfoController.Verifikator))
                .Returns<DashboardInfo>();
            builder.Function(nameof(DashboardInfoController.KepalaSeksi))
                .Returns<DashboardInfo>();
            builder.Function(nameof(DashboardInfoController.KepalaSubDirektorat))
                .Returns<DashboardInfo>();
            builder.Function(nameof(DashboardInfoController.DirekturPelayananFarmasi))
                .Returns<DashboardInfo>();
            builder.Function(nameof(DashboardInfoController.DirekturJenderal))
                .Returns<DashboardInfo>();
            builder.Function(nameof(DashboardInfoController.ValidatorSertifikat))
                .Returns<DashboardInfo>();
        }
    }
}