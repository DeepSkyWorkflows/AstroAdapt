using AstroAdapt.Models;
using Xunit;

namespace Test.Models
{
    public class ConnectionTests
    {
        [Fact]
        public void HashCodeIsHashCodeOfId()
        {
            // arrange
            var target = new Connection();

            // act
            var hashCode = target.GetHashCode();

            // assert
            Assert.Equal(target.Id.GetHashCode(), hashCode);
        }

        [Fact]
        public void StringOverrideIsCombinationOfComponents()
        {
            // arrange
            var target = SampleData.GenerateTarget();
            var sensor = SampleData.GenerateSensor();
            var connection = target.ConnectTo(sensor);

            // act
            var str = connection.ToString();

            // assert
            Assert.Contains(target.ToString(), str);
            Assert.Contains(sensor.ToString(), str);
        }
    }
}
