using System.Collections.Generic;
using System.Linq;

namespace UWUVCI_AIO_WPF.Modules.N64Config
{
    public class IniDocument
    {
        public List<IniSection> Sections { get; } = new List<IniSection>();

        public IniSection GetOrAddSection(string name)
        {
            var s = Sections.FirstOrDefault(x => x.Name == name);
            if (s == null)
            {
                s = new IniSection(name);
                Sections.Add(s);
            }
            return s;
        }

        public IniSection? GetSection(string name) => Sections.FirstOrDefault(x => x.Name == name);

        public void RemoveSection(string name)
        {
            var s = GetSection(name);
            if (s != null) Sections.Remove(s);
        }

        public void PruneEmptySections()
        {
            Sections.RemoveAll(s => s.Properties.Count == 0);
        }
    }

    public class IniSection
    {
        public string Name { get; }
        public List<IniProperty> Properties { get; } = new List<IniProperty>();

        public IniSection(string name)
        {
            Name = name;
        }

        public string? Get(string key)
        {
            return Properties.FirstOrDefault(p => p.Key == key)?.Value;
        }

        public void Set(string key, string? value)
        {
            var prop = Properties.FirstOrDefault(p => p.Key == key);
            if (value == null)
            {
                if (prop != null) Properties.Remove(prop);
                return;
            }
            if (prop == null)
                Properties.Add(new IniProperty(key, value));
            else
                prop.Value = value;
        }
    }

    public class IniProperty
    {
        public string Key { get; }
        public string Value { get; set; }

        public IniProperty(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
