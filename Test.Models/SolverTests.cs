using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroAdapt.Models;
using Xunit;

namespace Test.Models
{
    public class SolverTests
    {
        [Fact]
        public void NewSolverConfiguresCorrectly()
        {
            var target = SampleData.GenerateTarget();
            var sensor = SampleData.GenerateSensor();
            var inventory = new Inventory(SampleData.GenerateSpacers(20));
            inventory.Add(SampleData.GenerateSpacer());
            inventory.Add(SampleData.GenerateSpacer());
            inventory.Add(SampleData.GenerateSpacer());
            var solutions = new List<Solver>();
            var solver = new Solver(target, sensor, inventory);
            Action<Solver> register = s => { };
            register = s =>
            {
                solutions.Add(s);
                s.Solve(register);
            };
            solver.Solve(register);
            Assert.True(solutions.Any());
        }
    }
}
