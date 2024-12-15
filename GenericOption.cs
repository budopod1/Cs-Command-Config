namespace CsCommandConfig;
public abstract class GenericOption<T> : IOption {
    protected readonly string name;

    int priority = -1;
    protected T value;

    public GenericOption(string name, T default_ = default) {
        this.name = name;
        value = ProcessValue(default_);
    }

    protected virtual T ProcessValue(T val) {
        return val;
    }

    public string GetName() => name;

    public void GiveValue(int newPriority, object newValue) {
        if (newPriority > priority) {
            value = ProcessValue((T)newValue);
            priority = newPriority;
        }
    }

    public object GetValue() => value;
}
