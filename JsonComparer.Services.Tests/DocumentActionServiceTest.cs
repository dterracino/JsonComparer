﻿namespace BigEgg.Tools.JsonComparer.Services.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json.Linq;

    using BigEgg.Tools.JsonComparer.Progress;

    public class DocumentActionServiceTest
    {
        [TestClass]
        public class SplitTest : TestClassBase
        {
            private const string TEST_JSON_FILE = "TestData\\JsonDocToSplit.json";
            private const string OUTPUT_PATH = "out";
            private const string VERSION_NODE_NAME = "version";
            private const string ARRAY_NODE_NAME = "array";
            private const string NUMBER_NODE_NAME = "number";
            private const string DATA_NODE_NAME = "data";
            private const string STATUS_NODE_NAME = "status";
            private readonly static string OutputFileNamePattern = "name_" + Constants.SPLIT_OUTPUT_FILE_NAME_REPLACER;

            protected override void OnTestCleanup()
            {
                if (Directory.Exists(OUTPUT_PATH))
                {
                    Directory.Delete(OUTPUT_PATH, true);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task FileName_Null()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(null, OUTPUT_PATH, DATA_NODE_NAME);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task FileName_EmptyString()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(string.Empty, OUTPUT_PATH, DATA_NODE_NAME);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task OutputPath_Null()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(TEST_JSON_FILE, null, DATA_NODE_NAME);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task OutputPath_EmptyString()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(TEST_JSON_FILE, string.Empty, DATA_NODE_NAME);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NodeName_Null()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(TEST_JSON_FILE, OUTPUT_PATH, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NodeName_EmptyString()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(TEST_JSON_FILE, OUTPUT_PATH, string.Empty);
            }

            [TestMethod]
            public async Task SplitObjectInObject()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(TEST_JSON_FILE, OUTPUT_PATH, DATA_NODE_NAME);

                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\BigEgg.json"));
                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\Pupil.json"));

                var documentSrv = Container.GetExportedValue<IJsonDocumentService>();
                var jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\BigEgg.json");
                var valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(30L, ((JValue)valueNode).Value);
                jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\Pupil.json");
                valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(31L, ((JValue)valueNode).Value);
            }

            [TestMethod]
            public async Task SplitObjectInArray()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(TEST_JSON_FILE, OUTPUT_PATH, ARRAY_NODE_NAME);

                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\1.json"));
                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\2.json"));

                var documentSrv = Container.GetExportedValue<IJsonDocumentService>();
                var jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\1.json");
                var valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(30L, ((JValue)valueNode).Value);
                jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\2.json");
                valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(31L, ((JValue)valueNode).Value);
            }

            [TestMethod]
            [ExpectedException(typeof(NotSupportedException))]
            public async Task SplitValue()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(TEST_JSON_FILE, OUTPUT_PATH, VERSION_NODE_NAME);
            }

            [TestMethod]
            public async Task SplitValueInObject()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(TEST_JSON_FILE, OUTPUT_PATH, STATUS_NODE_NAME);

                Assert.AreEqual(0, Directory.EnumerateFiles(OUTPUT_PATH).Count());
            }

            [TestMethod]
            public async Task SplitValueInArray()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(TEST_JSON_FILE, OUTPUT_PATH, STATUS_NODE_NAME);

                Assert.AreEqual(0, Directory.EnumerateFiles(OUTPUT_PATH).Count());
            }

            [TestMethod]
            public async Task SplitObjectInObject_WithFileNamePattern()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(TEST_JSON_FILE, OUTPUT_PATH, DATA_NODE_NAME, OutputFileNamePattern);

                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\name_BigEgg.json"));
                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\name_Pupil.json"));

                var documentSrv = Container.GetExportedValue<IJsonDocumentService>();
                var jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\name_BigEgg.json");
                var valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(30L, ((JValue)valueNode).Value);
                jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\name_Pupil.json");
                valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(31L, ((JValue)valueNode).Value);
            }

            [TestMethod]
            public async Task SplitObjectInArray_WithFileNamePattern()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                await service.SplitFile(TEST_JSON_FILE, OUTPUT_PATH, ARRAY_NODE_NAME, OutputFileNamePattern);

                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\name_1.json"));
                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\name_2.json"));

                var documentSrv = Container.GetExportedValue<IJsonDocumentService>();
                var jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\name_1.json");
                var valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(30L, ((JValue)valueNode).Value);
                jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\name_2.json");
                valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(31L, ((JValue)valueNode).Value);
            }

            [TestMethod]
            public async Task SplitObjectInObject_WithProgress()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                var calledCount = 0;
                var progress = new Progress<IProgressReport>(report =>
                {
                    calledCount++;
                    Assert.AreEqual(2, report.Total);
                    Assert.IsTrue(report.Current >= 0);
                    Assert.IsTrue(report.Current <= 2);
                });
                await service.SplitFile(TEST_JSON_FILE, OUTPUT_PATH, DATA_NODE_NAME, progress);

                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\BigEgg.json"));
                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\Pupil.json"));
                Assert.AreEqual(4, calledCount);

                var documentSrv = Container.GetExportedValue<IJsonDocumentService>();
                var jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\BigEgg.json");
                var valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(30L, ((JValue)valueNode).Value);
                jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\Pupil.json");
                valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(31L, ((JValue)valueNode).Value);
            }

            [TestMethod]
            public async Task SplitObjectInArray_WithProgress()
            {
                var service = Container.GetExportedValue<IDocumentActionService>();
                var calledCount = 0;
                var progress = new Progress<IProgressReport>(report =>
                {
                    calledCount++;
                    Assert.AreEqual(2, report.Total);
                    Assert.IsTrue(report.Current >= 0);
                    Assert.IsTrue(report.Current <= 2);
                });
                await service.SplitFile(TEST_JSON_FILE, OUTPUT_PATH, ARRAY_NODE_NAME, progress);

                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\1.json"));
                Assert.IsTrue(File.Exists($"{OUTPUT_PATH}\\2.json"));
                Assert.AreEqual(4, calledCount);

                var documentSrv = Container.GetExportedValue<IJsonDocumentService>();
                var jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\1.json");
                var valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(30L, ((JValue)valueNode).Value);
                jsonObj = documentSrv.ReadJsonFile($"{OUTPUT_PATH}\\2.json");
                valueNode = documentSrv.GetNode(jsonObj, "age");
                Assert.AreEqual(31L, ((JValue)valueNode).Value);
            }

            public class ProgressData
            {
                public int Data { get; set; }
            }
        }
    }
}
