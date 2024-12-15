using CsJSONTools;

namespace CsCommandConfig;
public class BoolOption(string name, bool default_ = false) : GenericOption<bool>(name, default_), IJSONCompatibleOption {
    public IJSONShape GetShape() {
        return new JSONBoolShape();
    }

    public object ValueFromJSON(ShapedJSON shaped) {
        return shaped.GetBool();
    }
}
