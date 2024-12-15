using CsJSONTools;

namespace CsCommandConfig;
public class EnumOption(string name, IEnumerable<string> members, string default_ = null) : GenericOption<string>(name, default_?.ToLower()), IJSONCompatibleOption {
    readonly IEnumerable<string> members = members
        .Select(member => member.ToLower());

    public EnumOption(string name, Type enumType, string default_) :
        this(name, Enum.GetNames(enumType), default_) {}

    protected override string ProcessValue(string val) {
        string lower = val.ToLower();
        if (!members.Contains(lower)) {
            throw new InvalidOptionValueException(
                $"'{val}' is not a listed as a member of the enum",
                name, val
            );
        }
        return lower;
    }

    public T ToEnum<T>() where T : struct {
        if (Enum.TryParse(value.ToUpper(), out T result)) {
            return result;
        }
        throw new ArgumentException($"Can't interpret '{value}' as a member of the specified enum");
    }

    public IEnumerable<string> GetMembers() {
        return members;
    }

    public static T EnumProp<T>(ProgramConfiguration config, string optionName) where T : struct {
        return ((EnumOption)config.GetOption(optionName)).ToEnum<T>();
    }

    public IJSONShape GetShape() {
        return new JSONStringOptionsShape(members);
    }

    public object ValueFromJSON(ShapedJSON shaped) {
        return shaped.GetString();
    }
}
