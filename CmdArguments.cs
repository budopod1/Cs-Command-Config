namespace CsCommandConfig;
public class CmdArguments(IEnumerable<string> arguments) {
    readonly Stack<string> arguments = new(arguments.Reverse());

    public string Peek(string requireError=null) {
        if (arguments.Count == 0) {
            throw new InvalidCommandArgsException(
                requireError ?? "Expected another argument"
            );
        }
        return arguments.Peek();
    }

    public string Pop(string requireError=null) {
        if (arguments.Count == 0) {
            throw new InvalidCommandArgsException(
                requireError ?? "Expected another argument"
            );
        }
        return arguments.Pop();
    }

    public void Add(string argument) {
        arguments.Push(argument);
    }

    public bool Any() {
        return arguments.Count > 0;
    }
}
