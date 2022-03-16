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

        [Fact]
        public void ConnectToThrowsWhenTargetIsNull()
        {
            // arrange
            var target = SampleData.GenerateTarget();
            var sensor = SampleData.GenerateSensor();
            var spacer = SampleData.GenerateSpacer();
            var connection = target.ConnectTo(spacer);
            var connector = new Connector(connection);

            // act and assert
            Assert.Throws<ArgumentNullException>(
                () => connector.ConnectTo(null!));
        }

        [Fact]
        public void ConnectToThrowsWhenConnectionIsNull()
        {
            // arrange
            var sensor = SampleData.GenerateSensor();
            var connector = new Connector(null!);

            // act and assert
            Assert.Throws<InvalidOperationException>(
                () => connector.ConnectTo(sensor));
        }

        [Fact]
        public void ConnectToThrowsWhenAlreadyConnected()
        {
            // arrange
            var target = SampleData.GenerateTarget();
            var sensor = SampleData.GenerateSensor();
            var spacer = SampleData.GenerateSpacer();
            var spacer2 = SampleData.GenerateSpacer();
            var connection = target.ConnectTo(spacer);
            var connector = new Connector(connection);
            var connection2 = spacer.ConnectTo(sensor);
            var connector2 = new Connector(connector, connection2);

            // act and assert
            Assert.Throws<InvalidOperationException>(
                () => connector.ConnectTo(spacer2));
        }        

        [Fact]
        public void ConnectToThrowsOnTypeMismatch()
        {
            // arrange
            var target = SampleData.GenerateTarget();
            var sensor = SampleData.GenerateSensor();
            var spacer = SampleData.GenerateSpacer();
            var connection = target.ConnectTo(spacer);
            var connector = new Connector(connection);
            spacer.SensorDirectionConnectionType = ConnectionTypes.Receiver;
            sensor.TargetDirectionConnectionType = ConnectionTypes.Receiver;

            // act and assert
            Assert.Throws<InvalidOperationException>(
                () => connector.ConnectTo(sensor));
        }

        [Fact]
        public void ConnectToConnectsTheSensorSideWhenTargetConnectorIsNull()
        {
            // arrange
            var target = SampleData.GenerateTarget();
            var sensor = SampleData.GenerateSensor();
            var spacer = SampleData.GenerateSpacer();
            var connection = target.ConnectTo(spacer);
            var connector = new Connector(connection);

            // act
            connector.ConnectTo(sensor);

            // assert
            Assert.Null(connector.TargetConnector);
            Assert.NotNull(connector.SensorConnector);
        }

        [Fact]
        public void ConnectToConnectsNewWhenTargetConnectorIsNull()
        {
            // arrange
            var target = SampleData.GenerateTarget();
            var sensor = SampleData.GenerateSensor();
            var spacer = SampleData.GenerateSpacer();
            var spacer2 = SampleData.GenerateSpacer();
            var connection = target.ConnectTo(spacer);
            var connector = new Connector(connection);
            var connector2 = connector.ConnectTo(spacer2);

            // act
            connector2.ConnectTo(sensor);

            // assert
            Assert.NotNull(connector2.TargetConnector);
            Assert.NotNull(connector2.SensorConnector);
        }                       
    }
}
