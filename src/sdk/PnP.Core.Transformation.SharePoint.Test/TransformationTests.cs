﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SharePoint.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Test.Utilities;

namespace PnP.Core.Transformation.SharePoint.Test
{
    [TestClass]
    public class TransformationTests
    {

        [TestMethod]
        public async Task SharepointTransformAsync()
        {
            var config = TestCommon.GetConfigurationSettings();

            var services = new ServiceCollection();
            services.AddTestPnPCore();
            // services.AddPnPSharePointTransformation();
            services.AddPnPSharePointTransformation(
                pnpOptions =>
                {
                    pnpOptions.DisableTelemetry = false;
                    pnpOptions.PersistenceProviderConnectionString = config["PersistenceProviderConnectionString"];
                }
                , spOptions =>
                {
                    //spOptions.WebPartMappingFile = config["WebPartMappingFile"];
                    //spOptions.PageLayoutMappingFile = config["PageLayoutMappingFile"];
                    spOptions.CopyPageMetadata = true;
                    spOptions.KeepPageSpecificPermissions = true;
                    spOptions.RemoveEmptySectionsAndColumns = true;
                    spOptions.ShouldMapUsers = true;
                    spOptions.TargetPageTakesSourcePageName = true;
                    spOptions.HandleWikiImagesAndVideos = true;
                    spOptions.AddTableListImageAsImageWebPart = true;
                }
            );

            var provider = services.BuildServiceProvider();

            var pnpContextFactory = provider.GetRequiredService<IPnPContextFactory>();
            var pageTransformator = provider.GetRequiredService<IPageTransformator>();

            var sourceContext = provider.GetRequiredService<ClientContext>();
            var targetContext = await pnpContextFactory.CreateAsync(TestCommon.TargetTestSite);
            var sourceUri = new Uri(config["SourceUri"]);

            var result = await pageTransformator.TransformSharePointAsync(sourceContext, targetContext, sourceUri);

            Assert.IsNotNull(result);
            var expectedUri = new Uri($"{targetContext.Web.Url}/SitePages/Migrated_{sourceUri.Segments[sourceUri.Segments.Length - 1]}");
            Assert.AreEqual(expectedUri, result);
        }

        [TestMethod]
        public async Task InMemoryExecutorSharePointTransformAsync()
        {
            var services = new ServiceCollection();
            services.AddTestPnPCore();
            services.AddPnPSharePointTransformation();

            var provider = services.BuildServiceProvider();

            var transformationExecutor = provider.GetRequiredService<ITransformationExecutor>();
            var pnpContextFactory = provider.GetRequiredService<IPnPContextFactory>();

            var sourceContext = provider.GetRequiredService<ClientContext>();

            var result = await transformationExecutor.TransformSharePointAsync(
                pnpContextFactory,
                sourceContext,
                TestCommon.TargetTestSite);

            Assert.IsNotNull(result);
            Assert.AreEqual(TransformationExecutionState.Completed, result.State);
        }

    }
}
