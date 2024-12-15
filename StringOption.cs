using CsJSONTools;

namespace CsCommandConfig;
public class StringOption(string name, string default_ = null) : GenericOption<string>(name, default_), IJSONCompatibleOption {
    public IJSONShape GetShape() {
        return new JSONStringShape();
    }

    public object ValueFromJSON(ShapedJSON shaped) {
        return shaped.GetString();
    }
}
