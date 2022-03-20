using System;
using System.Collections.Generic;
using System.Linq;
using AstroAdapt.Models;
using Xunit;

namespace Test.Models
{
    public class ExtensionsTests
    {
        public static IEnumerable<object[]> TypeComparisons() =>
            SampleData.NotMatchingTypes().Select(x => new object[] { x.Item1, x.Item2, false })
            .Union(SampleData.MatchingTypes().Select(y => new object[] { y.Item1, y.Item2, true }));

        public static IEnumerable<object?[]> ComponentComparisons()
        {
            yield return new object?[] { null, null, false };
            yield return new object?[] { null, new Component(), false };
            yield return new object?[] { new Component(), null, false };

            Func<(ConnectionTypes, ConnectionTypes)[]> typeFactory;
            Func<(ConnectionSizes, ConnectionSizes)[]> sizeFactory;

            for (var idx = 0; idx < 4; idx++)
            {
                bool match;
                switch (idx)
                {
                    case 0:
                        typeFactory = () => SampleData.MatchingTypes().Take(2).ToArray();
                        sizeFactory = () => SampleData.MatchingSizes().Take(2).ToArray();
                        match = true;
                        break;

                    case 1:
                        typeFactory = () => SampleData.NotMatchingTypes().Take(2).ToArray();
                        sizeFactory = () => SampleData.MatchingSizes().Take(2).ToArray();
                        match = false;
                        break;

                    case 2:
                        typeFactory = () => SampleData.MatchingTypes().Take(2).ToArray();
                        sizeFactory = () => SampleData.NotMatchingSizes().Take(2).ToArray();
                        match = false;
                        break;

                    default:
                        typeFactory = () => SampleData.NotMatchingTypes().Take(2).ToArray();
                        sizeFactory = () => SampleData.NotMatchingSizes().Take(2).ToArray();
                        match = false;
                        break;
                }

                foreach (var typeMatch in typeFactory())
                {
                    foreach (var sizeMatch in sizeFactory())
                    {
                        yield return new object?[]
                        {
                        new Component
                        {
                            TargetDirectionConnectionSize = sizeMatch.Item1,
                            TargetDirectionConnectionType = typeMatch.Item1,
                        },
                        new Component
                        {
                            SensorDirectionConnectionSize = sizeMatch.Item2,
                            SensorDirectionConnectionType = typeMatch.Item2,
                        },
                        match
                        };
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(TypeComparisons))]
        public void IsCompatibleWithWorksForTypes(ConnectionTypes target, ConnectionTypes sensor, bool shouldMatch)
            => Assert.Equal(shouldMatch, target.IsCompatibleWith(sensor));

        [Theory]
        [InlineData(ConnectionTypes.Dual, false, "][")]
        [InlineData(ConnectionTypes.Dual, true, "][")]
        [InlineData(ConnectionTypes.Receiver, true, "]")]
        [InlineData(ConnectionTypes.Receiver, false, "[")]
        [InlineData(ConnectionTypes.Inserter, true, "\\\\")]
        [InlineData(ConnectionTypes.Inserter, false, "//")]
        [InlineData(ConnectionTypes.Terminator, true, "*")]
        [InlineData(ConnectionTypes.Terminator, false, "X")]
        public void AsVisualStringShowsProperString(ConnectionTypes type, bool targetDirection, string expected) =>
            Assert.Equal(expected, type.AsVisualString(targetDirection));

        [Theory, MemberData(nameof(ComponentComparisons))]
        public void IsCompatibleWithWorksForComponents(Component target, Component sensor, bool shouldMatch) =>
            Assert.Equal(shouldMatch, Extensions.IsCompatibleWith(target, sensor).isCompatible);                                              
    }
}
