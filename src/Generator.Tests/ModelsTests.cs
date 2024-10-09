using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace Generator.Tests
{
    public class ModelsTests
    {
        [Test]
        public Task Verify_Awesome()
        {
            return Verify(Diff("Awesome"));
        }

        [Test]
        public Task Verify_Plugin()
        {
            return Verify(Diff("Plugin"));
        }

        [Test]
        public Task Verify_Release()
        {
            return Verify(Diff("Release"));
        }

        [Test]
        public Task Verify_Repository()
        {
            return Verify(Diff("Repository"));
        }

        private static string Diff(string model)
        {
            var generator = File.ReadAllText(@$"..\..\..\..\Generator\Models\{model}.cs");
            var plugin = File.ReadAllText(@$"..\..\..\..\Community.PowerToys.Run.Plugin.Install\Models\{model}.cs");
            var diff = InlineDiffBuilder.Diff(generator, plugin);

            var sb = new StringBuilder();
            foreach (var line in diff.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        sb.Append("+ ");
                        break;
                    case ChangeType.Deleted:
                        sb.Append("- ");
                        break;
                    default:
                        sb.Append("  ");
                        break;
                }
                sb.AppendLine(line.Text);
            }
            return sb.ToString();
        }
    }
}
