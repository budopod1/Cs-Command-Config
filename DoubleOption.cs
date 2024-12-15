using CsJSONTools;

namespace CsCommandConfig;
public class DoubleOption(string name, double default_ = 0) : GenericOption<double>(name, default_), IJSONCompatibleOption {
    public IJSONShape GetShape() {
        return new JSONDoubleShape();
    }

    public object ValueFromJSON(ShapedJSON shaped) {
        return shaped.GetDouble();
    }
}
