using CsJSONTools;

namespace CsCommandConfig;
public class JSONConfigParser(ProgramConfiguration config, int priority = 0) {
    readonly ProgramConfiguration config = config;
    readonly int priority = priority;

    IJSONShape GetShape() {
        Dictionary<string, IJSONShape> keysShape = [];
        foreach (IOption option in config.GetOptions()) {
            if (option is IJSONCompatibleOption jsonOption) {
                keysShape[option.GetName()] = jsonOption.GetShape();
            }
        }
        return new JSONObjectOptionalShape(keysShape);
    }

    public void Parse(string file, Action<string> useFileText) {
        IJSONValue jsonValue = JSONTools.ParseJSONFile(file, useFileText);
        ShapedJSON shaped = new(jsonValue, GetShape());
        foreach (KeyValuePair<string, ShapedJSON> pair in shaped.IterObject()) {
            IJSONCompatibleOption option = (IJSONCompatibleOption)config.GetOption(pair.Key);
            try {
                option.GiveValue(priority, option.ValueFromJSON(pair.Value));
            } catch (InvalidOptionValueException e) {
                throw new InvalidJSONException(e.Message, pair.Value.GetJSON());
            }
        }
    }

    public void Parse(string file) {
        Parse(file, text => {});
    }
}
