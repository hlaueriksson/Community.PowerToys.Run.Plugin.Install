namespace Generator.Models
{
    public class Plugin
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Website { get; set; }
        public Repository? Repository { get; set; }
        public Release? Release { get; set; }

        public IEnumerable<string> Validate()
        {
            return
                Validate(this)
                .Concat(Validate(Repository))
                .Concat(Repository?.Validate() ?? [])
                .Concat(Validate(Release))
                .Concat(Release?.Validate() ?? [])
                .Concat(Release?.Assets.SelectMany(Validate) ?? []);

            static IEnumerable<string> Validate(object? model)
            {
                if (model == null)
                {
                    yield break;
                }

                foreach (var property in model.GetType().GetProperties())
                {
                    var value = property.GetValue(model, null);

                    if (value == null)
                    {
                        yield return $"{property.Name} missing";
                    }
                }
            }
        }
    }
}
