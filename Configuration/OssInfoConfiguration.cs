﻿using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;
using PsefApi.Models;

namespace PsefApi.Configuration
{
    /// <summary>
    /// Represents the model configuration for Pemohon.
    /// </summary>
    public class OssInfoConfiguration : IModelConfiguration
    {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion)
        {
            EntityTypeConfiguration<OssInfo> ossInfo = builder
                .EntitySet<OssInfo>(nameof(OssInfo))
                .EntityType;

            ossInfo.HasKey(p => p.Nib);
            ossInfo
                .Select();
        }
    }
}