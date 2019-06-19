using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using QA.DotNetCore.Engine.QpData.Persistent.Dapper;
using QA.Engine.Administration.Data.Core;

namespace Tests
{
    public class SiteMapProviderTests
    {
        private SiteMapProvider _sqlSiteMapProvider;
        private SiteMapProvider _pgSiteMapProvider;
        private UnitOfWork _sqlUnitOfWork;
        private UnitOfWork _postgresUnitOfWork;

        [SetUp]
        public void Setup()
        {
            _sqlUnitOfWork =
                new UnitOfWork(
                    "Initial Catalog=qa_demosite;Data Source=spbdevsql01\\dev;Application Name=Admin;User ID=publishing;Password=QuantumartHost.SQL");
            var sqlMetaRepo = new MetaInfoRepository(_sqlUnitOfWork);
            var sqlAnalyzer = new NetNameQueryAnalyzer(sqlMetaRepo);
            _sqlSiteMapProvider = new SiteMapProvider(_sqlUnitOfWork, sqlAnalyzer, NullLogger<SiteMapProvider>.Instance);
            _postgresUnitOfWork =
                new UnitOfWork("Server=mscpgsql01;Port=5432;Database=qa_demosite;User Id=postgres;Password=1q2w-p=[;", "pg");
            var postgresMetaRepo = new MetaInfoRepository(_postgresUnitOfWork);
            var postgresAnalyzer = new NetNameQueryAnalyzer(postgresMetaRepo);
            _pgSiteMapProvider = new SiteMapProvider(_postgresUnitOfWork, postgresAnalyzer, NullLogger<SiteMapProvider>.Instance);
        }

        [Test]
        public void GetAllItemsTest()
        {
            Assert.DoesNotThrow(() =>
            {
                var res1 = _sqlSiteMapProvider.GetAllItems(52, false, false);
                var res2 = _pgSiteMapProvider.GetAllItems(52, false, false);
                Assert.AreEqual(res1.Count, res2.Count);
            });
        }
        
        [Test]
        public void GetAllItems_UseRegionsTest()
        {
            Assert.DoesNotThrow(() =>
            {
                var res1 = _sqlSiteMapProvider.GetAllItems(52, false, true);
                var res2 = _pgSiteMapProvider.GetAllItems(52, false, true);
                Assert.AreEqual(res1.Count, res2.Count);
            });
        }
        
        [Test]
        public void GetItemsTest()
        {
            Assert.DoesNotThrow(() =>
            {
                var res1 = _sqlSiteMapProvider.GetItems(52, false, new int[]{741114, 741304}, false);
                var res2 = _pgSiteMapProvider.GetItems(52, false, new int[]{741114, 741304},false);
                Assert.AreEqual(res1.Count, res2.Count);
            });
        }
        
        [Test]
        public void GetItems_UseRegionsTest()
        {
            Assert.DoesNotThrow(() =>
            {
                var res1 = _sqlSiteMapProvider.GetItems(52, false, new int[]{741114, 741304}, true);
                var res2 = _pgSiteMapProvider.GetItems(52, false, new int[]{741114, 741304},true);
                Assert.AreEqual(res1.Count, res2.Count);
            });
        }
        
    }
}