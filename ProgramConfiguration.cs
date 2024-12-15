namespace CsCommandConfig;
public class ProgramConfiguration {
    public dynamic this[string key] {
        get => RequireOption(key).GetValue();
        set => RequireOption(key).GiveValue(int.MaxValue, value);
    }

    readonly List<IOption> options = [];

    public void AddOption(IOption option) {
        IOption duplicateOf = options.FirstOrDefault(
            other => other.GetName() == option.GetName());
        if (duplicateOf != null) {
            throw new ArgumentException("Duplicate option names are not allowed");
        }
        options.Add(option);
    }

    public IOption GetOption(string optionName) {
        return options.FirstOrDefault(
            option => option.GetName() == optionName);
    }

    public IEnumerable<IOption> GetOptions() {
        return options;
    }

    public IOption RequireOption(string optionName) {
        IOption option = GetOption(optionName);
        if (option == null) {
            throw new KeyNotFoundException($"Option '{optionName}' not found");
        }
        return option;
    }
}
