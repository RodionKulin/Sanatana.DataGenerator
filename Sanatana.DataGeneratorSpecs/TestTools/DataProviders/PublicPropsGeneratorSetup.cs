using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Supervisors.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGeneratorSpecs.TestTools.DataProviders
{
    internal class PublicPropsGeneratorSetup : GeneratorSetup
    {
        //props
        public ISupervisor Supervisor 
        { 
            get { return _supervisor; }
            set { _supervisor = value; }
        }


        //init
        public PublicPropsGeneratorSetup(IEnumerable<IEntityDescription> entityDescriptions)
        {
            _entityDescriptions = entityDescriptions.ToDictionary(x => x.Type, x => x);
        }


        //methods
        public void PublicSetup()
        {
            GeneratorServices generatorServices = GetGeneratorServices();
            base.Setup(generatorServices);
        }

    }
}
