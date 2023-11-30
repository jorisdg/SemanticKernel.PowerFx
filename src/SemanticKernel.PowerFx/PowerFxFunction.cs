using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Types;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Orchestration;

namespace SemanticKernel.PowerFx
{
    public class PowerFxFunction : ISKFunction
    {
        RecalcEngine engine;

        public string Expression { get; protected set; }

        public bool HasSideEffects { get; protected set; }

        public List<ParameterView> Parameters { get; protected set; }

        public string Name { get; protected set; }

        public string PluginName { get; protected set; }

        public string Description { get; protected set; }

        #region Obsolete
        public AIRequestSettings RequestSettings { get; protected set; } = new AIRequestSettings();
        public bool IsSemantic => false;

        public string SkillName => PluginName;

        public IEnumerable<AIRequestSettings> ModelSettings { get; set; }
        #endregion

        public PowerFxFunction(RecalcEngine engine, string expression, bool hasSideEffects, string name, string skillName, string description, List<ParameterView> parameters)
        {
            this.engine = engine;
            Expression = expression;
            HasSideEffects = hasSideEffects;
            Name = name;
            PluginName = skillName;
            Description = description;
            Parameters = (parameters == null || parameters.Count <= 0) ? DefaultParameterList() : parameters;
        }

        public FunctionView Describe()
        {
            var view = new FunctionView(Name, PluginName, Description)
            {
                Parameters = this.Parameters
            };

            return view;
        }

        public async Task<FunctionResult> InvokeAsync(SKContext context, AIRequestSettings requestSettings = null, CancellationToken cancellationToken = default)
        {
            var newContext = context.Clone();
            var result = new FunctionResult(Name, PluginName, newContext);

            if (context != null)
            {
                var exprVariables = new List<NamedValue>();
                var contextVarEnumerator = context.Variables.GetEnumerator();
                while(contextVarEnumerator.MoveNext())
                {
                    exprVariables.Add(new NamedValue(contextVarEnumerator.Current.Key, FormulaValue.New(contextVarEnumerator.Current.Value)));
                }

                var exprContext = RecordValue.NewRecordFromFields(exprVariables);

                var exprResult = await engine.EvalAsync(Expression, cancellationToken, exprContext, new ParserOptions() { AllowsSideEffects = HasSideEffects });

                newContext.Variables.Set("INPUT", PrintResult(exprResult));
            }

            return result;
        }

        #region obsolete
        public ISKFunction SetAIConfiguration(AIRequestSettings settings)
        {
            // obsolete function
            return this;
        }

        public ISKFunction SetAIService(Func<ITextCompletion> serviceFactory)
        {
            // obsolete function
            return this;
        }

        public ISKFunction SetDefaultSkillCollection(IReadOnlyFunctionCollection functions)
        {
            // obsolete function
            return this;
        }

        public ISKFunction SetDefaultFunctionCollection(IReadOnlyFunctionCollection functions)
        {
            // obsolete function
            return this;
        }
        #endregion

        static private List<ParameterView> DefaultParameterList()
        {
            return new List<ParameterView>() { new ParameterView(Name: "input", Description: "Input string", DefaultValue: "") };
        }

        // Function from Power Fx REPL
        public static string PrintResult(FormulaValue value, bool minimal = false)
        {
            string resultString;

            if (value is BlankValue)
            {
                resultString = minimal ? string.Empty : "Blank()";
            }
            else if (value is ErrorValue errorValue)
            {
                resultString = minimal ? "<error>" : "<Error: " + errorValue.Errors[0].Message + ">";
            }
            else if (value is UntypedObjectValue)
            {
                resultString = minimal ? "<untyped>" : "<Untyped: Use Value, Text, Boolean, or other functions to establish the type>";
            }
            else if (value is StringValue str)
            {
                resultString = minimal ? str.Value : str.ToExpression();
            }
            else if (value is RecordValue record)
            {
                if (minimal)
                {
                    resultString = "<record>";
                }
                else
                {
                    var separator = string.Empty;
                    resultString = "{";
                    foreach (var field in record.Fields)
                    {
                        resultString += separator + $"{field.Name}:";
                        resultString += PrintResult(field.Value);
                        separator = ", ";
                    }

                    resultString += "}";
                }
            }
            else if (value is TableValue table)
            {
                if (minimal)
                {
                    resultString = "<table>";
                }
                else
                {
                    var columnCount = 0;
                    foreach (var row in table.Rows)
                    {
                        if (row.Value != null)
                        {
                            columnCount = Math.Max(columnCount, row.Value.Fields.Count());
                            break;
                        }
                    }

                    if (columnCount == 0)
                    {
                        return minimal ? string.Empty : "Table()";
                    }

                    var columnWidth = new int[columnCount];

                    foreach (var row in table.Rows)
                    {
                        if (row.Value != null)
                        {
                            var column = 0;
                            foreach (var field in row.Value.Fields)
                            {
                                columnWidth[column] = Math.Max(columnWidth[column], PrintResult(field.Value, true).Length);
                                column++;
                            }
                        }
                    }

                    // special treatment for single column table named Value
                    if (columnWidth.Length == 1 && table.Rows.First().Value != null && table.Rows.First().Value.Fields.First().Name == "Value")
                    {
                        var separator = string.Empty;
                        resultString = "[";
                        foreach (var row in table.Rows)
                        {
                            resultString += separator + PrintResult(row.Value.Fields.First().Value);
                            separator = ", ";
                        }

                        resultString += "]";
                    }

                    // otherwise a full table treatment is needed
                    //else if (_formatTable)
                    //{
                    //    resultString = "\n ";
                    //    var column = 0;

                    //    foreach (var row in table.Rows)
                    //    {
                    //        if (row.Value != null)
                    //        {
                    //            column = 0;
                    //            foreach (var field in row.Value.Fields)
                    //            {
                    //                columnWidth[column] = Math.Max(columnWidth[column], field.Name.Length);
                    //                resultString += " " + field.Name.PadLeft(columnWidth[column]) + "  ";
                    //                column++;
                    //            }

                    //            break;
                    //        }
                    //    }

                    //    resultString += "\n ";

                    //    foreach (var width in columnWidth)
                    //    {
                    //        resultString += new string('=', width + 2) + " ";
                    //    }

                    //    foreach (var row in table.Rows)
                    //    {
                    //        column = 0;
                    //        resultString += "\n ";
                    //        if (row.Value != null)
                    //        {
                    //            foreach (var field in row.Value.Fields)
                    //            {
                    //                resultString += " " + PrintResult(field.Value, true).PadLeft(columnWidth[column]) + "  ";
                    //                column++;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            resultString += row.IsError ? row.Error?.Errors?[0].Message : "Blank()";
                    //        }
                    //    }
                    //}
                    else
                    {
                        // table without formatting 

                        resultString = "[";
                        var separator = string.Empty;
                        foreach (var row in table.Rows)
                        {
                            resultString += separator + PrintResult(row.Value);
                            separator = ", ";
                        }

                        resultString += "]";
                    }
                }
            }
            else
            {
                var sb = new StringBuilder();
                var settings = new FormulaValueSerializerSettings()
                {
                    UseCompactRepresentation = true,
                };
                value.ToExpression(sb, settings);

                resultString = sb.ToString();
            }

            return resultString;
        }
    }
}
