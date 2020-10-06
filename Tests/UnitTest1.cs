﻿using System;
using System.Collections.Generic;
using System.Linq;
using Comparish;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    public enum DataDescriptors
    {
        Metadata,
        Semantic
    }


    public class TestingObjectA
    {
        [DataDescriptor(DataDescriptors.Metadata)]
        public string MetadataA { get; set; }

        [DataDescriptor(DataDescriptors.Metadata)]
        public string MetadataB { get; set; }

        [DataDescriptor(DataDescriptors.Semantic)]
        public string SemanticDataA { get; set; }

        [DataDescriptor(DataDescriptors.Semantic)]
        public string SemanticDataB { get; set; }

        //No DataDescriptor Specified
        public string NullDataA { get; set; }

        //No DataDescriptor Specified
        public string NullDataB { get; set; }
    }

    public class TestingObjectB
    {
        [DataDescriptor(DataDescriptors.Semantic)]
        public string SemanticDataA { get; set; }

        [DataDescriptor(DataDescriptors.Semantic)]
        public string SemanticDataB { get; set; }
    }
    
    public class TestingObjectC
    {
        [DataDescriptor(DataDescriptors.Semantic)]
        public string SemanticDataA { get; set; }
    }

    public class TestingObjectD
    {
        [DataDescriptor(DataDescriptors.Metadata)]
        public string MetadataA { get; set; }

        [DataDescriptor(DataDescriptors.Semantic)]
        public TestingObjectA[] list { get; set; }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestForBasicDataTypeEquality()
        {
            var objA = new TestingObjectA
            {
                MetadataA = Guid.NewGuid().ToString(),
                MetadataB = Guid.NewGuid().ToString(),
                SemanticDataA = "Foo",
                SemanticDataB = "Bar",
            };

            var objB = new TestingObjectA
            {
                MetadataA = Guid.NewGuid().ToString(),
                MetadataB = Guid.NewGuid().ToString(),
                SemanticDataA = "Foo",
                SemanticDataB = "Bar",
            };

            //Assert that Metadata fields (random guid values) are not equal
            new Comparishon(objA, objB, DataDescriptors.Metadata);
            Assert.IsFalse(new Comparishon(objA, objB, DataDescriptors.Metadata));
            Assert.IsFalse(new Comparishon(objB, objA, DataDescriptors.Metadata));

            //Assert that SemanticData fields (specific values) are equal
            Assert.IsTrue(new Comparishon(objA, objB, DataDescriptors.Semantic));
            Assert.IsTrue(new Comparishon(objB, objA, DataDescriptors.Semantic));
        }

        [TestMethod]
        public void TestForNullDataTypeEquality()
        {
            var objA = new TestingObjectA
            {
                SemanticDataA = "Foo",
                SemanticDataB = "Bar",
                NullDataA = "Fooz",
                NullDataB = "Barz",
            };

            var objB = new TestingObjectA
            {
                MetadataA = Guid.NewGuid().ToString(),
                MetadataB = Guid.NewGuid().ToString(),
                NullDataA = "Fooz",
                NullDataB = "Barz",
            };

            //Assert that Metadata fields (random guids vs null) are not equal
            Assert.IsFalse((new Comparishon(objA, objB, DataDescriptors.Metadata)));
            Assert.IsFalse((new Comparishon(objB, objA, DataDescriptors.Metadata)));

            //Assert that semantic data fields (specific values vs null) are not equal
            Assert.IsFalse((new Comparishon(objA, objB, DataDescriptors.Semantic)));
            Assert.IsFalse((new Comparishon(objB, objA, DataDescriptors.Semantic)));

            //Assert that null data tyupe fields (specific values) are equal
            Assert.IsTrue((new Comparishon(objA, objB, null)));
            Assert.IsTrue((new Comparishon(objB, objA, null)));
        }

        [TestMethod]
        public void TestForSubSettingTypeRequirement()
        {
            var objA = new TestingObjectA
            {
                SemanticDataA = "Foo",
                SemanticDataB = "Bar",
                NullDataA = "Fooz",
                NullDataB = "Barz",
            };
            var objB = new TestingObjectB
            {
                SemanticDataA = "Foo",
                SemanticDataB = "Bar",
            };
            var objC = new TestingObjectC
            {
                SemanticDataA = "Foo"
            };

            //Assert that SemanticData fields (specific values) for compatible types are equal
            Assert.IsTrue(new Comparishon(objA, objB, DataDescriptors.Semantic));
            Assert.IsTrue(new Comparishon(objB, objA, DataDescriptors.Semantic));

            //Assert that semantic data fields allow subsetting, but not supersetting
            Assert.IsTrue(new Comparishon(objA, objC, DataDescriptors.Semantic, AllowPropertySubsetting: true));
            Assert.IsFalse(new Comparishon(objC, objA, DataDescriptors.Semantic, AllowPropertySubsetting: true));

        }

        [TestMethod]
        public void TestForListularDataTypeEquality()
        {
            {

                var objA = new TestingObjectD()
                {
                    MetadataA = Guid.NewGuid().ToString(),
                    list = new TestingObjectA[]
                    {
                        new TestingObjectA()
                        {
                            MetadataA = Guid.NewGuid().ToString(),
                            MetadataB = Guid.NewGuid().ToString(),
                            SemanticDataA = "Foo",
                            SemanticDataB = "Bar",
                        } 
                    }
                };

                var objB = new TestingObjectD()
                {
                    MetadataA = Guid.NewGuid().ToString(),
                    list = new TestingObjectA[]
                    {
                        new TestingObjectA()
                        {
                            MetadataA = Guid.NewGuid().ToString(),
                            MetadataB = Guid.NewGuid().ToString(),
                            SemanticDataA = "Foo",
                            SemanticDataB = "Bar",
                        }
                    }
                };

                var objC = new TestingObjectD()
                {
                    MetadataA = Guid.NewGuid().ToString(),
                    list = new TestingObjectA[]
                    {
                        new TestingObjectA()
                        {
                            MetadataA = Guid.NewGuid().ToString(),
                            MetadataB = Guid.NewGuid().ToString(),
                            SemanticDataA = "Foo!",
                            SemanticDataB = "Bar!",
                        }
                    }
                };

                //Assert that Metadata fields (random guid values) are not equal
                Assert.IsFalse(new Comparishon(objA, objB, DataDescriptors.Metadata));
                Assert.IsFalse(new Comparishon(objB, objA, DataDescriptors.Metadata));

                //Assert that SemanticData fields (specific values) are equal
                Assert.IsTrue(new Comparishon(objA, objB, DataDescriptors.Semantic));
                Assert.IsTrue(new Comparishon(objB, objA, DataDescriptors.Semantic));

                Assert.IsFalse(new Comparishon(objA, objC, DataDescriptors.Semantic));
                Assert.IsFalse(new Comparishon(objC, objA, DataDescriptors.Semantic));
            }
        }

        [TestMethod]
        public void TestForAccurateDataSummarizationOutput()
        {
            var objA = new TestingObjectA
            {
                MetadataA = Guid.NewGuid().ToString(),
                MetadataB = Guid.NewGuid().ToString(),
                SemanticDataA = "Foo",
                SemanticDataB = "Bar",
            };

            var objB = new TestingObjectA
            {
                MetadataA = Guid.NewGuid().ToString(),
                MetadataB = Guid.NewGuid().ToString(),
                SemanticDataA = "Foo",
                SemanticDataB = "Bar",
            };

            //Assert that Metadata fields (random guid values) are not equal
            var ABMetadata = new Comparishon(objA, objB, DataDescriptors.Metadata);
            var BAMetadata = new Comparishon(objB, objA, DataDescriptors.Metadata);
            Assert.IsFalse(ABMetadata);
            Assert.IsFalse(ABMetadata.ChildrenEvaluations.First(x => x.FieldName == "MetadataA").Matches);
            Assert.IsFalse(ABMetadata.ChildrenEvaluations.First(x => x.FieldName == "MetadataB").Matches);
            Assert.IsFalse(BAMetadata);
            Assert.IsFalse(BAMetadata.ChildrenEvaluations.First(x => x.FieldName == "MetadataA").Matches);
            Assert.IsFalse(BAMetadata.ChildrenEvaluations.First(x => x.FieldName == "MetadataB").Matches);

            //Assert that SemanticData fields (specific values) are equal
            var ABSemantic = new Comparishon(objA, objB, DataDescriptors.Semantic);
            var BASemantic = new Comparishon(objB, objA, DataDescriptors.Semantic);
            Assert.IsTrue(ABSemantic);
            Assert.IsTrue(ABSemantic.ChildrenEvaluations.First(x => x.FieldName == "SemanticDataA").Matches);
            Assert.IsTrue(ABSemantic.ChildrenEvaluations.First(x => x.FieldName == "SemanticDataB").Matches); 
            Assert.IsTrue(BASemantic);
            Assert.IsTrue(BASemantic.ChildrenEvaluations.First(x => x.FieldName == "SemanticDataA").Matches);
            Assert.IsTrue(BASemantic.ChildrenEvaluations.First(x => x.FieldName == "SemanticDataB").Matches);
        }
    }
}
