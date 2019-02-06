using QA.DotNetCore.Engine.Persistent.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace QA.Engine.Administration.Data.Core
{
    public static class NetNameQueryAnalyzerExtantion
    {
        public static string PrepareQueryExtabtion(this INetNameQueryAnalyzer netNameQueryAnalyzer, IMetaInfoRepository metaInfoRepository, string netNameQuery, int siteId)
        {
            //вычленим из запроса токены с указанными netname таблиц и полей
            //таблицы указываются как [|tableNetName|]
            //столюцы указываются как [|tableNetName.columnNetName|]
            var regexp = new Regex(@"\[\|[a-zA-Z0-9]+(\.[a-zA-Z0-9]+)?\|\]");
            var matches = regexp.Matches(netNameQuery);

            var columnsByTables = new Dictionary<string, List<string>>();

            foreach (var m in matches)
            {
                var token = m.ToString().Replace("[|", "").Replace("|]", "");
                if (token.Contains("."))
                {
                    var tokens = token.Split('.');
                    var tableNetName = tokens[0];
                    if (!columnsByTables.ContainsKey(tableNetName))
                        columnsByTables[tableNetName] = new List<string>();

                    var columnNetName = tokens[1];
                    if (!columnsByTables[tableNetName].Contains(columnNetName))
                        columnsByTables[tableNetName].Add(columnNetName);
                }
                else
                {
                    if (!columnsByTables.ContainsKey(token))
                        columnsByTables[token] = new List<string>();
                }
            }

            //заменим в запросе netname на настоящие названия таблиц и столбцов в базе
            var sb = new StringBuilder(netNameQuery, netNameQuery.Length * 2);
            foreach (var tableNetName in columnsByTables.Keys)
            {
                var contentMetaInfo = metaInfoRepository.GetContent(tableNetName, siteId);
                if (contentMetaInfo == null)
                {
                    throw new Exception($"Content with netname '{tableNetName}' was not found for site {siteId}");
                }

                var tableNetNameValue = $"content_{contentMetaInfo.ContentId}_united";

                sb.Replace($"|{tableNetName}|", tableNetNameValue);

                foreach (var columnNetName in columnsByTables[tableNetName])
                {
                    var contentAttribute = contentMetaInfo.ContentAttributes.FirstOrDefault(ca => ca.NetName == columnNetName);
                    if (contentAttribute == null)
                    {
                        throw new Exception($"Content attribute with netname '{columnNetName}' was not found for table '{tableNetName}' and site {siteId}");
                    }

                    sb.Replace($"|{tableNetName}.{columnNetName}|", contentAttribute.ColumnName);
                }
            }

            return sb.ToString();
        }
    }
}
