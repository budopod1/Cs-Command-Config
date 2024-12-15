namespace CsCommandConfig;
public class InvalidOptionValueException(string message, string optionName, object providedValue) : Exception(message) {
    public readonly string optionName = optionName;
    public readonly object providedValue = providedValue;
}
