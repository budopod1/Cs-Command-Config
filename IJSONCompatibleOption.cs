using CsJSONTools;

namespace CsCommandConfig;
public interface IJSONCompatibleOption : IOption {
    public IJSONShape GetShape();
    public object ValueFromJSON(ShapedJSON shaped);
}
