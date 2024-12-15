using System.Runtime.InteropServices;

namespace CsCommandConfig;
public class IntArgumentValue : IArgumentConfigValue {
    public string GenerateUsage(IOption option) {
        return $"<{option.GetName()}>";
    }

    public object GetValue(CmdArguments args) {
        string arg = args.Pop();
        if (int.TryParse(arg, out int result)) {
            return result;
        }
        throw new InvalidCommandArgsException(
            $"'{arg}' is not a valid integer"
        );
    }
}
