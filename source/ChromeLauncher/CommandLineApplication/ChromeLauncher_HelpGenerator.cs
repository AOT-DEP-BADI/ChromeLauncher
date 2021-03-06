﻿using System;
using System.Linq;
using System.Text;
using CLAP;

namespace ChromeLauncher.CommandLineApplication
{
    public class ChromeLauncher_HelpGenerator : HelpGeneratorBase
    {
        /// <summary>
        /// Add blank before a given string
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string AddBlankInString(int number)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < number; i++)
            {
                sb.Append(" ");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Override Help 
        /// </summary>
        /// <param name="helpInfo"></param>
        /// <returns></returns>
        protected override string GetHelpString(CLAP.HelpInfo helpInfo)
        {
            const string verbsLead = "  ";
            const string parametersLead = "     ";
            const string validationsLead = "      ";

            var sb = new StringBuilder();
            sb.Append("Commandes:");

            var count = helpInfo.Parsers.Count;
            var multi = count > 1;
            StringBuilder sb_temp_verb = new StringBuilder();
            StringBuilder sb_temp_parameter = new StringBuilder();
            int length_verb = 0;
            for (int i = 0; i < count; i++)
            {
                var parser = helpInfo.Parsers[i];

                foreach (var verb in parser.Verbs.OrderByDescending(v => v.IsDefault))
                {
                    sb.AppendLine();
                    sb_temp_verb.Clear();
                    sb_temp_verb.Append(verbsLead);
                    if (multi)
                    {
                        sb_temp_verb.AppendFormat("{0}.", parser.Type.Name.ToLowerInvariant());
                    }
                    sb_temp_verb.Append(verb.Names.StringJoin("|").ToLowerInvariant());
                    length_verb = sb_temp_verb.Length + 1;
                    if (verb.IsDefault)
                    {
                        sb_temp_verb.Append(" (défaut)");
                    }

                    if (!string.IsNullOrEmpty(verb.Description))
                    {
                        sb_temp_verb.AppendFormat(": {0}", verb.Description);
                    }
                    sb.Append(sb_temp_verb.ToString().Replace("\n", "\n" + AddBlankInString(length_verb)));

                    sb.AppendLine();
                    if (verb.Parameters.Any())
                    {
                        var longestParameter = verb.Parameters.Max(p => p.Names.StringJoin(" -").Length);
                        var longestType = verb.Parameters.Max(p => p.Type.GetGenericTypeName().Length);

                        foreach (var p in verb.Parameters.OrderBy(p => p.Names.First()))
                        {
                            sb_temp_parameter.Clear();
                            //sb_temp_parameter.Append(parametersLead);
                            sb_temp_parameter.AppendFormat("-{0} : ", p.Names.StringJoin(" -").ToLowerInvariant().PadRight(longestParameter, ' '));

                            if (!string.IsNullOrEmpty(p.Description))
                            {
                                sb_temp_parameter.AppendFormat("{0} ", p.Description);
                            }

                            var typeName = GetTypeName(p.Type);

                            if (!string.IsNullOrEmpty(typeName))
                            {
                                sb_temp_parameter.AppendFormat("({0}) ", typeName);
                            }

                            if (p.Required)
                            {
                                sb_temp_parameter.Append("(Requis) ");
                            }

                            if (p.Separator != null && p.Separator != SeparatorAttribute.DefaultSeparator)
                            {
                                sb_temp_parameter.AppendFormat("(Separator = {0}) ", p.Separator);
                            }

                            if (p.Default != null)
                            {
                                sb_temp_parameter.AppendFormat("(Defaut = {0}) ", p.Default);
                            }

                            if (p.Validations.Any())
                            {
                                sb_temp_parameter.AppendFormat("({0}) ", p.Validations.StringJoin(", "));
                            }

                            
                            
                            sb.Append(sb_temp_parameter.ToString());
                            sb.AppendLine();
                        } // foreach (var p in verb.Parameters
                    }

                    if (verb.Validations.Any())
                    {
                        sb.AppendLine();
                        sb.Append(parametersLead);
                        sb.AppendLine("Validation:");

                        foreach (var v in verb.Validations)
                        {
                            sb.Append(validationsLead);
                            sb.AppendLine(v);
                        }
                    }

                } // foreach (var verb in parser.Verbs

                if (parser.Globals.Any())
                {
                    sb.AppendLine();
                    //sb.Append(verbsLead);
                    sb.AppendLine("Paramètres globaux:");

                    var longestGlobal = parser.Globals.Max(p => p.Names.StringJoin("|").Length);

                    foreach (var g in parser.Globals.OrderBy(g => g.Names.First()))
                    {
                        sb.Append(parametersLead);
                        sb.AppendFormat("/{0} : ",
                            g.Names.StringJoin("|").ToLowerInvariant().PadRight(longestGlobal, ' '));

                        if (!string.IsNullOrEmpty(g.Description))
                        {
                            sb.AppendFormat("{0} ", g.Description);
                        }

                        var typeName = GetTypeName(g.Type);

                        if (!string.IsNullOrEmpty(typeName))
                        {
                            sb.AppendFormat("({0}) ", typeName);
                        }

                        if (g.Separator != null && g.Separator != SeparatorAttribute.DefaultSeparator)
                        {
                            sb.AppendFormat("(Separator = {0}) ", g.Separator);
                        }

                        if (g.Validations != null && g.Validations.Any())
                        {
                            sb.AppendFormat("({0}) ", g.Validations.StringJoin(", "));
                        }

                        sb.AppendLine();
                    } // foreach (var g in parser.Globals
                }


                if (multi && i < count - 1)
                {
                    sb.AppendLine();
                    sb.Append(verbsLead);
                    sb.AppendLine(string.Empty.PadRight(80, '-'));
                }
            }
            return sb.ToString();
        }


        private static string GetTypeName(Type type)
        {
            if (type.IsEnum)
            {
                return string.Format("{0} ({1})", type.Name, string.Join("/", Enum.GetNames(type)));
            }

            if (type == typeof(bool))
            {
                return string.Empty;
            }

            return type.GetGenericTypeName();
        }
    }
}
