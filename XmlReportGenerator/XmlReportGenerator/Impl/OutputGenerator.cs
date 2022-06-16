using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlReportGenerator.Helpers;
using XmlReportGenerator.Interfaces;
using XmlReportGenerator.Models;

namespace XmlReportGenerator.Impl
{
    public class OutputGenerator : IOutputGenerator
    {
        private readonly ConcurrentBag<GeneratorTotal>  _generatorTotals;
        private readonly ConcurrentBag<DayEmission>  _dayEmissions;
        private readonly ConcurrentBag<ActualHeatRate>  _actualHeatRates;
        private readonly IXmlFile _xmlFile;

        public OutputGenerator(IXmlFile xmlFile)
        {
            _generatorTotals = new ConcurrentBag<GeneratorTotal>();
            _dayEmissions = new ConcurrentBag<DayEmission>();
            _actualHeatRates = new ConcurrentBag<ActualHeatRate>();
            _xmlFile = xmlFile;
        }

        public async ValueTask<bool> AddToOuput(WindGenerator windGenerator)
        {
            return await AddGenerationOutput(windGenerator);
        }

        public async ValueTask<bool> AddToOuput(GasGenerator gasGenerator)
        {
            await AddGenerationOutput(gasGenerator);
            return await AddHighestDailyEmission(gasGenerator, GeneratorHelper.GeneratorFactors.EmissionsFactor.Medium);
        }

        public async ValueTask<bool> AddToOuput(CoalGenerator coalGenerator)
        {
            await AddGenerationOutput(coalGenerator);
            await AddActualHeatRate(coalGenerator);
            return await AddHighestDailyEmission(coalGenerator, GeneratorHelper.GeneratorFactors.EmissionsFactor.High);
        }
        private async ValueTask<bool> AddGenerationOutput(WindGenerator windGenerator)
        {
            if (windGenerator != null)
            {
                var existingGenerator = GetGeneratorTotal(windGenerator.Name);

                if (existingGenerator != null)
                {
                    decimal generationValue = decimal.Zero;

                    if (string.Compare(windGenerator.Location, "Offshore") == 0)
                        generationValue = GetGenerationValue(windGenerator.Generation, GeneratorHelper.GeneratorFactors.ValueFactor.Low);
                    else if (string.Compare(windGenerator.Location, "Onshore") == 0)
                        generationValue = GetGenerationValue(windGenerator.Generation, GeneratorHelper.GeneratorFactors.ValueFactor.High);

                    existingGenerator.Total += generationValue;
                    return await ValueTask.FromResult(true);
                }
            }
            return await ValueTask.FromResult(false);
        }

        private async ValueTask<bool> AddGenerationOutput(Generator generator)
        {
            if (generator != null)
            {
                var existingGenerator = GetGeneratorTotal(generator.Name);
                if (existingGenerator != null)
                {
                    var generationValue = GetGenerationValue(generator.Generation, GeneratorHelper.GeneratorFactors.ValueFactor.Medium);
                    existingGenerator.Total += generationValue;                 
                    return await ValueTask.FromResult(true);
                }
            }
            return await ValueTask.FromResult(false);
        }

        private GeneratorTotal? GetGeneratorTotal(string generatorName)
        {
            if ( !string.IsNullOrWhiteSpace(generatorName) )
            {
                GeneratorTotal? existingGenerator = null;

                if (_generatorTotals?.Count > 0 )
                    existingGenerator = _generatorTotals.Where(g => string.Compare(g.Name, generatorName, true) == 0)
                                                        .FirstOrDefault();

                if (existingGenerator == null)
                    existingGenerator = new GeneratorTotal() { Name = generatorName };

                _generatorTotals.Add(existingGenerator);
                return existingGenerator;
            }
            return null;
        }

        private decimal GetGenerationValue(Generation generation, decimal factor)
        {
            if (generation != null)
            {
               var  generationValue = generation?.Days?.Sum(d => d.Energy * d.Price * factor);

                return generationValue.GetValueOrDefault();
            }
            return decimal.Zero;
        }

        private async ValueTask<bool> AddActualHeatRate(CoalGenerator coalGenerator)
        {
            if (coalGenerator != null)
            {
                ActualHeatRate? existingHeatRate = null;

                if (_actualHeatRates?.Count > 0)
                    existingHeatRate = _actualHeatRates.Where(h => string.Compare(h.Name, coalGenerator.Name, true) == 0)
                                                       .FirstOrDefault();
                if (existingHeatRate == null)
                {
                    existingHeatRate = new ActualHeatRate() { Name = coalGenerator.Name };
                    _actualHeatRates.Add(existingHeatRate);
                }

                //Assumption : We get distinct Coal generators by name. Otherwise the value will be over-written
                existingHeatRate.HeatRate = Decimal.Divide(coalGenerator.TotalHeatInput, coalGenerator.ActualNetGeneration);

                return await ValueTask.FromResult(true);
            }
            return await ValueTask.FromResult(false);
        }

        private async ValueTask<bool> AddHighestDailyEmission(EmissionGenerator  emissionGenerator, decimal factor)
        {
            if (emissionGenerator != null)
            {
                DayEmission? dayEmission = null;

                var emissions = emissionGenerator?
                                .Generation?
                                .Days?
                                .Select(d => new { EmissionDate = d.Date.Date,
                                                   Emission = d.Energy * factor * emissionGenerator.EmissionsRating
                                                 })
                                .ToList();

                if (emissions?.Count >0)
                {
                    foreach (var emission in emissions)
                    {
                        if (_dayEmissions.Count > 0)
                            dayEmission = _dayEmissions.Where(e => e.Date.Date == emission.EmissionDate).FirstOrDefault();

                        if (dayEmission == null)
                        {
                            dayEmission = new DayEmission()
                            {
                                Date = emission.EmissionDate,
                                Emission = emission.Emission,
                                Name = emissionGenerator.Name
                            };
                            _dayEmissions.Add(dayEmission);
                        }
                        else
                        {
                            if (dayEmission.Emission < emission.Emission)
                            {
                                dayEmission.Emission = emission.Emission;
                                dayEmission.Name = emissionGenerator.Name;
                            }
                        }
                    }
                    return await ValueTask.FromResult(true);
                }
            }
            return await ValueTask.FromResult(false);
        }

        public void Reset()
        {
            _actualHeatRates?.Clear();
            _dayEmissions?.Clear();
            _generatorTotals?.Clear();
        }

        private GenerationOutput GetGenerationOutput()
        {
            return new GenerationOutput()
            {
                ActualHeatRates = _actualHeatRates.ToArray(),
                MaxEmissionGenerators = _dayEmissions.ToArray(),
                Totals = _generatorTotals.ToArray()
            };
        }

        public void WriteToFile(string filename)
        {
            _xmlFile.WriteToXML(GetGenerationOutput(), filename);            
        }
    }
}
