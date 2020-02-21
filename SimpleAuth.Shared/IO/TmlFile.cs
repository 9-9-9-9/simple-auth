using System;
using System.Collections.Generic;
using System.Linq;
using SimpleAuth.Shared.Extensions;

namespace SimpleAuth.Shared.IO
{
    public static class TmlFile
    {
        public static TmlNode Parse(IEnumerable<string> lines)
        {
            var root = new TmlNode
            {
                Content = null
            };

            var rootByLevelDict = new Dictionary<int, TmlNode>()
            {
                {0, root}
            };

            var lineNumber = 1;
            var lastLvl = 0;

            foreach (var line in lines)
            {
                if (!line.IsBlank())
                {
                    var lvl = GetLevel(line);

                    if (lvl - lastLvl > 1)
                        throw new ArgumentException($"Line {lineNumber} has invalid padding");

                    lastLvl = lvl;

                    var node = new TmlNode
                    {
                        Content = line.Trim()
                    };

                    GetRootLevel(lvl - 1).ChildrenNodes.Add(node);

                    if (rootByLevelDict.ContainsKey(lvl))
                        rootByLevelDict[lvl] = node;
                    else
                        rootByLevelDict.Add(lvl, node);
                }

                lineNumber++;
            }

            return root;

            TmlNode GetRootLevel(int level) => rootByLevelDict.TryGetValue(level, out var rootByLevel) ? rootByLevel : null;

            int GetLevel(string line)
            {
                var trimmedStart = line.TrimStart();
                var trimmedCharsCnt = line.Length - trimmedStart.Length;
                var lvl = trimmedCharsCnt + 1;

                if (lvl == 1) return 1;

                var trimmedChars = line.Substring(0, trimmedCharsCnt);
                
                if (trimmedChars.ToCharArray().Any(x => x != '\t'))
                    throw new ArgumentException($"Line {lineNumber} has invalid tabs at the beginning");
                
                return lvl;
            }
        }

        public class TmlNode
        {
            public List<TmlNode> ChildrenNodes { get; set; } = new List<TmlNode>();
            public bool HasChildNode => ChildrenNodes.IsAny();
            public string Content { get; set; }

            public TmlNode SingleOrDefault(Func<TmlNode, bool> expression)
            {
                return ChildrenNodes.SingleOrDefault(expression);
            }

            public TmlNode SingleOrDefault(string content, bool ignoreCase = true)
            {
                if (ignoreCase)
                    return SingleOrDefault(x => x.Content?.ToLowerInvariant() == content?.ToLowerInvariant());
                return SingleOrDefault(x => x.Content == content);
            }

            public TmlNode FirstOrDefault(Func<TmlNode, bool> expression)
            {
                return ChildrenNodes.FirstOrDefault(expression);
            }

            public TmlNode FirstOrDefault(string content, bool ignoreCase = true)
            {
                if (ignoreCase)
                    return FirstOrDefault(x => x.Content?.ToLowerInvariant() == content?.ToLowerInvariant());
                return FirstOrDefault(x => x.Content == content);
            }

            public IEnumerable<TmlNode> Where(Func<TmlNode, bool> expression)
            {
                return ChildrenNodes.Where(expression);
            }

            public IEnumerable<TmlNode> Where(string content, bool ignoreCase = true)
            {
                if (ignoreCase)
                    return Where(x => x.Content?.ToLowerInvariant() == content?.ToLowerInvariant());
                return Where(x => x.Content == content);
            }
        }
    }
}