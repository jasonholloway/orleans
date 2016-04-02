using System;
using System.Collections.Generic;
using System.Linq;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Orleans.Runtime;
using Orleans.Serialization;
using Xunit;
using Orleans.CodeGeneration;
using Orleans;
using Orleans.Concurrency;

namespace UnitTests
{
    public class GrainTypeMappingTests 
    {   

        abstract class TestClasses
        {
            public interface IGrain1 : IGrain { }
            public interface IGrain2 : IGrain { }
            public interface IGrain3 : IGrain { }
            public interface IGrain4 : IGrain { }
            public interface IGrain5 : IGrain { }
            public interface IGrain6 : IGrain5 { }
            public interface IGrain7 : IGrain6 { }
            public interface IGrain8 : IGrain7 { }
            public interface IGrain9 : IGrain { }
            
            public class Grain0 : Grain, IGrain1 { }
            public class Grain1 : Grain, IGrain1 { }
            public class Grain2 : Grain, IGrain1 { }
            
            [StatelessWorker]
            public class Grain3 : Grain, IGrain3 { }
            public class Grain4 : Grain3, IGrain4 { }

            public class Grain5 : Grain, IGrain5 { }
            public class Grain6 : Grain5, IGrain6 { }
            public class Grain7 : Grain, IGrain7 { }

            public class Grain8 : Grain<GrainState1>, IGrain8 { }
            public class Grain9 : Grain<GrainState2>, IGrain9 { }

            public class GrainState1 : IGrainState
            {
                public string ETag { get; set; }
                public object State { get; set; }
            }
            
            public class GrainState2 : IGrainState
            {
                public string ETag { get; set; }
                public object State { get; set; }
            }
            
            public class GrainState3 : GrainState2 { }
            
            public class AnotherClass1 { }
            public class AnotherClass2 { }            
        }



        static GrainTypeData[] GenerateTypeDataFrom<TCont>() 
        {
            return typeof(TCont).GetNestedTypes()
                                  .Where(t => TypeUtils.IsGrainClass(t))
                                  .Select(t => Type2TypeData(t))
                                  .ToArray();
        }

        static GrainTypeData Type2TypeData(Type type) 
        {
            var genGrainType = type.GetClasses()
                                    .FirstOrDefault(t => t.IsSpecializationOf(typeof(Grain<>)));

            var storageType = genGrainType != null
                                    ? genGrainType.GetGenericArguments().First()
                                    : null;

            return new GrainTypeData(type, storageType);
        }






        [Fact, TestCategory("GrainTypeMapping")]
        public void IncludesAllImplementedInterfaces()
        {
            var typeDatas = GenerateTypeDataFrom<TestClasses>();

            var ifaceIDs = typeDatas.SelectMany(d => d.RemoteInterfaceTypes)
                                        .Distinct()
                                        .Select(t => GrainInterfaceUtils.GetGrainInterfaceId(t));

            var map = GrainTypeMapper.BuildMap(typeDatas, false);

            foreach(var ifaceID in ifaceIDs) {
                Assert.IsTrue(map.ContainsGrainInterface(ifaceID));
            }
        }


        [Fact, TestCategory("GrainTypeMapping")]
        public void EachInterfaceHasOnePrimaryImplementation() 
        {            
            var typeDatas = GenerateTypeDataFrom<TestClasses>();

            var ifaceNames = typeDatas.SelectMany(d => d.RemoteInterfaceTypes)
                                        .Distinct()
                                        .Select(t => TypeUtils.GetRawClassName(t.GetFullName()));

            var map = GrainTypeMapper.BuildMap(typeDatas, false);

            foreach(var ifaceName in ifaceNames) {
                string implName;
                Assert.IsTrue(map.TryGetPrimaryImplementation(ifaceName, out implName));
            }
        }


        [Fact, TestCategory("GrainTypeMapping")]
        public void MatchingNameMeansPrimaryImplementation() 
        {
            var typeDatas = GenerateTypeDataFrom<TestClasses>();
            
            var map = GrainTypeMapper.BuildMap(typeDatas, false);
                        
            string grainName;
            Assert.IsTrue(map.TryGetPrimaryImplementation(typeof(TestClasses.IGrain1).GetFullName(), out grainName));
            Assert.AreEqual(grainName, typeof(TestClasses.Grain1).GetFullName());            
        }


        [Fact, TestCategory("GrainTypeMapping")]
        public void OnlyStatelessWorkersAreUnordered() 
        {
            var typeDatas = GenerateTypeDataFrom<TestClasses>();

            var map = GrainTypeMapper.BuildMap(typeDatas, false);
            
            Assert.IsTrue(map.IsUnordered(GrainInterfaceUtils.GetGrainClassTypeCode(typeof(TestClasses.Grain3))));
            Assert.IsTrue(map.IsUnordered(GrainInterfaceUtils.GetGrainClassTypeCode(typeof(TestClasses.Grain4))));
            Assert.IsFalse(map.IsUnordered(GrainInterfaceUtils.GetGrainClassTypeCode(typeof(TestClasses.Grain5))));
            Assert.IsFalse(map.IsUnordered(GrainInterfaceUtils.GetGrainClassTypeCode(typeof(TestClasses.Grain6))));
        }





    }
}
