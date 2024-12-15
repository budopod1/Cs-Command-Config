namespace CsCommandConfig;
public class ConstArgumentValue<T>(T value) : IArgumentConfigValue {
    readonly T value = value;

    public string GenerateUsage(IOption option) {
        return "";
    }

    public object GetValue(CmdArguments args) {
        return value;
    }
}
