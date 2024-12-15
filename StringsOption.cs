using CsJSONTools;

namespace CsCommandConfig;
public class StringsOption(string name) : IJSONCompatibleOption {
    protected readonly string name = name;

    protected readonly List<string> collected = [];

    public string GetName() => name;

    public void GiveValue(int newPriority, object newValue) {
        if (newValue is string str) {
            collected.Add(str);
        } else if (newValue is IEnumerable<string> strs) {
            collected.AddRange(strs);
        } else {
            throw new NotSupportedException("Can't add this type to a strings option");
        }
    }

    public object GetValue() => collected;

    public IJSONShape GetShape() {
        return new JSONListShape(new JSONStringShape());
    }

    public object ValueFromJSON(ShapedJSON shaped) {
        return shaped.IterList().Select(val => val.GetString());
    }
}
