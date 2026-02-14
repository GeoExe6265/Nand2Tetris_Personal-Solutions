using System;
using System.Collections.Generic;

namespace VMTranslator;

public class Parser
{
    /// <summary>
    /// Читает список строк, пропускает строки, не являющиеся инструкциями,
    /// и возвращает массив инструкций
    /// </summary>
    public VmInstruction[] Parse(string[] vmStrings)
    {
        var instructions = new List<VmInstruction>();
        int lineNumber = 0;

        foreach (string str in vmStrings)
        {
            lineNumber++;

            VmInstruction instruction = ParseLine(str, lineNumber);

            if (instruction != null)
            {
                instructions.Add(instruction);
            }
        }

        return instructions.ToArray();
    }

    private VmInstruction ParseLine(string line, int lineNumber)
    {
        string cleanedLine = RemoveComments(line);
        string trimmedLine = cleanedLine.Trim();

        if (SkipLine(trimmedLine))
        {
            return null;
        }

        string[] parts = SplitLine(trimmedLine);

        return CreateInstructionFromParts(parts, lineNumber);
    }

    private string RemoveComments(string line)
    {
        if (line.Contains("//"))
        {
            return line.Substring(0, line.IndexOf("//"));
        }

        return line;
    }

    private bool SkipLine(string line)
    {
        return string.IsNullOrEmpty(line);
    }

    private string[] SplitLine(string line)
    {
        return line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private VmInstruction CreateInstructionFromParts(string[] parts, int lineNumber)
    {
        string commandName = GetCommandName(parts);
        string[] arguments = GetArguments(parts);

        return CreateInstruction(lineNumber, commandName, arguments);
    }

    private string GetCommandName(string[] parts)
    {
        return parts[0];
    }

    private string[] GetArguments(string[] parts)
    {
        if (parts.Length > 1)
        {
            return new ArraySegment<string>(parts, 1, parts.Length - 1).ToArray();
        }

        return Array.Empty<string>();
    }

    private VmInstruction CreateInstruction(int lineNumber, string commandName, string[] arguments)
    {
        if (arguments.Length > 0)
        {
            return new VmInstruction(lineNumber, commandName, arguments);
        }

        return new VmInstruction(lineNumber, commandName);
    }
}
