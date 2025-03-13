using AutoBogus;

namespace Sanatana.DataGenerator.AutoBogus.AutoGeneratorOverrides
{
    public class UtcDateTimeGeneratorOverride : AutoGeneratorOverride
    {
        public override bool CanOverride(AutoGenerateContext context)
        {
            return context.GenerateType == typeof(DateTime);
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            var dateTime = context.Faker.Date.Recent();
            context.Instance = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }
}
