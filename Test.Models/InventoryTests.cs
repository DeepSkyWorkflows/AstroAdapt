using System.Linq;
using System.Security.Cryptography;
using AstroAdapt.Models;
using Xunit;

namespace Test.Models
{
    public class InventoryTests
    {
        [Fact]
        public void NewInstanceInitializesEmpty()
        {
            // arrange and act
            var i = new Inventory();

            Assert.Empty(i.Available);
        }

        [Fact]
        public void NewWithComponentsMakesThemAvailable()
        {
            // arrange
            var components = SampleData.GenerateSpacers(20);

            // act
            var i = new Inventory(components);

            // assert
            Assert.Equal(components, i.Available);
        }

        [Fact]
        public void AddMakesAvailable()
        {
            // arrange
            var i = new Inventory();
            var component = SampleData.GenerateSpacer();

            // act
            var add = i.Add(component);

            // assert
            Assert.True(add);
            Assert.Single(i.Available, component);
        }


        [Fact]
        public void AddRejectsDuplicates()
        {
            // arrange
            var i = new Inventory();
            var component = SampleData.GenerateSpacer();

            // act
            i.Add(component);
            var add = i.Add(component);

            // assert
            Assert.False(add);
            Assert.Single(i.Available, component);
        }

        [Fact]
        public void ConsumeRejectsUnavailableComponents()
        {
            // arrange
            var i = new Inventory();
            var components = SampleData.GenerateSpacers(2);
            i.Add(components.First());

            // act
            var success = i.Consume(components.Last());
            
            // assert
            Assert.False(success);            
        }

        [Fact]
        public void ConsumeRejectsAlreadyConsumed()
        {
            // arrange
            var i = new Inventory();
            var component = SampleData.GenerateSpacer();
            i.Add(component);
            i.Consume(component);

            // act            
            var success = i.Consume(component);

            // assert
            Assert.False(success);
        }

        [Fact]
        public void ConsumeRemovesFromAvailable()
        {
            // arrange
            var i = new Inventory();
            var component = SampleData.GenerateSpacer();
            i.Add(component);
            
            // act            
            var success = i.Consume(component);

            // assert
            Assert.True(success);
            Assert.Empty(i.Available);
        }

        [Fact]
        public void CloneMakesCopy()
        {
            // arrange
            var components = SampleData.GenerateSpacers(20);
            var i = new Inventory(components);

            // act
            var i2 = i.Clone();

            // assert
            Assert.Equal(i.Available, i2.Available);
        }

        [Fact]
        public void CloneReplaysConsumed()
        {
            // arrange
            var components = SampleData.GenerateSpacers(20);
            var i = new Inventory(components);
            var success = i.Consume(components.First()) && i.Consume(components.Last());

            // act
            var i2 = i.Clone();

            // assert
            Assert.True(success);
            Assert.Equal(i.Available, i2.Available);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void AvailableForReturnsMatchingComponents(int numMatches)
        {
            // arrange
            var target = SampleData.GenerateSpacer(); //M42 Receiver
            var spacers = SampleData.GenerateSpacers(10);
            for (var idx = 0; idx < spacers.Length; idx++)
            {
                if (idx < numMatches)
                {
                    spacers[idx].TargetDirectionConnectionType = ConnectionTypes.Inserter;
                    spacers[idx].TargetDirectionConnectionSize = target.SensorDirectionConnectionSize;
                }
                else
                {
                    spacers[idx].TargetDirectionConnectionType = ConnectionTypes.Receiver;
                }
            }
            var i = new Inventory(spacers);

            // act
            var matches = i.AvailableFor(target);

            // assert
            Assert.Equal(numMatches, matches.Count());
        }
    }
}
