using System;
using AstroAdapt.Models;
using Xunit;

namespace Test.Models
{
    public class ConnectorTests
    {
        [Fact]
        public void ConnectorThrowsWhenTargetAlreadyConnected()
        {
            // arrange
            var target = SampleData.GenerateTarget();
            var sensor = SampleData.GenerateSensor();
            var spacer = SampleData.GenerateSpacer();
            var connection = target.ConnectTo(spacer);
            var connector = new Connector(connection);
            var connection2 = spacer.ConnectTo(sensor);
            var connector2 = new Connector(connector, connection2);

            // act and assert
            Assert.Throws<InvalidOperationException>(
                () => new Connector(connector, connection2));
        }

        [Fact]
        public void ConnectorSetsSensorConnectorOnTarget()
        {
            // arrange
            var target = SampleData.GenerateTarget();
            var sensor = SampleData.GenerateSensor();
            var spacer = SampleData.GenerateSpacer();
            var connection = target.ConnectTo(spacer);
            var connector = new Connector(connection);
            var connection2 = spacer.ConnectTo(sensor);

            // act
            var connector2 = new Connector(connector, connection2);

            // assert
            Assert.Equal(connector.SensorConnector, connector2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TailAlwaysResolvesToLastConnector(int connectors)
        {
            // arrange
            var target = SampleData.GenerateTarget();
            var sensor = SampleData.GenerateSensor();
            var spacer = SampleData.GenerateSpacer();
            var spacer2 = SampleData.GenerateSpacer();

            Connector connector, expected;

            if (connectors == 1)
            {
                var connection = target.ConnectTo(sensor);
                connector = new Connector(connection);
                expected = connector;
            }
            else if (connectors == 2)
            {
                var connection = target.ConnectTo(spacer);
                connector = new Connector(connection);
                var connection2 = spacer.ConnectTo(sensor);
                var connector2 = new Connector(connector, connection2);
                expected = connector2;
            }
            else
            {
                var connection = target.ConnectTo(spacer);
                connector = new Connector(connection);
                var connection2 = spacer.ConnectTo(spacer2);
                var connector2 = new Connector(connector, connection2);
                var connection3 = spacer2.ConnectTo(sensor);
                var connector3 = new Connector(connector2, connection3);
                expected = connector3;
            }

            // act
            var actual = connector.Tail;

            // assert
            Assert.True(actual == expected);
        }

        [Fact]
        public void SetsConnectionWhenPassedInConstructor()
        {
            // arrange
            var conn = SampleData.GenerateConnection();

            // act
            var connector = new Connector(conn);

            // assert
            Assert.Equal(conn, connector.Connection);
        }

        [Fact]
        public void ThrowsWhenTargetAlreadyIsConnected()
        {
            // arrange
            var conn = SampleData.GenerateConnection(
                SampleData.GenerateTarget(), SampleData.GenerateSpacer());
            var connector = new Connector(conn);
            connector.ConnectTo(SampleData.GenerateSpacer());
            var newConnection = connector.Tail.Connection.SensorDirectionComponent!
                .ConnectTo(SampleData.GenerateSensor());

            // act and assert
            Assert.Throws<InvalidOperationException>(
                () => new Connector(connector, conn));
        }

        [Fact]
        public void SetsTargetConnector()
        {
            // arrange
            var conn = SampleData.GenerateConnection(
                SampleData.GenerateTarget(), SampleData.GenerateSpacer());
            var connector = new Connector(conn);

            // act
            var target = connector.ConnectTo(SampleData.GenerateSpacer());

            // assert
            Assert.Equal(connector, target.TargetConnector);
        }

        [Fact]
        public void ChainsToSelf()
        {
            // arrange
            var conn = SampleData.GenerateConnection(
                SampleData.GenerateTarget(), SampleData.GenerateSpacer());
            var connector = new Connector(conn);

            // act
            var target = connector.ConnectTo(SampleData.GenerateSpacer());

            // assert
            Assert.Equal(connector.SensorConnector, target);
        }
    }
}
