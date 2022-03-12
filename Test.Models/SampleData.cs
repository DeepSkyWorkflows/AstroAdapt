using System;
using System.Collections.Generic;
using System.Linq;
using AstroAdapt.Models;

namespace Test.Models
{
    public static class SampleData
    {
        private static readonly Random rando = new ();

        public static T SampleRandom<T>() where T: struct, Enum
        {
            var list = (T[])Enum.GetValues(typeof(T));
            return list[rando.Next(list.Length)];
        }

        public static Component GenerateTarget() => new()
        {
            ComponentType = ComponentTypes.OTA,
            TargetDirectionConnectionType = ConnectionTypes.Terminator,
            TargetDirectionConnectionSize = ConnectionSizes.Zero,
            SensorDirectionConnectionType = ConnectionTypes.Receiver,
            SensorDirectionConnectionSize = ConnectionSizes.M42,
            LengthMm = 280,
            BackFocusMm = 55,
            Model = "Test Scope"
        };

        public static Component GenerateSpacer() => new()
        {
            ComponentType = ComponentTypes.Spacer,
            TargetDirectionConnectionType = ConnectionTypes.Inserter,
            TargetDirectionConnectionSize = ConnectionSizes.M42,
            SensorDirectionConnectionType = ConnectionTypes.Receiver,
            SensorDirectionConnectionSize = ConnectionSizes.M42,
            IsReversible = true,
            LengthMm = 11,
            Model = "Spacer"
        };

        public static Connection GenerateConnection(
            Component? target = null,
            Component? sensor = null) =>
            (target ?? GenerateTarget()).ConnectTo(sensor ?? GenerateSensor());

        public static Component[] GenerateSpacers(int count = 10)
        {
            var result = new Component[count];
            for (var idx = 0; idx < count; idx++)
            {
                result[idx] = new()
                {
                    ComponentType = ComponentTypes.Spacer,
                    TargetDirectionConnectionType = SampleRandom<ConnectionTypes>(),
                    TargetDirectionConnectionSize = SampleRandom<ConnectionSizes>(),
                    SensorDirectionConnectionType = SampleRandom<ConnectionTypes>(),
                    SensorDirectionConnectionSize = SampleRandom<ConnectionSizes>(),
                    IsReversible = rando.NextDouble() < 0.8,
                    LengthMm = rando.Next(54) + (rando.NextDouble() < 0.5 ? 1 : 1.5),
                    Model = "Spacer"
                };
            }
            return result;
        }

        public static Component GenerateSensor() => new()
        {
            ComponentType = ComponentTypes.Sensor,
            TargetDirectionConnectionType = ConnectionTypes.Inserter,
            TargetDirectionConnectionSize = ConnectionSizes.M42,
            SensorDirectionConnectionType = ConnectionTypes.Terminator,
            SensorDirectionConnectionSize = ConnectionSizes.Zero,
            LengthMm = 20,
            Model = "Test Camera"
        };

        public static Component QuickGen(
            ComponentTypes type,
            double lengthMm,
            string model,
            ConnectionTypes targetType = ConnectionTypes.Terminator,
            ConnectionSizes targetSize = ConnectionSizes.Zero,
            ConnectionTypes sensorType = ConnectionTypes.Terminator,
            ConnectionSizes sensorSize = ConnectionSizes.Zero,
            InsertionPoints insertionPoint = InsertionPoints.NoPreference,
            double backFocusMm = 0,
            bool reversible = false,
            double threadSizeMm = 0) =>
            new()
            {
                Model = model,
                BackFocusMm = backFocusMm,
                ComponentType = type,
                TargetDirectionConnectionType = targetType,
                TargetDirectionConnectionSize = targetSize,
                SensorDirectionConnectionType = sensorType,
                SensorDirectionConnectionSize = sensorSize,
                InsertionPoint = insertionPoint,
                LengthMm = lengthMm,
                IsReversible = reversible,
                ThreadRecessMm = threadSizeMm,
            };
       
        public static int GetRange<T>() where T : struct, Enum =>
            Enum.GetValues<T>().Length;

        public static (ConnectionTypes, ConnectionTypes)[] MatchingTypes() =>
            new[]
            {
                (ConnectionTypes.Inserter, ConnectionTypes.Dual),
                (ConnectionTypes.Inserter, ConnectionTypes.Receiver),
                (ConnectionTypes.Receiver, ConnectionTypes.Dual),
                (ConnectionTypes.Receiver, ConnectionTypes.Inserter),
                (ConnectionTypes.Dual, ConnectionTypes.Dual),
            };

        public static (ConnectionTypes, ConnectionTypes)[] NotMatchingTypes() =>
            new[]
            {
                (ConnectionTypes.Inserter, ConnectionTypes.Terminator),
                (ConnectionTypes.Inserter, ConnectionTypes.Inserter),
                (ConnectionTypes.Receiver, ConnectionTypes.Terminator),
                (ConnectionTypes.Receiver, ConnectionTypes.Receiver),
                (ConnectionTypes.Dual, ConnectionTypes.Terminator),
                (ConnectionTypes.Terminator, ConnectionTypes.Dual),
                (ConnectionTypes.Terminator, ConnectionTypes.Inserter),
                (ConnectionTypes.Terminator, ConnectionTypes.Receiver),
                (ConnectionTypes.Terminator, ConnectionTypes.Terminator)
            };

        public static (ConnectionSizes, ConnectionSizes)[] MatchingSizes() =>
            Enum.GetValues<ConnectionSizes>().Where(s => s != ConnectionSizes.Zero)
            .Select(cs => (cs, cs)).ToArray();

        public static (ConnectionSizes, ConnectionSizes)[] NotMatchingSizes() =>
            Enum.GetValues<ConnectionSizes>().SelectMany(cs => Enum.GetValues<ConnectionSizes>(),
                (cs1, cs2) => (cs1, cs2)).Where(pair => pair.cs1 == ConnectionSizes.Zero ||
                pair.cs2 == ConnectionSizes.Zero || pair.cs1 != pair.cs2).ToArray();

        public static IEnumerable<object[]> FullPermutationsOfComponents()
        {
            var maxSize = GetRange<ConnectionSizes>();
            var maxType = GetRange<ConnectionTypes>();

            int srcType = 0, srcSize = 0;
            int destType = 0, destSize = 0;
            int recess = 0, bf = 0;

            bool done = false;

            while (!done)
            {
                var component = new Component();
                component.Manufacturer.Name = "Test";
                component.BackFocusMm = bf == 0 ? 0 : 55;
                component.ThreadRecessMm = recess == 0 ? 0 : 6;
                component.LengthMm = 11.5;
                component.TargetDirectionConnectionType = (ConnectionTypes)srcType;
                component.TargetDirectionConnectionSize = (ConnectionSizes)srcSize;
                component.SensorDirectionConnectionType = (ConnectionTypes)destType;
                component.SensorDirectionConnectionSize = (ConnectionSizes)destSize;
                yield return new[] { component };

                static bool Increment(ref int src, int max)
                {
                    src++;
                    if (src == max)
                    {
                        src = 0;
                        return false;
                    }
                    return true;
                }

                if (Increment(ref bf, 2))
                {
                    continue;
                }

                if (Increment(ref recess, 2))
                {
                    continue;
                }

                if (Increment(ref srcSize, maxSize))
                {
                    continue;
                }

                if (Increment(ref srcType, maxType))
                {
                    continue;
                }

                if (Increment(ref destSize, maxSize))
                {
                    continue;
                }

                if (Increment(ref destType, maxType))
                {
                    continue;
                }

                done = true;
            }
        }

        public static IEnumerable<object[]> CompatibilityMatrix()
        {
            // perfect fits
            foreach (var typeMatch in MatchingTypes())
            {
                foreach (var sizeMatch in MatchingSizes())
                {
                    var target = new Component
                    {
                        TargetDirectionConnectionType = typeMatch.Item1,
                        TargetDirectionConnectionSize = sizeMatch.Item1
                    };

                    var sensor = new Component
                    {
                        SensorDirectionConnectionType = typeMatch.Item2,
                        SensorDirectionConnectionSize = sizeMatch.Item2
                    };

                    yield return new object[] { target, sensor, true };
                }
            }            

            // type mismatch
            foreach (var typeMatch in NotMatchingTypes())
            {
                foreach (var sizeMatch in MatchingSizes())
                {
                    var target = new Component
                    {
                        TargetDirectionConnectionType = typeMatch.Item1,
                        TargetDirectionConnectionSize = sizeMatch.Item1
                    };

                    var sensor = new Component
                    {
                        SensorDirectionConnectionType = typeMatch.Item2,
                        SensorDirectionConnectionSize = sizeMatch.Item2
                    };

                    yield return new object[] { target, sensor, false };
                }
            }

            // size mismatch
            foreach (var typeMatch in MatchingTypes())
            {
                foreach (var sizeMatch in NotMatchingSizes())
                {
                    var target = new Component
                    {
                        TargetDirectionConnectionType = typeMatch.Item1,
                        TargetDirectionConnectionSize = sizeMatch.Item1
                    };

                    var sensor = new Component
                    {
                        SensorDirectionConnectionType = typeMatch.Item2,
                        SensorDirectionConnectionSize = sizeMatch.Item2
                    };

                    yield return new object[] { target, sensor, false };
                }
            }

            // type and size mismatch
            foreach (var typeMatch in NotMatchingTypes())
            {
                foreach (var sizeMatch in NotMatchingSizes())
                {
                    var target = new Component
                    {
                        TargetDirectionConnectionType = typeMatch.Item1,
                        TargetDirectionConnectionSize = sizeMatch.Item1
                    };

                    var sensor = new Component
                    {
                        SensorDirectionConnectionType = typeMatch.Item2,
                        SensorDirectionConnectionSize = sizeMatch.Item2
                    };

                    yield return new object[] { target, sensor, false };
                }
            }
        }
    }
}
