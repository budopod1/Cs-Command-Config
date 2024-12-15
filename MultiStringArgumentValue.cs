namespace CsCommandConfig;
public class MultiStringArgumentValue(string terminator = null) : IArgumentConfigValue {
    readonly string terminator = terminator;

    public string GenerateUsage(IOption option) {
        string usage = $"<{option.GetName()}>...";
        if (terminator != null) usage += " " + terminator;
        return usage;
    }

    public object GetValue(CmdArguments args) {
        if (terminator == null) {
            List<string> result = [];
            while (args.Any() && !args.Peek().StartsWith('-')) {
                result.Add(args.Pop());
            }
            return result;
        } else {
            List<string> result = [];
            while (args.Any()) {
                string nxt = args.Pop();
                if (nxt == terminator) return result;
                result.Add(nxt);
            }
            throw new InvalidCommandArgsException(
                $"Expected terminator {terminator}"
            );
        }
    }
}
