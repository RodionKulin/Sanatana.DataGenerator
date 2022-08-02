using System.Collections.Generic;
using AutoBogus;
using AutoBogus.FakeItEasy;
using Sanatana.DataGenerator.Internals;

namespace Sanatana.DataGeneratorSpecs.TestTools
{
    public class CloneVerification
    {
        public static T Fake<T>()
            where T : class
        {
            AutoFaker<T> faker = new AutoFaker<T>()
                .Configure(config => config.WithBinder<FakeItEasyBinder>());
            List<T> instances = faker.UseSeed(1)
                .Generate(1);
            return instances[0];
        }

        public static TemporaryStorage FakeTemporaryStorage()
        {
            //throws exception

            AutoFaker<TemporaryStorage> faker = new AutoFaker<TemporaryStorage>()
                .Configure(config => config.WithBinder<FakeItEasyBinder>());
            List<TemporaryStorage> instances = faker.UseSeed(1)
                .Ignore(x => x.MaxTasksRunning)
                .Generate(1);
            return instances[0];
        }
    }
}
