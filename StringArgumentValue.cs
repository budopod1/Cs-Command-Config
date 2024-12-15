namespace CsCommandConfig;
public class StringArgumentValue : IArgumentConfigValue {
    public string GenerateUsage(IOption option) {
        if (option is EnumOption enumOption) {
            return string.Join('|', enumOption.GetMembers());
        } else {
            return $"<{option.GetName()}>";
        }
    }

    public object GetValue(CmdArguments args) {
        return args.Pop();
    }
}
