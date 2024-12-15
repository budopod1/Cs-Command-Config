namespace CsCommandConfig;
public interface IArgumentConfigValue {
    object GetValue(CmdArguments args);
    string GenerateUsage(IOption option);
}
