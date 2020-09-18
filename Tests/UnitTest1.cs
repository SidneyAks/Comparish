using System;
using Comparish;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
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
            Assert.IsFalse(Meaning.MeaninglyEquals(objA, objB, DataDescriptors.Metadata));
            Assert.IsFalse(Meaning.MeaninglyEquals(objB, objA, DataDescriptors.Metadata));

            //Assert that SemanticData fields (specific values) are equal
            Assert.IsTrue(Meaning.MeaninglyEquals(objA, objB, DataDescriptors.Semantic));
            Assert.IsTrue(Meaning.MeaninglyEquals(objB, objA, DataDescriptors.Semantic));
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
            Assert.IsFalse(Meaning.MeaninglyEquals(objA, objB, DataDescriptors.Metadata));
            Assert.IsFalse(Meaning.MeaninglyEquals(objB, objA, DataDescriptors.Metadata));

            //Assert that semantic data fields (specific values vs null) are not equal
            Assert.IsFalse(Meaning.MeaninglyEquals(objA, objB, DataDescriptors.Semantic));
            Assert.IsFalse(Meaning.MeaninglyEquals(objB, objA, DataDescriptors.Semantic));

            //Assert that null data tyupe fields (specific values) are equal
            Assert.IsTrue(Meaning.MeaninglyEquals(objA, objB, null));
            Assert.IsTrue(Meaning.MeaninglyEquals(objB, objA, null));
        }

        [TestMethod]
        public void TestForTypeInferencingEquality()
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
            Assert.IsTrue(Meaning.MeaninglyEquals(objA, objB, DataDescriptors.Semantic));
            Assert.IsTrue(Meaning.MeaninglyEquals(objB, objA, DataDescriptors.Semantic));

            //Assert that semantic data fields from incompatible types results in exception
            Assert.ThrowsException<IncompatibleMeaningException>(() => Meaning.MeaninglyEquals(objA, objC, DataDescriptors.Semantic));
            Assert.ThrowsException<IncompatibleMeaningException>(() => Meaning.MeaninglyEquals(objC, objA, DataDescriptors.Semantic));
        }

        [TestMethod]
        public void TestForMatchingTypeRequirement()
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
            Assert.IsTrue(Meaning.MeaninglyEquals(objA, objB, DataDescriptors.Semantic));
            Assert.IsTrue(Meaning.MeaninglyEquals(objB, objA, DataDescriptors.Semantic));

            //Assert that semantic data fields from incompatible types results in exception
            Assert.ThrowsException<IncompatibleMeaningException>(() => Meaning.MeaninglyEquals(objA, objB, DataDescriptors.Semantic, requireMatchingTypes: true));
            Assert.ThrowsException<IncompatibleMeaningException>(() => Meaning.MeaninglyEquals(objB, objA, DataDescriptors.Semantic, requireMatchingTypes: true));

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
            Assert.IsTrue(Meaning.MeaninglyEquals(objA, objB, DataDescriptors.Semantic));
            Assert.IsTrue(Meaning.MeaninglyEquals(objB, objA, DataDescriptors.Semantic));

            //Assert that semantic data fields allow subsetting, but not supersetting
            Assert.IsTrue(Meaning.MeaninglyEquals(objA, objC, DataDescriptors.Semantic, BSubSetsA: true));
            Assert.ThrowsException<IncompatibleMeaningException>(() => Meaning.MeaninglyEquals(objC, objA, DataDescriptors.Semantic, BSubSetsA: true));

        }

        [TestMethod]
        public void TestForVacuousComparison()
        {
            var objA = new TestingObjectA
            {
                MetadataA = Guid.NewGuid().ToString(),
                MetadataB = Guid.NewGuid().ToString(),
                SemanticDataA = "Foo",
                SemanticDataB = "Bar",
                NullDataA = "Fooz",
                NullDataB = "Barz",
            };

            //Assert that vacuous comparison is equal when allowed
            Assert.IsTrue(Meaning.MeaninglyEquals(objA, objA, "foo", AllowVacuouslyComparison: true));
            Assert.IsTrue(Meaning.MeaninglyEquals(objA, objA, "foo", AllowVacuouslyComparison: true));

            //Assert that vacuous comparison throws exception when not allowed
            Assert.ThrowsException<IncompatibleMeaningException>(() => Meaning.MeaninglyEquals(objA, objA, "foo"));
        }
    }
}
