using CsJSONTools;

namespace CsCommandConfig;
public class IntOption(string name, int default_ = 0) : GenericOption<int>(name, default_), IJSONCompatibleOption {
    public IJSONShape GetShape() {
        return new JSONDoubleShape();
    }

    public object ValueFromJSON(ShapedJSON shaped) {
        return shaped.GetInt();
    }
}
