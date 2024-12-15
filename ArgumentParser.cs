using System.Text;

namespace CsCommandConfig;
public class ArgumentParser {
    readonly ProgramConfiguration config;
    readonly string programName;
    readonly string description;
    readonly string helpFooter;
    readonly List<string> helpFlags = [];
    readonly int priority;
    readonly ArgParserBranch root = new();
    ArgParserBranch currentBranch;

    public int HelpCommandIndent { get; set; } = 4;
    public int HelpCommandAlignPag { get; set; } = 13;
    public int HelpFlagIndent { get; set; } = 2;
    public int HelpFlagAlignPad { get; set; } = 25;
    public int HelpPositionalIndent { get; set; } = 2;
    public int HelpPositionalAlignPad { get; set; } = 13;

    public ArgumentParser(ProgramConfiguration config, string programName, string description=null, string helpFooter=null, bool defaultHelpFlags=true, int priority=0) {
        this.config = config;
        this.programName = programName;
        this.description = description;
        this.helpFooter = helpFooter;
        currentBranch = root;
        this.priority = priority;

        if (defaultHelpFlags) {
            AddHelpFlags("help", "h");
        }
    }

    public void AddHelpFlags(params string[] flags) {
        helpFlags.AddRange(flags);
    }

    protected class PositionalArg(IOption option, IArgumentConfigValue configValue, bool isOptional, string help) {
        public readonly IOption Option = option;
        public readonly IArgumentConfigValue ConfigValue = configValue;
        public readonly bool IsOptional = isOptional;
        public readonly string Help = help;

        public string GenerateUsage() {
            string usage = ConfigValue.GenerateUsage(Option);
            return IsOptional ? $"[{usage}]" : usage;
        }

        public void Parse(CmdArguments args, int priority) {
            try {
                Option.GiveValue(priority, ConfigValue.GetValue(args));
            } catch (InvalidOptionValueException e) {
                throw new InvalidCommandArgsException(e.Message);
            }
        }
    }

    protected class BaseFlagArg(IEnumerable<string> names, string help) {
        public readonly IEnumerable<string> Names = names;
        public readonly string Help = help;

        protected virtual string GenerateValueUsage() {
            return "";
        }

        public string GenerateUsage() {
            string valueUsage = GenerateValueUsage();
            return string.Join(",", Names.Select(
                name => (name.Length > 1 ? "--" : "-") + name)
            ) + (valueUsage.Length == 0 ? "" : " " + valueUsage);
        }
    }

    protected class FlagArg(IEnumerable<string> names, IOption option, IArgumentConfigValue configValue, string help) : BaseFlagArg(names, help) {
        public readonly IOption Option = option;
        public readonly IArgumentConfigValue ConfigValue = configValue;

        protected override string GenerateValueUsage() {
            return ConfigValue.GenerateUsage(Option);
        }

        public void Parse(CmdArguments args, int priority) {
            try {
                Option.GiveValue(priority, ConfigValue.GetValue(args));
            } catch (InvalidOptionValueException e) {
                throw new InvalidCommandArgsException(e.Message);
            }
        }
    }

    protected class ArgParserBranch {
        public readonly Dictionary<string, ArgParserBranch> Subbranches = [];
        public readonly List<PositionalArg> Positionals = [];
        public readonly List<FlagArg> Flags = [];
        public string Help = null;

        public string GenerateUsage() {
            if (Positionals.Count == 0) return "";
            StringBuilder usage = new();
            foreach (PositionalArg arg in Positionals) {
                string argUsage = arg.GenerateUsage();
                if (argUsage == "") continue;
                usage.Append(' ');
                usage.Append(argUsage);
            }
            return usage.ToString();
        }
    }

    protected ArgParserBranch FindBranch(IEnumerable<string> path) {
        ArgParserBranch branch = root;
        foreach (string part in path) {
            if (!branch.Subbranches.TryGetValue(part, out ArgParserBranch nextBranch)) {
                nextBranch = new ArgParserBranch();
                branch.Subbranches[part] = nextBranch;
            }
            branch = nextBranch;
        }
        return branch;
    }

    public void Tree(params string[] path) {
        currentBranch = FindBranch(path);
    }

    public void SetBranchHelp(string help) {
        currentBranch.Help = help;
    }

    public void Positional(string optionName, IArgumentConfigValue configValue, string help=null, bool isOptional=false) {
        PositionalArg positionalArg = new(config.RequireOption(optionName), configValue, isOptional, help);
        currentBranch.Positionals.Add(positionalArg);
    }

    public void Flag(IEnumerable<string> flagNames, string optionName, IArgumentConfigValue configValue, string help=null) {
        FlagArg flagArg = new(flagNames, config.RequireOption(optionName), configValue, help);
        currentBranch.Flags.Add(flagArg);
    }

    protected void _Parse(IEnumerable<string> arguments) {
        CmdArguments args = new(arguments);

        ArgParserBranch branch = root;
        List<FlagArg> activeFlags = [..root.Flags];

        bool acceptingFlags = true;

        Stack<PositionalArg> remainingPositionals = new(root.Positionals);

        while (args.Any()) {
            string arg = args.Pop();
            if (arg.Length == 0) continue;

            if (arg[0] == '-' && arg.Length > 1 && acceptingFlags) {
                if (helpFlags.Contains(arg.TrimStart('-'))) {
                    ShowHelpAndExit();
                } if (arg == "--") {
                    acceptingFlags = false;
                } else {
                    IEnumerable<string> namedFlags;
                    if (arg[1] == '-') {
                        namedFlags = [arg[2..]];
                    } else {
                        namedFlags = arg[1..].Split();
                    }
                    foreach (string flagName in namedFlags) {
                        FlagArg flag = activeFlags.FirstOrDefault(
                            flag => flag.Names.Contains(flagName));
                        if (flag == null) {
                            throw new InvalidCommandArgsException(
                                $"Unrecognized option: '{flagName}'"
                            );
                        }
                        flag.Parse(args, priority);
                    }
                }
            } else if (remainingPositionals.Count > 0) {
                args.Add(arg);
                remainingPositionals.Pop().Parse(args, priority);
            } else if (branch.Subbranches.Count > 0) {
                if (branch.Subbranches.TryGetValue(arg, out ArgParserBranch subbranch)) {
                    branch = subbranch;
                    config["branch"] = arg;
                    activeFlags.AddRange(branch.Flags);
                    remainingPositionals = new Stack<PositionalArg>(branch.Positionals);
                } else {
                    throw new InvalidCommandArgsException(
                        $"'{arg}' is not a valid command"
                    );
                }
            } else {
                throw new InvalidCommandArgsException(
                    $"Too many arguments, did not expect argument {arg}"
                );
            }
        }

        if (branch.Subbranches.Count > 0) {
            ShowHelpAndExit();
        }

        PositionalArg remainingRequired = remainingPositionals
            .FirstOrDefault(arg => !arg.IsOptional);
        if (remainingRequired != null) {
            throw new InvalidCommandArgsException(
                $"Not enough arguments, expected {remainingRequired.GenerateUsage()}"
            );
        }
    }

    public void Parse(IEnumerable<string> arguments, bool catchErrs=true) {
        if (catchErrs) {
            try {
                _Parse(arguments);
            } catch (InvalidCommandArgsException e) {
                ErrorAndExit(e.Message);
            }
        } else {
            _Parse(arguments);
        }
    }

    public string GenerateHelpOutput(IEnumerable<string> branchPath) {
        StringBuilder output = new();

        StringBuilder branchesUsage = new();
        List<ArgParserBranch> activeBranches = [root];
        ArgParserBranch branch = root;
        foreach (string branchName in branchPath) {
            branch = branch.Subbranches[branchName];
            activeBranches.Add(branch);
            branchesUsage.Append(' ');
            branchesUsage.Append(branchName);
            branchesUsage.Append(branch.GenerateUsage());
        }

        IEnumerable<FlagArg> flags = activeBranches
            .SelectMany(branch => branch.Flags);
        IEnumerable<PositionalArg> positionals = activeBranches
            .SelectMany(branch => branch.Positionals);

        if (branch.Help != null) {
            output.Append(branch.Help);
            output.Append("\n\n");
        } else if (description != null) {
            output.Append(description);
            output.Append("\n\n");
        }

        output.Append("usage: ");
        output.Append(programName);

        output.Append(branchesUsage);

        bool hasSubbranches = branch.Subbranches.Count > 0;

        if (hasSubbranches) output.Append(" <command>");

        output.Append(" [options]\n");

        if (positionals.Any()) {
            output.Append("\narguments:\n");
            foreach (PositionalArg positional in positionals) {
                output.Append(new string(' ', HelpPositionalIndent));
                string argumentUsage = positional.GenerateUsage();
                if (argumentUsage.Length > HelpPositionalAlignPad && positional.Help != null) {
                    output.Append(argumentUsage);
                    output.Append('\n');
                    int totalIndent = HelpPositionalIndent + HelpPositionalAlignPad + 1;
                    output.Append(new string(' ', totalIndent));
                    output.Append(positional.Help);
                } else {
                    output.Append(argumentUsage.PadRight(HelpPositionalAlignPad));
                    output.Append(' ');
                    output.Append(positional.Help ?? "");
                }
                output.Append('\n');
            }
        }

        if (flags.Any()) {
            output.Append("\noptions:\n");
            BaseFlagArg helpFlag = new(helpFlags, "Show this help");
            IEnumerable<BaseFlagArg> printFlags = flags.Concat([helpFlag]);
            foreach (BaseFlagArg flag in printFlags) {
                output.Append(new string(' ', HelpFlagIndent));
                string flagUsage = flag.GenerateUsage();
                if (flagUsage.Length > HelpFlagAlignPad && flag.Help != null) {
                    output.Append(flagUsage);
                    output.Append('\n');
                    int totalIndent = HelpFlagIndent + HelpFlagAlignPad + 1;
                    output.Append(new string(' ', totalIndent));
                    output.Append(flag.Help);
                } else {
                    output.Append(flagUsage.PadRight(HelpFlagAlignPad));
                    output.Append(' ');
                    output.Append(flag.Help ?? "");
                }
                output.Append('\n');
            }
        }

        if (hasSubbranches) {
            output.Append("\ncommands:\n");
            foreach (KeyValuePair<string, ArgParserBranch> pair in branch.Subbranches) {
                output.Append(new string(' ', HelpCommandIndent));
                output.Append(pair.Key.PadRight(HelpCommandAlignPag));
                output.Append(' ');
                output.Append(pair.Value.Help ?? "");
                output.Append('\n');
            }
        }

        if (helpFooter != null) {
            output.Append('\n');
            output.Append(helpFooter);
            output.Append('\n');
        }

        return output.ToString();
    }

    public void ShowHelpAndExit() {
        Console.Write(GenerateHelpOutput(config["branch"]));
        Environment.Exit(0);
    }

    public void ErrorAndExit(string error) {
        Console.WriteLine($"{programName}: {error}");
        if (helpFlags.Count > 0) {
            string between = "";
            if (config["branch"].Count > 0) between = " <command>";
            string helpFlag = helpFlags[0];
            helpFlag = (helpFlag.Length == 1 ? "-" : "--") + helpFlag;
            Console.WriteLine($"use '{programName}{between} {helpFlag}' to view usage");
        }
        Environment.Exit(1);
    }
}
