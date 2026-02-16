using System;
using System.Collections.Generic;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Транслирует инструкции:
    /// * push [segment] [index] — записывает на стек значение взятое из ячейки [index] сегмента [segment].
    /// * pop [segment] [index] — снимает со стека значение и записывает его в ячейку [index] сегмента [segment].
    ///
    /// Сегменты:
    /// * constant — виртуальный сегмент, по индексу [index] содержит значение [index]
    /// * local — начинается в памяти по адресу Ram[LCL]
    /// * argument — начинается в памяти по адресу Ram[ARG]
    /// * this — начинается в памяти по адресу Ram[THIS]
    /// * that — начинается в памяти по адресу Ram[THAT]
    /// * pointer - по индексу 0, содержит значение Ram[THIS], а по индексу 1 — значение Ram[THAT] 
    /// * temp - начинается в памяти по адресу 5
    /// * static — хранит значения по адресу, который ассемблер выделит переменной @{moduleName}.{index}
    /// </summary>
    /// <returns>
    /// true − если это инструкция работы со стеком, иначе — false.
    /// Если метод возвращает false, он не должен менять ResultAsmCode
    /// </returns>
    private bool TryWriteStackCode(VmInstruction instruction, string moduleName)
    {
        if (instruction.Name != "push" && instruction.Name != "pop")
        {
            return false;
        }

        var segment = instruction.Args[0];
        var index = int.Parse(instruction.Args[1]);

        if (instruction.Name == "push")
        {
            WritePushBySegment(segment, index, moduleName);
        }
        else
        {
            WritePopBySegment(segment, index, moduleName);
        }

        return true;
    }

    private void WritePushBySegment(string segment, int index, string moduleName)
    {
        if (segment == "constant")
        {
            WritePushConstant(index);
        }
        else if (segment == "local" || segment == "argument" || segment == "this" || segment == "that")
        {
            WritePushMemorySegment(segment, index);
        }
        else if (segment == "temp")
        {
            WritePushTemp(index);
        }
        else if (segment == "pointer")
        {
            WritePushPointer(index);
        }
        else if (segment == "static")
        {
            WritePushStatic(index, moduleName);
        }
    }

    private void WritePopBySegment(string segment, int index, string moduleName)
    {
        if (segment == "local" || segment == "argument" || segment == "this" || segment == "that")
        {
            WritePopMemorySegment(segment, index);
        }
        else if (segment == "temp")
        {
            WritePopTemp(index);
        }
        else if (segment == "pointer")
        {
            WritePopPointer(index);
        }
        else if (segment == "static")
        {
            WritePopStatic(index, moduleName);
        }
    }

    private void WritePushConstant(int index)
    {
        WriteAsm($"@{index}", "D=A");
        WritePushD();
    }

    private void WritePushMemorySegment(string segment, int index)
    {
        var segmentSymbol = GetSegmentSymbol(segment);
        WriteAsm($"@{index}", "D=A", $"@{segmentSymbol}", "A=M+D", "D=M");
        WritePushD();
    }

    private void WritePushTemp(int index)
    {
        WriteAsm($"@{5 + index}", "D=M");
        WritePushD();
    }

    private void WritePushPointer(int index)
    {
        if (index == 0)
        {
            WriteAsm("@THIS", "D=M");
        }
        else if (index == 1)
        {
            WriteAsm("@THAT", "D=M");
        }
        WritePushD();
    }

    private void WritePushStatic(int index, string moduleName)
    {
        WriteAsm($"@{moduleName}.{index}", "D=M");
        WritePushD();
    }

    private void WritePopMemorySegment(string segment, int index)
    {
        var segmentSymbol = GetSegmentSymbol(segment);
        WriteAsm($"@{index}", "D=A", $"@{segmentSymbol}", "D=M+D", "@R13", "M=D");
        WritePopToD();
        WriteAsm("@R13", "A=M", "M=D");
    }

    private void WritePopTemp(int index)
    {
        WritePopToD();
        WriteAsm($"@{5 + index}", "M=D");
    }

    private void WritePopPointer(int index)
    {
        WritePopToD();
        if (index == 0)
        {
            WriteAsm("@THIS", "M=D");
        }
        else if (index == 1)
        {
            WriteAsm("@THAT", "M=D");
        }
    }

    private void WritePopStatic(int index, string moduleName)
    {
        WritePopToD();
        WriteAsm($"@{moduleName}.{index}", "M=D");
    }

    private string GetSegmentSymbol(string segment)
    {
        if (segment == "local")
        {
            return "LCL";
        }
        if (segment == "argument")
        {
            return "ARG";
        }
        if (segment == "this")
        {
            return "THIS";
        }
        if (segment == "that")
        {
            return "THAT";
        }

        throw new ArgumentException($"Unknown segment: {segment}");
    }

    private void WritePushD()
    {
        WriteAsm("@SP", "A=M", "M=D", "@SP", "M=M+1");
    }

    private void WritePopToD()
    {
        WriteAsm("@SP", "M=M-1", "A=M", "D=M");
    }
}
