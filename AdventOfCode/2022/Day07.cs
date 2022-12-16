using System;
using System.Collections.Generic;
using System.Linq;
using AdventOfCode.Utils;
using ProblemsLibrary;

namespace AdventOfCode._2022;

[Problem("2022-07-01", MethodName = nameof(ExecutePart1))]
[Problem("2022-07-02", MethodName = nameof(ExecutePart2))]
public class Day07
{
    private const string TestData = @"$ cd /
$ ls
dir a
14848514 b.txt
8504156 c.dat
dir d
$ cd a
$ ls
dir e
29116 f
2557 g
62596 h.lst
$ cd e
$ ls
584 i
$ cd ..
$ cd ..
$ cd d
$ ls
4060174 j
8033020 d.log
5626152 d.ext
7214296 k";

    private interface ICommand
    {
        public void Execute(ref Navigator n);
    }
    private sealed record CdUpCommand : ICommand
    {
        public void Execute(ref Navigator n)
        {
            n = n.Parent!;
        }
    }

    private sealed record CdRootCommand : ICommand
    {
        public void Execute(ref Navigator n)
        {
            n = n.Root;
        }
    }

    private sealed record CdCommand(string Target) : ICommand
    {
        public void Execute(ref Navigator n)
        {
            n = n.ChangeDirectory(Target);
        }
    }

    private sealed record LsCommand(IEnumerable<ILsEntry> Entries) : ICommand
    {
        public void Execute(ref Navigator n)
        {
            n.Current.AddLsEntries(Entries);
        }
    }

    private interface ILsEntry
    {
        string Name { get; }
        INode ToNode();
    }
    private sealed record LsEntryFile(int Size, string Name) : ILsEntry
    {
        public INode ToNode() => new FileNode(Size, Name);
    }
    private sealed record LsEntryDirectory(string Name) : ILsEntry
    {
        public INode ToNode() => DirectoryNode.CreateEmpty(Name);
    }

    private interface INode
    {
        int Size { get; }
        (int, int) SumOfDirectoriesNotBiggerThan(int size);
        int SizeOfSmallestDirectoryAtLeast(int size);
    }
    private sealed record DirectoryNode(Dictionary<string, INode> Children, string Name) : INode
    {
        public static DirectoryNode CreateEmpty(string name) => new(new Dictionary<string, INode>(), name);
        public void AddLsEntries(IEnumerable<ILsEntry> entries)
        {
            foreach (var entry in entries)
                if (!Children.ContainsKey(entry.Name))
                    Children[entry.Name] = entry.ToNode();
        }
        public DirectoryNode GetChildDirectory(string name)
        {
            if (!Children.TryGetValue(name, out var dir))
                dir = Children[name] = DirectoryNode.CreateEmpty(name);
            return (DirectoryNode)dir;
        }

        private int _size = -1;
        public int Size => _size >= 0 ? _size : (_size = Children.Values.Sum(x => x.Size));
        public (int, int) SumOfDirectoriesNotBiggerThan(int size)
        {
            var exactSize = 0;
            var sum = 0;
            foreach (var child in Children.Values)
            {
                var (subSum, subExactSize) = child.SumOfDirectoriesNotBiggerThan(size);
                exactSize += subExactSize;
                sum += subSum;
            }
            if (exactSize < size)
                sum += exactSize;
            return (sum, exactSize);
        }
        public int SizeOfSmallestDirectoryAtLeast (int size)
        {
            if (Size < size)
                return int.MaxValue;
            var minChild = Children.Values.Min(c => c.SizeOfSmallestDirectoryAtLeast(size));
            return Math.Min(minChild, Size);
        }
    }
    private sealed record FileNode(int Size, string Name) : INode
    {
        public (int, int) SumOfDirectoriesNotBiggerThan(int size) => (0, Size);
        public int SizeOfSmallestDirectoryAtLeast(int size) => int.MaxValue;
    }

    class Navigator
    {
        public static Navigator CreateRoot(DirectoryNode current) => new(current, null);
        private Navigator(DirectoryNode current, Navigator? parent)
        {
            Current = current;
            Parent = parent;
        }

        public DirectoryNode Current { get; }
        public Navigator? Parent { get; }
        public Navigator ChangeDirectory(string name) => new(Current.GetChildDirectory(name), this);
        public Navigator Root => Parent?.Root ?? this;
        public override string ToString() => Parent != null ? $"{Parent}/{Current.Name}" : Current.Name;
    }

    [TestCase(TestData, 95437)]
    public static int ExecutePart1(string input)
    {
        var rootDir = ReadInput(input);
        return rootDir.SumOfDirectoriesNotBiggerThan(100000).Item1;
    }

    private static Parser<ICommand> CreateCommandParser()
    {
        var cdCommand = Parser.FormattedString($"$ cd {Parser.Line}", m => (ICommand)(
            m[0] == ".." ? new CdUpCommand() :
            m[0] == "/" ? new CdRootCommand() :
            new CdCommand(m[0])));
        var lsResultLine = Parser.OneOf(
            Parser.FormattedString($"dir {Parser.Line}", m => (ILsEntry)new LsEntryDirectory(m[0])),
            Parser.FormattedString($"{Parser.Int32} {Parser.Line}", m => (ILsEntry)new LsEntryFile(m[0], m[1])));
        var lsCommand =
            from cmd in Parser.FormattedString($"$ ls{Parser.Line}")
            from entries in lsResultLine.Repeat()
            select (ICommand)new LsCommand(entries);
        return Parser.OneOf(cdCommand, lsCommand);
    }

    private static DirectoryNode ReadInput(string input)
    {
        var parserCommand = CreateCommandParser();
        var rootDir = DirectoryNode.CreateEmpty("");
        var navigator = Navigator.CreateRoot(rootDir);
        foreach (var cmd in parserCommand.ParseRepeated(input))
            cmd.Execute(ref navigator);
        return rootDir;
    }
    [TestCase(TestData, 24933642)]
    public static int ExecutePart2(string input)
    {
        var rootDir = ReadInput(input);
        return rootDir.SizeOfSmallestDirectoryAtLeast(30000000 - (70000000 - rootDir.Size));
    }
}