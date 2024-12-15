namespace CsCommandConfig;
public interface IOption {
    string GetName();
    void GiveValue(int priority, object value);
    object GetValue();
}
