using Microsoft.AspNetCore.Mvc;
using Poly6502.Microprocessor;
using Poly6502.Microprocessor.Flags;
using PolyNES.WRam;

namespace Poly6502.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/6502")]
public class ProcessorController : ControllerBase
{
    private M6502 _processor;
    private RandomAccessMemory _memory;
    private ProcessorRegisters _registers;

    public ProcessorController()
    {
        _processor = new M6502();
        _registers = new ProcessorRegisters();
        _memory = new RandomAccessMemory();
    }

    [HttpGet("registers")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProcessorRegisters))]
    public IActionResult GetProcessorRegisters()
    {
        _registers.Update(_processor.A, _processor.X, _processor.Y, _processor.SP, _processor.Pc, _processor.P);

        return Ok(_registers);
    }

    [HttpPut("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Reset()
    {
        _processor.RES();

        return Ok();
    }
    
    [HttpPost("attachMemory")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProcessorRegisters))]
    public IActionResult AttachMemory(int size)
    {
        _memory = new RandomAccessMemory(size);
        _processor.RegisterDevice(_memory);

        return Ok();
    }

    [HttpPost("loadProgram")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult LoadProgram(byte[] blob)
    {
        _memory.SetRam(blob);

        return Ok();
    }

    [HttpPost("clock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Clock()
    {
        _processor.Clock();

        return Ok();
    }
}